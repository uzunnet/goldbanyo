using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Desadoor.UI.Servisler;

namespace Desadoor.UI.Pages.Admin;

public partial class TemaYonetimi : ComponentBase
{
    [Inject] ISnackbar Snackbar { get; set; } = default!;
    [Inject] IJSRuntime JS { get; set; } = default!;
    [Inject] ApiIstemcisi Api { get; set; } = default!;

    private record TemaSablonu(
        string Ad,
        string Baslik,
        string Aciklama,
        string Birincil,
        string Vurgu,
        string ArkaPlan,
        string Yuzey,
        bool KoyuTemaMi
    );

    private sealed class FirmaTemaDto
    {
        public string? AdminTema { get; set; }
        public string? SiteTema { get; set; }
    }

    private sealed class FirmaTemaGuncelleDto
    {
        public string AdminTema { get; set; } = "endustri-karanlik";
        public string SiteTema { get; set; } = "endustri-karanlik";
    }

    private readonly List<TemaSablonu> _temalar = new()
    {
        new("endustri-karanlik",  "Endüstri Karanlık",   "Siyah zemin, altın vurgu — fabrika hissi",
            "#0a0a0a", "#C5A059", "#0a0a0a", "#111111", true),
        new("klasik-aydinlik",    "Klasik Aydınlık",      "Beyaz ağırlıklı, siyah metin — temiz ofis",
            "#1A1A27", "#C8952A", "#F8F6F2", "#FFFFFF", false),
        new("altin-siyah",        "Altın Siyah",           "Derin siyah, yoğun altın — premium lüks",
            "#000000", "#D4A843", "#080808", "#151515", true),
        new("modern-gri",         "Modern Gri",            "Soğuk gri tonlar, çelik mavi — minimal",
            "#1E1E24", "#8BA4BC", "#121218", "#1C1C22", true),
        new("komuta-mavi",        "Komuta Mavi",           "Koyu lacivert, canlı mavi veri panelleri — operasyon merkezi",
            "#061222", "#2D8CFF", "#050F1D", "#081F37", true),
        new("windows-11",         "Windows 11",            "Açık akrilik yüzey, mavi vurgu — sade ve tanıdık",
            "#F3F6FB", "#2563EB", "#F3F6FB", "rgba(255,255,255,0.78)", false),
    };

    private string _seciliTema = "endustri-karanlik";
    private string _seciliTemaBaslik = "Endüstri Karanlık";
    private string _seciliBirincil = "#0a0a0a";
    private string _seciliVurgu = "#C5A059";
    private string _seciliArkaPlan = "#0a0a0a";
    private string _seciliYuzey = "#111111";
    private string _aktifSiteTema = "endustri-karanlik";

    protected override async Task OnInitializedAsync()
    {
        var uygulanacakTema = await FirmaTemasiniGetirAsync();
        if (!string.IsNullOrWhiteSpace(uygulanacakTema) && _temalar.Any(t => t.Ad == uygulanacakTema))
        {
            await TemaUygulaAsync(uygulanacakTema);
            return;
        }

        try
        {
            var kayitli = await JS.InvokeAsync<string>("localStorage.getItem", "desadoor_admin_tema");
            if (!string.IsNullOrEmpty(kayitli) && _temalar.Any(t => t.Ad == kayitli))
            {
                await TemaUygulaAsync(kayitli);
            }
        }
        catch { }
    }

    private async Task TemaSecAsync(TemaSablonu tema)
    {
        await TemaUygulaAsync(tema.Ad);
        var sonuc = await Api.PutAsync<FirmaTemaDto>("api/firma-tema", new FirmaTemaGuncelleDto
        {
            AdminTema = tema.Ad,
            SiteTema = _aktifSiteTema
        });

        if (sonuc?.BasariliMi != true)
        {
            Snackbar.Add("Tema firma ayarına kaydedilemedi. Oturum yetkisini kontrol edin.", Severity.Warning);
        }

        await JS.InvokeVoidAsync("localStorage.setItem", "desadoor_admin_tema", tema.Ad);
        Snackbar.Add($"'{tema.Baslik}' teması uygulandı.", Severity.Success);
    }

    private async Task<string?> FirmaTemasiniGetirAsync()
    {
        try
        {
            var firmaTema = await Api.GetAsync<FirmaTemaDto>("api/firma-tema");
            if (!string.IsNullOrWhiteSpace(firmaTema?.SiteTema))
            {
                _aktifSiteTema = firmaTema.SiteTema;
            }
            return firmaTema?.AdminTema;
        }
        catch
        {
            return null;
        }
    }

    private async Task TemaUygulaAsync(string temaAdi)
    {
        var tema = _temalar.FirstOrDefault(t => t.Ad == temaAdi);
        if (tema == null) return;

        _seciliTema = tema.Ad;
        _seciliTemaBaslik = tema.Baslik;
        _seciliBirincil = tema.Birincil;
        _seciliVurgu = tema.Vurgu;
        _seciliArkaPlan = tema.ArkaPlan;
        _seciliYuzey = tema.Yuzey;

        await JS.InvokeVoidAsync("desadoorTema.uygula",
            tema.Birincil,
            tema.Vurgu,
            tema.ArkaPlan,
            tema.Yuzey,
            tema.KoyuTemaMi,
            tema.Ad);
    }
}
