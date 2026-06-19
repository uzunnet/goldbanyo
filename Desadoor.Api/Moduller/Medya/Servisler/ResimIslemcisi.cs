using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;

namespace Desadoor.Api.Moduller.Medya.Servisler;

public interface IResimIslemcisi
{
    Task<string> KucukBoyutOlusturAsync(string kaynakYol, string hedefKlasor, int genislik = 200, int yukseklik = 200);
    string HashHesapla(Stream dosya);
    Task<string> WebpDonusturAsync(string kaynakYol, string hedefYol);
}

public class ResimIslemcisi : IResimIslemcisi
{
    public async Task<string> KucukBoyutOlusturAsync(string kaynakYol, string hedefKlasor, int genislik = 200, int yukseklik = 200)
    {
        if (!Directory.Exists(hedefKlasor)) Directory.CreateDirectory(hedefKlasor);
        var dosyaAdi = Path.GetFileName(kaynakYol);
        var hedefYol = Path.Combine(hedefKlasor, $"thumb_{dosyaAdi}");

        if (!File.Exists(kaynakYol)) return hedefYol;

        using var resim = await Image.LoadAsync(kaynakYol);
        resim.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(genislik, yukseklik),
            Mode = ResizeMode.Max
        }));
        await resim.SaveAsync(hedefYol);

        return hedefYol;
    }

    public string HashHesapla(Stream dosya)
    {
        dosya.Position = 0;
        var hash = SHA256.HashData(dosya);
        dosya.Position = 0;
        return Convert.ToHexStringLower(hash);
    }

    public async Task<string> WebpDonusturAsync(string kaynakYol, string hedefYol)
    {
        if (!File.Exists(kaynakYol)) return hedefYol;

        using var resim = await Image.LoadAsync(kaynakYol);
        var webpYol = Path.ChangeExtension(hedefYol, ".webp");
        await resim.SaveAsWebpAsync(webpYol);
        return webpYol;
    }
}
