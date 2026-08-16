using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class ProjeDetay : IDisposable
{
    [Parameter] public string Slug { get; set; } = string.Empty;

    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private Proje? _proje;
    private bool _yukleniyor = true;
    private string? _hataMesaji;
    private string? _sonYuklenenSlug;
    private string? _seciliResimUrl;
    private bool _buyukResimAcik;
    private string? _buyukResimYolu;

    protected override void OnInitialized()
    {
        DilServisi.DilDegisti += DilDegistiginde;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (string.Equals(_sonYuklenenSlug, Slug, StringComparison.OrdinalIgnoreCase))
            return;

        _sonYuklenenSlug = Slug;
        await ProjeYukleAsync();
    }

    private async Task ProjeYukleAsync()
    {
        _yukleniyor = true;
        _hataMesaji = null;
        _proje = null;

        if (string.IsNullOrWhiteSpace(Slug))
        {
            _hataMesaji = DilServisi.T("projeDetay.slugBos", "Proje adresi eksik.");
            _yukleniyor = false;
            return;
        }

        var proje = await Api.GetAsync<Proje>($"api/projeler/slug/{Uri.EscapeDataString(Slug)}");
        if (proje is null)
        {
            _hataMesaji = DilServisi.T("projeDetay.yuklenemedi", "Proje bilgileri yüklenemedi.");
        }
        else
        {
            _proje = proje;
        }

        _yukleniyor = false;
    }

    private List<ProjeResim> GaleriResimleri()
    {
        return _proje?.Resimler?
            .Where(r => !string.IsNullOrWhiteSpace(r.Url))
            .OrderBy(r => r.Sira)
            .ThenBy(r => r.Id)
            .ToList() ?? [];
    }

    private string? AnaKapakYolu()
    {
        if (!string.IsNullOrWhiteSpace(_seciliResimUrl))
            return _seciliResimUrl;

        var kapak = ResimYoluDuzenle(_proje?.KapakResim);
        if (!string.IsNullOrWhiteSpace(kapak))
            return kapak;

        return ResimYoluDuzenle(GaleriResimleri().FirstOrDefault()?.Url);
    }

    private static string? ResimYoluDuzenle(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
            return null;

        var temizYol = yol.Trim();

        if (temizYol.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            temizYol.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            temizYol.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            temizYol.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            return temizYol;

        if (temizYol.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            return null;

        if (temizYol.StartsWith('/'))
            return temizYol;

        return "/" + temizYol.TrimStart('/');
    }

    private void ResimSec(string? yol)
    {
        _seciliResimUrl = yol;
    }

    private void ResmiBuyut(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
            return;

        _buyukResimYolu = yol;
        _buyukResimAcik = true;
    }

    private void ResmiKapat()
    {
        _buyukResimAcik = false;
        _buyukResimYolu = null;
    }

    private string KartSinifi(string resimYolu) =>
        string.Equals(resimYolu, _seciliResimUrl, StringComparison.OrdinalIgnoreCase)
            ? "gb-detail-medya__kart gb-detail-medya__kart--aktif"
            : "gb-detail-medya__kart";

    private string TarihMetni(Proje proje)
    {
        if (proje.ProjeTarihi.HasValue)
            return proje.ProjeTarihi.Value.ToString("yyyy");

        return proje.OlusturulmaTarihi.ToString("yyyy");
    }

    private string AciklamaMetni()
    {
        if (string.IsNullOrWhiteSpace(_proje?.Aciklama))
            return string.Empty;

        return Regex.Replace(_proje.Aciklama, "<.*?>", string.Empty).Trim();
    }

    private void DilDegistiginde() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        DilServisi.DilDegisti -= DilDegistiginde;
    }
}