using Microsoft.AspNetCore.Components;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.UI.Pages;

public partial class Hakkimizda : ComponentBase, IAsyncDisposable
{
    private bool _yukleniyor = true;

    private string _sayfaBasligi = "Hakkımızda | Gold Banyo";
    private string _kurumsalEtiket = "Gold Banyo Hikayesi";
    private string _heroBaslik = "Lüks banyolara altın dokunuşlar";
    private string _heroAciklama = "2005'ten bu yana üretim disiplinimizi estetik, dayanıklılık ve tasarım detaylarıyla birleştiriyoruz.";
    private string _hikayeBasligi = "Bursa'dan başlayan tasarım ve üretim yolculuğumuz";
    private string _hikayeMetni =
        "Gold Banyo, Bursa Nilüfer'deki üretim altyapısını güçlü malzeme bilgisi ve çağdaş tasarım yaklaşımıyla bir araya getirerek banyo mobilyasında seçkin çözümler geliştirmek için kuruldu.\n\n" +
        "Premium, Trend, Exclusive ve özel proje koleksiyonlarımızda akrilik, lake, membran, UV lak ve farklı yüzey alternatiflerini bir araya getiriyor; showroom, bayi ve proje kanallarına sürdürülebilir kalite standardı sunuyoruz.\n\n" +
        "Bugün üretim gücümüzü yalnızca ürün çıkarmak için değil, mimarların, uygulamacıların ve son kullanıcıların ihtiyacına göre ölçülü, estetik ve uzun ömürlü yaşam alanları kurmak için kullanıyoruz.";
    private string _misyon = "Banyo mobilyasında tasarım kalitesini üretim disipliniyle birleştirerek müşterilerimize uzun ömürlü, şık ve güvenilir çözümler sunmak.";
    private string _vizyon = "Türkiye'de ve ihracat pazarlarında Gold Banyo imzasını; özgün tasarım, güçlü servis ve sürdürülebilir kalite ile anılan bir referans marka haline getirmek.";

    private string _videoUrl = string.Empty;
    private string _youtubeUrl = string.Empty;
    private string _pdfKatalogUrl = string.Empty;
    private string _katalogButonYazi = "Kurumsal kataloğu incele";
    private string _hakkimizdaGorselUrl = "/medya/goldbanyo/hakkimizda/fabrika.jpg";

    private string _yilTecrube = "20+";
    private string _tamamlananProje = "5.000+";
    private string _bayiSayisi = "120+";
    private string _personelSayisi = "80+";

    private List<EkipUyesi> _ekipUyeleri = [];

    private readonly List<(string Yil, string Baslik, string Aciklama)> _varsayilanTarihce =
    [
        ("2005", "Marka kuruluşu", "Gold Banyo, banyo mobilyasında butik kalite anlayışıyla üretim yolculuğuna başladı."),
        ("2012", "Koleksiyon derinliği", "Farklı yüzey, renk ve seri kurgularıyla proje ve showroom kanalı için ürün gamı genişletildi."),
        ("2018", "İhracat ve bayi ağı", "Yurt içi satış kanallarına ek olarak ihracat ve kurumsal bayi yapılanması güçlendirildi."),
        ("2024", "Dijital dönüşüm", "Katalog, medya ve yönetim akışları daha güçlü bir dijital altyapıyla yeniden yapılandırıldı.")
    ];

    private List<(string Yil, string Baslik, string Aciklama)> _tarihsel = [];

    private IReadOnlyList<string> HikayeParagraflari =>
        _hikayeMetni
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

    protected override async Task OnInitializedAsync()
    {
        _tarihsel = [.. _varsayilanTarihce];
        await IcerikleriYukleAsync();
        _ekipUyeleri = await api.GetAsync<List<EkipUyesi>>("api/ekip") ?? [];
        _yukleniyor = false;

        dil.DilDegisti += DilDegistinde;
    }

    private async void DilDegistinde()
    {
        await IcerikleriYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task IcerikleriYukleAsync()
    {
        var sozluk = await api.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/hakkimizda?dil={dil.AktifDil}");
        if (sozluk is null)
        {
            _tarihsel = [.. _varsayilanTarihce];
            return;
        }

        _sayfaBasligi = sozluk.GetValueOrDefault("SayfaBasligi", _sayfaBasligi);
        _kurumsalEtiket = sozluk.GetValueOrDefault("KurumsalEtiket", _kurumsalEtiket);
        _heroBaslik = sozluk.GetValueOrDefault("HeroBaslik", _heroBaslik);
        _heroAciklama = sozluk.GetValueOrDefault("HeroAciklama", _heroAciklama);
        _hikayeBasligi = sozluk.GetValueOrDefault("HikayeBasligi", _hikayeBasligi);
        _hikayeMetni = sozluk.GetValueOrDefault("HikayeMetni", _hikayeMetni);
        _misyon = sozluk.GetValueOrDefault("Misyon", _misyon);
        _vizyon = sozluk.GetValueOrDefault("Vizyon", _vizyon);
        _yilTecrube = sozluk.GetValueOrDefault("YilTecrube", _yilTecrube);
        _tamamlananProje = sozluk.GetValueOrDefault("TamamlananProje", _tamamlananProje);
        _bayiSayisi = sozluk.GetValueOrDefault("BayiSayisi", _bayiSayisi);
        _personelSayisi = sozluk.GetValueOrDefault("PersonelSayisi", _personelSayisi);

        _videoUrl = sozluk.GetValueOrDefault("VideoUrl", string.Empty);
        _youtubeUrl = sozluk.GetValueOrDefault("YoutubeUrl", string.Empty);
        _pdfKatalogUrl = sozluk.GetValueOrDefault("PdfKatalogUrl", string.Empty);
        _katalogButonYazi = sozluk.GetValueOrDefault("KatalogButonYazi", _katalogButonYazi);
        _hakkimizdaGorselUrl = sozluk.GetValueOrDefault("HakkimizdaGorselUrl", _hakkimizdaGorselUrl);
        _tarihsel = TarihceOlustur(sozluk);
    }

    private List<(string Yil, string Baslik, string Aciklama)> TarihceOlustur(Dictionary<string, string> sozluk)
    {
        var liste = new List<(string Yil, string Baslik, string Aciklama)>();

        for (var indeks = 0; indeks < _varsayilanTarihce.Count; indeks++)
        {
            var sira = indeks + 1;
            var varsayilan = _varsayilanTarihce[indeks];
            var yil = sozluk.GetValueOrDefault($"TarihceYil{sira}", varsayilan.Yil);
            var baslik = sozluk.GetValueOrDefault($"TarihceBaslik{sira}", varsayilan.Baslik);
            var aciklama = sozluk.GetValueOrDefault($"TarihceAciklama{sira}", varsayilan.Aciklama);
            liste.Add((yil, baslik, aciklama));
        }

        return liste;
    }

    private string HakkimizdaGorselTamUrl()
    {
        if (string.IsNullOrWhiteSpace(_hakkimizdaGorselUrl))
        {
            return "/medya/vizitlink3d_default.png";
        }

        if (_hakkimizdaGorselUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return _hakkimizdaGorselUrl;
        }

        return $"{api.ApiBaseUrl}{(_hakkimizdaGorselUrl.StartsWith('/') ? _hakkimizdaGorselUrl : "/" + _hakkimizdaGorselUrl)}";
    }

    private string KatalogPdfGostericiUrl()
    {
        var dosya = _pdfKatalogUrl.StartsWith(api.ApiBaseUrl, StringComparison.OrdinalIgnoreCase)
            ? _pdfKatalogUrl[api.ApiBaseUrl.Length..].TrimStart('/')
            : _pdfKatalogUrl.TrimStart('/');

        return $"/pdf-gosterici?dosya={Uri.EscapeDataString(dosya)}&baslik={Uri.EscapeDataString(_heroBaslik)}&donus={Uri.EscapeDataString("/hakkimizda")}";
    }

    private string EkipResimUrl(string? resim)
    {
        if (string.IsNullOrWhiteSpace(resim))
        {
            return "/medya/vizitlink3d_default.png";
        }

        if (resim.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return resim;
        }

        return $"{api.ApiBaseUrl}{(resim.StartsWith('/') ? resim : "/" + resim)}";
    }

    public async ValueTask DisposeAsync()
    {
        dil.DilDegisti -= DilDegistinde;
        await ValueTask.CompletedTask;
    }
}
