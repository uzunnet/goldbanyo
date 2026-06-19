using Desadoor.Api.Servisler;
using Desadoor.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Desadoor.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/goc")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class GocKontrolcu(KapakGocServisi kapakGocServisi, MedyaGocServisi medyaGocServisi) : ControllerBase
{
    [HttpPost("kapak-urun")]
    public async Task<Cevap<GocSonucu>> KapakGocu()
    {
        var sonuc = await kapakGocServisi.GogAsync();
        return sonuc;
    }

    [HttpPost("dosya-aktarimi")]
    public async Task<Cevap<AktarimSonucu>> DosyaAktarimi()
    {
        var sonuc = await medyaGocServisi.GocEtAsync();
        return sonuc;
    }
}
