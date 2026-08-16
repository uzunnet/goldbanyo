using MediatR;
using Microsoft.AspNetCore.Mvc;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu;

/// <summary>
/// Teklif isteği endpoint'leri (BOM içeren).
/// Anonim erişime açık, tenant domain middleware ile izole edilir.
/// </summary>
[ApiController]
[Route("api/konfigurasyon/teklif")]
public class TeklifKontrolcu(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Müşteri konfigürasyonundan teklif isteği oluşturur.
    /// BOM otomatik hesaplanır, KonfiguratorTeklif entity'si oluşturulur.
    /// </summary>
    [HttpPost("olustur")]
    public async Task<Cevap<TeklifYanitDto>> TeklifOlustur(
        [FromBody] TeklifIstegiOlusturDto dto)
    {
        var dogrulayici = new TeklifIstegiOlusturDogrulayici();
        var dogrulamaSonucu = await dogrulayici.ValidateAsync(dto);
        if (!dogrulamaSonucu.IsValid)
        {
            var hataMesaji = string.Join("; ", dogrulamaSonucu.Errors.Select(e => e.ErrorMessage));
            return Cevap<TeklifYanitDto>.Hata(hataMesaji);
        }

        var komut = new TeklifOlusturKomutu(
            dto.MusteriKonfigurasyonuId,
            dto.UrunId,
            dto.MusteriAdSoyad,
            dto.Eposta,
            dto.Telefon,
            dto.Not);

        return await mediator.Send(komut);
    }
}
