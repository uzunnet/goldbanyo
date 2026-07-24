using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using MudBlazor;
using MudBlazor.Services;
using VizitLink3D.Konfigurator.Servisler;
using System.Globalization;
using System.Security.Claims;

var yapilandirici = WebApplication.CreateBuilder(args);

// ── Typed Config — Options pattern ──
yapilandirici.Services.Configure<UygulamaAyarlari>(
    yapilandirici.Configuration.GetSection(UygulamaAyarlari.BolumAdi));
yapilandirici.Services.Configure<ApiAyarlari>(
    yapilandirici.Configuration.GetSection(ApiAyarlari.BolumAdi));
yapilandirici.Services.Configure<BffGuvenlikAyarlari>(
    yapilandirici.Configuration.GetSection(BffGuvenlikAyarlari.BolumAdi));

// ── Cookie Authentication (BFF) ──
yapilandirici.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(secenekler =>
    {
        secenekler.LoginPath = "/admin";
        secenekler.LogoutPath = "/oturum/cikis";
        secenekler.AccessDeniedPath = "/admin";
        secenekler.Cookie.HttpOnly = true;
        secenekler.Cookie.SameSite = SameSiteMode.Lax;
        secenekler.Cookie.SecurePolicy = yapilandirici.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        secenekler.ExpireTimeSpan = TimeSpan.FromHours(8);
        secenekler.SlidingExpiration = true;
    });

yapilandirici.Services.AddAuthorization();
yapilandirici.Services.AddCascadingAuthenticationState();
yapilandirici.Services.AddHttpContextAccessor();

// ── Blazor Interactive Server ──
yapilandirici.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── MudBlazor servisleri ──
yapilandirici.Services.AddMudServices();

// ── Kendi bagimsiz servislerimiz ──
yapilandirici.Services.AddSingleton<DilServisi>();
yapilandirici.Services.AddScoped<KimlikServisi>();

// P04 ret duzeltmesi: Uc Boyut Goruntuleyici Servisi — IJSRuntime soyutlamasi
// Scoped: her Blazor circuit icin bir instance, JS modul referansi circuit omru boyunca yasar
yapilandirici.Services.AddScoped<IUcBoyutGoruntuleyiciServisi, UcBoyutGoruntuleyiciServisi>();

// Ortak API URL — tum HttpClient istemcileri icin
var apiUrl = yapilandirici.Configuration.GetValue<string>("ApiAyarlari:BaseUrl")
    ?? (yapilandirici.Environment.IsDevelopment() ? "http://localhost:5116/" : "");

// API istemcisi: appsettings'ten BaseUrl okur
yapilandirici.Services.AddHttpClient<ApiIstemcisi>(istemci =>
{
    istemci.BaseAddress = new Uri(apiUrl);
    istemci.Timeout = TimeSpan.FromSeconds(10);
});

// Modeller yonetim servisi — kendi HttpClient'i ile BFF secret kullanir
yapilandirici.Services.AddHttpClient<ModellerYonetimServisi>(istemci =>
{
    istemci.BaseAddress = new Uri(apiUrl);
    istemci.Timeout = TimeSpan.FromSeconds(30);
});

// ── Statik dosya sunucusu: KENDI wwwroot ──
yapilandirici.Services.AddResponseCompression();

var uygulama = yapilandirici.Build();
var uygulamaKonfig = uygulama.Services.GetRequiredService<IOptions<UygulamaAyarlari>>().Value;
var googleFontsEtkin = yapilandirici.Configuration.GetValue<bool>("Guvenlik:GoogleFontsEtkin", true);

// ── Guvenlik basliklari ──
uygulama.Use(async (baglam, sonraki) =>
{
    var basliklar = baglam.Response.Headers;

    basliklar["X-Content-Type-Options"] = "nosniff";
    basliklar["Referrer-Policy"] = "strict-origin-when-cross-origin";
    basliklar["X-Frame-Options"] = "DENY";
    basliklar["Cross-Origin-Opener-Policy"] = "same-origin";

    // CSP: Blazor Interactive Server + MudBlazor icin en dar izin seti
    var stilSrc = "'self' 'unsafe-inline'";
    var fontSrc = "'self'";
    if (googleFontsEtkin)
    {
        stilSrc += " https://fonts.googleapis.com";
        fontSrc += " https://fonts.gstatic.com";
    }

    // P04: Three.js CDN (cdn.jsdelivr.net) script-src izni.
    // Three.js npm paketi Blazor SSR ile uyumlu olmadigindan,
    // tarayici tarafi ES module import icin pinned CDN kullanilir.
    // Sadece gerekli origin izinlidir; unsafe-inline veya unsafe-eval yoktur.
    var scriptSrc = "'self' https://cdn.jsdelivr.net";

    basliklar["Content-Security-Policy"] =
        $"default-src 'self'; " +
        $"script-src {scriptSrc}; " +
        $"style-src {stilSrc}; " +
        $"font-src {fontSrc}; " +
        $"img-src 'self' data: blob:; " +
        $"connect-src 'self' ws:; " +
        $"worker-src 'self' blob:; " +
        $"frame-ancestors 'none';";

    await sonraki();
});

// ── Hassas dosyalari engelle ──
uygulama.Use(async (baglam, sonraki) =>
{
    var yol = baglam.Request.Path.Value ?? "";

    if (yol.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
    {
        baglam.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await sonraki();
});

// ── KENDI wwwroot statik dosyalari ──
uygulama.UseResponseCompression();
uygulama.UseStaticFiles();

// ── Auth middleware (Antiforgery'den ONCE) ──
uygulama.UseAuthentication();
uygulama.UseAuthorization();
uygulama.UseAntiforgery();

// ── Oturum endpoint'leri (BFF) ──

// POST /oturum/giris — form tabanli giris, ApiIstemcisi uzerinden API dogrulama
uygulama.MapPost("/oturum/giris", async (
    HttpContext baglam,
    IAntiforgery antiforgery,
    ApiIstemcisi api) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(baglam);
    }
    catch (AntiforgeryValidationException)
    {
        baglam.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    try
    {
        var form = await baglam.Request.ReadFormAsync();
        var kullaniciAdi = form["username"].FirstOrDefault();
        var sifre = form["password"].FirstOrDefault();
        var returnUrl = form["returnUrl"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(sifre))
        {
            baglam.Response.Redirect("/admin?hata=giris_bos");
            return;
        }

        var yanit = await api.PostHamAsync<GirisYanitVerisi>("api/kimlik/giris",
            new { KullaniciAdi = kullaniciAdi, Sifre = sifre });

        if (yanit?.BasariliMi != true || yanit.Veri is null)
        {
            baglam.Response.Redirect("/admin?hata=giris_basarisiz");
            return;
        }

        var veri = yanit.Veri;
        var kullaniciId = veri.KullaniciId?.ToString(CultureInfo.InvariantCulture) ?? "";
        var kullaniciAdiVal = veri.KullaniciAdi ?? "";
        var rol = veri.Rol ?? "";

        var talepler = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, kullaniciId),
            new(ClaimTypes.Name, kullaniciAdiVal),
            new("KullaniciId", kullaniciId),
            new("KullaniciAdi", kullaniciAdiVal),
            new(ClaimTypes.Role, rol),
            new("Rol", rol)
        };

        var kimlik = new ClaimsIdentity(talepler, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(kimlik);

        await baglam.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        // Yalnizca local (/ ile baslayan, // ile baslamayan) URL'lere izin ver — open redirect onlemi
        if (!string.IsNullOrWhiteSpace(returnUrl)
            && returnUrl.StartsWith('/')
            && !returnUrl.StartsWith("//"))
        {
            baglam.Response.Redirect(returnUrl);
        }
        else
        {
            baglam.Response.Redirect("/admin/dashboard");
        }
    }
    catch
    {
        // Hata detayi sizdirilmaz — generic hata mesaji
        baglam.Response.Redirect("/admin?hata=sunucu_hatasi");
    }
});

// POST /oturum/cikis — cookie oturumunu sonlandirir
uygulama.MapPost("/oturum/cikis", async (
    HttpContext baglam,
    IAntiforgery antiforgery) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(baglam);
    }
    catch (AntiforgeryValidationException)
    {
        baglam.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    await baglam.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    baglam.Response.Redirect("/admin");
});

// POST /oturum/sifre-sifirlama-istegi — sifre sifirlama eposta istegi (BFF proxy)
// Account enumeration onlemi: API basarisiz olsa bile generic basarili yanit doner
uygulama.MapPost("/oturum/sifre-sifirlama-istegi", async (
    HttpContext baglam,
    IAntiforgery antiforgery,
    ApiIstemcisi api) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(baglam);
    }
    catch (AntiforgeryValidationException)
    {
        baglam.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    try
    {
        var form = await baglam.Request.ReadFormAsync();
        var eposta = form["email"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(eposta))
        {
            baglam.Response.Redirect("/sifre-sifirla?durum=basarisiz");
            return;
        }

        // API'ye istek gonder — basarisiz olsa bile generic basarili goster
        _ = await api.PostHamAsync<object>("api/kimlik/sifre-sifirlama-istegi",
            new { eposta });
    }
    catch
    {
        // API hatasi: sessizce gec, enumeration onlemi
    }

    // Her zaman basarili yonlendir (ic detay sizdirilmaz)
    baglam.Response.Redirect("/sifre-sifirla?durum=basarili");
});

// POST /oturum/sifre-yenile — token ile sifre yenileme (BFF proxy)
uygulama.MapPost("/oturum/sifre-yenile", async (
    HttpContext baglam,
    IAntiforgery antiforgery,
    ApiIstemcisi api) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(baglam);
    }
    catch (AntiforgeryValidationException)
    {
        baglam.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    try
    {
        var form = await baglam.Request.ReadFormAsync();
        var token = form["token"].FirstOrDefault();
        var yeniSifre = form["yeniSifre"].FirstOrDefault();
        var yeniSifreTekrar = form["yeniSifreTekrar"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(yeniSifre))
        {
            baglam.Response.Redirect("/sifre-yenile?durum=basarisiz");
            return;
        }

        // ── Defense-in-depth: Şifre eşleşme kontrolü BFF'de yapılır ──
        // API çağrısı YAPILMADAN önce kontrol edilir; eşleşmezse
        // şifre değişimi tetiklenmez, güvenli hata redirect döner.
        // API tarafında FluentValidation da aynı kontrolü yapar (çift katman).
        if (yeniSifre != yeniSifreTekrar)
        {
            baglam.Response.Redirect("/sifre-yenile?durum=sifreler-eslesmiyor");
            return;
        }

        var yanit = await api.PostHamAsync<object>("api/kimlik/sifre-yenile",
            new { token, yeniSifre, yeniSifreTekrar });

        if (yanit?.BasariliMi == true)
        {
            baglam.Response.Redirect("/sifre-yenile?durum=basarili");
        }
        else
        {
            baglam.Response.Redirect("/sifre-yenile?durum=basarisiz");
        }
    }
    catch
    {
        // Hata detayi sizdirilmaz — generic hata
        baglam.Response.Redirect("/sifre-yenile?durum=basarisiz");
    }
});

// ── Blazor Interactive Server haritalama ──
uygulama.MapRazorComponents<VizitLink3D.Konfigurator.App>()
    .AddInteractiveServerRenderMode();

// ═══════════════════════════════════════════════════════════════
// P04: Public 3D Viewer BFF Proxy Endpoint'leri
// Browser 5116'ya dogrudan erisemez; tum istekler BFF uzerinden.
// ═══════════════════════════════════════════════════════════════

// GET /api/public/modeller — Public model listesi (safe DTO)
uygulama.MapGet("/api/public/modeller", async (
    HttpContext baglam,
    IServiceScopeFactory kapsamFabrikasi,
    CancellationToken iptal) =>
{
    await using var kapsam = kapsamFabrikasi.CreateAsyncScope();
    var servis = kapsam.ServiceProvider.GetRequiredService<ModellerYonetimServisi>();
    var liste = await servis.PublicModelListesiGetirAsync(iptal);

    if (liste is null)
        return Results.Json(new { basariliMi = false, mesaj = "Model listesi alinamadi." }, statusCode: 502);

    return Results.Json(new { basariliMi = true, veri = liste });
});

// GET /api/public/modeller/{slug} — Public model detay (safe DTO)
uygulama.MapGet("/api/public/modeller/{slug}", async (
    string slug,
    HttpContext baglam,
    IServiceScopeFactory kapsamFabrikasi,
    CancellationToken iptal) =>
{
    if (string.IsNullOrWhiteSpace(slug))
        return Results.Json(new { basariliMi = false, mesaj = "Gecersiz slug." }, statusCode: 400);

    await using var kapsam = kapsamFabrikasi.CreateAsyncScope();
    var servis = kapsam.ServiceProvider.GetRequiredService<ModellerYonetimServisi>();
    var detay = await servis.PublicModelDetayGetirAsync(slug, iptal);

    if (detay is null)
        return Results.Json(new { basariliMi = false, mesaj = "Model bulunamadi." }, statusCode: 404);

    return Results.Json(new { basariliMi = true, veri = detay });
});

// GET /api/public/modeller/{slug}/dosya — GLB binary dosya proxy
// BFF, API'den model detayini alir, DosyaAdi ile GLB'yi indirir, tarayiciya stream eder.
uygulama.MapGet("/api/public/modeller/{slug}/dosya", async (
    string slug,
    HttpContext baglam,
    IServiceScopeFactory kapsamFabrikasi,
    CancellationToken iptal) =>
{
    if (string.IsNullOrWhiteSpace(slug))
    {
        baglam.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    await using var kapsam = kapsamFabrikasi.CreateAsyncScope();
    var servis = kapsam.ServiceProvider.GetRequiredService<ModellerYonetimServisi>();
    var (akis, icerikTuru, dosyaAdi) = await servis.ModelDosyasiIndirAsync(slug, iptal);

    if (akis is null)
    {
        baglam.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    baglam.Response.ContentType = icerikTuru ?? "model/gltf-binary";
    baglam.Response.Headers.ContentDisposition = $"inline; filename=\"{dosyaAdi ?? "model.glb"}\"";
    baglam.Response.Headers.CacheControl = "public, max-age=3600";

    await akis.CopyToAsync(baglam.Response.Body, iptal);
});

// ── Saglik kontrolu (config'ten okur) ──
var port = uygulamaKonfig.Port;
var proje = uygulamaKonfig.Proje;
var versiyon = uygulamaKonfig.Versiyon;
var aciklama = uygulamaKonfig.Aciklama;

uygulama.MapGet("/saglik", () => Results.Ok(new
{
    durum = "calisiyor",
    port,
    proje,
    versiyon,
    aciklama
}));

Console.WriteLine($"[KONFIGURATOR] VizitLink3D Studio Bagimsiz Runtime — http://localhost:{port}");
Console.WriteLine($"[KONFIGURATOR] Public 3D Viewer   — http://localhost:{port}/");
Console.WriteLine($"[KONFIGURATOR] Public API modeller — http://localhost:{port}/api/public/modeller");
Console.WriteLine($"[KONFIGURATOR] Admin Giris        — http://localhost:{port}/admin");
Console.WriteLine($"[KONFIGURATOR] Admin Dashboard    — http://localhost:{port}/admin/dashboard");
Console.WriteLine($"[KONFIGURATOR] Admin Modeller     — http://localhost:{port}/admin/modeller");
Console.WriteLine($"[KONFIGURATOR] Saglik             — http://localhost:{port}/saglik");
uygulama.Run();

/// <summary>
/// Test projeleri icin WebApplicationFactory erisimi saglar.
/// </summary>
public partial class Program { }
