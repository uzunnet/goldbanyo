using System.Text.Json.Serialization;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;

/// <summary>
/// Konfigurator SaaS için firma (tenant) entity'si.
/// Ana API'deki Firma tablosundan bağımsız, Konfigurator'e özel hafif tanımdır.
/// Tenant izolasyonu için gereklidir.
/// </summary>
public class KonfiguratorFirma
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? YedekDomain { get; set; }
    public bool AktifMi { get; set; } = true;

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    // Soft delete
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
