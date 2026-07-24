using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Pages.Public;

public partial class Anasayfa : ComponentBase, IAsyncDisposable
{
    [Inject] private IUcBoyutGoruntuleyiciServisi GoruntuleyiciServisi { get; set; } = default!;
    [Inject] private ModellerYonetimServisi ModellerServisi { get; set; } = default!;

    private List<PublicModelListeOgesiDto>? _modeller;
    private bool _yukleniyor = true;
    private bool _hataVar;
    private string _hataMesaji = "";
    private int? _seciliModelId;
    private bool _modelYukleniyor;
    private bool _modelHataVar;
    private string _modelHataMesaji = "";
    private bool _goruntuleyiciHazir;

    // P04 ret duzeltmesi: DotNetObjectReference lifecycle guvenligi Anasayfa'da korunur.
    // JS → .NET callback'leri (OnModelYukleniyor, OnModelYuklendi, OnModelHata)
    // bu referans uzerinden calisir. Wrapper servisi IJSRuntime soyutlamasi yapar;
    // dotNetRef dogrudan servise iletilir.
    private DotNetObjectReference<Anasayfa>? _dotNetRef;

    /// <summary>
    /// Sayfaya ozel CSS dosyasi.
    /// </summary>
    private const string SayfaCss = "css/sistem/moduller/anasayfa.css";

    protected override async Task OnInitializedAsync()
    {
        await ModelListesiniYukleAsync();
    }

    protected override async Task OnAfterRenderAsync(bool ilkRender)
    {
        if (ilkRender)
        {
            await GoruntuleyiciyiBaslatAsync();
        }
    }

    /// <summary>
    /// BFF proxy uzerinden model listesini ceker.
    /// </summary>
    private async Task ModelListesiniYukleAsync()
    {
        _yukleniyor = true;
        _hataVar = false;
        StateHasChanged();

        try
        {
            _modeller = await ModellerServisi.PublicModelListesiGetirAsync();

            if (_modeller is null)
            {
                _hataVar = true;
                _hataMesaji = "Model listesi alinamadi. Lutfen daha sonra tekrar deneyin.";
            }
        }
        catch
        {
            _hataVar = true;
            _hataMesaji = "Beklenmeyen bir hata olustu. Lutfen sayfayi yenileyin.";
        }
        finally
        {
            _yukleniyor = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Three.js goruntuleyiciyi baslatir.
    /// P04 ret duzeltmesi: IJSRuntime dogrudan kullanilmaz;
    /// IUcBoyutGoruntuleyiciServisi uzerinden JS modul lazy import edilir.
    /// </summary>
    private async Task GoruntuleyiciyiBaslatAsync()
    {
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);

            await GoruntuleyiciServisi.BaslatAsync(_dotNetRef, "ucboyut-canvas");
            _goruntuleyiciHazir = true;
        }
        catch (Exception ex)
        {
            // JS interop hatasi — viewer kullanilamaz ama sayfa hala gosterilir
            _modelHataVar = true;
            _modelHataMesaji = "3D goruntuleyici baslatilamadi. Tarayiciniz WebGL desteklemiyor olabilir.";
            System.Console.Error.WriteLine($"[Anasayfa] Goruntuleyici baslatma hatasi: {ex.Message}");
        }
    }

    /// <summary>
    /// Kullanici model kartina tikladiginda cagrilir.
    /// Secili modelin GLB dosyasini BFF proxy uzerinden goruntuleyiciye yukler.
    /// </summary>
    private async Task ModelSecAsync(PublicModelListeOgesiDto model)
    {
        if (_seciliModelId == model.Id) return;

        _seciliModelId = model.Id;
        _modelYukleniyor = true;
        _modelHataVar = false;
        _modelHataMesaji = "";
        StateHasChanged();

        try
        {
            if (GoruntuleyiciServisi.HazirMi && _goruntuleyiciHazir)
            {
                await GoruntuleyiciServisi.ModelYukleAsync(model.ModelUrl);
            }
        }
        catch (Exception ex)
        {
            _modelHataVar = true;
            _modelHataMesaji = "Model yuklenirken hata olustu.";
            System.Console.Error.WriteLine($"[Anasayfa] Model yukleme hatasi: {ex.Message}");
        }
        finally
        {
            _modelYukleniyor = false;
            StateHasChanged();
        }
    }

    #region JS Callbacks (DotNet JS interop ile cagrilir)

    /// <summary>
    /// JS tarafindan model yukleme basladiginda cagrilir.
    /// </summary>
    [JSInvokable]
    public void OnModelYukleniyor()
    {
        _modelYukleniyor = true;
        _modelHataVar = false;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// JS tarafindan model basariyla yuklendiginde cagrilir.
    /// </summary>
    [JSInvokable]
    public void OnModelYuklendi()
    {
        _modelYukleniyor = false;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// JS tarafindan hata olustugunda cagrilir.
    /// </summary>
    [JSInvokable]
    public void OnModelHata(string hataMesaji)
    {
        _modelYukleniyor = false;
        _modelHataVar = true;
        _modelHataMesaji = hataMesaji ?? "Model yuklenirken hata olustu.";
        InvokeAsync(StateHasChanged);
    }

    #endregion

    /// <summary>
    /// Bellek temizligi: DotNet referansi ve goruntuleyici servisi serbest birakilir.
    /// P04 ret duzeltmesi: JS modul temizligi IUcBoyutGoruntuleyiciServisi.DisposeAsync
    /// tarafindan yonetilir; burada sadece DotNetObjectReference dispose edilir.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        await GoruntuleyiciServisi.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Bayt cinsinden dosya boyutunu insan okunabilir formata cevirir.
    /// Ornek: 1.23 MB, 567 KB
    /// </summary>
    private static string ModelBoyutuFormatla(long boyutBayt)
    {
        return boyutBayt switch
        {
            >= 1_048_576 => $"{boyutBayt / 1_048_576.0:F2} MB",
            >= 1_024 => $"{boyutBayt / 1024.0:F0} KB",
            _ => $"{boyutBayt} B"
        };
    }
}
