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

/// <summary>
/// Firma bazlı yönetilebilir parça kategorileri için CRUD endpoint'leri.
/// ParcaTuru enum'unun yerini alır — her firma kendi kategorilerini ekleyebilir.
/// </summary>
[ApiController]
[Route("api/yonetim/parca-kategorileri")]
[ServiceFilter(typeof(BffGuvenlikFilter))]
[EnableRateLimiting("yonetim")]
public class ParcaKategorisiKontrolcu : ControllerBase
{
    private readonly KonfiguratorDbContext _db;
    private readonly IValidator<ParcaKategorisiKaydetDto> _dogrulayici;
    private readonly KonfiguratorKiraciServisi _kiraciServisi;

    public ParcaKategorisiKontrolcu(
        KonfiguratorDbContext db,
        IValidator<ParcaKategorisiKaydetDto> dogrulayici,
        KonfiguratorKiraciServisi kiraciServisi)
    {
        _db = db;
        _dogrulayici = dogrulayici;
        _kiraciServisi = kiraciServisi;
    }

    /// <summary>
    /// GET: Firmanın tüm parça kategorilerini listeler.
    /// Tenant izolasyonu uygulanır.
    /// </summary>
    [HttpGet]
    public async Task<KonfiguratorCevap<List<ParcaKategorisiDto>>> ListeleAsync(CancellationToken iptal = default)
    {
        var sorgu = _db.ParcaKategorileri.AsNoTracking();

        // Tenant izolasyonu: sadece kendi firmasının veya sistem geneli kategorileri
        if (_kiraciServisi.TenantAktifMi)
        {
            var firmaId = _kiraciServisi.MevcutFirmaId!.Value;
            sorgu = sorgu.Where(k => k.FirmaId == null || k.FirmaId == firmaId);
        }

        var kategoriler = await sorgu
            .OrderBy(k => k.SiraNo)
            .ThenBy(k => k.Ad)
            .Select(k => new ParcaKategorisiDto(
                k.Id,
                k.Ad,
                k.Aciklama,
                k.AktifMi,
                k.SiraNo,
                k.OlusturulmaTarihi,
                k.GuncellenmeTarihi
            ))
            .ToListAsync(iptal);

        return KonfiguratorCevap<List<ParcaKategorisiDto>>.Basarili(kategoriler);
    }

    /// <summary>
    /// POST: Yeni parça kategorisi oluşturur.
    /// Kategori otomatik olarak mevcut firmaya atanır.
    /// </summary>
    [HttpPost]
    public async Task<KonfiguratorCevap<ParcaKategorisiDto>> EkleAsync(
        [FromBody] ParcaKategorisiKaydetDto dto,
        CancellationToken iptal = default)
    {
        var dogrulamaSonucu = await _dogrulayici.ValidateAsync(dto, iptal);
        if (!dogrulamaSonucu.IsValid)
        {
            var hatalar = dogrulamaSonucu.Errors.Select(x => x.ErrorMessage).ToList();
            return KonfiguratorCevap<ParcaKategorisiDto>.Hata(string.Join(" ", hatalar));
        }

        // Aynı isimde kategori var mı kontrol et (tenant scope içinde)
        var firmaId = _kiraciServisi.MevcutFirmaId;
        var mevcutVarMi = await _db.ParcaKategorileri
            .AnyAsync(k => k.Ad == dto.Ad && k.FirmaId == firmaId, iptal);

        if (mevcutVarMi)
            return KonfiguratorCevap<ParcaKategorisiDto>.Hata("Bu isimde bir kategori zaten mevcut.");

        var kategori = new ParcaKategorisi
        {
            Ad = dto.Ad,
            Aciklama = dto.Aciklama,
            AktifMi = dto.AktifMi,
            SiraNo = dto.SiraNo,
            FirmaId = firmaId,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        _db.ParcaKategorileri.Add(kategori);
        await _db.SaveChangesAsync(iptal);

        var sonucDto = new ParcaKategorisiDto(
            kategori.Id,
            kategori.Ad,
            kategori.Aciklama,
            kategori.AktifMi,
            kategori.SiraNo,
            kategori.OlusturulmaTarihi,
            kategori.GuncellenmeTarihi
        );

        return KonfiguratorCevap<ParcaKategorisiDto>.Basarili(sonucDto, "Kategori eklendi.");
    }

    /// <summary>
    /// PUT: Kategori günceller.
    /// Sadece kendi firmasının kategorileri güncellenebilir.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<KonfiguratorCevap<ParcaKategorisiDto>> GuncelleAsync(
        int id,
        [FromBody] ParcaKategorisiKaydetDto dto,
        CancellationToken iptal = default)
    {
        if (id <= 0)
            return KonfiguratorCevap<ParcaKategorisiDto>.Hata("Geçersiz kategori kimliği.");

        var dogrulamaSonucu = await _dogrulayici.ValidateAsync(dto, iptal);
        if (!dogrulamaSonucu.IsValid)
        {
            var hatalar = dogrulamaSonucu.Errors.Select(x => x.ErrorMessage).ToList();
            return KonfiguratorCevap<ParcaKategorisiDto>.Hata(string.Join(" ", hatalar));
        }

        var kategori = await _db.ParcaKategorileri.FindAsync(new object[] { id }, iptal);
        if (kategori is null)
            return KonfiguratorCevap<ParcaKategorisiDto>.Hata("Kategori bulunamadı.");

        // Tenant izolasyonu: sadece kendi firmasının kategorisi güncellenebilir
        if (_kiraciServisi.TenantAktifMi &&
            kategori.FirmaId.HasValue &&
            kategori.FirmaId != _kiraciServisi.MevcutFirmaId)
            return KonfiguratorCevap<ParcaKategorisiDto>.Izinsiz("Bu kategoriye erişim yetkiniz yok.");

        // Aynı isimde başka bir kategori var mı?
        var firmaId = _kiraciServisi.MevcutFirmaId;
        var isimCakisiyor = await _db.ParcaKategorileri
            .AnyAsync(k => k.Ad == dto.Ad && k.FirmaId == firmaId && k.Id != id, iptal);

        if (isimCakisiyor)
            return KonfiguratorCevap<ParcaKategorisiDto>.Hata("Bu isimde başka bir kategori zaten mevcut.");

        kategori.Ad = dto.Ad;
        kategori.Aciklama = dto.Aciklama;
        kategori.AktifMi = dto.AktifMi;
        kategori.SiraNo = dto.SiraNo;
        kategori.GuncellenmeTarihi = DateTime.UtcNow;

        await _db.SaveChangesAsync(iptal);

        var sonucDto = new ParcaKategorisiDto(
            kategori.Id,
            kategori.Ad,
            kategori.Aciklama,
            kategori.AktifMi,
            kategori.SiraNo,
            kategori.OlusturulmaTarihi,
            kategori.GuncellenmeTarihi
        );

        return KonfiguratorCevap<ParcaKategorisiDto>.Basarili(sonucDto, "Kategori güncellendi.");
    }

    /// <summary>
    /// DELETE: Kategoriyi soft-delete yapar.
    /// Bu kategoriyi kullanan parçalar etkilenmez (ParcaKategoriId kalır).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<KonfiguratorCevap<bool>> SilAsync(int id, CancellationToken iptal = default)
    {
        if (id <= 0)
            return KonfiguratorCevap<bool>.Hata("Geçersiz kategori kimliği.");

        var kategori = await _db.ParcaKategorileri.FindAsync(new object[] { id }, iptal);
        if (kategori is null)
            return KonfiguratorCevap<bool>.Hata("Kategori bulunamadı.");

        // Tenant izolasyonu
        if (_kiraciServisi.TenantAktifMi &&
            kategori.FirmaId.HasValue &&
            kategori.FirmaId != _kiraciServisi.MevcutFirmaId)
            return KonfiguratorCevap<bool>.Izinsiz("Bu kategoriye erişim yetkiniz yok.");

        // Soft delete
        kategori.SilindiMi = true;
        kategori.SilinmeTarihi = DateTime.UtcNow;
        kategori.GuncellenmeTarihi = DateTime.UtcNow;

        await _db.SaveChangesAsync(iptal);

        return KonfiguratorCevap<bool>.Basarili(true, "Kategori silindi.");
    }
}
