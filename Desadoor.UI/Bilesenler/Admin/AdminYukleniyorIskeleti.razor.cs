using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Bilesenler.Admin;

public partial class AdminYukleniyorIskeleti : ComponentBase
{
    [Parameter] public int SatirSayisi { get; set; } = 5;
}
