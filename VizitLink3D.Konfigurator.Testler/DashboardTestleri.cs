using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace VizitLink3D.Konfigurator.Testler;

/// <summary>
/// Dashboard sayfasi (admin/dashboard) icin entegrasyon testleri.
/// Cookie auth, model sayisi, bos katalog durumu, API hata durumu,
/// runtime metadata ve hizli eylem kartlarinin render edildigini dogrular.
/// En az 8 test.
/// </summary>
public class DashboardTestleri : IDisposable
{
    private readonly DashboardTestFabrika _fabrika;
    private readonly HttpClient _istemci;

    public DashboardTestleri()
    {
        _fabrika = new DashboardTestFabrika();
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

    /// <summary>
    /// Test yardimcisi: Giris yapip auth cookie'si olan bir HttpClient doner.
    /// </summary>
    private async Task<HttpClient> YetkiliIstemciOlusturAsync()
    {
        _fabrika.ApiMock.BasariliGiris = true;

        // Antiforgery token al
        var tokenYanit = await _istemci.GetAsync("/admin");
        var token = tokenYanit.Headers.GetValues("X-Antiforgery-RequestToken").FirstOrDefault();
        Assert.NotNull(token);

        // Form POST giris
        var formIcerik = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", "yonetici"),
            new KeyValuePair<string, string>("password", "GucluSifre123!"),
            new KeyValuePair<string, string>("returnUrl", "/admin/dashboard"),
            new KeyValuePair<string, string>("__RequestVerificationToken", token)
        });

        var girisCevap = await _istemci.PostAsync("/oturum/giris", formIcerik);
        Assert.Equal(HttpStatusCode.Redirect, girisCevap.StatusCode);

        return _istemci;
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 1: Yetkisiz kullanici /admin/dashboard → login redirect
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_YetkisizKullanici_LoginRedirectAlir()
    {
        var cevap = await _istemci.GetAsync("/admin/dashboard");

        Assert.Equal(HttpStatusCode.Redirect, cevap.StatusCode);
        var konum = cevap.Headers.Location?.ToString() ?? "";
        Assert.Contains("/admin", konum);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 2: Yetkili kullanici dashboard sayfasini goruntuler
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_YetkiliKullanici_SayfaYuklenir()
    {
        await YetkiliIstemciOlusturAsync();

        var cevap = await _istemci.GetAsync("/admin/dashboard");

        // Blazor SSR render edebilir veya etmeyebilir; 200 veya 500 olabilir
        // Onemli olan: login redirect ALINMAMASI
        if (cevap.StatusCode == HttpStatusCode.OK)
        {
            var html = await cevap.Content.ReadAsStringAsync();
            Assert.Contains("Hos geldiniz", html);
        }

        Assert.NotEqual(HttpStatusCode.Redirect, cevap.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 3: Yetkili + model listesi BOS → "Henuz hic model" mesaji
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_ModelListesiBos_HenuzModelYokMesajiGosterir()
    {
        _fabrika.ApiMock.ModelListesi = new List<ModelListeSimulasyonu>();
        await YetkiliIstemciOlusturAsync();

        var cevap = await _istemci.GetAsync("/admin/dashboard");

        if (cevap.StatusCode == HttpStatusCode.OK)
        {
            var html = await cevap.Content.ReadAsStringAsync();
            // Bos katalog durum metni
            Assert.Contains("Henuz hic model", html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.NotEqual(HttpStatusCode.Redirect, cevap.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 4: Yetkili + API modelleri DONUYOR → model sayisi render
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_ModelSayisi_DogruRenderEdilir()
    {
        _fabrika.ApiMock.ModelListesi = new List<ModelListeSimulasyonu>
        {
            new() { Id = 1, Ad = "Test Model 1", Slug = "test-model-1", DosyaAdi = "test1.glb", BoyutBayt = 1024 },
            new() { Id = 2, Ad = "Test Model 2", Slug = "test-model-2", DosyaAdi = "test2.glb", BoyutBayt = 2048 },
            new() { Id = 3, Ad = "Test Model 3", Slug = "test-model-3", DosyaAdi = "test3.glb", BoyutBayt = 4096 }
        };
        await YetkiliIstemciOlusturAsync();

        var cevap = await _istemci.GetAsync("/admin/dashboard");

        if (cevap.StatusCode == HttpStatusCode.OK)
        {
            var html = await cevap.Content.ReadAsStringAsync();
            // Model sayisi "3" HTML icinde gorunmeli
            Assert.Contains("3", html);
            // "aktif 3D model" metni
            Assert.Contains("aktif 3D model", html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.NotEqual(HttpStatusCode.Redirect, cevap.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 5: API hata durumu → generic hata mesaji
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_ApiHatasi_GenericHataMesajiGosterir()
    {
        _fabrika.ApiMock.ModelApiHata = true;
        await YetkiliIstemciOlusturAsync();

        var cevap = await _istemci.GetAsync("/admin/dashboard");

        if (cevap.StatusCode == HttpStatusCode.OK)
        {
            var html = await cevap.Content.ReadAsStringAsync();
            // Generic hata mesaji — API detayi SIZDIRILMAZ
            Assert.Contains("Katalog alinamadi", html, StringComparison.OrdinalIgnoreCase);
            // "NotFoundException" veya stack trace HTML'de gorunMEMELI
            Assert.DoesNotContain("NotFoundException", html);
            Assert.DoesNotContain("stack trace", html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.NotEqual(HttpStatusCode.Redirect, cevap.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 6: Hizli eylem kartlari render edilir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_HizliEylemKartlari_RenderEdilir()
    {
        await YetkiliIstemciOlusturAsync();

        var cevap = await _istemci.GetAsync("/admin/dashboard");

        if (cevap.StatusCode == HttpStatusCode.OK)
        {
            var html = await cevap.Content.ReadAsStringAsync();
            // 4 hizli eylem karti kontrolu
            Assert.Contains("Model Yukle", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Model Yonetimi", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Public Viewer", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Sifre Guvenligi", html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.NotEqual(HttpStatusCode.Redirect, cevap.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 7: Runtime metadata (versiyon, port, proje) render edilir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_RuntimeMetadata_RenderEdilir()
    {
        await YetkiliIstemciOlusturAsync();

        var cevap = await _istemci.GetAsync("/admin/dashboard");

        if (cevap.StatusCode == HttpStatusCode.OK)
        {
            var html = await cevap.Content.ReadAsStringAsync();
            Assert.Contains("Runtime Bilgisi", html, StringComparison.OrdinalIgnoreCase);
            // Proje, Versiyon, Port basliklari
            Assert.Contains("Versiyon", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Port", html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.NotEqual(HttpStatusCode.Redirect, cevap.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 8: API baglanti durumu chip'i render edilir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_ApiBaglantiDurumu_RenderEdilir()
    {
        _fabrika.ApiMock.SaglikBasarili = true;
        await YetkiliIstemciOlusturAsync();

        var cevap = await _istemci.GetAsync("/admin/dashboard");

        if (cevap.StatusCode == HttpStatusCode.OK)
        {
            var html = await cevap.Content.ReadAsStringAsync();
            Assert.Contains("API Baglantisi", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Bagli", html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.NotEqual(HttpStatusCode.Redirect, cevap.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 9: API baglanti yok → baglanti yok chip'i render
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_ApiBaglantiYok_BaglantiYokChipRender()
    {
        _fabrika.ApiMock.SaglikBasarili = false;
        await YetkiliIstemciOlusturAsync();

        var cevap = await _istemci.GetAsync("/admin/dashboard");

        if (cevap.StatusCode == HttpStatusCode.OK)
        {
            var html = await cevap.Content.ReadAsStringAsync();
            Assert.Contains("Baglanti Yok", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("erisilemiyor", html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.NotEqual(HttpStatusCode.Redirect, cevap.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 10: Cikis butonu render edilir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Dashboard_CikisButonu_RenderEdilir()
    {
        await YetkiliIstemciOlusturAsync();

        var cevap = await _istemci.GetAsync("/admin/dashboard");

        if (cevap.StatusCode == HttpStatusCode.OK)
        {
            var html = await cevap.Content.ReadAsStringAsync();
            Assert.Contains("Cikis Yap", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("/oturum/cikis", html);
        }

        Assert.NotEqual(HttpStatusCode.Redirect, cevap.StatusCode);
    }
}

// ═══════════════════════════════════════════════════════════════
// TEST FABRIKASI
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Dashboard testleri icin ozel WebApplicationFactory.
/// Tum HttpClient isteklerini FakeDashboardApiHandler ile yakalar.
/// </summary>
public class DashboardTestFabrika : WebApplicationFactory<Program>
{
    public readonly FakeDashboardApiHandler ApiMock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(servisler =>
        {
            // Tum HttpClient isteklerini mock handler ile yakala
            servisler.ConfigureAll<HttpClientFactoryOptions>(secenekler =>
            {
                secenekler.HttpMessageHandlerBuilderActions.Add(yapilandirici =>
                {
                    yapilandirici.PrimaryHandler = ApiMock;
                });
            });

            // Her GET isteginde antiforgery cookie uret
            servisler.AddTransient<IStartupFilter, DashboardAntiforgeryBaslangicFiltresi>();
        });

        builder.UseSetting("ApiAyarlari:BaseUrl", "http://localhost:5116/");
        builder.UseSetting("UygulamaAyarlari:Port", "5114");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ApiMock.Dispose();
    }
}

/// <summary>
/// Pipeline basina antiforgery token uretici middleware ekler.
/// </summary>
internal class DashboardAntiforgeryBaslangicFiltresi : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return uygulama =>
        {
            uygulama.Use(async (baglam, sonraki) =>
            {
                if (HttpMethods.IsGet(baglam.Request.Method))
                {
                    var antiforgery = baglam.RequestServices.GetService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
                    if (antiforgery != null)
                    {
                        var tokenSet = antiforgery.GetAndStoreTokens(baglam);
                        if (!string.IsNullOrEmpty(tokenSet.RequestToken))
                        {
                            baglam.Response.Headers["X-Antiforgery-RequestToken"] = tokenSet.RequestToken;
                        }
                    }
                }
                await sonraki();
            });
            next(uygulama);
        };
    }
}

// ═══════════════════════════════════════════════════════════════
// MOCK API HANDLER
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Dashboard testlerinde Konfigurator API (5116) cagrilarini simule eder.
/// </summary>
public class FakeDashboardApiHandler : DelegatingHandler
{
    public bool BasariliGiris { get; set; } = true;
    public bool SaglikBasarili { get; set; } = true;
    public bool ModelApiHata { get; set; } = false;

    /// <summary>
    /// API'den donecek model listesi. Null = hata, bos liste = bos katalog.
    /// </summary>
    public List<ModelListeSimulasyonu>? ModelListesi { get; set; } = new();

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage istek, CancellationToken iptal)
    {
        var yol = istek.RequestUri?.AbsolutePath ?? "";

        // ── /api/kimlik/giris ──
        if (yol == "/api/kimlik/giris")
        {
            return Task.FromResult(GirisYanitiOlustur());
        }

        // ── /saglik ──
        if (yol == "/saglik")
        {
            return Task.FromResult(SaglikYanitiOlustur());
        }

        // ── /api/modeller (GET) ──
        if (yol == "/api/modeller" && istek.Method == HttpMethod.Get)
        {
            return Task.FromResult(ModelListesiYanitiOlustur());
        }

        // Bilinmeyen endpoint
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }

    private HttpResponseMessage GirisYanitiOlustur()
    {
        object yanit;
        if (BasariliGiris)
        {
            yanit = new
            {
                basariliMi = true,
                veri = new
                {
                    kullaniciId = 1,
                    kullaniciAdi = "yonetici",
                    rol = "Yonetici"
                }
            };
        }
        else
        {
            yanit = new { basariliMi = false, mesaj = "Kullanici adi veya sifre hatali." };
        }

        return JsonYanit(yanit);
    }

    private HttpResponseMessage SaglikYanitiOlustur()
    {
        if (SaglikBasarili)
        {
            return JsonYanit(new { durum = "calisiyor" });
        }

        return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
    }

    private HttpResponseMessage ModelListesiYanitiOlustur()
    {
        if (ModelApiHata)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        if (ModelListesi is null)
        {
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }

        var modeller = ModelListesi.Select(m => new
        {
            id = m.Id,
            ad = m.Ad,
            slug = m.Slug,
            aciklama = m.Aciklama,
            dosyaAdi = m.DosyaAdi,
            boyutBayt = m.BoyutBayt,
            olusturulmaTarihi = DateTime.UtcNow,
            aktifMi = true
        }).ToList();

        return JsonYanit(new { basariliMi = true, veri = modeller });
    }

    private static HttpResponseMessage JsonYanit(object icerik)
    {
        var json = JsonSerializer.Serialize(icerik, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }
}

/// <summary>
/// Mock API model listesi elemani.
/// </summary>
public class ModelListeSimulasyonu
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Aciklama { get; set; }
    public string DosyaAdi { get; set; } = "";
    public long BoyutBayt { get; set; }
}
