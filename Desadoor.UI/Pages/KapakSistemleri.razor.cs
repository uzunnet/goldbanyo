using Microsoft.AspNetCore.Components;
using Desadoor.UI.Models;

namespace Desadoor.UI.Pages;

public partial class KapakSistemleri : ComponentBase, IDisposable
{
    [Inject] private NavigationManager Navigasyon { get; set; } = default!;

    private string _sayfaBasligi = "Kapak Sistemleri | DesaDoor";
    private string _sayfaAciklamasi = "Membran, lake, laminat, melamin ve kaplama kapak seçeneklerimizle mutfak ve banyolarınız için mükemmel çözümü bulun.";
    private string _heroBaslik = "Kapak Sistemleri";
    private string _heroAltBaslik = "ÜRÜN KATALOĞU";
    private bool _yukleniyor = true;
    private string _aramaMetni = string.Empty;
    private string _secilenKategori = "Tümü";
    private string _siralama = "yeni";

    private List<KapakModeliDto> _tumKapaklar = [];
    private List<KapakModeliDto> _cokIzlenenler = [];
    private KapakModeliDto? _secilenKapak;
    private string _aktifModelTuru = "Kapak";
    private List<string> _kategoriler = ["Tümü"];

    private int _sutunAdet = 4;
    private int _sayfaBasinaAdet = 12;
    private bool _sayfalamaAktif = true;
    private int _aktifSayfa = 1;

    private int _toplamUrun => _tumKapaklar
        .Where(k => _secilenKategori == "Tümü" || k.Kategori == _secilenKategori)
        .Where(k => string.IsNullOrWhiteSpace(_aramaMetni)
                    || k.ModelAdi.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase)
                    || k.ModelKodu.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase))
        .Count();

    private int _toplamSayfa => Math.Max(1, (int)Math.Ceiling((double)_toplamUrun / _sayfaBasinaAdet));

    private List<KapakModeliDto> _filtrelenmisKapaklar =>
        _tumKapaklar
            .Where(k => _secilenKategori == "Tümü" || k.Kategori == _secilenKategori)
            .Where(k => string.IsNullOrWhiteSpace(_aramaMetni)
                        || k.ModelAdi.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase)
                        || k.ModelKodu.Contains(_aramaMetni, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(k => _siralama == "yeni" ? k.YeniMi : k.OneCikanMi)
            .ThenBy(k => k.ModelAdi)
            .Skip(_sayfalamaAktif ? (_aktifSayfa - 1) * _sayfaBasinaAdet : 0)
            .Take(_sayfaBasinaAdet)
            .ToList();

    private string MudGridClass => _sutunAdet switch
    {
        2 => "desa-grid-2",
        3 => "desa-grid-3",
        4 => "desa-grid-4",
        5 => "desa-grid-5",
        6 => "desa-grid-6",
        _ => "desa-grid-4"
    };

    private string MudItemClass => _sutunAdet switch
    {
        2 => "",
        3 => "",
        4 => "",
        5 => "",
        6 => "",
        _ => ""
    };

    private int MudItemXs => _sutunAdet <= 2 ? 6 : _sutunAdet <= 4 ? 6 : 4;
    private int MudItemSm => _sutunAdet <= 2 ? 6 : _sutunAdet <= 4 ? 6 : 4;
    private int MudItemMd => 12 / Math.Min(_sutunAdet, 4);
    private int MudItemLg => 12 / Math.Min(_sutunAdet, 6);

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        SayfaTurunuBelirle();
        await Task.WhenAll(
            SayfaIceriginiYukleAsync(),
            DuzenAyariYukleAsync(),
            KapaklariYukleAsync(),
            CokIzlenenleriYukleAsync()
        );
    }

    private void SayfaTurunuBelirle()
    {
        var yol = Navigasyon.ToBaseRelativePath(Navigasyon.Uri).ToLowerInvariant();
        _aktifModelTuru = yol.Contains("kapi-modelleri") ? "Kapi" : "Kapak";
        if (_aktifModelTuru == "Kapi")
        {
            _sayfaBasligi = "Kapı Modelleri | DesaDoor";
            _sayfaAciklamasi = "Lake, membran, camlı, düz ve özel seri kapı modellerimizi inceleyin.";
            _heroBaslik = "Kapı Modelleri";
            _heroAltBaslik = "DİNAMİK MODEL KATALOĞU";
        }
    }

    private async Task SayfaIceriginiYukleAsync()
    {
        var sayfaKodu = _aktifModelTuru == "Kapi" ? "kapi-modelleri" : "kapak-sistemleri";
        var sozluk = await api.GetAsync<Dictionary<string, string>>($"api/desadoor/sayfa-icerigi/{sayfaKodu}?dil={dil.AktifDil}");
        if (sozluk != null)
        {
            _sayfaBasligi = sozluk.GetValueOrDefault("SayfaBasligi", _sayfaBasligi);
            _sayfaAciklamasi = sozluk.GetValueOrDefault("SayfaAciklamasi", _sayfaAciklamasi);
            _heroBaslik = sozluk.GetValueOrDefault("HeroBaslik", _heroBaslik);
            _heroAltBaslik = sozluk.GetValueOrDefault("HeroAltBaslik", _heroAltBaslik);
        }
    }

    private async Task DuzenAyariYukleAsync()
    {
        var sayfaKodu = _aktifModelTuru == "Kapi" ? "kapi-modelleri" : "kapak-sistemleri";
        var ayar = await api.GetAsync<SayfaDuzenAyariDto>($"api/desadoor/sayfa-duzen-ayarlari/{sayfaKodu}");
        if (ayar != null)
        {
            _sutunAdet = ayar.SutunAdet > 0 ? ayar.SutunAdet : 4;
            _sayfaBasinaAdet = ayar.SayfaBasinaAdet > 0 ? ayar.SayfaBasinaAdet : 12;
            _sayfalamaAktif = ayar.SayfalamaAktif;
        }
    }

    private async Task KapaklariYukleAsync()
    {
        _yukleniyor = true;
        var modeller = await api.GetAsync<List<KapakModeliDto>>($"api/kapak-modelleri?modelTuru={_aktifModelTuru}");
        if (modeller != null)
        {
            _tumKapaklar = modeller.Select(ModeliHazirla).ToList();
            _kategoriler = ["Tümü", .. _tumKapaklar
                .Select(k => k.Kategori)
                .Where(k => !string.IsNullOrEmpty(k))
                .Distinct()
                .OrderBy(k => k)];
        }
        _yukleniyor = false;
    }

    private async Task CokIzlenenleriYukleAsync()
    {
        var modeller = await api.GetAsync<List<KapakModeliDto>>($"api/kapak-modelleri/cok-izlenen?modelTuru={_aktifModelTuru}&adet=8");
        _cokIzlenenler = modeller?.Select(ModeliHazirla).ToList() ?? [];
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

    private void SiralamaDegistir(string siralama)
    {
        _siralama = siralama;
        _aktifSayfa = 1;
    }

    private void AramaYap(string? deger)
    {
        _aramaMetni = deger ?? string.Empty;
        _aktifSayfa = 1;
    }

    private void KapakAc(KapakModeliDto kapak)
    {
        Navigasyon.NavigateTo(DetayUrl(kapak));
    }

    private void KapakKapat()
    {
        _secilenKapak = null;
    }

    private async void OnDilDegisti()
    {
        SayfaTurunuBelirle();
        await Task.WhenAll(
            SayfaIceriginiYukleAsync(),
            DuzenAyariYukleAsync(),
            KapaklariYukleAsync(),
            CokIzlenenleriYukleAsync()
        );
        await InvokeAsync(StateHasChanged);
    }

    private KapakModeliDto ModeliHazirla(KapakModeliDto model)
    {
        // Eğer AnaGorselUrl boşsa ModelKodu'ndan sayısal kısım alarak yatay_ fallback üret
        if (string.IsNullOrWhiteSpace(model.AnaGorselUrl))
        {
            var numStr = new string(model.ModelKodu.Where(char.IsDigit).ToArray());
            model.AnaGorselUrl = string.IsNullOrWhiteSpace(numStr)
                ? "/medya/desadoor_default.png"
                : $"/medya/kapaklar/yatay_{numStr}.png";
        }
        else
        {
            model.AnaGorselUrl = TamUrl(model.AnaGorselUrl);
        }

        model.GorselYolu = string.IsNullOrWhiteSpace(model.GorselYolu) ? model.AnaGorselUrl : TamUrl(model.GorselYolu);
        model.ResimUrl = string.IsNullOrWhiteSpace(model.ResimUrl) ? model.AnaGorselUrl : TamUrl(model.ResimUrl);
        model.Url = DetayUrl(model);

        if (model.UygulamaGorselleri?.Count > 0)
            model.UygulamaGorselleri = model.UygulamaGorselleri.Select(g => TamUrl(g)).ToList();

        if (model.RenkSecenekleri == null || !model.RenkSecenekleri.Any())
            model.RenkSecenekleri = RalKatalogu.GetRalRenkleri();

        return model;
    }

    private string DetayUrl(KapakModeliDto model)
    {
        var kok = model.ModelTuru == "Kapi" ? "kapi" : "kapak";
        return $"{kok}/{model.Id}/{Uri.EscapeDataString(model.ModelKodu)}";
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol) || yol.Contains("unsplash.com") || yol.Contains("placeholder-kapak.webp"))
            return "/medya/desadoor_default.png";
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase) || yol.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            return yol;
        return $"{api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }

    private string GetDetayliGorsel(KapakModeliDto kapak)
    {
        if (kapak.UygulamaGorselleri?.Count > 0)
            return kapak.UygulamaGorselleri[0];
        return kapak.AnaGorselUrl;
    }
}
