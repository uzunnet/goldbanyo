using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

// ============================================================================
// ModelDosyaHazirlik — bir kerelik dosya hazirlama araci.
//
// I:\modeller klasorundeki tutarsiz isimli NRD kapak gorsellerini ve 3D
// modellerini, seed'in (NrdKapak) bekledigi standart isimlere donusturup
// wwwroot/medya altina kopyalar. 3D modeli olan 41 hedef numara islenir;
// 3D'si olmayan numaralarin kaynak gorselleri silinir.
//
// Standart hedef isimler (TohumVerisi.NrdKapak ile uyumlu):
//   thumb_{N}.jpg     <- "{N} T.jpg"        (ana gorsel)
//   yatay_{N}.png     <- "{N} YATAY.png"
//   kapaklar_{N}.png  <- "{N} KAPAKLAR.png"
//   ek_{N}_y.png      <- "{N} Y.png"        (galeri)
//   ek_{N}_yan.png    <- "{N} YAN.png"      (galeri)
//   nrd_{N}.glb       <- "NRD {N}.glb"      (varsayilan 3D)
//   nrd_{N}_{slug}.glb<- "NRD {N}_{etiket}.glb" (varyant 3D)
//
// Kullanim: dotnet run --project Araclar/ModelDosyaHazirlik [kaynak] [proje-kok]
// Idempotent: hedefte var olan dosyalari atlar.
// ============================================================================

var konumArgs = args.Where(a => !a.StartsWith("--")).ToArray();
bool silmeyiUygula = args.Contains("--sil"); // bayraksiz: silinecekleri sadece listeler (guvenli kuru calisma)
string kaynak = konumArgs.Length > 0 ? konumArgs[0] : @"I:\modeller";
string projeKok = konumArgs.Length > 1 ? konumArgs[1] : @"I:\desedoorweb";
string kapaklarHedef = Path.Combine(projeKok, "Desadoor.UI", "wwwroot", "medya", "kapaklar");
string glbHedef = Path.Combine(projeKok, "Desadoor.UI", "wwwroot", "medya", "3d");

// 3D modeli olan 41 hedef numara (sade NRD {N}.glb mevcut olanlar).
int[] hedefNumaralar =
[
    100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,118,120,121,
    124,125,128,130,134,135,144,147,149,150,151,152,153,154,156,157,158,160,
    161,162,164,166,167
];
var hedefSet = new HashSet<int>(hedefNumaralar);

if (!Directory.Exists(kaynak))
{
    Console.WriteLine($"HATA: Kaynak klasor yok: {kaynak}");
    return 1;
}
Directory.CreateDirectory(kapaklarHedef);
Directory.CreateDirectory(glbHedef);

int kopyalanan = 0, atlanan = 0, silinen = 0, dokunulmayan = 0;
var silinecekler = new List<string>();

// Numarayi dosya adinin basindan cikarir ("100-2 YATAY.png" -> 100).
static int? NumaraCikar(string ad)
{
    var m = Regex.Match(ad, @"^(\d+)");
    return m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : (int?)null;
}

// "NRD {N}_{etiket}" -> slug ("104 KAPAKLAR" -> "kapaklar", "NRD 104" -> "nrd")
static string Slugla(string metin)
{
    var s = metin.Trim().ToLowerInvariant();
    s = Regex.Replace(s, @"\s+", "_");
    s = Regex.Replace(s, @"[^a-z0-9_]", "");
    return s.Trim('_');
}

static void KopyalaIdempotent(string src, string hedefYol, ref int kopyalanan, ref int atlanan)
{
    if (File.Exists(hedefYol)) { atlanan++; return; }
    File.Copy(src, hedefYol);
    kopyalanan++;
}

// ---- 1) RESIMLER ----
foreach (var dosya in Directory.GetFiles(kaynak).Where(f =>
             f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
             f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
{
    var ad = Path.GetFileName(dosya);
    var numara = NumaraCikar(ad);
    if (numara is null) { dokunulmayan++; continue; } // sayisal olmayan (logo vb.)

    if (!hedefSet.Contains(numara.Value)) { silinecekler.Add(dosya); continue; }

    // Numaradan sonraki etiketi al: "104  T.jpg" -> "T", "100-2 YATAY.png" -> "YATAY"
    var m = Regex.Match(Path.GetFileNameWithoutExtension(ad), @"^\d+(?:-\d+)?\s+(.+)$");
    var etiket = m.Success ? Regex.Replace(m.Groups[1].Value, @"\s+", " ").Trim().ToUpperInvariant() : "";

    string? hedefAd = etiket switch
    {
        "T" => $"thumb_{numara}.jpg",
        "YATAY" => $"yatay_{numara}.png",
        "KAPAKLAR" => $"kapaklar_{numara}.png",
        "Y" => $"ek_{numara}_y.png",
        "YAN" => $"ek_{numara}_yan.png",
        _ => null // taninmayan etiket (MM FREZE, TOPLU, KAPAK ...) -> atla
    };

    if (hedefAd is null) { dokunulmayan++; continue; }
    KopyalaIdempotent(dosya, Path.Combine(kapaklarHedef, hedefAd), ref kopyalanan, ref atlanan);
}

// ---- 2) GLB MODELLER ----
foreach (var dosya in Directory.GetFiles(kaynak, "*.glb"))
{
    var bazAd = Path.GetFileNameWithoutExtension(Path.GetFileName(dosya));
    // Sade: "NRD 104"   Varyant: "NRD 104_104 KAPAKLAR"
    var m = Regex.Match(bazAd, @"^NRD\s*(\d+)(?:_(.+))?$", RegexOptions.IgnoreCase);
    if (!m.Success) { dokunulmayan++; continue; } // BOY/CAM/PANJUR — bu isin disinda

    var numara = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
    if (!hedefSet.Contains(numara)) { dokunulmayan++; continue; }

    string hedefAd = m.Groups[2].Success
        ? $"nrd_{numara}_{Slugla(m.Groups[2].Value)}.glb"
        : $"nrd_{numara}.glb";

    KopyalaIdempotent(dosya, Path.Combine(glbHedef, hedefAd), ref kopyalanan, ref atlanan);
}

// ---- 3) FAZLA GORSELLERI SIL (3D'si olmayan numaralar) ----
if (silmeyiUygula)
{
    foreach (var dosya in silinecekler)
    {
        File.Delete(dosya);
        silinen++;
    }
}

Console.WriteLine("=== ModelDosyaHazirlik tamamlandi ===");
Console.WriteLine($"Kopyalanan : {kopyalanan}");
Console.WriteLine($"Atlanan    : {atlanan} (hedefte zaten vardi)");
Console.WriteLine($"Dokunulmaz : {dokunulmayan} (taninmayan etiket / BOY-CAM-PANJUR)");
if (silmeyiUygula)
{
    Console.WriteLine($"Silinen    : {silinen} (3D'si olmayan kaynak gorsel)");
}
else
{
    Console.WriteLine($"SILINECEK  : {silinecekler.Count} dosya (--sil bayragi ile silinir). Ornekler:");
    foreach (var d in silinecekler.Take(15))
        Console.WriteLine($"   - {Path.GetFileName(d)}");
    if (silinecekler.Count > 15) Console.WriteLine($"   ... +{silinecekler.Count - 15} dosya daha");
}
return 0;
