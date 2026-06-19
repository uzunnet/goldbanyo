using Desadoor.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Desadoor.UI.Servisler;

namespace Desadoor.UI.Pages;

/// <summary>
/// Hakkimizda — DesaDoor hakkımızda sayfasının kod-arkası sınıfıdır.
/// Şirket hikayesi, misyon, vizyon, istatistikler ve ekip bilgileri API'den dinamik olarak yüklenir.
/// Tüm metinler SayfaYonetimi üzerinden admin panelinden güncellenebilir.
/// Dil değişiminde içeriği otomatik olarak yeniden yükler (DilServisi.DilDegisti event'i).
/// </summary>
public partial class Hakkimizda : ComponentBase, IAsyncDisposable
{
    private bool _yukleniyor = true;

    // ─── Sayfa Meta Verileri ─────────────────────────────────────────────────
    private string _sayfaBasligi = "Hakkımızda | DesaDoor";
    private string _heroBaslik = "Hakkımızda 1992'den Günümüze";
    private string _heroAciklama = "Sizler istediniz, bizler tasarladık ve hep daha iyisini ürettik.";
    private string _hikayeBasligi = "Hikayemiz";
    private string _hikayeMetni = "Bursa Çalı bölgesinde bulunan fabrika alanı içerisine kurduğumuz üretim tesisimizde akrilik, laminat, uv lak, akripet, pvc, membran, kapak-laminat, pvc membran, lake kapı ve ev/ofis mobilyaları üretimi yapılmaktadır.\n\nTürkiye iç pazarında büyük bir paya sahip olan DESADOOR & NORDEN MOBİLYA birden fazla ülkeye ihracat yaparak ülke ekonomisine katkıda bulunmaktadır. Profesyonel ve deneyimli kadromuz ile sizlere en iyiyi sunmayı hedefliyoruz.\n\nİnşaat sektörü günümüzde hızla gelişip kendini yenilerken, paralelinde sektörümüzde bu yenilikleri kullanmak, geliştirmek ve uygulamak zorundadır. DESADOOR & NORDEN MOBİLYA olarak yenilikleri ülkemizle birlikte sektörümüze kazandırmakla yetinmeyip takipçisi olmuştur.";
    private string _misyon = "Müşterilerimize en kaliteli ürünleri sunmak, memnuniyeti en üst düzeyde tutmak.";
    private string _vizyon = "Sektörde yenilikçi ve öncü olmak, global pazarda Türk mobilyasını temsil etmek.";
    
    // ─── Medya & Katalog ─────────────────────────────────────────────────────
    private string _videoUrl = string.Empty;
    private string _youtubeUrl = string.Empty;
    private string _pdfKatalogUrl = string.Empty;

    // ─── İstatistikler ───────────────────────────────────────────────────────
    private string _yilTecrube = "30+";
    private string _tamamlananProje = "5000+";
    private string _bayiSayisi = "120+";
    private string _personelSayisi = "250+";

    // ─── Ekip ────────────────────────────────────────────────────────────────
    private List<EkipUyesi> _ekipUyeleri = [];

    // ─── Timeline ────────────────────────────────────────────────────────────
    private List<(string Yil, string Baslik, string Aciklama)> _tarihsel =
    [
        ("1995", "Kuruluş", "Bursa Nilüfer'de küçük bir atölye olarak üretim başladı."),
        ("2003", "Fabrikaya Geçiş", "Modern makine parkuruyla büyük ölçekli üretime geçildi."),
        ("2010", "Ürün Gamı Genişlemesi", "Membran, lake ve laminat kapak serileri eklendi."),
        ("2018", "Dijital Dönüşüm", "Online katalog ve 3D görüntüleme altyapısı kuruldu."),
        ("2024", "DesaDoor 2.0", "Yeni nesil tasarım stüdyosu ve showroom açıldı."),
    ];

    protected override async Task OnInitializedAsync()
    {
        await IcerikleriYukleAsync();
        _ekipUyeleri = await api.GetAsync<List<EkipUyesi>>("api/desadoor/ekip") ?? [];
        _yukleniyor = false;

        dil.DilDegisti += DilDegistinde;
    }

    private async void DilDegistinde()
    {
        await IcerikleriYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// API'den tüm sayfa içeriklerini çeker.
    /// Her alan için API'de karşılık gelen anahtar bulunamazsa varsayılan değer kullanılır.
    /// </summary>
    private async Task IcerikleriYukleAsync()
    {
        var sozluk = await api.GetAsync<Dictionary<string, string>>($"api/desadoor/sayfa-icerigi/hakkimizda?dil={dil.AktifDil}");
        if (sozluk != null)
        {
            _sayfaBasligi  = sozluk.GetValueOrDefault("SayfaBasligi", _sayfaBasligi);
            _heroBaslik    = sozluk.GetValueOrDefault("HeroBaslik", _heroBaslik);
            _heroAciklama  = sozluk.GetValueOrDefault("HeroAciklama", _heroAciklama);
            _hikayeBasligi = sozluk.GetValueOrDefault("HikayeBasligi", _hikayeBasligi);
            _hikayeMetni   = sozluk.GetValueOrDefault("HikayeMetni", _hikayeMetni);
            _misyon        = sozluk.GetValueOrDefault("Misyon", _misyon);
            _vizyon        = sozluk.GetValueOrDefault("Vizyon", _vizyon);
            _yilTecrube    = sozluk.GetValueOrDefault("YilTecrube", _yilTecrube);
            _tamamlananProje = sozluk.GetValueOrDefault("TamamlananProje", _tamamlananProje);
            _bayiSayisi    = sozluk.GetValueOrDefault("BayiSayisi", _bayiSayisi);
            _personelSayisi = sozluk.GetValueOrDefault("PersonelSayisi", _personelSayisi);

            _videoUrl = sozluk.GetValueOrDefault("VideoUrl", "");
            _youtubeUrl = sozluk.GetValueOrDefault("YoutubeUrl", "");
            _pdfKatalogUrl = sozluk.GetValueOrDefault("PdfKatalogUrl", "");
        }
    }

    private string EkipResimUrl(string? resim)
    {
        if (string.IsNullOrWhiteSpace(resim)) return "/medya/desadoor_default.png";
        if (resim.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return resim;
        return $"{api.ApiBaseUrl}{(resim.StartsWith('/') ? resim : "/" + resim)}";
    }

    public async ValueTask DisposeAsync()
    {
        dil.DilDegisti -= DilDegistinde;
        await ValueTask.CompletedTask;
    }
}
