namespace VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;

/// <summary>
/// Müşteriden teklif isteği oluşturma DTO'su.
/// </summary>
public record TeklifIstegiOlusturDto(
    int MusteriKonfigurasyonuId,
    int UrunId,
    string MusteriAdSoyad,
    string Eposta,
    string? Telefon = null,
    string? Not = null
);

/// <summary>
/// Teklif yanıt DTO'su.
/// </summary>
public record TeklifYanitDto(
    int Id,
    int? FirmaId,
    int? MusteriKonfigurasyonuId,
    int? UrunId,
    string? OturumAnahtari,
    string? MusteriAdSoyad,
    string? Eposta,
    string? Telefon,
    string? Not,
    string? BomJson,
    decimal? ToplamFiyat,
    string Durum,
    DateTime? DurumGuncellemeTarihi,
    string? AdminNotu,
    DateTime OlusturulmaTarihi
);

/// <summary>
/// Teklif listeleme filtresi.
/// </summary>
public record TeklifListeleFiltreDto(
    int Sayfa = 1,
    int SayfaBuyuklugu = 20,
    string? Durum = null
);

/// <summary>
/// BOM kalemi (malzeme listesi satırı).
/// </summary>
public record BomKalemiDto(
    string ParcaAdi,
    string? ParcaKodu,
    int Miktar,
    decimal? BirimFiyat,
    decimal? SatirToplami,
    string? SeciliRenk,
    string? SeciliMalzeme,
    string? SeciliKaplama
);

/// <summary>
/// BOM özeti.
/// </summary>
public record BomOzetDto(
    string UrunAdi,
    string? UrunKodu,
    List<BomKalemiDto> Kalemler,
    decimal? GenelToplam,
    DateTime OlusturulmaTarihi
);
