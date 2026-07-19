using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;
using VizitLink3D.UI.Bilesenler.Anasayfa;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.UI.Pages;

public partial class AnaSayfa : IDisposable
{
    [Inject] public IJSRuntime JS { get; set; } = default!;
    [Inject] public AnimasyonMotoruServisi AnimasyonMotoru { get; set; } = default!;

    // ─── HERO ────────────────────────────────────────────────────
    private string _heroUstBaslik = "Siz Hayal Edin";
    private string _heroBaslik = "Biz Tasarlayalım.";
    private string _heroAciklama = "32 yıllık zanaat mirasımızı, modern mimari dokunuşlar ve kişiye özel tasarım anlayışıyla banyonuza taşıyoruz.";
    private string _heroGorselUrl = "/medya/goldbanyo/hero-banyo-mobilyasi.jpg";
    private string _heroCta1Metin = "Koleksiyonları Keşfet";
    private string _heroCta1Url = "/urunler";
    private string _heroCta2Metin = "Projelerimiz";
    private string _heroCta2Url = "/projeler";

    // ─── İSTATİSTİKLER ───────────────────────────────────────────
    private List<IstatistikBolumu.IstatistikOgesi> _istatistikler = [];

    // ─── KOLEKSİYONLAR BENTO GRID ────────────────────────────────
    private string _koleksiyonBaslik = "Koleksiyonlarımız";
    private string _koleksiyonAciklama = "Mekanınıza sofistike bir zarafet katmak için tasarlanmış özel seriler.";
    private string _kesfetMetin = "Keşfet";
    private List<KoleksiyonlarBentoGrid.BentoKartVerisi> _koleksiyonKartlari = [];

    // ─── ENDÜSTRİYEL ZANAAT ──────────────────────────────────────
    private string _endustriyelBaslik = "Nero Marquina & Mat Altın";
    private string _endustriyelAciklama = "Detaylardaki kusursuzluk, Gold Banyo'nun 32 yıllık üretim mirasının temelidir. Her bir mobilya, İtalyan mermer desenleri ve el işçiliği altın detaylarla hayat bulur.";
    private string _endustriyelGorsel = "https://www.goldbanyom.com.tr/wp-content/uploads/2022/04/gold-exclusive-en.jpg";
    private List<EndustriyelZanaatBolumu.OzellikOgesi> _endustriyelOzellikler = [];

    // ─── AKILLI YAŞAM ────────────────────────────────────────────
    private string _akilliBaslik = "Teknoloji ile Estetiğin Buluşması";
    private string _akilliAciklama = "Akıllı ev sistemleriyle entegre, modüler yapıda banyo çözümleri. Aydınlatmadan sıcaklık kontrolüne kadar her detay parmaklarınızın ucunda.";
    private string _akilliGorsel = "https://www.goldbanyom.com.tr/wp-content/uploads/2022/04/gold-premium-en.jpg";
    private string _akilliUrunKodu = "SMART-X1";
    private List<AkilliYasamBolumu.OzellikOgesi> _akilliOzellikler = [];

    // ─── KATALOG BANNER ──────────────────────────────────────────
    private string _katalogBaslik = "Yeniliği Keşfedin: 2024 Master Kataloğu";
    private string _katalogAciklama = "Gold Banyo'nun tüm koleksiyonlarını, özel tasarım seçeneklerini ve malzeme paletini tek bir katalogda keşfedin.";
    private string _katalogButonMetin = "Kataloğu İncele";
    private string _katalogButonUrl = "/kataloglar";
    private string? _katalogIndirmeUrl;
    private string _katalogGorsel = "";

    // ─── KÜRESEL GÜÇ ─────────────────────────────────────────────
    private string _kucreselBaslik = "Dünyaya Açılan Türk Zanaati";
    private string _kucreselAciklama = "20'den fazla ülkeye ihracat yapan, %100 yerli üretim gücüyle global pazarda söz sahibi bir markayız.";
    private string? _kucreselAlinti;
    private string? _kucreselAlintiSahibi;
    private List<KucreselGucBolumu.GucOgesi> _kucreselOgeler = [];

    // ─── SEÇKİN PROJELER ─────────────────────────────────────────
    private List<ProjelerBolumu.ProjeVerisi> _projeler = [];

    // ─── CTA BANNER ──────────────────────────────────────────────
    private string _ctaBaslik = "Banyonuz İçin Lüksün Mimarı Olun";
    private string _ctaAciklama = "Mimarlarımızla ücretsiz ön görüşme planlayın.";
    private string _ctaButonMetin = "Şimdi Teklif Al";
    private string _ctaButonUrl = "/iletisim";

    // ─── MEVCUT VERİLER (geriye dönük uyumlu) ────────────────────
    private List<HeroSliderOgesi> _heroSliderOgeleri = [];
    private List<KapakModeliDto> _oneCikanKapaklar = [];
    private List<string> _galeriGorselleri = [];
    private int _oneCikanAdet = 4;
#pragma warning disable CS0414
    private bool _kapakYukleniyor = true;
#pragma warning restore CS0414

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await AnimasyonMotoru.ScrollAnimasyonlariniBaslatAsync();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        await Task.WhenAll(
            SayfaIceriginiYukle(),
            OneCikanKapaklariYukle(),
            GaleriGorselleriniYukle()
        );
    }

    private async Task SayfaIceriginiYukle()
    {
        try
        {
            var sozluk = await api.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/anasayfa?dil={dil.AktifDil}");
            if (sozluk != null && sozluk.Count > 0)
            {
                // Hero
                _heroUstBaslik = sozluk.TryGetValue("HeroUstBaslik", out var hub) && !string.IsNullOrEmpty(hub) ? hub : _heroUstBaslik;
                _heroBaslik = sozluk.TryGetValue("HeroBaslik", out var hb) && !string.IsNullOrEmpty(hb) ? hb : _heroBaslik;
                _heroAciklama = sozluk.TryGetValue("HeroAciklama", out var ha) && !string.IsNullOrEmpty(ha) ? ha : _heroAciklama;
                _heroGorselUrl = sozluk.TryGetValue("HeroGorselUrl", out var hg) && !string.IsNullOrEmpty(hg) ? hg : _heroGorselUrl;
                _heroCta1Metin = sozluk.TryGetValue("HeroCta1Metin", out var hc1m) && !string.IsNullOrEmpty(hc1m) ? hc1m : _heroCta1Metin;
                _heroCta1Url = sozluk.TryGetValue("HeroCta1Url", out var hc1u) && !string.IsNullOrEmpty(hc1u) ? hc1u : _heroCta1Url;
                _heroCta2Metin = sozluk.TryGetValue("HeroCta2Metin", out var hc2m) && !string.IsNullOrEmpty(hc2m) ? hc2m : _heroCta2Metin;
                _heroCta2Url = sozluk.TryGetValue("HeroCta2Url", out var hc2u) && !string.IsNullOrEmpty(hc2u) ? hc2u : _heroCta2Url;

                // İstatistikler
                _istatistikler =
                [
                    new() { Deger = sozluk.TryGetValue("Ist1Deger", out var i1d) && !string.IsNullOrEmpty(i1d) ? i1d : "1993'ten beri", Etiket = sozluk.TryGetValue("Ist1Etiket", out var i1e) && !string.IsNullOrEmpty(i1e) ? i1e : "Yıllık Miras" },
                    new() { Deger = sozluk.TryGetValue("Ist2Deger", out var i2d) && !string.IsNullOrEmpty(i2d) ? i2d : "500+", Etiket = sozluk.TryGetValue("Ist2Etiket", out var i2e) && !string.IsNullOrEmpty(i2e) ? i2e : "Özgün Model" },
                    new() { Deger = sozluk.TryGetValue("Ist3Deger", out var i3d) && !string.IsNullOrEmpty(i3d) ? i3d : "20+", Etiket = sozluk.TryGetValue("Ist3Etiket", out var i3e) && !string.IsNullOrEmpty(i3e) ? i3e : "İhracat Ülkesi" },
                    new() { Deger = sozluk.TryGetValue("Ist4Deger", out var i4d) && !string.IsNullOrEmpty(i4d) ? i4d : "%100", Etiket = sozluk.TryGetValue("Ist4Etiket", out var i4e) && !string.IsNullOrEmpty(i4e) ? i4e : "Müşteri Memnuniyeti" },
                ];

                // Koleksiyonlar
                _koleksiyonBaslik = sozluk.TryGetValue("KoleksiyonBaslik", out var kb) && !string.IsNullOrEmpty(kb) ? kb : _koleksiyonBaslik;
                _koleksiyonAciklama = sozluk.TryGetValue("KoleksiyonAciklama", out var ka) && !string.IsNullOrEmpty(ka) ? ka : _koleksiyonAciklama;
                _kesfetMetin = sozluk.TryGetValue("KesfetMetin", out var km) && !string.IsNullOrEmpty(km) ? km : _kesfetMetin;
                _koleksiyonKartlari =
                [
                    new() { Baslik = sozluk.TryGetValue("Kol1Baslik", out var k1b) && !string.IsNullOrEmpty(k1b) ? k1b : dil.T("kol1_baslik", "Gold Exclusive"), Etiket = sozluk.TryGetValue("Kol1Etiket", out var k1e) && !string.IsNullOrEmpty(k1e) ? k1e : dil.T("kol1_etiket", "EXCLUSIVE"), Aciklama = sozluk.TryGetValue("Kol1Aciklama", out var k1a) && !string.IsNullOrEmpty(k1a) ? k1a : dil.T("kol1_aciklama", "Lüksün en saf hali. 24 ayar altın detaylar ve el işçiliği mermer dokuları."), GorselUrl = TamUrl(sozluk.TryGetValue("Kol1Gorsel", out var k1g) && !string.IsNullOrEmpty(k1g) ? k1g : "/medya/goldbanyo/hero-banyo-mobilyasi.jpg"), Href = "/urunler#exclusive", Buyuk = true },
                    new() { Baslik = sozluk.TryGetValue("Kol2Baslik", out var k2b) && !string.IsNullOrEmpty(k2b) ? k2b : dil.T("kol2_baslik", "Premium"), Etiket = sozluk.TryGetValue("Kol2Etiket", out var k2e) && !string.IsNullOrEmpty(k2e) ? k2e : dil.T("kol2_etiket", "PREMIUM"), Aciklama = sozluk.TryGetValue("Kol2Aciklama", out var k2a) && !string.IsNullOrEmpty(k2a) ? k2a : dil.T("kol2_aciklama", "Modern çizgiler, güçlü malzeme kalitesi."), GorselUrl = TamUrl(sozluk.TryGetValue("Kol2Gorsel", out var k2g) && !string.IsNullOrEmpty(k2g) ? k2g : "/medya/goldbanyo/showroom.jpg"), Href = "/urunler#premium", Buyuk = false },
                    new() { Baslik = sozluk.TryGetValue("Kol3Baslik", out var k3b) && !string.IsNullOrEmpty(k3b) ? k3b : dil.T("kol3_baslik", "Trend"), Etiket = sozluk.TryGetValue("Kol3Etiket", out var k3e) && !string.IsNullOrEmpty(k3e) ? k3e : dil.T("kol3_etiket", "TREND"), Aciklama = sozluk.TryGetValue("Kol3Aciklama", out var k3a) && !string.IsNullOrEmpty(k3a) ? k3a : dil.T("kol3_aciklama", "Güncel yaşam stilleri için."), GorselUrl = TamUrl(sozluk.TryGetValue("Kol3Gorsel", out var k3g) && !string.IsNullOrEmpty(k3g) ? k3g : "/medya/goldbanyo/uretim.jpg"), Href = "/urunler#trend", Buyuk = false },
                    new() { Baslik = sozluk.TryGetValue("Kol4Baslik", out var k4b) && !string.IsNullOrEmpty(k4b) ? k4b : dil.T("kol4_baslik", "Standard"), Etiket = sozluk.TryGetValue("Kol4Etiket", out var k4e) && !string.IsNullOrEmpty(k4e) ? k4e : dil.T("kol4_etiket", "STANDARD"), Aciklama = sozluk.TryGetValue("Kol4Aciklama", out var k4a) && !string.IsNullOrEmpty(k4a) ? k4a : dil.T("kol4_aciklama", "Klasik tasarım, kalıcı zarafet."), GorselUrl = TamUrl(sozluk.TryGetValue("Kol4Gorsel", out var k4g) && !string.IsNullOrEmpty(k4g) ? k4g : "/medya/goldbanyo/hero-banyo-mobilyasi.jpg"), Href = "/urunler#standart", Buyuk = false },
                ];

                // Endüstriyel Zanaat
                _endustriyelBaslik = sozluk.TryGetValue("EndustriyelBaslik", out var eb) && !string.IsNullOrEmpty(eb) ? eb : _endustriyelBaslik;
                _endustriyelAciklama = sozluk.TryGetValue("EndustriyelAciklama", out var ea) && !string.IsNullOrEmpty(ea) ? ea : _endustriyelAciklama;
                _endustriyelGorsel = TamUrl(sozluk.TryGetValue("EndustriyelGorsel", out var eg) && !string.IsNullOrEmpty(eg) ? eg : _endustriyelGorsel);
                _endustriyelOzellikler =
                [
                    new() { Etiket = sozluk.TryGetValue("EndOz1Etiket", out var eo1e) && !string.IsNullOrEmpty(eo1e) ? eo1e : dil.T("end_oz1_etiket", "A+ Kalite Malzeme"), Deger = sozluk.TryGetValue("EndOz1Deger", out var eo1d) && !string.IsNullOrEmpty(eo1d) ? eo1d : dil.T("end_oz1_deger", "Avrupa standartlarında, sertifikalı hammaddeler") },
                    new() { Etiket = sozluk.TryGetValue("EndOz2Etiket", out var eo2e) && !string.IsNullOrEmpty(eo2e) ? eo2e : dil.T("end_oz2_etiket", "Hassas Üretim"), Deger = sozluk.TryGetValue("EndOz2Deger", out var eo2d) && !string.IsNullOrEmpty(eo2d) ? eo2d : dil.T("end_oz2_deger", "CNC kontrollü, milimetrik işçilik") },
                ];

                // Akıllı Yaşam
                _akilliBaslik = sozluk.TryGetValue("AkilliBaslik", out var ab) && !string.IsNullOrEmpty(ab) ? ab : _akilliBaslik;
                _akilliAciklama = sozluk.TryGetValue("AkilliAciklama", out var aa) && !string.IsNullOrEmpty(aa) ? aa : _akilliAciklama;
                _akilliGorsel = TamUrl(sozluk.TryGetValue("AkilliGorsel", out var ag) && !string.IsNullOrEmpty(ag) ? ag : _akilliGorsel);
                _akilliUrunKodu = sozluk.TryGetValue("AkilliUrunKodu", out var auk) && !string.IsNullOrEmpty(auk) ? auk : _akilliUrunKodu;
                _akilliOzellikler =
                [
                    new() { Ikon = "smart_home", Baslik = sozluk.TryGetValue("AkOz1Baslik", out var ao1b) && !string.IsNullOrEmpty(ao1b) ? ao1b : dil.T("ak_oz1_baslik", "Akıllı Entegrasyon"), Aciklama = sozluk.TryGetValue("AkOz1Aciklama", out var ao1a) && !string.IsNullOrEmpty(ao1a) ? ao1a : dil.T("ak_oz1_aciklama", "Alexa, Google Home ve Apple HomeKit ile tam uyumlu aydınlatma ve iklimlendirme.") },
                    new() { Ikon = "dashboard_customize", Baslik = sozluk.TryGetValue("AkOz2Baslik", out var ao2b) && !string.IsNullOrEmpty(ao2b) ? ao2b : dil.T("ak_oz2_baslik", "Modüler Mimari"), Aciklama = sozluk.TryGetValue("AkOz2Aciklama", out var ao2a) && !string.IsNullOrEmpty(ao2a) ? ao2a : dil.T("ak_oz2_aciklama", "İhtiyacınıza göre genişleyen, sök-tak mekanizmalı dolap ve tezgah sistemleri.") },
                ];

                // Katalog Banner
                _katalogBaslik = sozluk.TryGetValue("KatalogBaslik", out var kab) && !string.IsNullOrEmpty(kab) ? kab : _katalogBaslik;
                _katalogAciklama = sozluk.TryGetValue("KatalogAciklama", out var kaa) && !string.IsNullOrEmpty(kaa) ? kaa : _katalogAciklama;
                _katalogButonMetin = sozluk.TryGetValue("KatalogButonMetin", out var kbm) && !string.IsNullOrEmpty(kbm) ? kbm : _katalogButonMetin;
                _katalogButonUrl = sozluk.TryGetValue("KatalogButonUrl", out var kbu) && !string.IsNullOrEmpty(kbu) ? kbu : _katalogButonUrl;
                _katalogIndirmeUrl = sozluk.TryGetValue("KatalogIndirmeUrl", out var kiu) ? kiu : null;
                _katalogGorsel = TamUrl(sozluk.TryGetValue("KatalogGorsel", out var kg) ? kg : "");

                // Küresel Güç
                _kucreselBaslik = sozluk.TryGetValue("KucreselBaslik", out var kub) && !string.IsNullOrEmpty(kub) ? kub : _kucreselBaslik;
                _kucreselAciklama = sozluk.TryGetValue("KucreselAciklama", out var kua) && !string.IsNullOrEmpty(kua) ? kua : _kucreselAciklama;
                _kucreselAlinti = sozluk.TryGetValue("KucreselAlinti", out var kual) ? kual : null;
                _kucreselAlintiSahibi = sozluk.TryGetValue("KucreselAlintiSahibi", out var kuas) ? kuas : null;
                _kucreselOgeler =
                [
                    new() { Ikon = "language", Baslik = sozluk.TryGetValue("KucOg1Baslik", out var ko1b) && !string.IsNullOrEmpty(ko1b) ? ko1b : dil.T("kuc_og1_baslik", "Global Bayi Ağı"), Aciklama = sozluk.TryGetValue("KucOg1Aciklama", out var ko1a) && !string.IsNullOrEmpty(ko1a) ? ko1a : dil.T("kuc_og1_aciklama", "20+ ülkede 150'den fazla yetkili satış noktası ile dünya çapında erişim.") },
                    new() { Ikon = "architecture", Baslik = sozluk.TryGetValue("KucOg2Baslik", out var ko2b) && !string.IsNullOrEmpty(ko2b) ? ko2b : dil.T("kuc_og2_baslik", "İmza Projeler"), Aciklama = sozluk.TryGetValue("KucOg2Aciklama", out var ko2a) && !string.IsNullOrEmpty(ko2a) ? ko2a : dil.T("kuc_og2_aciklama", "Türkiye ve dünyada özel konut, otel ve rezidans projelerinde Gold Banyo imzası.") },
                    new() { Ikon = "reviews", Baslik = sozluk.TryGetValue("KucOg3Baslik", out var ko3b) && !string.IsNullOrEmpty(ko3b) ? ko3b : dil.T("kuc_og3_baslik", "Müşteri Yorumları"), Aciklama = sozluk.TryGetValue("KucOg3Aciklama", out var ko3a) && !string.IsNullOrEmpty(ko3a) ? ko3a : dil.T("kuc_og3_aciklama", "%100 müşteri memnuniyeti hedefiyle, her projede kusursuz deneyim sunuyoruz.") },
                ];

                // CTA Banner
                _ctaBaslik = sozluk.TryGetValue("CtaBaslik", out var cb) && !string.IsNullOrEmpty(cb) ? cb : _ctaBaslik;
                _ctaAciklama = sozluk.TryGetValue("CtaAciklama", out var ca) && !string.IsNullOrEmpty(ca) ? ca : _ctaAciklama;
                _ctaButonMetin = sozluk.TryGetValue("CtaButonMetin", out var cbm) && !string.IsNullOrEmpty(cbm) ? cbm : _ctaButonMetin;
                _ctaButonUrl = sozluk.TryGetValue("CtaButonUrl", out var cbu) && !string.IsNullOrEmpty(cbu) ? cbu : _ctaButonUrl;

                // Seçkin Projeler
                _projeler =
                [
                    new() { GorselUrl = TamUrl(sozluk.TryGetValue("Proje1Gorsel", out var p1g) ? p1g : ""), Sehir = sozluk.TryGetValue("Proje1Sehir", out var p1s) && !string.IsNullOrEmpty(p1s) ? p1s : dil.T("proje1_sehir", "İstanbul, Türkiye"), ProjeAd = sozluk.TryGetValue("Proje1Ad", out var p1a) && !string.IsNullOrEmpty(p1a) ? p1a : dil.T("proje1_ad", "Boğaziçi Rezidans"), Aciklama = sozluk.TryGetValue("Proje1Aciklama", out var p1ac) && !string.IsNullOrEmpty(p1ac) ? p1ac : dil.T("proje1_aciklama", "Boğaz manzaralı lüks rezidansta özel banyo tasarımı.") },
                    new() { GorselUrl = TamUrl(sozluk.TryGetValue("Proje2Gorsel", out var p2g) ? p2g : ""), Sehir = sozluk.TryGetValue("Proje2Sehir", out var p2s) && !string.IsNullOrEmpty(p2s) ? p2s : dil.T("proje2_sehir", "Antalya, Türkiye"), ProjeAd = sozluk.TryGetValue("Proje2Ad", out var p2a) && !string.IsNullOrEmpty(p2a) ? p2a : dil.T("proje2_ad", "Titanic Luxury Otel"), Aciklama = sozluk.TryGetValue("Proje2Aciklama", out var p2ac) && !string.IsNullOrEmpty(p2ac) ? p2ac : dil.T("proje2_aciklama", "Akdeniz manzaralı otel süitinde altın detaylı banyo koleksiyonu.") },
                    new() { GorselUrl = TamUrl(sozluk.TryGetValue("Proje3Gorsel", out var p3g) ? p3g : ""), Sehir = sozluk.TryGetValue("Proje3Sehir", out var p3s) && !string.IsNullOrEmpty(p3s) ? p3s : dil.T("proje3_sehir", "Bodrum, Türkiye"), ProjeAd = sozluk.TryGetValue("Proje3Ad", out var p3a) && !string.IsNullOrEmpty(p3a) ? p3a : dil.T("proje3_ad", "Yalıkavak Villaları"), Aciklama = sozluk.TryGetValue("Proje3Aciklama", out var p3ac) && !string.IsNullOrEmpty(p3ac) ? p3ac : dil.T("proje3_aciklama", "Ege manzaralı özel villada modern lüks banyo tasarımı.") },
                ];

                // Mevcut veriler (geriye dönük uyumlu)
                if (sozluk.TryGetValue("OneCikanAdet", out var ocAdet) && int.TryParse(ocAdet, out var adet) && adet > 0)
                    _oneCikanAdet = adet;

                // Hero slider fallback
                _heroSliderOgeleri =
                [
                    new() {
                        GorselUrl = _heroGorselUrl,
                        Etiket = "GOLD BANYO",
                        Baslik1 = _heroUstBaslik,
                        Baslik2 = _heroBaslik,
                        Aciklama = _heroAciklama
                    }
                ];
            }
            else
            {
                VarsayilanDegerleriYukle();
            }
        }
        catch
        {
            VarsayilanDegerleriYukle();
        }
    }

    private void VarsayilanDegerleriYukle()
    {
        _istatistikler =
        [
            new() { Deger = dil.T("ist_yillik_miras_deger", "1993'ten beri"), Etiket = dil.T("ist_yillik_miras_etiket", "Yıllık Miras") },
            new() { Deger = dil.T("ist_ozgun_model_deger", "500+"), Etiket = dil.T("ist_ozgun_model_etiket", "Özgün Model") },
            new() { Deger = dil.T("ist_ihracat_ulke_deger", "20+"), Etiket = dil.T("ist_ihracat_ulke_etiket", "İhracat Ülkesi") },
            new() { Deger = dil.T("ist_musteri_memnuniyeti_deger", "%100"), Etiket = dil.T("ist_musteri_memnuniyeti_etiket", "Müşteri Memnuniyeti") },
        ];

        _koleksiyonKartlari =
        [
            new() { Baslik = dil.T("kol_exclusive_baslik", "Gold Exclusive"), Etiket = dil.T("kol_exclusive_etiket", "EXCLUSIVE"), Aciklama = dil.T("kol_exclusive_aciklama", "Lüksün en saf hali. 24 ayar altın detaylar ve el işçiliği mermer dokuları."), GorselUrl = "/medya/goldbanyo/hero-banyo-mobilyasi.jpg", Href = "/urunler#exclusive", Buyuk = true },
            new() { Baslik = dil.T("kol_premium_baslik", "Premium"), Etiket = dil.T("kol_premium_etiket", "PREMIUM"), Aciklama = dil.T("kol_premium_aciklama", "Modern çizgiler, güçlü malzeme kalitesi."), GorselUrl = "/medya/goldbanyo/showroom.jpg", Href = "/urunler#premium", Buyuk = false },
            new() { Baslik = dil.T("kol_trend_baslik", "Trend"), Etiket = dil.T("kol_trend_etiket", "TREND"), Aciklama = dil.T("kol_trend_aciklama", "Güncel yaşam stilleri için."), GorselUrl = "/medya/goldbanyo/uretim.jpg", Href = "/urunler#trend", Buyuk = false },
            new() { Baslik = dil.T("kol_standard_baslik", "Standard"), Etiket = dil.T("kol_standard_etiket", "STANDARD"), Aciklama = dil.T("kol_standard_aciklama", "Klasik tasarım, kalıcı zarafet."), GorselUrl = "/medya/goldbanyo/hero-banyo-mobilyasi.jpg", Href = "/urunler#standart", Buyuk = false },
        ];

        _endustriyelOzellikler =
        [
            new() { Etiket = dil.T("end_kalite_malzeme_etiket", "A+ Kalite Malzeme"), Deger = dil.T("end_kalite_malzeme_deger", "Avrupa standartlarında, sertifikalı hammaddeler") },
            new() { Etiket = dil.T("end_hassas_uretim_etiket", "Hassas Üretim"), Deger = dil.T("end_hassas_uretim_deger", "CNC kontrollü, milimetrik işçilik") },
        ];

        _akilliOzellikler =
        [
            new() { Ikon = "smart_home", Baslik = dil.T("ak_akilli_entegrasyon_baslik", "Akıllı Entegrasyon"), Aciklama = dil.T("ak_akilli_entegrasyon_aciklama", "Alexa, Google Home ve Apple HomeKit ile tam uyumlu aydınlatma ve iklimlendirme.") },
            new() { Ikon = "dashboard_customize", Baslik = dil.T("ak_moduler_mimari_baslik", "Modüler Mimari"), Aciklama = dil.T("ak_moduler_mimari_aciklama", "İhtiyacınıza göre genişleyen, sök-tak mekanizmalı dolap ve tezgah sistemleri.") },
        ];

        _kucreselOgeler =
        [
            new() { Ikon = "language", Baslik = dil.T("kuc_og1_baslik_fb", "Global Bayi Ağı"), Aciklama = dil.T("kuc_og1_aciklama_fb", "20+ ülkede 150'den fazla yetkili satış noktası ile dünya çapında erişim.") },
            new() { Ikon = "architecture", Baslik = dil.T("kuc_og2_baslik_fb", "İmza Projeler"), Aciklama = dil.T("kuc_og2_aciklama_fb", "Türkiye ve dünyada özel konut, otel ve rezidans projelerinde Gold Banyo imzası.") },
            new() { Ikon = "reviews", Baslik = dil.T("kuc_og3_baslik_fb", "Müşteri Yorumları"), Aciklama = dil.T("kuc_og3_aciklama_fb", "%100 müşteri memnuniyeti hedefiyle, her projede kusursuz deneyim sunuyoruz.") },
        ];

        _projeler =
        [
            new() { GorselUrl = "/medya/projeler/placeholder.jpg", Sehir = dil.T("proje1_sehir_fb", "İstanbul, Türkiye"), ProjeAd = dil.T("proje1_ad_fb", "Boğaziçi Rezidans"), Aciklama = dil.T("proje1_aciklama_fb", "Boğaz manzaralı lüks rezidansta özel banyo tasarımı.") },
            new() { GorselUrl = "/medya/projeler/placeholder.jpg", Sehir = dil.T("proje2_sehir_fb", "Antalya, Türkiye"), ProjeAd = dil.T("proje2_ad_fb", "Titanic Luxury Otel"), Aciklama = dil.T("proje2_aciklama_fb", "Akdeniz manzaralı otel süitinde altın detaylı banyo koleksiyonu.") },
            new() { GorselUrl = "/medya/projeler/placeholder.jpg", Sehir = dil.T("proje3_sehir_fb", "Bodrum, Türkiye"), ProjeAd = dil.T("proje3_ad_fb", "Yalıkavak Villaları"), Aciklama = dil.T("proje3_aciklama_fb", "Ege manzaralı özel villada modern lüks banyo tasarımı.") },
        ];

        _heroSliderOgeleri =
        [
            new() {
                GorselUrl = _heroGorselUrl,
                Etiket = "GOLD BANYO",
                Baslik1 = _heroUstBaslik,
                Baslik2 = _heroBaslik,
                Aciklama = _heroAciklama
            }
        ];
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
            var liste2 = await api.GetAsync<List<string>>("api/galeri-gorselleri");
            if (liste2 != null)
                _galeriGorselleri = liste2;
        }
        catch
        {
            // sessiz
        }
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol)) return "";
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return yol;
        if (yol.StartsWith("/api/")) return $"{api.ApiBaseUrl}{yol}";
        if (yol.StartsWith("/medya/")) return $"{api.ApiBaseUrl}{yol}";
        return yol;
    }

    private string TamMedyaUrl(string yol)
    {
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return yol;
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
            SayfaIceriginiYukle(),
            OneCikanKapaklariYukle(),
            GaleriGorselleriniYukle()
        );
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }
}
