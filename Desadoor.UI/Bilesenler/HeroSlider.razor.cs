using Desadoor.Ortak.Modeller;
using Desadoor.UI.Servisler;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Bilesenler;

public partial class HeroSlider : ComponentBase, IDisposable
{
    [Parameter] public string SayfaKodu { get; set; } = "anasayfa";

    private List<Slayt> _slaytlar = [];
    private bool _yukleniyor = true;
    private int _aktifIndex;
    private Timer? _zamanlayici;

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += DilDegistiginde;
        await SlaytlariYukleAsync();
    }

    private async void DilDegistiginde()
    {
        await SlaytlariYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task SlaytlariYukleAsync()
    {
        try
        {
            var slaytListesi = await api.GetAsync<List<Slayt>>($"api/desadoor/slaytlar?dil={dil.AktifDil}&sayfaKodu={SayfaKodu}");
            if (slaytListesi is { Count: > 0 })
            {
                _slaytlar = slaytListesi.Where(s => s.AktifMi).OrderBy(s => s.SiraNo).ToList();
            }
        }
        catch { /* slayt yuklenemezse bos goster */ }
        finally
        {
            _yukleniyor = false;
            if (_slaytlar.Count > 1)
            {
                _zamanlayici?.Dispose();
                _zamanlayici = new Timer(async _ => await SonrakiSlayt(), null, 5000, 5000);
            }
        }
    }

    private void SlaytaGit(int index)
    {
        _aktifIndex = index;
        StateHasChanged();
    }

    private string ResimUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol)) return string.Empty;
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return yol;
        return $"{api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private async Task SonrakiSlayt()
    {
        _aktifIndex = (_aktifIndex + 1) % _slaytlar.Count;
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _zamanlayici?.Dispose();
    }
}
