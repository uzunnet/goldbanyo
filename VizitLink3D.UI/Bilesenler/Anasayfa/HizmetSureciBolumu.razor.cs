using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class HizmetSureciBolumu : ComponentBase
{
    private List<HizmetAdimi> _adimlar = [];

    protected override async Task OnInitializedAsync()
    {
        try { var l = await api.GetAsync<List<HizmetAdimi>>("api/hizmet-adimlari"); if (l != null) _adimlar = l; } catch { /* API erişilemezse hizmet süreci bölümü boş kalır */ }
    }
}

