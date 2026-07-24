using System.Security.Cryptography;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Servisler;

public class GlbDosyaServisi
{
    private readonly string _medyaKlasorYolu;
    private static readonly byte[] GlbSihirliBayt = { 0x67, 0x6C, 0x54, 0x46 }; // "glTF"

    public GlbDosyaServisi(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _medyaKlasorYolu = Path.Combine(environment.WebRootPath, "medya", "3d-modeller");
    }

    public string MedyaKlasorYolu => _medyaKlasorYolu;

    public string GuvenliDosyaAdiOlustur()
    {
        return Guid.NewGuid().ToString("N") + ".glb";
    }

    /// <summary>
    /// GLB 12-byte başlık doğrulaması:
    ///   Bayt 0-3: magic = "glTF"
    ///   Bayt 4-7: version = 2 (uint32 LE)
    ///   Bayt 8-11: declared total length (uint32 LE) = actual stream length olmalı
    /// Geçersiz durumda false döner (generic hata mesajı controller'da verilir).
    /// </summary>
    public bool SihirliBaytDogrula(Stream dosya)
    {
        if (dosya is null)
            return false;

        // 12 bayt başlık için yeterli uzunluk kontrolü
        if (dosya.Length < 12)
        {
            if (dosya.CanSeek) dosya.Position = 0;
            return false;
        }

        Span<byte> baslik = stackalloc byte[12];
        var okunanUzunluk = dosya.Read(baslik);

        if (dosya.CanSeek)
            dosya.Position = 0;

        if (okunanUzunluk < 12)
            return false;

        // 1) Magic kontrolü: "glTF"
        if (baslik[0] != 0x67 || baslik[1] != 0x6C || baslik[2] != 0x54 || baslik[3] != 0x46)
            return false;

        // 2) Version kontrolü: uint32 LE = 2
        var version = BitConverter.ToUInt32(baslik.Slice(4, 4));
        if (version != 2)
            return false;

        // 3) Declared total length = actual file length
        var bildirilenUzunluk = BitConverter.ToUInt32(baslik.Slice(8, 4));
        if (bildirilenUzunluk != (uint)dosya.Length)
            return false;

        return true;
    }

    public string Sha256Hesapla(Stream dosya)
    {
        if (dosya.CanSeek)
            dosya.Position = 0;

        var sha256 = SHA256.HashData(dosya);

        if (dosya.CanSeek)
            dosya.Position = 0;

        return Convert.ToHexStringLower(sha256);
    }

    public async Task<(string dosyaAdi, string dosyaYolu, long boyut, string hash)> KaydetAsync(
        IFormFile dosya,
        CancellationToken iptal = default)
    {
        var guvenliAd = GuvenliDosyaAdiOlustur();

        if (!Directory.Exists(_medyaKlasorYolu))
            Directory.CreateDirectory(_medyaKlasorYolu);

        var tamYol = Path.Combine(_medyaKlasorYolu, guvenliAd);

        await using var gelenAkis = dosya.OpenReadStream();

        // Önce hash hesapla (gelen stream'den)
        using var bellekAkisi = new MemoryStream();
        await gelenAkis.CopyToAsync(bellekAkisi, iptal);
        bellekAkisi.Position = 0;

        var hash = Sha256Hesapla(bellekAkisi);
        var boyut = bellekAkisi.Length;

        // Sonra dosyaya yaz
        bellekAkisi.Position = 0;
        await using var dosyaAkisi = new FileStream(tamYol, FileMode.Create, FileAccess.Write);
        await bellekAkisi.CopyToAsync(dosyaAkisi, iptal);
        await dosyaAkisi.FlushAsync(iptal);

        var goreceliYol = "/medya/3d-modeller/" + guvenliAd;

        return (guvenliAd, goreceliYol, boyut, hash);
    }

    public void Temizle(string dosyaYolu)
    {
        if (string.IsNullOrWhiteSpace(dosyaYolu))
            return;

        var guvenliAd = Path.GetFileName(dosyaYolu);
        if (string.IsNullOrWhiteSpace(guvenliAd))
            return;

        var tamYol = Path.Combine(_medyaKlasorYolu, guvenliAd);

        if (File.Exists(tamYol))
            File.Delete(tamYol);
    }
}
