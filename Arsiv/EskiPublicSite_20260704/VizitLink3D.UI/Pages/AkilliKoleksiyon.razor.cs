using Microsoft.AspNetCore.Components;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Bilesenler.Stitch;

namespace VizitLink3D.UI.Pages;

public partial class AkilliKoleksiyon
{
    private bool _yukleniyor = true;
    private List<UrunOzetDto> _urunler = [];
    private List<UrunOzetDto> _tumUrunler = [];
    private string? _seciliAile;
    private string? _seciliKategori;
    private List<string> _urunAileleri = ["Hermes", "Bottega", "Giorgio", "Diago", "Capelli", "Hera"];
    private List<string> _kategoriler = ["Premium", "Trend", "Exclusive"];

    protected override async Task OnInitializedAsync()
    {
        await UrunleriYukle();
    }

    private async Task UrunleriYukle()
    {
        try
        {
            var liste = await api.GetAsync<List<UrunOzetDto>>("api/urunler?dil=" + dil.AktifDil);
            if (liste != null)
            {
                _tumUrunler = liste;
                Filtrele();
            }
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private void Filtrele()
    {
        var sonuc = _tumUrunler.AsEnumerable();

        if (!string.IsNullOrEmpty(_seciliAile))
            sonuc = sonuc.Where(u => u.UrunAilesiAdi != null && u.UrunAilesiAdi.Contains(_seciliAile, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(_seciliKategori))
            sonuc = sonuc.Where(u => u.UrunKategoriAdi != null && u.UrunKategoriAdi.Contains(_seciliKategori, StringComparison.OrdinalIgnoreCase));

        _urunler = sonuc.Take(12).ToList();
    }

    private async Task AileDegisti(string? deger)
    {
        _seciliAile = deger;
        Filtrele();
        await InvokeAsync(StateHasChanged);
    }

    private async Task KategoriDegisti(string? deger)
    {
        _seciliKategori = deger;
        Filtrele();
        await InvokeAsync(StateHasChanged);
    }

    private void UrunDetayaGit(UrunOzetDto urun)
    {
        nav.NavigateTo("/urun/" + urun.Slug);
    }
}
