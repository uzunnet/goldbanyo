using MediatR;
using Microsoft.AspNetCore.Mvc;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu;

/// <summary>
/// Analytics olay kayıt endpoint'i.
/// Public/anonymous erişime açık, tenant domain middleware ile izole edilir.
/// Roomle veya başka rakip SDK/API KULLANILMAZ — tamamen kendi implementasyonumuz.
/// IP otomatik anonimleştirilir, hassas veri kaydedilmez.
/// </summary>
[ApiController]
[Route("api/konfigurasyon/analitik")]
public class AnalitikKontrolcu(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Konfigüratör kullanıcı etkileşim olayını kaydeder.
    /// Olay tipleri: SayfaGoruntulendi, ParcaSecildi, RenkDegisti,
    /// MalzemeDegisti, KaplamaDegisti, TeklifIstendi,
    /// ModelYuklendi, ModelHatasi, EmbedAcildi
    /// </summary>
    [HttpPost("olay-kaydet")]
    public async Task<Cevap<int>> OlayKaydet(
        [FromBody] AnalitikKomutlari.OlayKaydetKomutu komut)
    {
        var dogrulayici = new OlayKaydetDogrulayici();
        var dogrulamaSonucu = await dogrulayici.ValidateAsync(komut);
        if (!dogrulamaSonucu.IsValid)
        {
            var hataMesaji = string.Join("; ", dogrulamaSonucu.Errors.Select(e => e.ErrorMessage));
            return Cevap<int>.Hata(hataMesaji);
        }

        // Client IP ve User-Agent bilgisini backend'te al (istemciden güvenme)
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var tarayici = Request.Headers.UserAgent.ToString();

        var guvenliKomut = komut with
        {
            KullaniciIp = komut.KullaniciIp ?? ip,
            TarayiciBilgisi = komut.TarayiciBilgisi ?? tarayici
        };

        return await mediator.Send(guvenliKomut);
    }
}
