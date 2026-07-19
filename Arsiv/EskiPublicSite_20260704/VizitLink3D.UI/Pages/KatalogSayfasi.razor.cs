using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Pages;

public partial class KatalogSayfasi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;

    private List<Ortak.Modeller.Katalog> _kataloglar = new();
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        await YukleAsync();
    }

    private async Task YukleAsync()
    {
        _yukleniyor = true;
        try
        {
            var cevap = await Api.GetAsync<List<Ortak.Modeller.Katalog>>("api/kataloglar");
            _kataloglar = cevap?.Where(k => k.AktifMi).OrderBy(k => k.SiraNo).ToList() ?? new();
        }
        catch
        {
            _kataloglar = new();
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private string OnizlemeUrl(Ortak.Modeller.Katalog katalog)
    {
        if (!string.IsNullOrWhiteSpace(katalog.KapakResim))
        {
            return TamUrl(katalog.KapakResim);
        }

        return $"{Api.ApiBaseUrl}/api/belge-onizleme?dosya={Uri.EscapeDataString(katalog.PdfDosyaYolu)}";
    }

    private string PdfGostericiUrl(Ortak.Modeller.Katalog katalog)
    {
        return $"/pdf-gosterici?dosya={Uri.EscapeDataString(katalog.PdfDosyaYolu)}&baslik={Uri.EscapeDataString(katalog.Baslik)}&donus={Uri.EscapeDataString("/katalog")}";
    }

    private string IndirUrl(Ortak.Modeller.Katalog katalog)
    {
        return BelgeDosyaUrl(katalog.PdfDosyaYolu);
    }

    private string TamUrl(string yol)
    {
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return yol;
        }

        return $"{Api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private string BelgeDosyaUrl(string yol)
    {
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return yol;
        }

        return $"{Api.ApiBaseUrl}/api/belge-dosya?dosya={Uri.EscapeDataString(yol)}";
    }
}

