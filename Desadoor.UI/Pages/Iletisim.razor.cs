using Microsoft.AspNetCore.Components;
using MudBlazor;
using Desadoor.UI.Models;

namespace Desadoor.UI.Pages;

/// <summary>
/// Iletisim — DesaDoor iletişim sayfasının kod-arkası sınıfıdır.
/// Sayfa içeriği (adres, telefon, çalışma saatleri) API'den dinamik olarak yüklenir.
/// Kullanıcı tarafından doldurulan form POST isteğiyle API'ye gönderilir.
/// Başarılı gönderimde snackbar ile kullanıcı bilgilendirilir.
/// Dil değişiminde içeriği otomatik olarak yeniden yükler (DilServisi.DilDegisti event'i).
/// </summary>
public partial class Iletisim : ComponentBase, IAsyncDisposable
{
    // ─── Sayfa Meta Verileri (API'den dinamik) ────────────────────────────────
    private string _sayfaBasligi = "İletişim | DesaDoor";
    private string _adres = "Çalı Mah. Ömer Biltekin Bulv. No:3/1A, Nilüfer / BURSA";
    private string _telefon1 = "+90 224 482 24 00";
    private string _telefon2 = "+90 533 597 32 14";
    private string _email = "info@desadoor.com.tr";
    private string _calismaGunleri = "Pazartesi – Cumartesi";
    private string _calismaSaatleri = "09:00 – 18:00";
    private string _haritaUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3044.7!2d28.8!3d40.2!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2zDesaDoor!5e0!3m2!1str!2str!4v1";

    // ─── Form Alanları ────────────────────────────────────────────────────────
    private string _formAd = string.Empty;
    private string _formEmail = string.Empty;
    private string _formTelefon = string.Empty;
    private string _formKonu = string.Empty;
    private string _formMesaj = string.Empty;
    private string _formModelKodu = string.Empty;

    private bool _formGonderiliyor = false;
    private bool _formBasarili = false;

    [SupplyParameterFromQuery(Name = "model")]
    public string? ModelKodu { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(ModelKodu))
            _formModelKodu = ModelKodu;

        await IletisimBilgileriniYukleAsync();
        dil.DilDegisti += DilDegistinde;
    }

    private async void DilDegistinde()
    {
        await IletisimBilgileriniYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// API'den iletişim bilgilerini (adres, telefon, e-posta, çalışma saatleri) çeker.
    /// API cevap vermezse varsayılan değerler korunur.
    /// </summary>
    private async Task IletisimBilgileriniYukleAsync()
    {
        var sozluk = await api.GetAsync<Dictionary<string, string>>($"api/desadoor/sayfa-icerigi/iletisim?dil={dil.AktifDil}");
        if (sozluk != null)
        {
            _sayfaBasligi = sozluk.GetValueOrDefault("SayfaBasligi", _sayfaBasligi);
            _adres = sozluk.GetValueOrDefault("Adres", _adres);
            _telefon1 = sozluk.GetValueOrDefault("Telefon1", _telefon1);
            _telefon2 = sozluk.GetValueOrDefault("Telefon2", _telefon2);
            _email = sozluk.GetValueOrDefault("Email", _email);
            _calismaGunleri = sozluk.GetValueOrDefault("CalismaGunleri", _calismaGunleri);
            _calismaSaatleri = sozluk.GetValueOrDefault("CalismaSaatleri", _calismaSaatleri);
            _haritaUrl = sozluk.GetValueOrDefault("HaritaUrl", _haritaUrl);
        }
    }

    public async ValueTask DisposeAsync()
    {
        dil.DilDegisti -= DilDegistinde;
        await ValueTask.CompletedTask;
    }

    /// <summary>
    /// Kullanıcının doldurduğu iletişim formunu API'ye POST eder.
    /// Gönderim sırasında buton devre dışı bırakılır; başarı/hata snackbar ile gösterilir.
    /// </summary>
    private async Task FormuGonder()
    {
        if (string.IsNullOrWhiteSpace(_formAd) || string.IsNullOrWhiteSpace(_formEmail) || string.IsNullOrWhiteSpace(_formMesaj))
        {
            snackbar.Add("Lütfen zorunlu alanları doldurunuz.", Severity.Warning);
            return;
        }

        _formGonderiliyor = true;
        var veri = new
        {
            AdSoyad = _formAd,
            Email = _formEmail,
            Telefon = _formTelefon,
            Konu = string.IsNullOrWhiteSpace(_formModelKodu) ? _formKonu : $"{_formKonu} (Model: {_formModelKodu})",
            Mesaj = _formMesaj
        };

        var yanit = await api.PostAsync<object>("api/desadoor/iletisim", veri);
        _formGonderiliyor = false;

        if (yanit?.BasariliMi == true)
        {
            _formBasarili = true;
            snackbar.Add("Mesajınız alındı! En kısa sürede size döneceğiz.", Severity.Success);
            _formAd = _formEmail = _formTelefon = _formKonu = _formMesaj = _formModelKodu = string.Empty;
        }
        else
        {
            snackbar.Add(yanit?.Mesaj ?? "Gönderim sırasında bir hata oluştu. Lütfen tekrar deneyin.", Severity.Error);
        }
    }
}
