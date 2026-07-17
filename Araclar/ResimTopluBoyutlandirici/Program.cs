using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

// Kullanim: ResimTopluBoyutlandirici <kokKlasor> [maksKenar=1000] [webpKalite=85] [--uygula]
// --uygula verilmezse SADECE rapor uretilir (dry-run), hicbir dosya degistirilmez.
// Dosya adi/uzanti/format DEGISTIRILMEZ: PNG->PNG, JPG->JPG, WEBP->WEBP olarak kaydedilir.
// "_yedek" veya "_toplu_yedek" ile baslayan klasorler taranmaz.

string kokKlasor = args.Length > 0 ? args[0] : @"I:\goldbanyo_web\VizitLink3D.UI\wwwroot\medya";
int maksKenar = args.Length > 1 ? int.Parse(args[1]) : 1000;
int webpKalite = args.Length > 2 ? int.Parse(args[2]) : 85;
bool uygula = args.Contains("--uygula");
// --zorlaYenidenKodla: boyutu zaten maksKenar altinda olan dosyalari da (sikistirmayi iyilestirmek icin) yeniden kodlar.
bool zorlaYenidenKodla = args.Contains("--zorlaYenidenKodla");

var resimUzantilari = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".tif", ".tiff" };

if (!Directory.Exists(kokKlasor))
{
    Console.WriteLine($"HATA: Klasor bulunamadi: {kokKlasor}");
    return;
}

var tumDosyalar = Directory.GetFiles(kokKlasor, "*.*", SearchOption.AllDirectories);

var dosyalar = tumDosyalar.Where(f =>
{
    var goreceliYol = Path.GetRelativePath(kokKlasor, f);
    var parcalar = goreceliYol.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    // "_yedek" veya "_toplu_yedek" ile baslayan herhangi bir ust klasoru iceren yollari atla
    if (parcalar.Take(parcalar.Length - 1).Any(p => p.StartsWith("_yedek", StringComparison.OrdinalIgnoreCase) || p.StartsWith("_toplu_yedek", StringComparison.OrdinalIgnoreCase)))
        return false;
    var ad = Path.GetFileName(f);
    if (ad.StartsWith('.')) return false;
    return resimUzantilari.Contains(Path.GetExtension(f));
}).ToArray();

int islenen = 0, atlanan = 0, hata = 0;
long eskiToplam = 0, yeniToplam = 0;
var degisenler = new List<string>();
var hatalar = new List<string>();

foreach (var dosya in dosyalar)
{
    try
    {
        using var image = Image.Load(dosya);
        int uzunKenar = Math.Max(image.Width, image.Height);

        bool boyutAsimi = uzunKenar > maksKenar;
        if (!boyutAsimi && !zorlaYenidenKodla)
        {
            atlanan++;
            continue;
        }

        long eskiBytes = new FileInfo(dosya).Length;

        if (boyutAsimi)
        {
            float oran = (float)maksKenar / uzunKenar;
            int yeniGenislik = Math.Max(1, (int)Math.Round(image.Width * oran));
            int yeniYukseklik = Math.Max(1, (int)Math.Round(image.Height * oran));

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(yeniGenislik, yeniYukseklik),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Bicubic
            }));
        }

        var uzanti = Path.GetExtension(dosya).ToLowerInvariant();

        if (uygula)
        {
            // Ayni dosya adi / ayni uzanti / ayni format ile UZERINE YAZ.
            IImageEncoder encoder = uzanti switch
            {
                ".jpg" or ".jpeg" => new JpegEncoder { Quality = webpKalite },
                ".webp" => new WebpEncoder { Quality = webpKalite },
                _ => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression }
            };
            using var geciciAkis = new MemoryStream();
            image.Save(geciciAkis, encoder);
            var yeniVeri = geciciAkis.ToArray();

            // Zorla-yeniden-kodlamada eger yeni dosya daha buyukse ve boyut degismediyse orijinali koru.
            if (!boyutAsimi && zorlaYenidenKodla && yeniVeri.Length >= eskiBytes)
            {
                atlanan++;
                continue;
            }

            File.WriteAllBytes(dosya, yeniVeri);
        }

        long yeniBytes = uygula ? new FileInfo(dosya).Length : eskiBytes;
        eskiToplam += eskiBytes;
        yeniToplam += yeniBytes;

        degisenler.Add($"{Path.GetRelativePath(kokKlasor, dosya)}  {image.Width}x{image.Height} ({eskiBytes / 1024.0:F1} KB -> {yeniBytes / 1024.0:F1} KB)");
        islenen++;
    }
    catch (Exception ex)
    {
        hata++;
        hatalar.Add($"{Path.GetRelativePath(kokKlasor, dosya)}: {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("===========================================");
Console.WriteLine($"      TOPLU BOYUTLANDIRMA RAPORU {(uygula ? "(UYGULANDI)" : "(DRY-RUN, hicbir dosya degismedi)")}");
Console.WriteLine("===========================================");
Console.WriteLine($"Kok klasor:            {kokKlasor}");
Console.WriteLine($"Maksimum kenar:        {maksKenar}px");
Console.WriteLine($"Toplam taranan dosya:  {dosyalar.Length}");
Console.WriteLine($"Islenen (>{maksKenar}px): {islenen}");
Console.WriteLine($"Atlanan (<= {maksKenar}px): {atlanan}");
Console.WriteLine($"Hata:                  {hata}");
if (uygula)
{
    Console.WriteLine($"Eski toplam boyut:     {eskiToplam / 1024.0 / 1024.0:F2} MB");
    Console.WriteLine($"Yeni toplam boyut:     {yeniToplam / 1024.0 / 1024.0:F2} MB");
}
Console.WriteLine();
if (degisenler.Count > 0)
{
    Console.WriteLine(uygula ? "Degistirilen dosyalar:" : "Degistirilecek dosyalar (dry-run):");
    foreach (var d in degisenler)
        Console.WriteLine($"  {d}");
}
if (hatalar.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("HATALAR:");
    foreach (var h in hatalar)
        Console.WriteLine($"  {h}");
}
