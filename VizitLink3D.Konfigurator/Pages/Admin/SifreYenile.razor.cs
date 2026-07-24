using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Pages.Admin;

public partial class SifreYenile : ComponentBase
{
    [Inject] private DilServisi Dil { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private string? _token;
    private string? _durum;
    private bool _sifreGorunuyor1;
    private bool _sifreGorunuyor2;

    protected override void OnInitialized()
    {
        var uri = new Uri(Nav.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        query.TryGetValue("token", out var tokenValues);
        _token = tokenValues.FirstOrDefault();

        query.TryGetValue("durum", out var durumValues);
        _durum = durumValues.FirstOrDefault();
    }
}
