using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VizitLink3D.Ortak.Modeller.Renkler;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class UrunDetay : ComponentBase, IDisposable
{
    [Parameter] public string Slug { get; set; } = string.Empty;

    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    protected override void OnInitialized()
    {
        dil.DilDegisti += DilDegistiginde;
    }

    private void DilDegistiginde() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        dil.DilDegisti -= DilDegistiginde;
    }

    private Urun? _urun;
    private List<Urun> BenzerUrunler { get; set; } = [];
    private List<string> GaleriGorselleri { get; set; } = [];
    private List<UrunUcBoyutModeli> Modeller { get; set; } = [];
    private List<RalRengi> Renkler { get; set; } = [];
    private List<UrunMedya> Medyalar { get; set; } = [];
    private string KoleksiyonAdi { get; set; } = string.Empty;
    private string KoleksiyonAciklama { get; set; } = string.Empty;
    private string KategoriAdi { get; set; } = string.Empty;
    private string HeroGorselUrl { get; set; } = "/medya/vizitlink3d_default.png";
    private string? HataMesaji { get; set; }
    private bool _yukleniyor = true;
    private GoldBanyoKatalogUrunu? _katalogVerisi;

    private List<string> KatalogOzellikleri => _katalogVerisi?.Ozellikler.ToList() ?? [];
    private List<GoldBanyoKatalogOlcusu> KatalogOlculer => _katalogVerisi?.Olculer.ToList() ?? [];

    // ─── LIGHTBOX (galeri buyutme) ─────────────────────────────────────
    private bool _lightboxAcik;
    private int _lightboxIndex;

    private void LightboxAc(int index)
    {
        if (index < 0 || index >= GaleriGorselleri.Count) return;
        _lightboxIndex = index;
        _lightboxAcik = true;
    }

    private void LightboxKapat() => _lightboxAcik = false;

    private void LightboxOnceki()
    {
        if (GaleriGorselleri.Count == 0) return;
        _lightboxIndex = (_lightboxIndex - 1 + GaleriGorselleri.Count) % GaleriGorselleri.Count;
    }

    private void LightboxSonraki()
    {
        if (GaleriGorselleri.Count == 0) return;
        _lightboxIndex = (_lightboxIndex + 1) % GaleriGorselleri.Count;
    }

    private List<(string Ad, string Hex)> RenkSwatchListesi =>
        _katalogVerisi is not null && _katalogVerisi.Renkler.Length > 0
            ? _katalogVerisi.Renkler.Select(r => (Ad: r, Hex: GoldBanyoKatalogRenkPaleti.HexBul(r))).ToList()
            : Renkler.Select(r => (Ad: r.Ad, Hex: r.HexKod ?? "#B0A99A")).ToList();

    /// <summary>
    /// Admin panelinden urune ozel yuklenen teknik cizim varsa onu kullan;
    /// yoksa (eski) statik katalog gorseline dus.
    /// </summary>
    private string? TeknikCizimUrl =>
        Medyalar.FirstOrDefault(m => m.MedyaTuru.Equals("TeknikCizim", StringComparison.OrdinalIgnoreCase))?.MedyaUrl
            ?? _katalogVerisi?.TeknikGorselUrl;

    private static readonly Dictionary<string, string> OzellikIkonEslesmesi = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Soft Kapak"] = "/img/ozellik-ikonlar/soft-kapak.svg",
        ["MDF Ahşap"] = "/img/ozellik-ikonlar/mdf.svg",
        ["Dokunmatik Ledli Ayna"] = "/img/ozellik-ikonlar/dokunmatik.svg",
        ["Kolay Montaj"] = "/img/ozellik-ikonlar/kolay-montaj.svg",
        ["Kolay Temizlenir"] = "/img/ozellik-ikonlar/kolay-temizlik.svg",
        ["Renk Seçenekleri"] = "/img/ozellik-ikonlar/renk.svg",
        ["Stone Lavabo"] = "/img/ozellik-ikonlar/stone-lavabo.svg",
        ["Cam Lavabo"] = "/img/ozellik-ikonlar/stone-lavabo.svg",
    };

    private static string? OzellikIkonuBul(string ozellik) =>
        OzellikIkonEslesmesi.TryGetValue(ozellik, out var ikon) ? ikon : null;

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            _yukleniyor = true;
            HataMesaji = null;
            _urun = null;
            _katalogVerisi = null;
            BenzerUrunler = [];
            GaleriGorselleri = [];
            Modeller = [];
            Renkler = [];
            Medyalar = [];
            KoleksiyonAdi = string.Empty;
            KoleksiyonAciklama = string.Empty;
            KategoriAdi = string.Empty;
            HeroGorselUrl = "/medya/vizitlink3d_default.png";

            if (string.IsNullOrWhiteSpace(Slug))
                return;

            var urun = await Api.GetAsync<Urun>($"api/urunler/slug/{Uri.EscapeDataString(Slug)}?dil=tr");
            if (urun is null)
                return;

            _urun = urun;
            _katalogVerisi = UrunGorunumYardimcisi.KatalogVerisiBul(urun);

            var aileler = (await Api.GetAsync<List<UrunAilesi>>("api/urun-ailesi") ?? [])
                .Where(x => x.AktifMi && !x.SilindiMi)
                .ToDictionary(x => x.Id);
            var kategoriler = (await Api.GetAsync<List<UrunKategori>>("api/urun-kategorileri") ?? [])
                .Where(x => x.AktifMi && !x.SilindiMi)
                .ToDictionary(x => x.Id);

            Modeller = (await Api.GetAsync<List<UrunUcBoyutModeli>>($"api/urunler/{urun.Id}/uc-boyut-modelleri") ?? [])
                .Where(x => x.AktifMi && !x.SilindiMi)
                .ToList();
            Renkler = (await Api.GetAsync<List<RalRengi>>($"api/urunler/{urun.Id}/renkler") ?? [])
                .Where(x => x.AktifMi)
                .ToList();
            Medyalar = (await Api.GetAsync<List<UrunMedya>>($"api/urunler/{urun.Id}/medyalar") ?? [])
                .OrderBy(x => x.SiraNo)
                .ToList();
            BenzerUrunler = (await Api.GetAsync<List<Urun>>($"api/urunler/{urun.Id}/benzer?adet=6&dil=tr") ?? [])
                .Where(x => x.AktifMi && !x.SilindiMi && x.Id != urun.Id)
                .Take(3)
                .ToList();

            KoleksiyonAdi = UrunGorunumYardimcisi.KoleksiyonAdiBul(urun, aileler);
            KategoriAdi = UrunGorunumYardimcisi.KategoriAdiBul(urun, kategoriler);
            KoleksiyonAciklama = aileler.TryGetValue(urun.UrunAilesiId, out var aile) && !string.IsNullOrWhiteSpace(aile.Aciklama)
                ? aile.Aciklama!
                : kategoriler.TryGetValue(urun.UrunKategoriId ?? -1, out var kategori) && !string.IsNullOrWhiteSpace(kategori.Aciklama)
                    ? kategori.Aciklama!
                    : UrunGorunumYardimcisi.OzetMetniBul(urun);

            HeroGorselUrl = UrunGorunumYardimcisi.AnaGorselUrl(urun, Api.ApiBaseUrl);

            // Admin'den urune ozel yuklenen gercek fotograflar varsa (MedyaTuru
            // Resim/Gorsel) onlar kullanilir; yoksa (eski) statik katalog
            // gorsellerine dusulur. Boylece her urunun galerisi admin'den
            // bagimsiz olarak yonetilebilir, sayfa basina 1/2/3+ gorsel olabilir.
            var dbGorseller = Medyalar
                .Where(x => x.MedyaTuru.Equals("Gorsel", StringComparison.OrdinalIgnoreCase)
                    || x.MedyaTuru.Equals("Resim", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.SiraNo)
                .Select(x => x.MedyaUrl)
                .Distinct()
                .ToList();

            if (dbGorseller.Count > 0)
            {
                GaleriGorselleri = dbGorseller;
            }
            else if (_katalogVerisi is not null)
            {
                // Katalog ürünlerinin görselleri UI'nin kendi statik wwwroot'unda duruyor,
                // API üzerinden değil — bu yüzden doğrudan (API taban URL'siz) kullanılır.
                GaleriGorselleri = new List<string>
                {
                    _katalogVerisi.HeroGorselUrl,
                    _katalogVerisi.KatalogGorselUrl,
                    _katalogVerisi.TeknikGorselUrl
                }.Distinct().ToList();
            }
            else
            {
                GaleriGorselleri = [];
            }

            if (GaleriGorselleri.Count == 0)
                GaleriGorselleri = [HeroGorselUrl];
            else
                HeroGorselUrl = GaleriGorselleri[0];
        }
        catch (Exception ex)
        {
            HataMesaji = ex.Message;
            Console.Error.WriteLine($"[UrunDetay] {ex}");
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Icerik API'den async geldigi icin ilk render'da .gb-reveal elemanlari
        // henuz DOM'da olmuyor — bu yuzden her render'da (sadece firstRender'da degil)
        // yeni eklenen (henuz .gorunur olmayan) elemanlari gozlemciye kaydediyoruz.
        try
        {
            await JS.InvokeVoidAsync("eval", @"
                (function () {
                    if (!window.__gbRevealObserver) {
                        window.__gbRevealObserver = new IntersectionObserver((entries) => {
                            entries.forEach(entry => {
                                if (entry.isIntersecting) {
                                    entry.target.classList.add('gorunur');
                                    window.__gbRevealObserver.unobserve(entry.target);
                                }
                            });
                        }, { threshold: 0.1 });
                    }
                    document.querySelectorAll('.gb-reveal:not(.gorunur)').forEach(el => window.__gbRevealObserver.observe(el));
                })();
            ");
        }
        catch { }
    }

    private string AnaOzeti =>
        _urun is null
            ? string.Empty
            : UrunGorunumYardimcisi.OzetMetniBul(_urun);

    private string ModelOzetMetni =>
        Modeller.Count > 0
            ? $"{Modeller.Count} 3D model"
            : "3D model bilgisi yok";

    private string RenkOzetMetni =>
        Renkler.Count > 0
            ? $"{Renkler.Count} renk seçeneği"
            : "Renk bilgisi yok";

    private string MedyaOzetMetni =>
        Medyalar.Count > 0
            ? $"{Medyalar.Count} medya"
            : "Medya bilgisi yok";

    private string ModelBasligi(UrunUcBoyutModeli model) =>
        string.IsNullOrWhiteSpace(model.ModelAdi) ? "Model" : model.ModelAdi;
}
