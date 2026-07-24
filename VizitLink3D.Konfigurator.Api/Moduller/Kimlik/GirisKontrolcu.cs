using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik;

[ApiController]
[Route("api/kimlik")]
public class GirisKontrolcu : ControllerBase
{
    private readonly KonfiguratorDbContext _db;

    // P03-A: User enumeration/timing attack önlemi için sabit BCrypt hash.
    // Kullanıcı bulunamadığında bu fake hash ile Verify çalıştırılarak
    // zamanlama farkı minimize edilir.
    private static readonly string SabitFakeHash =
        "$2a$12$LJ3m4ys3GZfnYMz8k.3NteVhVPxjHyO9kJ6JUxE6GqLJwNx6BOPGq"; // BCrypt hash of "fake_fixed_salt_for_timing"

    public GirisKontrolcu(KonfiguratorDbContext db)
    {
        _db = db;
    }

    [HttpPost("giris")]
    [EnableRateLimiting("giris")]
    public async Task<KonfiguratorCevap<GirisCevapDto>> Giris(GirisDto dto)
    {
        var kullanici = await _db.Kullanicilar
            .FirstOrDefaultAsync(k => k.KullaniciAdi == dto.KullaniciAdi);

        bool basarili;

        if (kullanici is not null)
        {
            // Kullanıcı var → kendi hash'i ile doğrula
            basarili = BCrypt.Net.BCrypt.Verify(dto.Sifre, kullanici.SifreHash);
        }
        else
        {
            // Kullanıcı YOK → SABİT fake hash ile Verify çalıştır
            // Bu sayede "kullanıcı yok" ile "şifre yanlış" arasındaki
            // zamanlama farkı minimize edilir (user enumeration önlemi).
            BCrypt.Net.BCrypt.Verify(dto.Sifre, SabitFakeHash);
            basarili = false;
        }

        if (!basarili)
            return KonfiguratorCevap<GirisCevapDto>.Hata("Kullanici adi veya sifre hatali.");

        var cevap = new GirisCevapDto
        {
            KullaniciId = kullanici!.Id,
            KullaniciAdi = kullanici.KullaniciAdi,
            Rol = kullanici.Rol
        };

        return KonfiguratorCevap<GirisCevapDto>.Basarili(cevap, "Giris basarili.");
    }
}
