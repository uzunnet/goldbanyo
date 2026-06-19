using Desadoor.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Bilesenler.Urunler;

public partial class UrunListeKart : ComponentBase
{
    [Parameter, EditorRequired] public Urun Urun { get; set; } = null!;
    [Parameter] public EventCallback<Urun> OnClick { get; set; }

    private string _gorselUrl
    {
        get
        {
            if (Urun.AnaGorselMedyaId is long medyaId and > 0)
                return $"{api.ApiBaseUrl}/api/medya/dosya/{medyaId}";
            var kod = Urun.Kod?.ToUpperInvariant() ?? "";
            if (kod.StartsWith("LAKE"))
                return "/medya/kapaklar/LAKE KAPAK.jpg";
            var numStr = new string(Urun.Kod.Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(numStr) ? "/medya/desadoor_default.png" : $"/medya/kapaklar/yatay_{numStr}.png";
        }
    }

    private async Task Tiklandi()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(Urun);
    }

    private async Task HakkindaIste()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(Urun);
    }
}
