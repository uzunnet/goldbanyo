using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Pages;

public partial class Referanslar : ComponentBase, IDisposable
{
    private bool _yukleniyor = true;
    private string _sayfaBasligi = "";

    private class YorumOgesi
    {
        public string Yorum { get; set; } = "";
        public string MusteriAdi { get; set; } = "";
        public string? MusteriSehir { get; set; }
    }

    private class RefOgesi
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string? Logo { get; set; }
        public string? Aciklama { get; set; }
        public string Tip { get; set; } = "";
    }

    private List<YorumOgesi> _yorumlar = [];
    private List<RefOgesi> _referanslar = [];

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        _sayfaBasligi = dil.T("referanslar.sayfaBasligi", "Referanslar | DesaDoor");
        await VerileriYukleAsync();
    }

    private async Task VerileriYukleAsync()
    {
        _yukleniyor = true;
        try
        {
            var refListe = await api.GetAsync<List<RefOgesi>>("api/desadoor/referanslar");
            _referanslar = refListe ?? [];

            var yorumListe = await api.GetAsync<List<YorumOgesi>>("api/desadoor/musteri-yorumlari");
            _yorumlar = yorumListe ?? [];
        }
        catch { }
        finally { _yukleniyor = false; }
    }

    private string LogoUrl(string? logo)
    {
        if (string.IsNullOrWhiteSpace(logo)) return "";
        if (logo.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return logo;
        return $"{api.ApiBaseUrl}{(logo.StartsWith('/') ? logo : "/" + logo)}";
    }

    private async void OnDilDegisti()
    {
        _sayfaBasligi = dil.T("referanslar.sayfaBasligi", "Referanslar | DesaDoor");
        await VerileriYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => dil.DilDegisti -= OnDilDegisti;
}
