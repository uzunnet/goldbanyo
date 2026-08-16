using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;

/// <summary>
/// Seçilen parça/malzeme/renk/kaplamalardan BOM (Bill of Materials) hesaplar.
/// MusteriKonfigurasyonu ve ona bağlı MusteriKonfigurasyonParcasi üzerinden çalışır.
/// Roomle veya başka rakip SDK/API KULLANILMAZ — tamamen kendi implementasyonumuz.
/// </summary>
public interface IBomHesaplayici
{
    /// <summary>
    /// Verilen konfigürasyon ID'sine göre BOM hesaplar.
    /// Dönüş: (BomJson, ToplamFiyat)
    /// </summary>
    Task<BomSonucu> HesaplaAsync(int musteriKonfigurasyonuId, CancellationToken iptal = default);
}

public record BomSonucu(string BomJson, decimal? ToplamFiyat);

public class BomHesaplayici(VizitLink3DDbContext vt) : IBomHesaplayici
{
    private static readonly JsonSerializerOptions _jsonOpt = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<BomSonucu> HesaplaAsync(int musteriKonfigurasyonuId, CancellationToken iptal = default)
    {
        // Konfigürasyonu parçalarıyla birlikte getir
        var konfigurasyon = await vt.MusteriKonfigurasyonlari
            .AsNoTracking()
            .Include(k => k.Parcalar)
            .FirstOrDefaultAsync(k => k.Id == musteriKonfigurasyonuId && !k.SilindiMi, iptal);

        if (konfigurasyon is null)
            throw new InvalidOperationException("Müşteri konfigürasyonu bulunamadı.");

        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == konfigurasyon.UrunId && !u.SilindiMi, iptal);

        // BOM kalemlerini oluştur
        var kalemler = new List<BomKalemi>();
        decimal toplam = 0;

        if (konfigurasyon.Parcalar.Count > 0)
        {
            var parcaIdleri = konfigurasyon.Parcalar.Select(p => p.UrunUcBoyutParcasiId).ToList();

            // Parça bilgilerini toplu getir
            var parcalar = await vt.UrunUcBoyutParcalari
                .AsNoTracking()
                .Where(p => parcaIdleri.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, iptal);

            foreach (var konfigParca in konfigurasyon.Parcalar)
            {
                if (!parcalar.TryGetValue(konfigParca.UrunUcBoyutParcasiId, out var parca))
                    continue;

                string? renkAdi = null;
                string? malzemeAdi = null;
                string? kaplamaAdi = null;
                decimal ekFiyat = 0;

                // Renk bilgisi
                if (konfigParca.SeciliRenkId.HasValue)
                {
                    var renkSecenegi = await vt.UrunParcaRenkSecenekleri
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => r.Id == konfigParca.SeciliRenkId.Value, iptal);
                    if (renkSecenegi is not null && renkSecenegi.RalRengiId.HasValue)
                    {
                        var ral = await vt.RalRenkleri
                            .AsNoTracking()
                            .FirstOrDefaultAsync(r => r.Id == renkSecenegi.RalRengiId.Value, iptal);
                        if (ral is not null)
                            renkAdi = $"{ral.Kod} {ral.Ad}";
                    }
                }

                // Malzeme bilgisi
                if (konfigParca.SeciliMalzemeId.HasValue)
                {
                    var malzeme = await vt.Malzemeler
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Id == konfigParca.SeciliMalzemeId.Value, iptal);
                    if (malzeme is not null)
                        malzemeAdi = malzeme.Ad;
                }

                // Kaplama bilgisi
                if (konfigParca.SeciliKaplamaId.HasValue)
                {
                    var kaplama = await vt.KaplamaSecenekleri
                        .AsNoTracking()
                        .FirstOrDefaultAsync(k => k.Id == konfigParca.SeciliKaplamaId.Value, iptal);
                    if (kaplama is not null)
                        kaplamaAdi = kaplama.Ad;
                }
                // Alternatif: SeciliDoku text olarak
                else if (!string.IsNullOrWhiteSpace(konfigParca.SeciliDoku))
                {
                    kaplamaAdi = konfigParca.SeciliDoku;
                }

                // Varsa parça grubu adını al
                string? parcaGrubuAdi = null;
                if (parca.ParcaGrubuId.HasValue)
                {
                    var grup = await vt.UrunParcaGruplari
                        .AsNoTracking()
                        .FirstOrDefaultAsync(g => g.Id == parca.ParcaGrubuId.Value, iptal);
                    if (grup is not null)
                        parcaGrubuAdi = grup.Ad;
                }

                var gorunenAd = parcaGrubuAdi is not null
                    ? $"{parcaGrubuAdi} - {parca.GorunenAd}"
                    : parca.GorunenAd;

                kalemler.Add(new BomKalemi(
                    gorunenAd,
                    parca.ParcaTipi,
                    1,
                    ekFiyat,
                    renkAdi,
                    malzemeAdi,
                    kaplamaAdi,
                    konfigParca.GorunurMu
                ));
            }
        }

        // BOM JSON oluştur
        var bomOzet = new
        {
            UrunAdi = urun?.Ad ?? "Bilinmeyen Ürün",
            UrunSlug = urun?.Slug,
            OlusturulmaTarihi = DateTime.UtcNow,
            Kalemler = kalemler,
            KalemSayisi = kalemler.Count,
            GenelToplam = toplam > 0 ? toplam : (decimal?)null
        };

        var json = JsonSerializer.Serialize(bomOzet, _jsonOpt);
        return new BomSonucu(json, toplam > 0 ? toplam : null);
    }

    private record BomKalemi(
        string ParcaAdi,
        string? ParcaTipi,
        int Miktar,
        decimal? EkFiyat,
        string? Renk,
        string? Malzeme,
        string? Kaplama,
        bool GorunurMu
    );
}
