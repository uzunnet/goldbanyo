using Desadoor.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Desadoor.UI.Pages;

public partial class ProjeDetay : ComponentBase
{
    [Parameter] public string Slug { get; set; } = string.Empty;

    private bool _yukleniyor = true;
    private string _sayfaBasligi = "Proje Detayi | DesaDoor";
    private Proje? _proje;
    private List<Proje> _enCokIncelenenler = [];

    private IEnumerable<SurecMaddesi> SurecMaddeleri =>
    [
        new(dil.T("projeDetay.kesif", "Kesif"), dil.T("projeDetay.kesifAciklama", "Olcu, mekan ve ihtiyac analizi tek dosyada toplandi."), Icons.Material.Filled.Straighten),
        new(dil.T("projeDetay.uretim", "Uretim"), dil.T("projeDetay.uretimAciklama", "Malzeme, yuzey ve aksesuar kararları kontrollu uretime aktarıldı."), Icons.Material.Filled.PrecisionManufacturing),
        new(dil.T("projeDetay.montaj", "Montaj"), dil.T("projeDetay.montajAciklama", "Saha montaji temiz teslim ve son kontrol ile kapatildi."), Icons.Material.Filled.Verified)
    ];

    protected override async Task OnParametersSetAsync()
    {
        await DetayYukleAsync();
    }

    private async Task DetayYukleAsync()
    {
        _yukleniyor = true;

        _proje = int.TryParse(Slug, out var id)
            ? await api.GetAsync<Proje>($"api/projeler/{id}")
            : await api.GetAsync<Proje>($"api/projeler/slug/{Slug}");

        _enCokIncelenenler = await api.GetAsync<List<Proje>>("api/projeler/en-cok-incelenen") ?? [];

        if (_proje is not null)
            _sayfaBasligi = $"{_proje.Baslik} | DesaDoor";

        _yukleniyor = false;
    }

    private static string ProjeGorseli(Proje proje)
        => string.IsNullOrWhiteSpace(proje.KapakResim) ? "/medya/desadoor_default.png" : proje.KapakResim;

    private static string ProjeDetayBaglantisi(Proje proje)
        => string.IsNullOrWhiteSpace(proje.Slug) ? $"/projeler/{proje.Id}" : $"/projeler/{proje.Slug}";

    private string DegerYaz(string? deger)
        => string.IsNullOrWhiteSpace(deger) ? dil.T("ortak.belirtilmedi", "Belirtilmedi") : deger;

    private string ProjeTarihiYaz(Proje proje)
        => proje.ProjeTarihi?.ToString("dd MMM yyyy") ?? dil.T("ortak.belirtilmedi", "Belirtilmedi");

    private string AciklamaYaz(Proje proje)
    {
        var metin = !string.IsNullOrWhiteSpace(proje.Aciklama)
            ? proje.Aciklama
            : proje.KisaAciklama ?? dil.T("projeDetay.aciklamaYok", "Bu proje icin detay aciklamasi hazirlaniyor.");
        
        return HtmlDuzyazi(metin);
    }

    private static string HtmlDuzyazi(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";
        var t = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
        t = t.Replace("&nbsp;", " ").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">");
        return t.Trim();
    }

    private sealed record SurecMaddesi(string Baslik, string Aciklama, string Ikon);
}
