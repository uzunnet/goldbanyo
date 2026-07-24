using Microsoft.AspNetCore.Mvc;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;

namespace VizitLink3D.Konfigurator.Api.Moduller.Sistem;

[ApiController]
[Route("")]
public class SaglikKontrolcu : ControllerBase
{
    [HttpGet("saglik")]
    public KonfiguratorCevap<string> Saglik() =>
        KonfiguratorCevap<string>.Basarili("Calisiyor", "API saglikli.");
}
