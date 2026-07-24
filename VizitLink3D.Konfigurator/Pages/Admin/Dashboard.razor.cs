using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using MudBlazor;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Pages.Admin;

[Microsoft.AspNetCore.Authorization.Authorize]
public partial class Dashboard : ComponentBase
{
    [Inject] private AuthenticationStateProvider AuthState { get; set; } = default!;
    [Inject] private KimlikServisi Kimlik { get; set; } = default!;
    [Inject] private ModellerYonetimServisi ModellerServisi { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;
    [Inject] private IOptions<UygulamaAyarlari> UygulamaAyarlari { get; set; } = default!;

    private string? _kullaniciAdi;
    private string? _rol;
    private bool _apiDurum;
    private bool _yukleniyor = true;
    private bool _modelHatasi;
    private int _modelSayisi;
    private UygulamaAyarlari _runtimeBilgi = default!;

    protected override async Task OnInitializedAsync()
    {
        var durum = await AuthState.GetAuthenticationStateAsync();
        var user = durum.User;

        _kullaniciAdi = user.FindFirst("KullaniciAdi")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? "-";
        _rol = user.FindFirst(ClaimTypes.Role)?.Value
            ?? user.FindFirst("Rol")?.Value
            ?? "-";

        _runtimeBilgi = UygulamaAyarlari.Value;

        // Paralel: API durumu + model sayisi
        var apiGorev = Kimlik.ApiDurumKontrolAsync();
        var modelGorev = ModelSayisiYukleAsync();

        await Task.WhenAll(apiGorev, modelGorev);
        _apiDurum = apiGorev.Result;
        _yukleniyor = false;
    }

    /// <summary>
    /// ModellerYonetimServisi uzerinden model sayisini guvenli sekilde ceker.
    /// API'ye dogrudan erisim yoktur — BFF gizli anahtar kullanilir.
    /// Hata durumunda generic mesaj gosterir, API detayi sizdirmaz.
    /// </summary>
    private async Task ModelSayisiYukleAsync()
    {
        try
        {
            var liste = await ModellerServisi.ListeleAsync();

            if (liste is null)
            {
                _modelHatasi = true;
                _modelSayisi = 0;
            }
            else
            {
                _modelSayisi = liste.Count;
            }
        }
        catch
        {
            _modelHatasi = true;
            _modelSayisi = 0;
        }
    }
}
