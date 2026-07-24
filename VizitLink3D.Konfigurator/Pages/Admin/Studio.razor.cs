using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Pages.Admin;

[Microsoft.AspNetCore.Authorization.Authorize]
public partial class Studio : ComponentBase, IAsyncDisposable
{
    [Inject] private ModellerYonetimServisi ModellerServisi { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;
    [Inject] private IUcBoyutGoruntuleyiciServisi Goruntuleyici { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<ModelYonetimListeOgesiDto> _modeller = [];
    private ModelYonetimListeOgesiDto? _seciliModel;
    private bool _yukleniyor = true;
    private bool _listeHatasi;
    private bool _yapilandirmaHatasi;
    private bool _toggleCalisiyor;

    // Preview durumu
    private string? _previewModelUrl;
    private bool _previewHazir;
    private bool _previewHata;
    private bool _goruntuleyiciBaslatildi;

    // P06-C: Parca paneli durumu
    private List<ParcaYonetimDto> _parcalar = [];
    private bool _parcaYukleniyor;
    private bool _parcaHatasi;
    private bool _senkronizeEdiliyor;
    private ParcaSenkronizeSonucDto? _senkronizeSonuc;
    private int? _seciliParcaId;
    private bool _parcaKaydediliyor;

    // Secili parca duzenleme formu
    private string _duzenlemeGorunenAd = "";
    private string _duzenlemeParcaTuru = "Diger";
    private bool _duzenlemeRenkDegistirilebilirMi;
    private bool _duzenlemeGorunurMu = true;
    private string _duzenlemeVarsayilanRenk = "#C8952A";

    private DotNetObjectReference<Studio>? _dotNetRef;

    protected override async Task OnInitializedAsync()
    {
        if (!ModellerServisi.BffAnahtarTanimliMi)
        {
            _yapilandirmaHatasi = true;
            _yukleniyor = false;
            return;
        }

        await ModelListesiniYukleAsync();
    }

    /// <summary>
    /// Model listesini BFF uzerinden admin API'den ceker.
    /// </summary>
    private async Task ModelListesiniYukleAsync()
    {
        _yukleniyor = true;
        _listeHatasi = false;
        StateHasChanged();

        try
        {
            var liste = await ModellerServisi.YonetimListeleAsync();

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
    /// Kullanicinin listeden bir model secmesi.
    /// Secilen modelin dosya URL'i guvenli ise 3D preview baslatilir.
    /// P06-C: Model secildiginde parca listesi de BFF uzerinden yuklenir.
    /// </summary>
    private async Task ModelSec(int id)
    {
        _seciliModel = _modeller.FirstOrDefault(m => m.Id == id);

        if (_seciliModel is null)
            return;

        // Onceki goruntuleyiciyi temizle
        await GoruntuleyiciyiTemizleAsync();

        _previewModelUrl = null;
        _previewHazir = false;
        _previewHata = false;

        // P06-C: Parca panelini sifirla
        _parcalar = [];
        _parcaHatasi = false;
        _senkronizeSonuc = null;
        _seciliParcaId = null;

        // Guvenli dosya URL kontrolu: sadece same-origin BFF proxy
        var modelUrl = $"/api/public/modeller/{_seciliModel.Slug}/dosya";

        if (!string.IsNullOrWhiteSpace(modelUrl) && modelUrl.StartsWith("/"))
        {
            _previewModelUrl = modelUrl;
            _previewHazir = true;

            // UI guncellendikten sonra goruntuleyiciyi baslat
            StateHasChanged();
            await GecikmeliGoruntuleyiciBaslatAsync();

            // P06-C: Model yuklendikten sonra parca listesini cek
            // (otomatik senkronizasyon YAPILMAZ — explicit admin action gerekir)
            await ParcalariYukleAsync();
        }
        else
        {
            _previewHazir = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Kisa gecikmeyle 3D goruntuleyiciyi baslatir ve modeli yukler.
    /// DOM'un guncellenmesi icin kucuk bir gecikme gereklidir.
    /// </summary>
    private async Task GecikmeliGoruntuleyiciBaslatAsync()
    {
        try
        {
            await Task.Delay(100);

            _dotNetRef = DotNetObjectReference.Create(this);

            await Goruntuleyici.BaslatAsync(_dotNetRef, "studio-3d-preview");

            if (!string.IsNullOrWhiteSpace(_previewModelUrl))
            {
                await Goruntuleyici.ModelYukleAsync(_previewModelUrl);
                _goruntuleyiciBaslatildi = true;
            }
        }
        catch
        {
            _previewHata = true;
            _previewHazir = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// 3D goruntuleyiciyi guvenli sekilde temizler.
    /// </summary>
    private async Task GoruntuleyiciyiTemizleAsync()
    {
        try
        {
            if (_goruntuleyiciBaslatildi)
            {
                await Goruntuleyici.YokEtAsync();
                _goruntuleyiciBaslatildi = false;
            }
        }
        catch
        {
            // Temizlik sirasinda hata sessizce yutulur
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    /// <summary>
    /// Secili modelin yayin durumunu (AktifMi) BFF uzerinden degistirir.
    /// Basarili ise listedeki durum guncellenir ve bildirim gosterilir.
    /// </summary>
    private async Task YayinDurumuDegistir()
    {
        if (_seciliModel is null || _toggleCalisiyor)
            return;

        _toggleCalisiyor = true;
        var yeniDurum = !_seciliModel.AktifMi;
        StateHasChanged();

        try
        {
            var sonuc = await ModellerServisi.YayinDurumuGuncelleAsync(
                _seciliModel.Id, yeniDurum);

            if (sonuc is not null)
            {
                // Listedeki modeli guncelle
                var index = _modeller.FindIndex(m => m.Id == _seciliModel.Id);
                if (index >= 0)
                {
                    _modeller[index] = sonuc;
                }

                _seciliModel = sonuc;

                var mesaj = yeniDurum
                    ? Dil.T("studio.aktifYapBasarili", "Model aktif yayina alindi.")
                    : Dil.T("studio.pasifYapBasarili", "Model pasif yapildi.");

                Snackbar.Add(mesaj, Severity.Success);
            }
            else
            {
                Snackbar.Add(
                    Dil.T("studio.durumHata", "Yayin durumu guncellenemedi. Lutfen tekrar deneyin."),
                    Severity.Error);
            }
        }
        catch
        {
            Snackbar.Add(
                Dil.T("studio.durumHata", "Yayin durumu guncellenemedi. Lutfen tekrar deneyin."),
                Severity.Error);
        }
        finally
        {
            _toggleCalisiyor = false;
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

    // ═══════════════════════════════════════════════════════════════
    // P06-C: Parca paneli metodlari
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Secili modelin admin parca listesini BFF uzerinden ceker.
    /// Model degistiginde veya senkronizasyon sonrasi cagrilir.
    /// </summary>
    private async Task ParcalariYukleAsync()
    {
        if (_seciliModel is null)
            return;

        _parcaYukleniyor = true;
        _parcaHatasi = false;
        _seciliParcaId = null;
        StateHasChanged();

        try
        {
            var liste = await ModellerServisi.ParcalariGetirAsync(_seciliModel.Id);

            if (liste is null)
            {
                _parcaHatasi = true;
                _parcalar = [];
            }
            else
            {
                _parcalar = liste;
            }
        }
        catch
        {
            _parcaHatasi = true;
            _parcalar = [];
        }
        finally
        {
            _parcaYukleniyor = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// "Parcalari Tara ve Senkronize Et" butonu.
    /// 1) Goruntuleyiciden mesh isimlerini alir
    /// 2) BFF uzerinden API'ye senkronize eder
    /// 3) Parca listesini yeniler
    /// Otomatik DB mutasyonu YAPILMAZ — sadece explicit admin action ile.
    /// </summary>
    private async Task ParcalariSenkronizeEtAsync()
    {
        if (_seciliModel is null || !_goruntuleyiciBaslatildi || _senkronizeEdiliyor)
            return;

        _senkronizeEdiliyor = true;
        _senkronizeSonuc = null;
        StateHasChanged();

        try
        {
            // Adim 1: Goruntuleyicideki mesh isimlerini al
            string[] meshAdlari;
            try
            {
                meshAdlari = await Goruntuleyici.MeshleriGetirAsync();
            }
            catch
            {
                Snackbar.Add(
                    Dil.T("studio.senkronizeMeshHata", "Mesh listesi alinamadi. Lutfen modelin yuklendiginden emin olun."),
                    Severity.Error);
                return;
            }

            if (meshAdlari.Length == 0)
            {
                Snackbar.Add(
                    Dil.T("studio.senkronizeMeshBos", "Modelde hic mesh bulunamadi."),
                    Severity.Warning);
                return;
            }

            // Adim 2: BFF uzerinden API'ye senkronize et
            var sonuc = await ModellerServisi.ParcalariSenkronizeEtAsync(
                _seciliModel.Id, meshAdlari);

            if (sonuc is null)
            {
                Snackbar.Add(
                    Dil.T("studio.senkronizeHata", "Parca senkronizasyonu basarisiz oldu. Lutfen tekrar deneyin."),
                    Severity.Error);
                return;
            }

            _senkronizeSonuc = sonuc;

            // Adim 3: Parca listesini yeniden yukle
            await ParcalariYukleAsync();

            var mesaj = Dil.T("studio.senkronizeBasarili",
                "Senkronizasyon tamamlandı. Eklenen: {0}, Geri yüklenen: {1}, Kaldırılan: {2}");
            mesaj = mesaj.Replace("{0}", sonuc.Eklenen.ToString())
                         .Replace("{1}", sonuc.GeriYuklenen.ToString())
                         .Replace("{2}", sonuc.YumusakSilinen.ToString());

            Snackbar.Add(mesaj, Severity.Success);
        }
        catch
        {
            Snackbar.Add(
                Dil.T("studio.senkronizeSunucuHata", "Senkronizasyon sirasinda bir hata olustu."),
                Severity.Error);
        }
        finally
        {
            _senkronizeEdiliyor = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Parca listesinden bir parcaya tiklandiginda:
    /// 1) 3D goruntuleyicide o mesh'i highlight yapar
    /// 2) Duzenleme formunu o parca icin doldurur
    /// </summary>
    private async Task ParcaSecAsync(ParcaYonetimDto parca)
    {
        if (_goruntuleyiciBaslatildi)
        {
            await Goruntuleyici.MeshSecimiTemizleAsync();

            if (!string.IsNullOrWhiteSpace(parca.MeshAdi))
            {
                await Goruntuleyici.MeshSecAsync(parca.MeshAdi);
            }
        }

        _seciliParcaId = parca.Id;
        _duzenlemeGorunenAd = parca.GorunenAd;
        _duzenlemeParcaTuru = parca.ParcaTuru;
        _duzenlemeRenkDegistirilebilirMi = parca.RenkDegistirilebilirMi;
        _duzenlemeGorunurMu = parca.GorunurMu;
        _duzenlemeVarsayilanRenk = parca.VarsayilanRenk ?? "#C8952A";

        StateHasChanged();
    }

    /// <summary>
    /// Secili parcanin metadata'sini BFF uzerinden kaydeder.
    /// Sadece gonderilen alanlar guncellenir.
    /// </summary>
    private async Task ParcaMetadataKaydetAsync()
    {
        if (_seciliModel is null || _seciliParcaId is null || _parcaKaydediliyor)
            return;

        _parcaKaydediliyor = true;
        StateHasChanged();

        try
        {
            var dto = new ParcaMetadataGuncelleIstekDto
            {
                GorunenAd = _duzenlemeGorunenAd?.Trim(),
                ParcaTuru = _duzenlemeParcaTuru,
                RenkDegistirilebilirMi = _duzenlemeRenkDegistirilebilirMi,
                GorunurMu = _duzenlemeGorunurMu,
                VarsayilanRenk = string.IsNullOrWhiteSpace(_duzenlemeVarsayilanRenk)
                    ? null : _duzenlemeVarsayilanRenk.Trim()
            };

            var sonuc = await ModellerServisi.ParcaMetadataGuncelleAsync(
                _seciliModel.Id, _seciliParcaId.Value, dto);

            if (sonuc is null)
            {
                Snackbar.Add(
                    Dil.T("studio.parcaKaydetHata", "Parça güncellenemedi. Lütfen tekrar deneyin."),
                    Severity.Error);
                return;
            }

            // Listeyi guncelle
            var index = _parcalar.FindIndex(p => p.Id == sonuc.Id);
            if (index >= 0)
            {
                _parcalar[index] = sonuc;
            }

            Snackbar.Add(
                Dil.T("studio.parcaKaydetBasarili", "Parça başarıyla güncellendi."),
                Severity.Success);
        }
        catch
        {
            Snackbar.Add(
                Dil.T("studio.parcaKaydetSunucuHata", "Kaydetme sırasında bir hata oluştu."),
                Severity.Error);
        }
        finally
        {
            _parcaKaydediliyor = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Component disposed oldugunda 3D goruntuleyiciyi temizler.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await GoruntuleyiciyiTemizleAsync();
        GC.SuppressFinalize(this);
    }
}
