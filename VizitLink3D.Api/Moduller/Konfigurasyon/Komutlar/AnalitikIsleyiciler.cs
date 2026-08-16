using MediatR;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

/// <summary>
/// Analytics olay kayıt handler'ı.
/// Anonim kullanıcı etkileşimlerini güvenli şekilde kaydeder.
/// IP anonimleştirilir (son oktet maskelenir).
/// Roomle veya başka rakip SDK KULLANILMAZ.
/// </summary>
public class OlayKaydetIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi)
    : IRequestHandler<AnalitikKomutlari.OlayKaydetKomutu, Cevap<int>>
{
    public async Task<Cevap<int>> Handle(
        AnalitikKomutlari.OlayKaydetKomutu istek,
        CancellationToken iptal)
    {
        var firmaId = kiraciServisi.MevcutFirmaId;

        // IP anonimleştir (son okteti maskele)
        var anonimIp = IpAnonimlestir(istek.KullaniciIp);

        var olay = new KonfiguratorOlayKaydi
        {
            FirmaId = firmaId,
            UrunId = istek.UrunId,
            ModelId = istek.ModelId,
            OturumAnahtari = istek.OturumAnahtari,
            OlayTipi = istek.OlayTipi,
            OlayVerisiJson = istek.OlayVerisiJson,
            KullaniciIp = anonimIp,
            TarayiciBilgisi = istek.TarayiciBilgisi,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        vt.KonfiguratorOlayKayitlari.Add(olay);
        await vt.SaveChangesAsync(iptal);

        return Cevap<int>.Basarili(olay.Id, "Olay kaydedildi.");
    }

    /// <summary>
    /// IPv4 adresinin son oktetini maskeler (örn: 192.168.1.XXX → 192.168.1.0).
    /// IPv6 veya geçersiz format ise olduğu gibi döner.
    /// </summary>
    private static string? IpAnonimlestir(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return ip;
        var parcalar = ip.Split('.');
        if (parcalar.Length == 4)
        {
            parcalar[3] = "0";
            return string.Join(".", parcalar);
        }
        return ip;
    }
}
