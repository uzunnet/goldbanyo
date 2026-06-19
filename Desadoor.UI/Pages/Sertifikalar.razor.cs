using Desadoor.Ortak.Modeller;
using Desadoor.UI.Servisler;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Pages;

public partial class Sertifikalar : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;

    private List<Sertifika> _sertifikalar = [];
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        _sertifikalar = await Api.GetAsync<List<Sertifika>>("api/desadoor/sertifikalar") ?? [];
        _yukleniyor = false;
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return string.Empty;
        }

        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return yol;
        }

        return $"{Api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private static string? BelgeYolu(Sertifika sertifika)
    {
        if (!string.IsNullOrWhiteSpace(sertifika.PdfDosya))
        {
            return sertifika.PdfDosya;
        }

        return sertifika.Resim;
    }

    private string PdfGostericiUrl(Sertifika sertifika)
    {
        var belgeYolu = BelgeYolu(sertifika);
        if (string.IsNullOrWhiteSpace(belgeYolu))
        {
            return "/sertifikalar";
        }

        return $"/pdf-gosterici?dosya={Uri.EscapeDataString(belgeYolu)}&baslik={Uri.EscapeDataString(sertifika.Ad)}&donus={Uri.EscapeDataString("/sertifikalar")}";
    }

    private string OnizlemeUrl(Sertifika sertifika)
    {
        if (!string.IsNullOrWhiteSpace(sertifika.Resim))
        {
            return TamUrl(sertifika.Resim);
        }

        var belgeYolu = BelgeYolu(sertifika);
        return string.IsNullOrWhiteSpace(belgeYolu)
            ? string.Empty
            : $"{Api.ApiBaseUrl}/api/desadoor/belge-onizleme?dosya={Uri.EscapeDataString(belgeYolu)}";
    }

    private string TarihMetni(Sertifika sertifika)
    {
        if (sertifika.GecerlilikTarihi.HasValue)
        {
            return $"{Dil.T("sertifikalar.gecerlilik", "Gecerlilik")}: {sertifika.GecerlilikTarihi.Value:dd.MM.yyyy}";
        }

        if (sertifika.VerilmeTarihi.HasValue)
        {
            return $"{Dil.T("sertifikalar.verilme", "Verilme")}: {sertifika.VerilmeTarihi.Value:dd.MM.yyyy}";
        }

        return Dil.T("sertifikalar.guncelBelge", "Guncel belge");
    }
}
