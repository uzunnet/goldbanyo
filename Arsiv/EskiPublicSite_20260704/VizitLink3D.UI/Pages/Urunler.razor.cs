using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Models;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Pages;

public partial class Urunler : ComponentBase, IDisposable
{
    private sealed record KoleksiyonGrubuGorunumu(string Baslik, string Aciklama, List<Urun> Urunler);
    private sealed record KoleksiyonFiltreDto(string Ad, string Anchor, int Adet);
    private sealed record ModelFiltreDto(string Ad, string Slug);

    private List<Urun> _urunler = [];
    private List<UrunAilesi> _urunAileleri = [];
    private List<UrunKategori> _kategoriler = [];
    private bool _yukleniyor = true;
    private string _arama = "";
    private int? _seciliUrunAilesiId = 3;
    private int? _seciliKategoriId;
    private string? _seciliKoleksiyonGrubu;
    private string? _seciliModelSlug;
    private int _sayfa = 1;
    private int _sayfaBoyutu = 12;
    private int _sutunAdet = 4;
    private int _toplamSayfa => Math.Max(1, (int)Math.Ceiling(_urunler.Count / (double)_sayfaBoyutu));
    private string? _hataMesaji;
    private int ToplamKoleksiyonSayisi => GruplanmisUrunler().Count();
    private int ToplamOneCikanSayisi => FiltrelenmisUrunler().Count(x => x.OneCikanMi);
    private IReadOnlyList<KoleksiyonFiltreDto> KoleksiyonFiltreleri => GoldBanyoKatalogUrunleri.Tum
        .GroupBy(x => x.KoleksiyonGrubu)
        .OrderBy(x => GoldBanyoKatalogUrunleri.KoleksiyonSirasiGetir(x.Key))
        .Select(x => new KoleksiyonFiltreDto(x.Key, KoleksiyonAnchorOlustur(x.Key), x.Count()))
        .ToList();
    private IReadOnlyList<ModelFiltreDto> ModelFiltreleri => GoldBanyoKatalogUrunleri.Tum
        .Where(x => string.IsNullOrWhiteSpace(_seciliKoleksiyonGrubu) || string.Equals(x.KoleksiyonGrubu, _seciliKoleksiyonGrubu, StringComparison.OrdinalIgnoreCase))
        .OrderBy(x => x.SayfaNo)
        .Take(10)
        .Select(x => new ModelFiltreDto(x.Ad, x.Slug))
        .ToList();

    private int MudItemMd => 12 / Math.Min(_sutunAdet, 4);
    private int MudItemLg => 12 / Math.Min(_sutunAdet, 6);

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        await Task.WhenAll(
            DuzenAyariYukleAsync(),
            FiltreleriYukleAsync(),
            UrunleriYukleAsync()
        );
    }

    private async Task DuzenAyariYukleAsync()
    {
        var ayar = await api.GetAsync<SayfaDuzenAyariDto>("api/sayfa-duzen-ayarlari/urunler");
        if (ayar != null)
        {
            _sutunAdet = ayar.SutunAdet > 0 ? ayar.SutunAdet : 4;
            _sayfaBoyutu = ayar.SayfaBasinaAdet > 0 ? ayar.SayfaBasinaAdet : 12;
        }
    }

    private async Task FiltreleriYukleAsync()
    {
        try
        {
            var aileSonuc = await api.GetAsync<List<UrunAilesi>>("api/urun-ailesi");
            if (aileSonuc != null)
                _urunAileleri = aileSonuc;
        }
        catch { /* ürün aileleri yüklenemezse filtre boş kalır */ }

        try
        {
            var kategoriSonuc = await api.GetAsync<List<UrunKategori>>("api/urun-kategorileri");
            if (kategoriSonuc != null)
                _kategoriler = kategoriSonuc;
        }
        catch { /* kategoriler yüklenemezse filtre boş kalır */ }
    }

    private async Task UrunleriYukleAsync()
    {
        _yukleniyor = true;
        _hataMesaji = null;
        StateHasChanged();

        try
        {
            var sorguParcalari = new List<string> { $"dil={dil.AktifDil}" };
            if (_seciliKategoriId.HasValue)
                sorguParcalari.Add($"kategoriId={_seciliKategoriId}");
            sorguParcalari.Add($"urunAilesiId={(_seciliUrunAilesiId ?? 3)}");
            if (!string.IsNullOrWhiteSpace(_arama))
                sorguParcalari.Add($"arama={Uri.EscapeDataString(_arama)}");

            var sorgu = "?" + string.Join("&", sorguParcalari);
            var sonuc = await api.GetAsync<List<Urun>>($"api/urunler{sorgu}");
            _urunler = sonuc ?? [];
            if (_urunler.Count == 0 && _seciliUrunAilesiId is null or 3)
                _urunler = VarsayilanKatalogUrunleriniOlustur();
        }
        catch (Exception ex)
        {
            _hataMesaji = $"Hata: {ex.Message}";
            Console.Error.WriteLine($"[Urunler] Urun yukleme hatasi: {ex}");
            _urunler = _seciliUrunAilesiId is null or 3 ? VarsayilanKatalogUrunleriniOlustur() : [];
        }
        finally
        {
            _sayfa = 1;
            _yukleniyor = false;
        }
    }

    private IEnumerable<KoleksiyonGrubuGorunumu> GruplanmisUrunler()
    {
        return FiltrelenmisUrunler()
            .Select(urun => new
            {
                Urun = urun,
                KatalogUrunu = GoldBanyoKatalogUrunleri.SlugIleGetir(urun.Slug)
            })
            .GroupBy(x => x.KatalogUrunu?.KoleksiyonGrubu ?? "Diger")
            .OrderBy(x => GoldBanyoKatalogUrunleri.KoleksiyonSirasiGetir(x.Key))
            .Select(x => new KoleksiyonGrubuGorunumu(
                x.Key,
                GoldBanyoKatalogUrunleri.KoleksiyonAciklamasiGetir(x.Key),
                x.OrderBy(y => y.KatalogUrunu?.SayfaNo ?? int.MaxValue).Select(y => y.Urun).ToList()));
    }

    private IEnumerable<Urun> FiltrelenmisUrunler()
    {
        var sonuc = _urunler.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(_seciliKoleksiyonGrubu))
        {
            sonuc = sonuc.Where(urun =>
                string.Equals(
                    GoldBanyoKatalogUrunleri.SlugIleGetir(urun.Slug)?.KoleksiyonGrubu,
                    _seciliKoleksiyonGrubu,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(_seciliModelSlug))
            sonuc = sonuc.Where(urun => string.Equals(urun.Slug, _seciliModelSlug, StringComparison.OrdinalIgnoreCase));

        return sonuc;
    }

    private async Task UrunAilesiDegisti(int? deger)
    {
        _seciliUrunAilesiId = deger;
        await UrunleriYukleAsync();
    }

    private async Task KategoriDegisti(int? deger)
    {
        _seciliKategoriId = deger;
        await UrunleriYukleAsync();
    }

    private async Task AramaDegisti(string deger)
    {
        _arama = deger;
        _seciliModelSlug = null;
        await UrunleriYukleAsync();
    }

    private Task KoleksiyonDegistir(string? koleksiyon)
    {
        _seciliKoleksiyonGrubu = string.Equals(_seciliKoleksiyonGrubu, koleksiyon, StringComparison.OrdinalIgnoreCase) ? null : koleksiyon;
        _seciliModelSlug = null;
        _sayfa = 1;
        return Task.CompletedTask;
    }

    private void ModelDegistir(string? slug)
    {
        _seciliModelSlug = string.Equals(_seciliModelSlug, slug, StringComparison.OrdinalIgnoreCase) ? null : slug;
        _sayfa = 1;
    }

    private async Task SayfaDegisti(int sayfa)
    {
        _sayfa = sayfa;
        await Task.CompletedTask;
    }

    private void UrunDetayinaGit(string slug)
    {
        nav.NavigateTo(BanyoDolabiDetayYoluOlustur(slug));
    }

    private void PopupAc(Urun urun)
    {
        nav.NavigateTo(BanyoDolabiDetayYoluOlustur(urun.Slug));
    }

    private static string BanyoDolabiDetayYoluOlustur(string slug) => $"/banyo-dolabi/{slug}";
    private static string KoleksiyonAnchorOlustur(string koleksiyon) => koleksiyon.Trim().ToLowerInvariant().Replace(' ', '-');

    private async void OnDilDegisti()
    {
        var filtreGorevi = FiltreleriYukleAsync();
        var urunGorevi = UrunleriYukleAsync();
        await Task.WhenAll(filtreGorevi, urunGorevi);
        await InvokeAsync(StateHasChanged);
    }

    private List<Urun> VarsayilanKatalogUrunleriniOlustur()
    {
        var katalogUrunleri = GoldBanyoKatalogUrunleri.Tum.AsEnumerable();

        if (_seciliKategoriId.HasValue)
            katalogUrunleri = katalogUrunleri.Where(_ => _seciliKategoriId == 2);

        if (!string.IsNullOrWhiteSpace(_arama))
        {
            var arama = _arama.Trim();
            katalogUrunleri = katalogUrunleri.Where(x =>
                x.Ad.Contains(arama, StringComparison.OrdinalIgnoreCase) ||
                x.Kod.Contains(arama, StringComparison.OrdinalIgnoreCase) ||
                x.Slug.Contains(arama, StringComparison.OrdinalIgnoreCase));
        }

        return katalogUrunleri
            .OrderBy(x => x.SayfaNo)
            .Select((x, index) => new Urun
            {
                Id = 100000 + index,
                Slug = x.Slug,
                Kod = x.Kod,
                Ad = x.Ad,
                KisaAciklama = x.KisaAciklamaOlustur(),
                Aciklama = x.AciklamaHtmlOlustur(),
                UrunAilesiId = 3,
                UrunKategoriId = 2,
                AktifMi = true,
                OneCikanMi = x.OneCikanMi,
                YeniMi = x.YeniMi,
                Fiyat = x.Fiyat,
                Birim = "adet",
                SiraNo = x.SayfaNo,
                SeoBaslik = $"{x.Ad} | Gold Banyo 2026 Katalog",
                SeoAciklama = x.KisaAciklamaOlustur()
            })
            .ToList();
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }
}

