using System;
using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Konfigüratördeki kullanıcı etkileşimlerini kaydeden kendi analytics sistemimiz.
/// Roomle veya başka rakip SDK/API/CDN/katalog KULLANILMAZ.
/// Olaylar anonim olarak kaydedilir; tenant bazında raporlanır.
/// </summary>
public class KonfiguratorOlayKaydi
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }
    public int? UrunId { get; set; }
    public int? ModelId { get; set; }
    public string? OturumAnahtari { get; set; }

    /// <summary>
    /// Olay tipi: SayfaGoruntulendi, ParcaSecildi, RenkDegisti, MalzemeDegisti,
    /// KaplamaDegisti, TeklifIstendi, ModelYuklendi, ModelHatasi, EmbedAcildi
    /// </summary>
    public string OlayTipi { get; set; } = string.Empty;

    /// <summary>JSON formatında olaya özel ek veri</summary>
    public string? OlayVerisiJson { get; set; }

    /// <summary>Anonimleştirilmiş IP (son oktet maskeli)</summary>
    public string? KullaniciIp { get; set; }

    /// <summary>User-Agent özeti</summary>
    public string? TarayiciBilgisi { get; set; }

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    [JsonIgnore]
    public Firma? Firma { get; set; }

    [JsonIgnore]
    public Urun? Urun { get; set; }
}
