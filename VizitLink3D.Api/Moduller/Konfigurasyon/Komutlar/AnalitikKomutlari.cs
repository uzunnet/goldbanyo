using MediatR;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

public static class AnalitikKomutlari
{
    /// <summary>
    /// Konfigüratördeki kullanıcı etkileşim olayını kaydetme komutu.
    /// Anonim kullanıcılar için güvenli, IP maskelenir.
    /// </summary>
    public record OlayKaydetKomutu(
        string OturumAnahtari,
        string OlayTipi,
        string? OlayVerisiJson = null,
        int? UrunId = null,
        int? ModelId = null,
        string? KullaniciIp = null,
        string? TarayiciBilgisi = null
    ) : IRequest<Cevap<int>>;
}
