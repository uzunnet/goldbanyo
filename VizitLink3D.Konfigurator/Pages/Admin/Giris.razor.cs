using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Pages.Admin;

public partial class Giris : ComponentBase
{
    [Inject] private AuthenticationStateProvider AuthState { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private string _returnUrl = "/admin/dashboard";
    private string? _hataMesaji;
    private bool _sifreGorunuyor;

    private void SifreGorunurlukDegistir()
    {
        _sifreGorunuyor = !_sifreGorunuyor;
    }

    protected override async Task OnInitializedAsync()
    {
        // Query string'den returnUrl ve hata oku
        var uri = new Uri(Nav.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        if (query.TryGetValue("ReturnUrl", out var returnUrlValues))
        {
            var qReturnUrl = returnUrlValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(qReturnUrl)
                && qReturnUrl.StartsWith('/')
                && !qReturnUrl.StartsWith("//"))
            {
                _returnUrl = qReturnUrl;
            }
        }

        query.TryGetValue("hata", out var hataValues);
        var hataKey = hataValues.FirstOrDefault();
        _hataMesaji = hataKey switch
        {
            "giris_bos" => Dil.T("giris.hataliBos", "Kullanici adi ve sifre zorunludur."),
            "giris_basarisiz" => Dil.T("giris.hatali", "Kullanici adi veya sifre hatali."),
            "sunucu_hatasi" => Dil.T("giris.sunucuHatasi", "Sunucu hatasi. Lutfen tekrar deneyin."),
            _ => null
        };

        // Zaten giris yapilmissa dashboard'a yonlendir
        var durum = await AuthState.GetAuthenticationStateAsync();
        if (durum.User.Identity?.IsAuthenticated == true)
        {
            Nav.NavigateTo("/admin/dashboard");
        }
    }
}
