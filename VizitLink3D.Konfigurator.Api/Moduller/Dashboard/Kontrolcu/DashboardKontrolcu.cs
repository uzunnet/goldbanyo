using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Konfigurator.Api.AraYazilimlar;
using VizitLink3D.Konfigurator.Api.Moduller.Dashboard.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Api.Moduller.Dashboard.Kontrolcu;

[ApiController]
[Route("api/yonetim/dashboard")]
[ServiceFilter(typeof(BffGuvenlikFilter))]
public class DashboardKontrolcu : ControllerBase
{
    private readonly KonfiguratorDbContext _db;

    public DashboardKontrolcu(KonfiguratorDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<KonfiguratorCevap<DashboardIstatistikDto>> IstatistikGetirAsync(CancellationToken iptal = default)
    {
        var toplamModel = await _db.UcBoyutModeller.CountAsync(iptal);
        var aktifModel = await _db.UcBoyutModeller.CountAsync(x => x.AktifMi, iptal);
        var toplamParca = await _db.UcBoyutModelParcalari.CountAsync(iptal);

        var sonModeller = await _db.UcBoyutModeller
            .AsNoTracking()
            .OrderByDescending(x => x.OlusturulmaTarihi)
            .Take(5)
            .Select(x => new SonModelDto
            {
                Id = x.Id,
                Ad = x.Ad,
                Slug = x.Slug,
                BoyutBayt = x.BoyutBayt,
                OlusturulmaTarihi = x.OlusturulmaTarihi,
                AktifMi = x.AktifMi
            })
            .ToListAsync(iptal);

        var dto = new DashboardIstatistikDto
        {
            ToplamModelSayisi = toplamModel,
            AktifModelSayisi = aktifModel,
            ToplamParcaSayisi = toplamParca,
            SonEklenenModeller = sonModeller
        };

        return KonfiguratorCevap<DashboardIstatistikDto>.Basarili(dto);
    }
}
