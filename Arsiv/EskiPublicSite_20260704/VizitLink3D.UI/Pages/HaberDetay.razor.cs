using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Pages;

public partial class HaberDetay : ComponentBase
{
    [Parameter] public string Slug { get; set; } = string.Empty;

    private bool _yukleniyor = true;
    private string _sayfaBasligi = "Haber Detay� | VizitLink3D";
    private HaberYazisi? _yazi;
    private List<HaberOzet> _enCokOkunanlar = [];

    protected override async Task OnParametersSetAsync()
    {
        await DetayYukleAsync();
    }

    private async Task DetayYukleAsync()
    {
        _yukleniyor = true;

        _yazi = int.TryParse(Slug, out var id)
            ? await api.GetAsync<HaberYazisi>($"api/Haber-yazilari/{id}")
            : await api.GetAsync<HaberYazisi>($"api/Haber-yazilari/slug/{Slug}");

        _enCokOkunanlar = await api.GetAsync<List<HaberOzet>>("api/Haber-yazilari/en-cok-okunan") ?? [];

        if (_yazi is not null)
            _sayfaBasligi = $"{_yazi.Baslik} | VizitLink3D";

        _yukleniyor = false;
    }

    private static string HaberGorseli(HaberYazisi yazi)
        => string.IsNullOrWhiteSpace(yazi.AnaResimUrl) ? "/medya/vizitlink3d_default.png" : yazi.AnaResimUrl;

    private static string HaberGorseli(HaberOzet yazi)
        => string.IsNullOrWhiteSpace(yazi.AnaResimUrl) ? "/medya/vizitlink3d_default.png" : yazi.AnaResimUrl;

    private static string HaberDetayBaglantisi(HaberOzet yazi)
        => string.IsNullOrWhiteSpace(yazi.Slug) ? $"/haber/{yazi.Id}" : $"/haber/{yazi.Slug}";

    private string IlkEtiket(HaberYazisi yazi)
        => Etiketler(yazi).FirstOrDefault() ?? dil.T("Haber.genel", "Genel");

    private static IEnumerable<string> Etiketler(HaberYazisi yazi)
        => yazi.Etiketler?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

    private string TarihYaz(HaberYazisi yazi)
        => (yazi.YayinTarihi ?? yazi.OlusturmaTarihi).ToString("dd MMM yyyy");

    private sealed class HaberOzet
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";
        public string Baslik { get; set; } = "";
        public string AnaResimUrl { get; set; } = "";
        public int OkunmaSayisi { get; set; }
    }
}

