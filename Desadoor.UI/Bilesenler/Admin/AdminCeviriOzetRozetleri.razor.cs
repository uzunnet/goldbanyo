using Desadoor.UI.Servisler;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Bilesenler.Admin;

public partial class AdminCeviriOzetRozetleri : ComponentBase
{
    [Parameter] public AdminCeviriOzet? Ozet { get; set; }
    [Parameter] public string Class { get; set; } = "mb-3";
}
