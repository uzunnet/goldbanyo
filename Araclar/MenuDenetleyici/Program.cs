using System.Globalization;
using Microsoft.Data.Sqlite;

var kok = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
var temizlikYap = args.Any(a => string.Equals(a, "--temizle", StringComparison.OrdinalIgnoreCase));
var sadelestir = args.Any(a => string.Equals(a, "--sadelestir", StringComparison.OrdinalIgnoreCase));
var urunUcBoyutKapat = ArgumanDegeri(args, "--urun-3d-kapat");
var vtYolu = Path.Combine(kok, "VIZITLINK3D.Api", "VIZITLINK3D.db");

if (!File.Exists(vtYolu))
{
    Console.Error.WriteLine($"Veritabani bulunamadi: {vtYolu}");
    return 2;
}

await using var baglanti = new SqliteConnection($"Data Source={vtYolu};Mode={(temizlikYap || sadelestir || !string.IsNullOrWhiteSpace(urunUcBoyutKapat) ? "ReadWrite" : "ReadOnly")}");
await baglanti.OpenAsync();

if (temizlikYap)
{
    var simdi = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    await using var komut = baglanti.CreateCommand();
    komut.CommandText =
        """
        UPDATE MenuOgeleri
        SET SilindiMi = 1,
            SilinmeTarihi = $simdi,
            GuncellenmeTarihi = $simdi
        WHERE SilindiMi = 0
          AND Konum = 'AnaMenu';
        SELECT changes();
        """;
    komut.Parameters.AddWithValue("$simdi", simdi);
    var etkilenen = Convert.ToInt32(await komut.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    Console.WriteLine($"TEMIZLENEN_ESKI_ANAMENU={etkilenen}");
}

if (sadelestir)
{
    await AdminMenuSadelestirAsync();
}

if (!string.IsNullOrWhiteSpace(urunUcBoyutKapat))
{
    await UrunUcBoyutKapatAsync(urunUcBoyutKapat);
}

await YazdirAsync(
    "KONUM_OZETI",
    """
    SELECT Konum,
           COUNT(*) AS Toplam,
           SUM(CASE WHEN UstMenuId IS NULL THEN 1 ELSE 0 END) AS Kok,
           SUM(CASE WHEN UstMenuId IS NOT NULL THEN 1 ELSE 0 END) AS Alt,
           SUM(CASE WHEN AktifMi = 1 THEN 1 ELSE 0 END) AS Aktif
    FROM MenuOgeleri
    WHERE SilindiMi = 0
    GROUP BY Konum
    ORDER BY Konum;
    """);

await YazdirAsync(
    "TEKRAR_KONTROLU",
    """
    SELECT Konum, Baslik, Url, COUNT(*) AS Adet
    FROM MenuOgeleri
    WHERE SilindiMi = 0
    GROUP BY Konum, Baslik, Url
    HAVING COUNT(*) > 1
    ORDER BY Adet DESC, Konum, Baslik;
    """);

await YazdirAsync(
    "ADMINSOL_AGACI",
    """
    SELECT COALESCE(ust.Baslik, '') AS Ust,
           m.Id,
           m.Baslik,
           m.Url,
           m.Sira,
           m.AktifMi,
           m.SuperAdminGerekliMi
    FROM MenuOgeleri m
    LEFT JOIN MenuOgeleri ust ON ust.Id = m.UstMenuId
    WHERE m.SilindiMi = 0 AND m.Konum = 'AdminSol'
    ORDER BY COALESCE(m.UstMenuId, m.Id), m.UstMenuId IS NOT NULL, m.Sira, m.Id;
    """);

return 0;

async Task YazdirAsync(string baslik, string sql)
{
    Console.WriteLine($"---{baslik}---");
    await using var komut = baglanti.CreateCommand();
    komut.CommandText = sql;
    await using var okuyucu = await komut.ExecuteReaderAsync();

    var satirVarMi = false;
    while (await okuyucu.ReadAsync())
    {
        satirVarMi = true;
        var degerler = Enumerable.Range(0, okuyucu.FieldCount)
            .Select(i => $"{okuyucu.GetName(i)}={Deger(okuyucu, i)}");
        Console.WriteLine(string.Join(" | ", degerler));
    }

    if (!satirVarMi)
        Console.WriteLine("Kayit yok");
}

static string Deger(SqliteDataReader okuyucu, int sira)
{
    if (okuyucu.IsDBNull(sira))
        return "";

    var deger = okuyucu.GetValue(sira);
    return deger switch
    {
        IFormattable bicimli => bicimli.ToString(null, CultureInfo.InvariantCulture) ?? "",
        _ => deger.ToString() ?? ""
    };
}

async Task AdminMenuSadelestirAsync()
{
    var simdi = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    await using var islem = await baglanti.BeginTransactionAsync();

    try
    {
        await KomutAsync(
            """
            UPDATE MenuOgeleri
            SET Baslik = 'Urun ve 3D',
                Ikon = 'ViewInAr',
                Sira = 3,
                GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Urun Yonetimi';

            UPDATE MenuOgeleri
            SET Baslik = 'Icerik ve Medya',
                Ikon = 'PermMedia',
                Sira = 4,
                GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Icerik Yonetimi';

            UPDATE MenuOgeleri
            SET Baslik = 'Musteri ve Operasyon',
                Ikon = 'SupportAgent',
                Sira = 5,
                GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Pazarlama';

            UPDATE MenuOgeleri
            SET Sira = 1, GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Gosterge Paneli';

            UPDATE MenuOgeleri
            SET Sira = 2, GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Is Takip';

            UPDATE MenuOgeleri
            SET Sira = 6,
                SuperAdminGerekliMi = 0,
                GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Sistem';
            """,
            P("$simdi", simdi));

        await KomutAsync(
            """
            UPDATE MenuOgeleri
            SET UstMenuId = (SELECT Id FROM MenuOgeleri WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Urun ve 3D' LIMIT 1),
                GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0
              AND Konum = 'AdminSol'
              AND UstMenuId = (SELECT Id FROM MenuOgeleri WHERE Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = '3D / Konfigurator' LIMIT 1);

            UPDATE MenuOgeleri
            SET UstMenuId = (SELECT Id FROM MenuOgeleri WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Icerik ve Medya' LIMIT 1),
                GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0
              AND Konum = 'AdminSol'
              AND UstMenuId IN (
                  SELECT Id FROM MenuOgeleri
                  WHERE Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik IN ('Medya')
              );

            UPDATE MenuOgeleri
            SET UstMenuId = (SELECT Id FROM MenuOgeleri WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Icerik ve Medya' LIMIT 1),
                GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0
              AND Konum = 'AdminSol'
              AND Url = 'admin/katalog-yonetimi';

            UPDATE MenuOgeleri
            SET UstMenuId = (SELECT Id FROM MenuOgeleri WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Musteri ve Operasyon' LIMIT 1),
                GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0
              AND Konum = 'AdminSol'
              AND UstMenuId IN (
                  SELECT Id FROM MenuOgeleri
                  WHERE Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik IN ('İletişim / Destek', 'Organizasyon')
              );
            """,
            P("$simdi", simdi));

        await SiraAyarlaAsync(simdi, "admin/urun-sihirbazi", 1);
        await SiraAyarlaAsync(simdi, "admin/urun-yonetimi", 2);
        await SiraAyarlaAsync(simdi, "admin/uc-boyut-model-yonetimi", 3);
        await SiraAyarlaAsync(simdi, "admin/uc-boyut-parca-esleme", 4);
        await SiraAyarlaAsync(simdi, "admin/ral-renk-yonetimi", 5);
        await SiraAyarlaAsync(simdi, "admin/malzeme-yonetimi", 6);
        await SiraAyarlaAsync(simdi, "admin/kaplama-yonetimi", 7);
        await SiraAyarlaAsync(simdi, "admin/urun-ailesi-yonetimi", 8);
        await SiraAyarlaAsync(simdi, "admin/urun-kategori-yonetimi", 9);
        await SiraAyarlaAsync(simdi, "admin/sahne-ayarlari", 10);
        await SiraAyarlaAsync(simdi, "admin/konfigurasyon-sablonu-yonetimi", 11);
        await SiraAyarlaAsync(simdi, "admin/konfigurasyon-kurali-yonetimi", 12);

        await SiraAyarlaAsync(simdi, "admin/anasayfa-yonetimi", 1);
        await SiraAyarlaAsync(simdi, "admin/slayt-yonetimi", 2);
        await SiraAyarlaAsync(simdi, "admin/icerik-yonetimi", 3);
        await SiraAyarlaAsync(simdi, "admin/sayfa-yonetimi", 4);
        await SiraAyarlaAsync(simdi, "admin/blog-yonetimi", 5);
        await SiraAyarlaAsync(simdi, "admin/sss-yonetimi", 6);
        await SiraAyarlaAsync(simdi, "admin/seo-yonetimi", 7);
        await SiraAyarlaAsync(simdi, "admin/medya-havuzu", 8);
        await SiraAyarlaAsync(simdi, "admin/galeri", 9);
        await SiraAyarlaAsync(simdi, "admin/pdf-katalog-yonetimi", 10);
        await SiraAyarlaAsync(simdi, "admin/katalog-yonetimi", 11);

        await SiraAyarlaAsync(simdi, "admin/proje-yonetimi", 1);
        await SiraAyarlaAsync(simdi, "admin/referans-yonetimi", 2);
        await SiraAyarlaAsync(simdi, "admin/yorum-yonetimi", 3);
        await SiraAyarlaAsync(simdi, "admin/hizmet-adimi-yonetimi", 4);
        await SiraAyarlaAsync(simdi, "admin/bulten-yonetimi", 5);
        await SiraAyarlaAsync(simdi, "admin/eposta-sablonlari", 6);
        await SiraAyarlaAsync(simdi, "admin/iletisim-mesajlari", 7);
        await SiraAyarlaAsync(simdi, "admin/canli-sohbet", 8);
        await SiraAyarlaAsync(simdi, "admin/teklif-yonetimi", 9);
        await SiraAyarlaAsync(simdi, "admin/sube-yonetimi", 10);
        await SiraAyarlaAsync(simdi, "admin/ekip-yonetimi", 11);

        await KomutAsync(
            """
            UPDATE MenuOgeleri
            SET SilindiMi = 1,
                SilinmeTarihi = $simdi,
                GuncellenmeTarihi = $simdi
            WHERE SilindiMi = 0
              AND Konum = 'AdminSol'
              AND UstMenuId IS NULL
              AND Baslik IN ('3D / Konfigurator', 'Medya', 'İletişim / Destek', 'Organizasyon');
            """,
            P("$simdi", simdi));

        await islem.CommitAsync();
        Console.WriteLine("ADMIN_MENU_SADELESTIRILDI=1");
    }
    catch
    {
        await islem.RollbackAsync();
        throw;
    }
}

async Task SiraAyarlaAsync(string simdi, string url, int sira)
{
    await KomutAsync(
        """
        UPDATE MenuOgeleri
        SET Sira = $sira,
            GuncellenmeTarihi = $simdi
        WHERE SilindiMi = 0 AND Konum = 'AdminSol' AND Url = $url;
        """,
        P("$sira", sira),
        P("$simdi", simdi),
        P("$url", url));
}

async Task KomutAsync(string sql, params SqliteParameter[] parametreler)
{
    await using var komut = baglanti.CreateCommand();
    komut.CommandText = sql;
    komut.Parameters.AddRange(parametreler);
    await komut.ExecuteNonQueryAsync();
}

static SqliteParameter P(string ad, object? deger)
    => new(ad, deger ?? DBNull.Value);

static string? ArgumanDegeri(string[] argumanlar, string anahtar)
{
    for (var i = 0; i < argumanlar.Length - 1; i++)
    {
        if (string.Equals(argumanlar[i], anahtar, StringComparison.OrdinalIgnoreCase))
            return argumanlar[i + 1];
    }

    return null;
}

async Task UrunUcBoyutKapatAsync(string slug)
{
    var simdi = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    await using var islem = await baglanti.BeginTransactionAsync();

    try
    {
        await KomutAsync(
            """
            UPDATE UrunUcBoyutModelleri
            SET AktifMi = 0,
                VarsayilanMi = 0,
                GuncellenmeTarihi = $simdi
            WHERE UrunId = (
                SELECT Id FROM Urunler
                WHERE Slug = $slug AND SilindiMi = 0
                LIMIT 1
            );

            UPDATE Urunler
            SET VarsayilanUcBoyutModeliId = NULL,
                GuncellenmeTarihi = $simdi
            WHERE Slug = $slug AND SilindiMi = 0;
            """,
            P("$simdi", simdi),
            P("$slug", slug));

        await islem.CommitAsync();
        Console.WriteLine($"URUN_3D_KAPATILDI={slug}");
    }
    catch
    {
        await islem.RollbackAsync();
        throw;
    }
}
