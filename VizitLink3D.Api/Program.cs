using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Api.AraYazilimlar;
using VizitLink3D.Api.Servisler;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Net;
using System.Text;
using VizitLink3D.Api.Hubs;
using VizitLink3D.Ortak.Modeller.Urunler;

// Serilog yapilandirmasi (anayasa §15)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/vizitlink3d-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var yapici = WebApplication.CreateBuilder(args);
yapici.Host.UseSerilog();

// Veritabani
var vtYolu = yapici.Configuration["VeriTabani:Yol"] ?? "vizitlink3d.db";
yapici.Services.AddHttpContextAccessor();
yapici.Services.AddDbContext<VizitLink3DDbContext>((sp, sec) =>
{
    var httpErisimi = sp.GetService<IHttpContextAccessor>();
    sec.UseSqlite($"Data Source={vtYolu}")
       .AddInterceptors(new AuditInterceptor(httpErisimi));
});

// JWT Kimlik Dogrulama — anahtar yoksa public siteyi çökertmeden çalıştır
var jwtAnahtar = Environment.GetEnvironmentVariable("VIZITLINK3D_JWT_KEY")
    ?? yapici.Configuration["Jwt:Anahtar"];

if (!string.IsNullOrWhiteSpace(jwtAnahtar))
{
    yapici.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(sec =>
        {
            sec.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAnahtar)),
                ValidateIssuer = true,
                ValidIssuer = yapici.Configuration["Jwt:Yayinci"],
                ValidateAudience = true,
                ValidAudience = yapici.Configuration["Jwt:Izleyici"],
                ValidateLifetime = true
            };
        });
}
else
{
    Log.Warning("JWT anahtari bos oldugu icin kimlik dogrulama gecici olarak pasif.");
    yapici.Services.AddAuthentication();
}

yapici.Services.AddAuthorization();
yapici.Services.AddSignalR();

// Rate Limiting (anayasa §23.5)
yapici.Services.RateLimitingEkle();

// CORS - Bu kurulum tek domain GoldBanyo odaklidir, ama liste konfigden geldigi icin
// SaaS tarafina tekrar acilabilir.
var izinliDomainler = yapici.Configuration.GetSection("Cors:IzinliDomainler").Get<string[]>()
    ?? ["http://localhost:3113", "https://localhost:3113", "http://localhost:5113", "https://localhost:5113"];
yapici.Services.AddCors(sec => sec.AddDefaultPolicy(politika =>
{
    if (yapici.Environment.IsDevelopment())
    {
        politika.SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        return;
    }

    politika.WithOrigins(izinliDomainler)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
}));

yapici.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
yapici.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
yapici.Services.AddControllers();
yapici.Services.AddOpenApi();

// Onbellek ve Ceviri servisleri (anayasa §35)
yapici.Services.AddMemoryCache();
yapici.Services.AddSingleton<IOnbellekYonetici, OnbellekYonetici>();
yapici.Services.AddScoped<ICeviriServisi, CeviriServisi>();
yapici.Services.AddScoped<IOtomatikCeviriServisi, OtomatikCeviriServisi>();

// Medya Havuzu servisleri
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Medya.Servisler.IDepolamaAdaptoru, VizitLink3D.Api.Moduller.Medya.Servisler.YerelDepolama>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Medya.Servisler.IResimIslemcisi, VizitLink3D.Api.Moduller.Medya.Servisler.ResimIslemcisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Medya.Servisler.IYoutubeMetadataServisi, VizitLink3D.Api.Moduller.Medya.Servisler.YoutubeMetadataServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Medya.Servisler.IMedyaServisi, VizitLink3D.Api.Moduller.Medya.Servisler.MedyaServisi>();

// API Key Sifreleme ve PII Filtre
yapici.Services.AddDataProtection();
yapici.Services.AddSingleton<IApiKeySifrelemeServisi, ApiKeySifrelemeServisi>();
yapici.Services.AddSingleton<IPIIFiltreServisi, PIIFiltreServisi>();

// AI Asistan servisleri
yapici.Services.AddHttpClient();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.AI.Servisler.AIGuvenlikServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.AI.Servisler.AISaglayiciFabrikasi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.AI.Servisler.IAIMaliyetTakipServisi, VizitLink3D.Api.Moduller.AI.Servisler.AIMaliyetTakipServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.AI.Servisler.AIOrkestraServisi>();

// Resimden 3D uretim servisi (yerel Python/TripoSR servisine HTTP koprusu)
// CPU'da uretim ~1 dakika surebildigi icin varsayilan HttpClient timeout'u yetersiz kalir.
yapici.Services.AddHttpClient<VizitLink3D.Api.Moduller.UcBoyutUretim.Servisler.PythonUcBoyutSaglayici>(istemci =>
{
    istemci.Timeout = TimeSpan.FromMinutes(5);
});

// Kapak → Urun goc servisi
yapici.Services.AddScoped<KapakGocServisi>();
yapici.Services.AddScoped<UrunMedyaBaglamaServisi>();
yapici.Services.AddScoped<PdfCozumlemeServisi>();
yapici.Services.AddScoped<PdfOnizlemeServisi>();
yapici.Services.AddScoped<PdfUygulamaAlanServisi>();
yapici.Services.AddScoped<MedyaGocServisi>();

// Multi-tenant servisleri
yapici.Services.AddScoped<KiraciServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Tema.Servisler.StitchTemaServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Tema.Servisler.CokluTemaServisi>();

// Lisans servisi
yapici.Services.AddScoped<VizitLink3D.Api.Servisler.Kimlik.LisansServisi>();

var uygulama = yapici.Build();

// Veritabanini otomatik olustur ve baslangic verilerini tohumla
using (var kapsam = uygulama.Services.CreateScope())
{
    var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
    var webEnv = kapsam.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    // SQLite performans ayarlari: WAL modu okuma/yazmayi birbirini kilitlemeden
    // calistirir (coklu istek altinda hizli kalir), busy_timeout kilit
    // catismalarinda "database is locked" hatasi yerine kisa sure bekler.
    vt.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    vt.Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");
    vt.Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");

    var migrationAtla = string.Equals(Environment.GetEnvironmentVariable("VIZITLINK3D_SKIP_MIGRATION"), "1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable("VIZITLINK3D_SKIP_MIGRATION"), "true", StringComparison.OrdinalIgnoreCase);

    if (migrationAtla)
    {
        Log.Information("Veritabani migrasyonu ortam degiskeni ile atlandi.");
    }
    else
    {
    // SQLite EF migration kilidi, yarida kalan onceki calismalardan kalabiliyor.
    // Ilk acilista guvenli sekilde temizleyip migrasyonu devam ettiriyoruz.
    vt.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS \"__EFMigrationsLock\";");
    await vt.Database.MigrateAsync();
    }
    // Tohum verisi sadece Development ortaminda calissin (best practice).
    // Production'da DB zaten dolu, tohum verisi gereksiz islem + potansiyel AuditLog dongusu riski tasir.
    // Gerekirse FOR_SEED=1 ortam degiskeni ile zorla calistirilabilir.
    if (webEnv.IsDevelopment() || Environment.GetEnvironmentVariable("FORCE_SEED") == "1")
    {
        await VizitLink3D.Api.VeriTabani.TohumVerisi.TohumlaAsync(vt);
    }

    // DeepSeek API anahtarını şifreli tohumla (env veya config'den; kayıt varsa dokunma)
    var deepSeekKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
        ?? yapici.Configuration["AI:DeepSeekApiKey"];
    if (!string.IsNullOrWhiteSpace(deepSeekKey)
        && !vt.AISaglayicilari.Any(s => s.Tip == VizitLink3D.Ortak.Modeller.AI.AISaglayiciTipi.DeepSeek))
    {
        var sifreleyici = kapsam.ServiceProvider.GetRequiredService<VizitLink3D.Api.Servisler.IApiKeySifrelemeServisi>();
        vt.AISaglayicilari.Add(new VizitLink3D.Ortak.Modeller.AI.AISaglayicisi
        {
            Tip = VizitLink3D.Ortak.Modeller.AI.AISaglayiciTipi.DeepSeek,
            Ad = "DeepSeek (kod yazıcı)",
            ApiKeyEncrypted = sifreleyici.Sifrele(deepSeekKey),
            Model = "deepseek-chat",
            AylikLimitUsd = 50,
            AktifMi = true,
            SiraNo = 1
        });
        await vt.SaveChangesAsync();
        Log.Information("DeepSeek sağlayıcısı şifreli API anahtarıyla tohumlandı.");
    }

    // OpenCode Zen modellerini tohumla (2-6. modeller — paralel işçiler).
    // Ücretsiz modeller aktif; ücretli olanlar bakiye yüklenene kadar pasif.
    var zenKey = Environment.GetEnvironmentVariable("OPENCODE_ZEN_KEY")
        ?? Environment.GetEnvironmentVariable("OPENCODE_ZEN_KEY", EnvironmentVariableTarget.User);
    if (!string.IsNullOrWhiteSpace(zenKey)
        && !vt.AISaglayicilari.Any(s => s.Tip == VizitLink3D.Ortak.Modeller.AI.AISaglayiciTipi.OpenCodeZen))
    {
        var sifreleyiciZen = kapsam.ServiceProvider.GetRequiredService<VizitLink3D.Api.Servisler.IApiKeySifrelemeServisi>();
        var zenSifreli = sifreleyiciZen.Sifrele(zenKey);
        var zenModeller = new (string Ad, string Model, bool Aktif, int Sira)[]
        {
            ("Zen DeepSeek V4 Flash Free (işçi)", "deepseek-v4-flash-free", true,  2),
            ("Zen MiMo v2.5 Free (işçi)",         "mimo-v2.5-free",         true,  3),
            ("Zen MiniMax M3 (planlayıcı)",       "minimax-m3",             false, 4),
            ("Zen MiniMax M2.7 (analiz)",         "minimax-m2.7",           false, 5),
            ("Zen DeepSeek V4 Flash (ücretli)",   "deepseek-v4-flash",      false, 6)
        };
        foreach (var (ad, model, aktif, sira) in zenModeller)
        {
            vt.AISaglayicilari.Add(new VizitLink3D.Ortak.Modeller.AI.AISaglayicisi
            {
                Tip = VizitLink3D.Ortak.Modeller.AI.AISaglayiciTipi.OpenCodeZen,
                Ad = ad, Model = model, AktifMi = aktif, SiraNo = sira,
                ApiKeyEncrypted = zenSifreli, AylikLimitUsd = 25
            });
        }
        await vt.SaveChangesAsync();
        Log.Information("OpenCode Zen modelleri tohumlandı (2 aktif ücretsiz + 3 pasif ücretli).");
    }

    var postStartupSyncAtla = string.Equals(Environment.GetEnvironmentVariable("VIZITLINK3D_SKIP_POST_STARTUP_SYNC"), "1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable("VIZITLINK3D_SKIP_POST_STARTUP_SYNC"), "true", StringComparison.OrdinalIgnoreCase);

    if (postStartupSyncAtla)
    {
        Log.Information("Baslangic sonrasi tema ve medya senkronlari ortam degiskeni ile atlandi.");
    }
    else
    {
        // Gold Banyo frontend varsayılanı gold olmalı; admin teması ayrı hatta kalır.
        vt.Database.ExecuteSqlRaw("UPDATE Firmalar SET SiteTema = 'gold' WHERE Slug = 'goldbanyo' AND (SiteTema IS NULL OR SiteTema = '' OR SiteTema IN ('goldbanyo', 'goldbanyo-karanlik', 'gold-luxury-dark', 'altin-siyah', 'aurelian-onyx', 'endustri-karanlik'))");

        var cokluTemaServisi = kapsam.ServiceProvider.GetRequiredService<VizitLink3D.Api.Moduller.Tema.Servisler.CokluTemaServisi>();
        await cokluTemaServisi.TemaVarliklariniEsitleVeIceriAktarAsync();

        // Eski VIZITLINK3D kaydini Gold Banyo demo kaydina cevir
        vt.Database.ExecuteSqlRaw("UPDATE Firmalar SET Ad = 'Gold Banyo Demo', Unvan = 'Gold Banyo Demo', Slug = 'goldbanyo-demo', Aciklama = 'Gold Banyo demo firma kaydi', AciklamaKisa = 'Gold Banyo demo', Domain = 'demo.goldbanyom.local', YedekDomain = 'www.demo.goldbanyom.local', Eposta = 'demo@goldbanyom.local', Telefon1 = '', Telefon2 = '', Adres = '', Instagram = '', Facebook = '' WHERE Slug = 'VIZITLINK3D'");

        // WebRootPath production'da null olabilir - skip et
        if (!string.IsNullOrEmpty(webEnv.WebRootPath))
            await TohumVerisi.TemizleSlaytResimlerAsync(vt, webEnv.WebRootPath);

        // Kapak → Urun gocu (idempotent — tekrar calisinca cogaltmaz)
        var goc = kapsam.ServiceProvider.GetRequiredService<KapakGocServisi>();
        var gocSonuc = await goc.GogAsync();
        Log.Information("Kapak gocu: {Gocen} goc etti, {Atlandi} atlandi", gocSonuc.Veri?.Gocen, gocSonuc.Veri?.Atlandi);

        // NRD gorsel + 3D model baglama (idempotent — wwwroot/medya'daki standart dosyalari urunlere baglar)
        var medyaBaglama = kapsam.ServiceProvider.GetRequiredService<UrunMedyaBaglamaServisi>();
        var baglamaSonuc = await medyaBaglama.BaglaAsync();
        Log.Information("Urun medya baglama: {Urun} urun, {Gorsel} gorsel, {Model} 3D model",
            baglamaSonuc.Veri?.GuncellenenUrun, baglamaSonuc.Veri?.BaglananGorsel, baglamaSonuc.Veri?.BaglananModel);
    }
}

if (uygulama.Environment.IsDevelopment())
    uygulama.MapOpenApi();

// ==================== MIDDLEWARE SIRASI ====================
// 1. Hata yonetimi (en dis katman)
uygulama.UseMiddleware<HataYonetimiMiddleware>();

// 2. Firma cozumleme (multi-tenant)
uygulama.UseMiddleware<FirmaCozumlemeMiddleware>();

// 3. Lisans dogrulama
uygulama.UseMiddleware<LisansDogrulamaMiddleware>();

// 3. Guvenlik header'lari
uygulama.UseMiddleware<GuvenlikHeaderlariMiddleware>();

// 4. Routing
uygulama.UseRouting();

// 5. Rate limiting
uygulama.UseRateLimiter();

// 6. CORS
uygulama.UseCors();

// 7. Statik dosyalar (3D modeller icin GLB/GLTF MIME)
var saglayici = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
saglayici.Mappings[".glb"] = "model/gltf-binary";
saglayici.Mappings[".gltf"] = "model/gltf+json";
saglayici.Mappings[".wasm"] = "application/wasm";
saglayici.Mappings[".dll"] = "application/octet-stream";
saglayici.Mappings[".pdb"] = "application/octet-stream";
saglayici.Mappings[".dat"] = "application/octet-stream";
saglayici.Mappings[".js"] = "text/javascript";
saglayici.Mappings[".mjs"] = "text/javascript";
saglayici.Mappings[".css"] = "text/css";
saglayici.Mappings[".map"] = "application/json";
saglayici.Mappings[".br"] = "application/octet-stream";

uygulama.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = saglayici,
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

uygulama.ResimOptimizasyonKullan();

// 8. Kimlik dogrulama
uygulama.UseAuthentication();

// 9. Yetkilendirme
uygulama.UseAuthorization();

// 10. API Controller'lari
uygulama.MapControllers();

// 11. SignalR Hub
uygulama.MapHub<SohbetHub>("/hubs/sohbet");
uygulama.MapHub<BildirimHub>("/hubs/bildirim");
uygulama.MapHub<AIHub>("/hubs/ai");
uygulama.MapHub<SahneAyarHub>("/hubs/sahne-ayar");
uygulama.MapHub<TemaHub>("/hubs/tema");

uygulama.MapGet("/banyo-dolaplari", () =>
    Results.Content(BanyoDolaplariSayfasiUret(), "text/html; charset=utf-8"));

uygulama.MapGet("/banyo-dolabi/{slug}", (string slug) =>
    Results.Content(BanyoDolabiDetaySayfasiUret(slug), "text/html; charset=utf-8"));

// 12. Blazor WASM fallback
uygulama.MapFallbackToFile("index.html");

uygulama.Run();

static string BanyoDolaplariSayfasiUret()
{
    var urunler = GoldBanyoKatalogUrunleri.Tum
        .OrderBy(x => GoldBanyoKatalogUrunleri.KoleksiyonSirasiGetir(x.KoleksiyonGrubu))
        .ThenBy(x => x.SayfaNo)
        .ToList();

    var gruplar = urunler
        .GroupBy(x => x.KoleksiyonGrubu)
        .OrderBy(x => GoldBanyoKatalogUrunleri.KoleksiyonSirasiGetir(x.Key));

    var icerik = new StringBuilder();
    icerik.AppendLine("""
        <section class="hero">
            <div>
                <span class="etiket">Gold Banyo 2026 Koleksiyonları</span>
                <h1>Banyo Dolabı Koleksiyonları</h1>
                <p>Hermes'ten Lugano Plus'a kadar tüm Gold Banyo banyo dolabı serilerini çalışan canlı katalog görünümünde inceleyin.</p>
            </div>
            <div class="ozet-grid">
                <article class="ozet-kart"><span>Koleksiyon</span><strong>
        """);
    icerik.Append(urunler.Select(x => x.KoleksiyonGrubu).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    icerik.AppendLine("""
                </strong></article>
                <article class="ozet-kart"><span>Model</span><strong>
        """);
    icerik.Append(urunler.Count);
    icerik.AppendLine("""
                </strong></article>
                <article class="ozet-kart"><span>Öne Çıkan</span><strong>
        """);
    icerik.Append(urunler.Count(x => x.OneCikanMi));
    icerik.AppendLine("""
                </strong></article>
            </div>
        </section>
        """);

    foreach (var grup in gruplar)
    {
        icerik.Append("""
            <section class="koleksiyon">
                <div class="koleksiyon-baslik">
                    <div>
                        <h2>
            """);
        icerik.Append(HtmlKodla(grup.Key));
        icerik.Append("""
                        </h2>
                        <p>
            """);
        icerik.Append(HtmlKodla(GoldBanyoKatalogUrunleri.KoleksiyonAciklamasiGetir(grup.Key)));
        icerik.Append("""
                        </p>
                    </div>
                    <strong class="koleksiyon-adet">
            """);
        icerik.Append(grup.Count());
        icerik.Append("""
                        model
                    </strong>
                </div>
                <div class="kart-grid">
            """);

        foreach (var urun in grup)
        {
            icerik.Append("""
                <a class="kart" href="/banyo-dolabi/
            """);
            icerik.Append(Uri.EscapeDataString(urun.Slug));
            icerik.Append("""
                ">
                    <div class="kart-gorsel">
                        <img src="
            """);
            icerik.Append(urun.HeroGorselUrl);
            icerik.Append("""
                        " alt="
            """);
            icerik.Append(HtmlKodla(urun.Ad));
            icerik.Append("""
                        " />
                    </div>
                    <div class="kart-icerik">
                        <div class="kart-ust">
                            <span>
            """);
            icerik.Append(HtmlKodla(urun.Kod));
            icerik.Append("""
                            </span>
                            <span>
            """);
            icerik.Append(HtmlKodla(urun.KoleksiyonGrubu));
            icerik.Append("""
                            </span>
                        </div>
                        <h3>
            """);
            icerik.Append(HtmlKodla(urun.Ad));
            icerik.Append("""
                        </h3>
                        <p>
            """);
            icerik.Append(HtmlKodla(urun.KisaAciklamaOlustur()));
            icerik.Append("""
                        </p>
                        <div class="kart-rozetler">
            """);
            if (urun.OneCikanMi)
            {
                icerik.Append("""<span class="rozet">Öne Çıkan</span>""");
            }
            if (urun.YeniMi)
            {
                icerik.Append("""<span class="rozet">Yeni</span>""");
            }
            icerik.Append("""
                        </div>
                    </div>
                </a>
            """);
        }

        icerik.Append("""
                </div>
            </section>
        """);
    }

    return SayfaKabuguOlustur(
        "Gold Banyo Banyo Dolabı Koleksiyonları",
        "Gold Banyo 2026 koleksiyonundaki banyo dolabı modellerini ve seri kırılımlarını inceleyin.",
        icerik.ToString());
}

static string BanyoDolabiDetaySayfasiUret(string slug)
{
    var urun = GoldBanyoKatalogUrunleri.SlugIleGetir(slug);

    if (urun is null)
    {
        return SayfaKabuguOlustur(
            "Banyo Dolabı Bulunamadı",
            "İstenen banyo dolabı sayfası bulunamadı.",
            """
            <section class="bos-durum">
                <h1>Banyo dolabı bulunamadı</h1>
                <p>İstediğiniz ürün bu katalogta yer almıyor.</p>
                <a class="buton" href="/banyo-dolaplari">Koleksiyona dön</a>
            </section>
            """);
    }

    var icerik = new StringBuilder();
    icerik.Append("""
        <a class="geri-link" href="/banyo-dolaplari">← Koleksiyona geri dön</a>
        <section class="detay-grid detay-sahnesi">
            <div class="detay-medya reveal-sag">
                <div class="detay-medya-parilti"></div>
                <img src="
        """);
    icerik.Append(urun.KatalogGorselUrl);
    icerik.Append("""
            " alt="
        """);
    icerik.Append(HtmlKodla(urun.Ad));
    icerik.Append("""
            " />
            </div>
            <div class="detay-panel reveal-sol">
                <span class="detay-ust-etiket">Gold Banyo Canlı Detay Sayfası</span>
                <div class="kart-rozetler">
                    <span class="rozet">
        """);
    icerik.Append(HtmlKodla(urun.KoleksiyonGrubu));
    icerik.Append("""
                    </span>
        """);
    if (urun.OneCikanMi)
    {
        icerik.Append("""<span class="rozet">Öne Çıkan Seri</span>""");
    }
    if (urun.YeniMi)
    {
                icerik.Append("""<span class="rozet">2026 Sunumu</span>""");
    }
    icerik.Append("""
                </div>
                <div class="detay-vurgu-bandı">
                    <span>Showroom Sunumu</span>
                    <span>Stitch Esintili Geçişler</span>
                    <span>Canlı Koleksiyon Anlatısı</span>
                </div>
                <div class="kart-ust">
                    <span>
        """);
    icerik.Append(HtmlKodla(urun.Kod));
    icerik.Append("""
                    </span>
                    <span>Sayfa 
        """);
    icerik.Append(urun.SayfaNo);
    icerik.Append("""
                    </span>
                </div>
                <h1>
        """);
    icerik.Append(HtmlKodla(urun.Ad));
    icerik.Append("""
                </h1>
                <p>
        """);
    icerik.Append(HtmlKodla(urun.KisaAciklamaOlustur()));
    icerik.Append("""
                </p>
                <div>
                    <h3>Renkler</h3>
                    <ul class="etiket-liste">
        """);
    foreach (var renk in urun.Renkler)
    {
        icerik.Append("<li class=\"reveal-alt\">");
        icerik.Append(HtmlKodla(renk));
        icerik.Append("</li>");
    }
    icerik.Append("""
                    </ul>
                </div>
                <div>
                    <h3>Öne Çıkan Özellikler</h3>
                    <ul class="etiket-liste">
        """);
    foreach (var ozellik in urun.Ozellikler)
    {
        icerik.Append("<li class=\"reveal-alt\">");
        icerik.Append(HtmlKodla(ozellik));
        icerik.Append("</li>");
    }
    icerik.Append("""
                    </ul>
                </div>
                <div>
                    <h3>Teknik Ölçüler</h3>
                    <div class="olcu-grid">
        """);
    foreach (var olcu in urun.Olculer)
    {
        icerik.Append("""
            <article class="olcu-kart reveal-alt">
                <strong>
        """);
        icerik.Append(HtmlKodla(olcu.Baslik));
        icerik.Append("""
                </strong>
                <span>Yükseklik: 
        """);
        icerik.Append(HtmlKodla(olcu.Yukseklik));
        icerik.Append("""
                </span>
                <span>Genişlik: 
        """);
        icerik.Append(HtmlKodla(olcu.Genislik));
        icerik.Append("""
                </span>
                <span>Derinlik: 
        """);
        icerik.Append(HtmlKodla(olcu.Derinlik));
        icerik.Append("""
                </span>
            </article>
        """);
    }
    icerik.Append("""
                    </div>
                </div>
                <a class="buton" href="/iletisim">BANYO DOLABI İÇİN TEKLİF AL</a>
            </div>
        </section>
    """);

    return SayfaKabuguOlustur(
        $"{urun.Ad} | Gold Banyo Banyo Dolabı Detayı",
        urun.KisaAciklamaOlustur(),
        icerik.ToString());
}

static string SayfaKabuguOlustur(string baslik, string aciklama, string govdeIcerigi)
{
    return $$"""
    <!DOCTYPE html>
    <html lang="tr">
    <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <title>{{HtmlKodla(baslik)}}</title>
        <meta name="description" content="{{HtmlKodla(aciklama)}}" />
        <style>
            :root {
                color-scheme: light;
                --arka: #fcfaf5;
                --yuzey: rgba(255,255,255,0.92);
                --yuzey-cam: rgba(255,255,255,0.64);
                --kenar: rgba(197,160,89,0.28);
                --metin: #24180c;
                --ikincil: #655845;
                --vurgu: #c5a059;
                --vurgu-2: #efd7a1;
                --golge: 0 20px 60px rgba(36,24,12,0.10);
                --golge-vurgu: 0 0 30px rgba(197,160,89,0.22);
                --kose: 20px;
                --genislik: 1280px;
            }

            * { box-sizing: border-box; }
            body {
                margin: 0;
                font-family: "Segoe UI", Arial, sans-serif;
                color: var(--metin);
                overflow-x: hidden;
                background:
                    radial-gradient(circle at top right, rgba(197,160,89,0.18), transparent 30%),
                    radial-gradient(circle at 15% 20%, rgba(239,215,161,0.28), transparent 22%),
                    linear-gradient(180deg, #f6f1e5 0%, #fcfaf5 100%);
            }

            .kabuk {
                width: min(var(--genislik), calc(100% - 2rem));
                margin: 0 auto;
                padding: 2rem 0 4rem;
            }

            .hero, .detay-panel, .detay-medya, .bos-durum {
                background: var(--yuzey);
                border: 1px solid var(--kenar);
                border-radius: var(--kose);
                box-shadow: var(--golge);
            }

            @keyframes yumusakYukselis {
                from { opacity: 0; transform: translateY(42px); }
                to { opacity: 1; transform: translateY(0); }
            }

            @keyframes yumusakKayisSol {
                from { opacity: 0; transform: translateX(-32px); }
                to { opacity: 1; transform: translateX(0); }
            }

            @keyframes yumusakKayisSag {
                from { opacity: 0; transform: translateX(32px); }
                to { opacity: 1; transform: translateX(0); }
            }

            @keyframes altinParilti {
                0%, 100% { opacity: 0.22; transform: translateX(-8%) scale(1); }
                50% { opacity: 0.48; transform: translateX(8%) scale(1.04); }
            }

            @keyframes mediaNefes {
                0%, 100% { transform: scale(1); }
                50% { transform: scale(1.018); }
            }

            .reveal-alt,
            .reveal-sol,
            .reveal-sag {
                animation-duration: 0.85s;
                animation-fill-mode: both;
                animation-timing-function: cubic-bezier(.22, 1, .36, 1);
            }

            .reveal-alt { animation-name: yumusakYukselis; }
            .reveal-sol { animation-name: yumusakKayisSol; }
            .reveal-sag { animation-name: yumusakKayisSag; }

            .hero {
                display: grid;
                grid-template-columns: minmax(0, 1.8fr) minmax(280px, 1fr);
                gap: 1.25rem;
                padding: 2rem;
                margin-bottom: 2rem;
            }

            .etiket, .rozet, .koleksiyon-adet, .kart-ust, .yardimci-metin {
                color: var(--ikincil);
            }

            .etiket {
                display: inline-block;
                letter-spacing: 0.12em;
                text-transform: uppercase;
                color: var(--vurgu);
                font-size: 0.78rem;
            }

            h1, h2, h3 {
                font-family: Georgia, "Times New Roman", serif;
                margin: 0;
            }

            h1 { font-size: clamp(2.2rem, 5vw, 4rem); margin-top: 0.75rem; }
            h2 { font-size: clamp(1.7rem, 3vw, 2.3rem); }
            h3 { font-size: 1.1rem; margin-bottom: 0.85rem; }
            p { color: var(--ikincil); line-height: 1.7; }

            .ozet-grid, .kart-grid, .etiket-liste, .kart-rozetler, .olcu-grid {
                display: grid;
                gap: 1rem;
            }

            .ozet-grid { align-content: start; }
            .ozet-kart {
                padding: 1.2rem;
                border: 1px solid var(--kenar);
                border-radius: 16px;
                background: rgba(255,255,255,0.8);
            }
            .ozet-kart span { display: block; font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.08em; color: var(--ikincil); }
            .ozet-kart strong { display: block; margin-top: 0.4rem; font-size: 2rem; color: var(--metin); }

            .koleksiyon { margin-bottom: 2rem; }
            .koleksiyon-baslik {
                display: flex;
                justify-content: space-between;
                align-items: end;
                gap: 1rem;
                margin-bottom: 1rem;
            }

            .kart-grid {
                grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
            }

            .kart {
                display: flex;
                flex-direction: column;
                overflow: hidden;
                text-decoration: none;
                color: inherit;
                border: 1px solid var(--kenar);
                border-radius: var(--kose);
                background: rgba(255,255,255,0.9);
                box-shadow: var(--golge);
                transition: transform .35s ease, border-color .35s ease, box-shadow .35s ease;
            }

            .kart:hover { transform: translateY(-8px); border-color: var(--vurgu); box-shadow: var(--golge), var(--golge-vurgu); }
            .kart-gorsel { aspect-ratio: 4 / 3; overflow: hidden; background: #efe6d2; }
            .kart-gorsel img, .detay-medya img { width: 100%; height: 100%; object-fit: cover; display: block; }
            .kart-icerik { padding: 1rem; }
            .kart-ust {
                display: flex;
                justify-content: space-between;
                gap: 1rem;
                font-size: 0.82rem;
            }
            .kart h3 { margin: 0.75rem 0 0.5rem; }
            .kart p { margin: 0 0 0.75rem; }
            .kart-rozetler {
                grid-template-columns: repeat(auto-fit, minmax(120px, max-content));
                margin-bottom: 0.9rem;
            }
            .rozet, .etiket-liste li {
                display: inline-flex;
                align-items: center;
                justify-content: center;
                padding: 0.55rem 0.85rem;
                border-radius: 999px;
                border: 1px solid var(--kenar);
                background: rgba(255,255,255,0.78);
                backdrop-filter: blur(10px);
            }

            .geri-link, .buton {
                display: inline-flex;
                align-items: center;
                justify-content: center;
                text-decoration: none;
            }

            .geri-link {
                margin-bottom: 1rem;
                color: var(--metin);
                font-weight: 600;
            }

            .detay-grid {
                display: grid;
                grid-template-columns: minmax(0, 1.05fr) minmax(320px, 0.95fr);
                gap: 1.25rem;
                align-items: stretch;
            }

            .detay-panel, .detay-medya, .bos-durum {
                padding: 1.5rem;
            }

            .detay-medya {
                padding: 0;
                overflow: hidden;
                min-height: 420px;
                position: relative;
                isolation: isolate;
            }

            .detay-medya::after {
                content: "";
                position: absolute;
                inset: auto 0 0 0;
                height: 38%;
                background: linear-gradient(180deg, transparent, rgba(36,24,12,0.22));
                z-index: 1;
                pointer-events: none;
            }

            .detay-medya img {
                animation: mediaNefes 8s ease-in-out infinite;
                transition: transform .5s ease;
            }

            .detay-medya:hover img {
                transform: scale(1.04);
            }

            .detay-medya-parilti {
                position: absolute;
                inset: 0;
                background:
                    linear-gradient(115deg, transparent 20%, rgba(255,255,255,0.08) 38%, rgba(239,215,161,0.22) 52%, transparent 68%);
                mix-blend-mode: screen;
                animation: altinParilti 7s ease-in-out infinite;
                z-index: 2;
                pointer-events: none;
            }

            .detay-panel {
                position: relative;
                background:
                    linear-gradient(180deg, var(--yuzey) 0%, var(--yuzey-cam) 100%);
                backdrop-filter: blur(14px);
            }

            .detay-panel::before {
                content: "";
                position: absolute;
                inset: 0;
                border-radius: inherit;
                padding: 1px;
                background: linear-gradient(145deg, rgba(239,215,161,0.8), transparent 28%, transparent 72%, rgba(197,160,89,0.5));
                -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
                -webkit-mask-composite: xor;
                mask-composite: exclude;
                pointer-events: none;
            }

            .detay-ust-etiket {
                display: inline-flex;
                margin-bottom: 0.9rem;
                font-size: 0.76rem;
                letter-spacing: 0.18em;
                text-transform: uppercase;
                color: var(--vurgu);
            }

            .detay-vurgu-bandı {
                display: flex;
                flex-wrap: wrap;
                gap: 0.6rem;
                margin: 1rem 0 1.15rem;
            }

            .detay-vurgu-bandı span {
                padding: 0.55rem 0.8rem;
                border-radius: 999px;
                border: 1px solid rgba(197,160,89,0.24);
                background: rgba(255,255,255,0.52);
                color: var(--ikincil);
                font-size: 0.78rem;
                letter-spacing: 0.05em;
            }

            .etiket-liste {
                grid-template-columns: repeat(auto-fit, minmax(120px, max-content));
                list-style: none;
                padding: 0;
                margin: 0;
            }

            .olcu-grid {
                grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
            }

            .olcu-kart {
                display: flex;
                flex-direction: column;
                gap: 0.45rem;
                padding: 1rem;
                border-radius: 16px;
                border: 1px solid var(--kenar);
                background: rgba(255,255,255,0.78);
                transition: transform .35s ease, border-color .35s ease, box-shadow .35s ease;
            }

            .olcu-kart:hover {
                transform: translateY(-6px);
                border-color: rgba(197,160,89,0.65);
                box-shadow: var(--golge-vurgu);
            }

            .buton {
                min-height: 48px;
                padding: 0.9rem 1.2rem;
                border-radius: 14px;
                background: linear-gradient(135deg, var(--vurgu) 0%, var(--vurgu-2) 100%);
                color: #23170b;
                font-weight: 700;
                letter-spacing: 0.04em;
                transition: transform .3s ease, box-shadow .3s ease, filter .3s ease;
            }

            .buton:hover {
                transform: translateY(-3px);
                box-shadow: var(--golge-vurgu);
                filter: saturate(1.05);
            }

            .bos-durum {
                text-align: center;
                max-width: 680px;
                margin: 3rem auto 0;
            }

            @media (max-width: 960px) {
                .hero, .detay-grid { grid-template-columns: 1fr; }
                .koleksiyon-baslik { flex-direction: column; align-items: flex-start; }
            }
        </style>
    </head>
    <body>
        <main class="kabuk">
            {{govdeIcerigi}}
        </main>
    </body>
    </html>
    """;
}

static string HtmlKodla(string deger) => WebUtility.HtmlEncode(deger);

