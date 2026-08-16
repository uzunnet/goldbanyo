using Microsoft.EntityFrameworkCore;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Api.AraYazilimlar;

/// <summary>
/// Konfigurator API tenant çözümleme middleware'i.
/// Domain, X-Firma başlığı veya query parametresi ile firma tespiti yapar.
/// HttpContext.Items'a FirmaId, FirmaSlug, FirmaAd bilgilerini yazar.
///
/// SaaS modunda: tenant bazlı izolasyon aktif.
/// Tek firma modunda: tüm istekler varsayılan firmaya ait kabul edilir.
/// </summary>
public class KonfiguratorFirmaCozumlemeMiddleware
{
    private readonly RequestDelegate _sonraki;
    private readonly IConfiguration _konfigurasyon;

    public KonfiguratorFirmaCozumlemeMiddleware(RequestDelegate sonraki, IConfiguration konfigurasyon)
    {
        _sonraki = sonraki;
        _konfigurasyon = konfigurasyon;
    }

    public async Task InvokeAsync(HttpContext baglam)
    {
        var multiTenantAktif = _konfigurasyon.GetValue<bool>("SaaS:MultiTenantAktif", false);
        var varsayilanFirmaId = _konfigurasyon.GetValue<int?>("SaaS:VarsayilanFirmaId");
        var varsayilanFirmaSlug = _konfigurasyon.GetValue<string>("SaaS:VarsayilanFirmaSlug") ?? "goldbanyo";
        var varsayilanFirmaAd = _konfigurasyon.GetValue<string>("SaaS:VarsayilanFirmaAd") ?? "Gold Banyo";

        if (!multiTenantAktif)
        {
            // Tek firma modu — varsayılan firmayı kullan
            if (varsayilanFirmaId.HasValue)
            {
                baglam.Items["FirmaId"] = varsayilanFirmaId.Value;
                baglam.Items["FirmaSlug"] = varsayilanFirmaSlug;
                baglam.Items["FirmaAd"] = varsayilanFirmaAd;
            }
            await _sonraki(baglam);
            return;
        }

        // Multi-tenant modu: önce X-Firma başlığını kontrol et
        var firmaSlug = baglam.Request.Headers["X-Firma"].FirstOrDefault();

        // Sonra query parametresini kontrol et (dev ortam için)
        if (string.IsNullOrEmpty(firmaSlug))
        {
            firmaSlug = baglam.Request.Query["firma"].FirstOrDefault();
        }

        // Domain çözümleme
        if (string.IsNullOrEmpty(firmaSlug))
        {
            var host = baglam.Request.Host.Host.ToLowerInvariant();
            var localhostMu = host is "localhost" or "127.0.0.1" or "::1";

            if (!localhostMu)
            {
                // Domain bazlı firma arama
                using var kapsam = baglam.RequestServices.CreateScope();
                var vt = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

                var firma = await vt.Firmalar
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f =>
                        (f.Domain == host || f.YedekDomain == host) && f.AktifMi);

                if (firma != null)
                {
                    baglam.Items["FirmaId"] = firma.Id;
                    baglam.Items["FirmaSlug"] = firma.Slug;
                    baglam.Items["FirmaAd"] = firma.Ad;

                    await _sonraki(baglam);
                    return;
                }
            }
        }

        // FirmaSlug ile arama (header/query'den gelen)
        if (!string.IsNullOrEmpty(firmaSlug))
        {
            using var kapsam = baglam.RequestServices.CreateScope();
            var vt = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

            var firma = await vt.Firmalar
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Slug == firmaSlug && f.AktifMi);

            if (firma != null)
            {
                baglam.Items["FirmaId"] = firma.Id;
                baglam.Items["FirmaSlug"] = firma.Slug;
                baglam.Items["FirmaAd"] = firma.Ad;

                await _sonraki(baglam);
                return;
            }
        }

        // Fallback: varsayılan firma
        if (varsayilanFirmaId.HasValue)
        {
            baglam.Items["FirmaId"] = varsayilanFirmaId.Value;
            baglam.Items["FirmaSlug"] = varsayilanFirmaSlug;
            baglam.Items["FirmaAd"] = varsayilanFirmaAd;
        }

        await _sonraki(baglam);
    }
}
