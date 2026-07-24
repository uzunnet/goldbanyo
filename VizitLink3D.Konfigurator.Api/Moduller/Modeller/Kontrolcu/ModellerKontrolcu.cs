using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Kontrolcu;

[ApiController]
[Route("api/modeller")]
public class ModellerKontrolcu : ControllerBase
{
    private readonly KonfiguratorDbContext _db;

    public ModellerKontrolcu(KonfiguratorDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<KonfiguratorCevap<List<UcBoyutModelOzetDto>>> ListeleAsync(CancellationToken iptal = default)
    {
        var modeller = await _db.UcBoyutModeller
            .AsNoTracking()
            .Where(x => x.AktifMi)
            .OrderByDescending(x => x.OlusturulmaTarihi)
            .Select(x => new UcBoyutModelOzetDto(
                x.Id,
                x.Ad,
                x.Slug,
                x.Aciklama,
                x.DosyaAdi,
                x.BoyutBayt,
                x.OlusturulmaTarihi
            ))
            .ToListAsync(iptal);

        return KonfiguratorCevap<List<UcBoyutModelOzetDto>>.Basarili(modeller);
    }

    [HttpGet("{slug}")]
    public async Task<KonfiguratorCevap<UcBoyutModelDto>> GetirAsync(string slug, CancellationToken iptal = default)
    {
        var model = await _db.UcBoyutModeller
            .AsNoTracking()
            .Where(x => x.AktifMi && x.Slug == slug)
            .Select(x => new
            {
                x.Id,
                x.Ad,
                x.Slug,
                x.Aciklama,
                x.DosyaAdi,
                x.IcerikTuru,
                x.BoyutBayt,
                x.OlusturulmaTarihi
            })
            .FirstOrDefaultAsync(iptal);

        if (model is null)
            return KonfiguratorCevap<UcBoyutModelDto>.Hata("Model bulunamadı.");

        // Görünür, silinmemiş parçaları getir
        var parcalar = await _db.UcBoyutModelParcalari
            .AsNoTracking()
            .Where(p => p.ModelId == model.Id && p.GorunurMu)
            .OrderBy(p => p.ParcaTuru)
            .ThenBy(p => p.MeshAdi)
            .Select(p => new UcBoyutModelParcasiDto(
                p.Id,
                p.MeshAdi,
                p.GorunenAd,
                p.ParcaTuru.ToString(),
                p.RenkDegistirilebilirMi,
                p.GorunurMu,
                p.VarsayilanRenk,
                p.VarsayilanMalzeme
            ))
            .ToListAsync(iptal);

        var dto = new UcBoyutModelDto(
            model.Id,
            model.Ad,
            model.Slug,
            model.Aciklama,
            model.DosyaAdi,
            model.IcerikTuru,
            model.BoyutBayt,
            model.OlusturulmaTarihi,
            parcalar
        );

        return KonfiguratorCevap<UcBoyutModelDto>.Basarili(dto);
    }
}
