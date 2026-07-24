using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VizitLink3D.Konfigurator.Api.AraYazilimlar;

public class BffGuvenlikFilter : IAsyncActionFilter
{
    private readonly string? _beklenenAnahtar;

    public BffGuvenlikFilter(IConfiguration configuration)
    {
        _beklenenAnahtar = configuration["BffGuvenlik:Anahtar"];
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Yapılandırma eksik → 503
        if (string.IsNullOrWhiteSpace(_beklenenAnahtar))
        {
            context.Result = new ObjectResult(new
            {
                Mesaj = "BFF güvenlik yapılandırması eksik."
            })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
            return;
        }

        // Header eksik → 401
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Konfigurator-Bff-Anahtari", out var gelenAnahtar))
        {
            context.Result = new ObjectResult(new
            {
                Mesaj = "Yetkisiz erişim."
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        // Yanlış anahtar → 401 (sabit zamanlı karşılaştırma olmasa da secret loglanmaz)
        if (!string.Equals(gelenAnahtar.ToString(), _beklenenAnahtar, StringComparison.Ordinal))
        {
            context.Result = new ObjectResult(new
            {
                Mesaj = "Yetkisiz erişim."
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        await next();
    }
}
