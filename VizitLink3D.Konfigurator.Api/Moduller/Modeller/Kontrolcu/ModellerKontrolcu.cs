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
    private readonly IWebHostEnvironment _env;

    public ModellerKontrolcu(KonfiguratorDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
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

    /// <summary>
    /// GLB dosyasini dogrudan stream eder.
    /// BFF proxy bu endpoint uzerinden dosyayi indirir.
    /// DosyaYolu (UUID'li gercek saklama adi) kullanilir — DosyaAdi degil.
    /// </summary>
    [HttpGet("{slug}/dosya")]
    public async Task<IActionResult> DosyaIndirAsync(string slug, CancellationToken iptal = default)
    {
        // P04 ret: AktifMi filtresi KALDIRILDI — admin panelinde pasif modellerin
        // preview'ı için BFF proxy bu endpoint'i kullanır. Yetkilendirme BFF katmanında.
        var model = await _db.UcBoyutModeller
            .AsNoTracking()
            .Where(x => x.Slug == slug)
            .Select(x => new { x.DosyaYolu, x.IcerikTuru, x.DosyaAdi })
            .FirstOrDefaultAsync(iptal);

        if (model is null)
            return NotFound();

        // DosyaYolu: "/medya/3d-modeller/UUID.glb"
        var tamYol = Path.Combine(_env.WebRootPath, model.DosyaYolu.TrimStart('/'));

        if (!System.IO.File.Exists(tamYol))
            return NotFound();

        var akis = new FileStream(tamYol, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(akis, model.IcerikTuru ?? "model/gltf-binary", model.DosyaAdi);
    }
}
