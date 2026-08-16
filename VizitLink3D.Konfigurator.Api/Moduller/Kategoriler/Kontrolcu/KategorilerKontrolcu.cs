using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Konfigurator.Api.AraYazilimlar;
using VizitLink3D.Konfigurator.Api.Moduller.Kategoriler.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Kategoriler.Modeller;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kategoriler.Kontrolcu;

[ApiController]
[Route("api/yonetim/kategoriler")]
[ServiceFilter(typeof(BffGuvenlikFilter))]
[EnableRateLimiting("yonetim")]
public class KategorilerKontrolcu : ControllerBase
{
    private readonly KonfiguratorDbContext _db;
    private readonly IValidator<KategoriOlusturDto> _olusturValidator;
    private readonly IValidator<KategoriGuncelleDto> _guncelleValidator;

    public KategorilerKontrolcu(
        KonfiguratorDbContext db,
        IValidator<KategoriOlusturDto> olusturValidator,
        IValidator<KategoriGuncelleDto> guncelleValidator)
    {
        _db = db;
        _olusturValidator = olusturValidator;
        _guncelleValidator = guncelleValidator;
    }

    /// <summary>
    /// GET: Ağaç yapısında tüm kategorileri döndürür (alt kategoriler iç içe).
    /// </summary>
    [HttpGet]
    public async Task<KonfiguratorCevap<List<KategoriDto>>> AgacGetirAsync(CancellationToken iptal = default)
    {
        var tumKategoriler = await _db.Kategoriler
            .AsNoTracking()
            .OrderBy(k => k.Sira)
            .ThenBy(k => k.Ad)
            .ToListAsync(iptal);

        var agac = AgacOlustur(tumKategoriler, null);
        return KonfiguratorCevap<List<KategoriDto>>.Basarili(agac);
    }

    /// <summary>
    /// POST: Yeni kategori oluşturur.
    /// </summary>
    [HttpPost]
    public async Task<KonfiguratorCevap<KategoriDto>> EkleAsync(
        [FromBody] KategoriOlusturDto dto,
        CancellationToken iptal = default)
    {
        var dogrulamaSonucu = await _olusturValidator.ValidateAsync(dto, iptal);
        if (!dogrulamaSonucu.IsValid)
        {
            var hatalar = dogrulamaSonucu.Errors.Select(x => x.ErrorMessage).ToList();
            return KonfiguratorCevap<KategoriDto>.Hata(string.Join(" ", hatalar));
        }

        // Ust kategori kontrolu
        if (dto.UstKategoriId.HasValue)
        {
            var ustKategoriVarMi = await _db.Kategoriler
                .AnyAsync(k => k.Id == dto.UstKategoriId.Value, iptal);
            if (!ustKategoriVarMi)
                return KonfiguratorCevap<KategoriDto>.Hata("Üst kategori bulunamadı.");
        }

        var slug = await SlugOlusturAsync(dto.Ad, iptal);

        var kategori = new Kategori
        {
            Ad = dto.Ad,
            Slug = slug,
            Aciklama = dto.Aciklama,
            UstKategoriId = dto.UstKategoriId,
            Sira = dto.Sira,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        _db.Kategoriler.Add(kategori);
        await _db.SaveChangesAsync(iptal);

        var sonucDto = new KategoriDto(
            kategori.Id,
            kategori.Ad,
            kategori.Slug,
            kategori.Aciklama,
            kategori.UstKategoriId,
            kategori.Sira,
            kategori.AktifMi,
            null
        );

        return KonfiguratorCevap<KategoriDto>.Basarili(sonucDto, "Kategori oluşturuldu.");
    }

    /// <summary>
    /// PUT: Kategori günceller.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<KonfiguratorCevap<KategoriDto>> GuncelleAsync(
        int id,
        [FromBody] KategoriGuncelleDto dto,
        CancellationToken iptal = default)
    {
        if (id <= 0)
            return KonfiguratorCevap<KategoriDto>.Hata("Geçersiz kategori kimliği.");

        var dogrulamaSonucu = await _guncelleValidator.ValidateAsync(dto, iptal);
        if (!dogrulamaSonucu.IsValid)
        {
            var hatalar = dogrulamaSonucu.Errors.Select(x => x.ErrorMessage).ToList();
            return KonfiguratorCevap<KategoriDto>.Hata(string.Join(" ", hatalar));
        }

        var kategori = await _db.Kategoriler.FindAsync(new object[] { id }, iptal);
        if (kategori is null)
            return KonfiguratorCevap<KategoriDto>.Hata("Kategori bulunamadı.");

        // Kendini üst kategori olarak seçemez
        if (dto.UstKategoriId.HasValue && dto.UstKategoriId.Value == id)
            return KonfiguratorCevap<KategoriDto>.Hata("Bir kategori kendisinin üst kategorisi olamaz.");

        // Dairesel referans kontrolü
        if (dto.UstKategoriId.HasValue)
        {
            var daireselVarMi = await DaireselReferansKontroluAsync(id, dto.UstKategoriId.Value, iptal);
            if (daireselVarMi)
                return KonfiguratorCevap<KategoriDto>.Hata("Bu üst kategori seçimi dairesel referans oluşturur.");
        }

        // Slug güncelle (ad değiştiyse)
        if (!string.Equals(kategori.Ad, dto.Ad, StringComparison.Ordinal))
        {
            kategori.Slug = await SlugOlusturAsync(dto.Ad, iptal, kategori.Id);
        }

        kategori.Ad = dto.Ad;
        kategori.Aciklama = dto.Aciklama;
        kategori.UstKategoriId = dto.UstKategoriId;
        kategori.Sira = dto.Sira;
        kategori.AktifMi = dto.AktifMi;
        kategori.GuncellenmeTarihi = DateTime.UtcNow;

        await _db.SaveChangesAsync(iptal);

        var sonucDto = new KategoriDto(
            kategori.Id,
            kategori.Ad,
            kategori.Slug,
            kategori.Aciklama,
            kategori.UstKategoriId,
            kategori.Sira,
            kategori.AktifMi,
            null
        );

        return KonfiguratorCevap<KategoriDto>.Basarili(sonucDto, "Kategori güncellendi.");
    }

    /// <summary>
    /// DELETE: Soft delete — SilindiMi = true.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<KonfiguratorCevap<bool>> SilAsync(int id, CancellationToken iptal = default)
    {
        if (id <= 0)
            return KonfiguratorCevap<bool>.Hata("Geçersiz kategori kimliği.");

        var kategori = await _db.Kategoriler.FindAsync(new object[] { id }, iptal);
        if (kategori is null)
            return KonfiguratorCevap<bool>.Hata("Kategori bulunamadı.");

        // Alt kategorileri olan kategori silinemez
        var altKategoriVarMi = await _db.Kategoriler
            .AnyAsync(k => k.UstKategoriId == id, iptal);
        if (altKategoriVarMi)
            return KonfiguratorCevap<bool>.Hata("Alt kategorileri olan bir kategori silinemez. Önce alt kategorileri silin veya taşıyın.");

        kategori.SilindiMi = true;
        kategori.GuncellenmeTarihi = DateTime.UtcNow;

        await _db.SaveChangesAsync(iptal);

        return KonfiguratorCevap<bool>.Basarili(true, "Kategori silindi.");
    }

    // ───── Yardımcı metodlar ─────

    private static List<KategoriDto> AgacOlustur(List<Kategori> tumKategoriler, int? ustId)
    {
        return tumKategoriler
            .Where(k => k.UstKategoriId == ustId)
            .Select(k => new KategoriDto(
                k.Id,
                k.Ad,
                k.Slug,
                k.Aciklama,
                k.UstKategoriId,
                k.Sira,
                k.AktifMi,
                AgacOlustur(tumKategoriler, k.Id)
            ))
            .ToList();
    }

    private async Task<bool> DaireselReferansKontroluAsync(int kategoriId, int hedefUstId, CancellationToken iptal)
    {
        var mevcutId = hedefUstId;
        var ziyaretEdilenler = new HashSet<int> { kategoriId };

        while (true)
        {
            if (mevcutId == kategoriId)
                return true;

            if (!ziyaretEdilenler.Add(mevcutId))
                return true; // Döngü tespiti

            var ustKategori = await _db.Kategoriler
                .AsNoTracking()
                .Where(k => k.Id == mevcutId)
                .Select(k => k.UstKategoriId)
                .FirstOrDefaultAsync(iptal);

            if (!ustKategori.HasValue)
                return false;

            mevcutId = ustKategori.Value;
        }
    }

    private async Task<string> SlugOlusturAsync(string ad, CancellationToken iptal, int? haricId = null)
    {
        var slug = ad.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
            slug = "kategori";

        var orijinalSlug = slug;
        var sayac = 1;

        var sorgu = _db.Kategoriler.AsNoTracking();
        if (haricId.HasValue)
            sorgu = sorgu.Where(k => k.Id != haricId.Value);

        while (await sorgu.AnyAsync(k => k.Slug == slug, iptal))
        {
            slug = orijinalSlug + "-" + sayac;
            sayac++;
        }

        return slug;
    }
}
