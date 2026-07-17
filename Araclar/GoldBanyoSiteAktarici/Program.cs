using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

var kok = args.Length > 0 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
var apiKok = ProjeKlasorunuBul(kok, ["VizitLink3D.Api", "VizitLink.Api"]);
var uiKok = ProjeKlasorunuBul(kok, ["VizitLink3D.UI", "VizitLink.UI"]);
var veritabaniYolu = VeritabaniYolunuBul(apiKok);

if (string.IsNullOrWhiteSpace(apiKok) || string.IsNullOrWhiteSpace(uiKok))
{
    Console.Error.WriteLine($"Proje klasorleri bulunamadi. Kok={kok}");
    return 2;
}

if (string.IsNullOrWhiteSpace(veritabaniYolu) || !File.Exists(veritabaniYolu))
{
    Console.Error.WriteLine($"Veritabani bulunamadi: {veritabaniYolu}");
    return 2;
}

var apiMedyaKok = Path.Combine(apiKok, "wwwroot", "medya", "goldbanyo");
var uiMedyaKok = Path.Combine(uiKok, "wwwroot", "medya", "goldbanyo");
Directory.CreateDirectory(apiMedyaKok);
Directory.CreateDirectory(uiMedyaKok);
Directory.CreateDirectory(Path.Combine(apiMedyaKok, "urunler"));
Directory.CreateDirectory(Path.Combine(uiMedyaKok, "urunler"));

using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Codex", "1.0"));
http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(+https://openai.com)"));
http.Timeout = TimeSpan.FromSeconds(30);

var seriBaglantilari = new[]
{
    "https://www.goldbanyom.com.tr/exclusive-serisi-banyo-mobilyalari/",
    "https://www.goldbanyom.com.tr/premium-serisi-banyo-mobilyalari/",
    "https://www.goldbanyom.com.tr/gold-ban-yom-trend-serisi-banyo-mobilyalari/",
    "https://www.goldbanyom.com.tr/gold-ban-yom-standart-serisi-banyo-mobilyalari/"
};

var paylasilanDosyalar = new[]
{
    new PaylasilanGorsel("hero-banyo-mobilyasi.jpg", "https://www.goldbanyom.com.tr/wp-content/uploads/2024/11/gold-exclusive-banyo-dolaplari.jpg"),
    new PaylasilanGorsel("uretim.jpg", "https://www.goldbanyom.com.tr/wp-content/uploads/2022/06/kalite-1.jpg"),
    new PaylasilanGorsel("showroom.jpg", "https://www.goldbanyom.com.tr/wp-content/uploads/2020/12/gold-banyom-bayi-1.jpg"),
    new PaylasilanGorsel(Path.Combine("hakkimizda", "fabrika.jpg"), "https://www.goldbanyom.com.tr/wp-content/uploads/2022/06/kalite-2.jpg"),
    new PaylasilanGorsel(Path.Combine("hakkimizda", "fabrika_ic.jpg"), "https://www.goldbanyom.com.tr/wp-content/uploads/2022/06/kalite-3.jpg")
};

await using var baglanti = new SqliteConnection($"Data Source={veritabaniYolu}");
await baglanti.OpenAsync();
await using var islem = await baglanti.BeginTransactionAsync();

Console.WriteLine($"ApiKok={apiKok}");
Console.WriteLine($"UiKok={uiKok}");
Console.WriteLine($"Veritabani={veritabaniYolu}");

var tabloAdlari = await TabloAdlariniGetirAsync();
var tabloAdlariMetni = tabloAdlari.Count == 0 ? "(yok)" : string.Join(", ", tabloAdlari);
Console.WriteLine($"Tablolar={tabloAdlariMetni}");

var urunSayisi = 0;
var baglananMedyaSayisi = 0;
var indirilenDosyaSayisi = 0;
var guncellenenIcerikSayisi = 0;

try
{
    foreach (var gorsel in paylasilanDosyalar)
    {
        indirilenDosyaSayisi += await PaylasilanGorseliIndirAsync(gorsel);
    }

    guncellenenIcerikSayisi += await SayfaIcerikleriniGuncelleAsync();

    var urunBaglantilari = await UrunBaglantilariniToplaAsync();
    foreach (var baglantiBilgisi in urunBaglantilari)
    {
        var urunId = await TekilLongAsync(
            "SELECT Id FROM Urunler WHERE Slug = $slug AND SilindiMi = 0 LIMIT 1",
            Parametre("$slug", baglantiBilgisi.Slug));

        if (urunId == 0)
        {
            continue;
        }

        var html = await http.GetStringAsync(baglantiBilgisi.Url);
        var gorselUrl = MetaIcerigiBul(html, "og:image");
        if (string.IsNullOrWhiteSpace(gorselUrl))
        {
            continue;
        }

        var baslik = TemizMetin(MetaIcerigiBul(html, "og:title"));
        var aciklama = TemizMetin(MetaIcerigiBul(html, "og:description"));
        var uzanti = GuvenliUzanti(gorselUrl);
        var goreliYol = $"/medya/goldbanyo/urunler/{baglantiBilgisi.Slug}{uzanti}";

        indirilenDosyaSayisi += await DosyayiAynalaAsync(gorselUrl, goreliYol);

        var medyaId = await MedyaKaydiniGetirVeyaOlusturAsync(
            goreliYol,
            gorselUrl,
            string.IsNullOrWhiteSpace(baslik) ? baglantiBilgisi.Slug : baslik);

        baglananMedyaSayisi += await UruneMedyayiBaglaAsync(urunId, medyaId, goreliYol, baslik, aciklama);
        urunSayisi++;
    }

    await islem.CommitAsync();
}
catch
{
    await islem.RollbackAsync();
    throw;
}

Console.WriteLine($"IslenenUrun={urunSayisi}");
Console.WriteLine($"BaglananMedya={baglananMedyaSayisi}");
Console.WriteLine($"IndirilenDosya={indirilenDosyaSayisi}");
Console.WriteLine($"GuncellenenIcerik={guncellenenIcerikSayisi}");
return 0;

async Task<HashSet<UrunBaglantisi>> UrunBaglantilariniToplaAsync()
{
    var sonuc = new HashSet<UrunBaglantisi>(new UrunBaglantisiKarsilastirici());
    foreach (var seriBaglantisi in seriBaglantilari)
    {
        var html = await http.GetStringAsync(seriBaglantisi);
        foreach (Match eslesme in Regex.Matches(html, "https://www\\.goldbanyom\\.com\\.tr/urun/(?<slug>[^/\"?#]+)/", RegexOptions.IgnoreCase))
        {
            var slug = eslesme.Groups["slug"].Value.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(slug))
            {
                continue;
            }

            sonuc.Add(new UrunBaglantisi(slug, $"https://www.goldbanyom.com.tr/urun/{slug}/"));
        }
    }

    return sonuc;
}

async Task<int> SayfaIcerikleriniGuncelleAsync()
{
    if (!await TabloVarMiAsync("SayfaIcerikleri"))
    {
        Console.WriteLine("Uyari=SayfaIcerikleri tablosu yok, sayfa icerigi guncellemesi atlandi.");
        return 0;
    }

    var guncellenen = 0;

    guncellenen += await SayfaIcerigiYazAsync("anasayfa", "HeroGorselUrl", "/medya/goldbanyo/hero-banyo-mobilyasi.jpg");
    guncellenen += await SayfaIcerigiYazAsync("anasayfa", "HeroBaslik1", "Banyonuza Sanat Katan");
    guncellenen += await SayfaIcerigiYazAsync("anasayfa", "HeroBaslik2", "Banyo Mobilyası Modelleri");
    guncellenen += await SayfaIcerigiYazAsync("anasayfa", "HeroAciklama", "Gold Ban-yom; estetik, fonksiyonellik ve yüksek kaliteyi bir araya getiren banyo mobilyası serileri sunar.");
    guncellenen += await SayfaIcerigiYazAsync("anasayfa", "CtaButonYazi", "Ürünleri İncele");
    guncellenen += await SayfaIcerigiYazAsync("hakkimizda", "SayfaBasligi", "Hakkımızda | Gold Ban-yom");
    guncellenen += await SayfaIcerigiYazAsync("iletisim", "SayfaBasligi", "İletişim | Gold Ban-yom");

    if (await TabloVarMiAsync("Slaytlar"))
    {
        guncellenen += await KomutAsync(
            """
            UPDATE Slaytlar
            SET ArkaplanResim = CASE SiraNo
                WHEN 1 THEN '/medya/goldbanyo/hero-banyo-mobilyasi.jpg'
                WHEN 2 THEN '/medya/goldbanyo/uretim.jpg'
                WHEN 3 THEN '/medya/goldbanyo/showroom.jpg'
                ELSE ArkaplanResim
            END
            WHERE SayfaKodu = 'anasayfa' AND SilindiMi = 0;
            """);
    }
    else
    {
        Console.WriteLine("Uyari=Slaytlar tablosu yok, slayt guncellemesi atlandi.");
    }

    return guncellenen;
}

async Task<int> SayfaIcerigiYazAsync(string bolum, string anahtar, string deger)
{
    var mevcutId = await TekilLongAsync(
        "SELECT Id FROM SayfaIcerikleri WHERE Bolum = $bolum AND Anahtar = $anahtar AND Dil = 'tr' LIMIT 1",
        Parametre("$bolum", bolum),
        Parametre("$anahtar", anahtar));

    if (mevcutId == 0)
    {
        return await KomutAsync(
            """
            INSERT INTO SayfaIcerikleri (FirmaId, Bolum, Anahtar, Deger, Dil, GuncellemeTarihi, SilindiMi)
            VALUES (NULL, $bolum, $anahtar, $deger, 'tr', $simdi, 0);
            """,
            Parametre("$bolum", bolum),
            Parametre("$anahtar", anahtar),
            Parametre("$deger", deger),
            Parametre("$simdi", DateTime.UtcNow.ToString("O")));
    }

    return await KomutAsync(
        """
        UPDATE SayfaIcerikleri
        SET Deger = $deger,
            GuncellemeTarihi = $simdi
        WHERE Id = $id;
        """,
        Parametre("$deger", deger),
        Parametre("$simdi", DateTime.UtcNow.ToString("O")),
        Parametre("$id", mevcutId));
}

async Task<int> PaylasilanGorseliIndirAsync(PaylasilanGorsel gorsel)
{
    var goreliYol = "/medya/goldbanyo/" + gorsel.GoreliHedefYol.Replace('\\', '/');
    return await DosyayiAynalaAsync(gorsel.KaynakUrl, goreliYol);
}

async Task<int> DosyayiAynalaAsync(string kaynakUrl, string goreliYol)
{
    var apiHedef = FizikselMedyaYolu(apiMedyaKok, goreliYol);
    var uiHedef = FizikselMedyaYolu(uiMedyaKok, goreliYol);

    if (File.Exists(apiHedef) && File.Exists(uiHedef))
    {
        return 0;
    }

    var veri = await http.GetByteArrayAsync(kaynakUrl);

    var apiKlasor = Path.GetDirectoryName(apiHedef);
    var uiKlasor = Path.GetDirectoryName(uiHedef);
    if (!string.IsNullOrWhiteSpace(apiKlasor))
    {
        Directory.CreateDirectory(apiKlasor);
    }

    if (!string.IsNullOrWhiteSpace(uiKlasor))
    {
        Directory.CreateDirectory(uiKlasor);
    }

    await File.WriteAllBytesAsync(apiHedef, veri);
    await File.WriteAllBytesAsync(uiHedef, veri);
    return 1;
}

string FizikselMedyaYolu(string medyaKok, string goreliYol)
{
    var temiz = goreliYol.Replace('\\', '/').TrimStart('/');
    var altYol = temiz.StartsWith("medya/goldbanyo/", StringComparison.OrdinalIgnoreCase)
        ? temiz["medya/goldbanyo/".Length..]
        : temiz;
    return Path.Combine(medyaKok, altYol.Replace('/', Path.DirectorySeparatorChar));
}

async Task<long> MedyaKaydiniGetirVeyaOlusturAsync(string goreliYol, string kaynakUrl, string ad)
{
    if (!await TabloVarMiAsync("Medyalar"))
    {
        Console.WriteLine("Uyari=Medyalar tablosu yok, medya kaydi olusturma atlandi.");
        return 0;
    }

    var dosyaYolu = goreliYol.TrimStart('/');
    var mevcutId = await TekilLongAsync(
        "SELECT Id FROM Medyalar WHERE DosyaYolu = $yol AND SilindiMi = 0 LIMIT 1",
        Parametre("$yol", dosyaYolu));

    if (mevcutId != 0)
    {
        return mevcutId;
    }

    var orijinalAd = Path.GetFileName(dosyaYolu);
    return await TekilLongAsync(
        """
        INSERT INTO Medyalar
            (FirmaId, Tip, Kaynak, Ad, OrijinalAd, DosyaYolu, KaynakUrl, BoyutByte, MimeTipi, AltMetin, Aciklama, KullanimSayisi, SilindiMi, OlusturulmaTarihi)
        VALUES
            (NULL, 0, 0, $ad, $orijinalAd, $dosyaYolu, $kaynakUrl, 0, $mimeTipi, $altMetin, $aciklama, 0, 0, $simdi);
        SELECT last_insert_rowid();
        """,
        Parametre("$ad", KisaMetin(ad, 180)),
        Parametre("$orijinalAd", orijinalAd),
        Parametre("$dosyaYolu", dosyaYolu),
        Parametre("$kaynakUrl", kaynakUrl),
        Parametre("$mimeTipi", MimeTipi(Path.GetExtension(dosyaYolu))),
        Parametre("$altMetin", KisaMetin(ad, 180)),
        Parametre("$aciklama", "Gold Ban-yom resmi sitesinden içe aktarılan ürün görseli."),
        Parametre("$simdi", DateTime.UtcNow.ToString("O")));
}

async Task<int> UruneMedyayiBaglaAsync(long urunId, long medyaId, string goreliYol, string? baslik, string? aciklama)
{
    if (!await TabloVarMiAsync("UrunMedyalari"))
    {
        Console.WriteLine("Uyari=UrunMedyalari tablosu yok, urun-medya eslemesi atlandi.");
        return 0;
    }

    var degisti = 0;
    var mevcutBag = await TekilLongAsync(
        "SELECT Id FROM UrunMedyalari WHERE UrunId = $urunId AND MedyaUrl = $medyaUrl LIMIT 1",
        Parametre("$urunId", urunId),
        Parametre("$medyaUrl", goreliYol));

    if (mevcutBag == 0)
    {
        degisti += await KomutAsync(
            """
            INSERT INTO UrunMedyalari (UrunId, MedyaUrl, MedyaTuru, Aciklama, SiraNo, AnaGosterim)
            VALUES ($urunId, $medyaUrl, 'Gorsel', $aciklama, 1, 1);
            """,
            Parametre("$urunId", urunId),
            Parametre("$medyaUrl", goreliYol),
            Parametre("$aciklama", KisaMetin(string.IsNullOrWhiteSpace(aciklama) ? baslik : aciklama, 500)));
    }
    else
    {
        degisti += await KomutAsync(
            """
            UPDATE UrunMedyalari
            SET AnaGosterim = 1,
                Aciklama = COALESCE(NULLIF($aciklama, ''), Aciklama)
            WHERE Id = $id;
            """,
            Parametre("$aciklama", KisaMetin(string.IsNullOrWhiteSpace(aciklama) ? baslik : aciklama, 500)),
            Parametre("$id", mevcutBag));
    }

    degisti += await KomutAsync(
        """
        UPDATE Urunler
        SET AnaGorselMedyaId = $medyaId,
            KisaAciklama = CASE
                WHEN $aciklama <> '' THEN $aciklama
                ELSE KisaAciklama
            END,
            SeoAciklama = CASE
                WHEN $aciklama <> '' THEN $aciklama
                ELSE SeoAciklama
            END,
            GuncellenmeTarihi = $simdi
        WHERE Id = $urunId;
        """,
        Parametre("$medyaId", medyaId),
        Parametre("$aciklama", KisaMetin(aciklama, 300)),
        Parametre("$simdi", DateTime.UtcNow.ToString("O")),
        Parametre("$urunId", urunId));

    return degisti;
}

static string TemizMetin(string? deger)
{
    if (string.IsNullOrWhiteSpace(deger))
    {
        return string.Empty;
    }

    var cozulmus = WebUtility.HtmlDecode(deger);
    cozulmus = Regex.Replace(cozulmus, "<[^>]+>", " ");
    cozulmus = Regex.Replace(cozulmus, "\\s+", " ").Trim();
    return cozulmus;
}

static string MetaIcerigiBul(string html, string ozellik)
{
    var eslesme = Regex.Match(
        html,
        $"<meta[^>]+(?:property|name)=[\"']{Regex.Escape(ozellik)}[\"'][^>]+content=[\"'](?<icerik>[^\"']+)[\"']",
        RegexOptions.IgnoreCase);

    return eslesme.Success ? WebUtility.HtmlDecode(eslesme.Groups["icerik"].Value) : string.Empty;
}

static string GuvenliUzanti(string url)
{
    var uzanti = Path.GetExtension(url.Split('?', '#')[0]).ToLowerInvariant();
    return uzanti is ".jpg" or ".jpeg" or ".png" or ".webp" ? uzanti : ".jpg";
}

static string MimeTipi(string uzanti) => uzanti.ToLowerInvariant() switch
{
    ".png" => "image/png",
    ".webp" => "image/webp",
    ".jpeg" => "image/jpeg",
    _ => "image/jpeg"
};

static string KisaMetin(string? metin, int azami)
{
    var temiz = TemizMetin(metin);
    return temiz.Length <= azami ? temiz : temiz[..azami];
}

async Task<List<string>> TabloAdlariniGetirAsync()
{
    await using var komut = baglanti.CreateCommand();
    komut.Transaction = (SqliteTransaction)islem;
    komut.CommandText = """
        SELECT name
        FROM sqlite_master
        WHERE type = 'table'
        ORDER BY name;
        """;

    var sonuc = new List<string>();
    await using var okuyucu = await komut.ExecuteReaderAsync();
    while (await okuyucu.ReadAsync())
    {
        sonuc.Add(okuyucu.GetString(0));
    }

    return sonuc;
}

async Task<bool> TabloVarMiAsync(string tabloAdi)
{
    var adet = await TekilLongAsync(
        """
        SELECT COUNT(1)
        FROM sqlite_master
        WHERE type = 'table' AND name = $tabloAdi;
        """,
        Parametre("$tabloAdi", tabloAdi));

    return adet > 0;
}

async Task<long> TekilLongAsync(string sql, params SqliteParameter[] parametreler)
{
    await using var komut = baglanti.CreateCommand();
    komut.Transaction = (SqliteTransaction)islem;
    komut.CommandText = sql;
    komut.Parameters.AddRange(parametreler);
    var sonuc = await komut.ExecuteScalarAsync();
    return sonuc is null or DBNull ? 0 : Convert.ToInt64(sonuc);
}

async Task<int> KomutAsync(string sql, params SqliteParameter[] parametreler)
{
    await using var komut = baglanti.CreateCommand();
    komut.Transaction = (SqliteTransaction)islem;
    komut.CommandText = sql;
    komut.Parameters.AddRange(parametreler);
    return await komut.ExecuteNonQueryAsync();
}

static SqliteParameter Parametre(string ad, object? deger) => new(ad, deger ?? DBNull.Value);

static string ProjeKlasorunuBul(string kok, string[] adayKlasorler)
{
    foreach (var adayKlasor in adayKlasorler)
    {
        var tamYol = Path.Combine(kok, adayKlasor);
        if (Directory.Exists(tamYol))
        {
            return tamYol;
        }
    }

    return string.Empty;
}

static string VeritabaniYolunuBul(string apiKok)
{
    if (string.IsNullOrWhiteSpace(apiKok) || !Directory.Exists(apiKok))
    {
        return string.Empty;
    }

    var adaylar = new[]
    {
        Path.Combine(apiKok, "vizitlink3d.db"),
        Path.Combine(apiKok, "goldbanyo.db")
    };

    foreach (var aday in adaylar)
    {
        if (File.Exists(aday))
        {
            return aday;
        }
    }

    return Directory
        .GetFiles(apiKok, "*.db", SearchOption.TopDirectoryOnly)
        .OrderBy(dosya => dosya, StringComparer.OrdinalIgnoreCase)
        .FirstOrDefault() ?? string.Empty;
}

sealed record PaylasilanGorsel(string GoreliHedefYol, string KaynakUrl);
sealed record UrunBaglantisi(string Slug, string Url);

sealed class UrunBaglantisiKarsilastirici : IEqualityComparer<UrunBaglantisi>
{
    public bool Equals(UrunBaglantisi? x, UrunBaglantisi? y)
        => string.Equals(x?.Slug, y?.Slug, StringComparison.OrdinalIgnoreCase);

    public int GetHashCode(UrunBaglantisi obj)
        => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Slug);
}

