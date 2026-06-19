using Desadoor.UI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Desadoor.Ortak.Modeller.Urunler;

namespace Desadoor.UI.Pages;

public partial class AnaSayfa : IDisposable
{
    [Inject] public Microsoft.JSInterop.IJSRuntime JS { get; set; } = default!;
    [Inject] public Desadoor.UI.Servisler.AnimasyonMotoruServisi AnimasyonMotoru { get; set; } = default!;

    private bool _kapakYukleniyor = true;

    private string _heroGorselUrl = "/images/desadoor/hero-kapak.jpg";
    private List<HeroSliderOgesi> _heroSliderOgeleri = [];
    private List<KapakModeliDto> _oneCikanKapaklar = [];
    private KapakModeliDto? _secilenKapak;
    private List<string> _galeriGorselleri = [];
    private List<(string Deger, string Etiket)> _istatistikler = [];
    private int _oneCikanAdet = 4;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await AnimasyonMotoru.ScrollAnimasyonlariniBaslatAsync();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    // Kategori 1 (Kapak)
    private string _kat1Gorsel = "/medya/kapak_kategori.png";
    private string _kat1Etiket = "PREMIUM SERİ";
    private string _kat1Baslik = "Mobilya Kapak Sistemleri";
    private string _kat1Aciklama = "Mutfak ve banyolarınız için lake, membran ve akrilik kapak çözümlerimizle mutfak ve banyolarınızın havasını değiştiriyoruz. İnovatif tasarım ve dayanıklı yapıyla birleşen estetik.";

    // Kategori 2 (Kapı)
    private string _kat2Gorsel = "/medya/kapi_kategori.png";
    private string _kat2Etiket = "DAYANIKLI YAPI";
    private string _kat2Baslik = "İç Mekan Kapı Grubu";
    private string _kat2Aciklama = "Modern tasarımlar, yüksek ses yalıtımı ve üstün estetiği bir araya getiren iç mekan kapı koleksiyonumuz. Mimari detaylarla zenginleştirilmiş premium yüzeyler.";

    // Video ve Medya
    private string _videoUrl = "";
    private string _youtubeUrl = "";
    private string _youtubeVideoId = "";
    private string _videoHiz = "1.0";
    private bool _videoMute = true;
    private string _pdfKatalogUrl = "";
    private List<Ortak.Modeller.Katalog> _anasayfaKataloglar = [];

    // 4 Adim Surec
    private List<(string Baslik, string Aciklama)> _adimlar = new();

    // CTA Banner
    private string _ctaBaslik = "";
    private string _ctaAltBaslik = "";
    private string _ctaButonYazi = "";
    private string _ctaButonLink = "";

    // Referans Logolar
    private List<string> _referanslar = new();

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        await Task.WhenAll(
            HeroIceriginiYukle(),
            OneCikanKapaklariYukle(),
            GaleriGorselleriniYukle(),
            KataloglariYukle()
        );
    }

    private async Task HeroIceriginiYukle()
    {
        try
        {
            var sozluk = await api.GetAsync<Dictionary<string, string>>($"api/desadoor/sayfa-icerigi/anasayfa?dil={dil.AktifDil}");
            if (sozluk != null)
            {
                if (sozluk.TryGetValue("HeroGorselUrl", out var url) && !string.IsNullOrEmpty(url))
                    _heroGorselUrl = url;

                _heroSliderOgeleri =
                [
                    new() {
                        GorselUrl = sozluk.TryGetValue("HeroGorselUrl", out var hg) ? hg : "/medya/hero_banner.png",
                        Etiket = sozluk.TryGetValue("HeroEtiket", out var e) ? e : "MİMARİ ESTETİK & KALİTE",
                        Baslik1 = sozluk.TryGetValue("HeroBaslik1", out var b1) ? b1 : "Yaşam Alanlarınıza",
                        Baslik2 = sozluk.TryGetValue("HeroBaslik2", out var b2) ? b2 : "Sanatsal Dokunuşlar",
                        Aciklama = sozluk.TryGetValue("HeroAciklama", out var a) ? a : "DesaDoor, 25 yılı aşkın tecrübesiyle mobilya kapakları ve dekoratif panellerde Türkiye'nin öncü üreticilerinden biridir. İleri teknoloji 3D modelleme ve CNC kesim altyapımızla hayallerinizi gerçeğe dönüştürüyoruz."
                    }
                ];

                _istatistikler =
                [
                    (sozluk.TryGetValue("Ist1Deger", out var is1d) ? is1d : "25+", sozluk.TryGetValue("Ist1Etiket", out var is1e) ? is1e : "Yıllık Tecrübe"),
                    (sozluk.TryGetValue("Ist2Deger", out var is2d) ? is2d : "500+", sozluk.TryGetValue("Ist2Etiket", out var is2e) ? is2e : "Özgün Model"),
                    (sozluk.TryGetValue("Ist3Deger", out var is3d) ? is3d : "20+", sozluk.TryGetValue("Ist3Etiket", out var is3e) ? is3e : "İhracat Ülkesi"),
                    (sozluk.TryGetValue("Ist4Deger", out var is4d) ? is4d : "%100", sozluk.TryGetValue("Ist4Etiket", out var is4e) ? is4e : "Müşteri Memnuniyeti"),
                ];

                _kat1Gorsel = TamUrl(sozluk.TryGetValue("Kat1Gorsel", out var k1g) && !string.IsNullOrEmpty(k1g) ? k1g : _kat1Gorsel);
                _kat1Etiket = sozluk.TryGetValue("Kat1Etiket", out var k1e) && !string.IsNullOrEmpty(k1e) ? k1e : _kat1Etiket;
                _kat1Baslik = sozluk.TryGetValue("Kat1Baslik", out var k1b) && !string.IsNullOrEmpty(k1b) ? k1b : _kat1Baslik;
                _kat1Aciklama = sozluk.TryGetValue("Kat1Aciklama", out var k1a) && !string.IsNullOrEmpty(k1a) ? k1a : _kat1Aciklama;

                _kat2Gorsel = TamUrl(sozluk.TryGetValue("Kat2Gorsel", out var k2g) && !string.IsNullOrEmpty(k2g) ? k2g : _kat2Gorsel);
                _kat2Etiket = sozluk.TryGetValue("Kat2Etiket", out var k2e) && !string.IsNullOrEmpty(k2e) ? k2e : _kat2Etiket;
                _kat2Baslik = sozluk.TryGetValue("Kat2Baslik", out var k2b) && !string.IsNullOrEmpty(k2b) ? k2b : _kat2Baslik;
                _kat2Aciklama = sozluk.TryGetValue("Kat2Aciklama", out var k2a) && !string.IsNullOrEmpty(k2a) ? k2a : _kat2Aciklama;

                _videoUrl = sozluk.TryGetValue("VideoUrl", out var vu) ? vu : "";
                _youtubeUrl = sozluk.TryGetValue("YoutubeUrl", out var yu) ? yu : "";
                _youtubeVideoId = YoutubeVideoIdCikar(_youtubeUrl);
                _videoHiz = sozluk.TryGetValue("VideoHiz", out var vh) ? vh : "1.0";
                _videoMute = bool.TryParse(sozluk.TryGetValue("VideoMute", out var vm) ? vm : "true", out var vmb) ? vmb : true;
                _pdfKatalogUrl = TamUrl(sozluk.TryGetValue("PdfKatalogUrl", out var pku) ? pku : "");
                _adimlar =
                [
                    (sozluk.TryGetValue("Adim1Baslik", out var a1b) ? a1b : "Sizi Evinizde Ziyaret Ediyoruz", sozluk.TryGetValue("Adim1Aciklama", out var a1a) ? a1a : "Hayalinizdaki mobilyalarin olcusunu alip isteklerinizi ogrenecegiz."),
                    (sozluk.TryGetValue("Adim2Baslik", out var a2b) ? a2b : "On Tasarim Calismalarini Hazirlayacagiz", sozluk.TryGetValue("Adim2Aciklama", out var a2a) ? a2a : "Uzman mimarlarimiz on tasarim surecleri icin tasarim sureclerine baslayacak."),
                    (sozluk.TryGetValue("Adim3Baslik", out var a3b) ? a3b : "Ayrintili Tasarim Surecine Basliyoruz", sozluk.TryGetValue("Adim3Aciklama", out var a3a) ? a3a : "Mimarlarimizin on tasarimlarina vereceginiz onay ile detayli tasarimlarda calisacagiz."),
                    (sozluk.TryGetValue("Adim4Baslik", out var a4b) ? a4b : "Urunlerinizin Kurulumlarini Yapiyoruz", sozluk.TryGetValue("Adim4Aciklama", out var a4a) ? a4a : "Hazirlanan siparislerinizi uzman ekiplerimizle montaja gelecegiz."),
                ];

                _ctaBaslik = sozluk.TryGetValue("CtaBaslik", out var ctb) ? ctb : "Siz Hayal Edin, Biz Tasarlayalim";
                _ctaAltBaslik = sozluk.TryGetValue("CtaAltBaslik", out var cta) ? cta : "Size hayallerinizdeki mutlaki insa edebiliriz";
                _ctaButonYazi = sozluk.TryGetValue("CtaButonYazi", out var ctb2) ? ctb2 : "Iletisime Gec";
                _ctaButonLink = sozluk.TryGetValue("CtaButonLink", out var ctl) ? ctl : "iletisim";

                if (sozluk.TryGetValue("OneCikanAdet", out var ocAdet) && int.TryParse(ocAdet, out var adet) && adet > 0)
                    _oneCikanAdet = adet;
            }
            else
            {
                _heroSliderOgeleri =
                [
                    new() {
                        GorselUrl = "/medya/hero_banner.png",
                        Etiket = "MİMARİ ESTETİK & KALİTE",
                        Baslik1 = "Yaşam Alanlarınıza",
                        Baslik2 = "Sanatsal Dokunuşlar",
                        Aciklama = "DesaDoor, 25 yılı aşkın tecrübesiyle mobilya kapakları ve dekoratif panellerde Türkiye'nin öncü üreticilerinden biridir. İleri teknoloji 3D modelleme ve CNC kesim altyapımızla hayallerinizi gerçeğe dönüştürüyoruz."
                    }
                ];
                IstatistikleriVarsayilanYukle();
            }
        }
        catch
        {
            IstatistikleriVarsayilanYukle();
        }
        finally
        {
        }
    }

    private void IstatistikleriVarsayilanYukle()
    {
        if (_istatistikler.Count == 0)
        {
            _istatistikler =
            [
                ("25+", "Yıllık Tecrübe"),
                ("500+", "Özgün Model"),
                ("20+", "İhracat Ülkesi"),
                ("%100", "Müşteri Memnuniyeti"),
            ];
        }
    }

    private static string YoutubeVideoIdCikar(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        var temizUrl = url.Trim();
        if (!temizUrl.Contains('/') && !temizUrl.Contains('?') && temizUrl.Length == 11)
        {
            return temizUrl;
        }

        if (!Uri.TryCreate(temizUrl, UriKind.Absolute, out var uri))
        {
            return string.Empty;
        }

        if (uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
        {
            return YolParcasiIdCikar(uri.AbsolutePath);
        }

        if (uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
        {
            var videoId = SorgudanDegerAl(uri.Query, "v");
            if (!string.IsNullOrWhiteSpace(videoId))
            {
                return videoId;
            }

            var parcalar = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parcalar.Length >= 2 && (parcalar[0].Equals("embed", StringComparison.OrdinalIgnoreCase) || parcalar[0].Equals("shorts", StringComparison.OrdinalIgnoreCase)))
            {
                return parcalar[1];
            }
        }

        return string.Empty;
    }

    private static string YolParcasiIdCikar(string yol)
    {
        var parcalar = yol.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parcalar.Length == 0 ? string.Empty : parcalar[0];
    }

    private static string? SorgudanDegerAl(string sorgu, string anahtar)
    {
        var temizSorgu = sorgu.TrimStart('?');
        foreach (var parca in temizSorgu.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var esitlikIndeksi = parca.IndexOf('=');
            var parcaAnahtari = esitlikIndeksi >= 0 ? parca[..esitlikIndeksi] : parca;
            if (!parcaAnahtari.Equals(anahtar, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var deger = esitlikIndeksi >= 0 ? parca[(esitlikIndeksi + 1)..] : string.Empty;
            return Uri.UnescapeDataString(deger.Replace("+", " "));
        }

        return null;
    }

    private async Task OneCikanKapaklariYukle()
    {
        _kapakYukleniyor = true;
        try
        {
            var kategorilerListe = await api.GetAsync<List<UrunKategori>>("api/urun-kategorileri");
            var kategorilerSozluk = kategorilerListe?.ToDictionary(k => k.Id) ?? new Dictionary<int, UrunKategori>();

            var urunler = await api.GetAsync<List<Urun>>($"api/urunler?dil={dil.AktifDil}");
            if (urunler != null && urunler.Count > 0)
            {
                var rnd = new Random();
                var karisik = urunler.Where(u => u.AnaGorselMedyaId.HasValue).OrderBy(_ => rnd.Next()).Take(_oneCikanAdet);
                var dtoListe = new List<KapakModeliDto>();
                foreach (var urun in karisik)
                {
                    var dto = await KapakModeliDto.UrunuKapakDtoyaDonustur(urun, api, kategorilerSozluk);
                    dtoListe.Add(dto);
                }
                _oneCikanKapaklar = dtoListe;
            }
        }
        finally
        {
            _kapakYukleniyor = false;
        }
    }

    private async Task GaleriGorselleriniYukle()
    {
        try
        {
            var liste2 = await api.GetAsync<List<string>>("api/desadoor/galeri-gorselleri");
            if (liste2 != null)
                _galeriGorselleri = liste2;
        }
        finally
        {
        }
    }

    private async Task KataloglariYukle()
    {
        try
        {
            var liste = await api.GetAsync<List<Ortak.Modeller.Katalog>>("api/desadoor/kataloglar");
            _anasayfaKataloglar = liste?
                .Where(k => k.AktifMi && !string.IsNullOrWhiteSpace(k.PdfDosyaYolu))
                .OrderBy(k => k.SiraNo)
                .Take(2)
                .ToList() ?? [];
        }
        catch
        {
            _anasayfaKataloglar = [];
        }
    }

    private string KatalogOnizlemeUrl(Ortak.Modeller.Katalog katalog)
    {
        if (!string.IsNullOrWhiteSpace(katalog.KapakResim))
        {
            return TamMedyaUrl(katalog.KapakResim);
        }

        return $"{api.ApiBaseUrl}/api/desadoor/belge-onizleme?dosya={Uri.EscapeDataString(katalog.PdfDosyaYolu)}";
    }

    private string KatalogPdfGostericiUrl(Ortak.Modeller.Katalog katalog)
    {
        return $"/pdf-gosterici?dosya={Uri.EscapeDataString(katalog.PdfDosyaYolu)}&baslik={Uri.EscapeDataString(katalog.Baslik)}&donus={Uri.EscapeDataString("/")}";
    }

    private string KatalogIndirUrl(Ortak.Modeller.Katalog katalog)
    {
        return $"{api.ApiBaseUrl}/api/desadoor/belge-dosya?dosya={Uri.EscapeDataString(katalog.PdfDosyaYolu)}";
    }

    private string EskiKatalogPdfGostericiUrl()
    {
        var dosya = _pdfKatalogUrl.StartsWith(api.ApiBaseUrl, StringComparison.OrdinalIgnoreCase)
            ? _pdfKatalogUrl[api.ApiBaseUrl.Length..].TrimStart('/')
            : _pdfKatalogUrl.TrimStart('/');

        return $"/pdf-gosterici?dosya={Uri.EscapeDataString(dosya)}&baslik={Uri.EscapeDataString(dil.T("anasayfa.katalog.baslik", "2024 Tasarim Katalogumuz Hazir"))}&donus={Uri.EscapeDataString("/")}";
    }

    private string TamMedyaUrl(string yol)
    {
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return yol;
        }

        return $"{api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

private void KapakDetayiAc(KapakModeliDto kapak) => nav.NavigateTo($"/urun/{kapak.Slug}");

    private async Task GaleriAc(int index)
    {
        if (_galeriGorselleri == null || index < 0 || index >= _galeriGorselleri.Count) return;

        var parametreler = new DialogParameters
        {
            { "GorselUrl", _galeriGorselleri[index] },
            { "Index", index },
            { "Toplam", _galeriGorselleri.Count }
        };
        await dialogService.ShowAsync<Bilesenler.GaleriDialog>("Galeri", parametreler);
    }

    private async void OnDilDegisti()
    {
        await Task.WhenAll(
            HeroIceriginiYukle(),
            OneCikanKapaklariYukle(),
            GaleriGorselleriniYukle(),
            KataloglariYukle()
        );
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol)) return "/medya/desadoor_default.png";
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return yol;
        if (yol.StartsWith("/api/")) return $"{api.ApiBaseUrl}{yol}";
        if (yol.StartsWith("/medya/")) return $"{api.ApiBaseUrl}{yol}";
        return yol;
    }
}
