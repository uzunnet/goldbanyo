using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Pages.Admin;

[Microsoft.AspNetCore.Authorization.Authorize]
public partial class Modeller : ComponentBase
{
    [Inject] private ModellerYonetimServisi ModellerServisi { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthState { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<ModelListeOgesiDto> _modeller = [];
    private bool _yukleniyor = true;
    private bool _listeHatasi;
    private bool _yapilandirmaHatasi;

    // BFF durumu
    private bool _bffAnahtarTanimli;

    // Yukleme formu
    private string _modelAdi = "";
    private string _modelAciklama = "";
    private IBrowserFile? _seciliDosya;
    private bool _yukleniyorDosya;
    private string? _yuklemeHataMesaji;
    private string? _yuklemeBasariliMesaji;

    /// <summary>
    /// Yukleme butonunun aktif olup olmayacagini belirler.
    /// Ad dolu, dosya secili ve yukleme devam etmiyor olmali.
    /// </summary>
    private bool DosyaYuklemeyeHazirMi =>
        !string.IsNullOrWhiteSpace(_modelAdi) && _seciliDosya is not null && !_yukleniyorDosya;

    protected override async Task OnInitializedAsync()
    {
        // BFF anahtar durumunu kontrol et
        _bffAnahtarTanimli = ModellerServisi.BffAnahtarTanimliMi;

        if (!_bffAnahtarTanimli)
        {
            _yapilandirmaHatasi = true;
            _yukleniyor = false;
            return;
        }

        await ModelListesiniYukleAsync();
    }

    /// <summary>
    /// Model listesini API'den ceker.
    /// Hata durumunda generic mesaj gosterir, API detayi sizdirmaz.
    /// </summary>
    private async Task ModelListesiniYukleAsync()
    {
        _yukleniyor = true;
        _listeHatasi = false;
        StateHasChanged();

        try
        {
            var liste = await ModellerServisi.ListeleAsync();

            if (liste is null)
            {
                _listeHatasi = true;
                _modeller = [];
            }
            else
            {
                _modeller = liste;
            }
        }
        catch
        {
            _listeHatasi = true;
            _modeller = [];
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    /// <summary>
    /// InputFile'dan dosya secimi yapildiginda cagrilir.
    /// </summary>
    private void DosyaSecildi(InputFileChangeEventArgs e)
    {
        _yuklemeHataMesaji = null;
        _yuklemeBasariliMesaji = null;

        var dosya = e.GetMultipleFiles().FirstOrDefault();

        if (dosya is null)
        {
            _seciliDosya = null;
            return;
        }

        // Uzanti kontrolu (client-side yardimci, asil kontrol API'de)
        if (!dosya.Name.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
        {
            _yuklemeHataMesaji = Dil.T("modeller.gecersizUzanti", "Sadece .glb uzantili dosyalar kabul edilir.");
            _seciliDosya = null;
            return;
        }

        _seciliDosya = dosya;
    }

    /// <summary>
    /// Secili dosyayi temizler.
    /// </summary>
    private void DosyaTemizle(MudChip<string> chip)
    {
        _seciliDosya = null;
        _yuklemeHataMesaji = null;
        _yuklemeBasariliMesaji = null;
    }

    /// <summary>
    /// Dosyayi BFF uzerinden API'ye gonderir.
    /// Basarili ise listeyi yeniler.
    /// </summary>
    private async Task DosyaYukle()
    {
        if (!DosyaYuklemeyeHazirMi || _seciliDosya is null)
            return;

        _yukleniyorDosya = true;
        _yuklemeHataMesaji = null;
        _yuklemeBasariliMesaji = null;
        StateHasChanged();

        try
        {
            await using var dosyaAkisi = _seciliDosya.OpenReadStream(maxAllowedSize: 100_000_000);

            var sonuc = await ModellerServisi.YukleAsync(
                _modelAdi.Trim(),
                string.IsNullOrWhiteSpace(_modelAciklama) ? null : _modelAciklama.Trim(),
                dosyaAkisi,
                _seciliDosya.Name,
                _seciliDosya.ContentType,
                CancellationToken.None);

            if (sonuc is not null)
            {
                _yuklemeBasariliMesaji = Dil.T("modeller.yuklemeBasarili", "Model basariyla yuklendi.");
                _modelAdi = "";
                _modelAciklama = "";
                _seciliDosya = null;

                Snackbar.Add(
                    Dil.T("modeller.yuklemeBasariliSnack", "Model basariyla yuklendi."),
                    Severity.Success);

                // Listeyi tazele
                await ModelListesiniYukleAsync();
            }
            else
            {
                _yuklemeHataMesaji = Dil.T("modeller.yuklemeHata", "Model yuklenemedi. Dosya formatini ve boyutunu kontrol edin.");
            }
        }
        catch
        {
            _yuklemeHataMesaji = Dil.T("modeller.yuklemeSunucuHata", "Yukleme sirasinda bir hata olustu. Lutfen tekrar deneyin.");
        }
        finally
        {
            _yukleniyorDosya = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Dosya boyutunu insan-okunur formatta dondurur.
    /// </summary>
    private static string DosyaBoyutuFormatla(long bayt)
    {
        if (bayt < 1024) return $"{bayt} B";
        if (bayt < 1024 * 1024) return $"{bayt / 1024.0:F1} KB";
        if (bayt < 1024 * 1024 * 1024) return $"{bayt / (1024.0 * 1024.0):F1} MB";
        return $"{bayt / (1024.0 * 1024.0 * 1024.0):F1} GB";
    }
}
