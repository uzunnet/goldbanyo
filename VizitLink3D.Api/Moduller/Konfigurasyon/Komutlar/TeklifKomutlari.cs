using MediatR;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

/// <summary>
/// Müşteri konfigürasyonundan teklif isteği oluşturma komutu.
/// BOM otomatik hesaplanır, KonfiguratorTeklif entity'si oluşturulur.
/// </summary>
public record TeklifOlusturKomutu(
    int MusteriKonfigurasyonuId,
    int UrunId,
    string MusteriAdSoyad,
    string Eposta,
    string? Telefon = null,
    string? Not = null
) : IRequest<Cevap<TeklifYanitDto>>;
