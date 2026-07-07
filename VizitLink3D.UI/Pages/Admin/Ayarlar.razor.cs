using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class Ayarlar : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private FirmaBilgisiServisi FirmaBilgisi { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private bool _yukleniyor;
    private bool _yukleniyorSayfa = true;

    private string _firmaAdi = "VizitLink3D";
    private string _logoUrl = "/img/goldbanyo-logo-kare.png";
    private string _faviconUrl = "/favicon.png";
    private string LogoOnizlemeYolu => MarkaVarligiNormalizeEt(_logoUrl, "/img/goldbanyo-logo-kare.png");
    private string FaviconOnizlemeYolu => MarkaVarligiNormalizeEt(_faviconUrl, "/favicon.png");

    private string _siteBasligi = "Firma vitrini";
    private string _aciklama = "Firma vitrini, koleksiyonlar ve proje çözümleri.";
    private string _anahtarKelimeler = "firma, koleksiyon, ürün, vitrin";
    private string _varsayilanDil = "tr";
    private string _temaModu = "koyu";

    private string _telefon = "+90 224 482 24 00";
    private string _eposta = "info@goldbanyom.com.tr";
    private string _adres = "Çalı Mah. Ömer Biltekin Bulv. No:3/1A Nilüfer / BURSA";
    private string _whatsapp = "+90 533 597 32 14";

    private string _instagram = "https://www.instagram.com/gold.banyom/";
    private string _facebook = "https://www.facebook.com/gold.banyo";
    private string _youtube = "";

    protected override async Task OnInitializedAsync()
    {
        await DilServisi.BaslatAsync();
        var firma = await FirmaBilgisi.GetFirmaAsync();
        if (firma != null)
        {
            _firmaAdi = string.IsNullOrWhiteSpace(firma.Ad) ? _firmaAdi : firma.Ad;
            _logoUrl = MarkaVarligiNormalizeEt(firma.Logo, "/img/goldbanyo-logo-kare.png");
            _faviconUrl = MarkaVarligiNormalizeEt(firma.Favicon, "/favicon.png");
            _siteBasligi = string.IsNullOrWhiteSpace(firma.Ad) ? _siteBasligi : $"{firma.Ad} - Kurumsal Site";
        }

        await AyarlariYukle();
        _yukleniyorSayfa = false;
    }

    private async Task AyarlariYukle()
    {
        try
        {
            var sozluk = await Api.GetAsync<Dictionary<string, string>>("api/sayfa-icerigi/ayarlar");
            if (sozluk != null)
            {
                _logoUrl = MarkaVarligiNormalizeEt(sozluk.GetValueOrDefault("LogoUrl", _logoUrl), "/img/goldbanyo-logo-kare.png");
                _faviconUrl = MarkaVarligiNormalizeEt(sozluk.GetValueOrDefault("FaviconUrl", _faviconUrl), "/favicon.png");
                _siteBasligi = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("SiteBasligi", _siteBasligi), _siteBasligi);
                _aciklama = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Aciklama", _aciklama), _aciklama);
                _anahtarKelimeler = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("AnahtarKelimeler", _anahtarKelimeler), _anahtarKelimeler);
                _varsayilanDil = sozluk.GetValueOrDefault("VarsayilanDil", _varsayilanDil).ToLowerInvariant();
                _temaModu = sozluk.GetValueOrDefault("TemaModu", _temaModu).ToLowerInvariant() == "acik" ? "acik" : "koyu";
                _telefon = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Telefon1", _telefon), _telefon);
                _eposta = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Eposta", _eposta), _eposta);
                _adres = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Adres", _adres), _adres);
                _whatsapp = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Whatsapp", _whatsapp), _whatsapp);
                _instagram = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Instagram", sozluk.GetValueOrDefault("InstagramUrl", _instagram)), _instagram);
                _facebook = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Facebook", sozluk.GetValueOrDefault("FacebookUrl", _facebook)), _facebook);
                _youtube = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Youtube", sozluk.GetValueOrDefault("YoutubeUrl", _youtube)), _youtube);
            }
        }
        catch { /* ayarlar yüklenemezse varsayılan değerler kullanılır */ }
    }

    private async Task Kaydet(int bolum)
    {
        _yukleniyor = true;
        try
        {
            var ayarlar = new Dictionary<string, string>();

            if (bolum == 4)
            {
                ayarlar["LogoUrl"] = _logoUrl;
                ayarlar["FaviconUrl"] = _faviconUrl;
            }
            else if (bolum == 1)
            {
                ayarlar["SiteBasligi"] = _siteBasligi;
                ayarlar["Aciklama"] = _aciklama;
                ayarlar["AnahtarKelimeler"] = _anahtarKelimeler;
            }
            else if (bolum == 5)
            {
                ayarlar["VarsayilanDil"] = _varsayilanDil;
                ayarlar["TemaModu"] = _temaModu;
            }
            else if (bolum == 2)
            {
                ayarlar["Telefon1"] = _telefon;
                ayarlar["Eposta"] = _eposta;
                ayarlar["Adres"] = _adres;
                ayarlar["Whatsapp"] = _whatsapp;
            }
            else if (bolum == 3)
            {
                ayarlar["Instagram"] = _instagram;
                ayarlar["InstagramUrl"] = _instagram;
                ayarlar["Facebook"] = _facebook;
                ayarlar["FacebookUrl"] = _facebook;
                ayarlar["Youtube"] = _youtube;
                ayarlar["YoutubeUrl"] = _youtube;
            }

            foreach (var (anahtar, deger) in ayarlar)
            {
                await Api.PutAsync<object>("api/sayfa-icerigi", new
                {
                    Bolum = "ayarlar",
                    Anahtar = anahtar,
                    Deger = deger,
                    Dil = "tr"
                });
            }

            Snackbar.Add(DilServisi.T("admin.ayarlar.guncellendi", "Ayarlar başarıyla güncellendi."), Severity.Success);
        }
        catch
        {
            Snackbar.Add(DilServisi.T("admin.ayarlar.hataOlustu", "Ayarlar kaydedilirken hata oluştu."), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task LogoYukle(IBrowserFile dosya)
    {
        if (dosya == null) return;
        _yukleniyor = true;
        try
        {
            using var icerik = new MultipartFormDataContent();
            using var dosyaAkisi = dosya.OpenReadStream(10_000_000); // En fazla 10MB
            using var dosyaIcerigi = new StreamContent(dosyaAkisi);
            icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

            var yanit = await Api.PostMultipartAsync<VizitLink3D.Ortak.Modeller.Medya.Medya>("api/medya/yukle", icerik);
            if (yanit?.BasariliMi == true && yanit.Veri?.DosyaYolu != null)
            {
                _logoUrl = yanit.Veri.DosyaYolu.StartsWith('/') ? yanit.Veri.DosyaYolu : "/" + yanit.Veri.DosyaYolu;
                Snackbar.Add(DilServisi.T("admin.ayarlar.logoYuklendi", "Logo başarıyla yüklendi. Kaydet butonuna basarak güncelleyebilirsiniz."), Severity.Info);
            }
            else
            {
                Snackbar.Add(yanit?.Mesaj ?? DilServisi.T("admin.ayarlar.logoYuklemeHatasi", "Logo yüklenirken hata oluştu."), Severity.Error);
            }
        }
        catch (Exception hata)
        {
            Snackbar.Add(string.Format(DilServisi.T("admin.ayarlar.logoYuklemeHataDetay", "Logo yüklenirken hata oluştu: {0}"), hata.Message), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task FaviconYukle(IBrowserFile dosya)
    {
        if (dosya == null) return;
        _yukleniyor = true;
        try
        {
            using var icerik = new MultipartFormDataContent();
            using var dosyaAkisi = dosya.OpenReadStream(2_000_000); // En fazla 2MB
            using var dosyaIcerigi = new StreamContent(dosyaAkisi);
            icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

            var yanit = await Api.PostMultipartAsync<VizitLink3D.Ortak.Modeller.Medya.Medya>("api/medya/yukle", icerik);
            if (yanit?.BasariliMi == true && yanit.Veri?.DosyaYolu != null)
            {
                _faviconUrl = yanit.Veri.DosyaYolu.StartsWith('/') ? yanit.Veri.DosyaYolu : "/" + yanit.Veri.DosyaYolu;
                Snackbar.Add(DilServisi.T("admin.ayarlar.faviconYuklendi", "Favicon başarıyla yüklendi. Kaydet butonuna basarak güncelleyebilirsiniz."), Severity.Info);
            }
            else
            {
                Snackbar.Add(yanit?.Mesaj ?? DilServisi.T("admin.ayarlar.faviconYuklemeHatasi", "Favicon yüklenirken hata oluştu."), Severity.Error);
            }
        }
        catch (Exception hata)
        {
            Snackbar.Add(string.Format(DilServisi.T("admin.ayarlar.faviconYuklemeHataDetay", "Favicon yüklenirken hata oluştu: {0}"), hata.Message), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private static string MarkaVarligiNormalizeEt(string? deger, string varsayilanDeger)
    {
        if (string.IsNullOrWhiteSpace(deger))
        {
            return varsayilanDeger;
        }

        var normalizeDeger = deger.Contains("vizitlink3d", StringComparison.OrdinalIgnoreCase)
            ? varsayilanDeger
            : deger;

        if (normalizeDeger.Equals("/img/goldbanyo-logo.svg", StringComparison.OrdinalIgnoreCase)
            || normalizeDeger.Equals("img/goldbanyo-logo.svg", StringComparison.OrdinalIgnoreCase))
        {
            normalizeDeger = "/img/goldbanyo-logo-kare.png";
        }

        if (normalizeDeger.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || normalizeDeger.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || normalizeDeger.StartsWith("/", StringComparison.Ordinal))
        {
            return normalizeDeger;
        }

        return "/" + normalizeDeger.TrimStart('~').TrimStart('/');
    }

    private static string MarkaMetniNormalizeEt(string? deger, string varsayilanDeger)
    {
        if (string.IsNullOrWhiteSpace(deger))
        {
            return varsayilanDeger;
        }

        return deger.Contains("vizitlink3d", StringComparison.OrdinalIgnoreCase)
            || deger.Contains("3dvizitlink", StringComparison.OrdinalIgnoreCase)
            ? varsayilanDeger
            : deger;
    }
}

