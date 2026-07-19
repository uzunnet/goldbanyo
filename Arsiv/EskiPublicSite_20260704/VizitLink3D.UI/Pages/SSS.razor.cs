using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Pages;

public partial class SSS : ComponentBase, IDisposable
{
    private bool _yukleniyor = true;
    private string _sayfaBasligi = "";
    private string _heroBaslik = "";

    private class SssOgesi
    {
        public string Soru { get; set; } = "";
        public string Cevap { get; set; } = "";
    }

    private List<SssOgesi> _sorular = new();

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        DilleriAyarla();
        await SorulariYukleAsync();
    }

    private void DilleriAyarla()
    {
        _heroBaslik = dil.T("sss.baslik", "Sıkça Sorulan Sorular");
        _sayfaBasligi = dil.T("sss.sayfaBasligi", "Sıkça Sorulan Sorular | Gold Banyo");
    }

    private async Task SorulariYukleAsync()
    {
        _yukleniyor = true;
        try
        {
            var liste = await api.GetAsync<List<SikSorulanSoru>>("api/sss");
            if (liste != null && liste.Any())
            {
                _sorular = liste
                    .Where(s => s.AktifMi)
                    .OrderBy(s => s.SiraNo)
                    .Select(s => new SssOgesi { Soru = s.Soru, Cevap = s.Cevap })
                    .ToList();
            }
        }
        catch { }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async void OnDilDegisti()
    {
        DilleriAyarla();
        await SorulariYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }

    public class SikSorulanSoru
    {
        public string Soru { get; set; } = "";
        public string Cevap { get; set; } = "";
        public bool AktifMi { get; set; }
        public int SiraNo { get; set; }
    }
}
