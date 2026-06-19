using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Desadoor.Api.Kontrolcüler.Core;

[ApiController]
[Route("api/health")]
public class SaglikKontrolcu : ControllerBase
{
    private readonly VeriTabani.DesadoorDbContext _vt;

    public SaglikKontrolcu(VeriTabani.DesadoorDbContext vt)
    {
        _vt = vt;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dbSaglikli = await _vt.Database.CanConnectAsync();
        return Ok(new
        {
            durum = dbSaglikli ? "sağlıklı" : "hata",
            veritabani = dbSaglikli,
            zaman = DateTime.UtcNow,
            surum = "1.0.0"
        });
    }
}
