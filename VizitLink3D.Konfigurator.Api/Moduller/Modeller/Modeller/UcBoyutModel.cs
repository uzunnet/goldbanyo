using System.Text.Json.Serialization;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;

public class UcBoyutModel
{
    public int Id { get; set; }

    /// <summary>
    /// Multi-tenant izolasyon: Modelin ait olduğu firma. null ise sistem geneli.
    /// </summary>
    public int? FirmaId { get; set; }

    /// <summary>
    /// Modelin bağlı olduğu kategori. null ise kategorisiz modeldir.
    /// </summary>
    public int? KategoriId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Kategoriler.Modeller.Kategori? Kategori { get; set; }

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
