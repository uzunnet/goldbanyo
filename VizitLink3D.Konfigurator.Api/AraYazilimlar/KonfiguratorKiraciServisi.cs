namespace VizitLink3D.Konfigurator.Api.AraYazilimlar;

/// <summary>
/// Konfigurator SaaS tenant izolasyon servisi.
/// HttpContext.Items üzerinden mevcut firma bilgilerini sağlar.
/// FirmaCozumlemeMiddleware tarafından doldurulur.
/// </summary>
public class KonfiguratorKiraciServisi
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public KonfiguratorKiraciServisi(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? MevcutFirmaId =>
        _httpContextAccessor.HttpContext?.Items["FirmaId"] as int?;

    public string? MevcutFirmaSlug =>
        _httpContextAccessor.HttpContext?.Items["FirmaSlug"] as string;

    public string? MevcutFirmaAd =>
        _httpContextAccessor.HttpContext?.Items["FirmaAd"] as string;

    /// <summary>
    /// Multi-tenant aktif mi? Değilse tüm veriler görünür.
    /// </summary>
    public bool TenantAktifMi => MevcutFirmaId.HasValue;
}
