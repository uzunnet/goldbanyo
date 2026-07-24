using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Konfigurator.Api.AraYazilimlar;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dogrulayicilar;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Servisler;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Kontrolcu;

[ApiController]
[Route("api/yonetim/modeller")]
[ServiceFilter(typeof(BffGuvenlikFilter))]
public class YonetimModellerKontrolcu : ControllerBase
{
    private readonly KonfiguratorDbContext _db;
    private readonly GlbDosyaServisi _dosyaServisi;
    private readonly IConfiguration _configuration;
    private readonly IValidator<UcBoyutModelYukleKomutu> _dogrulayici;

    public YonetimModellerKontrolcu(
        KonfiguratorDbContext db,
        GlbDosyaServisi dosyaServisi,
        IConfiguration configuration,
        IValidator<UcBoyutModelYukleKomutu> dogrulayici)
    {
        _db = db;
        _dosyaServisi = dosyaServisi;
        _configuration = configuration;
        _dogrulayici = dogrulayici;
    }

    [HttpPost]
    [RequestSizeLimit(100_000_000)]
    [EnableRateLimiting("modelyukleme")]
    public async Task<KonfiguratorCevap<UcBoyutModelDto>> YukleAsync(
        [FromForm] string ad,
        [FromForm] string? aciklama,
        IFormFile dosya,
        CancellationToken iptal = default)
    {
        // FluentValidation ile metadata doğrulama (config tabanlı boyut sınırı dahil)
        var komut = new UcBoyutModelYukleKomutu(ad, aciklama, dosya);
        var dogrulamaSonucu = await _dogrulayici.ValidateAsync(komut, iptal);

        if (!dogrulamaSonucu.IsValid)
        {
            var hatalar = dogrulamaSonucu.Errors
                .Select(x => x.ErrorMessage)
                .ToList();
            return KonfiguratorCevap<UcBoyutModelDto>.Hata(string.Join(" ", hatalar));
        }

        // .glb extension kontrolü (FluentValidation'dakine ek olarak)
        var uzanti = Path.GetExtension(dosya.FileName);
        if (!string.Equals(uzanti, ".glb", StringComparison.OrdinalIgnoreCase))
            return KonfiguratorCevap<UcBoyutModelDto>.Hata("Sadece .glb uzantılı dosyalar kabul edilir.");

        // GLB 12-byte başlık doğrulaması (magic, version, total length)
        await using var kontrolAkisi = dosya.OpenReadStream();
        if (!_dosyaServisi.SihirliBaytDogrula(kontrolAkisi))
            return KonfiguratorCevap<UcBoyutModelDto>.Hata("Geçersiz GLB dosyası. Dosya glTF binary formatında değil veya başlık bilgisi hatalı.");

        // Dosya boyutu kontrolü (appsettings'ten okunur)
        var maxDosyaBoyutuMb = _configuration.GetValue<int>("GlbYukleme:MaxDosyaBoyutuMb", 30);
        var maxDosyaBoyutuBayt = maxDosyaBoyutuMb * 1024L * 1024L;
        if (dosya.Length > maxDosyaBoyutuBayt)
            return KonfiguratorCevap<UcBoyutModelDto>.Hata($"Dosya boyutu {maxDosyaBoyutuMb} MB'dan büyük olamaz.");

        string? dosyaYolu = null;

        try
        {
            // Güvenli kaydet
            var (kayitliAd, yol, boyut, hash) = await _dosyaServisi.KaydetAsync(dosya, iptal);
            dosyaYolu = yol;

            // Slug oluştur
            var slug = await SlugOlusturAsync(ad, iptal);

            var model = new UcBoyutModel
            {
                Ad = ad,
                Slug = slug,
                Aciklama = aciklama,
                DosyaAdi = dosya.FileName,
                DosyaYolu = dosyaYolu,
                IcerikTuru = "model/gltf-binary",
                BoyutBayt = boyut,
                Sha256Hash = hash,
                AktifMi = true,
                OlusturulmaTarihi = DateTime.UtcNow
            };

            _db.UcBoyutModeller.Add(model);
            await _db.SaveChangesAsync(iptal);

            var dto = new UcBoyutModelDto(
                model.Id,
                model.Ad,
                model.Slug,
                model.Aciklama,
                model.DosyaAdi,
                model.IcerikTuru,
                model.BoyutBayt,
                model.OlusturulmaTarihi,
                []
            );

            return KonfiguratorCevap<UcBoyutModelDto>.Basarili(dto, "Model başarıyla yüklendi.");
        }
        catch
        {
            // Başarısız upload'ta yeni oluşturulan dosyayı temizle
            if (dosyaYolu is not null)
                _dosyaServisi.Temizle(dosyaYolu);

            throw;
        }
    }

    [HttpGet]
    public async Task<KonfiguratorCevap<List<UcBoyutModelYonetimDto>>> ListeleAsync(CancellationToken iptal = default)
    {
        var modeller = await _db.UcBoyutModeller
            .AsNoTracking()
            .OrderByDescending(x => x.OlusturulmaTarihi)
            .Select(x => new UcBoyutModelYonetimDto(
                x.Id,
                x.Ad,
                x.Slug,
                x.Aciklama,
                x.BoyutBayt,
                x.AktifMi,
                x.OlusturulmaTarihi,
                x.GuncellenmeTarihi
            ))
            .ToListAsync(iptal);

        return KonfiguratorCevap<List<UcBoyutModelYonetimDto>>.Basarili(modeller);
    }

    [HttpPut("{id}/yayin-durumu")]
    [EnableRateLimiting("yonetim")]
    public async Task<KonfiguratorCevap<UcBoyutModelYonetimDto>> YayinDurumuGuncelleAsync(
        int id,
        [FromBody] YayinDurumuDto dto,
        [FromServices] IValidator<YayinDurumuDto> dogrulayici,
        CancellationToken iptal = default)
    {
        // Path validation: id pozitif olmalı
        if (id <= 0)
            return KonfiguratorCevap<UcBoyutModelYonetimDto>.Hata("Geçersiz model kimliği.");

        // FluentValidation
        var dogrulamaSonucu = await dogrulayici.ValidateAsync(dto, iptal);
        if (!dogrulamaSonucu.IsValid)
        {
            var hatalar = dogrulamaSonucu.Errors.Select(x => x.ErrorMessage).ToList();
            return KonfiguratorCevap<UcBoyutModelYonetimDto>.Hata(string.Join(" ", hatalar));
        }

        // Model bul (query filter: SilindiMi=false)
        var model = await _db.UcBoyutModeller.FindAsync(new object[] { id }, iptal);
        if (model is null)
            return KonfiguratorCevap<UcBoyutModelYonetimDto>.Hata("Model bulunamadı.");

        // AktifMi güncelle + audit
        model.AktifMi = dto.AktifMi;
        model.GuncellenmeTarihi = DateTime.UtcNow;
        await _db.SaveChangesAsync(iptal);

        var sonucDto = new UcBoyutModelYonetimDto(
            model.Id,
            model.Ad,
            model.Slug,
            model.Aciklama,
            model.BoyutBayt,
            model.AktifMi,
            model.OlusturulmaTarihi,
            model.GuncellenmeTarihi
        );
        return KonfiguratorCevap<UcBoyutModelYonetimDto>.Basarili(sonucDto, "Yayın durumu güncellendi.");
    }

    private async Task<string> SlugOlusturAsync(string ad, CancellationToken iptal = default)
    {
        var slug = ad.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
            slug = "model";

        var orijinalSlug = slug;
        var sayac = 1;

        while (await _db.UcBoyutModeller.AnyAsync(x => x.Slug == slug, iptal))
        {
            slug = orijinalSlug + "-" + sayac;
            sayac++;
        }

        return slug;
    }
}
