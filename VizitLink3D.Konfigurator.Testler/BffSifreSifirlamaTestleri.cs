using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VizitLink3D.Konfigurator.Testler;

/// <summary>
/// BFF sifre sifirlama endpoint'leri icin entegrasyon testleri.
/// WebApplicationFactory ile Konfigurator BFF ayaga kaldirilir.
/// API calismadigi durumda endpoint'lerin dogru yonlendirme yaptigi test edilir.
/// </summary>
public class BffSifreSifirlamaTestleri : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrika;

    public BffSifreSifirlamaTestleri()
    {
        _fabrika = new WebApplicationFactory<Program>();
    }

    /// <summary>
    /// GET /sifre-sifirla sayfasindan antiforgery token'ini ayiklar.
    /// </summary>
    private async Task<(HttpClient Istemci, string CsrfToken, string CsrfCookie)> CsrfBilgisiAlAsync()
    {
        var istemci = _fabrika.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var sayfaYanit = await istemci.GetAsync("/sifre-sifirla");
        var html = await sayfaYanit.Content.ReadAsStringAsync();

        // Antiforgery hidden input degerini ayikla
        var eslesme = Regex.Match(html,
            @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");

        var csrfToken = eslesme.Success ? eslesme.Groups[1].Value : "";

        // Antiforgery cookie'yi ayikla (set-cookie header'indan)
        var csrfCookie = "";
        if (sayfaYanit.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var cookieHeaders = cookies.ToList();
            // .AspNetCore.Antiforgery ile baslayan cookie'yi bul
            foreach (var c in cookieHeaders)
            {
                var parts = c.Split(';')[0]; // name=value kismi
                if (parts.StartsWith(".AspNetCore.Antiforgery", StringComparison.OrdinalIgnoreCase))
                {
                    csrfCookie = parts;
                    break;
                }
            }
        }

        return (istemci, csrfToken, csrfCookie);
    }

    // ──────────────────────────────────────────────
    // TEST 1: Antiforgery olmadan POST -> 400
    // ──────────────────────────────────────────────
    [Fact]
    public async Task SifreSifirlamaIstegi_AntiforgeryYoksa_400Doner()
    {
        // Arrange
        var istemci = _fabrika.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var icerik = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("email", "test@example.com")
        });

        // Act
        var yanit = await istemci.PostAsync("/oturum/sifre-sifirlama-istegi", icerik);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
    }

    // ──────────────────────────────────────────────
    // TEST 2: Sifre yenile antiforgery olmadan POST -> 400
    // ──────────────────────────────────────────────
    [Fact]
    public async Task SifreYenile_AntiforgeryYoksa_400Doner()
    {
        // Arrange
        var istemci = _fabrika.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var icerik = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", "test-token"),
            new KeyValuePair<string, string>("yeniSifre", "YeniSifre1!")
        });

        // Act
        var yanit = await istemci.PostAsync("/oturum/sifre-yenile", icerik);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
    }

    // ──────────────────────────────────────────────
    // TEST 3: Gecerli antiforgery ile sifre sifirlama istegi -> yonlendirme
    // API calismadigi icin catch blogu calisir ve generic basariliya yonlendirir
    // ──────────────────────────────────────────────
    [Fact]
    public async Task SifreSifirlamaIstegi_AntiforgeryGecerli_BasariliyaYonlendirir()
    {
        // Arrange
        var (istemci, csrfToken, _) = await CsrfBilgisiAlAsync();

        Assert.False(string.IsNullOrEmpty(csrfToken), "CSRF token HTML'den ayiklanamadi.");

        var icerik = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("email", "test@example.com"),
            new KeyValuePair<string, string>("__RequestVerificationToken", csrfToken)
        });

        // Act
        var yanit = await istemci.PostAsync("/oturum/sifre-sifirlama-istegi", icerik);

        // Assert: API olmasa bile catch blogu generic basariliya yonlendirir
        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        Assert.NotNull(yanit.Headers.Location);
        var konum = yanit.Headers.Location.OriginalString;
        Assert.Contains("/sifre-sifirla", konum);
        Assert.Contains("durum=basarili", konum);
    }

    // ──────────────────────────────────────────────
    // TEST 4: Sifre yenile - sifreler eslesmezse API cagrisi yapilmaz
    // ──────────────────────────────────────────────
    [Fact]
    public async Task SifreYenile_SifrelerEslesmezse_HataRedirectVeApiCagrilmaz()
    {
        // Arrange
        var (istemci, csrfToken, _) = await CsrfBilgisiAlAsync();

        Assert.False(string.IsNullOrEmpty(csrfToken), "CSRF token HTML'den ayiklanamadi.");

        // Şifreler kasıtlı olarak eşleşmiyor
        var icerik = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", "gecerli-token"),
            new KeyValuePair<string, string>("yeniSifre", "GucluSifre1!"),
            new KeyValuePair<string, string>("yeniSifreTekrar", "FarkliSifre2@"),
            new KeyValuePair<string, string>("__RequestVerificationToken", csrfToken)
        });

        // Act
        var yanit = await istemci.PostAsync("/oturum/sifre-yenile", icerik);

        // Assert: API çağrısı yapılmaz, direkt hata redirect
        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        Assert.NotNull(yanit.Headers.Location);
        var konum = yanit.Headers.Location.OriginalString;
        Assert.Contains("/sifre-yenile", konum);
        Assert.Contains("durum=sifreler-eslesmiyor", konum);
    }

    // ──────────────────────────────────────────────
    // TEST 5: Sifre yenile - sifreler eslesirse normal akis devam eder
    // (API calismadigi icin basarisiz olur ama BFF eslesme kontrolunu gecer)
    // ──────────────────────────────────────────────
    [Fact]
    public async Task SifreYenile_SifrelerEslesirse_AkaryisDevamEder()
    {
        // Arrange
        var (istemci, csrfToken, _) = await CsrfBilgisiAlAsync();

        Assert.False(string.IsNullOrEmpty(csrfToken), "CSRF token HTML'den ayiklanamadi.");

        var icerik = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", "test-token"),
            new KeyValuePair<string, string>("yeniSifre", "GucluSifre1!"),
            new KeyValuePair<string, string>("yeniSifreTekrar", "GucluSifre1!"),
            new KeyValuePair<string, string>("__RequestVerificationToken", csrfToken)
        });

        // Act
        var yanit = await istemci.PostAsync("/oturum/sifre-yenile", icerik);

        // Assert: Şifreler eşleştiği için BFF kontrolünü geçer,
        // API'ye istek gider (API çalışmadığı için catch → basarisiz)
        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        Assert.NotNull(yanit.Headers.Location);
        var konum = yanit.Headers.Location.OriginalString;
        Assert.Contains("/sifre-yenile", konum);
        // Eşleşme hatası DEĞİL (normal akış devam etti)
        Assert.DoesNotContain("durum=sifreler-eslesmiyor", konum);
    }

    // ──────────────────────────────────────────────
    // TEST 6: Sifre sifirlama istegi bos email -> basarisiz yonlendirme
    // ──────────────────────────────────────────────
    [Fact]
    public async Task SifreSifirlamaIstegi_BosEmail_BasarisizaYonlendirir()
    {
        // Arrange
        var (istemci, csrfToken, _) = await CsrfBilgisiAlAsync();

        var icerik = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("email", ""),
            new KeyValuePair<string, string>("__RequestVerificationToken", csrfToken)
        });

        // Act
        var yanit = await istemci.PostAsync("/oturum/sifre-sifirlama-istegi", icerik);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        Assert.NotNull(yanit.Headers.Location);
        var konum = yanit.Headers.Location.OriginalString;
        Assert.Contains("/sifre-sifirla", konum);
        Assert.Contains("durum=basarisiz", konum);
    }

    // ──────────────────────────────────────────────
    // TEST 7: GET /sifre-sifirla sayfasi basariyla yuklenir
    // ──────────────────────────────────────────────
    [Fact]
    public async Task SifreSifirlaSayfasi_Yuklenir()
    {
        // Arrange
        var istemci = _fabrika.CreateClient();

        // Act
        var yanit = await istemci.GetAsync("/sifre-sifirla");

        // Assert
        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        var html = await yanit.Content.ReadAsStringAsync();
        Assert.Contains("Sifre Sifirlama", html, StringComparison.OrdinalIgnoreCase);
    }

    // ──────────────────────────────────────────────
    // TEST 8: GET /sifre-yenile sayfasi token olmadan uyari gosterir
    // ──────────────────────────────────────────────
    [Fact]
    public async Task SifreYenileSayfasi_TokenYok_UyariGosterir()
    {
        // Arrange
        var istemci = _fabrika.CreateClient();

        // Act
        var yanit = await istemci.GetAsync("/sifre-yenile");

        // Assert
        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        var html = await yanit.Content.ReadAsStringAsync();
        Assert.Contains("Token bulunamadi", html, StringComparison.OrdinalIgnoreCase);
    }
}
