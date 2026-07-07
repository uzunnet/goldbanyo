using System.Security.Cryptography;
using System.Text.RegularExpressions;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Servisler;

/// <summary>
/// wwwroot/medya altindaki standart isimli (thumb_{N}.jpg, nrd_{N}*.glb) NRD kapak
/// dosyalarini, slug'i "nrd-{N}" olan urunlere ana gorsel ve 3D model olarak baglar.
/// Idempotent: var olan Medya/UcBoyut kayitlarini cogaltmaz, mevcut baglantilari korur.
/// </summary>
public partial class UrunMedyaBaglamaServisi(VizitLink3DDbContext vt, IWebHostEnvironment env)
{
    [GeneratedRegex(@"^nrd-(\d+)$")]
    private static partial Regex SlugNumaraDeseni();
    [GeneratedRegex(@"^nrd-(cam|boy-kpk)-(\d+)$")]
    private static partial Regex SlugCamBoyDeseni();

    public async Task<Cevap<UrunMedyaBaglamaSonucu>> BaglaAsync(CancellationToken iptal = default)
    {
        var sonuc = new UrunMedyaBaglamaSonucu();

        if (string.IsNullOrEmpty(env.WebRootPath))
            return Cevap<UrunMedyaBaglamaSonucu>.Basarili(sonuc, "WebRootPath tanimli degal, medya baglama atlandi.");

        var kapaklarKlasor = Path.Combine(env.WebRootPath, "medya", "kapaklar");
        var glbKlasor = Path.Combine(env.WebRootPath, "medya", "3d");

        var urunler = await vt.Urunler.IgnoreQueryFilters()
            .Where(u => !u.SilindiMi)
            .ToListAsync(iptal);

        foreach (var urun in urunler)
        {
            var eslesme = SlugNumaraDeseni().Match(urun.Slug);
            if (!eslesme.Success) continue; // sadece duz "nrd-{N}" serisi (cam/boy haric)

            var numara = eslesme.Groups[1].Value;
            bool degisti = false;

            // --- 1) Ana gorsel: thumb_{N}.jpg ---
            var thumbTam = Path.Combine(kapaklarKlasor, $"thumb_{numara}.jpg");
            if (File.Exists(thumbTam))
            {
                var medya = await MedyaGetirVeyaOlusturAsync(
                    thumbTam, $"medya/kapaklar/thumb_{numara}.jpg", "image/jpeg", iptal);
                if (urun.AnaGorselMedyaId != medya.Id)
                {
                    urun.AnaGorselMedyaId = medya.Id;
                    degisti = true;
                    sonuc.BaglananGorsel++;
                }
            }

            // --- 2) 3D modeller: nrd_{N}.glb (varsayilan) + nrd_{N}_*.glb (varyant) ---
            if (Directory.Exists(glbKlasor))
            {
                var sadeAd = $"nrd_{numara}.glb";
                var glbDosyalar = Directory.GetFiles(glbKlasor, $"nrd_{numara}*.glb")
                    .Where(f => Path.GetFileName(f) == sadeAd ||
                                Path.GetFileName(f).StartsWith($"nrd_{numara}_"))
                    .OrderBy(f => Path.GetFileName(f) == sadeAd ? 0 : 1) // sade once
                    .ToList();

                UrunUcBoyutModeli? varsayilan = null;
                foreach (var glb in glbDosyalar)
                {
                    var ad = Path.GetFileName(glb);
                    bool sadeMi = ad == sadeAd;
                    var ucBoyut = await UcBoyutGetirVeyaOlusturAsync(
                        urun.Id, glb, $"/medya/3d/{ad}", sadeMi, iptal);
                    sonuc.BaglananModel += ucBoyut.YeniMi ? 1 : 0;
                    if (sadeMi) varsayilan = ucBoyut.Model;
                }

                varsayilan ??= (await vt.UrunUcBoyutModelleri.IgnoreQueryFilters()
                    .Where(m => m.UrunId == urun.Id && !m.SilindiMi)
                    .OrderByDescending(m => m.VarsayilanMi)
                    .FirstOrDefaultAsync(iptal));

                if (varsayilan is not null && urun.VarsayilanUcBoyutModeliId != varsayilan.Id)
                {
                    urun.VarsayilanUcBoyutModeliId = varsayilan.Id;
                    degisti = true;
                }
            }

            if (degisti)
            {
                urun.GuncellenmeTarihi = DateTime.UtcNow;
                sonuc.GuncellenenUrun++;
            }
        }

        // 3D modeli olan ama VarsayilanUcBoyutModeliId atanmamis urunleri duzelt
        var eksikUrunler = await vt.Urunler.IgnoreQueryFilters()
            .Where(u => !u.SilindiMi && u.VarsayilanUcBoyutModeliId == null)
            .ToListAsync(iptal);
        foreach (var urun in eksikUrunler)
        {
            var varsayilanModel = await vt.UrunUcBoyutModelleri.IgnoreQueryFilters()
                .Where(m => m.UrunId == urun.Id && !m.SilindiMi)
                .OrderByDescending(m => m.VarsayilanMi)
                .FirstOrDefaultAsync(iptal);
            if (varsayilanModel is not null)
            {
                urun.VarsayilanUcBoyutModeliId = varsayilanModel.Id;
                sonuc.GuncellenenUrun++;
            }
        }

        await vt.SaveChangesAsync(iptal);
        return Cevap<UrunMedyaBaglamaSonucu>.Basarili(sonuc,
            $"Urun medya baglama tamamlandi. {sonuc.GuncellenenUrun} urun, {sonuc.BaglananGorsel} gorsel, {sonuc.BaglananModel} 3D model.");
    }

    private async Task<Medya> MedyaGetirVeyaOlusturAsync(
        string tamYol, string bagilYol, string mime, CancellationToken iptal)
    {
        var mevcut = await vt.Medyalar.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.DosyaYolu == bagilYol && !m.SilindiMi, iptal);
        if (mevcut is not null) return mevcut;

        string hash;
        await using (var stream = File.OpenRead(tamYol))
            hash = Convert.ToHexStringLower(SHA256.HashData(stream));

        var medya = new Medya
        {
            Ad = Path.GetFileNameWithoutExtension(bagilYol),
            OrijinalAd = Path.GetFileName(bagilYol),
            DosyaYolu = bagilYol,
            Tip = MedyaTipi.Resim,
            Kaynak = MedyaKaynagi.Yerel,
            BoyutByte = new FileInfo(tamYol).Length,
            MimeTipi = mime,
            Hash = hash,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        vt.Medyalar.Add(medya);
        await vt.SaveChangesAsync(iptal); // Id almak icin
        return medya;
    }

    private async Task<(UrunUcBoyutModeli Model, bool YeniMi)> UcBoyutGetirVeyaOlusturAsync(
        int urunId, string tamYol, string bagilYol, bool varsayilan, CancellationToken iptal)
    {
        var mevcut = await vt.UrunUcBoyutModelleri.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.UrunId == urunId && m.ModelDosyaYolu == bagilYol && !m.SilindiMi, iptal);
        if (mevcut is not null) return (mevcut, false);

        var model = new UrunUcBoyutModeli
        {
            UrunId = urunId,
            ModelAdi = Path.GetFileNameWithoutExtension(bagilYol),
            ModelDosyaYolu = bagilYol,
            ModelYolu = bagilYol,
            ModelTipi = "Glb",
            DosyaBoyutuByte = new FileInfo(tamYol).Length,
            VarsayilanMi = varsayilan,
            Versiyon = 1,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        vt.UrunUcBoyutModelleri.Add(model);
        await vt.SaveChangesAsync(iptal); // Id almak icin
        return (model, true);
    }
}

public class UrunMedyaBaglamaSonucu
{
    public int GuncellenenUrun { get; set; }
    public int BaglananGorsel { get; set; }
    public int BaglananModel { get; set; }
}
