using Desadoor.Api.VeriTabani;
using Microsoft.EntityFrameworkCore;

namespace Desadoor.Api.AraYazilimlar;

public class FirmaCozumlemeMiddleware(RequestDelegate sonraki)
{
    public async Task InvokeAsync(HttpContext baglam)
    {
        using var kapsam = baglam.RequestServices.CreateScope();
        var vt = kapsam.ServiceProvider.GetRequiredService<DesadoorDbContext>();

        var host = baglam.Request.Host.Host.ToLowerInvariant();

        // Geliştirme: query param ile override
        if (baglam.Request.Query.TryGetValue("firma", out var firmaSlug))
        {
            var gelistirmeFirma = await vt.Firmalar
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Slug == firmaSlug.ToString() && f.AktifMi);

            if (gelistirmeFirma != null)
            {
                baglam.Items["FirmaId"] = gelistirmeFirma.Id;
                baglam.Items["FirmaDomain"] = gelistirmeFirma.Domain;
                baglam.Items["FirmaSlug"] = gelistirmeFirma.Slug;
                baglam.Items["FirmaAd"] = gelistirmeFirma.Ad;
                await sonraki(baglam);
                return;
            }
        }

        // Üretim: domain eşleştirme
        if (host != "localhost" && host != "127.0.0.1")
        {
            var firma = await vt.Firmalar
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    (f.Domain == host || f.YedekDomain == host) && f.AktifMi);

            if (firma != null)
            {
                baglam.Items["FirmaId"] = firma.Id;
                baglam.Items["FirmaDomain"] = firma.Domain;
                baglam.Items["FirmaSlug"] = firma.Slug;
                baglam.Items["FirmaAd"] = firma.Ad;
                await sonraki(baglam);
                return;
            }
        }

        // Varsayılan firma (DesaDoor)
        var varsayilanFirma = await vt.Firmalar
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Slug == "desadoor" && f.AktifMi);

        if (varsayilanFirma != null)
        {
            baglam.Items["FirmaId"] = varsayilanFirma.Id;
            baglam.Items["FirmaDomain"] = varsayilanFirma.Domain;
            baglam.Items["FirmaSlug"] = varsayilanFirma.Slug;
            baglam.Items["FirmaAd"] = varsayilanFirma.Ad;
        }

        await sonraki(baglam);
    }
}
