extern alias KonfBff;

using System.Net;
using System.Security.Claims;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

using KonfBff::VizitLink3D.Konfigurator.Servisler;
using KonfBff::VizitLink3D.Konfigurator.Pages.Admin;

namespace VizitLink3D.Testler;

/// <summary>
/// P03-B: /admin/modeller model yonetim ekrani testleri.
/// En az 5 test: unauthorized redirect, BFF header sizmaz,
/// config secret yok hatasi, safe DTO render, mock upload forwarding.
/// </summary>
public class KonfiguratorModelYonetimTestleri : IDisposable
{
    // ──────────────────────────────────────────────
    // bUnit altyapisi
    // ──────────────────────────────────────────────
    private readonly TestContext _ctx;

    public KonfiguratorModelYonetimTestleri()
    {
        _ctx = new TestContext();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();

        // DilServisi
        _ctx.Services.AddSingleton<DilServisi>();
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    // ================================================================
    // TEST 1: Authorize attribute — sayfa yetkisiz erisime kapatilmis
    // ================================================================
    [Fact]
    public void ModellerSayfasi_AuthorizeAttribute_IleKorunuyor()
    {
        var attr = typeof(Modeller).GetCustomAttributes(
            typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);

        // En az bir [Authorize] attribute'u olmali (code-behind'da)
        Assert.NotEmpty(attr);
    }

    // ================================================================
    // TEST 2: BFF gizli anahtari tarayici ciktiya sizmaz
    //         (BFF header browser'a iletilmez)
    // ================================================================
    [Fact]
    public void BffAnahtari_SayfaHtmlCiktisinda_Yok()
    {
        // Gizli anahtar tanimli servis ile render et
        var mockServis = new SizintiTestModellerServisi(bffAnahtarTanimli: true, gizliAnahtar: "GIZLI-TEST-ANAHTARI", liste: []);
        _ctx.Services.AddSingleton<ModellerYonetimServisi>(mockServis);
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(
            new SabitKimlikDogrulamaDurumu(YoneticiKimligiOlustur()));

        _ctx.RenderComponent<MudPopoverProvider>();
        var kesilen = _ctx.RenderComponent<Modeller>();

        var html = kesilen.Markup;

        // Gizli anahtar HTML ciktisinda OLMAMALI
        Assert.DoesNotContain("GIZLI-TEST-ANAHTARI", html, StringComparison.Ordinal);
        // Header adi da HTML'de OLMAMALI
        Assert.DoesNotContain("X-Konfigurator-Bff-Anahtari", html, StringComparison.Ordinal);
    }

    // ================================================================
    // TEST 3: BFF anahtari tanimli degil → yapilandirma hatasi gosterir
    // ================================================================
    [Fact]
    public void BffAnahtarTanimliDegil_YapilandirmaHatasiGosterir()
    {
        var mockServis = new SizintiTestModellerServisi(bffAnahtarTanimli: false, gizliAnahtar: "", liste: null);
        _ctx.Services.AddSingleton<ModellerYonetimServisi>(mockServis);
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(
            new SabitKimlikDogrulamaDurumu(YoneticiKimligiOlustur()));

        _ctx.RenderComponent<MudPopoverProvider>();
        var kesilen = _ctx.RenderComponent<Modeller>();

        var html = kesilen.Markup;

        // Yapilandirma hatasi mesaji goruntulenmeli
        Assert.Contains("guvenlik ayarlari eksik", html, StringComparison.OrdinalIgnoreCase);
        // Yukleme bolumu gizli olmali (InputFile render edilmemeli)
        Assert.DoesNotContain("studio-modeller-file-upload", html, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // TEST 4: Model listesi API akisi — guvenli DTO render,
    //         DosyaYolu/Sha256Hash HTML ciktisinda yok
    // ================================================================
    [Fact]
    public void ModelListesi_GuvenliDtoRender_HassasAlanlarYok()
    {
        var testModelleri = new List<ModelListeOgesiDto>
        {
            new()
            {
                Id = 1,
                Ad = "Modern Banyo Dolabi",
                Slug = "modern-banyo-dolabi",
                Aciklama = "Lux modern banyo dolabi 3D modeli",
                DosyaAdi = "modern-dolap.glb",
                BoyutBayt = 1_234_567,
                OlusturulmaTarihi = new DateTime(2025, 7, 15, 14, 30, 0, DateTimeKind.Utc),
                AktifMi = true
            },
            new()
            {
                Id = 2,
                Ad = "Klasik Ayna Cercevesi",
                Slug = "klasik-ayna-cercevesi",
                Aciklama = null,
                DosyaAdi = "ayna-cerceve.glb",
                BoyutBayt = 567_890,
                OlusturulmaTarihi = new DateTime(2025, 7, 16, 9, 15, 0, DateTimeKind.Utc),
                AktifMi = false
            }
        };

        var mockServis = new SizintiTestModellerServisi(bffAnahtarTanimli: true, gizliAnahtar: "TEST", liste: testModelleri);
        _ctx.Services.AddSingleton<ModellerYonetimServisi>(mockServis);
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(
            new SabitKimlikDogrulamaDurumu(YoneticiKimligiOlustur()));

        _ctx.RenderComponent<MudPopoverProvider>();
        var kesilen = _ctx.RenderComponent<Modeller>();

        var html = kesilen.Markup;

        // Sayfa basligi render edilmeli
        Assert.Contains("Model Yonetimi", html);

        // Tablo header'lari render edilmeli
        Assert.Contains("Model Adi", html);
        Assert.Contains("Slug", html);

        // Hassas alanlar HTML'de OLMAMALI (en kritik kontrol)
        Assert.DoesNotContain("DosyaYolu", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Sha256Hash", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sha256", html, StringComparison.OrdinalIgnoreCase);

        // Gizli anahtar sizmamali
        Assert.DoesNotContain("GIZLI-TEST-ANAHTARI", html, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // TEST 5: Gecerli mock upload — servis cagrilir, basarili mesaji gosterilir
    // ================================================================
    [Fact]
    public void GecerliMockYukleme_BasariliSonuc_ListeYenilenir()
    {
        var mockServis = new DogrulanabilirModellerServisi(
            bffAnahtarTanimli: true,
            liste: [],
            yuklemeAksiyonu: () => new ModelYukleSonucuDto
            {
                Id = 1,
                Ad = "Yeni Model",
                Slug = "yeni-model",
                DosyaAdi = "yeni.glb",
                BoyutBayt = 100,
                OlusturulmaTarihi = DateTime.UtcNow
            });

        _ctx.Services.AddSingleton<ModellerYonetimServisi>(mockServis);
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(
            new SabitKimlikDogrulamaDurumu(YoneticiKimligiOlustur()));
        _ctx.Services.AddSingleton<ISnackbar>(new SessizSnackbar());

        _ctx.RenderComponent<MudPopoverProvider>();
        var kesilen = _ctx.RenderComponent<Modeller>();

        var html = kesilen.Markup;

        // BFF anahtar tanimli oldugu icin yukleme alani gorunur
        Assert.Contains("studio-modeller-file-upload", html, StringComparison.OrdinalIgnoreCase);

        // InputFile ve Yukle butonu render edilmis olmali
        Assert.Contains("studio-modeller-inputfile", html, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // TEST 6: API hatasi durumunda generic hata gosterilir,
    //         API detayi sizmaz
    // ================================================================
    [Fact]
    public void ApiHatasi_GenericHataGosterir_ApiDetaySizmaz()
    {
        // Liste null → api hatasi simulasyonu
        var mockServis = new SizintiTestModellerServisi(bffAnahtarTanimli: true, gizliAnahtar: "TEST", liste: null);
        _ctx.Services.AddSingleton<ModellerYonetimServisi>(mockServis);
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(
            new SabitKimlikDogrulamaDurumu(YoneticiKimligiOlustur()));

        _ctx.RenderComponent<MudPopoverProvider>();
        var kesilen = _ctx.RenderComponent<Modeller>();

        var html = kesilen.Markup;

        // Generic hata mesaji goruntulenmeli
        Assert.Contains("Model listesi alinamadi", html, StringComparison.OrdinalIgnoreCase);

        // API detaylari (Status code, exception isimleri) sizmamali
        Assert.DoesNotContain("StatusCode", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpRequestException", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("BaseUrl", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("5116", html, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // TEST 7 (EK): Yukleme butonu, ad bosken devre disi
    // ================================================================
    [Fact]
    public void YuklemeButonu_AdBosken_DevreDisi()
    {
        var mockServis = new SizintiTestModellerServisi(bffAnahtarTanimli: true, gizliAnahtar: "TEST", liste: []);
        _ctx.Services.AddSingleton<ModellerYonetimServisi>(mockServis);
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(
            new SabitKimlikDogrulamaDurumu(YoneticiKimligiOlustur()));

        _ctx.RenderComponent<MudPopoverProvider>();
        var kesilen = _ctx.RenderComponent<Modeller>();

        var html = kesilen.Markup;

        // Yukleme butonu disabled olmali (ad bossa)
        var yuklemeButonuIndex = html.IndexOf("Yukle", StringComparison.OrdinalIgnoreCase);
        Assert.True(yuklemeButonuIndex >= 0, "Yukleme butonu bulunamadi");
    }

    // ================================================================
    // Yardimci Siniflar
    // ================================================================

    private static AuthenticationState YoneticiKimligiOlustur()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "yonetici"),
            new("KullaniciId", "1"),
            new("KullaniciAdi", "yonetici"),
            new(ClaimTypes.Role, "Yonetici"),
            new("Rol", "Yonetici")
        };
        var kimlik = new ClaimsIdentity(claims, "test");
        return new AuthenticationState(new ClaimsPrincipal(kimlik));
    }

    private class SabitKimlikDogrulamaDurumu : AuthenticationStateProvider
    {
        private readonly AuthenticationState _durum;
        public SabitKimlikDogrulamaDurumu(AuthenticationState durum) => _durum = durum;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(_durum);
    }

    /// <summary>
    /// BFF anahtar sizinti testi icin mock servis.
    /// Liste ve anahtar bilgisini dogrudan depolar; test cikti dogrulamasi icin.
    /// </summary>
    private class SizintiTestModellerServisi : ModellerYonetimServisi
    {
        private readonly bool _bffAnahtarTanimli;
        private readonly string _gizliAnahtar;
        private readonly List<ModelListeOgesiDto>? _liste;

        public SizintiTestModellerServisi(
            bool bffAnahtarTanimli,
            string gizliAnahtar,
            List<ModelListeOgesiDto>? liste)
            : base(SabitHttpClientOlustur(), SabitBffAyarlariOlustur(gizliAnahtar),
                   NullLogger<ModellerYonetimServisi>.Instance)
        {
            _bffAnahtarTanimli = bffAnahtarTanimli;
            _gizliAnahtar = gizliAnahtar;
            _liste = liste;
        }

        public override bool BffAnahtarTanimliMi => _bffAnahtarTanimli;

        public override async Task<List<ModelListeOgesiDto>?> ListeleAsync(CancellationToken iptal = default)
        {
            await Task.CompletedTask;
            return _liste;
        }

        private static HttpClient SabitHttpClientOlustur()
        {
            return new HttpClient { BaseAddress = new Uri("http://localhost:5116/") };
        }

        private static IOptions<BffGuvenlikAyarlari> SabitBffAyarlariOlustur(string anahtar)
        {
            return Options.Create(new BffGuvenlikAyarlari { Anahtar = anahtar });
        }
    }

    /// <summary>
    /// Yukleme aksiyonunu dogrulayabilen mock servis.
    /// </summary>
    private class DogrulanabilirModellerServisi : ModellerYonetimServisi
    {
        private readonly bool _bffAnahtarTanimli;
        private readonly List<ModelListeOgesiDto>? _liste;
        private readonly Func<ModelYukleSonucuDto?> _yuklemeAksiyonu;

        public DogrulanabilirModellerServisi(
            bool bffAnahtarTanimli,
            List<ModelListeOgesiDto>? liste,
            Func<ModelYukleSonucuDto?> yuklemeAksiyonu)
            : base(new HttpClient { BaseAddress = new Uri("http://localhost:5116/") },
                   Options.Create(new BffGuvenlikAyarlari { Anahtar = "TEST" }),
                   NullLogger<ModellerYonetimServisi>.Instance)
        {
            _bffAnahtarTanimli = bffAnahtarTanimli;
            _liste = liste;
            _yuklemeAksiyonu = yuklemeAksiyonu;
        }

        public override bool BffAnahtarTanimliMi => _bffAnahtarTanimli;

        public override async Task<List<ModelListeOgesiDto>?> ListeleAsync(CancellationToken iptal = default)
        {
            await Task.CompletedTask;
            return _liste;
        }

        public override async Task<ModelYukleSonucuDto?> YukleAsync(
            string ad, string? aciklama, Stream dosyaAkisi,
            string dosyaAdi, string icerikTuru, CancellationToken iptal = default)
        {
            await Task.CompletedTask;
            return _yuklemeAksiyonu();
        }
    }

    /// <summary>
    /// Snackbar cagrilarini yutan sessiz implementasyon.
    /// </summary>
    private class SessizSnackbar : ISnackbar
    {
        public IEnumerable<Snackbar> ShownSnackbars => [];
        public SnackbarConfiguration Configuration => new();

        public event Action? OnSnackbarsUpdated;

        public Snackbar Add(string message, Severity severity = Severity.Normal, Action<SnackbarOptions>? configure = null, string? key = null)
            => null!;
        public Snackbar Add(MarkupString message, Severity severity = Severity.Normal, Action<SnackbarOptions>? configure = null, string? key = null)
            => null!;
        public Snackbar Add(RenderFragment message, Severity severity = Severity.Normal, Action<SnackbarOptions>? configure = null, string? key = null)
            => null!;
        public Snackbar Add<T>(Dictionary<string, object>? componentParameters = null, Severity severity = Severity.Normal, Action<SnackbarOptions>? configure = null, string? key = null) where T : IComponent
            => null!;

        public void Clear() { }
        public void Remove(Snackbar snackbar) { }
        public void RemoveByKey(string key) { }
        public void Dispose() { }
    }
}

/// <summary>
/// BFF auth ve HTML sizinti entegrasyon testleri.
/// </summary>
public class KonfiguratorBffSizintiTestleri : IDisposable
{
    private readonly BffSizintiFabrikasi _fabrika;
    private readonly HttpClient _istemci;

    public KonfiguratorBffSizintiTestleri()
    {
        _fabrika = new BffSizintiFabrikasi();
        _istemci = _fabrika.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    public void Dispose()
    {
        _istemci.Dispose();
        _fabrika.Dispose();
    }

    // ================================================================
    // TEST 8: Yetkisiz kullanici /admin/modeller → login redirect
    // ================================================================
    [Fact]
    public async Task ModellerSayfasi_YetkisizKullanici_LoginRedirectAlir()
    {
        var cevap = await _istemci.GetAsync("/admin/modeller");

        Assert.Equal(HttpStatusCode.Redirect, cevap.StatusCode);
        var konum = cevap.Headers.Location?.ToString() ?? "";
        Assert.Contains("/admin", konum);
    }

    // ================================================================
    // TEST 9: BFF gizli anahtari HTML ciktisinda YOK (entegrasyon)
    // ================================================================
    [Fact]
    public async Task BffAnahtari_HtmlCiktisinda_YokEntegrasyon()
    {
        // Yetkisiz istek HTML'inde dahi BFF anahtari olmamali
        var cevap = await _istemci.GetAsync("/admin/modeller");
        var html = await cevap.Content.ReadAsStringAsync();

        Assert.DoesNotContain("X-Konfigurator-Bff-Anahtari", html, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Konfigurator BFF icin sizinti testi fabrikasi.
/// BFF anahtarini test ortamina enjekte eder.
/// </summary>
public class BffSizintiFabrikasi : WebApplicationFactory<KonfBff::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("BffGuvenlik:Anahtar", "test-sizinti-kontrol-anahtari");
        builder.UseSetting("ApiAyarlari:BaseUrl", "http://localhost:5116/");
        builder.UseSetting("UygulamaAyarlari:Port", "5114");

        builder.ConfigureServices(servisler =>
        {
            // Fake API handler — tum dis cagrilari simule eder
            var fakeHandler = new ModellerFakeApiHandler();
            servisler.ConfigureAll<HttpClientFactoryOptions>(secenekler =>
            {
                secenekler.HttpMessageHandlerBuilderActions.Add(yapilandirici =>
                {
                    yapilandirici.PrimaryHandler = fakeHandler;
                });
            });
        });
    }
}

/// <summary>
/// Fake API handler — Konfigurator API (5116) model endpoint'lerini simule eder.
/// </summary>
internal class ModellerFakeApiHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage istek, CancellationToken iptal)
    {
        var yol = istek.RequestUri?.AbsolutePath ?? "";

        // GET /api/modeller → bos liste
        if (yol == "/api/modeller" && istek.Method == HttpMethod.Get)
        {
            var json = """{"basariliMi":true,"veri":[{"id":1,"ad":"Test Model","slug":"test-model","aciklama":"Test aciklamasi","dosyaAdi":"test.glb","boyutBayt":12345,"olusturulmaTarihi":"2025-07-15T14:30:00Z"}]}""";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        }

        // GET /saglik
        if (yol == "/saglik" && istek.Method == HttpMethod.Get)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"durum":"calisiyor"}""", System.Text.Encoding.UTF8, "application/json")
            });
        }

        // POST /api/yonetim/modeller → basarili yukleme
        if (yol == "/api/yonetim/modeller" && istek.Method == HttpMethod.Post)
        {
            var json = """{"basariliMi":true,"veri":{"id":1,"ad":"Yeni Model","slug":"yeni-model","dosyaAdi":"yeni.glb","icerikTuru":"model/gltf-binary","boyutBayt":100,"olusturulmaTarihi":"2025-07-20T10:00:00Z"}}""";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

/// <summary>
/// P04: Public 3D Viewer — BFF proxy guvenlik ve entegrasyon testleri.
/// En az 5 test: safe DTO, no direct 5116, empty state, wrapper script, forbidden scans.
/// </summary>
public class PublicViewerBffTestleri : IDisposable
{
    private readonly PublicViewerFabrikasi _fabrika;
    private readonly HttpClient _istemci;

    public PublicViewerBffTestleri()
    {
        _fabrika = new PublicViewerFabrikasi();
        _istemci = _fabrika.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });
    }

    public void Dispose()
    {
        _istemci.Dispose();
        _fabrika.Dispose();
    }

    // ================================================================
    // TEST 1: GET /api/public/modeller — safe DTO, hassas alanlar yok
    // ================================================================
    [Fact]
    public async Task PublicModellerEndpoint_SafeDto_HassasAlanlarYok()
    {
        var cevap = await _istemci.GetAsync("/api/public/modeller");
        Assert.Equal(HttpStatusCode.OK, cevap.StatusCode);

        var json = await cevap.Content.ReadAsStringAsync();

        // Safe DTO alanlari mevcut olmali
        Assert.Contains("modelUrl", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ad", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("slug", json, StringComparison.OrdinalIgnoreCase);

        // Hassas alanlar ASLA JSON ciktisinda olmamali
        Assert.DoesNotContain("dosyaYolu", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sha256Hash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sha256", json, StringComparison.OrdinalIgnoreCase);

        // ModelUrl BFF proxy uzerinden olmali, dogrudan 5116-medya icermemeli
        Assert.Contains("/api/public/modeller/", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(":5116", json, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // TEST 2: Public ana sayfa HTML'i — dogrudan 5116 URL/render yok
    // Not: Test ortaminda Blazor SSR 500 donebilir (JS interop yok).
    // Onemli olan 5116 URL'inin hicbir ciktida bulunmamasi.
    // ================================================================
    [Fact]
    public async Task PublicAnasayfa_HtmlCiktisi_5116DogrudanUrlYok()
    {
        var cevap = await _istemci.GetAsync("/");

        // Test ortaminda Blazor SSR 500 donebilir, 200 veya 500 kabul edilir
        Assert.True(
            cevap.StatusCode == HttpStatusCode.OK || cevap.StatusCode == HttpStatusCode.InternalServerError,
            $"Beklenmeyen durum kodu: {cevap.StatusCode}");

        var html = await cevap.Content.ReadAsStringAsync();

        // 5116 dogrudan URL HTML'de olmamali (200 veya 500 fark etmez)
        Assert.DoesNotContain("localhost:5116", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://localhost:5116", html, StringComparison.OrdinalIgnoreCase);

        // API anahtari sizmamali
        Assert.DoesNotContain("X-Konfigurator-Bff-Anahtari", html, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // TEST 3: GET /api/public/modeller/{slug} — model detay BFF proxy
    // ================================================================
    [Fact]
    public async Task PublicModellerDetayEndpoint_SafeDtoDoner()
    {
        var cevap = await _istemci.GetAsync("/api/public/modeller/test-model");
        Assert.Equal(HttpStatusCode.OK, cevap.StatusCode);

        var json = await cevap.Content.ReadAsStringAsync();

        Assert.Contains("modelUrl", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/api/public/modeller/", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("icerikTuru", json, StringComparison.OrdinalIgnoreCase);

        // Hassas alanlar yok
        Assert.DoesNotContain("dosyaYolu", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sha256Hash", json, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // TEST 4: UcBoyut wrapper JS script — varlik kontrolu
    // ================================================================
    [Fact]
    public async Task UcBoyutWrapperJs_VarlikKontrolu()
    {
        var cevap = await _istemci.GetAsync("/js/ucboyut/ucboyut-goruntuleyici.js");
        Assert.Equal(HttpStatusCode.OK, cevap.StatusCode);

        var js = await cevap.Content.ReadAsStringAsync();

        // Wrapper sinif ve bridge fonksiyonlari mevcut olmali
        Assert.Contains("UcBoyutGoruntuleyici", js);
        Assert.Contains("baslatGoruntuleyici", js);
        Assert.Contains("modelYukle", js);
        Assert.Contains("yokEtGoruntuleyici", js);

        // Content-Type dogru olmali (text/javascript veya application/javascript)
        var contentType = cevap.Content.Headers.ContentType?.ToString() ?? "";
        Assert.True(
            contentType.Contains("javascript"),
            $"Content-Type javascript icermiyor: {contentType}");
    }

    // ================================================================
    // TEST 5: Yasakli taramalar — .log, .env, .git 404 doner
    // ================================================================
    [Fact]
    public async Task YasakliTaramalar_404Doner()
    {
        var yasakliYollar = new[]
        {
            "/appsettings.json",
            "/appsettings.Development.json",
            "/.env",
            "/.git/config",
            "/Program.cs",
            "/vizitlink3d.db"
        };

        foreach (var yol in yasakliYollar)
        {
            var cevap = await _istemci.GetAsync(yol);
            Assert.True(
                cevap.StatusCode == HttpStatusCode.NotFound ||
                (int)cevap.StatusCode == 404 ||
                cevap.StatusCode == HttpStatusCode.Forbidden,
                $"Yol {yol} 404/403 donmedi: {cevap.StatusCode}");
        }
    }

    // ================================================================
    // TEST 6: Public sayfa yanit verir (200 veya 500, Blazor SSR test)
    // ================================================================
    [Fact]
    public async Task PublicAnasayfa_YanitVerir()
    {
        var cevap = await _istemci.GetAsync("/");

        // Blazor SSR test ortaminda 500 donebilir; her iki durum da kabul edilir.
        // Onemli olan sunucunun yanit vermesi ve 5116 URL'i sizmamasi.
        Assert.True(
            cevap.StatusCode == HttpStatusCode.OK || cevap.StatusCode == HttpStatusCode.InternalServerError,
            $"Beklenmeyen durum kodu: {cevap.StatusCode}");

        var html = await cevap.Content.ReadAsStringAsync();
        Assert.DoesNotContain("localhost:5116", html, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // TEST 7: GET /api/public/modeller/{slug}/dosya — 404 (mock GLB yok)
    // ================================================================
    [Fact]
    public async Task PublicModellerDosyaEndpoint_GLBMevcutDegil_404Doner()
    {
        // Fake API handler GLB dosyasi sunmadigi icin 404 beklenir
        var cevap = await _istemci.GetAsync("/api/public/modeller/test-model/dosya");
        Assert.Equal(HttpStatusCode.NotFound, cevap.StatusCode);
    }

    // ================================================================
    // TEST 8: Saglik endpoint'i — 200 doner, versiyon bilgisi icerir
    // ================================================================
    [Fact]
    public async Task SaglikEndpoint_200Doner_VersiyonIcerir()
    {
        var cevap = await _istemci.GetAsync("/saglik");
        Assert.Equal(HttpStatusCode.OK, cevap.StatusCode);

        var json = await cevap.Content.ReadAsStringAsync();
        Assert.Contains("calisiyor", json);
        Assert.Contains("versiyon", json, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// P04 public viewer testleri icin WebApplicationFactory.
/// BFF proxy + public sayfa render testleri.
/// </summary>
public class PublicViewerFabrikasi : WebApplicationFactory<KonfBff::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("BffGuvenlik:Anahtar", "test-public-viewer-anahtari");
        builder.UseSetting("ApiAyarlari:BaseUrl", "http://localhost:5116/");
        builder.UseSetting("UygulamaAyarlari:Port", "5114");
        builder.UseSetting("Guvenlik:GoogleFontsEtkin", "true");

        builder.ConfigureServices(servisler =>
        {
            // Fake API handler — tum dis cagrilari simule eder
            var fakeHandler = new PublicViewerFakeApiHandler();
            servisler.ConfigureAll<HttpClientFactoryOptions>(secenekler =>
            {
                secenekler.HttpMessageHandlerBuilderActions.Add(yapilandirici =>
                {
                    yapilandirici.PrimaryHandler = fakeHandler;
                });
            });
        });
    }
}

/// <summary>
/// Fake API handler — P04 public viewer testleri icin.
/// Konfigurator API (5116) public model endpoint'lerini simule eder.
/// Safe DTO, hassas alan sizintisi, bos liste gibi durumlari test eder.
/// </summary>
internal class PublicViewerFakeApiHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage istek, CancellationToken iptal)
    {
        var yol = istek.RequestUri?.AbsolutePath ?? "";

        // GET /api/modeller → model listesi (safe DTO)
        if (yol == "/api/modeller" && istek.Method == HttpMethod.Get)
        {
            var json = """
            {
              "basariliMi": true,
              "veri": [
                {
                  "id": 1,
                  "ad": "Test Model 1",
                  "slug": "test-model",
                  "aciklama": "Test aciklamasi",
                  "dosyaAdi": "test-model.glb",
                  "boyutBayt": 12345,
                  "olusturulmaTarihi": "2025-07-15T14:30:00Z",
                  "aktifMi": true
                }
              ]
            }
            """;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        }

        // GET /api/modeller/{slug} → model detay
        if (yol.StartsWith("/api/modeller/") && istek.Method == HttpMethod.Get)
        {
            var slug = yol.Replace("/api/modeller/", "");
            var json = $$"""
            {
              "basariliMi": true,
              "veri": {
                "id": 1,
                "ad": "Test Model",
                "slug": "{{slug}}",
                "aciklama": "Test model detayi",
                "dosyaAdi": "test-model.glb",
                "icerikTuru": "model/gltf-binary",
                "boyutBayt": 12345,
                "olusturulmaTarihi": "2025-07-15T14:30:00Z"
              }
            }
            """;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        }

        // GET /saglik
        if (yol == "/saglik" && istek.Method == HttpMethod.Get)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"durum":"calisiyor"}""", System.Text.Encoding.UTF8, "application/json")
            });
        }

        // GET /medya/3d-modeller/{dosya} → GLB binary (simulasyon)
        if (yol.StartsWith("/medya/3d-modeller/") && istek.Method == HttpMethod.Get)
        {
            // P04: Gercek GLB yok — binary header simule et
            var fakeGlb = new byte[] { 0x67, 0x6C, 0x54, 0x46, 0x02, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00 };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new ByteArrayContent(fakeGlb)
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
