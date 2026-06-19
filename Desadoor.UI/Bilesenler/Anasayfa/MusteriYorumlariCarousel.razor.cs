using Desadoor.Ortak.Modeller;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Bilesenler.Anasayfa;

public partial class MusteriYorumlariCarousel : ComponentBase
{
    private List<MusteriYorumu> _yorumlar = [];

    protected override async Task OnInitializedAsync()
    {
        try { var l = await api.GetAsync<List<MusteriYorumu>>("api/desadoor/musteri-yorumlari"); if (l != null) _yorumlar = l; } catch { /* API erişilemezse yorum carousel boş kalır */ }
    }
}
