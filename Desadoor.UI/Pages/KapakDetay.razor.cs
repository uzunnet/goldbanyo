using Desadoor.UI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Desadoor.UI.Pages;

/// <summary>
/// KapakDetay sayfasının code-behind (partial class) dosyası.
/// Anayasa §5.3 gereği tüm C# mantığı buraya taşındı — .razor dosyasında @code bloğu YASAKTIR.
/// Bu sayfa, seçilen kapak modelini 3D Three.js motoru ile görüntüler.
/// Kullanıcı renk seçtiğinde 3D model anlık olarak güncellenir (sayfa yenilenmez).
/// </summary>
public partial class KapakDetay : ComponentBase, IDisposable
{
    [Parameter] public int ModelId { get; set; }
    [Parameter] public string? ModelKodu { get; set; }

    [Inject] private NavigationManager Navigasyon { get; set; } = default!;
    [Inject] private IConfiguration Yapilandirma { get; set; } = default!;

    private KapakModeliDto? _model;
    private List<KapakModeliDto> _benzerModeller = [];
    private List<KapakModeliDto> _cokIzlenenModeller = [];
    private RalRengi? _secilenRenk;
    private string _secilenRenkHex = "#E8E4DF";
    private string _baslangicRenkHex = "#E8E4DF";
    private bool _yukleniyor = true;
    private int _sonYuklenenModelId;

    // Galeri state
    private List<string> _galeriGorseller = [];
    private int _aktifGorselIndex;
    private bool _goster3D;

    // Breadcrumb ve Yönlendirme Değişkenleri
    private string _listeUrl = "/katalog";
    private string _listeBaslik = "Katalog";
    private string _baseRoute = "/kapak";

    /// <summary>
    /// Sayfa yüklendiğinde model verilerini API'den getirir.
    /// Model yüklendikten sonra aynı kategorideki benzer modeller de çekilir.
    /// Veritabanında renk tanımı yoksa RenkDto.VarsayilanPalet() devreye girer (DRY).
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        await Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_sonYuklenenModelId != ModelId)
        {
            _sonYuklenenModelId = ModelId;
            await ModelVerileriniYukleAsync();
        }
    }

    private async Task ModelVerileriniYukleAsync()
    {
        _yukleniyor = true;

        // URL'den hangi tipten gelindiğini anla
        var mevcutUrl = Navigasyon.Uri.ToLower();
        if (mevcutUrl.Contains("/kapi/"))
        {
            _listeUrl = "kapi-modelleri";
            _listeBaslik = "Kapı Modelleri";
            _baseRoute = "kapi";
        }
        else if (mevcutUrl.Contains("/kapak/"))
        {
            _listeUrl = "kapak-sistemleri";
            _listeBaslik = "Kapak Modelleri";
            _baseRoute = "kapak";
        }
        else
        {
            _listeUrl = "";
            _listeBaslik = "Ürün Detayı";
            _baseRoute = "kapak";
        }

        var model = await api.GetAsync<KapakModeliDto>($"api/kapak-modelleri/{ModelId}");
        if (model != null)
        {
            // Kapak modelleri Urun'e goc ettiyse yeni detay sayfasina yonlendir
            if (!string.IsNullOrWhiteSpace(model.Slug) && mevcutUrl.Contains("/kapak/"))
            {
                var urun = await api.GetAsync<Desadoor.Ortak.Modeller.Urunler.Urun>($"api/urunler/slug/{model.Slug}");
                if (urun != null)
                {
                    Navigasyon.NavigateTo($"/urun/{urun.Slug}");
                    return;
                }
            }
            _model = ModeliHazirla(model);

            // Galeri: once ortam gorselleri (room scenes), sonra kapak close-up
            _galeriGorseller = [];
            if (_model.UygulamaGorselleri?.Count > 0)
                _galeriGorseller.AddRange(_model.UygulamaGorselleri);
            var closeup = _model.AnaGorselUrl;
            if (!string.IsNullOrWhiteSpace(closeup) && closeup != "/medya/desadoor_default.png"
                && !_galeriGorseller.Contains(closeup))
                _galeriGorseller.Add(closeup);
            _aktifGorselIndex = 0;
            _goster3D = _galeriGorseller.Count == 0 && !string.IsNullOrWhiteSpace(_model.ModelDosyaYolu);

            // Renk paleti yoksa RAL katalogundan fallback al — DRY
            if (_model.RenkSecenekleri == null || !_model.RenkSecenekleri.Any())
                _model.RenkSecenekleri = RalKatalogu.GetRalRenkleri();

            _secilenRenk = _model.RenkSecenekleri.FirstOrDefault();
            _baslangicRenkHex = _secilenRenk?.HexKod ?? "#E8E4DF";
            _secilenRenkHex = _baslangicRenkHex;

            var benzerler = await api.GetAsync<List<KapakModeliDto>>($"api/kapak-modelleri/benzer/{ModelId}?adet=4");
            _benzerModeller = benzerler?.Select(ModeliHazirla).ToList() ?? [];

            var cokIzlenenler = await api.GetAsync<List<KapakModeliDto>>($"api/kapak-modelleri/cok-izlenen?modelTuru={_model.ModelTuru}&adet=4");
            _cokIzlenenModeller = cokIzlenenler?
                .Where(m => m.Id != _model.Id)
                .Select(ModeliHazirla)
                .ToList() ?? [];
        }

        _yukleniyor = false;
    }

    /// <summary>
    /// Renk seçici bileşeninden renk seçim bildirimi geldiğinde çağrılır.
    /// Seçilen renk hex kodu 3D viewer parametresine aktarılır; viewer sayfa yenilenmeden güncellenir.
    /// </summary>
    private void RenkDegisti(RalRengi renk)
    {
        _secilenRenk = renk;
        _secilenRenkHex = renk.HexKod ?? "#E8E4DF";
    }

    /// <summary>
    /// Kullanıcıyı ilgili listeye (Kapı veya Kapak listesi) yönlendirir.
    /// </summary>
    private void KatalogaDon() => Navigasyon.NavigateTo(_listeUrl);

    private KapakModeliDto ModeliHazirla(KapakModeliDto model)
    {
        var gercekGorselYok = string.IsNullOrWhiteSpace(model.AnaGorselUrl) && string.IsNullOrWhiteSpace(model.GorselYolu);

        model.AnaGorselUrl = TamUrl(model.AnaGorselUrl);
        model.GorselYolu = gercekGorselYok
            ? null
            : TamUrl(string.IsNullOrWhiteSpace(model.GorselYolu) ? model.AnaGorselUrl : model.GorselYolu);
        model.ResimUrl = TamUrl(string.IsNullOrWhiteSpace(model.ResimUrl) ? model.AnaGorselUrl : model.ResimUrl);

        if (model.UygulamaGorselleri?.Count > 0)
            model.UygulamaGorselleri = model.UygulamaGorselleri.Select(TamUrl).ToList();

        if (!string.IsNullOrWhiteSpace(model.ModelDosyaYolu))
        {
            var isDummy = model.ModelDosyaYolu.Contains("nrd_", StringComparison.OrdinalIgnoreCase) ||
                          model.ModelDosyaYolu.Contains("mk-", StringComparison.OrdinalIgnoreCase) ||
                          model.ModelDosyaYolu.Contains("kl-", StringComparison.OrdinalIgnoreCase) ||
                          model.ModelDosyaYolu.Contains("dk-", StringComparison.OrdinalIgnoreCase) ||
                          model.ModelDosyaYolu.Contains("test_", StringComparison.OrdinalIgnoreCase) ||
                          model.ModelDosyaYolu.Contains("NRD", StringComparison.OrdinalIgnoreCase);

            if (isDummy)
            {
                model.ModelDosyaYolu = null;
            }
            else if (model.ModelDosyaYolu.StartsWith("/"))
            {
                model.ModelDosyaYolu = $"{api.ApiBaseUrl}{model.ModelDosyaYolu}";
            }
            else if (!model.ModelDosyaYolu.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                model.ModelDosyaYolu = $"{api.ApiBaseUrl}/{model.ModelDosyaYolu}";
            }
        }

        return model;
    }

    private void GorselSec(int index) { _aktifGorselIndex = index; _goster3D = false; }
    private void UcBoyutGoster() => _goster3D = true;

    private string DetayUrl(KapakModeliDto model)
    {
        var kok = model.ModelTuru == "Kapi" ? "kapi" : "kapak";
        return $"{kok}/{model.Id}/{Uri.EscapeDataString(model.ModelKodu)}";
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol) || yol.Contains("unsplash.com") || yol.Contains("placeholder-kapak.webp"))
        {
            return "/medya/desadoor_default.png";
        }

        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase) || yol.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
        {
            return yol;
        }

        return $"{api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private async void OnDilDegisti()
    {
        await ModelVerileriniYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }
}
