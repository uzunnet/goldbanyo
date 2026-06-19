using Desadoor.Ortak.Modeller;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Pages;

public partial class Bayiler : ComponentBase, IDisposable
{
    private List<Sube> _bayiler = [];
    private bool _yukleniyor = true;
    private string _sayfaBasligi = "Bayilerimiz | DesaDoor";

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        await BayileriYukleAsync();
    }

    private async Task BayileriYukleAsync()
    {
        _yukleniyor = true;
        try
        {
            var liste = await api.GetAsync<List<Sube>>("api/desadoor/subeler");
            _bayiler = liste?.Where(s => s.AktifMi && !s.SilindiMi).OrderBy(s => s.SiraNo).ToList() ?? [];
        }
        catch { }
        _yukleniyor = false;
    }

    private async void OnDilDegisti()
    {
        await BayileriYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }
}