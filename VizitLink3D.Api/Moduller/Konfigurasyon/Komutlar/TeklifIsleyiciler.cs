using MediatR;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

/// <summary>
/// Teklif oluşturma handler'ı.
/// Müşteri konfigürasyonundan BOM hesaplar ve KonfiguratorTeklif entity'si oluşturur.
/// Tenant izolasyonu: KiraciServisi üzerinden FirmaId alınır.
/// </summary>
public class TeklifOlusturIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi,
    IBomHesaplayici bomHesaplayici)
    : IRequestHandler<TeklifOlusturKomutu, Cevap<TeklifYanitDto>>
{
    public async Task<Cevap<TeklifYanitDto>> Handle(
        TeklifOlusturKomutu istek,
        CancellationToken iptal)
    {
        var firmaId = kiraciServisi.MevcutFirmaId;
        if (firmaId is null or 0)
            return Cevap<TeklifYanitDto>.Hata("Firma tanımlanamadı.");

        // Konfigürasyonun firmaya ait olduğunu doğrula
        var konfigurasyon = await vt.MusteriKonfigurasyonlari
            .AsNoTracking()
            .FirstOrDefaultAsync(k =>
                k.Id == istek.MusteriKonfigurasyonuId &&
                k.FirmaId == firmaId &&
                !k.SilindiMi,
                iptal);

        if (konfigurasyon is null)
            return Cevap<TeklifYanitDto>.Hata("Konfigürasyon bulunamadı.");

        // Ürün doğrulaması
        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == istek.UrunId &&
                u.FirmaId == firmaId &&
                !u.SilindiMi &&
                u.AktifMi,
                iptal);

        if (urun is null)
            return Cevap<TeklifYanitDto>.Hata("Ürün bulunamadı.");

        // BOM hesapla
        BomSonucu bomSonuc;
        try
        {
            bomSonuc = await bomHesaplayici.HesaplaAsync(istek.MusteriKonfigurasyonuId, iptal);
        }
        catch (InvalidOperationException ex)
        {
            return Cevap<TeklifYanitDto>.Hata(ex.Message);
        }

        // Teklif entity'si oluştur
        var teklif = new KonfiguratorTeklif
        {
            FirmaId = firmaId,
            MusteriKonfigurasyonuId = istek.MusteriKonfigurasyonuId,
            UrunId = istek.UrunId,
            OturumAnahtari = konfigurasyon.OturumAnahtari,
            MusteriAdSoyad = istek.MusteriAdSoyad,
            Eposta = istek.Eposta,
            Telefon = istek.Telefon,
            Not = istek.Not,
            BomJson = bomSonuc.BomJson,
            ToplamFiyat = bomSonuc.ToplamFiyat,
            Durum = "Bekliyor",
            DurumGuncellemeTarihi = DateTime.UtcNow,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        vt.KonfiguratorTeklifler.Add(teklif);
        await vt.SaveChangesAsync(iptal);

        // Konfigürasyon durumunu güncelle
        var guncellenecekKonfig = await vt.MusteriKonfigurasyonlari
            .FirstOrDefaultAsync(k => k.Id == istek.MusteriKonfigurasyonuId, iptal);
        if (guncellenecekKonfig is not null)
        {
            guncellenecekKonfig.Durum = "TeklifeDonustu";
            guncellenecekKonfig.GuncellenmeTarihi = DateTime.UtcNow;
            await vt.SaveChangesAsync(iptal);
        }

        var yanit = new TeklifYanitDto(
            teklif.Id,
            teklif.FirmaId,
            teklif.MusteriKonfigurasyonuId,
            teklif.UrunId,
            teklif.OturumAnahtari,
            teklif.MusteriAdSoyad,
            teklif.Eposta,
            teklif.Telefon,
            teklif.Not,
            teklif.BomJson,
            teklif.ToplamFiyat,
            teklif.Durum,
            teklif.DurumGuncellemeTarihi,
            teklif.AdminNotu,
            teklif.OlusturulmaTarihi
        );

        return Cevap<TeklifYanitDto>.Basarili(yanit, "Teklif isteği başarıyla oluşturuldu.");
    }
}
