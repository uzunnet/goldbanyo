using Microsoft.JSInterop;

namespace VizitLink3D.Konfigurator.Servisler;

/// <summary>
/// Uc Boyut Goruntuleyici Servisi — Three.js ES module wrapper koprusu.
///
/// Tarayici tarafindaki ucboyut-goruntuleyici.js modulunu lazy import eder,
/// Blazor bileşenlerinden JS interop cagrilarini soyutlar.
///
/// P04 ret duzeltmesi: IJSRuntime dogrudan kullanimini kaldirir;
/// tum JS module import/call islemleri bu servis uzerinden yapilir.
/// </summary>
public class UcBoyutGoruntuleyiciServisi : IUcBoyutGoruntuleyiciServisi
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _modul;
    private bool _hazir;
    private bool _temizlendi;

    public bool HazirMi => _hazir;

    /// <summary>
    /// DI uzerinden IJSRuntime alir. JS modul referansi lazy import edilir.
    /// </summary>
    public UcBoyutGoruntuleyiciServisi(IJSRuntime js)
    {
        _js = js;
    }

    /// <inheritdoc />
    public async Task BaslatAsync(object dotNetRef, string elemanId)
    {
        if (_temizlendi)
            throw new ObjectDisposedException(nameof(UcBoyutGoruntuleyiciServisi));

        try
        {
            // JS ES modulunu lazy import et
            _modul = await _js.InvokeAsync<IJSObjectReference>(
                "import", "./js/ucboyut/ucboyut-goruntuleyici.js");

            // Canvas host'unu bul ve goruntuleyiciyi baslat
            await _modul.InvokeVoidAsync("baslatGoruntuleyici", dotNetRef, elemanId);
            _hazir = true;
        }
        catch (Exception ex)
        {
            _hazir = false;
            System.Console.Error.WriteLine(
                $"[UcBoyutGoruntuleyiciServisi] Baslatma hatasi: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task ModelYukleAsync(string modelUrl)
    {
        if (_temizlendi)
            throw new ObjectDisposedException(nameof(UcBoyutGoruntuleyiciServisi));

        if (_modul is null || !_hazir)
            return;

        try
        {
            await _modul.InvokeVoidAsync("modelYukle", modelUrl);
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(
                $"[UcBoyutGoruntuleyiciServisi] Model yukleme hatasi: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string[]> MeshleriGetirAsync()
    {
        if (_temizlendi)
            throw new ObjectDisposedException(nameof(UcBoyutGoruntuleyiciServisi));

        if (_modul is null || !_hazir)
            return [];

        try
        {
            var isimler = await _modul.InvokeAsync<string[]>("meshIsimleriGetir");
            return isimler ?? [];
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(
                $"[UcBoyutGoruntuleyiciServisi] Mesh listesi alma hatasi: {ex.Message}");
            return [];
        }
    }

    /// <inheritdoc />
    public async Task<bool> MeshSecAsync(string meshAdi)
    {
        if (_temizlendi)
            throw new ObjectDisposedException(nameof(UcBoyutGoruntuleyiciServisi));

        if (_modul is null || !_hazir || string.IsNullOrWhiteSpace(meshAdi))
            return false;

        try
        {
            var sonuc = await _modul.InvokeAsync<bool>("meshSec", meshAdi);
            return sonuc;
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(
                $"[UcBoyutGoruntuleyiciServisi] Mesh secme hatasi: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task MeshSecimiTemizleAsync()
    {
        if (_temizlendi)
            throw new ObjectDisposedException(nameof(UcBoyutGoruntuleyiciServisi));

        if (_modul is null || !_hazir)
            return;

        try
        {
            await _modul.InvokeVoidAsync("meshSecimiTemizle");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(
                $"[UcBoyutGoruntuleyiciServisi] Mesh secimi temizleme hatasi: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task YokEtAsync()
    {
        if (_modul is null)
            return;

        try
        {
            await _modul.InvokeVoidAsync("yokEtGoruntuleyici");
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(
                $"[UcBoyutGoruntuleyiciServisi] Yok etme hatasi: {ex.Message}");
        }

        _hazir = false;
    }

    /// <summary>
    /// Servis bellekten kaldirildiginda JS modul referansini ve
    /// goruntuleyiciyi temizler. Bellek sizintisini onler.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_temizlendi)
            return;

        _temizlendi = true;

        await YokEtAsync();

        if (_modul is not null)
        {
            try
            {
                await _modul.DisposeAsync();
            }
            catch
            {
                // Dispose sirasinda hata sessizce yutulur
            }
            _modul = null;
        }

        GC.SuppressFinalize(this);
    }
}
