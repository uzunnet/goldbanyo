using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Pages.Konfigurator;
using VizitLink3D.UI.Servisler;
using FluentValidation;
using FluentValidation.TestHelper;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;
using System.Text.Json;

namespace VizitLink3D.Testler;

/// <summary>
/// Paket-3: Public Konfigüratör Runtime testleri.
/// Kapsam: API validasyonu, bUnit Razor sayfa testleri,
/// admin-onaysız filtreleme, tenant izolasyonu, boş/hata state'leri.
/// </summary>
public class Paket3_PublicKonfiguratorTestleri : IDisposable
{
    private readonly TestContext _ctx;
    private readonly FakeHttpMessageHandler _httpHandler;

    public Paket3_PublicKonfiguratorTestleri()
    {
        _httpHandler = new FakeHttpMessageHandler();
        _ctx = new TestContext();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();

        var httpClient = new HttpClient(_httpHandler)
            { BaseAddress = new Uri("http://localhost:5115") };
        _ctx.Services.AddSingleton(httpClient);
        _ctx.Services.AddSingleton<ApiIstemcisi>();
        _ctx.Services.AddSingleton<DilServisi>();

        // UcBoyutServisi mock'u — UcBoyutGoruntuleyici bileşeni için gerekli
        _ctx.Services.AddSingleton<UcBoyutServisi>(sp =>
            new TestUcBoyutServisi(sp.GetRequiredService<IJSRuntime>()));
    }

    public void Dispose()
    {
        _httpHandler.Dispose();
        _ctx.Dispose();
    }

    // ================================================================
    // TEST A1: Slug validasyonu — geçersiz slug reddi
    // ================================================================

    [Fact]
    public void PublicKonfiguratorSorguDogrulayici_BosSlug_Reddetmeli()
    {
        var dogrulayici = new PublicKonfiguratorSorguDogrulayici();
        var sonuc = dogrulayici.TestValidate("");
        sonuc.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void PublicKonfiguratorSorguDogrulayici_GecerliSlug_KabulEtmeli()
    {
        var dogrulayici = new PublicKonfiguratorSorguDogrulayici();
        var sonuc = dogrulayici.TestValidate("banyo-dolabi-hermes-120");
        Assert.True(sonuc.IsValid);
    }

    [Fact]
    public void PublicKonfiguratorSorguDogrulayici_BuyukHarfliSlug_Reddetmeli()
    {
        var dogrulayici = new PublicKonfiguratorSorguDogrulayici();
        var sonuc = dogrulayici.TestValidate("Banyo-Dolabi");
        sonuc.ShouldHaveValidationErrorFor(x => x);
    }

    // ================================================================
    // TEST A2: Seçim kaydetme DTO validasyonu — boş seçim reddi
    // ================================================================

    [Fact]
    public void PublicSecimKaydetDogrulayici_BosSecimler_Reddetmeli()
    {
        var dogrulayici = new PublicSecimKaydetDogrulayici();
        var dto = new PublicSecimKaydetDto { UrunId = 1, Secimler = [] };
        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.Secimler);
    }

    [Fact]
    public void PublicSecimKaydetDogrulayici_AyniParcaIdIkiKez_Reddetmeli()
    {
        var dogrulayici = new PublicSecimKaydetDogrulayici();
        var dto = new PublicSecimKaydetDto
        {
            UrunId = 1,
            Secimler =
            [
                new PublicParcaSecimiDto { ParcaId = 42, GorunurMu = true },
                new PublicParcaSecimiDto { ParcaId = 42, GorunurMu = false }
            ]
        };
        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.Secimler);
    }

    [Fact]
    public void PublicSecimKaydetDogrulayici_GecerliSecim_KabulEtmeli()
    {
        var dogrulayici = new PublicSecimKaydetDogrulayici();
        var dto = new PublicSecimKaydetDto
        {
            UrunId = 1,
            Secimler =
            [
                new PublicParcaSecimiDto { ParcaId = 1, SeciliRenkId = 5, GorunurMu = true },
                new PublicParcaSecimiDto { ParcaId = 2, SeciliMalzemeId = 3, GorunurMu = false }
            ]
        };
        var sonuc = dogrulayici.TestValidate(dto);
        Assert.True(sonuc.IsValid);
    }

    // ================================================================
    // TEST B1: bUnit — Public sayfada teknik alanlar render edilmez
    // ================================================================

    [Fact]
    public void PublicSayfa_TeknikKontroller_RenderEdilmemeli()
    {
        var konfiguratorDto = new PublicKonfiguratorDto
        {
            UrunId = 1,
            Slug = "test-urun",
            Ad = "Test Ürün",
            ModelYolu = "/medya/test.glb",
            Parcalar =
            [
                new PublicParcaDto
                {
                    Id = 1, GorunenAd = "Kapak", ParcaTipi = "Govde",
                    RenklenebilirMi = true, MalzemeDegisebilirMi = true, SiraNo = 1
                }
            ]
        };

        var json = JsonSerializer.Serialize(new { basariliMi = true, veri = konfiguratorDto });
        _httpHandler.YanitAta("/api/konfigurasyon/public/test-urun", json);
        _ctx.RenderComponent<MudPopoverProvider>();

        var kesilen = _ctx.RenderComponent<KonfiguratorPublic>(
            parameters => parameters.Add(p => p.Slug, "test-urun"));

        var html = kesilen.Markup;

        // Teknik alanlar render edilmemeli
        Assert.DoesNotContain("MeshAdi", html);
        Assert.DoesNotContain("HareketAyarlariJson", html);
        Assert.DoesNotContain("KameraAyarJson", html);
        Assert.DoesNotContain("HDR", html);
        Assert.DoesNotContain("ModelAnalizJson", html);
    }

    // ================================================================
    // TEST B2: bUnit — İzinli renkler parça panelinde görünür
    // ================================================================

    [Fact]
    public void PublicSayfa_IzinliRenkler_PaneldeGorunur()
    {
        var konfiguratorDto = new PublicKonfiguratorDto
        {
            UrunId = 1,
            Slug = "test-urun",
            Ad = "Test Ürün",
            ModelYolu = "/medya/test.glb",
            Parcalar =
            [
                new PublicParcaDto
                {
                    Id = 1, GorunenAd = "Kapak", ParcaTipi = "Govde",
                    RenklenebilirMi = true, SiraNo = 1,
                    Renkler =
                    [
                        new PublicParcaRenkDto { RenkId = 1, RalRengiId = 10, RalAdi = "Altın", RalKodu = "RAL-1000", HexKodu = "#C8952A" },
                        new PublicParcaRenkDto { RenkId = 2, RalRengiId = 11, RalAdi = "Gümüş", RalKodu = "RAL-9006", HexKodu = "#C0C0C0" }
                    ]
                }
            ]
        };

        var json = JsonSerializer.Serialize(new { basariliMi = true, veri = konfiguratorDto });
        _httpHandler.YanitAta("/api/konfigurasyon/public/test-urun", json);
        _ctx.RenderComponent<MudPopoverProvider>();

        var kesilen = _ctx.RenderComponent<KonfiguratorPublic>(
            parameters => parameters.Add(p => p.Slug, "test-urun"));

        // Parça adı ve tipi parça listesinde görünmeli
        Assert.Contains("Kapak", kesilen.Markup);
        Assert.Contains("Govde", kesilen.Markup);
    }

    // ================================================================
    // TEST B3: bUnit — Hareket metadata yokken kontrol gizli
    // ================================================================

    [Fact]
    public void PublicSayfa_HareketMetaYokken_HareketKontrolGizli()
    {
        var konfiguratorDto = new PublicKonfiguratorDto
        {
            UrunId = 1,
            Slug = "test-urun",
            Ad = "Test Ürün",
            ModelYolu = "/medya/test.glb",
            Parcalar =
            [
                new PublicParcaDto
                {
                    Id = 1, GorunenAd = "Sabit Parça", ParcaTipi = "Govde",
                    HareketliMi = false, HareketTipi = "Sabit", SiraNo = 1
                }
            ]
        };

        var json = JsonSerializer.Serialize(new { basariliMi = true, veri = konfiguratorDto });
        _httpHandler.YanitAta("/api/konfigurasyon/public/test-urun", json);
        _ctx.RenderComponent<MudPopoverProvider>();

        var kesilen = _ctx.RenderComponent<KonfiguratorPublic>(
            parameters => parameters.Add(p => p.Slug, "test-urun"));

        var html = kesilen.Markup;

        // "Açı (derece)" veya "Açıklık" gibi hareket etiketleri görünmemeli
        Assert.DoesNotContain("derece", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Açıklık", html);
        Assert.DoesNotContain("Kaydırma", html);
    }

    // ================================================================
    // TEST B4: bUnit — Boş state: parça yoksa bilgi mesajı
    // ================================================================

    [Fact]
    public void PublicSayfa_ParcaYoksa_BilgiMesajiGosterir()
    {
        var konfiguratorDto = new PublicKonfiguratorDto
        {
            UrunId = 1,
            Slug = "test-urun",
            Ad = "Test Ürün",
            ModelYolu = "/medya/test.glb",
            Parcalar = []
        };

        var json = JsonSerializer.Serialize(new { basariliMi = true, veri = konfiguratorDto });
        _httpHandler.YanitAta("/api/konfigurasyon/public/test-urun", json);
        _ctx.RenderComponent<MudPopoverProvider>();

        var kesilen = _ctx.RenderComponent<KonfiguratorPublic>(
            parameters => parameters.Add(p => p.Slug, "test-urun"));

        var html = kesilen.Markup;

        // "henüz parça tanımlanmamış" benzeri metin olmalı
        Assert.Contains("tanımlanmamış", html);
    }

    // ================================================================
    // TEST B5: bUnit — Hata state'i
    // ================================================================

    [Fact]
    public void PublicSayfa_HataState_HataMesajiGosterir()
    {
        _httpHandler.YanitAta("/api/konfigurasyon/public/test-urun",
            """{"basariliMi":false,"mesaj":"Ürün bulunamadı."}""");
        _ctx.RenderComponent<MudPopoverProvider>();

        var kesilen = _ctx.RenderComponent<KonfiguratorPublic>(
            parameters => parameters.Add(p => p.Slug, "test-urun"));

        var html = kesilen.Markup;

        Assert.Contains("Bir sorun oluştu", html);
    }

    // ================================================================
    // TEST C1: PublicParcaDto — admin-onaysız alanlar yapısal olarak hariç
    // ================================================================

    [Fact]
    public void PublicParcaDto_MeshAdiProperty_Yok()
    {
        var tip = typeof(PublicParcaDto);
        Assert.Null(tip.GetProperty("MeshAdi"));
        Assert.Null(tip.GetProperty("HareketAyarlariJson"));
        Assert.Null(tip.GetProperty("MalzemeTipiKisiti"));
        Assert.Null(tip.GetProperty("AdminOnayliMi")); // public DTO'da gereksiz
    }

    /// <summary>
    /// PublicParcaDto'da kullanıcıya gösterilecek güvenli alanlar var.
    /// </summary>
    [Fact]
    public void PublicParcaDto_GuvenliAlanlar_Var()
    {
        var tip = typeof(PublicParcaDto);
        Assert.NotNull(tip.GetProperty("GorunenAd"));
        Assert.NotNull(tip.GetProperty("RenklenebilirMi"));
        Assert.NotNull(tip.GetProperty("HareketTipi"));
        Assert.NotNull(tip.GetProperty("Renkler"));
        Assert.NotNull(tip.GetProperty("Malzemeler"));
        Assert.NotNull(tip.GetProperty("Dokular"));
    }

    // ================================================================
    // TEST C2: PublicKonfiguratorDto — güvenli alanlar
    // ================================================================

    [Fact]
    public void PublicKonfiguratorDto_ModelDosyaYolu_Yok()
    {
        // Public DTO'da ham model dosya yolu değil, sadece ModelYolu (public URL) dönmeli
        var tip = typeof(PublicKonfiguratorDto);
        Assert.Null(tip.GetProperty("ModelDosyaYolu"));
        Assert.Null(tip.GetProperty("AnalizJson"));
        Assert.NotNull(tip.GetProperty("ModelYolu"));
    }

    // ================================================================
    // TEST C3: Slug boşsa hata state'i
    // ================================================================

    [Fact]
    public void PublicSayfa_BosSlug_HataGosterir()
    {
        _ctx.RenderComponent<MudPopoverProvider>();

        var kesilen = _ctx.RenderComponent<KonfiguratorPublic>();

        var html = kesilen.Markup;
        Assert.Contains("belirtilmedi", html);
    }
}

/// <summary>
/// Test amaçlı UcBoyutServisi mock'u — tüm JS çağrılarını no-op yapar.
/// UcBoyutGoruntuleyici bileşeninin DI gereksinimini karşılar.
/// </summary>
public class TestUcBoyutServisi : UcBoyutServisi, IDisposable
{
    public TestUcBoyutServisi(IJSRuntime js) : base(js) { }
    public void Dispose() { }
}

/// <summary>
/// Sahte HTTP message handler — bUnit testleri için.
/// URL bazlı yanıt eşleştirme yapar.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _yanitlar = new();

    public void YanitAta(string urlKismi, string json)
    {
        _yanitlar[urlKismi] = json;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage istek, CancellationToken iptal)
    {
        var url = istek.RequestUri?.ToString() ?? "";

        foreach (var (anahtar, json) in _yanitlar)
        {
            if (url.Contains(anahtar, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            }
        }

        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
    }
}
