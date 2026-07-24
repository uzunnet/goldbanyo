using System.Text.Json.Serialization;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Modeller;

public class SifreSifirlamaIstegi
{
    public int Id { get; set; }
    public int KullaniciId { get; set; }

    [JsonIgnore]
    public KonfiguratorKullanicisi? Kullanici { get; set; }

    [JsonIgnore]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime BitisTarihi { get; set; }
    public bool KullanildiMi { get; set; }
    public DateTime? KullanilmaTarihi { get; set; }

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    // Soft delete
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
