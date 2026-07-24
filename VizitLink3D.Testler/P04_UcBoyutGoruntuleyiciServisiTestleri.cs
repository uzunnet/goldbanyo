extern alias KonfBff;

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
using KonfBff::VizitLink3D.Konfigurator.Servisler;
using KonfBff::VizitLink3D.Konfigurator.Pages.Public;

namespace VizitLink3D.Testler;

/// <summary>
/// P04 ret duzeltmesi: UcBoyutGoruntuleyiciServisi birim testleri.
///
/// Kapsam: IUcBoyutGoruntuleyiciServisi DI kaydi, BaslatAsync/ModelYukleAsync/YokEtAsync
/// davranisi, IAsyncDisposable yasam dongusu, IJSRuntime soyutlamasi dogrulamasi.
/// En az 5 test.
/// </summary>
public class P04_UcBoyutGoruntuleyiciServisiTestleri : IDisposable
{
    private readonly TestContext _ctx;

    public P04_UcBoyutGoruntuleyiciServisiTestleri()
    {
        _ctx = new TestContext();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();

        // DI kaydi — Program.cs'teki ile ayni
        _ctx.Services.AddScoped<IUcBoyutGoruntuleyiciServisi, UcBoyutGoruntuleyiciServisi>();
        _ctx.Services.AddSingleton<DilServisi>();
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    // ================================================================
    // TEST 1: Servis DI uzerinden cozulebilir ve HazirMi false baslar
    // ================================================================
    [Fact]
    public void Servis_DI_Uzerinden_Cozulur_Ve_HazirMi_False()
    {
        var servis = _ctx.Services.GetRequiredService<IUcBoyutGoruntuleyiciServisi>();

        Assert.NotNull(servis);
        Assert.False(servis.HazirMi);
    }

    // ================================================================
    // TEST 2: BaslatAsync cagrildiginda HazirMi true olur
    // ================================================================
    [Fact]
    public async Task BaslatAsync_HazirMi_True_Yapar()
    {
        var servis = _ctx.Services.GetRequiredService<IUcBoyutGoruntuleyiciServisi>();
        var dotNetRef = new object(); // Gercek DotNetObjectReference yerine object yeterli

        await servis.BaslatAsync(dotNetRef, "test-canvas");

        Assert.True(servis.HazirMi);
    }

    // ================================================================
    // TEST 3: ModelYukleAsync hazir degilken hata firlatmaz (no-op)
    // ================================================================
    [Fact]
    public async Task ModelYukleAsync_HazirDegilken_HataFirlatmaz()
    {
        var servis = _ctx.Services.GetRequiredService<IUcBoyutGoruntuleyiciServisi>();

        // Henuz BaslatAsync cagrilmadi, HazirMi = false
        Assert.False(servis.HazirMi);

        // Hata firlatmamali — sessizce no-op yapmali
        var exception = await Record.ExceptionAsync(
            () => servis.ModelYukleAsync("/api/public/modeller/test/dosya"));

        Assert.Null(exception);
    }

    // ================================================================
    // TEST 4: BaslatAsync + ModelYukleAsync basarili akis
    // ================================================================
    [Fact]
    public async Task BaslatAsync_Ve_ModelYukleAsync_Basarili_Akis()
    {
        var servis = _ctx.Services.GetRequiredService<IUcBoyutGoruntuleyiciServisi>();

        // Baslat
        await servis.BaslatAsync(new object(), "test-canvas");
        Assert.True(servis.HazirMi);

        // Model yukle — bUnit Loose mode'da JS cagrilari sessizce basarili olur
        var exception = await Record.ExceptionAsync(
            () => servis.ModelYukleAsync("/api/public/modeller/test-model/dosya"));

        Assert.Null(exception);
        Assert.True(servis.HazirMi);
    }

    // ================================================================
    // TEST 5: YokEtAsync HazirMi'yi false yapar
    // ================================================================
    [Fact]
    public async Task YokEtAsync_HazirMi_False_Yapar()
    {
        var servis = _ctx.Services.GetRequiredService<IUcBoyutGoruntuleyiciServisi>();

        await servis.BaslatAsync(new object(), "test-canvas");
        Assert.True(servis.HazirMi);

        await servis.YokEtAsync();
        Assert.False(servis.HazirMi);
    }

    // ================================================================
    // TEST 6: IAsyncDisposable — DisposeAsync sonrasi HazirMi false
    // ================================================================
    [Fact]
    public async Task DisposeAsync_Sonrasi_HazirMi_False()
    {
        var servis = _ctx.Services.GetRequiredService<IUcBoyutGoruntuleyiciServisi>();

        await servis.BaslatAsync(new object(), "test-canvas");
        Assert.True(servis.HazirMi);

        await servis.DisposeAsync();

        // DisposeAsync sonrasi HazirMi false olmali
        Assert.False(servis.HazirMi);
    }

    // ================================================================
    // TEST 7: DisposeAsync iki kez cagrilirsa hata firlatmaz
    // ================================================================
    [Fact]
    public async Task DisposeAsync_IkiKez_Cagrilirsa_HataFirlatmaz()
    {
        var servis = _ctx.Services.GetRequiredService<IUcBoyutGoruntuleyiciServisi>();
        await servis.BaslatAsync(new object(), "test-canvas");

        await servis.DisposeAsync();

        // Ikinci dispose hata firlatmamali (_temizlendi flag'i)
        var exception = await Record.ExceptionAsync(
            async () => await servis.DisposeAsync());

        Assert.Null(exception);
    }

    // ================================================================
    // TEST 8: DisposeAsync sonrasi BaslatAsync ObjectDisposedException
    // ================================================================
    [Fact]
    public async Task DisposeAsync_Sonrasi_BaslatAsync_ObjectDisposedException()
    {
        var servis = _ctx.Services.GetRequiredService<IUcBoyutGoruntuleyiciServisi>();
        await servis.BaslatAsync(new object(), "test-canvas");
        await servis.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => servis.BaslatAsync(new object(), "test-canvas"));
    }
}

/// <summary>
/// P04 ret duzeltmesi: Anasayfa bileseni wrapper servisi ile testleri.
///
/// IJSRuntime dogrudan kullaniminin kaldirildigini ve
/// IUcBoyutGoruntuleyiciServisi'nin inject edildigini dogrular.
/// </summary>
public class P04_AnasayfaWrapperTestleri : IDisposable
{
    private readonly TestContext _ctx;
    private readonly SahteUcBoyutGoruntuleyiciServisi _sahteServis;

    public P04_AnasayfaWrapperTestleri()
    {
        _sahteServis = new SahteUcBoyutGoruntuleyiciServisi();

        _ctx = new TestContext();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();

        // P04: IJSRuntime yerine IUcBoyutGoruntuleyiciServisi inject edilir
        _ctx.Services.AddSingleton<IUcBoyutGoruntuleyiciServisi>(_sahteServis);

        // ModellerYonetimServisi mock — bos liste doner
        var mockModellerServisi = new TestModellerYonetimServisi();
        _ctx.Services.AddSingleton<ModellerYonetimServisi>(mockModellerServisi);
        _ctx.Services.AddSingleton<DilServisi>();
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    // ================================================================
    // TEST 9: Anasayfa render edilirken IJSRuntime dogrudan kullanilmaz
    // ================================================================
    [Fact]
    public void Anasayfa_Render_IJSRuntime_Kullanilmaz()
    {
        // IJSRuntime alaninin Anasayfa'da [Inject] edilmedigini dogrula
        var jsRuntimeAlanlari = typeof(Anasayfa)
            .GetFields(System.Reflection.BindingFlags.NonPublic |
                       System.Reflection.BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(IJSRuntime) ||
                        f.FieldType == typeof(IJSObjectReference));

        Assert.Empty(jsRuntimeAlanlari);
    }

    // ================================================================
    // TEST 10: Anasayfa IUcBoyutGoruntuleyiciServisi'ni inject eder
    // ================================================================
    [Fact]
    public void Anasayfa_GoruntuleyiciServisi_Inject_Edilir()
    {
        var ozellikler = typeof(Anasayfa)
            .GetProperties(System.Reflection.BindingFlags.NonPublic |
                           System.Reflection.BindingFlags.Instance);

        var servisOzelligi = ozellikler.FirstOrDefault(p =>
            p.PropertyType == typeof(IUcBoyutGoruntuleyiciServisi));

        Assert.NotNull(servisOzelligi);

        // [Inject] attribute'u olmali
        var injectAttr = servisOzelligi!.GetCustomAttributes(
            typeof(Microsoft.AspNetCore.Components.InjectAttribute), false);
        Assert.Single(injectAttr);
    }

    // ================================================================
    // TEST 11: Anasayfa render edildiginde sahte servis BaslatAsync cagrilir
    // ================================================================
    [Fact]
    public void Anasayfa_Render_Sonrasi_SahteServis_BaslatAsync_Cagrilir()
    {
        var kesilen = _ctx.RenderComponent<Anasayfa>();

        Assert.NotNull(kesilen);
        // Sahte servis BaslatAsync cagrildi mi?
        Assert.True(_sahteServis.BaslatAsyncCagrildiMi);
    }

    // ================================================================
    // TEST 12: Anasayfa DisposeAsync cagrildiginda servis DisposeAsync cagrilir
    // ================================================================
    [Fact]
    public async Task Anasayfa_DisposeAsync_Servis_DisposeAsync_Cagrilir()
    {
        var kesilen = _ctx.RenderComponent<Anasayfa>();
        var anasayfa = kesilen.Instance;

        // bUnit Dispose() IAsyncDisposable'i tetiklemez; manuel cagir
        await anasayfa.DisposeAsync();

        Assert.True(_sahteServis.DisposeAsyncCagrildiMi);
    }

    // ================================================================
    // TEST 13: Anasayfa model listesi bos — uygun mesaj gosterir
    // ================================================================
    [Fact]
    public void Anasayfa_ModelListesiBos_BilgiMesaji_Gosterir()
    {
        var kesilen = _ctx.RenderComponent<Anasayfa>();

        var html = kesilen.Markup;

        // Bos model listesi mesaji icermeli
        Assert.Contains("yuklenmemis", html, StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // Sahte / Yardimci Siniflar
    // ================================================================

    /// <summary>
    /// IUcBoyutGoruntuleyiciServisi'nin test amacli sahte implementasyonu.
    /// Tum metot cagrilarini kaydeder, gercek JS interop yapmaz.
    /// </summary>
    private class SahteUcBoyutGoruntuleyiciServisi : IUcBoyutGoruntuleyiciServisi
    {
        public bool BaslatAsyncCagrildiMi { get; private set; }
        public bool ModelYukleAsyncCagrildiMi { get; private set; }
        public bool YokEtAsyncCagrildiMi { get; private set; }
        public bool DisposeAsyncCagrildiMi { get; private set; }
        public bool HazirMi => BaslatAsyncCagrildiMi && !YokEtAsyncCagrildiMi;

        public Task BaslatAsync(object dotNetRef, string elemanId)
        {
            BaslatAsyncCagrildiMi = true;
            return Task.CompletedTask;
        }

        public Task ModelYukleAsync(string modelUrl)
        {
            ModelYukleAsyncCagrildiMi = true;
            return Task.CompletedTask;
        }

        public Task YokEtAsync()
        {
            YokEtAsyncCagrildiMi = true;
            return Task.CompletedTask;
        }

        public Task<string[]> MeshleriGetirAsync()
        {
            return Task.FromResult(Array.Empty<string>());
        }

        public Task<bool> MeshSecAsync(string meshAdi)
        {
            return Task.FromResult(false);
        }

        public Task MeshSecimiTemizleAsync()
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            DisposeAsyncCagrildiMi = true;
            return ValueTask.CompletedTask;
        }
    }
}

/// <summary>
/// Test amacli ModellerYonetimServisi mock'u — bos liste doner.
/// </summary>
public class TestModellerYonetimServisi : ModellerYonetimServisi
{
    public TestModellerYonetimServisi()
        : base(
            new HttpClient { BaseAddress = new Uri("http://localhost:5116/") },
            Microsoft.Extensions.Options.Options.Create(
                new KonfBff::VizitLink3D.Konfigurator.Servisler.BffGuvenlikAyarlari
                { Anahtar = "test-anahtari" }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ModellerYonetimServisi>.Instance)
    {
    }

    public override async Task<List<PublicModelListeOgesiDto>?> PublicModelListesiGetirAsync(
        CancellationToken iptal = default)
    {
        await Task.CompletedTask;
        return [];
    }
}
