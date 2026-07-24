namespace VizitLink3D.Konfigurator.Servisler;

/// <summary>
/// Uc Boyut Goruntuleyici Servisi arayuzu.
/// Tarayici tarafindaki Three.js ES module wrapper (ucboyut-goruntuleyici.js)
/// ile Blazor arasinda kopru gorevi gorur.
///
/// P04 ret duzeltmesi: IJSRuntime dogrudan kullanimini soyutlar;
/// Razor code-behind sadece bu servisi inject eder.
/// </summary>
public interface IUcBoyutGoruntuleyiciServisi : IAsyncDisposable
{
    /// <summary>
    /// Three.js goruntuleyiciyi baslatir.
    /// JS modulunu lazy import eder, DotNet referansini kaydeder,
    /// belirtilen HTML element ID'si uzerinde sahneyi kurar.
    /// </summary>
    /// <param name="dotNetRef">JS → .NET callback'leri icin DotNetObjectReference</param>
    /// <param name="elemanId">Canvas host elementinin HTML ID'si</param>
    Task BaslatAsync(object dotNetRef, string elemanId);

    /// <summary>
    /// Aktif goruntuleyiciye GLB/GLTF model yukler.
    /// BFF proxy URL'i uzerinden dosya indirilir, onceki model temizlenir.
    /// </summary>
    /// <param name="modelUrl">BFF proxy GLB dosya URL'i</param>
    Task ModelYukleAsync(string modelUrl);

    /// <summary>
    /// Yuklu modeldeki tum mesh isimlerini dondurur.
    /// Isimsiz mesh'ler deterministik teknik tanimlayici ile doner.
    /// JS tarafinda traversal yapilir, essiz isimler string[] olarak doner.
    /// </summary>
    Task<string[]> MeshleriGetirAsync();

    /// <summary>
    /// Goruntuleyiciyi tamamen durdurur ve kaynaklari serbest birakir.
    /// Animasyon dongusu, renderer, model, event listener'lar temizlenir.
    /// </summary>
    Task YokEtAsync();

    /// <summary>
    /// Belirtilen isimdeki mesh'i secer ve emissive highlight uygular.
    /// JS tarafindaki meshSec fonksiyonunu cagirir.
    /// Sadece ISIMLI mesh'ler secilebilir; isimsiz olanlar atlanir.
    /// </summary>
    /// <param name="meshAdi">Secilecek mesh'in adi</param>
    /// <returns>En az bir mesh bulunup secildiyse true</returns>
    Task<bool> MeshSecAsync(string meshAdi);

    /// <summary>
    /// Tum mesh secimlerini temizler, orijinal materyal durumunu geri yukler.
    /// JS tarafindaki meshSecimiTemizle fonksiyonunu cagirir.
    /// </summary>
    Task MeshSecimiTemizleAsync();

    /// <summary>
    /// Goruntuleyicinin baslatilmis ve kullanima hazir oldugunu belirtir.
    /// </summary>
    bool HazirMi { get; }
}
