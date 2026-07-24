using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Konfigurator.Api.AraYazilimlar;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dogrulayicilar;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Kontrolcu;

[ApiController]
[Route("api/yonetim/modeller/{modelId}/parcalar")]
[ServiceFilter(typeof(BffGuvenlikFilter))]
[EnableRateLimiting("yonetim-parcalar")]
public class YonetimParcalarKontrolcu : ControllerBase
{
    private readonly KonfiguratorDbContext _db;
    private readonly IValidator<ParcaSenkronizeKomutu> _senkronizeDogrulayici;
    private readonly IValidator<ParcaMetadataGuncelleDto> _metadataDogrulayici;

    public YonetimParcalarKontrolcu(
        KonfiguratorDbContext db,
        IValidator<ParcaSenkronizeKomutu> senkronizeDogrulayici,
        IValidator<ParcaMetadataGuncelleDto> metadataDogrulayici)
    {
        _db = db;
        _senkronizeDogrulayici = senkronizeDogrulayici;
        _metadataDogrulayici = metadataDogrulayici;
    }

    /// <summary>
    /// GET: Admin parça listesi — silinmiş kayıtları da içerir.
    /// </summary>
    [HttpGet]
    public async Task<KonfiguratorCevap<List<UcBoyutModelParcasiYonetimDto>>> ListeleAsync(
        int modelId,
        CancellationToken iptal = default)
    {
        if (modelId <= 0)
            return KonfiguratorCevap<List<UcBoyutModelParcasiYonetimDto>>.Hata("Geçersiz model kimliği.");

        var modelVarMi = await _db.UcBoyutModeller
            .AsNoTracking()
            .AnyAsync(m => m.Id == modelId, iptal);

        if (!modelVarMi)
            return KonfiguratorCevap<List<UcBoyutModelParcasiYonetimDto>>.Hata("Model bulunamadı.");

        var parcalar = await _db.UcBoyutModelParcalari
            .AsNoTracking()
            .Where(p => p.ModelId == modelId)
            .OrderBy(p => p.ParcaTuru)
            .ThenBy(p => p.MeshAdi)
            .Select(p => new UcBoyutModelParcasiYonetimDto(
                p.Id,
                p.ModelId,
                p.MeshAdi,
                p.GorunenAd,
                p.ParcaTuru.ToString(),
                p.RenkDegistirilebilirMi,
                p.GorunurMu,
                p.VarsayilanRenk,
                p.VarsayilanMalzeme,
                p.OlusturulmaTarihi,
                p.GuncellenmeTarihi
            ))
            .ToListAsync(iptal);

        return KonfiguratorCevap<List<UcBoyutModelParcasiYonetimDto>>.Basarili(parcalar);
    }

    /// <summary>
    /// POST: İstemcide keşfedilen mesh adlarını senkronize eder.
    /// Yeni mesh'ler eklenir (varsayılan değerlerle), listede olmayanlar soft-delete yapılır.
    /// ASLA fiziksel silme yapılmaz. Parça türü tahmini yapılmaz (her şey Diger).
    /// </summary>
    [HttpPost("senkronize")]
    public async Task<KonfiguratorCevap<SenkronizeSonucDto>> SenkronizeAsync(
        int modelId,
        [FromBody] ParcaSenkronizeKomutu komut,
        CancellationToken iptal = default)
    {
        // Path validation
        if (modelId <= 0)
            return KonfiguratorCevap<SenkronizeSonucDto>.Hata("Geçersiz model kimliği.");

        // FluentValidation
        var dogrulamaSonucu = await _senkronizeDogrulayici.ValidateAsync(komut, iptal);
        if (!dogrulamaSonucu.IsValid)
        {
            var hatalar = dogrulamaSonucu.Errors.Select(x => x.ErrorMessage).ToList();
            return KonfiguratorCevap<SenkronizeSonucDto>.Hata(string.Join(" ", hatalar));
        }

        // Model varlık kontrolü (silinmiş modeller query filter ile zaten gelmez)
        var modelVarMi = await _db.UcBoyutModeller
            .AnyAsync(m => m.Id == modelId, iptal);

        if (!modelVarMi)
            return KonfiguratorCevap<SenkronizeSonucDto>.Hata("Model bulunamadı.");

        // Mevcut parçaları getir (silinmişler dahil)
        var mevcutParcalar = await _db.UcBoyutModelParcalari
            .IgnoreQueryFilters()
            .Where(p => p.ModelId == modelId)
            .ToListAsync(iptal);

        var gelenMeshAdlari = komut.MeshAdlari
            .Select(m => m.Trim())
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        int eklenen = 0;
        int geriYuklenen = 0;
        int yumusakSilinen = 0;

        // 1. Yeni mesh'leri ekle (veya silinmişse geri yükle)
        foreach (var meshAdi in gelenMeshAdlari)
        {
            var mevcut = mevcutParcalar
                .FirstOrDefault(p => string.Equals(p.MeshAdi, meshAdi, StringComparison.OrdinalIgnoreCase));

            if (mevcut is null)
            {
                // Yeni kayıt — varsayılan değerlerle ekle
                _db.UcBoyutModelParcalari.Add(new UcBoyutModelParcasi
                {
                    ModelId = modelId,
                    MeshAdi = meshAdi,
                    GorunenAd = meshAdi,           // varsayılan: mesh adı
                    ParcaTuru = ParcaTuru.Diger,    // tür tahmini YAPILMAZ
                    RenkDegistirilebilirMi = false, // varsayılan: kapalı
                    GorunurMu = true,               // varsayılan: görünür
                    OlusturulmaTarihi = DateTime.UtcNow
                });
                eklenen++;
            }
            else if (mevcut.SilindiMi)
            {
                // Daha önce silinmiş — geri yükle
                mevcut.SilindiMi = false;
                mevcut.SilinmeTarihi = null;
                mevcut.GuncellenmeTarihi = DateTime.UtcNow;
                geriYuklenen++;
            }
        }

        // 2. Gönderilmeyen mesh'leri soft-delete yap
        var silinecekler = mevcutParcalar
            .Where(p => !p.SilindiMi &&
                        !gelenMeshAdlari.Any(g =>
                            string.Equals(g, p.MeshAdi, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var parca in silinecekler)
        {
            parca.SilindiMi = true;
            parca.SilinmeTarihi = DateTime.UtcNow;
            parca.GuncellenmeTarihi = DateTime.UtcNow;
            yumusakSilinen++;
        }

        await _db.SaveChangesAsync(iptal);

        var sonuc = new SenkronizeSonucDto(eklenen, geriYuklenen, yumusakSilinen);
        return KonfiguratorCevap<SenkronizeSonucDto>.Basarili(sonuc, "Senkronizasyon tamamlandı.");
    }

    /// <summary>
    /// PUT: Tek bir parçanın metadata'sını günceller.
    /// </summary>
    [HttpPut("{parcaId}")]
    public async Task<KonfiguratorCevap<UcBoyutModelParcasiYonetimDto>> MetadataGuncelleAsync(
        int modelId,
        int parcaId,
        [FromBody] ParcaMetadataGuncelleDto dto,
        CancellationToken iptal = default)
    {
        // Path validation
        if (modelId <= 0)
            return KonfiguratorCevap<UcBoyutModelParcasiYonetimDto>.Hata("Geçersiz model kimliği.");

        if (parcaId <= 0)
            return KonfiguratorCevap<UcBoyutModelParcasiYonetimDto>.Hata("Geçersiz parça kimliği.");

        // FluentValidation
        var dogrulamaSonucu = await _metadataDogrulayici.ValidateAsync(dto, iptal);
        if (!dogrulamaSonucu.IsValid)
        {
            var hatalar = dogrulamaSonucu.Errors.Select(x => x.ErrorMessage).ToList();
            return KonfiguratorCevap<UcBoyutModelParcasiYonetimDto>.Hata(string.Join(" ", hatalar));
        }

        // Parçayı bul (query filter: SilindiMi=false)
        var parca = await _db.UcBoyutModelParcalari
            .FirstOrDefaultAsync(p => p.Id == parcaId && p.ModelId == modelId, iptal);

        if (parca is null)
            return KonfiguratorCevap<UcBoyutModelParcasiYonetimDto>.Hata("Parça bulunamadı.");

        // Sadece gönderilen alanları güncelle
        if (dto.GorunenAd is not null)
            parca.GorunenAd = dto.GorunenAd;

        if (dto.ParcaTuru is not null && Enum.TryParse<ParcaTuru>(dto.ParcaTuru, true, out var tur))
            parca.ParcaTuru = tur;

        if (dto.RenkDegistirilebilirMi.HasValue)
            parca.RenkDegistirilebilirMi = dto.RenkDegistirilebilirMi.Value;

        if (dto.GorunurMu.HasValue)
            parca.GorunurMu = dto.GorunurMu.Value;

        if (dto.VarsayilanRenk is not null)
            parca.VarsayilanRenk = dto.VarsayilanRenk;

        if (dto.VarsayilanMalzeme is not null)
            parca.VarsayilanMalzeme = dto.VarsayilanMalzeme;

        parca.GuncellenmeTarihi = DateTime.UtcNow;
        await _db.SaveChangesAsync(iptal);

        var sonucDto = new UcBoyutModelParcasiYonetimDto(
            parca.Id,
            parca.ModelId,
            parca.MeshAdi,
            parca.GorunenAd,
            parca.ParcaTuru.ToString(),
            parca.RenkDegistirilebilirMi,
            parca.GorunurMu,
            parca.VarsayilanRenk,
            parca.VarsayilanMalzeme,
            parca.OlusturulmaTarihi,
            parca.GuncellenmeTarihi
        );

        return KonfiguratorCevap<UcBoyutModelParcasiYonetimDto>.Basarili(sonucDto, "Parça güncellendi.");
    }
}
