using System.Text.Json.Serialization;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;

public class UcBoyutModel
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string DosyaAdi { get; set; } = string.Empty;

    [JsonIgnore]
    public string DosyaYolu { get; set; } = string.Empty;

    public string IcerikTuru { get; set; } = "model/gltf-binary";
    public long BoyutBayt { get; set; }

    [JsonIgnore]
    public string Sha256Hash { get; set; } = string.Empty;

    public bool AktifMi { get; set; } = true;

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    // Soft delete
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
