using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Bilesenler.Admin;

public partial class AdminHataDurumu : ComponentBase
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string Mesaj { get; set; } = "";
    [Parameter] public string? TekrarDeneMetni { get; set; }
    [Parameter] public EventCallback TekrarDene { get; set; }
}
