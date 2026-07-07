using System.Globalization;
using System.Net;
using System.Text;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public sealed class GoldBanyoKatalogUrunu
{
    private static readonly CultureInfo TrKultur = new("tr-TR");

    public required string Slug { get; init; }
    public required string Ad { get; init; }
    public required string Kod { get; init; }
    public required string KoleksiyonGrubu { get; init; }
    public required int SayfaNo { get; init; }
    public int[] EkGaleriSayfalari { get; init; } = [];
    public required decimal Fiyat { get; init; }
    public decimal? BoyDolabiFiyati { get; init; }
    public required string[] Renkler { get; init; }
    public required string[] Ozellikler { get; init; }
    public required GoldBanyoKatalogOlcusu[] Olculer { get; init; }
    public bool OneCikanMi { get; init; } = true;
    public bool YeniMi { get; init; } = true;

    public IReadOnlyList<int> TumSayfalar => [SayfaNo, .. EkGaleriSayfalari];
    public string HeroGorselUrl => $"/medya/gold-katalog/sayfa-{SayfaNo:000}-hero.png";
    public string KatalogGorselUrl => $"/medya/gold-katalog/sayfa-{SayfaNo:000}-spread.png";
    public string TeknikGorselUrl => $"/medya/gold-katalog/sayfa-{SayfaNo:000}-detay.png";

    public string KisaAciklamaOlustur()
    {
        var ilkOzellikler = string.Join(", ", Ozellikler.Take(3));
        return $"{KoleksiyonGrubu} koleksiyonundaki {Ad}; {ilkOzellikler.ToLowerInvariant()} ve {Renkler.Length} renk seçeneğiyle sunulur.";
    }

    public string AciklamaHtmlOlustur()
    {
        var olcuHtml = new StringBuilder();
        foreach (var olcu in Olculer)
        {
            olcuHtml.Append("<div class=\"gb-olcu-kart\">");
            olcuHtml.Append($"<h4>{Kodla(olcu.Baslik)}</h4>");
            olcuHtml.Append("<ul>");
            olcuHtml.Append($"<li>Yükseklik: {Kodla(olcu.Yukseklik)}</li>");
            olcuHtml.Append($"<li>Genişlik: {Kodla(olcu.Genislik)}</li>");
            olcuHtml.Append($"<li>Derinlik: {Kodla(olcu.Derinlik)}</li>");
            olcuHtml.Append("</ul></div>");
        }

        var ozellikHtml = string.Join(string.Empty, Ozellikler.Select(o => $"<li>{Kodla(o)}</li>"));
        var renkHtml = string.Join(string.Empty, Renkler.Select(r => $"<li>{Kodla(r)}</li>"));
        var tumFiyatlar = BoyDolabiFiyati.HasValue
            ? $"<p><strong>Liste Fiyatı:</strong> {FiyatYaz(Fiyat)}<br /><strong>Boy Dolabı:</strong> {FiyatYaz(BoyDolabiFiyati.Value)}</p>"
            : $"<p><strong>Liste Fiyatı:</strong> {FiyatYaz(Fiyat)}</p>";

        return $"""
            <div class="gb-katalog-icerik">
              <p class="gb-katalog-rozet">{Kodla(KoleksiyonGrubu)} Koleksiyonu</p>
              <h2>{Kodla(Ad)}</h2>
              <p>{Kodla(Ad)}, Gold Banyo 2026 katalog sayfa kompozisyonu ile birebir beslenen; koleksiyon renkleri, teknik ölçüler ve fiyat etiketi korunmuş özel bir ürün sayfasıdır.</p>
              {tumFiyatlar}
              <h3>Özellikler</h3>
              <ul>{ozellikHtml}</ul>
              <h3>Renkler</h3>
              <ul>{renkHtml}</ul>
              <h3>Teknik Ölçüler</h3>
              <div class="gb-olcu-grid">{olcuHtml}</div>
            </div>
            """;
    }

    private static string Kodla(string deger) => WebUtility.HtmlEncode(deger);

    private static string FiyatYaz(decimal fiyat) => fiyat.ToString("N0", TrKultur) + " TL";
}

public sealed class GoldBanyoKatalogOlcusu
{
    public required string Baslik { get; init; }
    public required string Yukseklik { get; init; }
    public required string Genislik { get; init; }
    public required string Derinlik { get; init; }
}

public static class GoldBanyoKatalogUrunleri
{
    private static readonly Dictionary<string, int> KoleksiyonSiralari = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Exclusive"] = 1,
        ["Premium"] = 2,
        ["Trend"] = 3,
        ["Standart"] = 4,
        ["Diger"] = 5
    };

    private static readonly Dictionary<string, string> KoleksiyonAciklamalari = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Exclusive"] = "İmza koleksiyonları. Geniş modül yapısı, yüksek yüzey kalitesi ve katalog sayfasındaki premium sunum ile öne çıkar.",
        ["Premium"] = "Zengin malzeme ve renk kombinasyonlarıyla showroom etkisi veren seçkin ürün grubu.",
        ["Trend"] = "Çağdaş banyolar için dengeli fiyat, güçlü malzeme ve sıcak renk seçimleri sunan seri.",
        ["Standart"] = "Günlük kullanıma uygun, ölçü dengesi ve fiyat avantajı yüksek Gold Banyo ürünleri.",
        ["Diger"] = "Özel içerik olarak eklenmiş ürünler."
    };

    public static int KoleksiyonSirasiBul(string koleksiyonGrubu) =>
        KoleksiyonSiralari.TryGetValue(koleksiyonGrubu, out var sira) ? sira : int.MaxValue;

    public static string KoleksiyonAciklamasiBul(string koleksiyonGrubu) =>
        KoleksiyonAciklamalari.TryGetValue(koleksiyonGrubu, out var aciklama) ? aciklama : string.Empty;

    public static IReadOnlyList<GoldBanyoKatalogUrunu> Tum { get; } =
    [
        new()
        {
            Slug = "hermes-120",
            Ad = "Hermes 120",
            Kod = "HERMES-120",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 6,
            Fiyat = 122500m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Siyah", "Bej", "Kahve", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "160 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "hermes-80",
            Ad = "Hermes 80",
            Kod = "HERMES-80",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 7,
            Fiyat = 102000m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Siyah", "Bej", "Kahve", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "160 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "bottega-120",
            Ad = "Bottega 120",
            Kod = "BOTTEGA-120",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 8,
            Fiyat = 117500m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Beyaz", "Siyah"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "110 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "bottega-100",
            Ad = "Bottega 100",
            Kod = "BOTTEGA-100",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 9,
            Fiyat = 110000m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Beyaz", "Siyah"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "100 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "bottega-80",
            Ad = "Bottega 80",
            Kod = "BOTTEGA-80",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 10,
            Fiyat = 102000m,
            Renkler = ["Beyaz", "Siyah"],
            BoyDolabiFiyati = null,
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "80 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "giorgio-120",
            Ad = "Giorgio 120",
            Kod = "GIORGIO-120",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 12,
            Fiyat = 117500m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Gri", "Siyah", "Ahşap"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "100 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "giorgio-100",
            Ad = "Giorgio 100",
            Kod = "GIORGIO-100",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 13,
            Fiyat = 110000m,
            BoyDolabiFiyati = 60000m,
            Renkler = ["Gri", "Siyah", "Ahşap"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "100 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "giorgio-80",
            Ad = "Giorgio 80",
            Kod = "GIORGIO-80",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 14,
            Fiyat = 102000m,
            Renkler = ["Gri", "Siyah", "Ahşap"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Stone Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "diago-120",
            Ad = "Diago 120",
            Kod = "DIAGO-120",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 16,
            Fiyat = 112500m,
            BoyDolabiFiyati = 55000m,
            Renkler = ["Siyah", "Kahve Rengi"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Cam Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "diago-100",
            Ad = "Diago 100",
            Kod = "DIAGO-100",
            KoleksiyonGrubu = "Exclusive",
            SayfaNo = 17,
            Fiyat = 99500m,
            Renkler = ["Siyah", "Kahve Rengi"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir", "Cam Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "capelli-150",
            Ad = "Capelli 150",
            Kod = "CAPELLI-150",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 19,
            Fiyat = 97000m,
            Renkler = ["Mocha", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "150 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "capelli-100",
            Ad = "Capelli 100",
            Kod = "CAPELLI-100",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 20,
            Fiyat = 67500m,
            Renkler = ["Mocha", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "hera-120",
            Ad = "Hera 120",
            Kod = "HERA-120",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 21,
            EkGaleriSayfalari = [22],
            Fiyat = 110000m,
            Renkler = ["Hareli Meşe", "Antik Ceviz"],
            Ozellikler = ["Soft Kapak", "Doğal Ağaç", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "46,5 cm" },
                new() { Baslik = "Ayna", Yukseklik = "69 cm", Genislik = "120 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "hera-80",
            Ad = "Hera 80",
            Kod = "HERA-80",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 23,
            Fiyat = 104000m,
            Renkler = ["Hareli Meşe", "Antik Ceviz"],
            Ozellikler = ["Soft Kapak", "Doğal Ağaç", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46,5 cm" },
                new() { Baslik = "Ayna", Yukseklik = "69 cm", Genislik = "80 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "cavalli-110",
            Ad = "Cavalli 110",
            Kod = "CAVALLI-110",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 24,
            EkGaleriSayfalari = [25],
            Fiyat = 72000m,
            BoyDolabiFiyati = 40000m,
            Renkler = ["Antrasit", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "110 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "45 cm", Derinlik = "32 cm" }
            ]
        },
        new()
        {
            Slug = "cavalli-90",
            Ad = "Cavalli 90",
            Kod = "CAVALLI-90",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 26,
            Fiyat = 66000m,
            Renkler = ["Antrasit", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "90 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "45 cm", Derinlik = "32 cm" }
            ]
        },
        new()
        {
            Slug = "tiffany-120",
            Ad = "Tiffany 120",
            Kod = "TIFFANY-120",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 28,
            Fiyat = 67500m,
            BoyDolabiFiyati = 30000m,
            Renkler = ["Antrasit", "Beyaz", "Mocha"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "120 cm", Derinlik = "45 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "120 cm", Genislik = "36 cm", Derinlik = "36 cm" }
            ]
        },
        new()
        {
            Slug = "tiffany-100",
            Ad = "Tiffany 100",
            Kod = "TIFFANY-100",
            KoleksiyonGrubu = "Premium",
            SayfaNo = 29,
            EkGaleriSayfalari = [30],
            Fiyat = 62000m,
            Renkler = ["Antrasit", "Beyaz", "Mocha"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "45 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "120 cm", Genislik = "36 cm", Derinlik = "36 cm" }
            ]
        },
        new()
        {
            Slug = "siena-80",
            Ad = "Siena 80",
            Kod = "SIENA-80",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 31,
            EkGaleriSayfalari = [32],
            Fiyat = 97500m,
            BoyDolabiFiyati = 30000m,
            Renkler = ["Bej", "Gri"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "60 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "valentino-100",
            Ad = "Valentino 100",
            Kod = "VALENTINO-100",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 33,
            EkGaleriSayfalari = [34],
            Fiyat = 67500m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Spesiyal Gri", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "70 cm", Genislik = "90 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "zanussi-100",
            Ad = "Zanussi 100",
            Kod = "ZANUSSI-100",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 35,
            Fiyat = 75000m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Spesiyal Gri", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "zanussi-80",
            Ad = "Zanussi 80",
            Kod = "ZANUSSI-80",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 36,
            Fiyat = 67500m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Spesiyal Gri", "Krem"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "50 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "perla-80",
            Ad = "Perla 80",
            Kod = "PERLA-80",
            KoleksiyonGrubu = "Trend",
            SayfaNo = 37,
            EkGaleriSayfalari = [38, 39],
            Fiyat = 57500m,
            BoyDolabiFiyati = 27500m,
            Renkler = ["Siyah", "Yeşil", "Turuncu"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Cam Lavabo"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "90 cm", Genislik = "70 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "valencia-90",
            Ad = "Valencia 90",
            Kod = "VALENCIA-90",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 40,
            Fiyat = 49000m,
            Renkler = ["Amerikan Ceviz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "90 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" }
            ]
        },
        new()
        {
            Slug = "otto-100",
            Ad = "Otto 100",
            Kod = "OTTO-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 41,
            Fiyat = 59900m,
            BoyDolabiFiyati = 37500m,
            Renkler = ["Gri", "Kum Beji"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "otto-80",
            Ad = "Otto 80",
            Kod = "OTTO-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 42,
            Fiyat = 57500m,
            BoyDolabiFiyati = 37500m,
            Renkler = ["Gri", "Kum Beji"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "otto-65",
            Ad = "Otto 65",
            Kod = "OTTO-65",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 43,
            Fiyat = 55000m,
            BoyDolabiFiyati = 37500m,
            Renkler = ["Gri", "Kum Beji"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Kolay Montaj", "Kolay Temizlenir", "Renk Seçenekleri"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "65 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "65 cm", Derinlik = "16 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "lugano-100",
            Ad = "Lugano 100",
            Kod = "LUGANO-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 44,
            Fiyat = 49000m,
            BoyDolabiFiyati = 28800m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "lugano-80",
            Ad = "Lugano 80",
            Kod = "LUGANO-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 45,
            Fiyat = 45000m,
            BoyDolabiFiyati = 28800m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "75 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "lugano-plus-100",
            Ad = "Lugano Plus 100",
            Kod = "LUGANO-PLUS-100",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 46,
            Fiyat = 49000m,
            BoyDolabiFiyati = 28800m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "100 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "100 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        },
        new()
        {
            Slug = "lugano-plus-80",
            Ad = "Lugano Plus 80",
            Kod = "LUGANO-PLUS-80",
            KoleksiyonGrubu = "Standart",
            SayfaNo = 47,
            Fiyat = 45000m,
            BoyDolabiFiyati = 28800m,
            Renkler = ["Gri", "Beyaz"],
            Ozellikler = ["Soft Kapak", "MDF Ahşap", "Dokunmatik Ledli Ayna", "Kolay Montaj", "Kolay Temizlenir"],
            Olculer =
            [
                new() { Baslik = "Dolap", Yukseklik = "85 cm", Genislik = "80 cm", Derinlik = "46 cm" },
                new() { Baslik = "Ayna", Yukseklik = "75 cm", Genislik = "80 cm", Derinlik = "5 cm" },
                new() { Baslik = "Boy Dolap", Yukseklik = "140 cm", Genislik = "38 cm", Derinlik = "33 cm" }
            ]
        }
    ];

    public static GoldBanyoKatalogUrunu? SlugIleGetir(string? slug) =>
        string.IsNullOrWhiteSpace(slug)
            ? null
            : Tum.FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public static int KoleksiyonSirasiGetir(string? koleksiyonGrubu)
    {
        if (string.IsNullOrWhiteSpace(koleksiyonGrubu))
        {
            return KoleksiyonSiralari["Diger"];
        }

        return KoleksiyonSiralari.TryGetValue(koleksiyonGrubu, out var sira)
            ? sira
            : KoleksiyonSiralari["Diger"];
    }

    public static string KoleksiyonAciklamasiGetir(string? koleksiyonGrubu)
    {
        if (string.IsNullOrWhiteSpace(koleksiyonGrubu))
        {
            return KoleksiyonAciklamalari["Diger"];
        }

        return KoleksiyonAciklamalari.TryGetValue(koleksiyonGrubu, out var aciklama)
            ? aciklama
            : KoleksiyonAciklamalari["Diger"];
    }
}
