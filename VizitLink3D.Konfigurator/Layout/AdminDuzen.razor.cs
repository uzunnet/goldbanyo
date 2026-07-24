using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace VizitLink3D.Konfigurator.Layout;

public partial class AdminDuzen
{
    [Inject] private AuthenticationStateProvider AuthState { get; set; } = default!;

    private bool _cekmeceAcik = true;
    private bool _koyuTema;
    private string _kullaniciAdi = "";

    private MudTheme _tema = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#3B82F6",
            Secondary = "#8B5CF6",
            AppbarBackground = "#0F172A",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#F1F5F9",
            DrawerText = "#1E293B",
            Surface = "#FFFFFF",
            Background = "#F8FAFC"
        }
    };

    protected override async Task OnInitializedAsync()
    {
        var durum = await AuthState.GetAuthenticationStateAsync();
        var user = durum.User;

        _kullaniciAdi = user.FindFirst("KullaniciAdi")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? "-";
    }

    private void CekmeceAcKapat() => _cekmeceAcik = !_cekmeceAcik;

    private void TemaDegistir() => _koyuTema = !_koyuTema;
}
