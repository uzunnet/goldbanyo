using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Pages.Admin;

public partial class SifreSifirla : ComponentBase
{
    [Inject] private DilServisi Dil { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private string? _durum;

    protected override void OnInitialized()
    {
        var uri = new Uri(Nav.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        query.TryGetValue("durum", out var durumValues);
        _durum = durumValues.FirstOrDefault();
    }
}
