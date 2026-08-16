using System;
using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Müşteri konfigürasyonundan oluşturulan BOM içeren teklif.
/// Seçilen parça/malzeme/renk/kaplamalardan otomatik BOM hesaplanır.
/// SuperAdmin ve FirmaAdmin tarafından yönetilir.
/// </summary>
public class KonfiguratorTeklif
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }
    public int? MusteriKonfigurasyonuId { get; set; }
    public int? UrunId { get; set; }
    public string? OturumAnahtari { get; set; }

    /// <summary>Müşteri ad soyad</summary>
    public string? MusteriAdSoyad { get; set; }

    /// <summary>İletişim e-posta</summary>
    public string? Eposta { get; set; }

    /// <summary>İletişim telefon</summary>
    public string? Telefon { get; set; }

    /// <summary>Müşteri notu / özel istek</summary>
    public string? Not { get; set; }

    /// <summary>JSON formatında BOM (malzeme listesi): parça adı, malzeme, renk, kaplama, adet</summary>
    public string? BomJson { get; set; }

    /// <summary>BOM'dan hesaplanan toplam fiyat</summary>
    public decimal? ToplamFiyat { get; set; }

    /// <summary>Durum: Bekliyor, Incelendi, TeklifHazirlandi, MusteriyeIletildi, Onaylandi, Reddedildi</summary>
    public string Durum { get; set; } = "Bekliyor";

    public DateTime? DurumGuncellemeTarihi { get; set; }

    /// <summary>Admin tarafından eklenen iç not</summary>
    public string? AdminNotu { get; set; }

    // === AUDIT ===
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public int? OlusturanKullaniciId { get; set; }
    public int? GuncelleyenKullaniciId { get; set; }

    // === SOFT DELETE ===
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    // === NAVIGATION ===
    [JsonIgnore]
    public MusteriKonfigurasyonu? MusteriKonfigurasyonu { get; set; }

    [JsonIgnore]
    public Urun? Urun { get; set; }
}
