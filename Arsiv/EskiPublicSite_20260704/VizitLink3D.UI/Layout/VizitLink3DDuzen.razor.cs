using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Models;

namespace VizitLink3D.UI.Layout;

/// <summary>
/// VizitLink3D kurumsal web sitesinin ana layout mantık dosyasıdır.
/// Navbar, footer, mobil menü, dil değiştirici ve sayfa geçiş animasyonlarını yönetir.
/// Menü öğeleri API'den dinamik olarak çekilir — admin panelden eklenip çıkarılabilir.
/// Servisler .razor dosyasındaki @inject direktiflerinden alınır (Blazor standardı).
/// </summary>
public partial class VizitLink3DDuzen : IAsyncDisposable
{
    // Global _Imports.razor'daki @inject ile enjekte edilen servisler burada
    // tekrar tanımlanmaz. Blazor'da partial class için _Imports.razor üzerinden
    // gelen inject'ler otomatik olarak kod dosyasına aktarılır.

    // ─── DURUM ALANLARI ───────────────────────────────────────────────────

    /// <summary>Koyu tema durumu</summary>
    private bool _isDarkMode = false;

    /// <summary>Mobil hamburger menünün açık/kapalı durumunu tutar.</summary>
    private bool _mobilMenuAcik = false;

    /// <summary>Masaustu dil panelinin acik/kapali durumunu tutar.</summary>
    private bool _dilMenusuAcik = false;

    /// <summary>Sayfa yükleme animasyonunun tamamlanıp tamamlanmadığını belirtir.</summary>
    private bool _sayfaHazir = false;

    /// <summary>Sayfa üstüne git butonunun görünürlüğünü kontrol eder.</summary>
    private bool _usaSayfaGoster = false;

    /// <summary>Menü yükleniyor mu? True iken skeleton gösterilir.</summary>
    private bool _menuYukleniyor = true;

    /// <summary>
    /// Aktif kullanıcı dili. "tr" veya "en" olabilir.
    /// localStorage'dan başlatılır, değiştirildiğinde kaydedilir.
    /// </summary>
    private string _aktifDil = "tr";

    /// <summary>
    /// Firmanın logosunun URL'si. Boş ise metin tabanlı logo gösterilir.
    /// </summary>
    private string _logoUrl = "/img/goldbanyo-logo.svg";

    /// <summary>
    /// Tarayıcı sekmesi ikonu (favicon). Admin panelden değiştirilebilir;
    /// boş ise index.html'deki varsayılan ikon kullanılır.
    /// </summary>
    private string _faviconUrl = "";
    private string LogoTamYolu => MarkaVarligiNormalizeEt(_logoUrl, "/img/goldbanyo-logo.svg");
    private string FaviconTamYolu => MarkaVarligiNormalizeEt(_faviconUrl, "/favicon.png");

    private string _footerAciklama = "Türkiye'nin lider banyo mobilyası üreticisi. 35+ ülkede hizmet veren, 600+ satış noktasına sahip kurumsal marka.";
    private string _facebookUrl = "https://facebook.com/gold.banyo";
    private string _instagramUrl = "https://www.instagram.com/gold.banyom/";
    private string _youtubeUrl = "";
    private string _pinterestUrl = "";
    private string _adres = "Çankırı Yolu 8. km Büğdüz Mah. 24. Sok. No: 4 Akyurt / Ankara";
    private string _telefon1 = "+90 312 847 55 22";
    private string _telefon2 = "";
    private string _mesaiSaatleri = "Pzt-Cmt 09:00 - 18:00";
    private string _aktifSiteTema = "gold";

    /// <summary>
    /// Dinamik menü öğeleri. Admin panelden yapılandırılabilir.
    /// Her öğe bir başlık ve bir URL içerir.
    /// </summary>
    private List<MenuOgesi> _menuOgeleri = new();

    /// <summary>Footer hızlı bağlantıları (API'den dinamik)</summary>
    private List<MenuOgesi> _footerLinkleri = new();

    /// <summary>Footer kategori bağlantıları (API'den dinamik)</summary>
    private List<MenuOgesi> _footerKategorileri = new();

    private sealed class FirmaTemaDto
    {
        public string? SiteTema { get; set; }
    }

    // ─── MUDBLAZOR TEMA TANIMLARI ─────────────────────────────────────────

    /// <summary>
    /// Atelier Monochrome temasını MudBlazor ThemeProvider için tanımlar.
    /// Siyah/beyaz/gri tonları, Noto Serif başlıklar, Manrope gövde metni.
    /// </summary>
    private MudTheme _tema = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#000000",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#1A1C1C",
            Background = "#F9F9F9",
            Surface = "#FFFFFF",
            AppbarBackground = "rgba(255,255,255,0.90)",
            AppbarText = "#1A1C1C",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#1A1C1C",
            TextPrimary = "#1A1C1C",
            TextSecondary = "#4C4546",
            Divider = "#EEEEEE",
            ActionDefault = "#1A1C1C",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Manrope", "Inter", "sans-serif" }
            },
            H1 = new H1Typography
            {
                FontFamily = new[] { "Noto Serif", "Georgia", "serif" },
                FontSize = "4rem",
                FontWeight = "400",
                LetterSpacing = "-0.02em"
            },
            H2 = new H2Typography
            {
                FontFamily = new[] { "Noto Serif", "Georgia", "serif" },
                FontSize = "2.5rem",
                FontWeight = "400"
            },
            H3 = new H3Typography
            {
                FontFamily = new[] { "Noto Serif", "Georgia", "serif" },
                FontSize = "2rem",
                FontWeight = "400"
            },
            H5 = new H5Typography
            {
                FontFamily = new[] { "Noto Serif", "Georgia", "serif" },
                FontSize = "1.5rem",
                FontWeight = "400"
            }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "0px",   // Sıfır köşe — Atelier Monochrome kuralı
            AppbarHeight = "72px"
        }
    };

    // ─── YAŞAM DÖNGÜSÜ ────────────────────────────────────────────────────

    /// <summary>
    /// Her render sonrası çalışır. İlk render'da data-tema-id attribute'ünü set eder.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await js.InvokeVoidAsync("localStorage.setItem", "vizitlink3d_site_tema", _aktifSiteTema);
            await js.InvokeVoidAsync("vizitlink3dTema.siteUygula", _aktifSiteTema);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// Bileşen başladığında çalışır. Dil tercihini localStorage'dan okur,
    /// FIRMA BİLGİSİNİ (logo, tema, renk) API'den çeker,
    /// menü öğelerini API'den çeker, sayfa geçiş animasyonunu tetikler.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        _sayfaHazir = false;

        try
        {
            var kayitliDil = await js.InvokeAsync<string>("localStorage.getItem", "vizitlink3dil");
            if (!string.IsNullOrEmpty(kayitliDil))
                _aktifDil = kayitliDil;
        }
        catch { }

        // SaaS: Firma bilgisini API'den çek (middleware setlediği firma ID'den)
        try
        {
            var firma = await firmaBilgisi.GetFirmaAsync();
            if (firma != null)
            {
                // Firma logosu
                if (!string.IsNullOrEmpty(firma.Logo))
                    _logoUrl = firma.Logo;

                // Firma site teması
                if (!string.IsNullOrEmpty(firma.SiteTema))
                    _aktifSiteTema = firma.SiteTema;
            }
        }
        catch { /* Firma bilgisi yüklenemezse varsayılan kullan */ }

        try
        {
            var firmaTema = await api.GetAsync<FirmaTemaDto>("api/firma-tema");
            if (!string.IsNullOrWhiteSpace(firmaTema?.SiteTema))
                _aktifSiteTema = firmaTema.SiteTema!;
        }
        catch { }

        await dil.BaslatAsync();

        // Dil degisince layout + tum sayfa govdesi yeniden render edilsin
        // (sayfa yenilemeden TR/EN gecisi icin zorunlu).
        dil.DilDegisti += DilDegistiginde;

        await Task.WhenAll(
            MenuOgeleriniYukle(),
            FooterMenuleriniYukle(),
            AyarlariYukle()
        );

        // AI destekli otomatik ceviri mekanizmasini tetikle (eksik anahtarlar icin)
        _ = dil.EksikCevirileriAIileTamamlaAsync();

        // Sayfa hazırlandı — fade-in animasyonunu başlat
        await Task.Delay(50); // DOM'un ilk render'ını bekle
        _sayfaHazir = true;

        // Scroll dinleyicisini kur (yukarı git butonu için)
        nav.LocationChanged += KonumDegistinde;
        _ = ZiyaretKaydetAsync(nav.Uri);
    }

    private async Task AyarlariYukle()
    {
        try
        {
            var sozluk = await api.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/ayarlar?dil={dil.AktifDil}");
            if (sozluk != null)
            {
                if (sozluk.TryGetValue("LogoUrl", out var logo) && !string.IsNullOrEmpty(logo))
                {
                    _logoUrl = MarkaVarligiNormalizeEt(logo, "/img/goldbanyo-logo.svg");
                }
                if (sozluk.TryGetValue("FaviconUrl", out var favicon) && !string.IsNullOrEmpty(favicon))
                {
                    _faviconUrl = MarkaVarligiNormalizeEt(favicon, "/favicon.png");
                }
                if (sozluk.TryGetValue("FooterAciklama", out var fa)) _footerAciklama = fa;
                if (sozluk.TryGetValue("FacebookUrl", out var fb)) _facebookUrl = fb;
                if (sozluk.TryGetValue("InstagramUrl", out var ins)) _instagramUrl = ins;
                if (sozluk.TryGetValue("YoutubeUrl", out var yt)) _youtubeUrl = yt;
                if (sozluk.TryGetValue("PinterestUrl", out var pin)) _pinterestUrl = pin;
                if (sozluk.TryGetValue("Adres", out var adr)) _adres = adr;
                if (sozluk.TryGetValue("Telefon1", out var t1)) _telefon1 = t1;
                if (sozluk.TryGetValue("Telefon2", out var t2)) _telefon2 = t2;
                if (sozluk.TryGetValue("MesaiSaatleri", out var ms)) _mesaiSaatleri = ms;
            }
        }
        catch { /* ayarlar yüklenemezse varsayılan değerler kullanılır */ }
    }

    /// <summary>
    /// Sayfa URL'i değiştiğinde çalışır. Mobil menüyü kapatır ve
    /// yeni sayfanın animasyonla açılmasını sağlar.
    /// </summary>
    private async void KonumDegistinde(object? sender, LocationChangedEventArgs e)
    {
        _mobilMenuAcik = false;
        _dilMenusuAcik = false;
        _sayfaHazir = false;
        StateHasChanged();

        await Task.Delay(150);
        _sayfaHazir = true;
        await ZiyaretKaydetAsync(e.Location);
        StateHasChanged();
    }

    private async Task ZiyaretKaydetAsync(string adres)
    {
        var yol = nav.ToBaseRelativePath(adres);
        if (string.IsNullOrWhiteSpace(yol))
            yol = "/";
        else if (!yol.StartsWith('/'))
            yol = "/" + yol;

        await api.PostAsync<object>("api/dashboard/ziyaret-kaydet", new ZiyaretKaydetDto(yol, null));
    }

    private static string MarkaVarligiNormalizeEt(string? deger, string varsayilanDeger)
    {
        if (string.IsNullOrWhiteSpace(deger))
        {
            return varsayilanDeger;
        }

        var normalizeDeger = deger.Contains("vizitlink3d", StringComparison.OrdinalIgnoreCase)
            ? varsayilanDeger
            : deger;

        if (normalizeDeger.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || normalizeDeger.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || normalizeDeger.StartsWith("/", StringComparison.Ordinal))
        {
            return normalizeDeger;
        }

        return "/" + normalizeDeger.TrimStart('~').TrimStart('/');
    }

    private record ZiyaretKaydetDto(string Sayfa, string? Referer);

    // ─── MENÜ YÜKLEMESİ ──────────────────────────────────────────────────

    /// <summary>
    /// Admin panelden yapılandırılan dinamik menü öğelerini API'den çeker.
    /// API yanıt vermezse varsayılan statik menü kullanılır — sistemin kilitlenmemesi için.
    /// </summary>
    private async Task MenuOgeleriniYukle()
    {
        _menuYukleniyor = true;
        try
        {
            // api.GetAsync<T> doğrudan T? döndürür (Cevap<T> değil)
            var liste = await api.GetAsync<List<MenuOgesi>>("api/menu/ana");
            if (liste?.Count > 0)
            {
                // URL'leri temizle (başındaki / işaretini kaldır)
                foreach (var menu in liste)
                {
                    if (menu.Url != null && menu.Url.StartsWith("/"))
                    {
                        menu.Url = menu.Url.TrimStart('/');
                    }

                    if (menu.AltMenuler != null)
                    {
                        foreach (var alt in menu.AltMenuler)
                        {
                            if (alt.Url != null && alt.Url.StartsWith("/"))
                            {
                                alt.Url = alt.Url.TrimStart('/');
                            }
                        }
                    }
                }
                _menuOgeleri = liste;
            }
            else
                _menuOgeleri = VarsayilanMenuOlustur();
        }
        catch
        {
            // API erişilemez — statik fallback menü
            _menuOgeleri = VarsayilanMenuOlustur();
        }
        finally
        {
            _menuYukleniyor = false;
        }
    }

    private async Task FooterMenuleriniYukle()
    {
        try
        {
            var hizli = await api.GetAsync<List<MenuOgesi>>("api/menu/footer");
            if (hizli?.Count > 0) _footerLinkleri = hizli;

            var kategoriler = await api.GetAsync<List<MenuOgesi>>("api/menu/footer-kategori");
            if (kategoriler?.Count > 0) _footerKategorileri = kategoriler;
        }
        catch { /* API erisilemezse footer bos kalir */ }
    }

    /// <summary>
    /// API erişilemez veya boş döndüğünde kullanılan yedek statik menü öğeleri.
    /// Bu liste VizitLink3D mevcut site yapısını yansıtır.
    /// </summary>
    private List<MenuOgesi> VarsayilanMenuOlustur() => new()
    {
        new MenuOgesi { Baslik = dil.T("ana_sayfa", "Ana Sayfa"), Url = "" },
        new MenuOgesi { Baslik = dil.T("menu.urunler", "Banyo Dolapları"), Url = "banyo-dolaplari", AltMenuler = new()
        {
            new MenuOgesi { Baslik = "Exclusive", Url = "banyo-dolaplari#exclusive" },
            new MenuOgesi { Baslik = "Premium", Url = "banyo-dolaplari#premium" },
            new MenuOgesi { Baslik = "Trend", Url = "banyo-dolaplari#trend" },
            new MenuOgesi { Baslik = "Standart", Url = "banyo-dolaplari#standart" },
            new MenuOgesi { Baslik = "Diago 100", Url = "banyo-dolabi/diago-100" },
            new MenuOgesi { Baslik = "Diago 360", Url = "banyo-dolabi/diago-360" },
            new MenuOgesi { Baslik = "Tüm Modeller", Url = "banyo-dolaplari" }
        } },
        new MenuOgesi { Baslik = dil.T("kurumsal", "Kurumsal"), Url = "kurumsal", AltMenuler = new() {
            new MenuOgesi { Baslik = dil.T("hakkimizda", "Hakkımızda"), Url = "hakkimizda" },
            new MenuOgesi { Baslik = dil.T("vizyon_misyon", "Vizyon & Misyon"), Url = "vizyon-misyon" },
            new MenuOgesi { Baslik = "Fabrikamız", Url = "fabrikamiz" }
        } },
        new MenuOgesi { Baslik = dil.T("menu.katalog", "Katalog"), Url = "katalog" },
        new MenuOgesi { Baslik = dil.T("projeler", "Projelerimiz"), Url = "projeler" },
        new MenuOgesi { Baslik = dil.T("iletisim", "İletişim"), Url = "iletisim" },
    };

    // ─── EYLEMLER ────────────────────────────────────────────────────────

    private void TemaDegistir()
    {
        _isDarkMode = !_isDarkMode;
    }

    /// <summary>Mobil hamburger menüsünü açar veya kapatır.</summary>
    private void MobilMenuToggle() => _mobilMenuAcik = !_mobilMenuAcik;

    /// <summary>Masaustu dil listesini acar veya kapatir.</summary>
    private void DilMenusuToggle() => _dilMenusuAcik = !_dilMenusuAcik;

    /// <summary>
    /// Kullanıcının dil tercihini değiştirir. Seçim localStorage'a kaydedilir
    /// ve DilServisi aracılığıyla tüm bileşenlere yansıtılır.
    /// </summary>
    private async Task DilDegistir(string yeniDil)
    {
        _aktifDil = yeniDil;
        _dilMenusuAcik = false;
        await js.InvokeVoidAsync("localStorage.setItem", "vizitlink3dil", yeniDil);
        await dil.DilDegistirAsync(yeniDil);
    }

    /// <summary>
    /// DilServisi.DilDegisti olayina baglanir. Layout ve altindaki @Body
    /// sayfa govdesini yeniden render ederek ceviri metinlerini gunceller.
    /// </summary>
    private async void DilDegistiginde()
    {
        _aktifDil = dil.AktifDil;
        await Task.WhenAll(
            MenuOgeleriniYukle(),
            FooterMenuleriniYukle(),
            AyarlariYukle()
        );
        _ = dil.EksikCevirileriAIileTamamlaAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>Tarayıcı sayfasını en üste kaydırır. Yukarı git butonu için kullanılır.</summary>
    private async Task SayfaBasinaGit()
    {
        await js.InvokeVoidAsync("window.scrollTo", 0, 0);
    }

    /// <summary>
    /// Bileşen kaldırıldığında LocationChanged olay dinleyicisini temizler.
    /// Bellek sızıntısını önlemek için zorunludur.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        nav.LocationChanged -= KonumDegistinde;
        dil.DilDegisti -= DilDegistiginde;
        return ValueTask.CompletedTask;
    }
}

