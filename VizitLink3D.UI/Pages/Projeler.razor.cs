using Microsoft.AspNetCore.Components;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class Projeler : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private NavigationManager Navigasyon { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private List<ProjeKategorisi> _kategoriler = [];
    private List<Proje> _projeler = [];
    private int? _seciliKategoriId;
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Kategorileri yükle; hata olursa fallback 6'lı liste
            try
            {
                _kategoriler = await Api.GetAsync<List<ProjeKategorisi>>("api/projeler/kategoriler") ?? [];
            }
            catch
            {
                _kategoriler =
                [
                    new() { Id = 1, Ad = "Mutfak", Slug = "mutfak", SiraNo = 1, AktifMi = true },
                    new() { Id = 2, Ad = "TV ünitesi", Slug = "tv-unitesi", SiraNo = 2, AktifMi = true },
                    new() { Id = 3, Ad = "Kapı", Slug = "kapi", SiraNo = 3, AktifMi = true },
                    new() { Id = 4, Ad = "Giyinme odası", Slug = "giyinme-odasi", SiraNo = 4, AktifMi = true },
                    new() { Id = 5, Ad = "Çamaşır odası", Slug = "camasir-odasi", SiraNo = 5, AktifMi = true },
                    new() { Id = 6, Ad = "Kahve köşesi", Slug = "kahve-kosesi", SiraNo = 6, AktifMi = true }
                ];
            }

            // Projeleri yükle
            try
            {
                _projeler = await Api.GetAsync<List<Proje>>("api/projeler") ?? [];
            }
            catch
            {
                _projeler = [];
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Projeler] {ex.Message}");
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private void KategoriSec(int? kategoriId)
    {
        _seciliKategoriId = _seciliKategoriId == kategoriId ? null : kategoriId;
    }

    /// <summary>
    /// Belirli bir kategorideki aktif projeleri döndürür.
    /// </summary>
    private List<Proje> KategoriProjeleri(int kategoriId)
    {
        return _projeler
            .Where(p => p.AktifMi && p.KategoriId == kategoriId)
            .OrderByDescending(p => p.OneCikanMi)
            .ThenBy(p => p.SiraNo)
            .ToList();
    }

    /// <summary>
    /// Ekranda gösterilecek kategorileri döndürür.
    /// Seçili kategori varsa yalnızca o; yoksa tüm kategoriler.
    /// </summary>
    private List<ProjeKategorisi> GorunenKategoriler()
    {
        if (_seciliKategoriId.HasValue)
        {
            var secili = _kategoriler.FirstOrDefault(k => k.Id == _seciliKategoriId.Value);
            return secili is not null ? [secili] : [];
        }

        return _kategoriler;
    }

    private string? KapakResmiBul(Proje proje)
    {
        var kapakYolu = ResimYoluDuzenle(proje.KapakResim);
        if (!string.IsNullOrWhiteSpace(kapakYolu))
            return kapakYolu;

        var ilkResim = proje.Resimler?.OrderBy(r => r.Sira).FirstOrDefault();
        return ResimYoluDuzenle(ilkResim?.Url);
    }

    private static string? ResimYoluDuzenle(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
            return null;

        var temizYol = yol.Trim();

        if (temizYol.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            temizYol.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            temizYol.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            temizYol.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            return temizYol;

        if (temizYol.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            return null;

        if (temizYol.StartsWith('/'))
            return temizYol;

        return "/" + temizYol.TrimStart('/');
    }

    private string TarihMetni(Proje proje)
    {
        if (proje.ProjeTarihi.HasValue)
            return proje.ProjeTarihi.Value.ToString("yyyy");

        return proje.OlusturulmaTarihi.ToString("yyyy");
    }

    private string KategoriSinifi(int? kategoriId) =>
        _seciliKategoriId == kategoriId ? "gb-chip gb-chip--active" : "gb-chip";
}
