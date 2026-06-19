using Desadoor.Ortak.Modeller;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Bilesenler.Anasayfa;

public partial class SSSBolumu : ComponentBase
{
    private List<SikSorulanSoru> _sss = [];

    protected override async Task OnInitializedAsync()
    {
        try { var l = await api.GetAsync<List<SikSorulanSoru>>("api/desadoor/sss"); if (l != null) _sss = l; } catch { /* API erişilemezse SSS bölümü boş kalır */ }
    }
}
