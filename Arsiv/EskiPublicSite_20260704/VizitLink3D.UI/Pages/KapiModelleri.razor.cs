using Microsoft.AspNetCore.Components;
using VizitLink3D.UI.Models;

namespace VizitLink3D.UI.Pages;

public partial class KapiModelleri : ComponentBase, IDisposable
{
    [Inject] private NavigationManager Navigasyon { get; set; } = default!;
    private bool _yukleniyor = true;
    private string _aramaMetni = string.Empty;
    private string _secilenKategori = "Tümü";
    private string _siralama = "yeni";

    private int _sutunAdet = 3;
    private int _sayfaBasinaAdet = 12;
    private bool _sayfalamaAktif = true;
    private int _aktifSayfa = 1;

    private List<KapakModeliDto> _tumModeller = [];
    private List<KapakModeliDto> _cokIzlenenler = [];
    private List<string> _altKategoriler = ["Tümü"];

    private int _toplamUrun => _tumModeller
        .Where(k => _secilenKategori == "Tümü" || k.Kategori == _secilenKategori)
        .Where(k => string.IsNullOrWhiteSpace(_aramaMetni)
                    || k.ModelAdi.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase)
                    || k.ModelKodu.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase))
        .Count();

    private int _toplamSayfa => Math.Max(1, (int)Math.Ceiling((double)_toplamUrun / _sayfaBasinaAdet));

    private List<KapakModeliDto> _filtrelenmisModeller =>
        _tumModeller
            .Where(k => _secilenKategori == "Tümü" || k.Kategori == _secilenKategori)
            .Where(k => string.IsNullOrWhiteSpace(_aramaMetni)
                        || k.ModelAdi.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase)
                        || k.ModelKodu.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(k => _siralama == "yeni" ? k.YeniMi : k.OneCikanMi)
            .ThenBy(k => k.ModelAdi)
            .Skip(_sayfalamaAktif ? (_aktifSayfa - 1) * _sayfaBasinaAdet : 0)
            .Take(_sayfaBasinaAdet)
            .ToList();

    private int MudItemMd => 12 / Math.Min(_sutunAdet, 4);
    private int MudItemLg => 12 / Math.Min(_sutunAdet, 6);

    protected override async Task OnInitializedAsync()
    {
        // Gold Banyo frontend: kapı sayfaları kaldırıldı, ana sayfaya yönlendir
        Navigasyon.NavigateTo("/", true);
    }

    private async Task EskiOnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        await Task.WhenAll(
            DuzenAyariYukleAsync(),
            ModelleriYukleAsync()
        );
    }

    private async Task DuzenAyariYukleAsync()
    {
        var ayar = await api.GetAsync<SayfaDuzenAyariDto>("api/sayfa-duzen-ayarlari/kapi-modelleri");
        if (ayar != null)
        {
            _sutunAdet = ayar.SutunAdet > 0 ? ayar.SutunAdet : 3;
            _sayfaBasinaAdet = ayar.SayfaBasinaAdet > 0 ? ayar.SayfaBasinaAdet : 12;
            _sayfalamaAktif = ayar.SayfalamaAktif;
        }
    }

    private async Task ModelleriYukleAsync()
    {
        _yukleniyor = true;

        var modeller = await api.GetAsync<List<KapakModeliDto>>("api/kapak-modelleri?modelTuru=Kapi");
        _tumModeller = modeller?.Select(ModeliHazirla).ToList() ?? [];

        var cokIzlenenler = await api.GetAsync<List<KapakModeliDto>>("api/kapak-modelleri/cok-izlenen?modelTuru=Kapi&adet=8");
        _cokIzlenenler = cokIzlenenler?.Select(ModeliHazirla).ToList() ?? [];

        _altKategoriler = ["Tümü", .. _tumModeller
            .Select(k => k.Kategori)
            .Where(k => !string.IsNullOrEmpty(k))
            .Distinct()
            .OrderBy(k => k)];

        _yukleniyor = false;
    }

    private void SayfaDegistir(int sayfa)
    {
        _aktifSayfa = Math.Clamp(sayfa, 1, _toplamSayfa);
        StateHasChanged();
    }

    private void KategoriSec(string kategori)
    {
        _secilenKategori = kategori;
        _aktifSayfa = 1;
    }

    private void AramaYap(string? deger)
    {
        _aramaMetni = deger ?? string.Empty;
        _aktifSayfa = 1;
    }

    private void SiralamaDegistir(string siralama)
    {
        _siralama = siralama;
        _aktifSayfa = 1;
    }

    private string DetayUrl(KapakModeliDto model) => $"/kapi/{model.Id}/{Uri.EscapeDataString(model.ModelKodu)}";

    private KapakModeliDto ModeliHazirla(KapakModeliDto model)
    {
        model.AnaGorselUrl = TamUrl(model.AnaGorselUrl);
        model.GorselYolu = TamUrl(string.IsNullOrWhiteSpace(model.GorselYolu) ? model.AnaGorselUrl : model.GorselYolu);
        model.ResimUrl = TamUrl(string.IsNullOrWhiteSpace(model.ResimUrl) ? model.AnaGorselUrl : model.ResimUrl);
        return model;
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol) || yol.Contains("unsplash.com") || yol.Contains("placeholder-kapak.webp"))
            return "/medya/vizitlink3d_default.png";
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase) || yol.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            return yol;
        return $"{api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private async void OnDilDegisti()
    {
        await Task.WhenAll(DuzenAyariYukleAsync(), ModelleriYukleAsync());
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => dil.DilDegisti -= OnDilDegisti;
}

