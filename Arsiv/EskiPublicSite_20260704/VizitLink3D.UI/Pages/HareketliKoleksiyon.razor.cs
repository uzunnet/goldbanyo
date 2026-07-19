using Microsoft.AspNetCore.Components;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Bilesenler.Stitch;

namespace VizitLink3D.UI.Pages;

public partial class HareketliKoleksiyon
{
    private bool _yukleniyor = true;
    private List<UrunOzetDto> _urunler = [];

    protected override async Task OnInitializedAsync()
    {
        await UrunleriYukle();
    }

    private async Task UrunleriYukle()
    {
        try
        {
            var liste = await api.GetAsync<List<UrunOzetDto>>("api/urunler?dil=" + dil.AktifDil);
            if (liste != null && liste.Count > 0)
            {
                _urunler = liste.Take(8).ToList();
            }
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private void UrunDetayaGit(UrunOzetDto urun)
    {
        nav.NavigateTo("/urun/" + urun.Slug);
    }
}
