using System.Text.Json.Serialization;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;

public class UcBoyutModelParcasi
{
    public int Id { get; set; }

    public int ModelId { get; set; }

    [JsonIgnore]
    public UcBoyutModel? Model { get; set; }

    /// <summary>
    /// Multi-tenant izolasyon: Parcanin ait olduğu firma. null ise sistem geneli.
    /// </summary>
    public int? FirmaId { get; set; }

    /// <summary>
    /// Parcanin bağlı olduğu firma bazlı parça kategorisi. null ise kategorisiz.
    /// </summary>
    public int? ParcaKategoriId { get; set; }

    [JsonIgnore]
    public ParcaKategorisi? ParcaKategori { get; set; }

    public string MeshAdi { get; set; } = string.Empty;
    public string GorunenAd { get; set; } = string.Empty;
    public ParcaTuru ParcaTuru { get; set; } = ParcaTuru.Diger;
    public bool RenkDegistirilebilirMi { get; set; }
    public bool GorunurMu { get; set; } = true;

    /// <summary>
    /// Varsayılan renk (hex format, örn: "#C8952A"). Null ise modelin orijinal rengi kullanılır.
    /// </summary>
    public string? VarsayilanRenk { get; set; }

    /// <summary>
    /// Varsayılan malzeme tipi. Null ise modelin orijinal malzemesi kullanılır.
    /// </summary>
    public string? VarsayilanMalzeme { get; set; }

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    // Soft delete
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
