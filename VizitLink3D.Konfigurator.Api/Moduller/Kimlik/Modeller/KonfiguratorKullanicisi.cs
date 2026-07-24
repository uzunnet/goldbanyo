using System.Text.Json.Serialization;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Modeller;

public class KonfiguratorKullanicisi
{
    public int Id { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;

    [JsonIgnore]
    public string SifreHash { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    // Soft delete
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
