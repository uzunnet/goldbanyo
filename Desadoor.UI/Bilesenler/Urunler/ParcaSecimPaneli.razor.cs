using Desadoor.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Bilesenler.Urunler;

public partial class ParcaSecimPaneli : ComponentBase
{
    [Parameter, EditorRequired] public List<UrunUcBoyutParcasi> Parcalar { get; set; } = [];
    [Parameter] public int? SeciliParcaId { get; set; }
    [Parameter] public EventCallback<UrunUcBoyutParcasi> ParcaSecildi { get; set; }

    private string ParcaSinifi(UrunUcBoyutParcasi parca)
    {
        var temel = "desa-parca-kalem";
        if (parca.Id == SeciliParcaId)
            temel += " desa-parca-secili";
        return temel;
    }

    private async Task ParcaTiklandi(UrunUcBoyutParcasi parca)
    {
        if (ParcaSecildi.HasDelegate)
            await ParcaSecildi.InvokeAsync(parca);
    }
}
