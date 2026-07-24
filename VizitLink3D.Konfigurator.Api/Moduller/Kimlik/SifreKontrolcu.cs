using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Servisler;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik;

[ApiController]
[Route("api/kimlik")]
public class SifreKontrolcu : ControllerBase
{
    private readonly SifreSifirlamaServisi _sifreSifirlamaServisi;

    public SifreKontrolcu(SifreSifirlamaServisi sifreSifirlamaServisi)
    {
        _sifreSifirlamaServisi = sifreSifirlamaServisi;
    }

    /// <summary>
    /// Şifre sıfırlama e-postası gönderir.
    /// Account enumeration önlemi: her zaman aynı başarılı yanıt döner.
    /// </summary>
    [HttpPost("sifre-sifirlama-istegi")]
    [EnableRateLimiting("sifre-sifirlama-istegi")]
    public async Task<KonfiguratorCevap<object>> SifreSifirlamaIstegi(SifreSifirlamaIstegiDto dto)
    {
        await _sifreSifirlamaServisi.SifreSifirlamaIstegiOlusturAsync(dto.Eposta);

        // Her zaman aynı generic başarılı yanıt (account enumeration engelleme)
        return KonfiguratorCevap<object>.Basarili(
            new { },
            "E-posta adresiniz sistemde kayitli ise, sifre sifirlama baglantisi gonderilmistir."
        );
    }

    /// <summary>
    /// Token ve yeni şifre ile şifre yenileme yapar.
    /// </summary>
    [HttpPost("sifre-yenile")]
    [EnableRateLimiting("sifre-yenile")]
    public async Task<KonfiguratorCevap<object>> SifreYenile(SifreYenileDto dto)
    {
        var basarili = await _sifreSifirlamaServisi.SifreYenileAsync(dto.Token, dto.YeniSifre);

        if (!basarili)
        {
            // Token geçersiz/süresi dolmuş → aynı generic hata (iç detay sızdırmaz)
            return KonfiguratorCevap<object>.Hata(
                "Sifre sifirlama baglantisi gecersiz veya suresi dolmus. Lutfen tekrar deneyin."
            );
        }

        return KonfiguratorCevap<object>.Basarili(
            new { },
            "Sifreniz basariyla yenilendi."
        );
    }
}
