extern alias KonfBff;

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System.Security.Claims;
using KonfBff::VizitLink3D.Konfigurator.Pages.Admin;
using KonfBff::VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Testler;

/// <summary>
/// Konfigurator Giris sayfasi Query parametre ve render testleri.
/// Hata duzeltmesi: query["ReturnUrl"] KeyNotFoundException → TryGetValue.
/// </summary>
public class KonfiguratorGirisTestleri : IDisposable
{
    private readonly TestContext _ctx;

    public KonfiguratorGirisTestleri()
    {
        _ctx = new TestContext();
        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton<DilServisi>();

        // Kimlik dogrulama yapilmamis (giris sayfasi)
        var authState = new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity()));
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(
            new SabitKimlikDogrulamaDurumu(authState));
    }

    // ===================================================================
    // TEST 1: /admin query olmadan — KeyNotFoundException yok, default returnUrl
    // ===================================================================
    [Fact]
    public void AdminSadece_QueryYokken_KeyNotFoundFirlatmaz_VeSayfaRenderOlur()
    {
        // NavigationManager'i /admin (query yok) olarak ayarla
        var navMan = _ctx.Services.GetRequiredService<NavigationManager>();
        navMan.NavigateTo("http://localhost:5114/admin");

        // Render — eskiden query["ReturnUrl"] KeyNotFoundException firlatirdi
        var kesilen = _ctx.RenderComponent<Giris>();

        // Sayfa render edilmis olmali
        Assert.NotNull(kesilen.Find(".studio-login-scene"));
        Assert.NotNull(kesilen.Find(".studio-login-card"));

        // Hidden input'taki returnUrl default olarak /admin/dashboard olmali
        var returnUrlInput = kesilen.Find("input[name='returnUrl']");
        Assert.Equal("/admin/dashboard", returnUrlInput.GetAttribute("value"));
    }

    // ===================================================================
    // TEST 2: /admin?ReturnUrl=/admin/ayarlar — gecerli ReturnUrl kullanilir
    // ===================================================================
    [Fact]
    public void Admin_GecerliReturnUrlIle_OReturnUrlKullanilir()
    {
        var navMan = _ctx.Services.GetRequiredService<NavigationManager>();
        navMan.NavigateTo("http://localhost:5114/admin?ReturnUrl=%2Fadmin%2Fayarlar");

        var kesilen = _ctx.RenderComponent<Giris>();

        var returnUrlInput = kesilen.Find("input[name='returnUrl']");
        Assert.Equal("/admin/ayarlar", returnUrlInput.GetAttribute("value"));
    }

    // ===================================================================
    // TEST 3: /admin?ReturnUrl=https://evil.com — open redirect engeli
    // ===================================================================
    [Fact]
    public void Admin_DisReturnUrlIle_DefaultKullanilir_OpenRedirectEngeli()
    {
        var navMan = _ctx.Services.GetRequiredService<NavigationManager>();
        navMan.NavigateTo("http://localhost:5114/admin?ReturnUrl=https%3A%2F%2Fevil.com");

        var kesilen = _ctx.RenderComponent<Giris>();

        var returnUrlInput = kesilen.Find("input[name='returnUrl']");
        Assert.Equal("/admin/dashboard", returnUrlInput.GetAttribute("value"));
    }

    // ===================================================================
    // TEST 4: /admin?ReturnUrl=//evil.com — double-slash open redirect engeli
    // ===================================================================
    [Fact]
    public void Admin_CiftSlashReturnUrlIle_DefaultKullanilir_OpenRedirectEngeli()
    {
        var navMan = _ctx.Services.GetRequiredService<NavigationManager>();
        navMan.NavigateTo("http://localhost:5114/admin?ReturnUrl=%2F%2Fevil.com");

        var kesilen = _ctx.RenderComponent<Giris>();

        var returnUrlInput = kesilen.Find("input[name='returnUrl']");
        Assert.Equal("/admin/dashboard", returnUrlInput.GetAttribute("value"));
    }

    // ===================================================================
    // TEST 5: /admin?hata=giris_basarisiz — hata mesaji gosterilir, default returnUrl
    // ===================================================================
    [Fact]
    public void Admin_HataParametresiIle_HataMesajiGosterilir_ReturnUrlDefault()
    {
        var navMan = _ctx.Services.GetRequiredService<NavigationManager>();
        navMan.NavigateTo("http://localhost:5114/admin?hata=giris_basarisiz");

        var kesilen = _ctx.RenderComponent<Giris>();

        // returnUrl default olmali
        var returnUrlInput = kesilen.Find("input[name='returnUrl']");
        Assert.Equal("/admin/dashboard", returnUrlInput.GetAttribute("value"));

        // Hata mesaji (MudAlert) render edilmeli
        // MudAlert bUnit'te mud-alert CSS sinifi ile render edilir
        var uyari = kesilen.Find(".mud-alert");
        Assert.NotNull(uyari);
        Assert.Contains("hatali", uyari.TextContent, StringComparison.OrdinalIgnoreCase);
    }

    // ===================================================================
    // TEST 6: QueryHelpers.TryGetValue birim testi — dogrudan fix mantigi
    // ===================================================================
    [Theory]
    [InlineData("", null)]                                       // Bos query
    [InlineData("foo=bar", null)]                                 // Ilgisiz parametre
    [InlineData("ReturnUrl=", null)]                               // Bos ReturnUrl (gecersiz sayilir)
    [InlineData("ReturnUrl=%2Fadmin%2Fayarlar", "/admin/ayarlar")] // Gecerli
    [InlineData("ReturnUrl=https%3A%2F%2Fevil.com", null)]        // Harici (reddedilir)
    [InlineData("ReturnUrl=%2F%2Fevil.com", null)]                // Double-slash (reddedilir)
    public void QueryHelper_TryGetValue_KeyNotFoundVermez(string queryString, string? beklenenReturnUrl)
    {
        // Bu test dogrudan fix edilen kodu taklit eder:
        // query.TryGetValue yerine query["ReturnUrl"] kullansaydi,
        // ilk iki senaryoda KeyNotFoundException firlatirdi.
        var query = QueryHelpers.ParseQuery(queryString);

        // KeyNotFoundException firlatmamali
        query.TryGetValue("ReturnUrl", out var returnUrlValues);
        var qReturnUrl = returnUrlValues.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(qReturnUrl)
            && qReturnUrl.StartsWith('/')
            && !qReturnUrl.StartsWith("//"))
        {
            Assert.Equal(beklenenReturnUrl, qReturnUrl);
        }
        else
        {
            // Gecersiz veya bos → default "/admin/dashboard" kullanilmali
            Assert.Null(beklenenReturnUrl);
        }
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    // ===================================================================
    // Yardimci Sinif
    // ===================================================================
    private class SabitKimlikDogrulamaDurumu : AuthenticationStateProvider
    {
        private readonly AuthenticationState _durum;
        public SabitKimlikDogrulamaDurumu(AuthenticationState durum) => _durum = durum;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(_durum);
    }
}
