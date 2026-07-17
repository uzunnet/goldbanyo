using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

var kok = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
var vtYolu = Path.Combine(kok, "VIZITLINK3D.Api", "VIZITLINK3D.db");
var manifestYolu = Path.Combine(kok, "VIZITLINK3D.UI", "wwwroot", "medya", "katalog", "manifest.json");

if (!File.Exists(vtYolu))
{
    Console.Error.WriteLine($"Veritabani bulunamadi: {vtYolu}");
    return 2;
}

if (!File.Exists(manifestYolu))
{
    Console.Error.WriteLine($"Manifest bulunamadi: {manifestYolu}");
    return 3;
}

await using var baglanti = new SqliteConnection($"Data Source={vtYolu}");
await baglanti.OpenAsync();

await using var islem = await baglanti.BeginTransactionAsync();
try
{
    var simdi = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestYolu));
    var kokEleman = manifest.RootElement;

    var medyaKlasorId = await KlasorIdGetirVeyaOlustur("Katalog Varliklari", "katalog-varliklari", simdi);
    var urunAilesiId = await UrunAilesiGetirVeyaOlustur(simdi);
    var kategoriId = await UrunKategoriGetirVeyaOlustur(simdi);

    var eklenenUrun = 0;
    var guncellenenUrun = 0;
    var eklenenMedya = 0;
    var eklenenUrunMedya = 0;
    var eklenenModel = 0;
    var eklenenKatalog = 0;
    var eklenenSlayt = 0;

    foreach (var urun in kokEleman.GetProperty("urunler").EnumerateArray())
    {
        var kod = urun.GetProperty("kod").GetString() ?? "";
        if (string.IsNullOrWhiteSpace(kod))
            continue;

        var urunKodu = $"NRD-{kod.PadLeft(3, '0')}";
        var slug = $"nrd-{kod.PadLeft(3, '0')}";
        var resimler = urun.GetProperty("resimler").EnumerateArray().ToList();
        var modeller = urun.GetProperty("modeller").EnumerateArray().ToList();
        var ad = UrunAdiOlustur(kod, resimler, modeller);

        var urunId = await TekilIntAsync("SELECT Id FROM Urunler WHERE (Kod = $kod OR Slug = $slug) AND SilindiMi = 0 LIMIT 1",
            P("$kod", urunKodu), P("$slug", slug));

        if (urunId == 0)
        {
            urunId = await ExecuteScalarIntAsync(
                """
                INSERT INTO Urunler
                    (Slug, Kod, Ad, KisaAciklama, Aciklama, UrunAilesiId, UrunKategoriId, AktifMi, OneCikanMi, YeniMi,
                     Fiyat, Birim, SiraNo, OlusturulmaTarihi, GuncellenmeTarihi, SeoBaslik, SeoAciklama, SilindiMi)
                VALUES
                    ($slug, $kod, $ad, $kisa, $aciklama, $aile, $kategori, 1, 0, 0,
                     NULL, NULL, $sira, $simdi, NULL, $seoBaslik, $seoAciklama, 0);
                SELECT last_insert_rowid();
                """,
                P("$slug", slug),
                P("$kod", urunKodu),
                P("$ad", ad),
                P("$kisa", "VIZITLINK3D katalog varliklarindan otomatik eslestirilen urun."),
                P("$aciklama", "Bu urun kaydi katalog gorselleri, slayt kaynaklari ve 3D GLB dosyalari eslestirilerek olusturuldu."),
                P("$aile", urunAilesiId),
                P("$kategori", kategoriId),
                P("$sira", SiraNo(kod)),
                P("$simdi", simdi),
                P("$seoBaslik", $"{ad} | VIZITLINK3D"),
                P("$seoAciklama", $"{ad} icin katalog gorselleri ve 3D model dosyalari."));
            eklenenUrun++;
        }
        else
        {
            await KomutAsync(
                """
                UPDATE Urunler
                SET UrunAilesiId = COALESCE(NULLIF(UrunAilesiId, 0), $aile),
                    UrunKategoriId = COALESCE(UrunKategoriId, $kategori),
                    GuncellenmeTarihi = $simdi
                WHERE Id = $id;
                """,
                P("$aile", urunAilesiId),
                P("$kategori", kategoriId),
                P("$simdi", simdi),
                P("$id", urunId));
            guncellenenUrun++;
        }

        long? anaGorselMedyaId = null;
        long? onizlemeMedyaId = null;
        var resimSira = 1;
        var anaResimYolu = AnaResimSec(resimler);

        foreach (var resim in resimler)
        {
            var webYolu = resim.GetProperty("webYolu").GetString() ?? "";
            if (string.IsNullOrWhiteSpace(webYolu))
                continue;

            var medyaId = await MedyaGetirVeyaOlustur(resim, medyaKlasorId, simdi);
            if (medyaId.YeniMi)
                eklenenMedya++;

            var medyaUrl = webYolu;
            var anaMi = webYolu == anaResimYolu;
            if (anaMi)
                anaGorselMedyaId = medyaId.Id;
            onizlemeMedyaId ??= medyaId.Id;

            var urunMedyaVarMi = await TekilIntAsync("SELECT Id FROM UrunMedyalari WHERE UrunId = $urunId AND MedyaUrl = $url LIMIT 1",
                P("$urunId", urunId), P("$url", medyaUrl));

            if (urunMedyaVarMi == 0)
            {
                await KomutAsync(
                    """
                    INSERT INTO UrunMedyalari (UrunId, MedyaUrl, MedyaTuru, Aciklama, SiraNo, AnaGosterim)
                    VALUES ($urunId, $url, $tur, $aciklama, $sira, $ana);
                    """,
                    P("$urunId", urunId),
                    P("$url", medyaUrl),
                    P("$tur", MedyaTuru(resim)),
                    P("$aciklama", resim.GetProperty("ad").GetString() ?? ad),
                    P("$sira", resimSira++),
                    P("$ana", anaMi ? 1 : 0));
                eklenenUrunMedya++;
            }
            else if (anaMi)
            {
                await KomutAsync("UPDATE UrunMedyalari SET AnaGosterim = 1 WHERE Id = $id", P("$id", urunMedyaVarMi));
            }
        }

        int? varsayilanModelId = null;
        var modelSira = 1;
        foreach (var model in modeller)
        {
            var webYolu = model.GetProperty("webYolu").GetString() ?? "";
            if (string.IsNullOrWhiteSpace(webYolu))
                continue;

            var medyaId = await MedyaGetirVeyaOlustur(model, medyaKlasorId, simdi, 3, "model/gltf-binary");
            if (medyaId.YeniMi)
                eklenenMedya++;

            var modelVarMi = await TekilIntAsync("SELECT Id FROM UrunUcBoyutModelleri WHERE UrunId = $urunId AND ModelYolu = $yol AND SilindiMi = 0 LIMIT 1",
                P("$urunId", urunId), P("$yol", webYolu));

            if (modelVarMi == 0)
            {
                var modelId = await ExecuteScalarIntAsync(
                    """
                    INSERT INTO UrunUcBoyutModelleri
                        (UrunId, ModelAdi, ModelDosyaYolu, ModelTipi, ModelYolu, MedyaId, OnizlemeMedyaId, DosyaBoyutuByte,
                         KameraAyarJson, IsikAyarJson, CevreAyarJson, VarsayilanMi, Versiyon, AktifMi,
                         OlusturulmaTarihi, GuncellenmeTarihi, SilindiMi)
                    VALUES
                        ($urunId, $ad, $dosyaYolu, 'GLB', $modelYolu, $medyaId, $onizleme, $boyut,
                         $kamera, $isik, $cevre, $varsayilan, 1, 1,
                         $simdi, NULL, 0);
                    SELECT last_insert_rowid();
                    """,
                    P("$urunId", urunId),
                    P("$ad", ModelAdi(model)),
                    P("$dosyaYolu", webYolu),
                    P("$modelYolu", webYolu),
                    P("$medyaId", medyaId.Id),
                    P("$onizleme", onizlemeMedyaId.HasValue ? onizlemeMedyaId.Value : DBNull.Value),
                    P("$boyut", model.TryGetProperty("boyut", out var boyut) ? boyut.GetInt64() : 0),
                    P("$kamera", "{\"baslangicAciX\":0.15,\"baslangicAciY\":0.42,\"baslangicAciZ\":3.25,\"hedefYukseklik\":0.05,\"otomatikDonme\":false}"),
                    P("$isik", "{\"siddet\":1.08,\"pozlama\":1.08}"),
                    P("$cevre", "{\"arkaPlanRengi\":\"#F6F1E8\"}"),
                    P("$varsayilan", modelSira == 1 ? 1 : 0),
                    P("$simdi", simdi));
                varsayilanModelId ??= modelId;
                eklenenModel++;
            }
            else
            {
                varsayilanModelId ??= modelVarMi;
            }

            modelSira++;
        }

        await KomutAsync(
            """
            UPDATE Urunler
            SET AnaGorselMedyaId = COALESCE(AnaGorselMedyaId, $anaMedya),
                VarsayilanUcBoyutModeliId = COALESCE(VarsayilanUcBoyutModeliId, $modelId),
                GuncellenmeTarihi = $simdi
            WHERE Id = $urunId;
            """,
            P("$anaMedya", anaGorselMedyaId.HasValue ? anaGorselMedyaId.Value : DBNull.Value),
            P("$modelId", varsayilanModelId.HasValue ? varsayilanModelId.Value : DBNull.Value),
            P("$simdi", simdi),
            P("$urunId", urunId));
    }

    if (kokEleman.TryGetProperty("pdfler", out var pdfler))
    {
        var pdfSira = 1;
        foreach (var pdf in pdfler.EnumerateArray())
        {
            if (!pdf.TryGetProperty("webYolu", out var yolEleman))
                continue;

            var webYolu = yolEleman.GetString() ?? "";
            if (string.IsNullOrWhiteSpace(webYolu))
                continue;

            var medyaId = await MedyaGetirVeyaOlustur(pdf, medyaKlasorId, simdi, 2, "application/pdf");
            if (medyaId.YeniMi)
                eklenenMedya++;

            var katalogVarMi = await TekilIntAsync("SELECT Id FROM Kataloglar WHERE PdfDosyaYolu = $yol LIMIT 1", P("$yol", webYolu));
            if (katalogVarMi == 0)
            {
                var baslik = Path.GetFileNameWithoutExtension(pdf.GetProperty("ad").GetString() ?? "Katalog");
                await KomutAsync(
                    """
                    INSERT INTO Kataloglar
                        (Baslik, Aciklama, KapakResim, PdfDosyaYolu, DosyaBoyutuMb, SayfaSayisi, Yil, IndirilmeSayisi, SiraNo, AktifMi, OlusturulmaTarihi)
                    VALUES
                        ($baslik, $aciklama, NULL, $yol, $mb, NULL, 2026, 0, $sira, 1, $simdi);
                    """,
                    P("$baslik", TemizAd(baslik)),
                    P("$aciklama", "VIZITLINK3D katalog PDF dosyasi."),
                    P("$yol", webYolu),
                    P("$mb", Math.Round((pdf.TryGetProperty("boyut", out var b) ? b.GetInt64() : 0) / 1024d / 1024d, 2)),
                    P("$sira", pdfSira++),
                    P("$simdi", simdi));
                eklenenKatalog++;
            }
        }
    }

    if (kokEleman.GetProperty("eslesmeyen").TryGetProperty("resimler", out var ozelResimler))
    {
        var sira = await TekilIntAsync("SELECT COALESCE(MAX(SiraNo), 0) FROM Slaytlar WHERE SilindiMi = 0") + 1;
        foreach (var resim in ozelResimler.EnumerateArray())
        {
            var ad = resim.GetProperty("ad").GetString() ?? "";
            var webYolu = resim.GetProperty("webYolu").GetString() ?? "";
            if (string.IsNullOrWhiteSpace(webYolu) || !SlaytOlabilirMi(ad))
                continue;

            var slaytVarMi = await TekilIntAsync("SELECT Id FROM Slaytlar WHERE ArkaplanResim = $yol AND SilindiMi = 0 LIMIT 1", P("$yol", webYolu));
            if (slaytVarMi != 0)
                continue;

            await KomutAsync(
                """
                INSERT INTO Slaytlar
                    (Dil, Baslik, AltBaslik, Aciklama, ArkaplanResim, ArkaplanResimMobil, ButonMetni1, ButonLink1,
                     ButonMetni2, ButonLink2, AnimasyonTipi, GecisHizi, GosterimSuresi, MetinHizalama, MetinRengi,
                     SiraNo, AktifMi, BaslangicTarihi, BitisTarihi, OlusturulmaTarihi, SilindiMi)
                VALUES
                    ('tr', $baslik, 'VIZITLINK3D', 'Katalog varliklarindan otomatik eklenen slayt gorseli.', $yol, NULL,
                     'Urunleri Incele', '/urunler', NULL, NULL, 'fade', 800, 5000, 'sol', NULL,
                     $sira, 1, NULL, NULL, $simdi, 0);
                """,
                P("$baslik", TemizAd(Path.GetFileNameWithoutExtension(ad))),
                P("$yol", webYolu),
                P("$sira", sira++),
                P("$simdi", simdi));
            eklenenSlayt++;
        }
    }

    await islem.CommitAsync();

    Console.WriteLine($"EklenenUrun={eklenenUrun}");
    Console.WriteLine($"GuncellenenUrun={guncellenenUrun}");
    Console.WriteLine($"EklenenMedya={eklenenMedya}");
    Console.WriteLine($"EklenenUrunMedya={eklenenUrunMedya}");
    Console.WriteLine($"EklenenModel={eklenenModel}");
    Console.WriteLine($"EklenenKatalog={eklenenKatalog}");
    Console.WriteLine($"EklenenSlayt={eklenenSlayt}");
    return 0;
}
catch
{
    await islem.RollbackAsync();
    throw;
}

async Task<int> UrunAilesiGetirVeyaOlustur(string simdi)
{
    var id = await TekilIntAsync("SELECT Id FROM UrunAilesileri WHERE Slug = 'dolap-kapagi' AND SilindiMi = 0 LIMIT 1");
    if (id != 0)
        return id;

    return await ExecuteScalarIntAsync(
        """
        INSERT INTO UrunAilesileri (Ad, Slug, Aciklama, VarsayilanDetaySablonu, SiraNo, AktifMi, OlusturulmaTarihi, SilindiMi)
        VALUES ('Dolap Kapagi', 'dolap-kapagi', 'Katalogdan aktarilan kapak ve aksesuar modelleri.', 'KatalogGorselAgirlikli', 50, 1, $simdi, 0);
        SELECT last_insert_rowid();
        """,
        P("$simdi", simdi));
}

async Task<int> UrunKategoriGetirVeyaOlustur(string simdi)
{
    var id = await TekilIntAsync("SELECT Id FROM UrunKategorileri WHERE Slug = 'katalog-modelleri' AND SilindiMi = 0 LIMIT 1");
    if (id != 0)
        return id;

    return await ExecuteScalarIntAsync(
        """
        INSERT INTO UrunKategorileri (Ad, Aciklama, Slug, UstKategoriId, SiraNo, AktifMi, OlusturulmaTarihi, SilindiMi)
        VALUES ('Katalog Modelleri', 'PDF ve 3D katalogdan aktarilan modeller.', 'katalog-modelleri', NULL, 50, 1, $simdi, 0);
        SELECT last_insert_rowid();
        """,
        P("$simdi", simdi));
}

async Task<int> KlasorIdGetirVeyaOlustur(string ad, string slug, string simdi)
{
    var id = await TekilIntAsync("SELECT Id FROM MedyaKlasorleri WHERE Slug = $slug LIMIT 1", P("$slug", slug));
    if (id != 0)
        return id;

    return await ExecuteScalarIntAsync(
        """
        INSERT INTO MedyaKlasorleri (FirmaId, UstKlasorId, Ad, Slug, Ikon, Renk, SiraNo, AktifMi, OlusturulmaTarihi)
        VALUES (NULL, NULL, $ad, $slug, 'Inventory2', '#C5A059', 50, 1, $simdi);
        SELECT last_insert_rowid();
        """,
        P("$ad", ad),
        P("$slug", slug),
        P("$simdi", simdi));
}

async Task<(long Id, bool YeniMi)> MedyaGetirVeyaOlustur(JsonElement kaynak, int klasorId, string simdi, int? tip = null, string? mime = null)
{
    var webYolu = kaynak.GetProperty("webYolu").GetString() ?? "";
    var dosyaYolu = webYolu.TrimStart('/');
    var varOlan = await TekilLongAsync("SELECT Id FROM Medyalar WHERE DosyaYolu = $yol AND SilindiMi = 0 LIMIT 1", P("$yol", dosyaYolu));
    if (varOlan != 0)
        return (varOlan, false);

    var ad = kaynak.TryGetProperty("ad", out var adEleman) ? adEleman.GetString() ?? Path.GetFileName(dosyaYolu) : Path.GetFileName(dosyaYolu);
    var boyut = kaynak.TryGetProperty("boyut", out var boyutEleman) ? boyutEleman.GetInt64() : 0L;
    var medyaTipi = tip ?? MedyaTipiTahminEt(dosyaYolu);
    var mimeTipi = mime ?? MimeTipi(dosyaYolu);

    var id = await ExecuteScalarLongAsync(
        """
        INSERT INTO Medyalar
            (FirmaId, Tip, Kaynak, Ad, OrijinalAd, DosyaYolu, MiniaturYolu, KaynakUrl, BoyutByte,
             Genislik, Yukseklik, SureSaniye, MimeTipi, Hash, AltMetin, Aciklama, EtiketlerJson,
             KlasorId, KullanimSayisi, YukleyenKullaniciId, SilindiMi, OlusturulmaTarihi, GuncellenmeTarihi)
        VALUES
            (NULL, $tip, 0, $ad, $orijinal, $dosyaYolu, NULL, $kaynakUrl, $boyut,
             NULL, NULL, NULL, $mime, NULL, $alt, 'Katalog varlik aktarimi', NULL,
             $klasor, 0, NULL, 0, $simdi, NULL);
        SELECT last_insert_rowid();
        """,
        P("$tip", medyaTipi),
        P("$ad", TemizAd(Path.GetFileNameWithoutExtension(ad))),
        P("$orijinal", ad),
        P("$dosyaYolu", dosyaYolu),
        P("$kaynakUrl", webYolu),
        P("$boyut", boyut),
        P("$mime", mimeTipi),
        P("$alt", TemizAd(Path.GetFileNameWithoutExtension(ad))),
        P("$klasor", klasorId),
        P("$simdi", simdi));
    return (id, true);
}

static SqliteParameter P(string ad, object? deger)
    => new(ad, deger ?? DBNull.Value);

async Task KomutAsync(string sql, params SqliteParameter[] parametreler)
{
    await using var komut = baglanti.CreateCommand();
    komut.Transaction = (SqliteTransaction)islem;
    komut.CommandText = sql;
    komut.Parameters.AddRange(parametreler);
    await komut.ExecuteNonQueryAsync();
}

async Task<int> TekilIntAsync(string sql, params SqliteParameter[] parametreler)
    => Convert.ToInt32(await TekilAsync(sql, parametreler) ?? 0, CultureInfo.InvariantCulture);

async Task<long> TekilLongAsync(string sql, params SqliteParameter[] parametreler)
    => Convert.ToInt64(await TekilAsync(sql, parametreler) ?? 0, CultureInfo.InvariantCulture);

async Task<object?> TekilAsync(string sql, params SqliteParameter[] parametreler)
{
    await using var komut = baglanti.CreateCommand();
    komut.Transaction = (SqliteTransaction)islem;
    komut.CommandText = sql;
    komut.Parameters.AddRange(parametreler);
    var sonuc = await komut.ExecuteScalarAsync();
    return sonuc is DBNull ? null : sonuc;
}

async Task<int> ExecuteScalarIntAsync(string sql, params SqliteParameter[] parametreler)
    => Convert.ToInt32(await ExecuteScalarAsync(sql, parametreler), CultureInfo.InvariantCulture);

async Task<long> ExecuteScalarLongAsync(string sql, params SqliteParameter[] parametreler)
    => Convert.ToInt64(await ExecuteScalarAsync(sql, parametreler), CultureInfo.InvariantCulture);

async Task<object?> ExecuteScalarAsync(string sql, params SqliteParameter[] parametreler)
{
    await using var komut = baglanti.CreateCommand();
    komut.Transaction = (SqliteTransaction)islem;
    komut.CommandText = sql;
    komut.Parameters.AddRange(parametreler);
    return await komut.ExecuteScalarAsync();
}

static string AnaResimSec(List<JsonElement> resimler)
{
    var kapak = resimler.FirstOrDefault(r => r.TryGetProperty("tur", out var t) && t.GetString() == "kapak");
    if (kapak.ValueKind != JsonValueKind.Undefined)
        return kapak.GetProperty("webYolu").GetString() ?? "";

    var uygulama = resimler.FirstOrDefault(r => r.TryGetProperty("tur", out var t) && t.GetString() == "uygulama");
    if (uygulama.ValueKind != JsonValueKind.Undefined)
        return uygulama.GetProperty("webYolu").GetString() ?? "";

    return resimler.FirstOrDefault().ValueKind == JsonValueKind.Undefined
        ? ""
        : resimler.First().GetProperty("webYolu").GetString() ?? "";
}

static string UrunAdiOlustur(string kod, List<JsonElement> resimler, List<JsonElement> modeller)
{
    var kaynak = resimler.FirstOrDefault(r => r.TryGetProperty("tur", out var t) && t.GetString() == "kapak");
    if (kaynak.ValueKind == JsonValueKind.Undefined)
        kaynak = resimler.FirstOrDefault();

    if (kaynak.ValueKind != JsonValueKind.Undefined)
    {
        var ad = Path.GetFileNameWithoutExtension(kaynak.GetProperty("ad").GetString() ?? "");
        if (Regex.IsMatch(ad, $"^{Regex.Escape(kod)}\\s*(KAPAK|KAPAKLAR)?$", RegexOptions.IgnoreCase))
            return $"NRD {kod.PadLeft(3, '0')} Kapak Modeli";
        return TemizAd(ad);
    }

    if (modeller.Count > 0)
        return TemizAd(Path.GetFileNameWithoutExtension(modeller[0].GetProperty("ad").GetString() ?? $"NRD {kod}"));

    return $"NRD {kod.PadLeft(3, '0')} Katalog Modeli";
}

static string ModelAdi(JsonElement model)
    => TemizAd(Path.GetFileNameWithoutExtension(model.GetProperty("ad").GetString() ?? "3D Model"));

static int SiraNo(string kod) => int.TryParse(kod, out var sayi) ? sayi : 9999;

static string MedyaTuru(JsonElement resim)
{
    var tur = resim.TryGetProperty("tur", out var turEleman) ? turEleman.GetString() : null;
    return tur switch
    {
        "kapak" => "Kapak",
        "uygulama" => "Uygulama",
        "thumbnail" => "Thumbnail",
        _ => "Gorsel"
    };
}

static int MedyaTipiTahminEt(string yol)
{
    var uzanti = Path.GetExtension(yol).ToLowerInvariant();
    return uzanti switch
    {
        ".pdf" => 2,
        ".glb" => 3,
        ".mp4" or ".webm" => 1,
        _ => 0
    };
}

static string MimeTipi(string yol)
{
    var uzanti = Path.GetExtension(yol).ToLowerInvariant();
    return uzanti switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        ".glb" => "model/gltf-binary",
        ".svg" => "image/svg+xml",
        _ => "application/octet-stream"
    };
}

static bool SlaytOlabilirMi(string ad)
{
    var kucuk = ad.ToLowerInvariant();
    return kucuk.Contains("slayt", StringComparison.Ordinal)
        || kucuk.Contains("fabrika", StringComparison.Ordinal)
        || kucuk.Contains("yaşam", StringComparison.Ordinal)
        || kucuk.Contains("yasam", StringComparison.Ordinal);
}

static string TemizAd(string ad)
{
    var temiz = Regex.Replace(ad.Replace('_', ' ').Replace('-', ' '), "\\s+", " ").Trim();
    if (string.IsNullOrWhiteSpace(temiz))
        return "Katalog Varligi";

    return temiz;
}
