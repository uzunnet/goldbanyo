using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using MudBlazor;
using Desadoor.UI.Servisler;

namespace Desadoor.UI.Pages.Admin;

public partial class Ayarlar : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private bool _yukleniyor;
    private bool _yukleniyorSayfa = true;

    private string _logoUrl = "/img/desadoor-logo.svg";
    private string _faviconUrl = "/img/desadoor-icon.svg";

    private string _siteBasligi = "DesaDoor - Kapak Sistemleri";
    private string _aciklama = "DesaDoor ile lüks ve modern kapak sistemleri.";
    private string _anahtarKelimeler = "kapak, membran, lake, akrilik, desadoor";

    private string _telefon = "+90 555 123 45 67";
    private string _eposta = "info@desadoor.com.tr";
    private string _adres = "Organize Sanayi Bölgesi, 1. Cadde, No: 12, Karabük";
    private string _whatsapp = "+90 555 123 45 67";

    private string _instagram = "https://instagram.com/desadoor.com.tr";
    private string _facebook = "https://facebook.com/desadoor";
    private string _youtube = "";

    protected override async Task OnInitializedAsync()
    {
        await AyarlariYukle();
        _yukleniyorSayfa = false;
    }

    private async Task AyarlariYukle()
    {
        try
        {
            var sozluk = await Api.GetAsync<Dictionary<string, string>>("api/desadoor/sayfa-icerigi/ayarlar");
            if (sozluk != null)
            {
                _logoUrl = sozluk.GetValueOrDefault("LogoUrl", _logoUrl);
                _faviconUrl = sozluk.GetValueOrDefault("FaviconUrl", _faviconUrl);
                _siteBasligi = sozluk.GetValueOrDefault("SiteBasligi", _siteBasligi);
                _aciklama = sozluk.GetValueOrDefault("Aciklama", _aciklama);
                _anahtarKelimeler = sozluk.GetValueOrDefault("AnahtarKelimeler", _anahtarKelimeler);
                _telefon = sozluk.GetValueOrDefault("Telefon1", _telefon);
                _eposta = sozluk.GetValueOrDefault("Eposta", _eposta);
                _adres = sozluk.GetValueOrDefault("Adres", _adres);
                _whatsapp = sozluk.GetValueOrDefault("Whatsapp", _whatsapp);
                _instagram = sozluk.GetValueOrDefault("Instagram", _instagram);
                _facebook = sozluk.GetValueOrDefault("Facebook", _facebook);
                _youtube = sozluk.GetValueOrDefault("Youtube", _youtube);
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
                ayarlar["Facebook"] = _facebook;
                ayarlar["Youtube"] = _youtube;
            }

            foreach (var (anahtar, deger) in ayarlar)
            {
                await Api.PutAsync<object>("api/desadoor/sayfa-icerigi", new
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

            var yanit = await Api.PostMultipartAsync<Desadoor.Ortak.Modeller.Medya.Medya>("api/medya/yukle", icerik);
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

            var yanit = await Api.PostMultipartAsync<Desadoor.Ortak.Modeller.Medya.Medya>("api/medya/yukle", icerik);
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
}
