using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Pages;

public partial class Haber : ComponentBase, IDisposable
{
    private bool _yukleniyor = true;
    private string _sayfaBasligi = "Haber | DesaDoor";
    private string _heroBaslik = "Haber";
    private string _heroAciklama = "DesaDoor'dan haberler ve icerikler.";
    private List<HaberOgesi> _yazilar = new();

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        await HaberYazilariniYukleAsync();
    }

    private async Task HaberYazilariniYukleAsync()
    {
        _yukleniyor = true;
        try
        {
            var liste = await api.GetAsync<List<HaberOgesi>>($"api/desadoor/Haber-yazilari?dil={dil.AktifDil}");
            if (liste != null)
                _yazilar = liste;
        }
        catch { }
        _yukleniyor = false;
    }

    private static string HaberGorseli(HaberOgesi yazi)
        => string.IsNullOrWhiteSpace(yazi.AnaResimUrl) ? "/medya/desadoor_default.png" : yazi.AnaResimUrl;

    private static string HaberDetayBaglantisi(HaberOgesi yazi)
        => string.IsNullOrWhiteSpace(yazi.Slug) ? $"/haber/{yazi.Id}" : $"/haber/{yazi.Slug}";

    private string IlkEtiket(HaberOgesi yazi)
        => yazi.Etiketler?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault()
            ?? dil.T("Haber.genel", "Genel");

    private async void OnDilDegisti()
    {
        await HaberYazilariniYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }

    public class HaberOgesi
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("slug")] public string Slug { get; set; } = "";
        [JsonPropertyName("baslik")] public string Baslik { get; set; } = "";
        [JsonPropertyName("ozet")] public string Ozet { get; set; } = "";
        [JsonPropertyName("anaResimUrl")] public string AnaResimUrl { get; set; } = "";
        [JsonPropertyName("etiketler")] public string? Etiketler { get; set; }
        [JsonPropertyName("tarih")] public DateTime Tarih { get; set; }
        [JsonPropertyName("okunmaSayisi")] public int OkunmaSayisi { get; set; }
    }
}
