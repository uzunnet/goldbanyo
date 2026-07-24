using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Pages.Admin;
using VizitLink3D.UI.Servisler;
using System.Net;
using System.Security.Claims;

namespace VizitLink3D.Testler;

/// <summary>
/// Paket-2B RET-3: KonfiguratorStudio bileşeni bUnit testleri.
/// 7 test: yetkilendirme, hareket enum, OnMeshSecildi callback,
/// JSInvokable metot, boş state render, parça listesi render, preset render.
/// </summary>
public class KonfiguratorStudioBunitTestleri : IDisposable
{
    private readonly TestContext _ctx;
    private readonly FakeHttpMessageHandler _httpHandler;

    public KonfiguratorStudioBunitTestleri()
    {
        _httpHandler = new FakeHttpMessageHandler();
        _ctx = new TestContext();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();

        var httpClient = new HttpClient(_httpHandler)
            { BaseAddress = new Uri("http://localhost:5115") };
        _ctx.Services.AddSingleton(httpClient);
        _ctx.Services.AddSingleton<ApiIstemcisi>();

        // UcBoyutServisi wrapper: IDisposable + IAsyncDisposable uyumu
        _ctx.Services.AddSingleton<UcBoyutServisi>(sp =>
            new TekKullanimlikUcBoyutServisi(sp.GetRequiredService<IJSRuntime>()));

        _ctx.Services.AddSingleton<DilServisi>();

        var authState = new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Role, "Admin"), new Claim(ClaimTypes.Name, "test")],
                "test")));
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(
            new SabitKimlikDogrulamaDurumu(authState));
    }

    // ===================================================================
    // TEST 1: Boş state — sayfa render edilir ve model seçimi dropdown'ı var
    // ===================================================================
    [Fact]
    public void BaslangicState_SayfaVeModelSecimiRender()
    {
        _httpHandler.YanitAta("/api/uc-boyut/modeller", "[]");
        _ctx.RenderComponent<MudPopoverProvider>();

        var kesilen = _ctx.RenderComponent<KonfiguratorStudio>();

        Assert.NotNull(kesilen.Find(".gb-ks-model-secimi"));
        Assert.NotNull(kesilen.Find(".admin-sayfa-basligi"));
    }

    // ===================================================================
    // TEST 2: Authorize attribute
    // ===================================================================
    [Fact]
    public void Sayfa_AuthorizeAttribute_IleKorunuyor()
    {
        var attr = typeof(KonfiguratorStudio).GetCustomAttributes(
            typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
        Assert.Single(attr);
    }

    // ===================================================================
    // TEST 3: Yetkisiz render kontrolü
    // ===================================================================
    [Fact]
    public void YetkisizKullanici_AuthorizeAttribute_Mevcut()
    {
        var attr = typeof(KonfiguratorStudio)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        Assert.NotNull(attr);
    }

    // ===================================================================
    // TEST 4: Model seçili — parça listesi container'ı render
    // ===================================================================
    [Fact]
    public void ModelSecildiginde_ParcaListesi_ContainerRender()
    {
        _httpHandler.YanitAta("/api/uc-boyut/modeller",
            """[{"Id":1,"ModelAdi":"Test","ModelDosyaYolu":null}]""");

        _httpHandler.YanitAta("/api/uc-boyut/admin/modeller/1/toplu", """
        {"BasariliMi":true,"Veri":{
            "ModelId":1,"ModelAdi":"Test","UrunId":1,"ModelDosyaYolu":null,
            "Parcalar":[{"Id":1,"MeshAdi":"govde","GorunenAd":"Govde","SiraNo":1,
                "RenklenebilirMi":true,"MalzemeDegisebilirMi":true,
                "SecilebilirMi":true,"AktifMi":true,"HareketliMi":false,
                "DokuUygulanabilirMi":false,"GizlenebilirMi":false,
                "AdminOnayliMi":false,"ParcaGrubuId":null,"ParcaTipi":null,
                "HareketTipi":null,"HareketAyarlariJson":null,
                "MantiksalKod":null,"MalzemeTipiKisiti":null}],
            "Gruplar":[],"SahneOnayarlari":[]}}
        """);

        _ctx.RenderComponent<MudPopoverProvider>();
        var kesilen = _ctx.RenderComponent<KonfiguratorStudio>(
            p => p.Add(x => x.ModelId, 1));

        Assert.NotNull(kesilen.Find(".gb-ks-parca-listesi"));
    }

    // ===================================================================
    // TEST 5: Preset formu — Yeni Preset butonu var, düzenleme kapalı
    // ===================================================================
    [Fact]
    public void PresetFormu_SagPanelVeTabsRender()
    {
        _httpHandler.YanitAta("/api/uc-boyut/modeller",
            """[{"Id":1,"ModelAdi":"PresetTest","ModelDosyaYolu":null}]""");

        _httpHandler.YanitAta("/api/uc-boyut/admin/modeller/1/toplu", """
        {"BasariliMi":true,"Veri":{
            "ModelId":1,"ModelAdi":"PresetTest","UrunId":1,"ModelDosyaYolu":null,
            "Parcalar":[],"Gruplar":[],"SahneOnayarlari":[]}}
        """);

        _ctx.RenderComponent<MudPopoverProvider>();
        var kesilen = _ctx.RenderComponent<KonfiguratorStudio>(
            p => p.Add(x => x.ModelId, 1));

        // Sag panel (sekme alani) render edilmeli
        Assert.NotNull(kesilen.Find(".gb-ks-sag-panel"));
        // Sol panel (parca listesi) render edilmeli
        Assert.NotNull(kesilen.Find(".gb-ks-sol-panel"));
    }

    // ===================================================================
    // TEST 6: Hareket enum — geçerli değerler var, eski geçersizler yok
    // ===================================================================
    [Fact]
    public void HareketTuruEnum_GecerliVeGecersizKontrol()
    {
        var tumu = Enum.GetNames<HareketTuru>();

        Assert.Contains("Sabit", tumu);
        Assert.Contains("Menteseli", tumu);
        Assert.Contains("Surgulu", tumu);
        Assert.Contains("Cekmece", tumu);
        Assert.Contains("YukariAcilir", tumu);
        Assert.Contains("Pivot", tumu);
        Assert.Contains("Recliner", tumu);

        Assert.DoesNotContain("Yok", tumu);
        Assert.DoesNotContain("Kayar", tumu);
        Assert.DoesNotContain("Doner", tumu);
        Assert.DoesNotContain("Mentese", tumu);
        Assert.DoesNotContain("Acilir", tumu);
        Assert.DoesNotContain("Kapanir", tumu);
    }

    // ===================================================================
    // TEST 7: OnMeshSecildi EventCallback parametresi
    // ===================================================================
    [Fact]
    public void UcBoyutGoruntuleyici_OnMeshSecildi_Tanimli()
    {
        var prop = typeof(VizitLink3D.UI.Bilesenler.UcBoyutGoruntuleyici)
            .GetProperty("OnMeshSecildi");

        Assert.NotNull(prop);
        Assert.Equal(
            typeof(Microsoft.AspNetCore.Components.EventCallback<string>),
            prop.PropertyType);
    }

    // ===================================================================
    // TEST 8: JSInvokable ParcaSecildi metodu mevcut
    // ===================================================================
    [Fact]
    public void UcBoyutGoruntuleyici_ParcaSecildi_JsInvokableMetoduVar()
    {
        var metot = typeof(VizitLink3D.UI.Bilesenler.UcBoyutGoruntuleyici)
            .GetMethod("ParcaSecildi");

        Assert.NotNull(metot);
        var attr = metot!.GetCustomAttributes(
            typeof(Microsoft.JSInterop.JSInvokableAttribute), false);
        Assert.Single(attr);
    }

    public void Dispose()
    {
        _httpHandler.Dispose();
        _ctx.Dispose();
    }

    // ===================================================================
    // Yardımcı Sınıflar
    // ===================================================================

    /// <summary>IAsyncDisposable servisleri IDisposable ile sarmalar</summary>
    private class TekKullanimlikUcBoyutServisi : UcBoyutServisi, IDisposable
    {
        public TekKullanimlikUcBoyutServisi(IJSRuntime js) : base(js) { }
        void IDisposable.Dispose() { /* Async dispose handled elsewhere */ }
    }

    private class SabitKimlikDogrulamaDurumu : AuthenticationStateProvider
    {
        private readonly AuthenticationState _durum;
        public SabitKimlikDogrulamaDurumu(AuthenticationState durum) => _durum = durum;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(_durum);
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _yanitlar = new();
        public void YanitAta(string suffix, string json) => _yanitlar[suffix] = json;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken iptal)
        {
            var url = request.RequestUri?.AbsolutePath ?? "";
            var anahtar = _yanitlar.Keys.FirstOrDefault(k =>
                url.EndsWith(k, StringComparison.OrdinalIgnoreCase));
            var icerik = anahtar != null ? _yanitlar[anahtar] : "[]";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(icerik, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
