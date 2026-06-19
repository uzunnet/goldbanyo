using Desadoor.Api.Servisler;
using Desadoor.Api.VeriTabani;
using Desadoor.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Desadoor.Api.Kontrolcüler.Sistem;

[ApiController]
[Route("api/firma-tema")]
public class FirmaTemaKontrolcu(DesadoorDbContext vt, KiraciServisi kiraci) : ControllerBase
{
    private static readonly HashSet<string> IzinliTemalar = new(StringComparer.OrdinalIgnoreCase)
    {
        "endustri-karanlik",
        "klasik-aydinlik",
        "altin-siyah",
        "modern-gri",
        "komuta-mavi",
        "windows-11"
    };

    [HttpGet]
    public async Task<IActionResult> AktifFirmaTemasiniGetir()
    {
        var firma = await AktifFirmaGetirAsync();
        if (firma is null)
        {
            return NotFound(Cevap<bool>.Hata("Firma bulunamadı."));
        }

        return Ok(Cevap<FirmaTemaDto>.Basarili(FirmaTemaOlustur(firma)));
    }

    [HttpPut]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> AktifFirmaTemasiniGuncelle([FromBody] FirmaTemaGuncelleDto istek)
    {
        var adminTema = TemaAdiniDogrula(istek.AdminTema);
        var siteTema = TemaAdiniDogrula(istek.SiteTema);

        if (adminTema is null || siteTema is null)
        {
            return BadRequest(Cevap<bool>.Hata("Tema adı geçersiz."));
        }

        var firma = await AktifFirmaGetirAsync();
        if (firma is null)
        {
            return NotFound(Cevap<bool>.Hata("Firma bulunamadı."));
        }

        firma.AdminTema = adminTema;
        firma.SiteTema = siteTema;
        firma.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();

        return Ok(Cevap<FirmaTemaDto>.Basarili(FirmaTemaOlustur(firma), "Firma teması güncellendi."));
    }

    private async Task<Firma?> AktifFirmaGetirAsync()
    {
        if (kiraci.MevcutFirmaId is int firmaId)
        {
            var firma = await vt.Firmalar.FirstOrDefaultAsync(f => f.Id == firmaId && f.AktifMi);
            if (firma is not null)
            {
                return firma;
            }
        }

        return await vt.Firmalar
            .OrderByDescending(f => f.Slug == "desadoor")
            .ThenBy(f => f.Id)
            .FirstOrDefaultAsync(f => f.AktifMi);
    }

    private static FirmaTemaDto FirmaTemaOlustur(Firma firma)
    {
        var adminTema = TemaAdiniDogrula(firma.AdminTema) ?? "endustri-karanlik";
        var siteTema = TemaAdiniDogrula(firma.SiteTema) ?? adminTema;

        return new FirmaTemaDto(
            firma.Id,
            firma.Slug,
            firma.Ad,
            adminTema,
            siteTema,
            firma.TasarimRengi1,
            firma.TasarimRengi2,
            firma.TasarimRengi3);
    }

    private static string? TemaAdiniDogrula(string? temaAdi)
    {
        if (string.IsNullOrWhiteSpace(temaAdi))
        {
            return null;
        }

        var temizTema = temaAdi.Trim().ToLowerInvariant();
        return IzinliTemalar.Contains(temizTema) ? temizTema : null;
    }
}

public sealed record FirmaTemaDto(
    int FirmaId,
    string Slug,
    string Ad,
    string AdminTema,
    string SiteTema,
    string? TasarimRengi1,
    string? TasarimRengi2,
    string? TasarimRengi3);

public sealed record FirmaTemaGuncelleDto(string AdminTema, string SiteTema);
