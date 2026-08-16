using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu;

/// <summary>
/// SuperAdmin model onay endpoint'i.
/// Yalnız SuperAdmin rolü erişebilir.
/// Onaylanan model PublicKonfiguratorSorgusu'nda AdminOnayliMi=true filtresinden geçer.
/// </summary>
[ApiController]
[Route("api/konfigurasyon/model-onay")]
[Authorize(Roles = "SuperAdmin")]
public class ModelOnayKontrolcu(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Belirtilen 3D model sürümünü SuperAdmin onaylar.
    /// Onay sonrası model public konfigüratörde görünür hale gelir.
    /// </summary>
    [HttpPost("{modelId:int}/onayla")]
    public async Task<Cevap<bool>> Onayla(int modelId)
    {
        if (modelId <= 0)
            return Cevap<bool>.Hata("Geçersiz model ID.");

        var komut = new ModelOnaylaKomutu(modelId);
        return await mediator.Send(komut);
    }
}
