using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Pages.Admin;

[Microsoft.AspNetCore.Authorization.Authorize]
public partial class Kategoriler : ComponentBase
{
    [Inject] private KategoriYonetimServisi KategoriServisi { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    // Ağaç veri
    private IReadOnlyCollection<TreeItemData<KategoriAgacDto>> _kategoriler = [];
    private HashSet<KategoriAgacDto> _genisletilenDugumler = [];

    // Yüklenme / hata durumları
    private bool _yukleniyor = true;
    private bool _listeHatasi;

    // Dialog durumları
    private bool _dialogAcik;
    private bool _duzenlemeModu;
    private string _dialogBaslik = "";
    private bool _kaydediliyor;

    // Form alanları
    private string _ad = "";
    private string? _aciklama;
    private int? _ustKategoriId;
    private int _sira;
    private bool _aktifMi = true;
    private int? _duzenlenenId;

    // Üst kategori seçimi için düz liste
    private List<KategoriAgacDto> _ustKategoriSecenekleri = [];

    // Silme dialogu
    private bool _silmeDialogAcik;
    private KategoriAgacDto? _silinecekKategori;
    private bool _siliniyor;
    private string _silmeHataMesaji = "";

    protected override async Task OnInitializedAsync()
    {
        await KategorileriYukleAsync();
    }

    private async Task KategorileriYukleAsync()
    {
        _yukleniyor = true;
        _listeHatasi = false;
        StateHasChanged();

        var cevap = await KategoriServisi.AgacGetirAsync();
        if (cevap.BasariliMi && cevap.Veri is not null)
        {
            _kategoriler = AgacVerisineDonustur(cevap.Veri).ToList();
        }
        else
        {
            _listeHatasi = true;
            _kategoriler = [];
        }

        _yukleniyor = false;
    }

    private async Task UstKategoriSecenekleriniYukleAsync(int? haricId = null)
    {
        var cevap = await KategoriServisi.ListeGetirAsync();
        if (cevap.BasariliMi && cevap.Veri is not null)
        {
            _ustKategoriSecenekleri = cevap.Veri
                .Where(k => k.Id != haricId)
                .OrderBy(k => k.Ad)
                .ToList();
        }
        else
        {
            _ustKategoriSecenekleri = [];
        }
    }

    // ──── EKLEME ────

    private async Task EkleDialogAc()
    {
        _duzenlemeModu = false;
        _duzenlenenId = null;
        _dialogBaslik = Dil.T("kategoriler.ekleBaslik", "Yeni Kategori Ekle");
        _ad = "";
        _aciklama = null;
        _ustKategoriId = null;
        _sira = 0;
        _aktifMi = true;

        await UstKategoriSecenekleriniYukleAsync();
        _dialogAcik = true;
    }

    // ──── DÜZENLEME ────

    private async Task DuzenleDialogAc(KategoriAgacDto kategori)
    {
        _duzenlemeModu = true;
        _duzenlenenId = kategori.Id;
        _dialogBaslik = Dil.T("kategoriler.duzenleBaslik", "Kategori Düzenle");
        _ad = kategori.Ad;
        _aciklama = kategori.Aciklama;
        _ustKategoriId = kategori.UstKategoriId;
        _sira = kategori.Sira;
        _aktifMi = kategori.AktifMi;

        await UstKategoriSecenekleriniYukleAsync(kategori.Id);
        _dialogAcik = true;
    }

    private void DialogKapat()
    {
        _dialogAcik = false;
    }

    // ──── KAYDET ────

    private async Task KaydetAsync()
    {
        if (string.IsNullOrWhiteSpace(_ad))
        {
            Snackbar.Add(Dil.T("kategoriler.adZorunlu", "Kategori adı zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        StateHasChanged();

        try
        {
            if (_duzenlemeModu && _duzenlenenId.HasValue)
            {
                var dto = new KategoriGuncelleIstekDto
                {
                    Ad = _ad.Trim(),
                    Aciklama = string.IsNullOrWhiteSpace(_aciklama) ? null : _aciklama.Trim(),
                    UstKategoriId = _ustKategoriId,
                    Sira = _sira,
                    AktifMi = _aktifMi
                };

                var cevap = await KategoriServisi.GuncelleAsync(_duzenlenenId.Value, dto);
                if (cevap.BasariliMi)
                {
                    Snackbar.Add(Dil.T("kategoriler.guncellendi", "Kategori güncellendi."), Severity.Success);
                    _dialogAcik = false;
                    await KategorileriYukleAsync();
                }
                else
                {
                    Snackbar.Add(cevap.Mesaj ?? Dil.T("kategoriler.guncellemeHata", "Güncelleme başarısız."), Severity.Error);
                }
            }
            else
            {
                var dto = new KategoriEkleIstekDto
                {
                    Ad = _ad.Trim(),
                    Aciklama = string.IsNullOrWhiteSpace(_aciklama) ? null : _aciklama.Trim(),
                    UstKategoriId = _ustKategoriId,
                    Sira = _sira
                };

                var cevap = await KategoriServisi.EkleAsync(dto);
                if (cevap.BasariliMi)
                {
                    Snackbar.Add(Dil.T("kategoriler.eklendi", "Kategori eklendi."), Severity.Success);
                    _dialogAcik = false;
                    await KategorileriYukleAsync();
                }
                else
                {
                    Snackbar.Add(cevap.Mesaj ?? Dil.T("kategoriler.eklemeHata", "Ekleme başarısız."), Severity.Error);
                }
            }
        }
        catch
        {
            Snackbar.Add(Dil.T("kategoriler.islemHata", "İşlem sırasında hata oluştu."), Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
            StateHasChanged();
        }
    }

    // ──── SİLME ────

    private void SilDialogAc(KategoriAgacDto kategori)
    {
        _silinecekKategori = kategori;
        _silmeHataMesaji = "";
        _silmeDialogAcik = true;
    }

    private void SilDialogKapat()
    {
        _silmeDialogAcik = false;
        _silinecekKategori = null;
        _silmeHataMesaji = "";
    }

    private async Task SilAsync()
    {
        if (_silinecekKategori is null) return;

        _siliniyor = true;
        _silmeHataMesaji = "";
        StateHasChanged();

        try
        {
            var cevap = await KategoriServisi.SilAsync(_silinecekKategori.Id);
            if (cevap.BasariliMi)
            {
                Snackbar.Add(Dil.T("kategoriler.silindi", "Kategori silindi."), Severity.Success);
                _silmeDialogAcik = false;
                _silinecekKategori = null;
                await KategorileriYukleAsync();
            }
            else
            {
                // Alt kategori engeli veya başka bir API hatası
                _silmeHataMesaji = cevap.Mesaj ?? Dil.T("kategoriler.silmeHata", "Silme başarısız.");
            }
        }
        catch
        {
            _silmeHataMesaji = Dil.T("kategoriler.silmeSunucuHata", "Silme sırasında hata oluştu.");
        }
        finally
        {
            _siliniyor = false;
            StateHasChanged();
        }
    }

    // ──── YARDIMCI ────

    private string AltKategoriSayisiMetin(KategoriAgacDto kat)
    {
        var sayi = kat.AltKategoriler?.Count ?? 0;
        return sayi > 0 ? $"({sayi} alt)" : "";
    }

    private static IEnumerable<TreeItemData<KategoriAgacDto>> AgacVerisineDonustur(List<KategoriAgacDto> liste)
    {
        return liste.Select(k =>
        {
            var item = new TreeItemData<KategoriAgacDto> { Value = k };
            if (k.AltKategoriler is { Count: > 0 })
                item.Children = AgacVerisineDonustur(k.AltKategoriler).ToList();
            return item;
        });
    }
}
