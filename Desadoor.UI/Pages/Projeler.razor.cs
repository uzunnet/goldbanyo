using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Pages;

public partial class Projeler : ComponentBase, IDisposable
{
    private bool _yukleniyor = true;
    private string _sayfaBasligi = "Projelerimiz | DesaDoor";
    private string _heroBaslik = "Projelerimiz";
    private string _heroAciklama = "Tamamladigimiz projeler.";
    private List<ProjeKartDto> _projeler = new();

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        await ProjeleriYukleAsync();
    }

    private async Task ProjeleriYukleAsync()
    {
        _yukleniyor = true;
        try
        {
            var liste = await api.GetAsync<List<ProjeKartDto>>($"api/projeler?dil={dil.AktifDil}");
            if (liste != null)
                _projeler = liste;
        }
        catch { }
        _yukleniyor = false;
    }

    private string ProjeGorseli(ProjeKartDto proje)
        => TamResimUrl(proje.KapakResim);

    private string ProjeLogo(ProjeKartDto proje)
        => TamResimUrl(proje.MusteriLogo);

    private string TamResimUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol)) return "/medya/desadoor_default.png";
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return yol;
        return $"{api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private static string ProjeDetayBaglantisi(ProjeKartDto proje)
        => string.IsNullOrWhiteSpace(proje.Slug) ? $"/projeler/{proje.Id}" : $"/projeler/{proje.Slug}";

    private async void OnDilDegisti()
    {
        await ProjeleriYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }

    public class ProjeKartDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";
        public string Baslik { get; set; } = "";
        public string? MusteriAdi { get; set; }
        public string? MusteriSehir { get; set; }
        public string? MusteriLogo { get; set; }
        public string? KapakResim { get; set; }
        public string? KisaAciklama { get; set; }
    }
}
