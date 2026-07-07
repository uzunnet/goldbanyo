using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Ortak.Modeller.Malzemeler;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Renkler;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.Ortak.Modeller.Tema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Servisler.Kimlik;

namespace VizitLink3D.Api.VeriTabani;

public static class TohumVerisi
{
    /// <summary>
    /// Bir tohum bolumunu izole calistirir. Bolum hata verirse loglar,
    /// ChangeTracker'i temizler ve sonraki bolumlere devam eder â€” boylece
    /// tek bir FK/constraint hatasi tum tohum verisini geri almaz.
    /// (Onceki davranis: Projeler FK hatasi tum batch'i abort edip siteyi
    /// bos birakiyordu.)
    /// </summary>
    private static async Task Bolum(VizitLink3DDbContext vt, string ad, Func<Task> islem)
    {
        try
        {
            await islem();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[TOHUM HATASI] '{ad}' atlandi: {ex.Message}");
            vt.ChangeTracker.Clear();
        }
    }

    public static async Task TemizleSlaytResimlerAsync(VizitLink3DDbContext vt, string webRootPath)
    {
        var slaytlar = await vt.Slaytlar
            .Where(s => s.ArkaplanResim != null && s.ArkaplanResim.StartsWith("/medya/"))
            .ToListAsync();

        var degisti = false;
        foreach (var s in slaytlar)
        {
            var dosyaYolu = Path.Combine(webRootPath,
                s.ArkaplanResim!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(dosyaYolu))
            {
                s.ArkaplanResim = "/medya/goldbanyo/uretim.jpg";
                degisti = true;
            }
        }
        if (degisti) await vt.SaveChangesAsync();
    }

    public static async Task TohumlaAsync(VizitLink3DDbContext vt)
    {
        var eskiVIZITLINK3DKapakTohumlariAktif = false;
        if (eskiVIZITLINK3DKapakTohumlariAktif)
        {
            await Bolum(vt, "KapiKategorileri", async () => { if (!vt.KapiKategorileri.Any()) await TohumlaKapiKategorileriAsync(vt); });
            await Bolum(vt, "KapakModelleri", async () => { if (!vt.KapakModelleri.Any()) await TohumlaKapakModelleriAsync(vt); });
            await Bolum(vt, "GercekKapakModelleri", async () => await TohumlaGercekKapakModelleriAsync(vt));
            await Bolum(vt, "DosyadanKapiModelleri", async () => await TohumlaDosyadanKapiModelleriAsync(vt));
        }

        await Bolum(vt, "Slaytlar", async () => await TohumlaSlaytlariAsync(vt));
        await Bolum(vt, "SlaytResimYoluDuzelt", async () => await SlaytResimYollariniDuzeltAsync(vt));
        await Bolum(vt, "HizmetAdimlari", async () => await TohumlaHizmetAdimlariniAsync(vt));
        await Bolum(vt, "SSS", async () => { if (!vt.SikSorulanSorular.Any()) await TohumlaSSSAsync(vt); });
        await Bolum(vt, "Diller", async () => { await TohumlaDilleriAsync(vt); });
        await Bolum(vt, "Ceviriler", async () => await TohumlaCevirileriAsync(vt));
        await Bolum(vt, "SayfaIcerikleri", async () => await TohumlaSayfaIcerikleriAsync(vt));
        await Bolum(vt, "AnaMenuler", async () =>
        {
            await TohumlaHeaderMenuleriAsync(vt);
            await TohumlaFooterMenuleriAsync(vt);
        });
        await Bolum(vt, "Ayarlar", async () => { if (!vt.SayfaIcerikleri.Any(s => s.Bolum == "ayarlar")) await TohumlaAyarlariAsync(vt); });
        await Bolum(vt, "Firma", async () => { if (!vt.Firmalar.Any()) await TohumlaFirmaAsync(vt); });
        await Bolum(vt, "Lisans", async () => { if (!vt.Lisanslar.Any()) await TohumlaLisansAsync(vt); });
        await Bolum(vt, "ProjeKategorileri", async () => { if (!vt.ProjeKategorileri.Any()) await TohumlaProjeKategorileriAsync(vt); });
        await Bolum(vt, "Referanslar", async () => { await TohumlaReferanslariAsync(vt); });
        await Bolum(vt, "ReferansLoglariGuncelle", async () => { await ReferansLogoGuncelleAsync(vt); });
        await Bolum(vt, "MusteriYorumlari", async () => { if (!vt.MusteriYorumlari.Any()) await TohumlaMusteriYorumlariniAsync(vt); });
        await Bolum(vt, "Projeler", async () => { if (!vt.Projeler.Any()) await TohumlaProjeleriAsync(vt); });
        await Bolum(vt, "MedyaKlasorleri", async () => { if (!vt.MedyaKlasorleri.Any()) await TohumlaMedyaKlasorleriAsync(vt); });
        await Bolum(vt, "IsTakip", async () => { if (!vt.IsTakipKayitlari.Any()) await TohumlaIsTakipAsync(vt); });

        // Admin kullanicisi (SuperAdmin degil) â€” yoksa ekle
        await Bolum(vt, "YoneticiKullanici", async () =>
        {
            if (!await vt.Kullanicilar.AnyAsync(k => k.KullaniciAdi == "yonetici"))
            {
                vt.Kullanicilar.Add(new Kullanici
                {
                    KullaniciAdi = "yonetici",
                    SifreHash = "$2b$12$K87L8R/orPfsSJZ.at8ze.4G6Lzv5KZ3m30ca3/ao2j4XvMidOyti", // Şifre: 123456
                    AdSoyad = "Firma Yöneticisi",
                    Eposta = "yonetici@3dvizitlink.com.tr",
                    Rol = Rol.Admin,
                    EmailDogrulandiMi = true,
                    AktifMi = true,
                    OlusturulmaTarihi = DateTime.UtcNow
                });
                await vt.SaveChangesAsync();
            }
        });

        // === URUN YONETIMI (3D Konfigurator) DEMO SEED ===
        await Bolum(vt, "UrunAilesileri", async () => { if (!vt.UrunAilesileri.Any()) await TohumlaUrunAilesileriniAsync(vt); });
        await Bolum(vt, "UrunKategorileri", async () => { if (!vt.UrunKategorileri.Any()) await TohumlaUrunKategorileriniAsync(vt); });
        await Bolum(vt, "RenkKataloglari", async () => { if (!vt.RenkKataloglari.Any()) await TohumlaRenkKataloglariniAsync(vt); });
        await Bolum(vt, "RalRenkleri", async () => { if (!vt.RalRenkleri.Any()) await TohumlaRalRenkleriniAsync(vt); });
        await Bolum(vt, "Malzemeler", async () => { if (!vt.Malzemeler.Any()) await TohumlaMalzemeleriAsync(vt); });
        await Bolum(vt, "KaplamaSecenekleri", async () => { if (!vt.KaplamaSecenekleri.Any()) await TohumlaKaplamaSecenekleriniAsync(vt); });
        await Bolum(vt, "ReferansUrunleri", async () => { if (!vt.Urunler.Any()) await TohumlaReferansUrunleriniAsync(vt); });

        // === EKSIK SEED: Bulten, Eposta, Teklif, Sube, AI, Katalog ===
        await Bolum(vt, "BultenAboneleri", async () => { if (!vt.BultenAboneleri.Any()) await TohumlaBultenAboneleriniAsync(vt); });
        await Bolum(vt, "EpostaSablonlari", async () => { if (!vt.EpostaSablonlari.Any()) await TohumlaEpostaSablonlariniAsync(vt); });
        await Bolum(vt, "TeklifIstekleri", async () => { if (!vt.TeklifIstekleri.Any()) await TohumlaTeklifIstekleriniAsync(vt); });
        await Bolum(vt, "Subeler", async () => { if (!vt.Subeler.Any()) await TohumlaSubeleriAsync(vt); });
        await Bolum(vt, "AISaglayicilari", async () => { if (!vt.AISaglayicilari.Any()) await TohumlaAISaglayicilariniAsync(vt); });
        await Bolum(vt, "Kataloglar", async () => await TohumlaKataloglariAsync(vt));
        await Bolum(vt, "GoldBanyoGecisDuzeltmeleri", async () => await GoldBanyoGecisDuzeltmeleriAsync(vt));
        await Bolum(vt, "MenuTohumlariniGarantiEt", async () => await MenuTohumlariniGarantiEtAsync(vt));

        await Bolum(vt, "AdminMenuleri", async () =>
        {
            // Her baslangicta admin menulerini guncelle — yeni sayfalar/menu ogeleri
            // eklendiginde otomatik yenilenir.
            var sistemVarMi = await vt.MenuOgeleri.AnyAsync(m => m.Konum == "AdminSol" && m.Baslik == "Sistem");
            var urunSihirbaziVarMi = await vt.MenuOgeleri.AnyAsync(m => m.Konum == "AdminSol" && m.Url == "admin/urun-sihirbazi");
            var kapakModelleriVarMi = await vt.MenuOgeleri.AnyAsync(m => m.Konum == "AdminSol" && m.Url == "admin/kapak-modeli-yonetimi");
            var pdfUygulamaVarMi = await vt.MenuOgeleri.AnyAsync(m => m.Konum == "AdminSol" && m.Url == "admin/pdf-uygulama-esleme");
            if (kapakModelleriVarMi || !sistemVarMi || !urunSihirbaziVarMi || !pdfUygulamaVarMi)
            {
                vt.MenuOgeleri.RemoveRange(vt.MenuOgeleri.Where(m => m.Konum == "AdminSol"));
                await vt.SaveChangesAsync();
                await TohumlaAdminMenuleriAsync(vt);
            }
        });

        await Bolum(vt, "EksikIcerikler", async () => await TohumlaEksikIceriklerAsync(vt));
        await Bolum(vt, "DemoIcerik", async () => await TohumlaDemoIcerikAsync(vt));
        
        await Bolum(vt, "KatalogMenuGuncelleme", async () =>
        {
            var katalogVarMi = await vt.MenuOgeleri.AnyAsync(m => m.Konum == "PublicHeader" && m.Url == "katalog");
            if (!katalogVarMi)
            {
                var digerleri = await vt.MenuOgeleri.Where(m => m.Konum == "PublicHeader" && m.Sira >= 3).ToListAsync();
                foreach (var d in digerleri) d.Sira++;
                vt.MenuOgeleri.Add(new MenuOgesi { Baslik = "Katalog", Url = "katalog", Sira = 3, Konum = "PublicHeader", Ikon = "PictureAsPdf", AktifMi = true });
                await vt.SaveChangesAsync();
            }
        });

        await Bolum(vt, "SonKayit", async () => await vt.SaveChangesAsync());

        // Duplicate PublicHeader temizligi
        await Bolum(vt, "MenuDuplicateTemizle", async () =>
        {
            var kokler = await vt.MenuOgeleri
                .Where(m => m.Konum == "PublicHeader" && m.UstMenuId == null && !m.SilindiMi)
                .OrderBy(m => m.Id)
                .ToListAsync();
            var gorulen = new HashSet<string>();
            var silinecek = new List<MenuOgesi>();
            foreach (var m in kokler)
            {
                var anahtar = $"{m.Baslik}|{m.Sira}";
                if (!gorulen.Add(anahtar))
                    silinecek.Add(m);
            }
            if (silinecek.Any())
            {
                vt.MenuOgeleri.RemoveRange(silinecek);
                await vt.SaveChangesAsync();
            }
        });

        // Acili 3D render NRD kapak modellerini soft-delete et (sadece duz-dik kapak fotograflari kalmali)
        await Bolum(vt, "AciliKapakModelleriniSil", async () => await AciliKapakModelleriniSilAsync(vt));

        // Urun serilerine mutfak/ortam gorselleri bagla (webp kitchen scene images)
        await Bolum(vt, "UrunOrtamGorselleri", async () => await UrunOrtamGorselleriniEkleAsync(vt));

        // Blog temizligi (Kullanici istegi uzerine "blog bunu sil fazla" — Haber menusu korunur)
        await Bolum(vt, "BlogTemizle", async () =>
        {
            var silinecekler = await vt.MenuOgeleri
                .Where(m => (m.Url == "blog" || m.Url == "admin/blog-yonetimi")
                    && m.Konum != "PublicHeader" && m.Konum != "PublicMobil" && m.Konum != "PublicFooterHizli")
                .ToListAsync();
            if (silinecekler.Any())
            {
                vt.MenuOgeleri.RemoveRange(silinecekler);
                await vt.SaveChangesAsync();
            }
        });

        await Bolum(vt, "HaberleriGuncelle2026", async () => await HaberleriGuncelle2026Async(vt));

        await Bolum(vt, "ProjeleriYenile2026", async () => await ProjeleriYenile2026Async(vt));

        await Bolum(vt, "FabrikaMenuVeSayfaEklendi2026", async () => await FabrikaMenuVeSayfaEkleAsync(vt));

        await Bolum(vt, "SayfaDuzenAyarlariVeMenuEklendi", async () =>
        {
            if (!await vt.SayfaDuzenAyarlari.AnyAsync(a => a.SayfaKodu == "kapak-sistemleri"))
                vt.SayfaDuzenAyarlari.Add(new SayfaDuzenAyari { SayfaKodu = "kapak-sistemleri", SayfaAdi = "Kapak Sistemleri", SutunAdet = 4, SatirAdet = 3, SayfaBasinaAdet = 12, SayfalamaAktif = true, AktifMi = true });

            if (!await vt.SayfaDuzenAyarlari.AnyAsync(a => a.SayfaKodu == "kapi-modelleri"))
                vt.SayfaDuzenAyarlari.Add(new SayfaDuzenAyari { SayfaKodu = "kapi-modelleri", SayfaAdi = "Kapı Modelleri", SutunAdet = 3, SatirAdet = 4, SayfaBasinaAdet = 12, SayfalamaAktif = true, AktifMi = true });

            if (!await vt.MenuOgeleri.AnyAsync(m => m.Url == "admin/sayfa-duzen-ayarlari" && m.Konum == "AdminSol"))
                vt.MenuOgeleri.Add(new MenuOgesi { Baslik = "Sayfa Duzen Ayarlari", Url = "admin/sayfa-duzen-ayarlari", Sira = 8, Konum = "AdminSol", Ikon = "GridView" });

            await vt.SaveChangesAsync();
        });

        // Tema sablonlari (sadece GOLD)
        await Bolum(vt, "TemaSablonlari", async () => { if (!vt.TemaSablonlari.Any()) await TohumlaTemaSablonlariAsync(vt); });

        // Urun gor sellerini KapakModelleri.AnaGorselUrl ile senkronize et (Bolum disinda — bagimsiz)
        await UrunGorselleriniSenkronizeEtAsync(vt);
    }

    private static async Task HaberleriGuncelle2026Async(VizitLink3DDbContext vt)
    {
        var eskiler = await vt.Haberler.Where(h => h.AktifMi).ToListAsync();
        foreach (var h in eskiler) h.AktifMi = false;
        if (eskiler.Any()) await vt.SaveChangesAsync();

        var simdi = DateTime.UtcNow;
        vt.Haberler.AddRange(
            new HaberYazisi
            {
                Baslik = "Bulgaristan'da 5.000 m² Showroom: Plovdiv'de Yakında Açılıyoruz",
                Slug = "plovdiv-showroom-acilis",
                Ozet = "Plovdiv'de 5.000 m² kapalı alana sahip dev showroomumuz yakında kapılarını açıyor. Balkan pazarına açılımımızda yeni ve büyük bir adım atıyoruz.",
                Icerik = "<p>VizitLink3D olarak uluslararası büyüme stratejimiz kapsamında <strong>Bulgaristan'ın Plovdiv şehrinde</strong> 5.000 m² alana yayılan entegre bir showroom merkezi kuruyoruz.</p><p>Bu merkez; kapı sistemlerimizi, mobilya kapak koleksiyonlarımızı ve özel üretim çözümlerimizi Balkan pazarında doğrudan tanıtmak amacıyla tasarlandı. Ziyaretçiler, canlı demo alanlarında ürünlerimizi yakından inceleme ve kişiye özel konfigürasyon danışmanlığı alma imkânı bulacak.</p><h3>Showroom Hakkında</h3><ul><li>5.000 m² kapalı sergi alanı</li><li>Tam ölçekli mutfak ve kapı demo bölümleri</li><li>Teknik danışmanlık ve 3D tasarım istasyonları</li><li>Bayi ve kurumsal müşterilere özel toplantı odaları</li></ul><p>Açılış tarihi ve davetli kayıtları için bültenimize abone olmayı unutmayın.</p>",
                AnaResimUrl = "/medya/goldbanyo/uretim.jpg",
                Etiketler = "showroom,bulgaristan,plovdiv,uluslararası,açılış",
                SeoBaslik = "Plovdiv Showroom Açılışı — VizitLink3D Bulgaristan",
                SeoAciklama = "VizitLink3D Plovdiv'de 5.000 m² showroom açıyor. Balkan pazarında yeni bir dönem başlıyor.",
                YayinTarihi = simdi.AddDays(-2),
                AktifMi = true
            },
            new HaberYazisi
            {
                Baslik = "Yeni Web Sitemiz Hizmetinizde",
                Slug = "yeni-web-sitemiz",
                Ozet = "Tamamen yenilenen web sitemizle ürünlerimizi, tamamlanmış projelerimizi ve online 3D konfigüratörümüzü çok daha kolay keşfedebilirsiniz.",
                Icerik = "<p>Kullanıcı deneyimini merkeze alan <strong>yeni VizitLink3D web sitesi</strong> bugün yayına girdi. Siteyi baştan sona yeniden tasarlayarak daha hızlı, daha modern ve daha işlevsel bir dijital deneyim sunmayı hedefledik.</p><h3>Yeni Özellikler</h3><ul><li><strong>3D Ürün Konfigüratörü:</strong> Kapak rengi, malzeme ve boyutunu gerçek zamanlı önizlemeyle seçin</li><li><strong>Gelişmiş Ürün Kataloğu:</strong> Kategori ve malzemeye göre filtrelenebilir geniş ürün yelpazesi</li><li><strong>Proje Galerisi:</strong> Tamamlanmış projelerimizden ilham alın</li><li><strong>Hızlı Teklif Formu:</strong> Birkaç adımda fiyat teklifi alın</li><li><strong>Mobil Uyumlu Tasarım:</strong> Telefon ve tabletlerde kusursuz deneyim</li></ul><p>Sitemizi gezerken karşılaştığınız öneri ve geri bildirimlerinizi iletişim formu üzerinden bizimle paylaşabilirsiniz.</p>",
                AnaResimUrl = "/medya/goldbanyo/showroom.jpg",
                Etiketler = "web,dijital,yenilik,konfigüratör",
                SeoBaslik = "Yeni Web Sitemiz Yayında — VizitLink3D",
                SeoAciklama = "VizitLink3D yeni web sitesi ile 3D konfigüratör, gelişmiş katalog ve daha fazlası.",
                YayinTarihi = simdi.AddDays(-7),
                AktifMi = true
            },
            new HaberYazisi
            {
                Baslik = "J Kulp Üretim Hattı Hizmete Girdi",
                Slug = "j-kulp-uretim-hatti",
                Ozet = "Fabrikamıza eklenen yeni J kulp üretim hattıyla birlikte artık tüm kulp ihtiyaçlarınızı yerli üretimle, kısa teslim sürelerinde karşılıyoruz.",
                Icerik = "<p>VizitLink3D fabrikasına entegre ettiğimiz <strong>yeni J kulp üretim hattı</strong>, mobilya kapak sistemlerindeki yerli üretim kapasitemizi önemli ölçüde artırdı.</p><p>Bu yatırımla birlikte J profil kulpları artık tamamen kendi bünyemizde üretiyoruz. Böylece hem teslimat sürelerini kısalttık hem de müşterilerimize daha fazla renk ve boyut esnekliği sunabiliyoruz.</p><h3>Yeni Üretim Hattının Avantajları</h3><ul><li>Yerli üretimle dışa bağımlılığın sona ermesi</li><li>7 iş günü içinde teslimat kapasitesi</li><li>50+ renk seçeneği ile tam özelleştirme</li><li>Mat, parlak ve fırçalanmış metal yüzey alternatifleri</li><li>Küçük adetli siparişlere uygun üretim esnekliği</li></ul><p>J kulp siparişleriniz ve teknik sorularınız için satış ekibimizle iletişime geçebilirsiniz.</p>",
                AnaResimUrl = "/medya/goldbanyo/uretim.jpg",
                Etiketler = "üretim,kulp,fabrika,yatırım,yerli",
                SeoBaslik = "J Kulp Üretim Hattı — VizitLink3D Fabrika",
                SeoAciklama = "VizitLink3D J kulp üretim hattını devreye aldı. Yerli üretim, kısa teslimat süresi ve 50+ renk seçeneği.",
                YayinTarihi = simdi.AddDays(-14),
                AktifMi = true
            },
            new HaberYazisi
            {
                Baslik = "Yeni Kapak ve Kapı Modellerimizi Beğeninize Sunduk",
                Slug = "yeni-kapak-kapi-modelleri-2026",
                Ozet = "2026 koleksiyonumuzdaki yeni kapak ve kapı modelleri; çağdaş estetik, üstün malzeme kalitesi ve yenilikçi yüzey işlemleriyle tasarlandı.",
                Icerik = "<p>Her yıl olduğu gibi bu yıl da ürün yelpazemizi güncel tasarım trendleri ve müşteri geri bildirimlerimiz doğrultusunda genişlettik. <strong>2026 yeni koleksiyonumuz</strong> şimdi sipariş ve incelemeye açık.</p><h3>Yeni Kapak Modelleri</h3><ul><li><strong>Lake Kapak Serisinde</strong> 3 yeni model: sıfır çerçeveli minimal tasarım</li><li><strong>Membran Kapak Serisinde</strong> doğal taş ve beton doku alternatifleri</li><li><strong>Akrilik Kapak Serisinde</strong> iki yönlü mat-parlak kombinasyon yüzeyler</li></ul><h3>Yeni Kapı Modelleri</h3><ul><li>Gizli menteşeli pivot kapı çözümleri</li><li>Akustik yalıtımlı ofis kapıları</li><li>Çift kanatlı özel genişlik seçenekleri</li></ul><p>Tüm yeni modelleri 3D konfigüratörümüzde keşfedebilir, dilediğiniz renk ve boyut kombinasyonunu anında önizleyebilirsiniz.</p>",
                AnaResimUrl = "/medya/gold-katalog/sayfa-006-hero.png",
                Etiketler = "yeni model,kapak,kapı,koleksiyon,2026,lake,membran,akrilik",
                SeoBaslik = "2026 Yeni Kapak ve Kapı Modelleri — VizitLink3D",
                SeoAciklama = "VizitLink3D 2026 koleksiyonu: lake, membran ve akrilik serilerde yeni kapak ve kapı modelleri.",
                YayinTarihi = simdi.AddDays(-21),
                AktifMi = true
            },
            new HaberYazisi
            {
                Baslik = "2026 Moda Renk Trendleri Artık VizitLink3D'da",
                Slug = "2026-moda-renk-trendleri",
                Ozet = "Pantone ve uluslararası tasarım otoritelerinin 2026 için öngördüğü toprak tonları, yosun yeşilleri ve sıcak griler artık ürün yelpazemizde.",
                Icerik = "<p>Tasarım dünyasının öncü kuruluşlarının 2026 renk tahminleri doğrultusunda <strong>VizitLink3D kapak ve kapı koleksiyonumuzu</strong> bu yılın en gözde tonlarıyla zenginleştirdik.</p><h3>2026'nın Öne Çıkan Renkleri</h3><ul><li><strong>Toprak Tonları:</strong> Kil, kum ve pişmiş toprak renkleri sıcak ve doğal atmosferler yaratıyor</li><li><strong>Yosun ve Adaçayı Yeşili:</strong> Doğa ile bağı güçlendiren huzurlu tonlar</li><li><strong>Sıcak Gri ve Taş:</strong> Minimalist ama davetkar mekanlar için ideal</li><li><strong>Derin Lacivert:</strong> Soylu ve zamansız bir seçenek olarak öne çıkıyor</li><li><strong>Kırık Beyaz ve Krem:</strong> Her dekor stiline kolayca uyum sağlayan nötr tonlar</li></ul><p>Bu renklerin tamamı lake, membran ve akrilik kapak serilerimizde mevcut. Siz de mutfağınızı, banyonuzu veya ofis alanınızı 2026'nın en gözde renkleriyle yenileyin.</p><p>Renk örneklerini fiziksel olarak incelemek için şubemizi ziyaret edebilir ya da online konfigüratörümüzde simüle edebilirsiniz.</p>",
                AnaResimUrl = "/medya/gold-katalog/sayfa-010-hero.png",
                Etiketler = "renk,trend,2026,pantone,tasarım,lake,membran",
                SeoBaslik = "2026 Renk Trendleri — VizitLink3D Kapak ve Kapı",
                SeoAciklama = "Pantone 2026 renk trendleri VizitLink3D koleksiyonunda. Toprak tonları, yosun yeşili ve sıcak griler.",
                YayinTarihi = simdi.AddDays(-28),
                AktifMi = true
            }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaKapiKategorileriAsync(VizitLink3DDbContext vt)
    {
        vt.KapiKategorileri.AddRange(
            new KapiKategorisi { Slug = "membran", Ad = "Membran Kapak", Aciklama = "MDF uzerine isil presleme.", SiraNo = 1 },
            new KapiKategorisi { Slug = "lake", Ad = "Lake Kapak", Aciklama = "UV lake boyama teknolojisi.", SiraNo = 2 },
            new KapiKategorisi { Slug = "laminant", Ad = "Laminant Kapak", Aciklama = "Yuksek basincla preslenmis.", SiraNo = 3 },
            new KapiKategorisi { Slug = "melamin", Ad = "Melamin Kapak", Aciklama = "Melamin recinesi emdirilmis.", SiraNo = 4 },
            new KapiKategorisi { Slug = "kaplama", Ad = "Kaplama Kapak", Aciklama = "Dogal agac kaplama.", SiraNo = 5 }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaSlaytlariAsync(VizitLink3DDbContext vt)
    {
        var tohumlar = new List<Slayt>
        {
            new Slayt { Dil = "tr", Baslik = "Her Mekana Her Yaşama", AltBaslik = "Özel Kapılar", Aciklama = "1992'den beri kalite ve estetik.", ArkaplanResim = "/medya/gold-katalog/sayfa-006-hero.png", ButonMetni1 = "Keşfet", ButonLink1 = "/kapak-sistemleri", SiraNo = 1 },
            new Slayt { Dil = "tr", Baslik = "Çok Boyutlu Şıklık", AltBaslik = "Modern Mutfak Kapakları", Aciklama = "Lake, akrilik ve membran modelleri.", ArkaplanResim = "/medya/gold-katalog/sayfa-010-hero.png", ButonMetni1 = "Modelleri Gör", ButonLink1 = "/kapak-sistemleri", SiraNo = 2 },
            new Slayt { Dil = "tr", Baslik = "Detaylarda Mükemmellik", AltBaslik = "Banyo Dolapları", Aciklama = "Suya dayanıklı özel çözümler.", ArkaplanResim = "/medya/goldbanyo/uretim.jpg", ButonMetni1 = "İncele", ButonLink1 = "/kapak-sistemleri", SiraNo = 3 },
            new Slayt { Dil = "tr", Baslik = "Sanal Tur ile Keşfet", AltBaslik = "3D Fabrika Turu", Aciklama = "Üretim tesisimizi 3 boyutlu keşfedin.", ArkaplanResim = "/medya/goldbanyo/uretim.jpg", ButonMetni1 = "Turu Başlat", ButonLink1 = "/hakkimizda", SiraNo = 4 },
            new Slayt { Dil = "en", Baslik = "A Lifetime for Every Space", AltBaslik = "Custom Doors", Aciklama = "Quality and aesthetics since 1992.", ArkaplanResim = "/medya/gold-katalog/sayfa-006-hero.png", ButonMetni1 = "Discover", ButonLink1 = "/kapak-sistemleri", SiraNo = 1 },
            new Slayt { Dil = "en", Baslik = "Multi-Dimensional Style", AltBaslik = "Modern Kitchen Panels", Aciklama = "Lacquer, acrylic and membrane models.", ArkaplanResim = "/medya/gold-katalog/sayfa-010-hero.png", ButonMetni1 = "View Models", ButonLink1 = "/kapak-sistemleri", SiraNo = 2 },
            new Slayt { Dil = "en", Baslik = "Perfection in Details", AltBaslik = "Bathroom Cabinets", Aciklama = "Water-resistant custom solutions.", ArkaplanResim = "/medya/goldbanyo/uretim.jpg", ButonMetni1 = "Explore", ButonLink1 = "/kapak-sistemleri", SiraNo = 3 },
            new Slayt { Dil = "en", Baslik = "Discover with Virtual Tour", AltBaslik = "3D Factory Tour", Aciklama = "Discover our production facility in 3D.", ArkaplanResim = "/medya/goldbanyo/uretim.jpg", ButonMetni1 = "Start Tour", ButonLink1 = "/hakkimizda", SiraNo = 4 },
            
            // Dinamik Sayfa Slaytları
            new Slayt { Dil = "tr", SayfaKodu = "ekibimiz", Baslik = "Güçlü Ekibimiz", AltBaslik = "Profesyonel Ailesi", Aciklama = "Alanında uzman, yenilikçi ve dinamik ekibimiz.", ArkaplanResim = "/medya/goldbanyo/showroom.jpg", SiraNo = 1 },
            new Slayt { Dil = "tr", SayfaKodu = "vizyon-misyon", Baslik = "Vizyonumuz & Misyonumuz", AltBaslik = "Geleceğe Bakış", Aciklama = "Global pazarda öncü olmak.", ArkaplanResim = "/medya/goldbanyo/uretim.jpg", SiraNo = 1 },
            new Slayt { Dil = "tr", SayfaKodu = "urunler", Baslik = "Geniş Ürün Yelpazesi", AltBaslik = "Tüm Modellerimiz", Aciklama = "Kalite ve estetiğin buluştuğu ürünler.", ArkaplanResim = "/medya/gold-katalog/sayfa-006-hero.png", SiraNo = 1 },
            new Slayt { Dil = "tr", SayfaKodu = "projeler", Baslik = "Projelerimiz", AltBaslik = "Başarı Hikayeleri", Aciklama = "Tamamladığımız gurur verici projeler.", ArkaplanResim = "/medya/goldbanyo/showroom.jpg", SiraNo = 1 },
            new Slayt { Dil = "tr", SayfaKodu = "referanslar", Baslik = "Referanslarımız", AltBaslik = "Bize Güvenenler", Aciklama = "Sektörün önde gelen markalarıyla çalışıyoruz.", ArkaplanResim = "/medya/goldbanyo/hakkimizda/fabrika.jpg", SiraNo = 1 },
            new Slayt { Dil = "tr", SayfaKodu = "haber", Baslik = "Haberler & Duyurular", AltBaslik = "Güncel Gelişmeler", Aciklama = "En son haberler ve duyurular.", ArkaplanResim = "/medya/goldbanyo/showroom.jpg", SiraNo = 1 },
            new Slayt { Dil = "tr", SayfaKodu = "sss", Baslik = "Sıkça Sorulan Sorular", AltBaslik = "Bilgi Bankası", Aciklama = "Merak ettiğiniz tüm soruların cevapları.", ArkaplanResim = "/medya/goldbanyo/uretim.jpg", SiraNo = 1 },

            new Slayt { Dil = "tr", SayfaKodu = "kapak-sistemleri", Baslik = "Kapak Sistemleri", AltBaslik = "Mutfak ve Banyo Çözümleri", Aciklama = "Membran, lake, laminat ve akrilik kapak modellerimiz.", ArkaplanResim = "/medya/gold-katalog/sayfa-006-hero.png", SiraNo = 1 },
            new Slayt { Dil = "tr", SayfaKodu = "bayiler", Baslik = "Bayilerimiz", AltBaslik = "Yetkili Satış Noktaları", Aciklama = "Türkiye genelinde VizitLink3D yetkili satış noktaları.", ArkaplanResim = "/medya/goldbanyo/showroom.jpg", SiraNo = 1 },
            new Slayt { Dil = "tr", SayfaKodu = "sertifikalar", Baslik = "Sertifikalarımız", AltBaslik = "Kalite Belgelerimiz", Aciklama = "Uluslararası standartlarda kalite ve güvence.", ArkaplanResim = "/medya/goldbanyo/uretim.jpg", SiraNo = 1 },
            new Slayt { Dil = "tr", SayfaKodu = "iletisim", Baslik = "İletişim", AltBaslik = "Bize Ulaşın", Aciklama = "Size en yakın VizitLink3D temsilcisiyle görüşün.", ArkaplanResim = "/medya/goldbanyo/showroom.jpg", SiraNo = 1 },
        };

        foreach (var tohum in tohumlar)
        {
            var mevcutSlayt = await vt.Slaytlar.FirstOrDefaultAsync(s => s.Dil == tohum.Dil && s.SayfaKodu == tohum.SayfaKodu && s.SiraNo == tohum.SiraNo);
            if (mevcutSlayt == null)
            {
                vt.Slaytlar.Add(tohum);
            }
            else
            {
                mevcutSlayt.Baslik = tohum.Baslik;
                mevcutSlayt.AltBaslik = tohum.AltBaslik;
                mevcutSlayt.Aciklama = tohum.Aciklama;
                mevcutSlayt.ButonMetni1 = tohum.ButonMetni1;
                mevcutSlayt.ArkaplanResim = tohum.ArkaplanResim;
            }
        }
        await vt.SaveChangesAsync();
    }

    private static async Task SlaytResimYollariniDuzeltAsync(VizitLink3DDbContext vt)
    {
        var yolHaritasi = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "/medya/slaytlar/Lake Kapılar, DSL  C110, Camlı Model.jpg", "/medya/gold-katalog/sayfa-010-hero.png" },
            { "/medya/slaytlar/Lake Kapılar, DSL 113,.jpg",                "/medya/gold-katalog/sayfa-006-hero.png" }
        };

        var guncelleme = false;
        foreach (var (eski, yeni) in yolHaritasi)
        {
            var slaytlar = await vt.Slaytlar.Where(s => s.ArkaplanResim == eski).ToListAsync();
            foreach (var s in slaytlar) { s.ArkaplanResim = yeni; guncelleme = true; }
        }
        if (guncelleme) await vt.SaveChangesAsync();
    }

    private static async Task AciliKapakModelleriniSilAsync(VizitLink3DDbContext vt)
    {
        // NRD membran serisi acili 3D render gorsellerine sahip — thumb_, yatay_, kapaklar_ prefix varyantlari
        // Kullanici istegi: sadece duz-dik kapak fotograflari kalmali (kapi-modelleri/ klasoru)
        var aciliModeller = await vt.KapakModelleri
            .Where(k => !k.SilindiMi
                     && k.AnaGorselUrl != null
                     && k.AnaGorselUrl.StartsWith("/medya/kapaklar/"))
            .ToListAsync();

        foreach (var m in aciliModeller)
        {
            m.SilindiMi = true;
            m.SilinmeTarihi = DateTime.UtcNow;
        }

        if (aciliModeller.Any())
            await vt.SaveChangesAsync();
    }

    /// <summary>
    /// Her kapak serisine uygulama/ortam gorsellerini (kitchen scene) baglar.
    /// lk- = Lake Kapı, mk- = Membran Kapı, kl- = BOY/Klasik, hg- = Camlı Modeller
    /// </summary>
    private static async Task UrunOrtamGorselleriniEkleAsync(VizitLink3DDbContext vt)
    {
        // ModelKodu prefix -> ona ozel mutfak/ortam (room scene) webp gorselleri
        // Bu gorsel listesi AnaGorselUrl'nin (kapak close-up) ONUNE gelir; o kullanici ana gorsel olarak gorur
        var haritasi = new Dictionary<string, string[]>
        {
            { "DSL", ["/medya/gold-katalog/sayfa-006-hero.png", "/medya/gold-katalog/sayfa-010-hero.png", "/medya/goldbanyo/uretim.jpg"] },
            { "DSM", ["/medya/gold-katalog/sayfa-006-hero.png", "/medya/gold-katalog/sayfa-010-hero.png", "/medya/goldbanyo/uretim.jpg"] },
            { "BOY", ["/medya/gold-katalog/sayfa-006-hero.png", "/medya/gold-katalog/sayfa-010-hero.png", "/medya/goldbanyo/uretim.jpg"] },
            { "CAM", ["/medya/gold-katalog/sayfa-006-hero.png", "/medya/gold-katalog/sayfa-010-hero.png", "/medya/goldbanyo/uretim.jpg"] },
        };

        var guncellendi = false;
        foreach (var (prefix, ortamGorselleri) in haritasi)
        {
            var urunler = await vt.KapakModelleri
                .Where(k => !k.SilindiMi && k.ModelKodu.StartsWith(prefix))
                .ToListAsync();

            foreach (var u in urunler)
            {
                // Mevcut listeyi deserialize et
                var mevcut = System.Text.Json.JsonSerializer.Deserialize<List<string>>(u.UygulamaGorselleriJson ?? "[]") ?? [];

                // Ortam gorselleri zaten basa eklenmisse atla
                if (mevcut.Count > 0 && mevcut[0].StartsWith("/medya/kapaklar/"))
                    continue;

                // Ortam gorsellerini basa, kapi close-up'i sona koy
                var yeni = new List<string>(ortamGorselleri);
                foreach (var m in mevcut)
                    if (!yeni.Contains(m)) yeni.Add(m);

                u.UygulamaGorselleriJson = System.Text.Json.JsonSerializer.Serialize(yeni);
                guncellendi = true;
            }
        }

        if (guncellendi) await vt.SaveChangesAsync();
    }

    private static async Task TohumlaHizmetAdimlariniAsync(VizitLink3DDbContext vt)
    {
        var tohumlar = new List<HizmetAdimi>
        {
            new HizmetAdimi { Baslik = "Ölçüm ve Keşif", Aciklama = "Uzman ekibimiz evinize gelerek hassas ölçümler yapar.", Ikon = "SquareFoot", AdimNo = 1, SiraNo = 1 },
            new HizmetAdimi { Baslik = "Ön Tasarım", Aciklama = "Mimarlarımız 3D ön tasarım çalışmalarını hazırlar.", Ikon = "DesignServices", AdimNo = 2, SiraNo = 2 },
            new HizmetAdimi { Baslik = "Detaylı Tasarım ve Üretim", Aciklama = "CNC makinelerimizde üretime başlanır.", Ikon = "PrecisionManufacturing", AdimNo = 3, SiraNo = 3 },
            new HizmetAdimi { Baslik = "Kurulum ve Teslim", Aciklama = "Uzman montaj ekibimiz ürünlerinizi kurar.", Ikon = "Build", AdimNo = 4, SiraNo = 4 }
        };

        foreach (var tohum in tohumlar)
        {
            var mevcutAdim = await vt.HizmetAdimlari.FirstOrDefaultAsync(h => h.AdimNo == tohum.AdimNo);
            if (mevcutAdim == null)
            {
                vt.HizmetAdimlari.Add(tohum);
            }
            else
            {
                mevcutAdim.Baslik = tohum.Baslik;
                mevcutAdim.Aciklama = tohum.Aciklama;
            }
        }
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaSSSAsync(VizitLink3DDbContext vt)
    {
        vt.SikSorulanSorular.AddRange(
            new SikSorulanSoru { Soru = "Urunlerinizin garanti suresi ne kadardir?", Cevap = "Tum VizitLink3D urunleri 24 ay garantilidir.", KategoriAdi = "Genel", SiraNo = 1 },
            new SikSorulanSoru { Soru = "Olculendirme islemi nasil yapilir?", Cevap = "Ucretisz olarak evinizde uzman ekip tarafindan yapilir.", KategoriAdi = "Genel", SiraNo = 2 },
            new SikSorulanSoru { Soru = "Teslimat suresi ne kadardir?", Cevap = "Uretime gore 7-15 is gunu arasinda degisir.", KategoriAdi = "Urun", SiraNo = 3 },
            new SikSorulanSoru { Soru = "Odeme secenekleri nelerdir?", Cevap = "Nakit, kredi karti ve taksitli odeme secenekleri mevcuttur.", KategoriAdi = "Genel", SiraNo = 4 },
            new SikSorulanSoru { Soru = "Sehir disina kargo yapiliyor mu?", Cevap = "Evet, Turkiyenin her yerine kargo ve montaj yapilir.", KategoriAdi = "Hizmet", SiraNo = 5 }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaDilleriAsync(VizitLink3DDbContext vt)
    {
        var diller = new List<Dil>
        {
            new Dil { Kod = "tr", Ad = "Turkce", Bayrak = "fi fi-tr", SiraNo = 1, VarsayilanMi = true },
            new Dil { Kod = "en", Ad = "English", Bayrak = "fi fi-gb", SiraNo = 2 },
            new Dil { Kod = "de", Ad = "Deutsch", Bayrak = "fi fi-de", SiraNo = 3 },
            new Dil { Kod = "fr", Ad = "Français", Bayrak = "fi fi-fr", SiraNo = 4 },
            new Dil { Kod = "ru", Ad = "Русский", Bayrak = "fi fi-ru", SiraNo = 5 },
            new Dil { Kod = "ar", Ad = "العربية", Bayrak = "fi fi-sa", SiraNo = 6 },
            new Dil { Kod = "es", Ad = "Español", Bayrak = "fi fi-es", SiraNo = 7 },
            new Dil { Kod = "zh", Ad = "中文", Bayrak = "fi fi-cn", SiraNo = 8 }
        };
        
        var mevcutDiller = await vt.Diller.ToListAsync();
        var updateMappings = new Dictionary<string, string>
        {
            { "tr", "fi fi-tr" }, { "en", "fi fi-gb" }, { "de", "fi fi-de" },
            { "fr", "fi fi-fr" }, { "ru", "fi fi-ru" }, { "ar", "fi fi-sa" },
            { "es", "fi fi-es" }, { "zh", "fi fi-cn" }
        };
        
        bool updated = false;
        foreach (var dil in mevcutDiller)
        {
            if (updateMappings.TryGetValue(dil.Kod, out var cssClass) && dil.Bayrak != cssClass)
            {
                dil.Bayrak = cssClass;
                updated = true;
            }
        }
        
        var mevcutKodlar = mevcutDiller.Select(d => d.Kod).ToList();
        var eklenecekler = diller.Where(d => !mevcutKodlar.Contains(d.Kod)).ToList();
        
        if (eklenecekler.Any())
        {
            vt.Diller.AddRange(eklenecekler);
            await vt.SaveChangesAsync();
        }
    }

    private static async Task TohumlaCevirileriAsync(VizitLink3DDbContext vt)
    {
        var mevcut = await vt.Ceviriler.Select(c => c.Anahtar + "|" + c.Dil).ToListAsync();
        var temelCeviriler = TemelCevirileriGetir();

        foreach (var (anahtar, tr, en) in temelCeviriler)
        {
            if (!mevcut.Contains(anahtar + "|tr"))
            {
                vt.Ceviriler.Add(new Ceviri { Anahtar = anahtar, Dil = "tr", Deger = tr, Bolum = anahtar.Contains('.') ? anahtar[..anahtar.IndexOf('.')] : "genel", OlusturulmaTarihi = DateTime.UtcNow });
                mevcut.Add(anahtar + "|tr");
            }
            if (!mevcut.Contains(anahtar + "|en"))
            {
                vt.Ceviriler.Add(new Ceviri { Anahtar = anahtar, Dil = "en", Deger = en, Bolum = anahtar.Contains('.') ? anahtar[..anahtar.IndexOf('.')] : "genel", OlusturulmaTarihi = DateTime.UtcNow });
                mevcut.Add(anahtar + "|en");
            }
        }
        await vt.SaveChangesAsync();
    }

    private static List<(string anahtar, string tr, string en)> TemelCevirileriGetir() => new()
    {
        ("ortak.kaydet", "KAYDET", "SAVE"),
        ("ortak.iptal", "İPTAL", "CANCEL"),
        ("ortak.sil", "SİL", "DELETE"),
        ("ortak.duzenle", "DÜZENLE", "EDIT"),
        ("ortak.ekle", "YENİ EKLE", "ADD NEW"),
        ("ortak.tamam", "TAMAM", "OK"),
        ("ortak.kapat", "Kapat", "Close"),
        ("ortak.evet", "Evet", "Yes"),
        ("ortak.ara", "Ara", "Search"),
        ("ortak.yukleniyor", "Yükleniyor...", "Loading..."),
        ("ortak.basarili", "İşlem başarılı.", "Success."),
        ("ortak.hata", "Hata oluştu.", "Error occurred."),
        ("ortak.cikis", "Çıkış", "Logout"),
        ("ortak.giris", "Giriş", "Login"),
        ("ortak.tumu", "Tümü", "All"),
        // Ortak kullanilan metinler
        ("ortak.onayGerekiyor", "Onay Gerekiyor", "Confirmation Required"),
        ("ortak.silmeOnayMesaji", "Bu işlem geri alınamaz.", "This action cannot be undone."),
        ("ortak.evetSil", "Evet, Sil", "Yes, Delete"),
        ("ortak.yenile", "Yenile", "Refresh"),
        ("ortak.geriDon", "Geri Dön", "Go Back"),
        ("ortak.anahtar", "Anahtar", "Key"),
        ("ortak.deger", "Değer", "Value"),
        ("ortak.tarih", "Tarih", "Date"),
        ("ortak.kullanici", "Kullanıcı", "User"),
        ("ortak.firma", "Firma", "Company"),
        ("ortak.islem", "İşlem", "Action"),
        ("ortak.aktif", "Aktif", "Active"),
        ("ortak.pasif", "Pasif", "Inactive"),
        ("ortak.yeni", "Yeni", "New"),
        ("ortak.kayit", "kayıt", "records"),
        ("ortak.kayitBulunamadi", "Kayıt bulunamadı.", "No records found."),
        ("ortak.baslik", "Başlık", "Title"),
        ("ortak.ad", "Ad", "Name"),
        ("ortak.aciklama", "Açıklama", "Description"),
        ("ortak.siraNo", "Sıra No", "Order"),
        ("ortak.islemler", "İşlemler", "Actions"),
        ("ortak.durum", "Durum", "Status"),
        ("ortak.olusturma", "Oluşturma", "Created"),
        ("ortak.tip", "Tip", "Type"),
        // Public site header/footer
        ("ortak.dil", "Dil", "Language"),
        ("ortak.acik", "Açık", "Light"),
        ("ortak.koyu", "Koyu", "Dark"),
        ("site.teklifAl", "Teklif Al", "Get a Quote"),
        ("site.koleksiyonlar", "Koleksiyonlar", "Collections"),
        ("site.katalog", "Katalog", "Catalogue"),
        ("site.kurumsal", "Kurumsal", "Corporate"),
        ("site.fabrikamiz", "Fabrikamız", "Our Factory"),
        ("site.iletisim", "İletişim", "Contact"),
        ("site.tumHaklariSaklidir", "Tüm hakları saklıdır.", "All rights reserved."),
        // Public site ust menu
        ("menu.anasayfa", "Ana Sayfa", "Home"),
        ("menu.urunler", "Ürünler", "Products"),
        ("menu.goldExclusive", "Gold Exclusive", "Gold Exclusive"),
        ("menu.goldPremium", "Gold Premium", "Gold Premium"),
        ("menu.goldTrend", "Gold Trend", "Gold Trend"),
        ("menu.goldStandart", "Gold Standart", "Gold Standard"),
        ("menu.tumUrunler", "Tüm Ürünler", "All Products"),
        ("menu.katalog", "Katalog", "Catalogue"),
        ("menu.projeler", "Projeler", "Projects"),
        ("menu.kurumsal", "Kurumsal", "Corporate"),
        ("menu.hakkimizda", "Hakkımızda", "About Us"),
        ("menu.vizyonMisyon", "Vizyon & Misyon", "Vision & Mission"),
        ("menu.ekibimiz", "Ekibimiz", "Our Team"),
        ("menu.sertifikalarimiz", "Sertifikalarımız", "Our Certificates"),
        ("menu.bayiler", "Bayiler", "Dealers"),
        ("menu.fabrikamiz", "Fabrikamız", "Our Factory"),
        ("menu.referanslar", "Referanslar", "References"),
        ("menu.haber", "Haber", "News"),
        ("menu.sss", "SSS", "FAQ"),
        ("menu.iletisim", "İletişim", "Contact"),
        // Urunler.razor
        ("urunler.heroBaslik", "Banyo dolaplarında altın oran, yumuşak çizgi ve showroom etkisi.", "The golden ratio, soft lines and showroom feel in bathroom cabinets."),
        ("urunler.heroAciklama", "Ürünler artık admin panelinden yönetilen gerçek ürün kayıtlarından beslenen temiz bir vitrin akışıyla sunuluyor. Koleksiyonlara göre gezebilir, her modelin detay sahnesine tek tıkla geçebilirsin.", "Products are now presented through a clean showcase fed by real product records managed from the admin panel. Browse by collection and jump into any model's detail scene in one click."),
        ("urunler.koleksiyonlar", "Koleksiyonları İncele", "Browse Collections"),
        ("urunler.model", "Model", "Model"),
        ("urunler.koleksiyon", "Koleksiyon", "Collection"),
        ("urunler.oneCikan", "Öne Çıkan Seri", "Featured Series"),
        ("urunler.sezon", "Katalog Sezonu", "Catalogue Season"),
        ("urunler.filtre", "Koleksiyon Filtresi", "Collection Filter"),
        ("urunler.filtreBaslik", "Koleksiyona göre gezinin", "Browse by collection"),
        ("urunler.fiyatBelliDegil", "Fiyat bilgisi yok", "Price not available"),
        ("urun.yeni", "Yeni", "New"),
        ("urun.oneCikan", "Öne Çıkan", "Featured"),
        ("urun.detayAc", "Detayları Keşfet", "View Details"),
        // UrunDetay.razor
        ("urun.detay", "Ürün detayı", "Product details"),
        ("urun.bulunamadi", "Model bulunamadı", "Model not found"),
        ("urun.listeyeDon", "Listeye dön", "Back to list"),
        ("urun.ozellikler", "Özellikler", "Features"),
        ("urun.renkler", "Renkler", "Colours"),
        ("urun.renkBilgisiYok", "Bu ürün için renk bilgisi tanımlı değil.", "No colour information is defined for this product."),
        ("urun.listeFiyati", "Liste Fiyatı", "List Price"),
        ("urun.teknikOlculer", "Teknik Ölçüler", "Technical Dimensions"),
        ("teklif.al", "Teklif Al", "Get a Quote"),
        ("urun.benzer", "Aynı Koleksiyondan", "From the Same Collection"),
        ("urun.benzerUrunler", "Benzer modeller", "Similar models"),
        ("urun.benzerAciklama", "Aynı aile veya kategori içindeki diğer ürünlere hızlıca geçebilirsin.", "Quickly move to other products in the same family or category."),
        // Iletisim.razor
        ("iletisim.telefon", "Telefon", "Phone"),
        ("iletisim.adres", "Adres", "Address"),
        ("iletisim.eposta", "E-Posta", "Email"),
        // KatalogSayfasi.razor
        ("katalog.baslik", "Gold Banyo Katalog", "Gold Banyo Catalogue"),
        ("katalog.aciklama", "2026 katalog secili banyo dolabi modellerini koleksiyon mantigiyla sunar.", "The 2026 catalogue presents selected bathroom cabinet models organised by collection."),
        ("katalog.pdfAc", "PDF Aç", "Open PDF"),
        // ProjeDetay.razor
        ("projeDetay.baslik", "Proje Detay", "Project Details"),
        ("projeDetay.aciklama", "Bu rota yeni public yapida sade tutuldu. Detay modulu ikinci asamada yeniden kurulacak.", "This route was kept minimal in the new public structure. The detail module will be rebuilt in the next phase."),
        // HaberDetay.razor
        ("haberDetay.baslik", "Haber Detay", "News Details"),
        // NotFound.razor
        ("bulunamadi.baslik", "Bulunamadı", "Not Found"),
        ("bulunamadi.mesaj", "Sayfa bulunamadı", "Page not found"),
        ("bulunamadi.anaSayfayaDon", "Ana sayfaya dön", "Back to home"),
        // AnaSayfa.razor
        ("anasayfa.title", "Gold Banyo | Lüks Banyo Mobilyaları", "Gold Banyo | Luxury Bathroom Furniture"),
        ("anasayfa.hero.etiket", "Gold Banyo 2026 Koleksiyonu", "Gold Banyo 2026 Collection"),
        ("anasayfa.hero.baslik1", "Siz Hayal Edin,", "You Imagine,"),
        ("anasayfa.hero.baslik2", "Biz Tasarlayalım.", "We Design."),
        ("anasayfa.hero.aciklama", "Premium malzemeler, kusursuz işçilik ve zamansız endüstriyel tasarım. Banyonuzu bir mimari şahesere dönüştürün.", "Premium materials, flawless craftsmanship and timeless industrial design. Transform your bathroom into an architectural masterpiece."),
        ("anasayfa.hero.butonKoleksiyon", "Koleksiyonları İncele", "Browse Collections"),
        ("anasayfa.hero.butonTeklif", "Teklif Al", "Get a Quote"),
        ("anasayfa.metrik.yilTecrubeSayi", "35+", "35+"),
        ("anasayfa.metrik.yilTecrubeEtiket", "Yıl Tecrübe", "Years of Experience"),
        ("anasayfa.metrik.projeSayi", "10K+", "10K+"),
        ("anasayfa.metrik.projeEtiket", "Proje", "Projects"),
        ("anasayfa.metrik.ozelKoleksiyonSayi", "20+", "20+"),
        ("anasayfa.metrik.ozelKoleksiyonEtiket", "Özel Koleksiyon", "Signature Collections"),
        ("anasayfa.metrik.ihracatAgiSayi", "Global", "Global"),
        ("anasayfa.metrik.ihracatAgiEtiket", "İhracat Ağı", "Export Network"),
        ("anasayfa.koleksiyon.baslik", "Koleksiyonlar", "Collections"),
        ("anasayfa.koleksiyon.exclusiveEtiket", "Premium Seri", "Premium Series"),
        ("anasayfa.koleksiyon.exclusiveBaslik", "Gold Exclusive", "Gold Exclusive"),
        ("anasayfa.koleksiyon.exclusiveAciklama", "Sınırları zorlayan mimari çizgiler.", "Architectural lines that push the boundaries."),
        ("anasayfa.koleksiyon.premiumBaslik", "Premium Serisi", "Premium Series"),
        ("anasayfa.koleksiyon.premiumAciklama", "Zarafet ve fonksiyonellik.", "Elegance and functionality."),
        ("anasayfa.koleksiyon.trendBaslik", "Trend Serisi", "Trend Series"),
        ("anasayfa.koleksiyon.trendAciklama", "Modern yaşam alanları için.", "For modern living spaces."),
        ("anasayfa.koleksiyon.standartBaslik", "Standart Seri", "Standard Series"),
        ("anasayfa.koleksiyon.standartAciklama", "Zamansız klasikler.", "Timeless classics."),
        ("anasayfa.uretim.etiket", "Üretim & İşçilik", "Production & Craftsmanship"),
        ("anasayfa.uretim.baslik1", "Endüstriyel Lüksün", "The Address of"),
        ("anasayfa.uretim.baslik2", "Adresi", "Industrial Luxury"),
        ("anasayfa.uretim.aciklama", "10.000 m² kapalı alanda, son teknoloji CNC tezgahları ve ustalık geleneğinin buluştuğu nokta. Her banyo dolabı, titiz bir kalite kontrol sürecinden geçer.", "Where state-of-the-art CNC machinery meets a tradition of craftsmanship across 10,000 m² of covered production space. Every bathroom cabinet undergoes a meticulous quality control process."),
        ("anasayfa.uretim.buton", "Fabrikamızı Keşfedin", "Discover Our Factory"),
        ("anasayfa.onecikanModeller.baslik", "Öne Çıkan Modeller", "Featured Models"),
        ("anasayfa.onecikanModeller.diago100Kod", "Diago 100", "Diago 100"),
        ("anasayfa.onecikanModeller.diago100Ad", "Diago 100 Canlı Detay", "Diago 100 Live Detail"),
        ("anasayfa.onecikanModeller.diago100Aciklama", "Çarpıcı geometrik çizgiler ve lüks yüzeyler.", "Striking geometric lines and luxurious surfaces."),
        ("anasayfa.onecikanModeller.giorgio100Kod", "Giorgio 100", "Giorgio 100"),
        ("anasayfa.onecikanModeller.giorgio100Ad", "Giorgio 100 Hareketli Detay", "Giorgio 100 Motion Detail"),
        ("anasayfa.onecikanModeller.giorgio100Aciklama", "Zarif form ve işlevsellik bir arada.", "Elegant form meets functionality."),
        ("anasayfa.onecikanModeller.diago360Kod", "Diago 360", "Diago 360"),
        ("anasayfa.onecikanModeller.diago360Ad", "Diago 360 3D Görünüm", "Diago 360 3D View"),
        ("anasayfa.onecikanModeller.diago360Aciklama", "360° döndürülebilir interaktif deneyim.", "A 360° rotatable interactive experience."),
        ("anasayfa.katalog.baslik", "2026 Kataloğumuz", "Our 2026 Catalogue"),
        ("anasayfa.katalog.aciklama", "Tüm koleksiyonlarımızı ve teknik detayları dijital kataloğumuzda inceleyin.", "Explore all our collections and technical details in our digital catalogue."),
        ("anasayfa.katalog.buton", "Kataloğu İncele", "Browse Catalogue"),
        ("anasayfa.cta.baslik", "Banyonuz İçin Teklif Alın", "Get a Quote for Your Bathroom"),
        ("anasayfa.cta.aciklama", "Uzman ekibimiz, mekanınıza en uygun koleksiyonu önermek için sizinle iletişime geçsin.", "Let our expert team contact you to recommend the best collection for your space."),
        ("anasayfa.cta.buton", "İletişime Geçin", "Contact Us"),
        ("admin.baslik", "Gold Banyo Yönetim", "Gold Banyo Admin"),
        ("admin.giris.altbaslik", "Devam etmek için giriş yapın", "Sign in to continue"),
        ("admin.giris.kullaniciadi", "Kullanıcı Adı", "Username"),
        ("admin.giris.sifre", "Şifre", "Password"),
        ("admin.giris.buton", "SİSTEME GİRİŞ YAP", "SIGN IN"),
        ("admin.giris.siteyedon", "Siteye Dön", "Back to Site"),
        ("admin.giris.kullaniciAdiPlaceholder", "admin", "admin"),
        ("admin.giris.sifrePlaceholder", "••••••••", "••••••••"),
        ("admin.giris.markaYil", "GOLD BANYO 1989", "GOLD BANYO 1989"),
        ("admin.giris.slogan1", "Her Mekana", "For Every Space"),
        ("admin.giris.slogan2", "Özel Banyolar", "Signature Bathrooms"),
        ("admin.giris.mirasAciklama", "Premium banyo mobilyalarında köklü üretim deneyimini modern koleksiyonlarla buluşturuyoruz.", "We blend deep manufacturing experience in premium bathroom furniture with modern collections."),
        ("admin.giris.yilTecrube", "KOLEKSİYON", "COLLECTION"),
        ("admin.giris.ozgunModel", "KOLEKSİYON", "COLLECTION"),
        ("admin.giris.ulkeIhracat", "KOLEKSİYON", "COLLECTION"),
        ("admin.dashboard", "Gösterge Paneli", "Dashboard"),
        ("istakip.baslik", "İş Takip Defteri", "Task Tracker"),
        ("istakip.bos", "Henüz iş kaydı yok.", "No tasks yet."),
        ("admin.sistemDurumu", "Sistem Durumu", "System Status"),
        ("admin.veritabani", "Veritabanı", "Database"),
        ("admin.depolama", "Depolama", "Storage"),
        ("admin.hizliEylemler", "Hızlı Eylemler", "Quick Actions"),
        ("admin.canliAkis", "Canlı Akış", "Live Stream"),
        ("admin.cevrimici", "Çevrimiçi", "Online"),
        ("admin.toplamZiyaret", "Toplam Ziyaret", "Total Visits"),
        ("admin.yeniMesaj", "Yeni Mesaj", "New Messages"),
        ("admin.bekleyenIsler", "Bekleyen İş", "Pending Tasks"),
        ("admin.kritikIsler", "Kritik İş", "Critical Tasks"),
        ("admin.toplamUrun", "Ürün", "Products"),
        ("admin.menuOgesi", "Menü", "Menu"),
        ("admin.yeniBlog", "Yeni Haber", "New Article"),
        ("admin.yeniSlayt", "Yeni Slayt", "New Slide"),
        ("admin.medyaYukle", "Medya Yükle", "Upload Media"),
        ("admin.temaDuzenle", "Tema Düzenle", "Edit Theme"),
        ("admin.aktiviteGecmisi", "Son Etkinlikler", "Recent Activity"),
        ("admin.icerikSagligi", "İçerik Sağlığı", "Content Health"),
        ("admin.etkinlikYok", "Henüz etkinlik yok.", "No activity yet."),
        ("admin.etkinlikBekleniyor", "Aktivite akışı bekleniyor...", "Waiting for activity..."),
        ("hata.404.baslik", "Sayfa Bulunamadı", "Page Not Found"),
        ("hata.404.aciklama", "Sayfa taşınmış olabilir.", "Page may have moved."),
        ("hata.404.anasayfa", "Anasayfaya Dön", "Go Home"),
        ("istakip.bekleyenIsler", "Bekleyen ve devam eden işler", "Pending and ongoing tasks"),
        ("istakip.bos", "Henüz iş kaydı yok. Yeni bir iş ekleyin.", "No tasks yet. Add a new task."),
        ("istakip.yeniIs", "Yeni İş", "New Task"),
        ("istakip.isiDuzenle", "İşi Düzenle", "Edit Task"),
        ("istakip.yeniIsEkle", "Yeni İş Ekle", "Add New Task"),
        ("istakip.tumunuGor", "Tümünü Gör", "View All"),
        ("istakip.baslikKolon", "Başlık", "Title"),
        ("istakip.kategoriKolon", "Kategori", "Category"),
        ("istakip.durumKolon", "Durum", "Status"),
        ("istakip.oncelikKolon", "Öncelik", "Priority"),
        ("istakip.tarihKolon", "Tarih", "Date"),
        ("istakip.aciklama", "Açıklama", "Description"),
        ("istakip.durum.bekliyor", "Bekliyor", "Pending"),
        ("istakip.durum.yapiliyor", "Yapılıyor", "In Progress"),
        ("istakip.durum.tamamlandi", "Tamamlandı", "Completed"),
        ("istakip.durum.iptal", "İptal", "Cancelled"),
        ("istakip.oncelik.dusuk", "Düşük", "Low"),
        ("istakip.oncelik.orta", "Orta", "Medium"),
        ("istakip.oncelik.yuksek", "Yüksek", "High"),
        ("istakip.oncelik.kritik", "Kritik", "Critical"),
        ("istakip.kategori.backend", "Backend", "Backend"),
        ("istakip.kategori.frontend", "Frontend", "Frontend"),
        ("istakip.kategori.tasarim", "Tasarım", "Design"),
        ("istakip.kategori.altyapi", "Altyapı", "Infrastructure"),
        ("istakip.kategori.diger", "Diğer", "Other"),
        ("admin.canli", "CANLI", "LIVE"),
        ("admin.baglaniyor", "BAĞLANIYOR...", "CONNECTING..."),
        ("admin.bagli", "Bağlı", "Connected"),
        ("admin.kapali", "Kapalı", "Offline"),

        // Public site translations
        ("teklif_al", "Teklif Al", "Get Quote"),
        ("hizli_baglantilar", "Hızlı Bağlantılar", "Quick Links"),
        ("kategoriler", "Kategoriler", "Categories"),
        ("iletisim_bilgileri", "İletişim Bilgileri", "Contact Info"),
        ("tum_haklar", "Tüm Hakları Saklıdır.", "All Rights Reserved."),
        ("gizlilik", "Gizlilik Politikası", "Privacy Policy"),
        ("ana_sayfa", "Ana Sayfa", "Home"),
        ("urunler", "Ürünler", "Products"),
        ("kurumsal", "Kurumsal", "Corporate"),
        ("projeler", "Projeler", "Projects"),
        ("referanslar", "Referanslar", "References"),
        ("nav_Blog", "Blog", "Blog"),
        ("nav_Haber", "Haber", "News"),
        ("nav_Katalog", "Katalog", "Catalog"),
        ("sss", "SSS", "FAQ"),
        ("iletisim", "İletişim", "Contact"),
        // Admin menu translations
        ("Gosterge Paneli", "Gösterge Paneli", "Dashboard"),
        ("Is Takip", "İş Takip", "Task Tracker"),
        ("Urun Yonetimi", "Ürün Yönetimi", "Product Management"),
        ("3D / Konfigurator", "3D / Konfigüratör", "3D / Configurator"),
        ("Icerik Yonetimi", "İçerik Yönetimi", "Content Management"),
        ("Medya", "Medya", "Media"),
        ("Pazarlama", "Pazarlama", "Marketing"),
        ("Iletisim / Destek", "İletişim / Destek", "Support"),
        ("Organizasyon", "Organizasyon", "Organization"),
        ("Sistem", "Sistem", "System"),
        ("Urunler", "Ürünler", "Products"),
        ("Kapak Modelleri", "Kapak Modelleri", "Panel Models"),
        ("Urun Aileleri", "Ürün Aileleri", "Product Families"),
        ("Urun Kategorileri", "Ürün Kategorileri", "Categories"),
        ("RAL Renk Yonetimi", "RAL Renk Yönetimi", "RAL Colors"),
        ("Malzeme Yonetimi", "Malzeme Yönetimi", "Materials"),
        ("Kaplama Yonetimi", "Kaplama Yönetimi", "Coatings"),
        ("3D Model Yonetimi", "3D Model Yönetimi", "3D Models"),
        ("Parca Esleme", "Parça Eşleme", "Part Matching"),
        ("Konfigurasyon Sablonlari", "Konfigürasyon Şablonları", "Config Templates"),
        ("Konfigurasyon Kurallari", "Konfigürasyon Kuralları", "Config Rules"),
        ("Sahne Ayarlari", "Sahne Ayarları", "Scene Settings"),
        ("Ana Sayfa", "Ana Sayfa", "Home Page"),
        ("Slayt Yonetimi", "Slayt Yönetimi", "Slides"),
        ("Sayfa Icerikleri", "Sayfa İçerikleri", "Page Content"),
        ("Blog Yonetimi", "Haber Yönetimi", "News"),
        ("SSS Yonetimi", "SSS Yönetimi", "FAQ"),
        ("Sayfa Yonetimi", "Sayfa Yönetimi", "Pages"),
        ("SEO Yonetimi", "SEO Yönetimi", "SEO"),
        ("Medya Havuzu", "Medya Havuzu", "Media Pool"),
        ("Medya Galerisi", "Medya Galerisi", "Gallery"),
        ("PDF Katalog", "PDF Katalog", "PDF Catalog"),
        ("Proje Yonetimi", "Proje Yönetimi", "Projects"),
        ("Musteri Yorumlari", "Müşteri Yorumları", "Reviews"),
        ("Hizmet Adimlari", "Hizmet Adımları", "Service Steps"),
        ("Katalog Yonetimi", "Katalog Yönetimi", "Catalogs"),
        ("Bulten Aboneleri", "Bülten Aboneleri", "Newsletter"),
        ("E-posta Sablonlari", "E-posta Şablonları", "Email Templates"),
        ("Gelen Mesajlar", "Gelen Mesajlar", "Inbox"),
        ("Canli Sohbet", "Canlı Sohbet", "Live Chat"),
        ("Teklif Yonetimi", "Teklif Yönetimi", "Quotes"),
        ("Sube Yonetimi", "Şube Yönetimi", "Branches"),
        ("Ekip Yonetimi", "Ekip Yönetimi", "Team"),
        ("Kullanici Yonetimi", "Kullanıcı Yönetimi", "Users"),
        ("Dil ve Ceviri", "Dil ve Çeviri", "Translations"),
        ("AI Ayarlari", "AI Ayarları", "AI Settings"),
        ("Gorunum & Tema", "Görünüm & Tema", "Appearance"),
        ("API Entegrasyonlari", "API Entegrasyonları", "API Integrations"),
        ("Sistem Ayarlari", "Sistem Ayarları", "Settings"),
        ("Denetim Loglari", "Denetim Logları", "Audit Logs"),
        ("Cop Kutusu", "Çöp Kutusu", "Trash"),
        ("Menu Yonetimi", "Menü Yönetimi", "Menu Editor"),
        /// Navbar menu exact titles (API'den gelen Baslik degerleri) â€” zaten
        /// kucuk harf anahtarlar mevcut, bunlar sadece navbar T() cagrilari icin.
        /// Cakismayi onlemek icin 'nav_' oneki eklenmistir.
        ("nav_Ana Sayfa", "Ana Sayfa", "Home"),
        ("nav_Urunler", "Ürünler", "Products"),
        ("nav_Kurumsal", "Kurumsal", "Corporate"),
        ("nav_Projeler", "Projeler", "Projects"),
        ("nav_Referanslar", "Referanslar", "References"),
        ("nav_SSS", "SSS", "FAQ"),
        ("nav_İletişim", "İletişim", "Contact"),
        ("nav_Hakkimizda", "Hakkımızda", "About Us"),
        ("nav_Vizyon & Misyon", "Vizyon & Misyon", "Vision & Mission"),
        ("nav_Ekibimiz", "Ekibimiz", "Our Team"),
        ("nav_Sertifikalarimiz", "Sertifikalarımız", "Certificates"),
        ("kapak_sistemleri", "Kapak Sistemleri", "Panel Systems"),
        ("kapi_modelleri", "Kapı Modelleri", "Door Models"),
        ("hakkimizda", "Hakkımızda", "About Us"),
        ("vizyon_misyon", "Vizyon & Misyon", "Vision & Mission"),
        // Urun / Kapi alanlari
        ("kapi.modelAdi", "Model Adı", "Model Name"),
        ("kapi.modelKodu", "Model Kodu", "Model Code"),
        ("kapi.kapakModeli", "Kapak Modeli", "Panel Model"),
        ("kapi.kapiModeli", "Kapı Modeli", "Door Model"),
        ("kapi.kategori", "Kategori", "Category"),
        ("kapi.fiyatBirim", "Fiyat (TL/m2)", "Price (TL/m2)"),
        ("kapi.onYazi", "On Yazi", "Summary"),
        ("urun.adi", "Urun Adi", "Product Name"),
        ("urun.kodu", "Urun Kodu", "Product Code"),
        ("urun.slug", "Slug", "Slug"),
        ("urun.ailesi", "Urun Ailesi", "Product Family"),
        ("urun.kategori", "Kategori", "Category"),
        ("urun.kisaAciklama", "Kisa Aciklama", "Short Description"),
        ("urun.fiyat", "Fiyat", "Price"),
        ("urun.birim", "Birim", "Unit"),
        ("urun.detayliAciklama", "Detayli Aciklama", "Detailed Description"),
        ("urun.seoBaslik", "SEO Baslik", "SEO Title"),
        ("urun.seoAciklama", "SEO Aciklama", "SEO Description"),
        ("urun.oneCikan", "One Cikan", "Featured"),
        ("urun.yeni", "Yeni", "New"),
        ("urun.yayinda", "Yayinda", "Published"),
        ("urun.bulunamadi", "Urun bulunamadi", "Product not found"),
        ("urun.urunlereDon", "Urunlere Don", "Back to Products"),
        ("urun.teklifIste", "Teklif Iste", "Request Quote"),
        ("urun.seciliRenk", "Secili Renk", "Selected Color"),
        ("urun.kameraSifirla", "Sıfırla", "Reset"),
        ("urun.durdur", "Durdur", "Stop"),
        ("urun.dondur", "Döndür", "Rotate"),
        ("admin.toplamUrun", "Ürün", "Products"),
        ("admin.toplamKapak", "Kapak", "Panels"),
        ("admin.toplam3DModel", "3D Model", "3D Models"),
        ("admin.toplamParca", "Parça", "Parts"),
        ("admin.toplamBlog", "Blog", "Blog"),
        ("admin.toplamHaber", "Haber", "News"),
        ("admin.toplamSayfa", "Sayfa", "Pages"),
        ("admin.toplamMedya", "Medya", "Media"),
        ("admin.toplamKatalog", "Katalog", "Catalog"),
        ("admin.toplamProje", "Proje", "Projects"),
        ("admin.toplamReferansKisa", "Ref", "Ref"),
        ("admin.toplamCeviri", "Çeviri", "Translations"),
        ("admin.toplamDil", "Dil", "Languages"),
        ("admin.bugunMesaj", "Bugün Mesaj", "Today"),
        ("admin.toplamZiyaret", "Ziyaret", "Visits"),
        // Admin sayfa ortak etiketleri
        ("admin.yeniEkle", "Yeni Ekle", "Add New"),
        ("admin.ara", "Ara...", "Search..."),
        ("admin.kayitSayisi", "{0} kayıt", "{0} records"),
        ("admin.renkOnizleme", "Renk Önizleme", "Color Preview"),
        ("admin.katman", "Katman", "Layer"),
        ("admin.grup", "Grup", "Group"),
        ("admin.yuzeyTipi", "Yüzey Tipi", "Surface Type"),
        ("admin.ustKategori", "Üst Kategori", "Parent Category"),
        ("admin.modelSeciniz", "Model Seçiniz", "Select Model"),
        ("admin.kapakTipi", "Kapak Tipi", "Panel Type"),
        ("admin.filtrele", "Filtrele", "Filter"),
        ("admin.onay", "Onay", "Approve"),
        ("admin.reddet", "Reddet", "Reject"),
        ("admin.secili", "seçili", "selected"),
        ("admin.kaydediliyor", "Kaydediliyor...", "Saving..."),
        ("admin.siliniyor", "Siliniyor...", "Deleting..."),
        ("admin.urunAdi", "Ürün Adı", "Product Name"),
        ("admin.urunKodu", "Ürün Kodu", "Product Code"),
        ("admin.foto", "Fotoğraf", "Photo"),
        ("admin.pdf", "PDF", "PDF"),
        ("admin.video", "Video", "Video"),
        ("admin.belge", "Belge", "Document"),
        ("admin.indir", "İndir", "Download"),
        ("admin.siteyeDon", "Siteye Dön", "Back to Site"),
        ("admin.cikisYap", "Çıkış Yap", "Logout"),
        ("admin.profil", "Profil", "Profile"),
        ("admin.superAdmin", "SuperAdmin", "SuperAdmin"),
        ("admin.ariyor", "Aranıyor...", "Searching..."),
        ("admin.kayitli", "Kayıtlı", "Saved"),
        ("admin.kayitliDegil", "Kayıtlı değil", "Not saved"),
        ("admin.temelBilgileriOnceKaydet", "Önce temel bilgileri kaydedin.", "Save basic info first."),
        ("admin.gorseller", "Görseller", "Images"),
        ("admin.3dModel", "3D Model", "3D Model"),
        ("admin.ozet", "Özet", "Summary"),
        ("admin.kaydetVeDevam", "Kaydet ve Devam", "Save & Continue"),
        ("admin.mevcut", "Mevcut", "Existing"),
        ("admin.3d.yeniModel", "Yeni Model", "New Model"),
        ("admin.3d.yeniParca", "Yeni Parça", "New Part"),
        ("admin.3d.modelSeciniz", "Model Seçiniz", "Select Model"),
        ("admin.3d.parcaSeciniz", "Parça Seçiniz", "Select Part"),
        ("admin.3d.modelAdi", "Model Adı", "Model Name"),
        ("admin.3d.modelDosyasi", "Model Dosyası", "Model File"),
        ("admin.3d.baslangicKonumu", "Başlangıç Konumu", "Start Position"),
        ("admin.3d.kameraAyar", "Kamera Ayarı", "Camera Setting"),
        ("admin.3d.isikAyar", "Işık Ayarı", "Light Setting"),
        ("admin.3d.sahne", "Sahne", "Scene"),
        ("admin.ral.renkKodu", "RAL Kodu", "RAL Code"),
        ("admin.ral.renkAdi", "Renk Adı", "Color Name"),
        ("admin.ral.hexKodu", "HEX Kodu", "HEX Code"),
        ("admin.ral.yeniRenk", "Yeni Renk", "New Color"),
        ("admin.malzeme.yeniMalzeme", "Yeni Malzeme", "New Material"),
        ("admin.malzeme.malzemeTuru", "Malzeme Türü", "Material Type"),
        ("admin.malzeme.hammadde", "Hammadde", "Raw Material"),
        ("admin.malzeme.kaplamaTipi", "Kaplama Tipi", "Coating Type"),
        ("admin.kaplama.yeniKaplama", "Yeni Kaplama", "New Coating"),
        ("admin.kaplama.yuzeyTipi", "Yüzey Tipi", "Surface Type"),
        ("admin.konfigurasyon.sablonAdi", "Şablon Adı", "Template Name"),
        ("admin.konfigurasyon.sablonTuru", "Şablon Türü", "Template Type"),
        ("admin.konfigurasyon.kuralTuru", "Kural Türü", "Rule Type"),
        ("admin.konfigurasyon.kosul", "Koşul", "Condition"),
        ("admin.konfigurasyon.sonuc", "Sonuç", "Result"),
        ("admin.konfigurasyon.yeniKural", "Yeni Kural", "New Rule"),
        ("admin.konfigurasyon.yeniSablon", "Yeni Şablon", "New Template"),
        ("admin.onay", "Onay", "Approve"),
        ("admin.ret", "Ret", "Reject"),
        ("admin.siralama", "Sıralama", "Sort Order"),
        ("admin.goruntulenme", "Görüntülenme", "Views"),
        ("admin.yayinDurumu", "Yayın Durumu", "Published"),
        ("admin.yayinla", "Yayınla", "Publish"),
        ("admin.geriAl", "Geri Al", "Undo"),
        ("admin.testEt", "Test Et", "Test"),
        ("ortak.baslat", "Başlat", "Start"),
        ("ortak.durdur", "Durdur", "Stop"),
        ("ortak.yukle", "Yükle", "Upload"),
        ("ortak.indir", "İndir", "Download"),
        ("ortak.onizle", "Önizle", "Preview"),
        ("ortak.onayla", "Onayla", "Confirm"),
        ("ortak.varsayilan", "Varsayılan", "Default"),
        // Footer linkleri
        ("footer_Urunler", "Ürünler", "Products"),
        ("footer_Projeler", "Projeler", "Projects"),
        ("footer_Hakkimizda", "Hakkımızda", "About Us"),
        ("footer_İletişim", "İletişim", "Contact"),
        ("footer_Blog", "Haber", "News"),
        ("footer_SSS", "SSS", "FAQ"),
        ("footer_Gizlilik Politikasi", "Gizlilik Politikası", "Privacy Policy"),
        // Footer kategorileri
        ("footer_Membran Kapak", "Membran Kapak", "Membrane Panel"),
        ("footer_Lake Kapak", "Lake Kapak", "Lacquer Panel"),
        ("footer_Akrilik Kapak", "Akrilik Kapak", "Acrylic Panel"),
        ("footer_Kapi Modelleri", "Kapı Modelleri", "Door Models"),
        ("footer_Dusakabin", "Duşakabin", "Shower Cabin"),
        ("footer_Banyo Dolabi", "Banyo Dolabı", "Bathroom Cabinet"),
        // İletişim formu
        ("iletisim.hero.etiket", "BİZE ULAŞIN", "CONTACT US"),
        ("iletisim.hero.baslik", "İletişim", "Contact"),
        ("iletisim.hero.aciklama", "Ürün bilgisi, fiyat teklifi veya teknik destek için bize yazın. Çalışma saatleri içinde en kısa sürede geri döneceğiz.", "Write to us for product information, price quotes or technical support. We will get back to you as soon as possible during working hours."),
        ("iletisim.adres", "ADRES", "ADDRESS"),
        ("iletisim.telefon", "TELEFON", "PHONE"),
        ("iletisim.eposta", "E-POSTA", "EMAIL"),
        ("iletisim.calismaSaatleri", "ÇALIŞMA SAATLERİ", "WORKING HOURS"),
        ("iletisim.form.baslik", "MESAJ GÖNDER", "SEND MESSAGE"),
        ("iletisim.form.yardim", "Size Nasıl Yardımcı Olabiliriz?", "How Can We Help You?"),
        ("iletisim.form.basariliMesaj", "Mesajınız başarıyla alındı. En kısa sürede size döneceğiz.", "Your message has been received. We will get back to you as soon as possible."),
        ("iletisim.form.yeniMesaj", "Yeni Mesaj Gönder", "Send New Message"),
        ("iletisim.form.modelIcinTeklif", "modeli için teklif talebi", "quote request for the model"),
        ("iletisim.form.adSoyad", "Ad Soyad", "Full Name"),
        ("iletisim.form.telefon", "Telefon", "Phone"),
        ("iletisim.form.eposta", "E-posta Adresi", "Email Address"),
        ("iletisim.form.konu", "Konu", "Subject"),
        ("iletisim.form.mesaj", "Mesajınız", "Your Message"),
        ("iletisim.form.gonder", "MESAJ GÖNDER", "SEND MESSAGE"),
        ("iletisim.konum", "KONUM", "LOCATION"),
        ("iletisim.ziyaret", "Bizi Ziyaret Edin", "Visit Us"),
        // Genel
        ("genel.hakkimizda", "Hakkımızda", "About Us"),
        ("genel.anasayfa", "Ana Sayfa", "Home"),
        // Ana sayfa hero/slider
        ("tumunu_gor", "TÜMÜNÜ GÖR", "VIEW ALL"),
        ("koleksiyonu_goster", "KOLEKSİYONU GÖSTER", "SHOW COLLECTION"),
        ("modelleri_goster", "MODELLERİ GÖSTER", "SHOW MODELS"),
        ("one_cikan_koleksiyon", "ÖNE ÇIKAN KOLEKSİYON", "FEATURED COLLECTION"),
        ("one_cikan_kapaklar", "Mimari Seçimler", "Architectural Choices"),
        ("ana_sayfa_title", "Gold Banyo | Banyo Mobilyalari ve Koleksiyonlari", "Gold Banyo | Bathroom Furniture and Collections"),
        ("anasayfa.video.baslik", "Üretimdeki Hassasiyetimiz", "Precision in Manufacturing"),
        ("anasayfa.surec.etiket", "vizitlink3d & Norden mobilya", "vizitlink3d & Norden furniture"),
        ("anasayfa.surec.baslik", "4 Adimda Ozel Proje Calismasi", "Custom Project Work in 4 Steps"),
        ("anasayfa.surec.aciklama", "Mutlakinizin duzeni bir duzenden digerine degisir. Mutlakin sekli dolaplar, tezgahlar ve aksesuarlar icin alani belirler.", "The layout of your kitchen varies from one to another. The shape of your kitchen determines the space for cabinets, countertops and accessories."),
        ("anasayfa.katalog.etiket", "DOKÜMANTASYON", "DOCUMENTATION"),
        ("anasayfa.katalog.baslik", "2024 Tasarım Kataloğumuz Hazır", "2024 Design Catalog is Ready"),
        ("anasayfa.katalog.aciklama", "Tüm modellerimizi içeren kataloğumuzu indirin.", "Download our catalog featuring all our models."),
        ("anasayfa.katalog.buton", "KATALOĞU İNDİR (PDF)", "DOWNLOAD CATALOG (PDF)"),
        ("uygulama_gorsel_yok", "Bu model için henüz uygulama görseli yüklenmemiştir.", "No application images uploaded for this model yet."),
        // Slider
        ("slider.yukleme_hatasi", "Slayt yüklenemedi.", "Slider could not be loaded."),
        // Hizmet Süreci
        ("hizmet_sureci.etiket", "ÇALIŞMA SÜRECİ", "WORK PROCESS"),
        ("hizmet_sureci.baslik", "Nasıl Çalışıyoruz?", "How We Work?"),
        // Müşteri Yorumları
        ("musteri_yorumlari.etiket", "MÜŞTERİ YORUMLARI", "CUSTOMER REVIEWS"),
        ("musteri_yorumlari.baslik", "Bizi Tercih Edenler", "Those Who Chose Us"),
        // Referanslar
        ("referanslar.etiket", "25 YILDIR HİZMETİNİZDEYİZ", "25 YEARS AT YOUR SERVICE"),
        ("referanslar.baslik", "Referanslarımız", "Our References"),
        // SSS
        ("sss.etiket", "SIKÇA SORULANLAR", "FREQUENTLY ASKED"),
        ("sss.baslik", "Merak Ettikleriniz", "What You're Wondering"),
        // Genel butonlar
        ("kesfet", "Keşfet", "Discover"),
        ("incele", "İncele", "Explore"),
        ("sayfa.bulunamadi", "Sayfa bulunamadı", "Page not found"),
        ("kvkk", "KVKK", "PDPL"),
        // Renk seçici
        ("renk.sonuc_bulunamadi", "Sonuç bulunamadı.", "No results found."),
        ("renk.secenek_sayisi", "{0} renk seçeneği mevcut", "{0} color options available"),
    };

    private static async Task TohumlaFirmaAsync(VizitLink3DDbContext vt)
    {
        // Firma 1: Goldbanyo
        vt.Firmalar.Add(new Firma
        {
            Ad = "Gold Ban-yom", Unvan = "Gold Ban-yom Banyo Mobilyası", Slug = "goldbanyo",
            AciklamaKisa = "Banyonuza sanat katan banyo mobilyası modelleri",
            Aciklama = "Türkiye'nin lider banyo mobilyası üreticisi. 35+ ülkede hizmet veren, 600+ satış noktasına sahip kurumsal marka.",
            Domain = "goldbanyom.com.tr", YedekDomain = "www.goldbanyom.com.tr",
            Logo = "/img/goldbanyo-logo.png",
            Favicon = "/favicon.png",
            Eposta = "info@goldbanyom.com.tr",
            Telefon1 = "+90 312 847 55 22", Telefon2 = "+90 312 847 55 99",
            Adres = "Çankırı Yolu 8. km Büğdüz Mah. 24. Sok. No: 4 Akyurt / Ankara",
            Sehir = "Ankara", Ilce = "Akyurt", Ulke = "Turkiye", KurulusYili = 1989,
            CalismaSaatleri = "Pzt-Cmt 09:00-18:00",
            Instagram = "https://www.instagram.com/gold.banyom/", Facebook = "https://www.facebook.com/gold.banyo",
            LinkedIn = "https://tr.linkedin.com/company/goldbanyo",
            AdminTema = "endustri-karanlik", SiteTema = "gold",
            AktifMi = true, OlusturulmaTarihi = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Firma 2: Gold Banyo Demo
        vt.Firmalar.Add(new Firma
        {
            Ad = "Gold Banyo Demo", Unvan = "Gold Banyo Demo", Slug = "goldbanyo-demo",
            AciklamaKisa = "Gold Banyo koleksiyonlari icin demo firma kaydi",
            Aciklama = "Gold Banyo demo kaydi, coklu firma akislarini test etmek icin kullanilir.",
            Domain = "demo.goldbanyom.local", YedekDomain = "www.demo.goldbanyom.local",
            Eposta = "demo@goldbanyom.local",
            Telefon1 = "+90 312 847 55 22", Telefon2 = "+90 312 847 55 99",
            Adres = "Çankırı Yolu 8. km Büğdüz Mah. 24. Sok. No: 4 Akyurt / Ankara",
            Sehir = "Ankara", Ilce = "Akyurt", Ulke = "Turkiye", KurulusYili = 2020,
            CalismaSaatleri = "Pzt-Cmt 09:00-18:00",
            Instagram = "https://www.instagram.com/gold.banyom/", Facebook = "https://www.facebook.com/gold.banyo",
            AdminTema = "endustri-karanlik", SiteTema = "endustri-karanlik",
            AktifMi = true, OlusturulmaTarihi = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Eski VizitLink3D firma (opsiyonel - demo olarak tutabiliriz)
        vt.Firmalar.Add(new Firma
        {
            Ad = "VizitLink3D", Unvan = "VizitLink3D Mobilya Kapak ve Kapi Sistemleri", Slug = "vizitlink3d",
            AciklamaKisa = "25 yillik tecrubeyle mobilya kapak ve kapi sistemleri.", Domain = "3dvizitlink.com.tr",
            YedekDomain = "www.3dvizitlink.com.tr", Eposta = "info@3dvizitlink.com.tr",
            Telefon1 = "+90 224 482 24 00", Telefon2 = "+90 533 597 32 14",
            Adres = "Cali Mah. Omer Biltekin Bulv. No:3/1A Nilufer / BURSA", Sehir = "Bursa", Ilce = "Nilufer",
            Ulke = "Turkiye", KurulusYili = 1992, CalismaSaatleri = "09:00 - 18:00",
            Instagram = "https://instagram.com/3dvizitlink.com.tr", Facebook = "https://facebook.com/vizitlink3d",
            AdminTema = "endustri-karanlik", SiteTema = "endustri-karanlik",
            AktifMi = true, OlusturulmaTarihi = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaLisansAsync(VizitLink3DDbContext vt)
    {
        var firma = await vt.Firmalar.FirstOrDefaultAsync(f => f.Slug == "goldbanyo");
        if (firma is null)
        {
            return;
        }

        var baslangic = DateTime.UtcNow.Date;

        vt.Lisanslar.Add(new Lisans
        {
            FirmaId = firma.Id,
            BirincilDomain = "www.goldbanyo.uzunnetreklam.com",
            YedekDomain = "goldbanyo.uzunnetreklam.com",
            BaslangicTarihi = baslangic,
            BitisTarihi = baslangic.AddYears(1),
            LisansTipi = LisansServisi.Yillik,
            SuresizMi = false,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });

        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaProjeKategorileriAsync(VizitLink3DDbContext vt)
    {
        vt.ProjeKategorileri.AddRange(
            new ProjeKategorisi { Ad = "Mutfak", Slug = "mutfak", SiraNo = 1 },
            new ProjeKategorisi { Ad = "Banyo", Slug = "banyo", SiraNo = 2 },
            new ProjeKategorisi { Ad = "Yatak Odasi", Slug = "yatak-odasi", SiraNo = 3 },
            new ProjeKategorisi { Ad = "Ofis", Slug = "ofis", SiraNo = 4 }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaReferanslariAsync(VizitLink3DDbContext vt)
    {
        var referanslar = new List<Referans>
        {
            new Referans { Ad = "SERTEPE İNŞAAT", Aciklama = "45 DAİRE", Tip = "Müşteri", SiraNo = 1 },
            new Referans { Ad = "ALPİŞ İNŞAAT", Aciklama = "120 DAİRE", Tip = "Müşteri", SiraNo = 2 },
            new Referans { Ad = "YG GÖKTAŞ İNŞ.", Aciklama = "96 DAİRE", Tip = "Müşteri", SiraNo = 3 },
            new Referans { Ad = "KUMOVA İNŞAAT", Aciklama = "196 DAİRE", Tip = "Müşteri", SiraNo = 4 },
            new Referans { Ad = "CELAL İNŞAAT", Aciklama = "40 DAİRE", Tip = "Müşteri", SiraNo = 5 },
            new Referans { Ad = "ULU ÇINAR", Aciklama = "16 VİLLA KOMPLE", Tip = "Müşteri", SiraNo = 6 },
            new Referans { Ad = "SADRİOĞULLARI İNŞ.", Aciklama = "200 DAİRE", Tip = "Müşteri", SiraNo = 7 },
            new Referans { Ad = "BEZEK MİMARLIK", Aciklama = "150 DAİRE", Tip = "Müşteri", SiraNo = 8 },
            new Referans { Ad = "FAHRETTİN DENGİZ İNŞ.", Aciklama = "60 DAİRE", Tip = "Müşteri", SiraNo = 9 },
            new Referans { Ad = "OLCAY ANIK İNŞAAT", Aciklama = "50 DAİRE", Tip = "Müşteri", SiraNo = 10 },
            new Referans { Ad = "SÜLEYMAN GARİP İNŞAAT", Aciklama = "30 DAİRE", Tip = "Müşteri", SiraNo = 11 },
            new Referans { Ad = "DİRLİK İNŞAAT", Aciklama = "35 DAİRE", Tip = "Müşteri", SiraNo = 12 },
            new Referans { Ad = "CEM İNŞAAT", Aciklama = "40 DAİRE", Tip = "Müşteri", SiraNo = 13 },
            new Referans { Ad = "KUDU İNŞAAT", Aciklama = "70 DAİRE", Tip = "Müşteri", SiraNo = 14 },
            new Referans { Ad = "SADİ ALAGÖZ İNŞAAT", Aciklama = "50 DAİRE", Tip = "Müşteri", SiraNo = 15 },
            new Referans { Ad = "KLAS İNŞAAT", Aciklama = "30 DAİRE", Tip = "Müşteri", SiraNo = 16 },
            new Referans { Ad = "YASİN TEKİN İNŞAAT", Aciklama = "40 DAİRE", Tip = "Müşteri", SiraNo = 17 },
            new Referans { Ad = "ŞURA İNŞAAT", Aciklama = "70 DAİRE", Tip = "Müşteri", SiraNo = 18 },
            new Referans { Ad = "AKAR İNŞAAT", Aciklama = "60 DAİRE", Tip = "Müşteri", SiraNo = 19 },
            new Referans { Ad = "EDT TEKSTİL", Aciklama = "30 DAİRE", Tip = "Müşteri", SiraNo = 20 },
            new Referans { Ad = "ZENGİN İNŞAAT", Aciklama = "60 DAİRE", Tip = "Müşteri", SiraNo = 21 }
        };

        var mevcutlar = await vt.Referanslar.Select(r => r.Ad).ToListAsync();
        var eklenecekler = referanslar.Where(r => !mevcutlar.Contains(r.Ad)).ToList();

        if (eklenecekler.Any())
        {
            var maxSira = mevcutlar.Any() ? await vt.Referanslar.MaxAsync(r => r.SiraNo) : 0;
            foreach (var r in eklenecekler)
            {
                maxSira++;
                r.SiraNo = maxSira;
                vt.Referanslar.Add(r);
            }
            await vt.SaveChangesAsync();
        }
    }

    private static async Task ReferansLogoGuncelleAsync(VizitLink3DDbContext vt)
    {
        // Ad → logo dosyası eşlemesi (yalnızca logo dosyası var olanlar)
        var logoHaritasi = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "SERTEPE İNŞAAT",        "/medya/referanslar/sertepe-insaat.png" },
            { "ALPİŞ İNŞAAT",          "/medya/referanslar/alpis-insaat.png" },
            { "YG GÖKTAŞ İNŞ.",        "/medya/referanslar/yg-goktas.png" },
            { "KUMOVA İNŞAAT",         "/medya/referanslar/kumova-insaat.png" },
            { "CELAL İNŞAAT",          "/medya/referanslar/celal-insaat.png" },
            { "ULU ÇINAR",             "/medya/referanslar/ulu-cinar.png" },
            { "SADRİOĞULLARI İNŞ.",    "/medya/referanslar/sadriogullari-insaat.png" },
            { "BEZEK MİMARLIK",        "/medya/referanslar/bezek-mimarlik.png" },
            { "DİRLİK İNŞAAT",         "/medya/referanslar/dirlik-insaat.png" },
            { "CEM İNŞAAT",            "/medya/referanslar/cem-insaat.png" },
            { "KUDU İNŞAAT",           "/medya/referanslar/kudu-insaat.png" },
            { "KLAS İNŞAAT",           "/medya/referanslar/klas-daire.png" },
            { "YASİN TEKİN İNŞAAT",    "/medya/referanslar/yasin-tekin-insaat.png" },
            { "ŞURA İNŞAAT",           "/medya/referanslar/sura-insaat.png" },
            { "AKAR İNŞAAT",           "/medya/referanslar/akar-insaat.png" },
            { "EDT TEKSTİL",           "/medya/referanslar/edt-tekstil.png" },
            { "ZENGİN İNŞAAT",         "/medya/referanslar/zengin-insaat.png" },
        };

        var referanslar = await vt.Referanslar.Where(r => !r.SilindiMi).ToListAsync();
        var degisti = false;

        foreach (var r in referanslar)
        {
            if (logoHaritasi.TryGetValue(r.Ad, out var logo) && r.Logo != logo)
            {
                r.Logo = logo;
                degisti = true;
            }
            else if (!string.IsNullOrEmpty(r.Logo) && !logoHaritasi.ContainsValue(r.Logo))
            {
                r.Logo = null;
                degisti = true;
            }
        }

        if (degisti) await vt.SaveChangesAsync();
    }

    private static async Task TohumlaMusteriYorumlariniAsync(VizitLink3DDbContext vt)
    {
        vt.MusteriYorumlari.AddRange(
            new MusteriYorumu { MusteriAdi = "Osman A.", MusteriSehir = "Bursa", Yorum = "Kapılarımızı yenilemede profesyonel bir firma seçtik. Tüm süreçler dijital ortamda prova yapıldı, çok profesyonel.", Puan = 5, Onaylandi = true, OneCikan = true, SiraNo = 1 },
            new MusteriYorumu { MusteriAdi = "Merih C.", MusteriSehir = "Istanbul", Yorum = "Mutfak dolaplarim icin tercih ettim, beklentimin cok ustunde is cikardilar.", Puan = 5, Onaylandi = true, OneCikan = true, SiraNo = 2 },
            new MusteriYorumu { MusteriAdi = "Ayse K.", MusteriSehir = "Ankara", Yorum = "Banyo dolaplarimiz 2 yildir sorunsuz kullaniyoruz. Suya dayanikliligi cok iyi.", Puan = 4, Onaylandi = true, SiraNo = 3 },
            new MusteriYorumu { MusteriAdi = "Mehmet D.", MusteriSehir = "Izmir", Yorum = "Ofis icin ozel olcu dolap yaptirdik. Teslimat ve montaj sureci profesyoneldi.", Puan = 5, Onaylandi = true, SiraNo = 4 },
            new MusteriYorumu { MusteriAdi = "Zeynep T.", MusteriSehir = "Bursa", Yorum = "Villa kapilarimiz cok modern ve ses yalitimi mukemmel. Tavsiye ederim.", Puan = 5, Onaylandi = true, OneCikan = true, SiraNo = 5 }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaKapakModelleriAsync(VizitLink3DDbContext vt)
    {
        await vt.SaveChangesAsync();
    }

    /// <summary>
    /// Urun.AnaGorselMedyaId'yi KapakModelleri.AnaGorselUrl'deki medya ID'siyle eslesir.
    /// Listeleme sayfalarinda mutfak sahnesi gorseli gorunsun, panel fotografi sadece detayda.
    /// </summary>
    private static async Task UrunGorselleriniSenkronizeEtAsync(VizitLink3DDbContext vt)
    {
        var kapakModelleri = await vt.KapakModelleri
            .Where(k => !k.SilindiMi && k.AnaGorselUrl != null && k.AnaGorselUrl.StartsWith("/api/medya/dosya/"))
            .Select(k => new { k.ModelKodu, k.Slug, k.AnaGorselUrl })
            .ToListAsync();

        Console.WriteLine($"[SENKRON] KapakModelleri /api/medya: {kapakModelleri.Count}");

        var urunler = await vt.Urunler
            .Where(u => !u.SilindiMi)
            .ToListAsync();

        var degisti = false;
        var eslesenAdet = 0;

        foreach (var urun in urunler)
        {
            var eslesenKapak = kapakModelleri.FirstOrDefault(k =>
                k.Slug == urun.Slug ||
                k.ModelKodu.Replace(" ", "-").ToLowerInvariant() == urun.Slug);

            if (eslesenKapak == null) continue;

            var urlParts = eslesenKapak.AnaGorselUrl.Split('/');
            if (urlParts.Length < 4) continue;
            if (!long.TryParse(urlParts[^1], out var yeniMedyaId)) continue;

            if (urun.AnaGorselMedyaId != yeniMedyaId)
            {
                Console.WriteLine($"[SENKRON] {urun.Slug}: {urun.AnaGorselMedyaId} -> {yeniMedyaId}");
                urun.AnaGorselMedyaId = yeniMedyaId;
                degisti = true;
            }
            eslesenAdet++;
        }

        Console.WriteLine($"[SENKRON] Eslesen: {eslesenAdet}, Degisen: {(degisti ? "var" : "yok")}");
        if (degisti) await vt.SaveChangesAsync();
    }

    private static async Task TohumlaSayfaIcerikleriAsync(VizitLink3DDbContext vt)
    {
        var mevcut = await vt.SayfaIcerikleri
            .Select(s => s.Bolum + "|" + s.Anahtar + "|" + s.Dil)
            .ToListAsync();

        List<SayfaIcerigi> tohumlar = new()
        {
            // ─── ANASAYFA ────────────────────────────────────────────────
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroGorselUrl", Deger = "/medya/vizitlink3d_default.png", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroGorselUrl", Deger = "/medya/vizitlink3d_default.png", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroEtiket", Deger = "GOLD BANYO KOLEKSİYONU", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroEtiket", Deger = "GOLD BANYO COLLECTION", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroBaslik1", Deger = "Banyolarınıza", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroBaslik1", Deger = "For Your Bathrooms", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroBaslik2", Deger = "Altın Dokunuşlar", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroBaslik2", Deger = "Golden Touches", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroAciklama", Deger = "Gold Banyo; premium, trend ve exclusive serileriyle banyolara tasarım, konfor ve uzun ömürlü kalite getirir.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "HeroAciklama", Deger = "Gold Banyo brings design, comfort and long-lasting quality to bathrooms with its premium, trend and exclusive collections.", Dil = "en" },
            // ─── İSTATİSTİKLER ──────────────────────────────────────────
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist1Deger", Deger = "2005'ten beri", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist1Deger", Deger = "Since 2005", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist1Etiket", Deger = "Üretim Disiplini", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist1Etiket", Deger = "Production Discipline", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist2Deger", Deger = "500+", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist2Deger", Deger = "500+", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist2Etiket", Deger = "Ana Koleksiyon", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist2Etiket", Deger = "Main Collections", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist3Deger", Deger = "20+", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist3Deger", Deger = "20+", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist3Etiket", Deger = "İhracat Pazarı", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist3Etiket", Deger = "Export Markets", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist4Deger", Deger = "%100", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist4Deger", Deger = "%100", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist4Etiket", Deger = "Müşteri Memnuniyeti", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Ist4Etiket", Deger = "Customer Satisfaction", Dil = "en" },
            // ─── KATEGORİ 1 (Kapak Sistemleri) ─────────────────────────
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat1Gorsel", Deger = "/medya/kapak_kategori.png", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat1Gorsel", Deger = "/medya/kapak_kategori.png", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat1Etiket", Deger = "PREMIUM SERİ", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat1Etiket", Deger = "PREMIUM SERIES", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat1Baslik", Deger = "Estetik banyo mobilyaları", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat1Baslik", Deger = "Aesthetic bathroom furniture", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat1Aciklama", Deger = "Modern çizgiler, güçlü malzeme kalitesi ve kusursuz işçilikle Gold Banyo Premium serisi banyolarınıza seçkin bir karakter kazandırır.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat1Aciklama", Deger = "Gold Banyo Premium adds a distinguished character to bathrooms with modern lines, strong material quality and precise workmanship.", Dil = "en" },
            // ─── KATEGORİ 2 (Kapı) ──────────────────────────────────────
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat2Gorsel", Deger = "/medya/kapi_kategori.png", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat2Gorsel", Deger = "/medya/kapi_kategori.png", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat2Etiket", Deger = "DAYANIKLI YAPI", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat2Etiket", Deger = "DURABLE CONSTRUCTION", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat2Baslik", Deger = "Güncel yaşam stilleri için", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat2Baslik", Deger = "For contemporary lifestyles", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat2Aciklama", Deger = "Trend serimiz; kompakt ölçüler, sıcak yüzeyler ve fonksiyonel çözümlerle çağdaş banyolara dengeli bir şıklık sunar.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Kat2Aciklama", Deger = "Our Trend collection offers balanced elegance for contemporary bathrooms with compact dimensions, warm surfaces and functional solutions.", Dil = "en" },
            // ─── 4 ADIM SÜRECİ ──────────────────────────────────────────
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim1Baslik", Deger = "Keşif ve Ölçü", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim1Baslik", Deger = "Measurement & Survey", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim1Aciklama", Deger = "Uzman ekibimiz evinize gelerek detaylı ölçü alır ve ihtiyaçlarınızı belirler.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim1Aciklama", Deger = "Our expert team visits your home, takes precise measurements, and understands your needs.", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim2Baslik", Deger = "3D Tasarım", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim2Baslik", Deger = "3D Design", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim2Aciklama", Deger = "3D konfigüratörümüzle hayalinizdeki tasarımı birlikte oluştururuz.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim2Aciklama", Deger = "We create your dream design together using our 3D configurator.", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim3Baslik", Deger = "Üretim", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim3Baslik", Deger = "Manufacturing", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim3Aciklama", Deger = "CNC ve otomatik pres hatlarımızda siparişiniz titizlikle üretilir.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim3Aciklama", Deger = "Your order is produced with precision on our CNC and automatic press lines.", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim4Baslik", Deger = "Montaj", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim4Baslik", Deger = "Installation", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim4Aciklama", Deger = "Profesyonel montaj ekibimiz ürünlerinizi yerinde kurar ve teslim eder.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "Adim4Aciklama", Deger = "Our professional installation team installs and delivers your products on-site.", Dil = "en" },
            // ─── CTA BANNER ─────────────────────────────────────────────
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "CtaBaslik", Deger = "Gold Banyo ile projenizi birlikte şekillendirelim", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "CtaBaslik", Deger = "Let us shape your project with Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "CtaAltBaslik", Deger = "Showroom, konut ve özel proje ihtiyaçlarınız için koleksiyonlarımızı birlikte planlayalım.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "CtaAltBaslik", Deger = "Let us plan your showroom, residential and bespoke project needs together.", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "CtaButonYazi", Deger = "İletişime Geç", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "CtaButonYazi", Deger = "Contact Us", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "CtaButonLink", Deger = "iletisim", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "CtaButonLink", Deger = "iletisim", Dil = "en" },
            // ─── VİDEO ─────────────────────────────────────────────
            // Firma tanitim videosu admin/anasayfa-yonetimi > Medya sekmesinden eklenir.
            // Onceki placeholder (Rickroll) kaldirildi; bos kalinca video bolumu gizlenir.
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "YoutubeUrl", Deger = "", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "YoutubeUrl", Deger = "", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "VideoUrl", Deger = "", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "VideoUrl", Deger = "", Dil = "en" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "VideoMute", Deger = "true", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "VideoMute", Deger = "true", Dil = "en" },
            // ─── KATALOG ──────────────────────────────────────────
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "PdfKatalogUrl", Deger = "medya/gold-katalog/GOLD-2026-KATALOG.pdf", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "PdfKatalogUrl", Deger = "medya/gold-katalog/GOLD-2026-KATALOG.pdf", Dil = "en" },
            // ─── MİMARİ SEÇİMLER ÜRÜN ADET ────────────────────────
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "OneCikanAdet", Deger = "4", Dil = "tr" },
            new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "OneCikanAdet", Deger = "4", Dil = "en" },
            // ─── HAKKIMIZDA ───────────────────────────────────────
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "SayfaBasligi", Deger = "Hakkımızda | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "SayfaBasligi", Deger = "About Us | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "SayfaIcerigi", Deger = "<h2>Hakkımızda</h2><p>Gold Banyo, Türkiye'nin en büyük banyo mobilyası üreticisidir. 10.000 m² modern üretim tesisinde, uluslararası standartlara uygun olarak tasarlanan ürünleri 35'den fazla ülkeye ve Türkiye'de 600+ satış noktasına sunmaktadır.</p><p>Gold Exclusive, Gold Premium, Gold Trend ve Gold Standart olmak üzere 4 ana koleksiyonumuz her bütçe ve tasarım tercihi için çözüm sunar.</p><p>Premium banyo mobilyalarında köklü üretim deneyimini modern koleksiyonlarla buluşturuyoruz.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "SayfaIcerigi", Deger = "<h2>About Us</h2><p>Gold Banyo is Turkey's largest bathroom furniture manufacturer. In our 10,000 m² modern production facility, we deliver products designed to international standards to over 35 countries and 600+ sales points throughout Turkey.</p><p>Our 4 main collections—Gold Exclusive, Gold Premium, Gold Trend and Gold Standart—provide solutions for every budget and design preference.</p><p>We combine deep production experience in premium bathroom furniture with modern collections.</p>", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "KurumsalEtiket", Deger = "Gold Banyo Hikayesi", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "KurumsalEtiket", Deger = "The Gold Banyo Story", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HeroBaslik", Deger = "Lüks banyolara altın dokunuşlar", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HeroBaslik", Deger = "Golden touches for luxury bathrooms", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HeroAciklama", Deger = "2005'ten bu yana üretim disiplinimizi estetik, dayanıklılık ve tasarım detaylarıyla birleştiriyoruz.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HeroAciklama", Deger = "Since 2005, we have been combining production discipline with aesthetics, durability and design detail.", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HikayeBasligi", Deger = "Bursa'dan başlayan tasarım ve üretim yolculuğumuz", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HikayeBasligi", Deger = "Our design and production journey that started in Bursa", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HikayeMetni", Deger = "Gold Banyo, Bursa Nilüfer'deki üretim altyapısını güçlü malzeme bilgisi ve çağdaş tasarım yaklaşımıyla bir araya getirerek banyo mobilyasında seçkin çözümler geliştirmek için kuruldu.\n\nPremium, Trend, Exclusive ve özel proje koleksiyonlarımızda akrilik, lake, membran, UV lak ve farklı yüzey alternatiflerini bir araya getiriyor; showroom, bayi ve proje kanallarına sürdürülebilir kalite standardı sunuyoruz.\n\nBugün üretim gücümüzü yalnızca ürün çıkarmak için değil, mimarların, uygulamacıların ve son kullanıcıların ihtiyacına göre ölçülü, estetik ve uzun ömürlü yaşam alanları kurmak için kullanıyoruz.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HikayeMetni", Deger = "Gold Banyo was founded to create distinctive bathroom furniture solutions by combining its production infrastructure in Bursa Nilufer with strong material expertise and a contemporary design approach.\n\nAcross our Premium, Trend, Exclusive and bespoke project collections, we bring together acrylic, lacquer, membrane, UV lacquer and other surface alternatives while delivering a sustainable quality standard for showrooms, dealers and projects.\n\nToday we use our production strength not only to manufacture products, but to build measured, aesthetic and long-lasting living spaces for architects, applicators and end users.", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "Misyon", Deger = "Banyo mobilyasında tasarım kalitesini üretim disipliniyle birleştirerek müşterilerimize uzun ömürlü, şık ve güvenilir çözümler sunmak.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "Misyon", Deger = "To combine design quality with production discipline in bathroom furniture and offer our customers durable, elegant and reliable solutions.", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "Vizyon", Deger = "Türkiye'de ve ihracat pazarlarında Gold Banyo imzasını; özgün tasarım, güçlü servis ve sürdürülebilir kalite ile anılan bir referans marka haline getirmek.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "Vizyon", Deger = "To make the Gold Banyo signature a reference brand in Turkey and export markets, known for original design, strong service and sustainable quality.", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "YilTecrube", Deger = "20+", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "YilTecrube", Deger = "20+", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TamamlananProje", Deger = "5.000+", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TamamlananProje", Deger = "5,000+", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "BayiSayisi", Deger = "120+", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "BayiSayisi", Deger = "120+", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "PersonelSayisi", Deger = "80+", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "PersonelSayisi", Deger = "80+", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HakkimizdaGorselUrl", Deger = "/medya/goldbanyo/hakkimizda/fabrika.jpg", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "HakkimizdaGorselUrl", Deger = "/medya/goldbanyo/hakkimizda/fabrika.jpg", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "KatalogButonYazi", Deger = "Kurumsal kataloğu incele", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "KatalogButonYazi", Deger = "Browse the corporate catalogue", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceYil1", Deger = "2005", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceYil1", Deger = "2005", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceBaslik1", Deger = "Marka kuruluşu", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceBaslik1", Deger = "Brand foundation", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceAciklama1", Deger = "Gold Banyo, banyo mobilyasında butik kalite anlayışıyla üretim yolculuğuna başladı.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceAciklama1", Deger = "Gold Banyo started its production journey with a boutique quality mindset in bathroom furniture.", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceYil2", Deger = "2012", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceYil2", Deger = "2012", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceBaslik2", Deger = "Koleksiyon derinliği", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceBaslik2", Deger = "Collection depth", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceAciklama2", Deger = "Farklı yüzey, renk ve seri kurgularıyla proje ve showroom kanalı için ürün gamı genişletildi.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceAciklama2", Deger = "The product range expanded for project and showroom channels with different surfaces, colours and series configurations.", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceYil3", Deger = "2018", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceYil3", Deger = "2018", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceBaslik3", Deger = "İhracat ve bayi ağı", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceBaslik3", Deger = "Export and dealer network", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceAciklama3", Deger = "Yurt içi satış kanallarına ek olarak ihracat ve kurumsal bayi yapılanması güçlendirildi.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceAciklama3", Deger = "In addition to domestic sales channels, export and corporate dealer structures were strengthened.", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceYil4", Deger = "2024", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceYil4", Deger = "2024", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceBaslik4", Deger = "Dijital dönüşüm", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceBaslik4", Deger = "Digital transformation", Dil = "en" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceAciklama4", Deger = "Katalog, medya ve yönetim akışları daha güçlü bir dijital altyapıyla yeniden yapılandırıldı.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "hakkimizda", Anahtar = "TarihceAciklama4", Deger = "Catalogue, media and management flows were restructured on a stronger digital foundation.", Dil = "en" },
            // ─── İLETİŞİM ──────────────────────────────────────────
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SayfaBasligi", Deger = "İletişim | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SayfaBasligi", Deger = "Contact | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SayfaIcerigi", Deger = "<h2>İletişim</h2><p>Gold Banyo ürünleri, bayilik talepleri ve proje iş birlikleri için bizimle iletişime geçin.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SayfaIcerigi", Deger = "<h2>Contact</h2><p>Get in touch with us for Gold Banyo products, dealership requests and project collaborations.</p>", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Adres", Deger = "Çankırı Yolu 8. km Büğdüz Mah. 24. Sok. No: 4 Akyurt ANKARA", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Adres", Deger = "Ankara Cankiri Road 8. km Bugduz District 24. Street No: 4 Akyurt ANKARA", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Telefon1", Deger = "+90 312 847 55 22", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Telefon1", Deger = "+90 312 847 55 22", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Telefon2", Deger = "+90 312 847 55 99", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Telefon2", Deger = "+90 312 847 55 99", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Telefon3", Deger = "+90 312 847 51 42", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Telefon3", Deger = "+90 312 847 51 42", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Showroom", Deger = "Rüzgarlı Ege Sk. Rüzgarlı İş Merkezi No:15/23 ANKARA", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Showroom", Deger = "Ruzgarli Ege St. Ruzgarli Business Center No:15/23 ANKARA", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "ShowroomTelefon1", Deger = "+90 312 309 06 88", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "ShowroomTelefon1", Deger = "+90 312 309 06 88", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "ShowroomTelefon2", Deger = "+90 312 309 06 48", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "ShowroomTelefon2", Deger = "+90 312 309 06 48", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Eposta", Deger = "info@goldbanyom.com.tr", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "Eposta", Deger = "info@goldbanyom.com.tr", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SosyalFacebook", Deger = "gold.banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SosyalFacebook", Deger = "gold.banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SosyalInstagram", Deger = "gold.banyom", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SosyalInstagram", Deger = "gold.banyom", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SosyalLinkedin", Deger = "goldbanyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "SosyalLinkedin", Deger = "goldbanyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "CalismaGunleri", Deger = "Pazartesi - Cuma", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "CalismaGunleri", Deger = "Monday - Friday", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "CalismaSaatleri", Deger = "08:00 - 17:00", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "CalismaSaatleri", Deger = "08:00 - 17:00", Dil = "en" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "HaritaUrl", Deger = "https://www.google.com/maps?q=C%CC%A7ankiri+Yolu+8+km+Ankara&output=embed", Dil = "tr" },
            new SayfaIcerigi { Bolum = "iletisim", Anahtar = "HaritaUrl", Deger = "https://www.google.com/maps?q=Ankara+Turkey&output=embed", Dil = "en" },
            // ─── BLOG ──────────────────────────────────────────
            new SayfaIcerigi { Bolum = "blog", Anahtar = "SayfaBasligi", Deger = "Haber | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "blog", Anahtar = "SayfaBasligi", Deger = "News | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "blog", Anahtar = "SayfaIcerigi", Deger = "<h2>Haber</h2><p>Sektör haberleri, tasarım trendleri ve ipuçları.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "blog", Anahtar = "SayfaIcerigi", Deger = "<h2>News</h2><p>Industry news, design trends and tips.</p>", Dil = "en" },
            // ─── SSS ──────────────────────────────────────────
            new SayfaIcerigi { Bolum = "sss", Anahtar = "SayfaBasligi", Deger = "Sıkça Sorulan Sorular | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "sss", Anahtar = "SayfaBasligi", Deger = "Frequently Asked Questions | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "sss", Anahtar = "SayfaIcerigi", Deger = "<h2>Sıkça Sorulan Sorular</h2><p>Merak ettikleriniz.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "sss", Anahtar = "SayfaIcerigi", Deger = "<h2>Frequently Asked Questions</h2><p>What you're wondering about.</p>", Dil = "en" },
            // ─── VİZYON MİSYON ──────────────────────────────────
            new SayfaIcerigi { Bolum = "vizyon-misyon", Anahtar = "SayfaBasligi", Deger = "Vizyon ve Misyon | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "vizyon-misyon", Anahtar = "SayfaBasligi", Deger = "Vision & Mission | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "vizyon-misyon", Anahtar = "SayfaIcerigi", Deger = "<h2>Vizyon</h2><p>Türkiye'nin ve Avrupa'nın en yenilikçi mobilya kapak ve kapı sistemleri üreticisi olmak. Üretim süreçlerimizde Endüstri 4.0 teknolojilerini tam entegre ederek müşterilerimize en yüksek kaliteyi sunmak.</p><h2>Misyon</h2><p>Müşterilerimizin yaşam alanlarını güzelleştirecek, dayanıklı ve estetik ürünler üretmek. Sürdürülebilir üretim pratikleriyle çevre dostu çözümler sunmak. Çalışanlarımızın gelişimini destekleyerek sektörün en nitelikli kadrosunu oluşturmak.</p><h3>Değerlerimiz</h3><ul><li><strong>Kalite:</strong> Her üründe taviz vermeden en yüksek standartlar</li><li><strong>Yenilikçilik:</strong> Sürekli Ar-Ge ile yeni malzeme ve teknik araştırma</li><li><strong>Müşteri Odaklılık:</strong> Kişiye özel çözümler ve 7/24 destek</li><li><strong>Sürdürülebilirlik:</strong> Çevre dostu üretim ve FSC sertifikalı malzemeler</li><li><strong>Güven:</strong> 32 yıllık sektörel tecrübe ve referanslar</li></ul>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "vizyon-misyon", Anahtar = "SayfaIcerigi", Deger = "<h2>Vision</h2><p>To be Turkey's and Europe's most innovative furniture door and panel systems manufacturer. To offer the highest quality to our customers by fully integrating Industry 4.0 technologies into our production processes.</p><h2>Mission</h2><p>To produce durable and aesthetic products that will beautify our customers' living spaces. To offer environmentally friendly solutions with sustainable production practices.</p>", Dil = "en" },
            // ─── EKİBİMİZ ──────────────────────────────────────
            new SayfaIcerigi { Bolum = "ekibimiz", Anahtar = "SayfaBasligi", Deger = "Ekibimiz | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ekibimiz", Anahtar = "SayfaBasligi", Deger = "Our Team | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "ekibimiz", Anahtar = "SayfaIcerigi", Deger = "<h2>Ekibimiz</h2><p>Gold Banyo ekibi; üretim, tasarım, kalite, satış ve proje yönetimi alanlarında birlikte çalışan deneyimli uzmanlardan oluşur.</p><h3>Departmanlar</h3><ul><li><strong>Üretim:</strong> Banyo mobilyası gövde, kapak ve aksesuar hazırlıkları</li><li><strong>Tasarım:</strong> Koleksiyon geliştirme ve özel proje çizimleri</li><li><strong>Kalite Kontrol:</strong> Yüzey, ölçü ve montaj uyum kontrolleri</li><li><strong>Satış:</strong> Bayi, showroom ve proje kanal yönetimi</li><li><strong>Müşteri İlişkileri:</strong> Teklif, sipariş ve teslimat iletişimi</li></ul>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ekibimiz", Anahtar = "SayfaIcerigi", Deger = "<h2>Our Team</h2><p>The Gold Banyo team brings together experienced specialists across production, design, quality control, sales and project coordination.</p>", Dil = "en" },
            // ─── SERTİFİKALAR ──────────────────────────────────
            new SayfaIcerigi { Bolum = "sertifikalar", Anahtar = "SayfaBasligi", Deger = "Sertifikalarımız | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "sertifikalar", Anahtar = "SayfaBasligi", Deger = "Our Certificates | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "sertifikalar", Anahtar = "SayfaIcerigi", Deger = "<h2>Sertifikalarımız</h2><p>Gold Banyo, üretim ve tedarik süreçlerinde kalite standardını sürdürülebilir biçimde korumayı hedefler. Güncel sertifika ve belge içerikleri admin panelinden yönetilebilir.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "sertifikalar", Anahtar = "SayfaIcerigi", Deger = "<h2>Our Certificates</h2><p>Gold Banyo aims to maintain a consistent quality standard across production and supply processes. Current certificate and document content can be managed from the admin panel.</p>", Dil = "en" },
            // ─── HABER ──────────────────────────────────────────
            new SayfaIcerigi { Bolum = "haber", Anahtar = "SayfaBasligi", Deger = "Haberler | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "haber", Anahtar = "SayfaBasligi", Deger = "News | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "haber", Anahtar = "SayfaIcerigi", Deger = "<h2>Haberler</h2><p>Gold Banyo'dan güncel duyurular, fuar katılımları ve yeni koleksiyon haberleri yakında burada.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "haber", Anahtar = "SayfaIcerigi", Deger = "<h2>News</h2><p>Current announcements, fair participations and new collection news from Gold Banyo will be here soon.</p>", Dil = "en" },
            // ─── BAYİLER ──────────────────────────────────────────
            new SayfaIcerigi { Bolum = "bayiler", Anahtar = "SayfaBasligi", Deger = "Bayilerimiz | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "bayiler", Anahtar = "SayfaBasligi", Deger = "Our Dealers | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "bayiler", Anahtar = "SayfaIcerigi", Deger = "<h2>Bayilerimiz</h2><p>Türkiye ve yurt dışındaki 600'ün üzerindeki satış noktamızın listesi yakında burada.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "bayiler", Anahtar = "SayfaIcerigi", Deger = "<h2>Our Dealers</h2><p>The list of our 600+ sales points across Turkey and abroad will be here soon.</p>", Dil = "en" },
            // ─── FABRİKAMIZ ──────────────────────────────────────
            new SayfaIcerigi { Bolum = "fabrikamiz", Anahtar = "SayfaBasligi", Deger = "Fabrikamız | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "fabrikamiz", Anahtar = "SayfaBasligi", Deger = "Our Factory | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "fabrikamiz", Anahtar = "SayfaIcerigi", Deger = "<h2>Fabrikamız</h2><p>Ankara Akyurt'ta bulunan 20.000 metrekarelik modern üretim tesisimizden fotoğraf ve üretim süreci detayları yakında burada.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "fabrikamiz", Anahtar = "SayfaIcerigi", Deger = "<h2>Our Factory</h2><p>Photos and production process details from our 20,000 square meter modern production facility in Ankara Akyurt will be here soon.</p>", Dil = "en" },
            // ─── KALİTE POLİTİKASI ──────────────────────────────
            new SayfaIcerigi { Bolum = "kalite-politikasi", Anahtar = "SayfaBasligi", Deger = "Kalite Politikası | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "kalite-politikasi", Anahtar = "SayfaBasligi", Deger = "Quality Policy | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "kalite-politikasi", Anahtar = "SayfaIcerigi", Deger = "<h2>Kalite Politikamız</h2><p>Gold Banyo olarak ölçü doğruluğu, yüzey dayanımı, suya karşı direnç ve uzun ömürlü kullanım kriterlerini üretimin her aşamasında takip ederiz.</p><h3>Kalite Yaklaşımımız</h3><ol><li>Malzeme kabulünde yüzey ve renk kontrolü</li><li>Kesim ve üretimde ölçü doğrulama</li><li>Montaj öncesi aksesuar ve gövde uyum kontrolü</li><li>Sevkiyat öncesi final görünüm ve paketleme denetimi</li></ol>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "kalite-politikasi", Anahtar = "SayfaIcerigi", Deger = "<h2>Quality Policy</h2><p>At Gold Banyo, we monitor dimensional accuracy, surface durability, moisture resistance and long-term usability at every stage of production.</p>", Dil = "en" },
            // ─── GİZLİLİK ──────────────────────────────────────
            new SayfaIcerigi { Bolum = "gizlilik", Anahtar = "SayfaBasligi", Deger = "Gizlilik Politikası | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "gizlilik", Anahtar = "SayfaBasligi", Deger = "Privacy Policy | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "gizlilik", Anahtar = "SayfaIcerigi", Deger = "<h2>Gizlilik Politikası</h2><p>Gold Banyo olarak kişisel verilerinizin korunmasına önem veriyoruz. İletişim, teklif ve sipariş süreçlerinde paylaştığınız bilgiler yalnızca hizmet sunumu ve yasal yükümlülüklerin yerine getirilmesi amacıyla işlenir.</p><p>KVKK kapsamındaki talepleriniz için <a href=\"mailto:info@goldbanyom.com.tr\">info@goldbanyom.com.tr</a> adresinden bize ulaşabilirsiniz.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "gizlilik", Anahtar = "SayfaIcerigi", Deger = "<h2>Privacy Policy</h2><p>At Gold Banyo, we care about protecting your personal data. Information shared during contact, quotation and order processes is processed only for service delivery and legal obligations.</p>", Dil = "en" },
            // ─── AKRİLİK ──────────────────────────────────────
            new SayfaIcerigi { Bolum = "akrilik", Anahtar = "SayfaBasligi", Deger = "Akrilik Kapak Sistemleri | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "akrilik", Anahtar = "SayfaBasligi", Deger = "Acrylic Panel Systems | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "akrilik", Anahtar = "SayfaIcerigi", Deger = "<h2>Akrilik Kapak Sistemleri</h2><p>Ultra parlak ayna efektli akrilik kapaklar, modern mutfaklar için ideal çözümdür. Çizilmeye karşı yüksek dayanım ve kolay temizlenebilirlik özelliğiyle öne çıkar.</p><h3>Özellikler</h3><ul><li>Ayna etkili parlak yüzey</li><li>Çizilmeye karşı dirençli</li><li>Leke tutmaz kaplama</li><li>UV korumalı - sararma yapmaz</li><li>12 farklı renk seçeneği</li></ul><h3>Kullanım Alanları</h3><p>Mutfak dolapları, banyo dolapları, vestiyer ve TV üniteleri için ideal. Özellikle modern ve minimalist tasarımlı mekanlarda tercih edilir.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "akrilik", Anahtar = "SayfaIcerigi", Deger = "<h2>Acrylic Panel Systems</h2><p>Ultra-gloss mirror-effect acrylic panels are the ideal solution for modern kitchens. They stand out with high scratch resistance and easy cleanability.</p>", Dil = "en" },
            // ─── LAMİNAT ──────────────────────────────────────
            new SayfaIcerigi { Bolum = "laminat", Anahtar = "SayfaBasligi", Deger = "Laminat Kapak Sistemleri | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "laminat", Anahtar = "SayfaBasligi", Deger = "Laminate Panel Systems | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "laminat", Anahtar = "SayfaIcerigi", Deger = "<h2>Laminat Kapak Sistemleri</h2><p>Yüksek basınçlı laminat (HPL) kapaklar, dayanıklılığı ve geniş desen seçenekleriyle öne çıkar. Ahşap, taş ve beton görünümlü dekoratif laminatlar mevcuttur.</p><h3>Avantajlar</h3><ul><li>Yüksek darbe dayanımı</li><li>Isıya dirençli yüzey (280C'ye kadar)</li><li>Hijyenik ve anti-bakteriyel</li><li>200+ desen seçeneği</li><li>Ekonomik fiyat</li></ul>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "laminat", Anahtar = "SayfaIcerigi", Deger = "<h2>Laminate Panel Systems</h2><p>High Pressure Laminate (HPL) panels stand out with their durability and wide range of pattern options. Decorative laminates with wood, stone and concrete appearances are available.</p>", Dil = "en" },
            // ─── MEMBRAN ──────────────────────────────────────
            new SayfaIcerigi { Bolum = "membran", Anahtar = "SayfaBasligi", Deger = "Membran Kapak Sistemleri | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "membran", Anahtar = "SayfaBasligi", Deger = "Membrane Panel Systems | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "membran", Anahtar = "SayfaIcerigi", Deger = "<h2>Membran Kapak Sistemleri</h2><p>MDF üzerine ısıl presleme teknolojisiyle uygulanan membran kapaklar, geniş renk ve desen seçenekleriyle mutfak ve banyo mobilyalarında en çok tercih edilen kapak türüdür.</p><h3>Avantajlar</h3><ul><li>Sınırsız renk ve desen seçeneği</li><li>3D freze uygulanabilir yüzey</li><li>Neme dayanıklı özel kaplamaları mevcut</li><li>Kolay bakım ve temizlik</li><li>Ekonomik fiyat/performans</li></ul><h3>Model Çeşitleri</h3><p>Düz, çerçeveli, camlı, kasetli, country ve modern olmak üzere 50'den fazla model mevcuttur.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "membran", Anahtar = "SayfaIcerigi", Deger = "<h2>Membrane Panel Systems</h2><p>Membrane panels applied to MDF with heat press technology are the most preferred panel type in kitchen and bathroom furniture with their wide color and pattern options.</p>", Dil = "en" },
            // ─── LAKE ──────────────────────────────────────
            new SayfaIcerigi { Bolum = "lake", Anahtar = "SayfaBasligi", Deger = "Lake Kapak Sistemleri | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "lake", Anahtar = "SayfaBasligi", Deger = "Lacquer Panel Systems | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "lake", Anahtar = "SayfaIcerigi", Deger = "<h2>Lake Kapak Sistemleri</h2><p>Çok katmanlı poliüretan lake boyama teknolojisiyle üretilen lake kapaklar, mat ve parlak yüzey seçenekleriyle lüks mekanların vazgeçilmez tercihi olmuştur.</p><h3>Avantajlar</h3><ul><li>Çizilmeye karşı dirençli özel formül</li><li>Sararmaya karşı UV koruma</li><li>Mat, yarı mat ve parlak seçenekler</li><li>RAL, NCS, Pantone renk uyumu</li><li>Özel efekt: metalik, inci, antik doku</li></ul><h3>Uygulama Alanları</h3><p>Mutfak dolapları, yatak odası dolapları, TV üniteleri ve özel tasarım mobilyalar için idealdir.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "lake", Anahtar = "SayfaIcerigi", Deger = "<h2>Lacquer Panel Systems</h2><p>Lacquer panels produced with multi-layer polyurethane lacquer painting technology have become the indispensable choice of luxury spaces with matte and glossy surface options.</p>", Dil = "en" },
            // ─── KAPAK SİSTEMLERİ ──────────────────────────────
            new SayfaIcerigi { Bolum = "kapak-sistemleri", Anahtar = "SayfaBasligi", Deger = "Kapak Sistemleri | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "kapak-sistemleri", Anahtar = "SayfaBasligi", Deger = "Panel Systems | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "kapak-sistemleri", Anahtar = "SayfaIcerigi", Deger = "<h2>Kapak Sistemleri</h2><p>Gold Banyo; akrilik, lake, membran ve özel yüzey uygulamalarında banyo mobilyasına uygun ölçü, renk ve kullanım çözümleri sunar.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "kapak-sistemleri", Anahtar = "SayfaIcerigi", Deger = "<h2>Panel Systems</h2><p>Gold Banyo offers dimension, colour and usability solutions tailored to bathroom furniture across acrylic, lacquer, membrane and special surface applications.</p>", Dil = "en" },
            new SayfaIcerigi { Bolum = "kapak-sistemleri", Anahtar = "UrunAdet", Deger = "12", Dil = "tr" },
            new SayfaIcerigi { Bolum = "kapak-sistemleri", Anahtar = "UrunAdet", Deger = "12", Dil = "en" },
            // ─── KAPI MODELLERI ─────────────────────────────────────
            new SayfaIcerigi { Bolum = "kapi-modelleri", Anahtar = "UrunAdet", Deger = "12", Dil = "tr" },
            new SayfaIcerigi { Bolum = "kapi-modelleri", Anahtar = "UrunAdet", Deger = "12", Dil = "en" },
            // ─── PROJELER ──────────────────────────────────────
            new SayfaIcerigi { Bolum = "projeler", Anahtar = "SayfaBasligi", Deger = "Projelerimiz | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "projeler", Anahtar = "SayfaBasligi", Deger = "Our Projects | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "projeler", Anahtar = "SayfaIcerigi", Deger = "<h2>Projelerimiz</h2><p>Showroom, toplu konut ve özel uygulama projelerinde Gold Banyo koleksiyonlarının kullanım örneklerini bu alanda paylaşabilirsiniz.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "projeler", Anahtar = "SayfaIcerigi", Deger = "<h2>Our Projects</h2><p>This area can present use cases of Gold Banyo collections across showrooms, housing developments and bespoke applications.</p>", Dil = "en" },
            // ─── REFERANSLAR ──────────────────────────────────
            new SayfaIcerigi { Bolum = "referanslar", Anahtar = "SayfaBasligi", Deger = "Referanslarımız | Gold Banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "referanslar", Anahtar = "SayfaBasligi", Deger = "Our References | Gold Banyo", Dil = "en" },
            new SayfaIcerigi { Bolum = "referanslar", Anahtar = "SayfaIcerigi", Deger = "<h2>Referanslarımız</h2><p>Gold Banyo ile çalışan showroom, bayi ve proje ortaklıklarını bu alanda sergileyebilirsiniz.</p>", Dil = "tr" },
            new SayfaIcerigi { Bolum = "referanslar", Anahtar = "SayfaIcerigi", Deger = "<h2>Our References</h2><p>This area can showcase showroom, dealer and project partners working with Gold Banyo.</p>", Dil = "en" }
        };

        foreach (var tohum in tohumlar)
        {
            var mevcutIcerik = await vt.SayfaIcerikleri
                .FirstOrDefaultAsync(s => s.Bolum == tohum.Bolum && s.Anahtar == tohum.Anahtar && s.Dil == tohum.Dil);
            if (mevcutIcerik == null)
            {
                vt.SayfaIcerikleri.Add(tohum);
            }
            else
            {
                mevcutIcerik.Deger = tohum.Deger;
            }
        }
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaAnaMenulerAsync(VizitLink3DDbContext vt)
    {
        await TohumlaHeaderMenuleriAsync(vt);
        await TohumlaFooterMenuleriAsync(vt);
    }

    private static async Task MenuTohumlariniGarantiEtAsync(VizitLink3DDbContext vt)
    {
        var aktifFirmaIdleri = await vt.Firmalar
            .Where(f => f.AktifMi)
            .Select(f => f.Id)
            .ToListAsync();

        if (!aktifFirmaIdleri.Any())
            return;

        var publicHeaderVarMi = await vt.MenuOgeleri.AnyAsync(m => m.Konum == "PublicHeader" && m.FirmaId != null);
        var publicFooterVarMi = await vt.MenuOgeleri.AnyAsync(m => m.Konum == "PublicFooterHizli" && m.FirmaId != null);
        var adminVarMi = await vt.MenuOgeleri.AnyAsync(m => m.Konum == "AdminSol" && m.FirmaId != null);

        if (publicHeaderVarMi && publicFooterVarMi && adminVarMi)
            return;

        vt.MenuOgeleri.RemoveRange(await vt.MenuOgeleri.ToListAsync());
        await vt.SaveChangesAsync();

        await TohumlaHeaderMenuleriAsync(vt);
        await TohumlaFooterMenuleriAsync(vt);
        await TohumlaAdminMenuleriAsync(vt);
    }

    private static async Task TohumlaHeaderMenuleriAsync(VizitLink3DDbContext vt)
    {
        // Tüm menüleri sil (firma-bağlı yenisini oluşturmak için)
        vt.MenuOgeleri.RemoveRange(await vt.MenuOgeleri.ToListAsync());
        await vt.SaveChangesAsync();

        // Firmalar getir
        var firmalar = await vt.Firmalar.Where(f => f.AktifMi).ToListAsync();

        foreach (var firma in firmalar)
        {
            var goldBanyoMu = string.Equals(firma.Slug, "goldbanyo", StringComparison.OrdinalIgnoreCase)
                || string.Equals(firma.Domain, "goldbanyom.com.tr", StringComparison.OrdinalIgnoreCase)
                || string.Equals(firma.YedekDomain, "www.goldbanyom.com.tr", StringComparison.OrdinalIgnoreCase);

            if (goldBanyoMu)
            {
                // === GOLD BANYO PUBLIC HEADER ===
                vt.MenuOgeleri.AddRange(
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ana Sayfa", Url = "", Sira = 1, Konum = "PublicHeader", Ikon = "Home" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ürünler", Url = "banyo-dolaplari", Sira = 2, Konum = "PublicHeader", Ikon = "Inventory2", AltMenuler = new List<MenuOgesi> {
                        new() { FirmaId = firma.Id, Baslik = "Gold Exclusive", Url = "banyo-dolaplari#exclusive", Sira = 1, Konum = "PublicHeader" },
                        new() { FirmaId = firma.Id, Baslik = "Gold Premium", Url = "banyo-dolaplari#premium", Sira = 2, Konum = "PublicHeader" },
                        new() { FirmaId = firma.Id, Baslik = "Gold Trend", Url = "banyo-dolaplari#trend", Sira = 3, Konum = "PublicHeader" },
                        new() { FirmaId = firma.Id, Baslik = "Gold Standart", Url = "banyo-dolaplari#standart", Sira = 4, Konum = "PublicHeader" },
                        new() { FirmaId = firma.Id, Baslik = "Tüm Ürünler", Url = "banyo-dolaplari", Sira = 5, Konum = "PublicHeader" }
                    }},
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Katalog", Url = "katalog", Sira = 3, Konum = "PublicHeader", Ikon = "PictureAsPdf" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Projeler", Url = "projeler", Sira = 4, Konum = "PublicHeader", Ikon = "Apartment" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Kurumsal", Url = "hakkimizda", Sira = 5, Konum = "PublicHeader", Ikon = "Info", AltMenuler = new List<MenuOgesi> {
                        new() { FirmaId = firma.Id, Baslik = "Hakkımızda", Url = "hakkimizda", Sira = 1, Konum = "PublicHeader" },
                        new() { FirmaId = firma.Id, Baslik = "Vizyon & Misyon", Url = "vizyon-misyon", Sira = 2, Konum = "PublicHeader" },
                        new() { FirmaId = firma.Id, Baslik = "Ekibimiz", Url = "ekibimiz", Sira = 3, Konum = "PublicHeader" },
                        new() { FirmaId = firma.Id, Baslik = "Sertifikalarımız", Url = "sertifikalar", Sira = 4, Konum = "PublicHeader" },
                        new() { FirmaId = firma.Id, Baslik = "Bayiler", Url = "bayiler", Sira = 5, Konum = "PublicHeader" },
                        new() { FirmaId = firma.Id, Baslik = "Fabrikamız", Url = "fabrikamiz", Sira = 6, Konum = "PublicHeader" }
                    }},
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Referanslar", Url = "referanslar", Sira = 6, Konum = "PublicHeader", Ikon = "Star" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Haber", Url = "haber", Sira = 7, Konum = "PublicHeader", Ikon = "Newspaper" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "SSS", Url = "sss", Sira = 8, Konum = "PublicHeader", Ikon = "Help" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "İletişim", Url = "iletisim", Sira = 9, Konum = "PublicHeader", Ikon = "Call" }
                );

                // === GOLD BANYO PUBLIC MOBIL ===
                vt.MenuOgeleri.AddRange(
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ana Sayfa", Url = "", Sira = 1, Konum = "PublicMobil", Ikon = "Home" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ürünler", Url = "banyo-dolaplari", Sira = 2, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Katalog", Url = "katalog", Sira = 3, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Projeler", Url = "projeler", Sira = 4, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Hakkımızda", Url = "hakkimizda", Sira = 5, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Vizyon & Misyon", Url = "vizyon-misyon", Sira = 6, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ekibimiz", Url = "ekibimiz", Sira = 7, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Sertifikalarımız", Url = "sertifikalar", Sira = 8, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Bayiler", Url = "bayiler", Sira = 9, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Fabrikamız", Url = "fabrikamiz", Sira = 10, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Referanslar", Url = "referanslar", Sira = 11, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Haber", Url = "haber", Sira = 12, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "SSS", Url = "sss", Sira = 13, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "İletişim", Url = "iletisim", Sira = 14, Konum = "PublicMobil" }
                );
            }
            else
            {
                // Diğer firmalar için daha genel kurumsal yapı
                vt.MenuOgeleri.AddRange(
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ana Sayfa", Url = "", Sira = 1, Konum = "PublicHeader", Ikon = "Home" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ürünler", Url = "urunler", Sira = 2, Konum = "PublicHeader", Ikon = "Inventory2" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Kurumsal", Url = "hakkimizda", Sira = 3, Konum = "PublicHeader", Ikon = "Info" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "İletişim", Url = "iletisim", Sira = 4, Konum = "PublicHeader", Ikon = "Call" }
                );

                vt.MenuOgeleri.AddRange(
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ana Sayfa", Url = "", Sira = 1, Konum = "PublicMobil", Ikon = "Home" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ürünler", Url = "urunler", Sira = 2, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Kurumsal", Url = "hakkimizda", Sira = 3, Konum = "PublicMobil" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "İletişim", Url = "iletisim", Sira = 4, Konum = "PublicMobil" }
                );
            }
        }

        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaFooterMenuleriAsync(VizitLink3DDbContext vt)
    {
        // Firmalar getir
        var firmalar = await vt.Firmalar.Where(f => f.AktifMi).ToListAsync();

        foreach (var firma in firmalar)
        {
            var goldBanyoMu = string.Equals(firma.Slug, "goldbanyo", StringComparison.OrdinalIgnoreCase)
                || string.Equals(firma.Domain, "goldbanyom.com.tr", StringComparison.OrdinalIgnoreCase)
                || string.Equals(firma.YedekDomain, "www.goldbanyom.com.tr", StringComparison.OrdinalIgnoreCase);

            if (goldBanyoMu)
            {
                // === GOLD BANYO FOOTER HIZLI LINKLER ===
                vt.MenuOgeleri.AddRange(
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ana Sayfa", Url = "", Sira = 1, Konum = "PublicFooterHizli" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Hakkımızda", Url = "hakkimizda", Sira = 2, Konum = "PublicFooterHizli" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Banyo Dolapları", Url = "banyo-dolaplari", Sira = 3, Konum = "PublicFooterHizli" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Katalog", Url = "katalog", Sira = 4, Konum = "PublicFooterHizli" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "İletişim", Url = "iletisim", Sira = 5, Konum = "PublicFooterHizli" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Gizlilik Politikası", Url = "gizlilik", Sira = 6, Konum = "PublicFooterHizli" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Çerez Politikası", Url = "cerez-politikasi", Sira = 7, Konum = "PublicFooterHizli" }
                );

                // === GOLD BANYO FOOTER KATEGORI ===
                vt.MenuOgeleri.AddRange(
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Gold Exclusive", Url = "banyo-dolaplari#exclusive", Sira = 1, Konum = "PublicFooterKategori" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Gold Premium", Url = "banyo-dolaplari#premium", Sira = 2, Konum = "PublicFooterKategori" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Gold Trend", Url = "banyo-dolaplari#trend", Sira = 3, Konum = "PublicFooterKategori" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Gold Standart", Url = "banyo-dolaplari#standart", Sira = 4, Konum = "PublicFooterKategori" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Tüm Ürünler", Url = "banyo-dolaplari", Sira = 5, Konum = "PublicFooterKategori" }
                );
            }
            else
            {
                vt.MenuOgeleri.AddRange(
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Ürünler", Url = "urunler", Sira = 1, Konum = "PublicFooterHizli" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Hakkımızda", Url = "hakkimizda", Sira = 2, Konum = "PublicFooterHizli" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "İletişim", Url = "iletisim", Sira = 3, Konum = "PublicFooterHizli" },
                    new MenuOgesi { FirmaId = firma.Id, Baslik = "Gizlilik Politikası", Url = "gizlilik", Sira = 4, Konum = "PublicFooterHizli" }
                );
            }
        }

        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaAdminMenuleriAsync(VizitLink3DDbContext vt)
    {
        var firmalar = await vt.Firmalar.Where(f => f.AktifMi).ToListAsync();

        foreach (var firma in firmalar)
        {
            vt.MenuOgeleri.AddRange(
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Gosterge Paneli", Url = "admin/dashboard", Sira = 1, Konum = "AdminSol", Ikon = "Dashboard" },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Is Takip", Url = "admin/is-takip", Sira = 2, Konum = "AdminSol", Ikon = "Timeline" },
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Urun Yonetimi", Url = "", Sira = 3, Konum = "AdminSol", Ikon = "Inventory2", GerekliRol = "Admin", AltMenuler = new List<MenuOgesi> {
                    new() { FirmaId = firma.Id, Baslik = "🪄 Urun Sihirbazi", Url = "admin/urun-sihirbazi", Sira = 0, Konum = "AdminSol", Ikon = "AutoFixHigh" },
                    new() { FirmaId = firma.Id, Baslik = "Urunler", Url = "admin/urun-yonetimi", Sira = 1, Konum = "AdminSol", Ikon = "Inventory" },
                    new() { FirmaId = firma.Id, Baslik = "Kapi/Kapak Modelleri", Url = "admin/kapak-modeli-yonetimi", Sira = 2, Konum = "AdminSol", Ikon = "DoorFront" },
                    new() { FirmaId = firma.Id, Baslik = "Urun Aileleri", Url = "admin/urun-ailesi-yonetimi", Sira = 3, Konum = "AdminSol", Ikon = "Category" },
                    new() { FirmaId = firma.Id, Baslik = "Urun Kategorileri", Url = "admin/urun-kategori-yonetimi", Sira = 4, Konum = "AdminSol", Ikon = "Class" },
                    new() { FirmaId = firma.Id, Baslik = "RAL Renk Yonetimi", Url = "admin/ral-renk-yonetimi", Sira = 5, Konum = "AdminSol", Ikon = "Palette" },
                    new() { FirmaId = firma.Id, Baslik = "Malzeme Yonetimi", Url = "admin/malzeme-yonetimi", Sira = 6, Konum = "AdminSol", Ikon = "Texture" },
                    new() { FirmaId = firma.Id, Baslik = "Kaplama Yonetimi", Url = "admin/kaplama-yonetimi", Sira = 7, Konum = "AdminSol", Ikon = "Layers" }
                }},
                new MenuOgesi { FirmaId = firma.Id, Baslik = "3D / Konfigurator", Url = "", Sira = 4, Konum = "AdminSol", Ikon = "ViewInAr", GerekliRol = "Admin", AltMenuler = new List<MenuOgesi> {
                    new() { FirmaId = firma.Id, Baslik = "3D Model Yonetimi", Url = "admin/uc-boyut-model-yonetimi", Sira = 1, Konum = "AdminSol", Ikon = "ThreeDRotation" },
                    new() { FirmaId = firma.Id, Baslik = "Parca Esleme", Url = "admin/uc-boyut-parca-esleme", Sira = 2, Konum = "AdminSol", Ikon = "Extension" },
                    new() { FirmaId = firma.Id, Baslik = "Konfigurasyon Sablonlari", Url = "admin/konfigurasyon-sablonu-yonetimi", Sira = 3, Konum = "AdminSol", Ikon = "Schema" },
                    new() { FirmaId = firma.Id, Baslik = "Konfigurasyon Kurallari", Url = "admin/konfigurasyon-kurali-yonetimi", Sira = 4, Konum = "AdminSol", Ikon = "Rule" },
                    new() { FirmaId = firma.Id, Baslik = "Sahne Ayarlari", Url = "admin/sahne-ayarlari", Sira = 5, Konum = "AdminSol", Ikon = "Tune" }
                }},
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Icerik Yonetimi", Url = "", Sira = 5, Konum = "AdminSol", Ikon = "Article", GerekliRol = "Admin", AltMenuler = new List<MenuOgesi> {
                    new() { FirmaId = firma.Id, Baslik = "Ana Sayfa", Url = "admin/anasayfa-yonetimi", Sira = 1, Konum = "AdminSol", Ikon = "HomeRepairService" },
                    new() { FirmaId = firma.Id, Baslik = "Slayt Yonetimi", Url = "admin/slayt-yonetimi", Sira = 2, Konum = "AdminSol", Ikon = "Slideshow" },
                    new() { FirmaId = firma.Id, Baslik = "Sayfa Icerikleri", Url = "admin/icerik-yonetimi", Sira = 3, Konum = "AdminSol", Ikon = "Description" },
                    new() { FirmaId = firma.Id, Baslik = "Haber Yonetimi", Url = "admin/haber-yonetimi", Sira = 4, Konum = "AdminSol", Ikon = "RssFeed" },
                    new() { FirmaId = firma.Id, Baslik = "SSS Yonetimi", Url = "admin/sss-yonetimi", Sira = 5, Konum = "AdminSol", Ikon = "Quiz" },
                    new() { FirmaId = firma.Id, Baslik = "Sayfa Yonetimi", Url = "admin/sayfa-yonetimi", Sira = 6, Konum = "AdminSol", Ikon = "Description" },
                    new() { FirmaId = firma.Id, Baslik = "SEO Yonetimi", Url = "admin/seo-yonetimi", Sira = 7, Konum = "AdminSol", Ikon = "TrendingUp" }
                }},
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Medya", Url = "", Sira = 6, Konum = "AdminSol", Ikon = "PermMedia", GerekliRol = "Admin", AltMenuler = new List<MenuOgesi> {
                    new() { FirmaId = firma.Id, Baslik = "Medya Havuzu", Url = "admin/medya-havuzu", Sira = 1, Konum = "AdminSol", Ikon = "CloudQueue" },
                    new() { FirmaId = firma.Id, Baslik = "Medya Galerisi", Url = "admin/galeri", Sira = 2, Konum = "AdminSol", Ikon = "PhotoLibrary" },
                    new() { FirmaId = firma.Id, Baslik = "PDF Katalog", Url = "admin/pdf-katalog-yonetimi", Sira = 3, Konum = "AdminSol", Ikon = "PictureAsPdf" },
                    new() { FirmaId = firma.Id, Baslik = "PDF Uygulama Esleme", Url = "admin/pdf-uygulama-esleme", Sira = 4, Konum = "AdminSol", Ikon = "ContentCut" }
                }},
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Pazarlama", Url = "", Sira = 7, Konum = "AdminSol", Ikon = "Campaign", GerekliRol = "Admin", AltMenuler = new List<MenuOgesi> {
                    new() { FirmaId = firma.Id, Baslik = "Proje Yonetimi", Url = "admin/proje-yonetimi", Sira = 1, Konum = "AdminSol", Ikon = "Engineering" },
                    new() { FirmaId = firma.Id, Baslik = "Referanslar", Url = "admin/referans-yonetimi", Sira = 2, Konum = "AdminSol", Ikon = "GroupWork" },
                    new() { FirmaId = firma.Id, Baslik = "Musteri Yorumlari", Url = "admin/yorum-yonetimi", Sira = 3, Konum = "AdminSol", Ikon = "RateReview" },
                    new() { FirmaId = firma.Id, Baslik = "Hizmet Adimlari", Url = "admin/hizmet-adimi-yonetimi", Sira = 4, Konum = "AdminSol", Ikon = "Timeline" },
                    new() { FirmaId = firma.Id, Baslik = "Katalog Yonetimi", Url = "admin/katalog-yonetimi", Sira = 5, Konum = "AdminSol", Ikon = "BookOnline" },
                    new() { FirmaId = firma.Id, Baslik = "Bulten Aboneleri", Url = "admin/bulten-yonetimi", Sira = 6, Konum = "AdminSol", Ikon = "Mail" },
                    new() { FirmaId = firma.Id, Baslik = "E-posta Sablonlari", Url = "admin/eposta-sablonlari", Sira = 7, Konum = "AdminSol", Ikon = "Email" }
                }},
                new MenuOgesi { FirmaId = firma.Id, Baslik = "İletişim / Destek", Url = "", Sira = 8, Konum = "AdminSol", Ikon = "SupportAgent", GerekliRol = "Admin", AltMenuler = new List<MenuOgesi> {
                    new() { FirmaId = firma.Id, Baslik = "Gelen Mesajlar", Url = "admin/iletisim-mesajlari", Sira = 1, Konum = "AdminSol", Ikon = "Message" },
                    new() { FirmaId = firma.Id, Baslik = "Canli Sohbet", Url = "admin/canli-sohbet", Sira = 2, Konum = "AdminSol", Ikon = "ChatBubbleOutline" },
                    new() { FirmaId = firma.Id, Baslik = "Teklif Yonetimi", Url = "admin/teklif-yonetimi", Sira = 3, Konum = "AdminSol", Ikon = "RequestQuote" }
                }},
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Organizasyon", Url = "", Sira = 9, Konum = "AdminSol", Ikon = "Business", GerekliRol = "Admin", AltMenuler = new List<MenuOgesi> {
                    new() { FirmaId = firma.Id, Baslik = "Sube Yonetimi", Url = "admin/sube-yonetimi", Sira = 1, Konum = "AdminSol", Ikon = "Store" },
                    new() { FirmaId = firma.Id, Baslik = "Ekip Yonetimi", Url = "admin/ekip-yonetimi", Sira = 2, Konum = "AdminSol", Ikon = "People" }
                }},
                new MenuOgesi { FirmaId = firma.Id, Baslik = "Sistem", Url = "", Sira = 10, Konum = "AdminSol", Ikon = "Settings", AltMenuler = new List<MenuOgesi> {
                    new() { FirmaId = firma.Id, Baslik = "Kullanici Yonetimi", Url = "admin/kullanici-yonetimi", Sira = 1, Konum = "AdminSol", Ikon = "Person", SuperAdminGerekliMi = true },
                    new() { FirmaId = firma.Id, Baslik = "Dil ve Ceviri", Url = "admin/ceviri-yonetimi", Sira = 2, Konum = "AdminSol", Ikon = "Translate" },
                    new() { FirmaId = firma.Id, Baslik = "AI Ayarlari", Url = "admin/ai-ayarlari", Sira = 3, Konum = "AdminSol", Ikon = "Psychology", SuperAdminGerekliMi = true },
                    new() { FirmaId = firma.Id, Baslik = "Gorunum & Tema", Url = "admin/tema-yonetimi", Sira = 4, Konum = "AdminSol", Ikon = "Palette" },
                    new() { FirmaId = firma.Id, Baslik = "Lisans Yonetimi", Url = "admin/lisans-yonetimi", Sira = 5, Konum = "AdminSol", Ikon = "VerifiedUser", SuperAdminGerekliMi = true },
                    new() { FirmaId = firma.Id, Baslik = "API Entegrasyonlari", Url = "admin/api-ayarlari", Sira = 6, Konum = "AdminSol", Ikon = "Api", SuperAdminGerekliMi = true },
                    new() { FirmaId = firma.Id, Baslik = "Sistem Ayarlari", Url = "admin/ayarlar", Sira = 7, Konum = "AdminSol", Ikon = "Tune", SuperAdminGerekliMi = true },
                    new() { FirmaId = firma.Id, Baslik = "Denetim Loglari", Url = "admin/denetim-log", Sira = 8, Konum = "AdminSol", Ikon = "History", SuperAdminGerekliMi = true },
                    new() { FirmaId = firma.Id, Baslik = "Cop Kutusu", Url = "admin/cop-kutusu", Sira = 9, Konum = "AdminSol", Ikon = "Delete", SuperAdminGerekliMi = true },
                    new() { FirmaId = firma.Id, Baslik = "Menu Yonetimi", Url = "admin/menu-yonetimi", Sira = 10, Konum = "AdminSol", Ikon = "Menu", SuperAdminGerekliMi = true }
                }}
            );
        }
        await vt.SaveChangesAsync();
        await AdminMenuleriniSadelestirAsync(vt);
    }

    private static async Task AdminMenuleriniSadelestirAsync(VizitLink3DDbContext vt)
    {
        var menuler = await vt.MenuOgeleri
            .Where(m => m.Konum == "AdminSol" && !m.SilindiMi)
            .ToListAsync();

        MenuOgesi? Kok(string baslik) => menuler.FirstOrDefault(m => m.UstMenuId == null && m.Baslik == baslik);
        MenuOgesi? Url(string url) => menuler.FirstOrDefault(m => m.Url == url);

        var urun = Kok("Urun Yonetimi");
        if (urun != null)
        {
            urun.Baslik = "Urun ve 3D";
            urun.Ikon = "ViewInAr";
            urun.Sira = 3;
        }

        var icerik = Kok("Icerik Yonetimi");
        if (icerik != null)
        {
            icerik.Baslik = "Icerik ve Medya";
            icerik.Ikon = "PermMedia";
            icerik.Sira = 4;
        }

        var musteri = Kok("Pazarlama");
        if (musteri != null)
        {
            musteri.Baslik = "Musteri ve Operasyon";
            musteri.Ikon = "SupportAgent";
            musteri.Sira = 5;
        }

        var dashboard = Kok("Gosterge Paneli");
        if (dashboard != null) dashboard.Sira = 1;

        var isTakip = Kok("Is Takip");
        if (isTakip != null) isTakip.Sira = 2;

        var sistem = Kok("Sistem");
        if (sistem != null)
        {
            sistem.Sira = 6;
            sistem.SuperAdminGerekliMi = false;
        }

        TasiVeKapat(Kok("3D / Konfigurator"), urun, menuler);
        TasiVeKapat(Kok("Medya"), icerik, menuler);
        TasiVeKapat(Kok("İletişim / Destek"), musteri, menuler);
        TasiVeKapat(Kok("Organizasyon"), musteri, menuler);

        var katalog = Url("admin/katalog-yonetimi");
        if (katalog != null && icerik != null)
            katalog.UstMenuId = icerik.Id;

        Sira(Url("admin/urun-sihirbazi"), 1);
        Sira(Url("admin/urun-yonetimi"), 2);
        Sira(Url("admin/kapak-modeli-yonetimi"), 3);
        Sira(Url("admin/uc-boyut-model-yonetimi"), 4);
        Sira(Url("admin/uc-boyut-parca-esleme"), 5);
        Sira(Url("admin/ral-renk-yonetimi"), 6);
        Sira(Url("admin/malzeme-yonetimi"), 7);
        Sira(Url("admin/kaplama-yonetimi"), 8);
        Sira(Url("admin/urun-ailesi-yonetimi"), 9);
        Sira(Url("admin/urun-kategori-yonetimi"), 10);
        Sira(Url("admin/sahne-ayarlari"), 11);
        Sira(Url("admin/konfigurasyon-sablonu-yonetimi"), 12);
        Sira(Url("admin/konfigurasyon-kurali-yonetimi"), 12);

        Sira(Url("admin/anasayfa-yonetimi"), 1);
        Sira(Url("admin/slayt-yonetimi"), 2);
        Sira(Url("admin/icerik-yonetimi"), 3);
        Sira(Url("admin/sayfa-yonetimi"), 4);
        Sira(Url("admin/haber-yonetimi"), 5);
        Sira(Url("admin/sss-yonetimi"), 6);
        Sira(Url("admin/seo-yonetimi"), 7);
        Sira(Url("admin/medya-havuzu"), 8);
        Sira(Url("admin/galeri"), 9);
        Sira(Url("admin/pdf-katalog-yonetimi"), 10);
        Sira(Url("admin/katalog-yonetimi"), 11);

        Sira(Url("admin/proje-yonetimi"), 1);
        Sira(Url("admin/referans-yonetimi"), 2);
        Sira(Url("admin/yorum-yonetimi"), 3);
        Sira(Url("admin/hizmet-adimi-yonetimi"), 4);
        Sira(Url("admin/bulten-yonetimi"), 5);
        Sira(Url("admin/eposta-sablonlari"), 6);
        Sira(Url("admin/iletisim-mesajlari"), 7);
        Sira(Url("admin/canli-sohbet"), 8);
        Sira(Url("admin/teklif-yonetimi"), 9);
        Sira(Url("admin/sube-yonetimi"), 10);
        Sira(Url("admin/ekip-yonetimi"), 11);

        await vt.SaveChangesAsync();

        static void TasiVeKapat(MenuOgesi? kaynak, MenuOgesi? hedef, List<MenuOgesi> menuler)
        {
            if (kaynak == null || hedef == null)
                return;

            foreach (var alt in menuler.Where(m => m.UstMenuId == kaynak.Id))
                alt.UstMenuId = hedef.Id;

            kaynak.SilindiMi = true;
            kaynak.SilinmeTarihi = DateTime.UtcNow;
            kaynak.AktifMi = false;
        }

        static void Sira(MenuOgesi? menu, int sira)
        {
            if (menu != null)
                menu.Sira = sira;
        }
    }

    private static List<object> VarsayilanRenkPaleti() => new()
    {
        new { Ad = "Kirik Beyaz", HexKod = "#F5F2ED", Grup = "Beyazlar", UreticiKodu = "W-001" },
        new { Ad = "Parlak Beyaz", HexKod = "#FFFFFF", Grup = "Beyazlar", UreticiKodu = "W-002" },
        new { Ad = "Krem", HexKod = "#E8DCC8", Grup = "Beyazlar", UreticiKodu = "W-003" },
        new { Ad = "Acik Gri", HexKod = "#D4D4D4", Grup = "Gri Tonlari", UreticiKodu = "G-001" },
        new { Ad = "Orta Gri", HexKod = "#9A9A9A", Grup = "Gri Tonlari", UreticiKodu = "G-002" },
        new { Ad = "Antrasit", HexKod = "#3D3D3D", Grup = "Gri Tonlari", UreticiKodu = "G-003" },
        new { Ad = "Mat Siyah", HexKod = "#1A1A1A", Grup = "Koyu Renkler", UreticiKodu = "D-001" },
        new { Ad = "Koyu Lacivert", HexKod = "#1C2B4A", Grup = "Koyu Renkler", UreticiKodu = "D-002" },
        new { Ad = "Petrol Yesili", HexKod = "#2D5A4A", Grup = "Dogal Renkler", UreticiKodu = "N-001" },
        new { Ad = "Sage Yesili", HexKod = "#7D9B76", Grup = "Dogal Renkler", UreticiKodu = "N-002" },
        new { Ad = "Toprak Kirmizi", HexKod = "#8B4543", Grup = "Sicak Renkler", UreticiKodu = "H-001" },
        new { Ad = "Bal Sarisi", HexKod = "#C8952A", Grup = "Sicak Renkler", UreticiKodu = "H-002" }
    };

    private static async Task TohumlaDosyadanKapiModelleriAsync(VizitLink3DDbContext vt)
    {
        var medyaKoku = KapiModelMedyaKokunuBul();
        if (string.IsNullOrWhiteSpace(medyaKoku) || !Directory.Exists(medyaKoku))
        {
            Console.WriteLine("[TOHUM] Kapi modeli medya klasoru bulunamadi, dosyadan seed atlandi.");
            return;
        }

        var mevcutKodlar = (await vt.KapakModelleri
            .Where(k => !k.SilindiMi)
            .Select(k => k.ModelKodu)
            .ToListAsync())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var mevcutSluglar = (await vt.KapakModelleri
            .Where(k => !k.SilindiMi)
            .Select(k => k.Slug)
            .ToListAsync())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var renkJson = JsonSerializer.Serialize(VarsayilanRenkPaleti());
        var eklenecekler = new List<KapakModeli>();
        var eklenecekKodlar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var eklenecekSluglar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dosyalar = Directory
            .EnumerateFiles(medyaKoku, "*.*", SearchOption.AllDirectories)
            .Where(y => y.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || y.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                || y.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || y.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            .OrderBy(y => y)
            .ToList();

        var sira = 10_000;
        foreach (var dosya in dosyalar)
        {
            var bagil = Path.GetRelativePath(medyaKoku, dosya).Replace('\\', '/');
            if (bagil.StartsWith("04-", StringComparison.OrdinalIgnoreCase) || bagil.StartsWith("99-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var bilgi = KapiModelBilgisiOlustur(bagil);
            if (mevcutKodlar.Contains(bilgi.ModelKodu)
                || mevcutSluglar.Contains(bilgi.Slug)
                || eklenecekKodlar.Contains(bilgi.ModelKodu)
                || eklenecekSluglar.Contains(bilgi.Slug))
            {
                continue;
            }

            var yol = $"/medya/kapi-modelleri/{bagil}";
            eklenecekler.Add(new KapakModeli
            {
                ModelAdi = bilgi.ModelAdi,
                ModelKodu = bilgi.ModelKodu,
                Slug = bilgi.Slug,
                Kategori = bilgi.Kategori,
                ModelTuru = "Kapi",
                AnaGorselUrl = yol,
                OnYazi = $"{bilgi.Kategori} serisinde endustriyel kapi modeli.",
                Aciklama = $"{bilgi.ModelAdi}; lake, membran ve ozel seri kapi koleksiyonlari icinde dinamik olarak yonetilen VizitLink3D modelidir.",
                RenkSecenekleriJson = renkJson,
                NiteliklerJson = JsonSerializer.Serialize(bilgi.Nitelikler),
                UygulamaGorselleriJson = JsonSerializer.Serialize(new[] { yol }),
                MinYukseklik = 1800,
                MaxYukseklik = 2600,
                MinGenislik = 600,
                MaxGenislik = 1100,
                OneCikanMi = eklenecekler.Count % 9 == 0,
                YeniMi = eklenecekler.Count < 12,
                SiraNo = sira++,
                OlusturulmaTarihi = DateTime.UtcNow
            });
            eklenecekKodlar.Add(bilgi.ModelKodu);
            eklenecekSluglar.Add(bilgi.Slug);
        }

        if (eklenecekler.Any())
        {
            vt.KapakModelleri.AddRange(eklenecekler);
            await vt.SaveChangesAsync();
            Console.WriteLine($"[TOHUM] {eklenecekler.Count} dosya kaynakli kapi modeli eklendi.");
        }
    }

    private static string? KapiModelMedyaKokunuBul()
    {
        var adaylar = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "wwwroot", "medya", "kapi-modelleri"),
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "medya", "kapi-modelleri"),
            Path.Combine(Directory.GetCurrentDirectory(), "VizitLink3D.Api", "wwwroot", "medya", "kapi-modelleri")
        };

        return adaylar.FirstOrDefault(Directory.Exists);
    }

    private static (string ModelAdi, string ModelKodu, string Slug, string Kategori, string[] Nitelikler) KapiModelBilgisiOlustur(string bagilYol)
    {
        var parcalar = bagilYol.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var dosyaAdi = Path.GetFileNameWithoutExtension(bagilYol);
        var temizAd = dosyaAdi.Replace('-', ' ');
        var seri = parcalar.Length > 0 ? parcalar[0] : "";
        var altSeri = parcalar.Length > 1 ? parcalar[1] : "";

        var ustKategori = seri.Contains("Lake", StringComparison.OrdinalIgnoreCase)
            ? "Lake Kapilar"
            : seri.Contains("Membran", StringComparison.OrdinalIgnoreCase)
                ? "Membran Kapilar"
                : "Ozel Seri Kapilar";
        var altKategori = altSeri.Contains("Camli", StringComparison.OrdinalIgnoreCase)
            ? "Camli Modeller"
            : altSeri.Contains("Duz", StringComparison.OrdinalIgnoreCase)
                ? "Duz Modeller"
                : altSeri.Replace('-', ' ');
        var kategori = string.IsNullOrWhiteSpace(altKategori)
            ? ustKategori
            : $"{ustKategori} / {altKategori}";

        var modelKodu = dosyaAdi
            .Replace("Lake-Kapi-", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Membran-Kapi-", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Ozel-Seri-Kapi-", "", StringComparison.OrdinalIgnoreCase)
            .Replace("-Camli-Model", "", StringComparison.OrdinalIgnoreCase)
            .Replace("-Duz-Model", "", StringComparison.OrdinalIgnoreCase)
            .Replace("-Model", "", StringComparison.OrdinalIgnoreCase)
            .ToUpperInvariant();

        var modelAdi = $"{temizAd} Kapi Modeli";
        var slug = dosyaAdi.ToLowerInvariant();
        var nitelikler = new[] { "Kapi Modeli", ustKategori, altKategori }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        return (modelAdi, modelKodu, slug, kategori, nitelikler);
    }

    private static async Task TohumlaAyarlariAsync(VizitLink3DDbContext vt)
    {
        vt.SayfaIcerikleri.AddRange(
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "LogoUrl", Deger = "/img/goldbanyo-logo.png", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "FaviconUrl", Deger = "/favicon.png", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "VarsayilanDil", Deger = "tr", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "TemaModu", Deger = "koyu", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "SiteBasligi", Deger = "Gold Banyo - Banyo Mobilyalari ve Koleksiyonlari", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "Aciklama", Deger = "Gold Banyo; premium, trend ve exclusive koleksiyonlariyla banyo mobilyasi ve proje cozumleri sunar.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "AnahtarKelimeler", Deger = "gold banyo, goldbanyo, banyo mobilyasi, banyo dolabi, banyo koleksiyonlari", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "Adres", Deger = "Çalı Mah. Ömer Biltekin Bulv. No:3/1A Nilüfer / BURSA", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "Telefon1", Deger = "+90 224 482 24 00", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "Telefon2", Deger = "+90 533 597 32 14", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "Eposta", Deger = "info@goldbanyom.com.tr", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "Whatsapp", Deger = "+90 533 597 32 14", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "Instagram", Deger = "https://www.instagram.com/gold.banyom/", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "InstagramUrl", Deger = "https://www.instagram.com/gold.banyom/", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "Facebook", Deger = "https://www.facebook.com/gold.banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "FacebookUrl", Deger = "https://www.facebook.com/gold.banyo", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "Youtube", Deger = "", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "YoutubeUrl", Deger = "", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "MesaiSaatleri", Deger = "09:00 - 18:00", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "FooterAciklama", Deger = "Gold Banyo, Bursa Nilüfer'deki üretim merkeziyle premium, trend ve exclusive banyo mobilyası koleksiyonlarını bayi ve proje kanallarına sunar.", Dil = "tr" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "FooterAciklama", Deger = "Gold Banyo delivers premium, trend and exclusive bathroom furniture collections from its production base in Bursa Nilufer to dealers and project partners.", Dil = "en" },
            new SayfaIcerigi { Bolum = "ayarlar", Anahtar = "MesaiSaatleri", Deger = "09:00 - 18:00", Dil = "en" }
        );
    }

    private static async Task TohumlaProjeleriAsync(VizitLink3DDbContext vt)
    {
        // FK guvenligi: KategoriId sabit yazilmaz. Kategoriler slug ile DB'den
        // eslenir â€” autoincrement Id kaymasi olsa bile FK constraint kirilmaz.
        // (Onceki sabit Kategori=1..4 kullanimi, tekrarli seed sonrasi Id drift'i
        // yuzunden 'FOREIGN KEY constraint failed' verip tum tohum batch'ini
        // geri aliyor, siteyi bos birakiyordu.)
        var kategoriler = await vt.ProjeKategorileri
            .ToDictionaryAsync(k => k.Slug, k => k.Id);

        // Kategori yoksa FK kirilmasin diye proje tohumu atlanir.
        if (kategoriler.Count == 0) return;

        int Kat(string slug) =>
            kategoriler.TryGetValue(slug, out var id) ? id : kategoriler.Values.First();

        var u1 = new Proje { Slug = "modern-mutfak-tasarimi", Baslik = "Modern Mutfak Tasarimi", KisaAciklama = "Nilufer'deki villa mutfagi icin ozel lake kapak tasarimi.", KategoriId = Kat("mutfak"), MusteriAdi = "Ahmet Yilmaz", MusteriSehir = "Bursa", OneCikanMi = true, SiraNo = 1, ProjeTarihi = new DateTime(2025, 3, 15) };
        var u2 = new Proje { Slug = "banyo-dolap-sistemi", Baslik = "Banyo Dolap Sistemi", KisaAciklama = "Suya dayanikli membran kapli banyo dolabi projesi.", KategoriId = Kat("banyo"), MusteriAdi = "Zeynep Kaya", MusteriSehir = "Istanbul", SiraNo = 2, ProjeTarihi = new DateTime(2025, 1, 20) };
        var u3 = new Proje { Slug = "luks-yatak-odasi", Baslik = "Luks Yatak Odasi", KisaAciklama = "Klasik taplali kapaklarla tasarlanmis genis yatak odasi.", KategoriId = Kat("yatak-odasi"), MusteriAdi = "Mehmet Demir", MusteriSehir = "Ankara", OneCikanMi = true, SiraNo = 3, ProjeTarihi = new DateTime(2024, 11, 5) };
        var u4 = new Proje { Slug = "ofis-mutak-dolabi", Baslik = "Ofis Mutfak Dolabi", KisaAciklama = "Kurumsal ofis icin kompakt ve modern mutfak dolabi.", KategoriId = Kat("ofis"), MusteriAdi = "Ece Ltd.", MusteriSehir = "Izmir", SiraNo = 4, ProjeTarihi = new DateTime(2024, 8, 10) };
        var u5 = new Proje { Slug = "villa-mutfak-projesi", Baslik = "Villa Mutfak Projesi", KisaAciklama = "Membran ve lake karisik ozel tasarim villa mutfagi.", KategoriId = Kat("mutfak"), MusteriAdi = "Ali Ozturk", MusteriSehir = "Bursa", OneCikanMi = true, SiraNo = 5, ProjeTarihi = new DateTime(2025, 4, 1) };
        var u6 = new Proje { Slug = "otel-banyo-yenileme", Baslik = "Otel Banyo Yenileme", KisaAciklama = "12 odali butik otel icin komple banyo dolap yenilemesi.", KategoriId = Kat("banyo"), MusteriAdi = "Otel A.S.", MusteriSehir = "Antalya", SiraNo = 6, ProjeTarihi = new DateTime(2024, 6, 20) };

        vt.Projeler.AddRange(u1, u2, u3, u4, u5, u6);
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaEksikIceriklerAsync(VizitLink3DDbContext vt)
    {
        if (!vt.GaleriGorselleri.Any())
            vt.GaleriGorselleri.AddRange(
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Membran Klasik Duz", Sira = 1 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Membran Camli", Sira = 2 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Membran Kasetli", Sira = 3 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Lake Premium", Sira = 4 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Lake Cerceveli", Sira = 5 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Mat Lake Ozel", Sira = 6 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Akrilik Parili", Sira = 7 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Metalik Gloss", Sira = 8 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Gloss Cift Renk", Sira = 9 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Klasik Taplali", Sira = 10 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Country Stil", Sira = 11 },
                new GaleriGorseli { Url = "/medya/vizitlink3d_default.png", Baslik = "Osmanli Motifli", Sira = 12 }
            );

        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaMedyaKlasorleriAsync(VizitLink3DDbContext vt)
    {
        vt.MedyaKlasorleri.AddRange(
            new MedyaKlasoru { Ad = "Kapılar", Slug = "kapilar", Ikon = "DoorFront", SiraNo = 1 },
            new MedyaKlasoru { Ad = "Mobilyalar", Slug = "mobilyalar", Ikon = "Chair", SiraNo = 2 },
            new MedyaKlasoru { Ad = "Projeler", Slug = "projeler", Ikon = "Construction", SiraNo = 3 },
            new MedyaKlasoru { Ad = "Slayt", Slug = "slayt", Ikon = "Slideshow", SiraNo = 4 },
            new MedyaKlasoru { Ad = "Logolar", Slug = "logolar", Ikon = "Image", SiraNo = 5 },
            new MedyaKlasoru { Ad = "Sertifikalar", Slug = "sertifikalar", Ikon = "Verified", SiraNo = 6 }
        );
        await vt.SaveChangesAsync();
    }

    // ==================== URUN YONETIMI (3D) SEED ====================

    private static async Task TohumlaIsTakipAsync(VizitLink3DDbContext vt)
    {
        vt.IsTakipKayitlari.AddRange(
            new IsTakipKaydi { Baslik = "Admin paneli mobil uyumlu hale getir", Aciklama = "Responsive tasarım kontrolü ve mobil menü düzeltmeleri", Durum = "Bekliyor", Oncelik = "Yuksek", Kategori = "Frontend", SiraNo = 1 },
            new IsTakipKaydi { Baslik = "Haber sayfası içerik yönetimi", Aciklama = "Haber ekleme, düzenleme, silme özellikleri", Durum = "Bekliyor", Oncelik = "Orta", Kategori = "Backend", SiraNo = 2 },
            new IsTakipKaydi { Baslik = "3D model yükleme optimizasyonu", Aciklama = "DRACO sıkıştırma ve progresif yükleme", Durum = "Yapiliyor", Oncelik = "Yuksek", Kategori = "Backend", SiraNo = 3 },
            new IsTakipKaydi { Baslik = "Endüstriyel tema token'ları tamamla", Aciklama = "Tüm CSS hardcoded renkleri var(--desa-*) token'larına çevir", Durum = "Yapiliyor", Oncelik = "Kritik", Kategori = "Tasarim", SiraNo = 4 },
            new IsTakipKaydi { Baslik = "Multi-tenant global query filter", Aciklama = "Tüm entity'lere FirmaId filtresi ekle, veri izolasyonunu tamamla", Durum = "Bekliyor", Oncelik = "Kritik", Kategori = "Altyapi", SiraNo = 5 },
            new IsTakipKaydi { Baslik = "Lisans yenileme bildirim sistemi", Aciklama = "30/20/15/7/3/1 gün kala email bildirimi gönder", Durum = "Bekliyor", Oncelik = "Yuksek", Kategori = "Backend", SiraNo = 6 },
            new IsTakipKaydi { Baslik = "Public site SEO meta tag'leri", Aciklama = "Her sayfa için dinamik title, description, og:image", Durum = "Bekliyor", Oncelik = "Orta", Kategori = "Frontend", SiraNo = 7 },
            new IsTakipKaydi { Baslik = "SignalR canlı dashboard bağlantısı", Aciklama = "Dashboard'a gerçek zamanlı veri akışı ekle", Durum = "Bekliyor", Oncelik = "Yuksek", Kategori = "Altyapi", SiraNo = 8 },
            new IsTakipKaydi { Baslik = "İş takip defteri filtreleme", Aciklama = "Durum, öncelik ve kategori bazlı filtreleme ekle", Durum = "Bekliyor", Oncelik = "Dusuk", Kategori = "Frontend", SiraNo = 9 },
            new IsTakipKaydi { Baslik = "Admin menü ikon düzeltmeleri tamamla", Aciklama = "Tüm MudNavGroup ikonlarının doğru görünmesini sağla", Durum = "Tamamlandi", Oncelik = "Yuksek", Kategori = "Frontend", SiraNo = 10, TamamlanmaTarihi = DateTime.UtcNow }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaUrunAilesileriniAsync(VizitLink3DDbContext vt)
    {
        vt.UrunAilesileri.AddRange(
            new UrunAilesi { Ad = "Kapak", Slug = "kapak", Aciklama = "Mobilya/dolap kapak sistemleri", VarsayilanDetaySablonu = "KapakKonfigurator", SiraNo = 1, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UrunAilesi { Ad = "Kapi", Slug = "kapi", Aciklama = "Ic ve dis kapi modelleri", VarsayilanDetaySablonu = "KapiKonfigurator", SiraNo = 2, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UrunAilesi { Ad = "Dolap / Banyo", Slug = "dolap-banyo", Aciklama = "Banyo dolabi, lavabo sistemleri", VarsayilanDetaySablonu = "BanyoKonfigurator", SiraNo = 3, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UrunAilesi { Ad = "Dusakabin", Slug = "dusakabin", Aciklama = "Modern dusakabin sistemleri", VarsayilanDetaySablonu = "DusakabinKonfigurator", SiraNo = 4, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new UrunAilesi { Ad = "Vestiyer", Slug = "vestiyer", Aciklama = "Vestiyer sistemleri", VarsayilanDetaySablonu = "Endustriyel3D", SiraNo = 5, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaUrunKategorileriniAsync(VizitLink3DDbContext vt)
    {
        vt.UrunKategorileri.AddRange(
            new UrunKategori { Ad = "Mutfak", Slug = "mutfak", SiraNo = 1, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new UrunKategori { Ad = "Banyo", Slug = "banyo", SiraNo = 2, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new UrunKategori { Ad = "Yatak Odasi", Slug = "yatak-odasi", SiraNo = 3, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new UrunKategori { Ad = "Oturma Odasi", Slug = "oturma-odasi", SiraNo = 4, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaRenkKataloglariniAsync(VizitLink3DDbContext vt)
    {
        vt.RenkKataloglari.AddRange(
            new RenkKatalogu { Ad = "RAL Klasik", Aciklama = "Standart RAL renk katalogu", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaRalRenkleriniAsync(VizitLink3DDbContext vt)
    {
        vt.RalRenkleri.AddRange(
            new RalRengi { Kod = "RAL 9016", Ad = "Trafik Beyazi", HexKod = "#F1F0EA", Grup = "Beyazlar", KatalogId = 1, SiraNo = 1, YuzeyTipi = "Parlak", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 9010", Ad = "Saf Beyaz", HexKod = "#F2ECE1", Grup = "Beyazlar", KatalogId = 1, SiraNo = 2, YuzeyTipi = "Parlak", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 9003", Ad = "Sinyal Beyazi", HexKod = "#F4F4F2", Grup = "Beyazlar", KatalogId = 1, SiraNo = 3, YuzeyTipi = "Parlak", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 9001", Ad = "Krem", HexKod = "#E9E0CB", Grup = "Beyazlar", KatalogId = 1, SiraNo = 4, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 1015", Ad = "Acik Fildisi", HexKod = "#E6D5B2", Grup = "Bejler", KatalogId = 1, SiraNo = 5, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 1013", Ad = "Inci Beyazi", HexKod = "#E4DDD0", Grup = "Bejler", KatalogId = 1, SiraNo = 6, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 7035", Ad = "Acik Gri", HexKod = "#C9C9C6", Grup = "Griler", KatalogId = 1, SiraNo = 7, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 7047", Ad = "Telegri 4", HexKod = "#D0D0D0", Grup = "Griler", KatalogId = 1, SiraNo = 8, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 7040", Ad = "Pencere Grisi", HexKod = "#9DA1A2", Grup = "Griler", KatalogId = 1, SiraNo = 9, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 7030", Ad = "Tas Grisi", HexKod = "#928E85", Grup = "Griler", KatalogId = 1, SiraNo = 10, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 7016", Ad = "Antrasit Grisi", HexKod = "#383E42", Grup = "Griler", KatalogId = 1, SiraNo = 11, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 7021", Ad = "Siyah Gri", HexKod = "#2C2E33", Grup = "Griler", KatalogId = 1, SiraNo = 12, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 9005", Ad = "Derin Siyah", HexKod = "#0E0E10", Grup = "Siyahlar", KatalogId = 1, SiraNo = 13, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 9004", Ad = "Sinyal Siyahi", HexKod = "#28282A", Grup = "Siyahlar", KatalogId = 1, SiraNo = 14, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 3003", Ad = "Yakut Kirmizisi", HexKod = "#861A22", Grup = "Kirmizilar", KatalogId = 1, SiraNo = 15, YuzeyTipi = "Parlak", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 3011", Ad = "Kahverengi Kirmizi", HexKod = "#781F1E", Grup = "Kirmizilar", KatalogId = 1, SiraNo = 16, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 5012", Ad = "Acik Mavi", HexKod = "#2C5370", Grup = "Maviler", KatalogId = 1, SiraNo = 17, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 5008", Ad = "Gri Mavi", HexKod = "#2B3A44", Grup = "Maviler", KatalogId = 1, SiraNo = 18, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 5024", Ad = "Pastel Mavi", HexKod = "#6093A2", Grup = "Maviler", KatalogId = 1, SiraNo = 19, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 6005", Ad = "Yosun Yesili", HexKod = "#0C4634", Grup = "Yesiller", KatalogId = 1, SiraNo = 20, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 6021", Ad = "Soluk Yesil", HexKod = "#8C9A7A", Grup = "Yesiller", KatalogId = 1, SiraNo = 21, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 6019", Ad = "Pastel Yesil", HexKod = "#B6CCB6", Grup = "Yesiller", KatalogId = 1, SiraNo = 22, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 8017", Ad = "Cikolata Kahverengisi", HexKod = "#442F29", Grup = "Kahverengiler", KatalogId = 1, SiraNo = 23, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new RalRengi { Kod = "RAL 8024", Ad = "Bej Kahverengi", HexKod = "#A1543C", Grup = "Kahverengiler", KatalogId = 1, SiraNo = 24, YuzeyTipi = "Mat", AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaMalzemeleriAsync(VizitLink3DDbContext vt)
    {
        vt.Malzemeler.AddRange(
            new Malzeme { Ad = "Membran", Aciklama = "MDF uzerine isil presleme kaplama", Tip = "Kaplama", SiraNo = 1, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new Malzeme { Ad = "Lake", Aciklama = "UV lake boyama", Tip = "Boya", SiraNo = 2, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new Malzeme { Ad = "Laminant", Aciklama = "Yuksek basincli laminant", Tip = "Kaplama", SiraNo = 3, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new Malzeme { Ad = "Akrilik", Aciklama = "Akrilik kaplama, parlak yuzey", Tip = "Kaplama", SiraNo = 4, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new Malzeme { Ad = "Cam", Aciklama = "Temperli cam", Tip = "Cam", SiraNo = 5, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new Malzeme { Ad = "Aluminyum", Aciklama = "Aluminyum profil", Tip = "Metal", SiraNo = 6, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new Malzeme { Ad = "MDF", Aciklama = "Orta yogunluklu lif levha", Tip = "Ahsap", SiraNo = 7, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new Malzeme { Ad = "Krom", Aciklama = "Krom kaplama metal", Tip = "Metal", SiraNo = 8, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new Malzeme { Ad = "Porselen", Aciklama = "Porselen seramik", Tip = "Seramik", SiraNo = 9, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaKaplamaSecenekleriniAsync(VizitLink3DDbContext vt)
    {
        vt.KaplamaSecenekleri.AddRange(
            new KaplamaSecenegi { Ad = "Mat", Aciklama = "Mat yuzey bitisi", MalzemeId = 2, SiraNo = 1, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new KaplamaSecenegi { Ad = "Parlak", Aciklama = "Parlak yuzey bitisi", MalzemeId = 2, SiraNo = 2, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new KaplamaSecenegi { Ad = "Yari Mat", Aciklama = "Yari mat yuzey", MalzemeId = 2, SiraNo = 3, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new KaplamaSecenegi { Ad = "Ahsap Desen", Aciklama = "Dogal ahsap goruntulu", MalzemeId = 1, SiraNo = 4, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new KaplamaSecenegi { Ad = "Duz", Aciklama = "Duz yuzey", MalzemeId = 1, SiraNo = 5, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new KaplamaSecenegi { Ad = "Krom", Aciklama = "Krom yuzey", MalzemeId = 8, SiraNo = 6, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new KaplamaSecenegi { Ad = "Siyah Krom", Aciklama = "Siyah krom kaplama", MalzemeId = 8, SiraNo = 7, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new KaplamaSecenegi { Ad = "Altin", Aciklama = "Altin rengi kaplama", MalzemeId = 8, SiraNo = 8, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaReferansUrunleriniAsync(VizitLink3DDbContext vt)
    {
        // --- Urun 1: Duz Kapak 402 (DEMO — kaldırıldı) ---
        // Gerçek ürünler KapakGocServisi tarafından yüklenir.

        // --- Urun 2: Dusakabin Luna ---
        var dusakabinUrun = new Urun
        {
            Slug = "dusakabin-luna",
            Kod = "DSK-001",
            Ad = "Luna Dusakabin",
            KisaAciklama = "Cercevesiz temperli cam, krom profil",
            Aciklama = "Modern cercevesiz dusakabin. 8 mm temperli cam, krom profiller ve manyetik conta.",
            UrunAilesiId = 4,
            UrunKategoriId = 2,
            AktifMi = true,
            OneCikanMi = true,
            YeniMi = true,
            Fiyat = 4250m,
            Birim = "adet",
            SiraNo = 2,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        vt.Urunler.Add(dusakabinUrun);
        await vt.SaveChangesAsync();

        var dusakabinMedyaResim = new UrunMedya
        {
            UrunId = dusakabinUrun.Id,
            MedyaUrl = "/medya/vizitlink3d_default.png",
            MedyaTuru = "Resim",
            SiraNo = 1,
            AnaGosterim = true
        };
        vt.UrunMedyalari.Add(dusakabinMedyaResim);
        await vt.SaveChangesAsync();
        dusakabinUrun.AnaGorselMedyaId = dusakabinMedyaResim.Id;

        vt.UrunUcBoyutModelleri.Add(new UrunUcBoyutModeli
        {
            UrunId = dusakabinUrun.Id,
            ModelAdi = "Luna Dusakabin",
            ModelYolu = "/medya/ucboyut/kapak1glb.glb",
            ModelDosyaYolu = "/medya/ucboyut/kapak1glb.glb",
            DosyaBoyutuByte = 140700,
            VarsayilanMi = true,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });
        await vt.SaveChangesAsync();

        vt.UrunMedyalari.Add(new UrunMedya
        {
            UrunId = dusakabinUrun.Id,
            MedyaUrl = "/medya/ucboyut/kapak1glb.glb",
            MedyaTuru = "3D",
            SiraNo = 2,
            AnaGosterim = false
        });

        var parcaGruplari = new[]
        {
            new UrunParcaGrubu { UrunId = dusakabinUrun.Id, Ad = "Cam Panel", SiraNo = 1, AktifMi = true },
            new UrunParcaGrubu { UrunId = dusakabinUrun.Id, Ad = "Profil", SiraNo = 2, AktifMi = true },
            new UrunParcaGrubu { UrunId = dusakabinUrun.Id, Ad = "Kulp", SiraNo = 3, AktifMi = true }
        };
        vt.UrunParcaGruplari.AddRange(parcaGruplari);
        await vt.SaveChangesAsync();

        vt.UrunKonfigurasyonSablonlari.Add(new UrunKonfigurasyonSablonu
        {
            UrunId = dusakabinUrun.Id,
            Ad = "Dusakabin Sablonu",
            DetaySablonu = "DusakabinKonfigurator",
            HeroAktifMi = true,
            TeknikOzellikAktifMi = true,
            PdfKaynakAktifMi = false,
            BenzerUrunlerAktifMi = true,
            TeklifFormuAktifMi = true,
            UcBoyutIlkAcilacakMi = true,
            AnimasyonTipi = "fade",
            MobilPanelDavranisi = "alt",
            RenkPaneliKonumu = "sag",
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });
        await vt.SaveChangesAsync();

        // --- Urun 3: Banyo Dolabi ---
        var banyoDolabiUrun = new Urun
        {
            Slug = "banyo-dolabi-stil",
            Kod = "BD-001",
            Ad = "Stil Banyo Dolabı",
            KisaAciklama = "5 parça - Ayna, Musluk, Lavabo, Üst & Alt Dolap",
            Aciklama = "Banyo dolaplarımızda 5 özel parça: Ayna yansıma efektli, musluk chrome, lavabo tek parça, üst ve alt dolap 5 renk 5 kaplama seçenekli.",
            UrunAilesiId = 3,
            UrunKategoriId = 2,
            AktifMi = true,
            OneCikanMi = true,
            YeniMi = true,
            Fiyat = 2500m,
            Birim = "adet",
            SiraNo = 3,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        vt.Urunler.Add(banyoDolabiUrun);
        await vt.SaveChangesAsync();

        var banyoMedyaResim = new UrunMedya
        {
            UrunId = banyoDolabiUrun.Id,
            MedyaUrl = "/medya/vizitlink3d_default.png",
            MedyaTuru = "Resim",
            SiraNo = 1,
            AnaGosterim = true
        };
        vt.UrunMedyalari.Add(banyoMedyaResim);
        await vt.SaveChangesAsync();
        banyoDolabiUrun.AnaGorselMedyaId = banyoMedyaResim.Id;

        vt.UrunUcBoyutModelleri.Add(new UrunUcBoyutModeli
        {
            UrunId = banyoDolabiUrun.Id,
            ModelAdi = "Stil Banyo Dolabı",
            ModelYolu = "/models/piedra.glb",
            ModelDosyaYolu = "/models/piedra.glb",
            DosyaBoyutuByte = 15812192,
            VarsayilanMi = true,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });

        var banyoParcaGruplari = new[]
        {
            new UrunParcaGrubu { UrunId = banyoDolabiUrun.Id, Ad = "Ayna", SiraNo = 1, AktifMi = true },
            new UrunParcaGrubu { UrunId = banyoDolabiUrun.Id, Ad = "Musluk", SiraNo = 2, AktifMi = true },
            new UrunParcaGrubu { UrunId = banyoDolabiUrun.Id, Ad = "Lavabo", SiraNo = 3, AktifMi = true },
            new UrunParcaGrubu { UrunId = banyoDolabiUrun.Id, Ad = "Üst Dolap", SiraNo = 4, AktifMi = true },
            new UrunParcaGrubu { UrunId = banyoDolabiUrun.Id, Ad = "Alt Dolap", SiraNo = 5, AktifMi = true }
        };
        vt.UrunParcaGruplari.AddRange(banyoParcaGruplari);
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaDemoIcerikAsync(VizitLink3DDbContext vt)
    {
        var simdi = DateTime.UtcNow;

        await GoldBanyoKatalogUrunleriniTohumlaAsync(vt, simdi);

        if (!await vt.Urunler.AnyAsync(u => u.Slug == "caprice-110-beyaz"))
        {
            var capriceUrun = new Urun
            {
                Slug = "caprice-110-beyaz",
                Kod = "CAPRICE-110-BEYAZ",
                Ad = "Caprice 110 Beyaz",
                KisaAciklama = "Gold Ban-Yom Exclusive Serisi banyo dolabı. Mermer tezgah, lake boya, rölyef desen ve LED aydınlatmalı dokunmatik aç kapa ayna ünitesi.",
                Aciklama = """
<h2>Gold Ban-Yom Exclusive Serisi</h2>
<h3>Caprice 110 Beyaz</h3>
<p>Caprice 110 Beyaz; <strong>mermer tezgah</strong>, <strong>lake boya</strong> ve <strong>rölyef desen</strong> detaylarını bir araya getiren modern banyo dolabı koleksiyonudur.</p>
<h3>Öne Çıkan Özellikler</h3>
<ol>
  <li>Mermer Tezgah</li>
  <li>Lake Boya</li>
  <li>Rölyef Desen</li>
</ol>
<h3>Ayna Ünitesi</h3>
<ol>
  <li>LED Aydınlatma</li>
  <li>Dokunmatik Aç Kapa</li>
</ol>
<h3>LED Aydınlatmalı Dokunmatik Aç Kapa Ayna Ünitesi</h3>
<p>Seriye özel ayna ünitesi, günlük kullanımda ergonomi ve premium görünümü bir araya getirir.</p>
<h3>Caprice 110 cm Banyo Dolabı Boyutları</h3>
<h4>Lavabo Ünitesi</h4>
<ul>
  <li>Genişlik: 110 cm</li>
  <li>Yükseklik: 85 cm (Dolap: 55 cm)</li>
  <li>Derinlik: 46 cm</li>
</ul>
<h4>Ayna Ünitesi</h4>
<ul>
  <li>Genişlik: 55 cm</li>
  <li>Yükseklik: 85 cm</li>
  <li>Derinlik: 6 cm</li>
</ul>
""",
                UrunAilesiId = 3,
                UrunKategoriId = 2,
                AktifMi = true,
                OneCikanMi = true,
                YeniMi = true,
                Birim = "adet",
                SiraNo = 1,
                SeoBaslik = "Caprice 110 Beyaz cm Banyo Dolabı",
                SeoAciklama = "Gold Ban-Yom Exclusive Serisi Caprice 110 Beyaz banyo dolabı. Mermer tezgah, lake boya, rölyef desen ve LED aydınlatmalı dokunmatik aç kapa ayna ünitesi.",
                OlusturulmaTarihi = simdi
            };

            vt.Urunler.Add(capriceUrun);
            await vt.SaveChangesAsync();

            vt.UrunMedyalari.AddRange(
                new UrunMedya
                {
                    UrunId = capriceUrun.Id,
                    MedyaUrl = "https://www.goldbanyom.com.tr/wp-content/uploads/2024/11/gold-banyom-40.jpg",
                    MedyaTuru = "Resim",
                    Aciklama = "Caprice 110 Beyaz ön görünüm",
                    SiraNo = 1,
                    AnaGosterim = true
                },
                new UrunMedya
                {
                    UrunId = capriceUrun.Id,
                    MedyaUrl = "https://www.goldbanyom.com.tr/wp-content/uploads/2024/11/gold-banyom-42.jpg",
                    MedyaTuru = "Resim",
                    Aciklama = "Caprice 110 Beyaz detay görünümü",
                    SiraNo = 2,
                    AnaGosterim = false
                },
                new UrunMedya
                {
                    UrunId = capriceUrun.Id,
                    MedyaUrl = "https://www.goldbanyom.com.tr/wp-content/uploads/2024/11/gold-banyom-39.jpg",
                    MedyaTuru = "Resim",
                    Aciklama = "Caprice 110 Beyaz lavabo ünitesi",
                    SiraNo = 3,
                    AnaGosterim = false
                },
                new UrunMedya
                {
                    UrunId = capriceUrun.Id,
                    MedyaUrl = "https://www.goldbanyom.com.tr/wp-content/uploads/2024/11/gold-banyom-43.jpg",
                    MedyaTuru = "Resim",
                    Aciklama = "Caprice 110 Beyaz ayna ünitesi",
                    SiraNo = 4,
                    AnaGosterim = false
                }
            );

            await vt.SaveChangesAsync();
        }

        if (!await vt.Haberler.AnyAsync(b => b.Baslik == "Mutfak Dolabı Seçerken Dikkat Edilmesi Gereken 5 Şey"))
        {
            vt.Haberler.AddRange(
                new HaberYazisi { Baslik = "Mutfak Dolabı Seçerken Dikkat Edilmesi Gereken 5 Şey", Ozet = "Doğru malzeme ve tasarım seçimi ile yıllarca sorunsuz kullanabilirsiniz.", Icerik = "<p>Mutfak dolapları günlük hayatın yoğun temposuna dayanıklı olmalıdır. <strong>Membran kapak</strong> nem ve ısıya karşı üstün direnç gösterirken, <strong>akrilik kapak</strong> modern ve parlak görünümüyle öne çıkar.</p><h3>Seçim Kriterleri</h3><ul><li>Kullanım sıklığı ve aile büyüklüğü</li><li>Mutfak nem oranı ve havalandırma</li><li>Bütçe ve estetik tercihler</li></ul><p>VizitLink3D olarak 32 yıllık tecrübemizle size en uygun çözümü sunuyoruz.</p>", Slug = "mutfak-dolabi-secimi", AnaResimUrl = "/medya/vizitlink3d_default.png", Etiketler = "mutfak,dolap,membran,lake", YayinTarihi = simdi.AddDays(-30), AktifMi = true },
                new HaberYazisi { Baslik = "2025 Kapı Trendleri: Endüstriyel Tasarım Yükseliyor", Ozet = "Minimalizm ve endüstriyel estetik 2025'in öne çıkan kapı tasarım trendleri arasında.", Icerik = "<p>2025 yılı kapı tasarımında <strong>endüstriyel lüks</strong> akımı hakim. Siyah mat yüzeyler, bronz kulplar ve minimalist çizgiler bu yılın vazgeçilmezleri arasında.</p><h3>Öne Çıkan Trendler</h3><ul><li>Gizli menteşe ve sıfır kapı çerçevesi</li><li>Akıllı kilit sistemleri</li><li>Ses yalıtımlı özel kapılar</li><li>Doğal ahşap ve metal kombinasyonları</li></ul>", Slug = "2025-kapi-trendleri", AnaResimUrl = "/medya/vizitlink3d_default.png", Etiketler = "kapı,trend,2025,endüstriyel", YayinTarihi = simdi.AddDays(-20), AktifMi = true },
                new HaberYazisi { Baslik = "Lake Kapak mı Akrilik Kapak mı? Karşılaştırma Rehberi", Ozet = "İki popüler kapak türü arasındaki farkları detaylıca inceledik.", Icerik = "<p>Lake kapaklar mat ve şık bir görünüm sunarken, akrilik (high gloss) kapaklar ayna gibi parlak yüzeyleriyle dikkat çeker.</p><h3>Lake Kapak</h3><ul><li>Çizilmelere karşı dayanıklı</li><li>Mat ve elegant görünüm</li><li>Geniş renk seçeneği</li></ul><h3>Akrilik Kapak</h3><ul><li>Yüksek parlaklık ve derinlik</li><li>Leke tutmaz yüzey</li><li>Modern mutfaklar için ideal</li></ul>", Slug = "lake-vs-akrilik", AnaResimUrl = "/medya/vizitlink3d_default.png", Etiketler = "lake,akrilik,kapak,mutfak", YayinTarihi = simdi.AddDays(-15), AktifMi = true },
                new HaberYazisi { Baslik = "Banyo Dolabı Seçiminde Suya Dayanıklılık Neden Önemli?", Ozet = "Banyo ortamı yüksek nem içerir. Yanlış malzeme seçimi 1 yıl içinde deformasyona yol açabilir.", Icerik = "<p>Banyo dolaplarında <strong>suya dayanıklılık</strong> en kritik faktördür. VizitLink3D'un özel su itici kaplama teknolojisi sayesinde dolaplarınız yıllarca ilk günkü görünümünü korur.</p>", Slug = "banyo-dolabi-su", AnaResimUrl = "/medya/vizitlink3d_default.png", Etiketler = "banyo,dolap,su,dayanıklılık", YayinTarihi = simdi.AddDays(-10), AktifMi = true },
                new HaberYazisi { Baslik = "Villa Kapısı Seçimi: Güvenlik ve Estetik Bir Arada", Ozet = "Villa kapıları hem güvenlik hem de estetik açıdan özel bir yaklaşım gerektirir.", Icerik = "<p>Villa kapıları, evinizin <strong>ilk izlenimi</strong> ve <strong>güvenlik kalkanıdır</strong>. VizitLink3D'un özel tasarım villa kapıları, çelik takviyeli yapısıyla maksimum koruma sağlar.</p>", Slug = "villa-kapisi-secimi", AnaResimUrl = "/medya/vizitlink3d_default.png", Etiketler = "villa,kapı,güvenlik,lüks", YayinTarihi = simdi.AddDays(-5), AktifMi = true },
                new HaberYazisi { Baslik = "Endüstriyel Tasarım Nedir? Mutfak ve Kapıda Modern Dokunuş", Ozet = "Ham malzemeler, keskin hatlar ve işlevsellik. Endüstriyel tasarım akımı ev dekorasyonunda yükselişte.", Icerik = "<p><strong>Endüstriyel tasarım</strong>, fabrika estetiğini yaşam alanlarına taşır. Çelik, beton ve ahşap gibi ham malzemelerin uyumu, modern ve karakterli mekanlar yaratır.</p>", Slug = "endustriyel-tasarim", AnaResimUrl = "/medya/vizitlink3d_default.png", Etiketler = "endüstriyel,tasarım,modern", YayinTarihi = simdi.AddDays(-3), AktifMi = true },
                new HaberYazisi { Baslik = "Enerji Tasarruflu Kapı Sistemleri ile Isı Kaybını Azaltın", Ozet = "Doğru kapı seçimi ile kış aylarında ciddi enerji tasarrufu sağlayabilirsiniz.", Icerik = "<p>Yalıtımlı kapı sistemleri, özellikle müstakil evlerde ve villalarda <strong>enerji faturalarını %40'a kadar düşürebilir</strong>.</p>", Slug = "enerji-tasarruflu-kapi", AnaResimUrl = "/medya/vizitlink3d_default.png", Etiketler = "enerji,tasarruf,kapı,ısı", YayinTarihi = simdi.AddDays(-1), AktifMi = true },
                new HaberYazisi { Baslik = "Ofis Mobilyalarında Kapak Seçimi: Profesyonel Görünüm İçin İpuçları", Ozet = "Ofis dolapları ve kapakları, kurumsal kimliğinizin bir parçasıdır.", Icerik = "<p>Ofis mobilyalarında <strong>dayanıklılık ve şıklık</strong> ön plandadır. VizitLink3D'un ofis koleksiyonu, yoğun kullanıma uygun laminat kaplamaları ve sessiz kapanma sistemleriyle fark yaratır.</p>", Slug = "ofis-mobilya-kapak", AnaResimUrl = "/medya/vizitlink3d_default.png", Etiketler = "ofis,mobilya,kapak", YayinTarihi = simdi.AddDays(-8), AktifMi = true }
            );
            await vt.SaveChangesAsync();
        }

        if (!await vt.Projeler.AnyAsync(p => p.Baslik == "Bursa Nilüfer Villa Mutfağı"))
        {
            vt.Projeler.AddRange(
                new Proje { Baslik = "Bursa Nilüfer Villa Mutfağı", Aciklama = "Özel tasarım akrilik kapaklı mutfak. 25 mÂ² alanda tamamen kişiye özel üretim.", KisaAciklama = "Akrilik kapaklı lüks villa mutfağı", KategoriId = 1, MusteriSehir = "Bursa", ProjeTarihi = new DateTime(2025, 1, 15), KapakResim = "/medya/vizitlink3d_default.png", OneCikanMi = true, SiraNo = 4, AktifMi = true, Slug = "bursa-villa-mutfagi" },
                new Proje { Baslik = "İstanbul Levent Ofis Projesi", Aciklama = "Kurumsal ofis için laminat kapaklı dolap sistemleri. 40 adet özel ölçü dolap.", KisaAciklama = "Laminat kapaklı kurumsal ofis dolapları", KategoriId = 4, MusteriSehir = "İstanbul", ProjeTarihi = new DateTime(2025, 2, 20), KapakResim = "/medya/vizitlink3d_default.png", OneCikanMi = true, SiraNo = 5, AktifMi = true, Slug = "istanbul-ofis" },
                new Proje { Baslik = "Ankara Çankaya Lüks Banyo", Aciklama = "Suya dayanıklı özel kaplama banyo dolapları. LED aydınlatmalı ayna sistemleri.", KisaAciklama = "Lüks banyo dolap ve ayna sistemleri", KategoriId = 2, MusteriSehir = "Ankara", ProjeTarihi = new DateTime(2025, 3, 10), KapakResim = "/medya/vizitlink3d_default.png", OneCikanMi = false, SiraNo = 6, AktifMi = true, Slug = "ankara-banyo" },
                new Proje { Baslik = "İzmir Urla Yazlık Villa", Aciklama = "Ege mimarisine uygun klasik kapı ve mutfak tasarımı. Doğal ahşap görünümlü membran kapaklar.", KisaAciklama = "Klasik tarz yazlık villa renovasyon", KategoriId = 1, MusteriSehir = "İzmir", ProjeTarihi = new DateTime(2024, 8, 5), KapakResim = "/medya/vizitlink3d_default.png", OneCikanMi = true, SiraNo = 7, AktifMi = true, Slug = "izmir-villa" },
                new Proje { Baslik = "Antalya Lara Otel Projesi", Aciklama = "120 odalı otel için lake kapı ve dolap sistemleri. Yangına dayanıklı özel malzeme.", KisaAciklama = "Otel odası kapı ve dolap sistemleri", KategoriId = 4, MusteriSehir = "Antalya", ProjeTarihi = new DateTime(2024, 6, 1), KapakResim = "/medya/vizitlink3d_default.png", OneCikanMi = false, SiraNo = 8, AktifMi = true, Slug = "antalya-otel" },
                new Proje { Baslik = "Eskişehir Modern Ofis", Aciklama = "Genç girişim ofisi için renkli membran kapaklı depolama üniteleri.", KisaAciklama = "Modüler ofis depolama sistemleri", KategoriId = 4, MusteriSehir = "Eskişehir", ProjeTarihi = new DateTime(2025, 4, 1), KapakResim = "/medya/vizitlink3d_default.png", OneCikanMi = false, SiraNo = 9, AktifMi = true, Slug = "eskisehir-ofis" }
            );
            await vt.SaveChangesAsync();
        }

        if (!await vt.MusteriYorumlari.AnyAsync(y => y.MusteriAdi == "Fatma E."))
        {
            vt.MusteriYorumlari.AddRange(
                new MusteriYorumu { MusteriAdi = "Fatma E.", MusteriSehir = "Bursa", Yorum = "Villa kapımızı 3 yıl önce yaptırdık, hala ilk günkü gibi. Ses yalıtımı harika.", Puan = 5, Onaylandi = true, SiraNo = 6 },
                new MusteriYorumu { MusteriAdi = "Kemal S.", MusteriSehir = "İstanbul", Yorum = "Ofis dolaplarımızın kalitesi tüm çalışanlarımızdan tam not aldı.", Puan = 5, Onaylandi = true, SiraNo = 7 },
                new MusteriYorumu { MusteriAdi = "Sevgi T.", MusteriSehir = "Antalya", Yorum = "Otel projemiz için 120 oda sipariş verdik. Zamanında teslim ve kusursuz işçilik.", Puan = 5, Onaylandi = true, SiraNo = 8 },
                new MusteriYorumu { MusteriAdi = "Okan B.", MusteriSehir = "İzmir", Yorum = "Mutfak dolaplarım için renk seçiminde çok yardımcı oldular. Sonuç mükemmel.", Puan = 5, Onaylandi = true, SiraNo = 9 },
                new MusteriYorumu { MusteriAdi = "Derya K.", MusteriSehir = "Ankara", Yorum = "Banyo dolabımızdaki su yalıtımı gerçekten etkileyici. 2 yıldır sorunsuz.", Puan = 4, Onaylandi = true, SiraNo = 10 },
                new MusteriYorumu { MusteriAdi = "Burak M.", MusteriSehir = "Eskişehir", Yorum = "Fiyat/performans olarak piyasadaki en iyi seçenek. Montaj ekibi çok profesyoneldi.", Puan = 5, Onaylandi = true, SiraNo = 11 },
                new MusteriYorumu { MusteriAdi = "Gülşen A.", MusteriSehir = "Kocaeli", Yorum = "3D konfigüratör sayesinde tam istediğim gibi bir mutfak tasarladık.", Puan = 5, Onaylandi = true, SiraNo = 12 },
                new MusteriYorumu { MusteriAdi = "Volkan Ç.", MusteriSehir = "Balıkesir", Yorum = "Lake kapakların mat dokusu çok şık. Temizliği de çok kolay.", Puan = 4, Onaylandi = true, SiraNo = 13 }
            );
            await vt.SaveChangesAsync();
        }

        if (!await vt.Referanslar.AnyAsync(r => r.Ad == "Borusan Holding"))
        {
            vt.Referanslar.AddRange(
                new Referans { Ad = "Borusan Holding", Tip = "Musteri", SiraNo = 11 },
                new Referans { Ad = "LC Waikiki", Tip = "Musteri", SiraNo = 12 },
                new Referans { Ad = "Türk Telekom", Tip = "Musteri", SiraNo = 13 },
                new Referans { Ad = "Vodafone", Tip = "Musteri", SiraNo = 14 },
                new Referans { Ad = "Migros", Tip = "Musteri", SiraNo = 15 },
                new Referans { Ad = "Yapı Kredi", Tip = "Musteri", SiraNo = 16 },
                new Referans { Ad = "Tübitak", Tip = "Kurumsal", SiraNo = 17 },
                new Referans { Ad = "İstanbul Büyükşehir", Tip = "Kurumsal", SiraNo = 18 }
            );
            await vt.SaveChangesAsync();
        }

        if (!await vt.SikSorulanSorular.AnyAsync(s => s.Soru == "Kapak ölçülerini kendim mi vermeliyim?"))
        {
            vt.SikSorulanSorular.AddRange(
                new SikSorulanSoru { Soru = "Kapak ölçülerini kendim mi vermeliyim?", Cevap = "Size en yakın showroom'umuzda ücretsiz keşif ve ölçü hizmeti sunuyoruz. Dilerseniz kendi ölçülerinizi de iletebilirsiniz.", SiraNo = 11, AktifMi = true },
                new SikSorulanSoru { Soru = "Teslimat süresi ne kadar?", Cevap = "Standart ölçülerde 5-7 iş günü, özel ölçü siparişlerde 10-15 iş günü içerisinde teslimat yapılmaktadır.", SiraNo = 12, AktifMi = true },
                new SikSorulanSoru { Soru = "Garanti süreniz nedir?", Cevap = "Tüm ürünlerimiz 2 yıl garanti kapsamındadır. Membran kapaklarda renk solması, lake kapaklarda çatlama garantisi veriyoruz.", SiraNo = 13, AktifMi = true },
                new SikSorulanSoru { Soru = "Fiyatlarınız neye göre belirleniyor?", Cevap = "Fiyatlar malzeme türü (membran/lake/akrilik), ölçü, profil detayı ve aksesuar seçimine göre değişmektedir. Ücretsiz fiyat teklifi için iletişime geçebilirsiniz.", SiraNo = 14, AktifMi = true },
                new SikSorulanSoru { Soru = "Montaj hizmetiniz var mı?", Cevap = "Evet, Bursa ve çevre illerde profesyonel montaj ekibimizle hizmet vermekteyiz. Diğer iller için anlaşmalı ekiplerimiz bulunmaktadır.", SiraNo = 15, AktifMi = true },
                new SikSorulanSoru { Soru = "Yurt dışına gönderim yapıyor musunuz?", Cevap = "Evet, 20'den fazla ülkeye ihracat yapmaktayız. Avrupa, Orta Doğu ve Kuzey Afrika'ya düzenli sevkiyatımız bulunmaktadır.", SiraNo = 16, AktifMi = true },
                new SikSorulanSoru { Soru = "Taksit imkanınız var mı?", Cevap = "Anlaşmalı bankalarımız aracılığıyla 12 aya varan taksit imkanı sunuyoruz.", SiraNo = 17, AktifMi = true },
                new SikSorulanSoru { Soru = "Renk değişimi sonradan yapılabilir mi?", Cevap = "Kapaklar değiştirilebilir ancak dolap gövdesinden bağımsız olarak düşünülmelidir. Yeni kapak siparişi vererek mevcut dolaplarınızı yenileyebilirsiniz.", SiraNo = 18, AktifMi = true }
            );
            await vt.SaveChangesAsync();
        }

        if (!await vt.Sertifikalar.AnyAsync())
        {
            vt.Sertifikalar.AddRange(
                new Sertifika { Ad = "ISO 9001:2015", Aciklama = "Kalite Yönetim Sistemi Belgesi", Resim = "medya/sertifikalar/iso.png", VerenKurum = "TÜV Rheinland", SiraNo = 1, AktifMi = true },
                new Sertifika { Ad = "TSE Belgesi", Aciklama = "Türk Standartları Enstitüsü Uygunluk Belgesi", Resim = "medya/sertifikalar/tse.png", VerenKurum = "TSE", SiraNo = 2, AktifMi = true },
                new Sertifika { Ad = "CE İşareti", Aciklama = "Avrupa Birliği Uygunluk İşareti", Resim = "medya/sertifikalar/ce.png", VerenKurum = "Avrupa Komisyonu", SiraNo = 3, AktifMi = true },
                new Sertifika { Ad = "FSC Sertifikası", Aciklama = "Sürdürülebilir Orman Yönetimi Sertifikası", Resim = "medya/sertifikalar/fsc.png", VerenKurum = "FSC International", SiraNo = 4, AktifMi = true }
            );
            await vt.SaveChangesAsync();
        }

        var yuklenecekSertifikalar = new[]
        {
            new Sertifika { Ad = "Yangına Dayanıklı Kapı Sertifikası", Aciklama = "Yangına dayanıklı kapı uygunluk belgesi.", PdfDosya = "/medya/sertifikalar/yangina-dayanikli-kapi-sertifika.pdf", VerenKurum = "Belgelendirme Kurumu", SiraNo = 10, AktifMi = true },
            new Sertifika { Ad = "Sertifika IMG 20260430 0001", Aciklama = "VizitLink3D sertifika dokümanı.", PdfDosya = "/medya/sertifikalar/img-20260430-0001.pdf", VerenKurum = "Belgelendirme Kurumu", SiraNo = 11, AktifMi = true },
            new Sertifika { Ad = "Sertifika IMG 20260430 0002", Aciklama = "VizitLink3D sertifika dokümanı.", PdfDosya = "/medya/sertifikalar/img-20260430-0002.pdf", VerenKurum = "Belgelendirme Kurumu", SiraNo = 12, AktifMi = true },
            new Sertifika { Ad = "Sertifika IMG 20260430 0003", Aciklama = "VizitLink3D sertifika dokümanı.", PdfDosya = "/medya/sertifikalar/img-20260430-0003.pdf", VerenKurum = "Belgelendirme Kurumu", SiraNo = 13, AktifMi = true }
        };

        foreach (var sertifika in yuklenecekSertifikalar)
        {
            if (!await vt.Sertifikalar.AnyAsync(s => s.Ad == sertifika.Ad))
            {
                vt.Sertifikalar.Add(sertifika);
            }
        }

        await vt.SaveChangesAsync();

        if (!await vt.IletisimMesajlari.AnyAsync(m => m.Eposta == "hakan@ornek.com"))
        {
            vt.IletisimMesajlari.AddRange(
                new IletisimMesaji { AdSoyad = "Hakan Yıldız", Eposta = "hakan@ornek.com", Telefon = "05321112233", Konu = "Fiyat Teklifi", Mesaj = "Merhaba, yeni evimizin mutfağı için akrilik kapak fiyat teklifi almak istiyorum. 12 mÂ² mutfak için membran ve akrilik alternatifli fiyat rica ediyorum." },
                new IletisimMesaji { AdSoyad = "Sema Koç", Eposta = "sema@ornek.com", Telefon = "05553334455", Konu = "Ürün Bilgisi", Mesaj = "Banyo dolaplarınızın suya dayanıklılık özellikleri hakkında detaylı bilgi alabilir miyim?" },
                new IletisimMesaji { AdSoyad = "Murat Tekin", Eposta = "murat@ornek.com", Telefon = "05445556677", Konu = "Bayilik Başvurusu", Mesaj = "VizitLink3D ürünlerini Antalya'da satmak istiyorum. Bayilik şartlarınız hakkında bilgi alabilir miyim?" },
                new IletisimMesaji { AdSoyad = "Gamze Arslan", Eposta = "gamze@ornek.com", Konu = "Renk Numunesi", Mesaj = "RAL renk kataloğundan 7016 ve 9005 kodlu renklerin fiziksel numunelerini talep ediyorum." },
                new IletisimMesaji { AdSoyad = "Ahmet Can", Eposta = "ahmet@ornek.com", Telefon = "05339876543", Konu = "Kurumsal Teklif", Mesaj = "Firmamız için 40 adet ofis dolabı ve 10 adet kapı sistemine ihtiyacımız var. Kurumsal fiyat teklifi rica ederim." }
            );
            await vt.SaveChangesAsync();
        }

        var gercekEkip = new List<EkipUyesi>
        {
            new() { AdSoyad = "Halil Barut",    Unvan = "Yönetim Kurulu Başkanı",    Bio = "", Resim = "/medya/goldbanyo/showroom.jpg", SiraNo = 1, AktifMi = true },
            new() { AdSoyad = "Mustafa Barut",  Unvan = "Operasyon Müdürü",          Bio = "", Resim = "/medya/goldbanyo/showroom.jpg", SiraNo = 2, AktifMi = true },
            new() { AdSoyad = "Yılmaz Birdane", Unvan = "Mimar",                     Bio = "", Resim = "/medya/goldbanyo/showroom.jpg", SiraNo = 3, AktifMi = true },
            new() { AdSoyad = "Ramazan Çetin",  Unvan = "Satış & Pazarlama Müdürü", Bio = "", Resim = "/medya/goldbanyo/showroom.jpg", SiraNo = 4, AktifMi = true }
        };

        foreach (var uye in gercekEkip)
        {
            var mevcut = await vt.EkipUyeleri.FirstOrDefaultAsync(e => e.AdSoyad == uye.AdSoyad);
            if (mevcut == null)
                vt.EkipUyeleri.Add(uye);
            else
            {
                mevcut.Unvan = uye.Unvan;
                mevcut.Resim = uye.Resim;
                mevcut.SiraNo = uye.SiraNo;
                mevcut.AktifMi = true;
            }
        }
        await vt.SaveChangesAsync();
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // EKSIK SEED METOTLARI
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    private static async Task TohumlaBultenAboneleriniAsync(VizitLink3DDbContext vt)
    {
        var simdi = DateTime.UtcNow;
        vt.BultenAboneleri.AddRange(
            new BultenAbonesi { Eposta = "ornek1@mail.com", AdSoyad = "Ahmet Yilmaz", AbonelikTarihi = simdi.AddDays(-30), AktifMi = true, DogrulandiMi = true, KaynakSayfa = "footer" },
            new BultenAbonesi { Eposta = "ornek2@mail.com", AdSoyad = "Ayse Kaya", AbonelikTarihi = simdi.AddDays(-20), AktifMi = true, DogrulandiMi = true, KaynakSayfa = "anasayfa" },
            new BultenAbonesi { Eposta = "ornek3@mail.com", AdSoyad = "Mehmet Demir", AbonelikTarihi = simdi.AddDays(-10), AktifMi = true, DogrulandiMi = true, KaynakSayfa = "haber" },
            new BultenAbonesi { Eposta = "ornek4@mail.com", AbonelikTarihi = simdi.AddDays(-5), AktifMi = true, DogrulandiMi = false, KaynakSayfa = "iletisim" }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaEpostaSablonlariniAsync(VizitLink3DDbContext vt)
    {
        var simdi = DateTime.UtcNow;
        vt.EpostaSablonlari.AddRange(
            new EpostaSablonu { Ad = "Hoş Geldiniz", Konu = "VizitLink3D'a Hoş Geldiniz!", IcerikHtml = "<h1>Hoş Geldiniz</h1><p>VizitLink3D ailesine katıldığınız için teşekkür ederiz.</p>", Tip = "HosGeldin", AktifMi = true, OlusturulmaTarihi = simdi },
            new EpostaSablonu { Ad = "İletişim Cevabı", Konu = "Mesajınız Alındı — VizitLink3D", IcerikHtml = "<p>Mesajınız başarıyla alındı. En kısa sürede dönüş yapacağız.</p>", Tip = "IletisimCevabi", AktifMi = true, OlusturulmaTarihi = simdi },
            new EpostaSablonu { Ad = "Teklif Cevabı", Konu = "Teklif Talebiniz Alındı", IcerikHtml = "<p>Teklif talebiniz değerlendiriliyor. 24 saat içinde dönüş yapılacaktır.</p>", Tip = "IletisimCevabi", AktifMi = true, OlusturulmaTarihi = simdi }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaTeklifIstekleriniAsync(VizitLink3DDbContext vt)
    {
        var simdi = DateTime.UtcNow;
        vt.TeklifIstekleri.AddRange(
            new TeklifIstegi { UrunId = 1, MusteriAdSoyad = "Kemal Yildiz", Eposta = "kemal@ornek.com", Telefon = "05321112233", Not = "Beyaz renk istiyorum.", Durum = "Bekliyor", OlusturulmaTarihi = simdi.AddDays(-7) },
            new TeklifIstegi { UrunId = 4, MusteriAdSoyad = "Selin Arslan", Eposta = "selin@ornek.com", Telefon = "05443332211", Not = "Acil kurulum gerekiyor.", Durum = "Inceleniyor", OlusturulmaTarihi = simdi.AddDays(-3) },
            new TeklifIstegi { MusteriAdSoyad = "Caner Ozturk", Eposta = "caner@ornek.com", Not = "Villa için tüm kapılar.", Durum = "Bekliyor", OlusturulmaTarihi = simdi.AddDays(-1) }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task GoldBanyoKatalogUrunleriniTohumlaAsync(VizitLink3DDbContext vt, DateTime simdi)
    {
        foreach (var katalogUrunu in GoldBanyoKatalogUrunleri.Tum)
        {
            var urun = await vt.Urunler.FirstOrDefaultAsync(u => u.Slug == katalogUrunu.Slug);
            if (urun == null)
            {
                urun = new Urun
                {
                    Slug = katalogUrunu.Slug,
                    OlusturulmaTarihi = simdi
                };
                vt.Urunler.Add(urun);
            }

            urun.Kod = katalogUrunu.Kod;
            urun.Ad = katalogUrunu.Ad;
            urun.KisaAciklama = katalogUrunu.KisaAciklamaOlustur();
            urun.Aciklama = katalogUrunu.AciklamaHtmlOlustur();
            urun.UrunAilesiId = 3;
            urun.UrunKategoriId = 2;
            urun.AktifMi = true;
            urun.OneCikanMi = katalogUrunu.OneCikanMi;
            urun.YeniMi = katalogUrunu.YeniMi;
            urun.Fiyat = katalogUrunu.Fiyat;
            urun.Birim = "adet";
            urun.SiraNo = katalogUrunu.SayfaNo;
            urun.SeoBaslik = $"{katalogUrunu.Ad} | Gold Banyo 2026 Katalog";
            urun.SeoAciklama = katalogUrunu.KisaAciklamaOlustur();
            urun.GuncellenmeTarihi = simdi;

            await vt.SaveChangesAsync();

            var medyaKuyrugu = new List<(string Url, bool Ana, int Sira)>
            {
                (katalogUrunu.HeroGorselUrl, true, 1),
                (katalogUrunu.KatalogGorselUrl, false, 2),
                (katalogUrunu.TeknikGorselUrl, false, 3)
            };

            medyaKuyrugu.AddRange(
                katalogUrunu.EkGaleriSayfalari.Select((sayfa, index) => (
                    Url: $"/medya/gold-katalog/sayfa-{sayfa:000}-spread.png",
                    Ana: false,
                    Sira: index + 4)));

            UrunMedya? heroMedya = null;

            foreach (var medya in medyaKuyrugu)
            {
                var mevcutMedya = await vt.UrunMedyalari
                    .FirstOrDefaultAsync(m => m.UrunId == urun.Id && m.MedyaUrl == medya.Url);

                if (mevcutMedya == null)
                {
                    mevcutMedya = new UrunMedya
                    {
                        UrunId = urun.Id,
                        MedyaUrl = medya.Url,
                        MedyaTuru = "Resim",
                        Aciklama = $"{katalogUrunu.Ad} katalog görseli",
                        SiraNo = medya.Sira,
                        AnaGosterim = medya.Ana
                    };
                    vt.UrunMedyalari.Add(mevcutMedya);
                }
                else
                {
                    mevcutMedya.SiraNo = medya.Sira;
                    mevcutMedya.AnaGosterim = medya.Ana;
                    mevcutMedya.Aciklama = $"{katalogUrunu.Ad} katalog görseli";
                }

                if (medya.Ana)
                    heroMedya = mevcutMedya;
            }

            await vt.SaveChangesAsync();

            if (heroMedya != null)
            {
                urun.AnaGorselMedyaId = heroMedya.Id;
                await vt.SaveChangesAsync();
            }
        }
    }

    private static async Task TohumlaSubeleriAsync(VizitLink3DDbContext vt)
    {
        var simdi = DateTime.UtcNow;
        vt.Subeler.AddRange(
            new Sube { Ad = "Gold Banyo Bursa Merkez", Adres = "Çalı Mah. Ömer Biltekin Bulv. No:3/1A Nilüfer", Sehir = "Bursa", Ilce = "Nilüfer", Telefon = "+90 224 482 24 00", Eposta = "info@goldbanyom.com.tr", CalismaSaatleri = "09:00 - 18:00", Enlem = 40.225, Boylam = 28.854, SiraNo = 1, AktifMi = true, OlusturulmaTarihi = simdi, Aciklama = "Gold Banyo üretim ve satış merkezi." },
            new Sube { Ad = "Gold Banyo Proje Hatti", Adres = "Çalı Mah. Ömer Biltekin Bulv. No:3/1A Nilüfer", Sehir = "Bursa", Ilce = "Nilüfer", Telefon = "+90 533 597 32 14", Eposta = "info@goldbanyom.com.tr", CalismaSaatleri = "09:00 - 18:00", Enlem = 40.225, Boylam = 28.854, SiraNo = 2, AktifMi = true, OlusturulmaTarihi = simdi, Aciklama = "Toplu proje ve kurumsal talepler için iletişim hattı." }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaAISaglayicilariniAsync(VizitLink3DDbContext vt)
    {
        var simdi = DateTime.UtcNow;
        vt.AISaglayicilari.AddRange(
            new AISaglayicisi { Tip = AISaglayiciTipi.LlamaLocal, Ad = "LlamaLocal — LM Studio", Model = "llama3.2:3b", ApiKeyEncrypted = "http://127.0.0.1:11434", AylikLimitUsd = 0, KullanilanUsd = 0, SonSifirlamaTarihi = new DateTime(simdi.Year, simdi.Month, 1), AktifMi = true, SiraNo = 1, OlusturulmaTarihi = simdi },
            new AISaglayicisi { Tip = AISaglayiciTipi.GoogleTranslate, Ad = "Google Cloud Translation", Model = "translate-v2", ApiKeyEncrypted = "", AylikLimitUsd = 10m, KullanilanUsd = 0, SonSifirlamaTarihi = new DateTime(simdi.Year, simdi.Month, 1), AktifMi = false, SiraNo = 2, OlusturulmaTarihi = simdi },
            new AISaglayicisi { Tip = AISaglayiciTipi.OpenAI, Ad = "OpenAI GPT-4o-mini", Model = "gpt-4o-mini", ApiKeyEncrypted = "", AylikLimitUsd = 100m, KullanilanUsd = 0, SonSifirlamaTarihi = new DateTime(simdi.Year, simdi.Month, 1), AktifMi = false, SiraNo = 3, OlusturulmaTarihi = simdi }
        );
        await vt.SaveChangesAsync();
    }

    private static async Task GoldBanyoGecisDuzeltmeleriAsync(VizitLink3DDbContext vt)
    {
        const string katalogYolu = "medya/gold-katalog/GOLD-2026-KATALOG.pdf";
        var degisti = false;

        var markaYiliKayitlari = await vt.Ceviriler
            .Where(x => x.Anahtar == "admin.giris.markaYil")
            .ToListAsync();
        foreach (var kayit in markaYiliKayitlari)
        {
            if (kayit.Deger == "GOLD BANYO 1989")
                continue;

            kayit.Deger = "GOLD BANYO 1989";
            kayit.GuncellenmeTarihi = DateTime.UtcNow;
            degisti = true;
        }

        var adminBaslikKayitlari = await vt.Ceviriler
            .Where(x => x.Anahtar == "admin.baslik")
            .ToListAsync();
        foreach (var kayit in adminBaslikKayitlari)
        {
            var hedefDeger = kayit.Dil == "en"
                ? "Gold Banyo Admin"
                : "Gold Banyo Yönetim";

            if (kayit.Deger == hedefDeger)
                continue;

            kayit.Deger = hedefDeger;
            kayit.GuncellenmeTarihi = DateTime.UtcNow;
            degisti = true;
        }

        var sloganIkiKayitlari = await vt.Ceviriler
            .Where(x => x.Anahtar == "admin.giris.slogan2")
            .ToListAsync();
        foreach (var kayit in sloganIkiKayitlari)
        {
            var hedefDeger = kayit.Dil == "en"
                ? "Signature Bathrooms"
                : "Özel Banyolar";

            if (kayit.Deger == hedefDeger)
                continue;

            kayit.Deger = hedefDeger;
            kayit.GuncellenmeTarihi = DateTime.UtcNow;
            degisti = true;
        }

        var mirasKayitlari = await vt.Ceviriler
            .Where(x => x.Anahtar == "admin.giris.mirasAciklama")
            .ToListAsync();
        foreach (var kayit in mirasKayitlari)
        {
            var hedefDeger = kayit.Dil == "en"
                ? "We blend deep manufacturing experience in premium bathroom furniture with modern collections."
                : "Premium banyo mobilyalarında köklü üretim deneyimini modern koleksiyonlarla buluşturuyoruz.";

            if (kayit.Deger == hedefDeger)
                continue;

            kayit.Deger = hedefDeger;
            kayit.GuncellenmeTarihi = DateTime.UtcNow;
            degisti = true;
        }

        var girisSlogan1Kayitlari = await vt.Ceviriler
            .Where(x => x.Anahtar == "admin.giris.slogan1")
            .ToListAsync();
        foreach (var kayit in girisSlogan1Kayitlari)
        {
            var hedefDeger = kayit.Dil == "en" ? "For Every Space" : "Her Mekana";

            if (kayit.Deger == hedefDeger)
                continue;

            kayit.Deger = hedefDeger;
            kayit.GuncellenmeTarihi = DateTime.UtcNow;
            degisti = true;
        }

        var girisKoleksiyonEtiketiAnahtarlari = new[]
        {
            "admin.giris.yilTecrube",
            "admin.giris.ozgunModel",
            "admin.giris.ulkeIhracat"
        };
        var girisKoleksiyonEtiketiKayitlari = await vt.Ceviriler
            .Where(x => girisKoleksiyonEtiketiAnahtarlari.Contains(x.Anahtar))
            .ToListAsync();
        foreach (var kayit in girisKoleksiyonEtiketiKayitlari)
        {
            var hedefDeger = kayit.Dil == "en" ? "COLLECTION" : "KOLEKSİYON";

            if (kayit.Deger == hedefDeger)
                continue;

            kayit.Deger = hedefDeger;
            kayit.GuncellenmeTarihi = DateTime.UtcNow;
            degisti = true;
        }

        var anaSayfaBaslikKayitlari = await vt.Ceviriler
            .Where(x => x.Anahtar == "ana_sayfa_title")
            .ToListAsync();
        foreach (var kayit in anaSayfaBaslikKayitlari)
        {
            var hedefDeger = kayit.Dil == "en"
                ? "Gold Banyo | Bathroom Furniture and Collections"
                : "Gold Banyo | Banyo Mobilyalari ve Koleksiyonlari";

            if (kayit.Deger == hedefDeger)
                continue;

            kayit.Deger = hedefDeger;
            kayit.GuncellenmeTarihi = DateTime.UtcNow;
            degisti = true;
        }

        var katalogBaglantilari = await vt.SayfaIcerikleri
            .Where(x => x.Bolum == "anasayfa" && x.Anahtar == "PdfKatalogUrl")
            .ToListAsync();
        foreach (var kayit in katalogBaglantilari)
        {
            if (kayit.Deger == katalogYolu)
                continue;

            kayit.Deger = katalogYolu;
            degisti = true;
        }

        var eskiMedyaOnEkleri = new[]
        {
            "/medya/slaytlar/",
            "/medya/fabrika/",
            "/medya/hakkimizda/",
            "/medya/kapaklar/",
            "/medya/ekip/"
        };

        var eskiSlaytlar = await vt.Slaytlar
            .Where(x => x.ArkaplanResim != null && eskiMedyaOnEkleri.Any(onEk => x.ArkaplanResim.StartsWith(onEk)))
            .ToListAsync();
        foreach (var slayt in eskiSlaytlar)
        {
            slayt.ArkaplanResim = "/medya/goldbanyo/uretim.jpg";
            degisti = true;
        }

        var eskiSayfaGorselleri = await vt.SayfaIcerikleri
            .Where(x => eskiMedyaOnEkleri.Any(onEk => x.Deger.StartsWith(onEk)))
            .ToListAsync();
        foreach (var kayit in eskiSayfaGorselleri)
        {
            kayit.Deger = kayit.Anahtar.Contains("Katalog", StringComparison.OrdinalIgnoreCase)
                ? katalogYolu
                : "/medya/goldbanyo/uretim.jpg";
            degisti = true;
        }

        var ayarlarKayitlari = await vt.SayfaIcerikleri
            .Where(x => x.Bolum == "ayarlar")
            .ToListAsync();
        foreach (var kayit in ayarlarKayitlari)
        {
            var hedefDeger = kayit.Anahtar switch
            {
                "LogoUrl" => "/img/goldbanyo-logo.png",
                "FaviconUrl" => "/favicon.png",
                "VarsayilanDil" => "tr",
                "TemaModu" => "koyu",
                "SiteBasligi" => "Gold Banyo - Banyo Mobilyalari ve Koleksiyonlari",
                "Aciklama" => "Gold Banyo; premium, trend ve exclusive koleksiyonlariyla banyo mobilyasi ve proje cozumleri sunar.",
                "AnahtarKelimeler" => "gold banyo, goldbanyo, banyo mobilyasi, banyo dolabi, banyo koleksiyonlari",
                "Telefon1" => "+90 224 482 24 00",
                "Telefon2" => "+90 533 597 32 14",
                "Eposta" => "info@goldbanyom.com.tr",
                "Adres" => "Çalı Mah. Ömer Biltekin Bulv. No:3/1A Nilüfer / BURSA",
                "Whatsapp" => "+90 533 597 32 14",
                "Instagram" => "https://www.instagram.com/gold.banyom/",
                "InstagramUrl" => "https://www.instagram.com/gold.banyom/",
                "Facebook" => "https://www.facebook.com/gold.banyo",
                "FacebookUrl" => "https://www.facebook.com/gold.banyo",
                "Youtube" => "",
                "YoutubeUrl" => "",
                _ => null
            };

            if (string.IsNullOrWhiteSpace(hedefDeger) || kayit.Deger == hedefDeger)
                continue;

            if (kayit.Deger.Contains("vizitlink3d", StringComparison.OrdinalIgnoreCase)
                || kayit.Deger.Contains("3dvizitlink", StringComparison.OrdinalIgnoreCase)
                || kayit.Deger.Contains("VIZITLINK3D", StringComparison.OrdinalIgnoreCase)
                || kayit.Anahtar is "LogoUrl" or "FaviconUrl" or "SiteBasligi" or "Aciklama" or "AnahtarKelimeler"
                    or "Telefon1" or "Telefon2" or "Eposta" or "Adres" or "Whatsapp" or "Instagram" or "InstagramUrl"
                    or "Facebook" or "FacebookUrl" or "Youtube" or "YoutubeUrl")
            {
                kayit.Deger = hedefDeger;
                degisti = true;
            }
        }

        var goldFirma = await vt.Firmalar
            .FirstOrDefaultAsync(x => x.Slug == "goldbanyo"
                || x.Domain == "goldbanyom.com.tr"
                || x.YedekDomain == "www.goldbanyom.com.tr");
        if (goldFirma != null)
        {
            if (goldFirma.Ad != "Gold Banyo")
            {
                goldFirma.Ad = "Gold Banyo";
                degisti = true;
            }

            if (goldFirma.Unvan != "Gold Banyo Banyo Mobilyası")
            {
                goldFirma.Unvan = "Gold Banyo Banyo Mobilyası";
                degisti = true;
            }

            if (goldFirma.Logo != "/img/goldbanyo-logo.png")
            {
                goldFirma.Logo = "/img/goldbanyo-logo.png";
                degisti = true;
            }

            if (goldFirma.Favicon != "/favicon.png")
            {
                goldFirma.Favicon = "/favicon.png";
                degisti = true;
            }
        }

        var kataloglar = await vt.Kataloglar
            .Where(x => x.Baslik.Contains("Gold Banyo") || x.PdfDosyaYolu.Contains("VIZITLINK3D") || x.PdfDosyaYolu.Contains("VIZITLINK3D"))
            .ToListAsync();
        foreach (var katalog in kataloglar)
        {
            katalog.PdfDosyaYolu = katalogYolu;
            katalog.Yil = 2026;

            if (katalog.Baslik.Contains("Ürün"))
                katalog.Baslik = "Gold Banyo Urun Dosyasi";
            else
                katalog.Baslik = "Gold Banyo Katalog";

            if (katalog.Aciklama.Contains("ürün") || katalog.Aciklama.Contains("urun"))
                katalog.Aciklama = "Gold Banyo urun seckisi ve teknik sunum dosyasi";
            else
                katalog.Aciklama = "Gold Banyo koleksiyon ve urun katalogu";

            degisti = true;
        }

        var eskiVIZITLINK3DFirmalari = await vt.Firmalar
            .Where(x => x.Slug == "VIZITLINK3D")
            .ToListAsync();
        foreach (var firma in eskiVIZITLINK3DFirmalari)
        {
            firma.Ad = "Gold Banyo Demo";
            firma.Unvan = "Gold Banyo Demo";
            firma.Slug = "goldbanyo-demo";
            firma.AciklamaKisa = "Gold Banyo koleksiyonlari icin demo firma kaydi";
            firma.Aciklama = "Gold Banyo demo kaydi, coklu firma akislarini test etmek icin kullanilir.";
            firma.Domain = "demo.goldbanyom.local";
            firma.YedekDomain = "www.demo.goldbanyom.local";
            firma.Eposta = "demo@goldbanyom.local";
            firma.Instagram = "https://www.instagram.com/gold.banyom/";
            firma.Facebook = "https://www.facebook.com/gold.banyo";
            firma.DemoMu = true;
            firma.GuncellenmeTarihi = DateTime.UtcNow;
            degisti = true;
        }

        if (degisti)
            await vt.SaveChangesAsync();
    }

    private static async Task TohumlaKataloglariAsync(VizitLink3DDbContext vt)
    {
        // PDF dosyalari wwwroot/medya/kataloglar altinda mevcut; /katalog sayfasi
        // bu kayitlardan beslenir (belge-onizleme PdfDosyaYolu ile render eder).
        var kataloglar = new List<Katalog>
        {
            new() { Baslik = "Gold Banyo Katalog", Aciklama = "Gold Banyo koleksiyon ve urun katalogu", PdfDosyaYolu = "medya/gold-katalog/GOLD-2026-KATALOG.pdf", Yil = 2026, SiraNo = 1, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow },
            new() { Baslik = "Gold Banyo Urun Dosyasi", Aciklama = "Gold Banyo urun seckisi ve teknik sunum dosyasi", PdfDosyaYolu = "medya/gold-katalog/GOLD-2026-KATALOG.pdf", Yil = 2026, SiraNo = 2, AktifMi = true, OlusturulmaTarihi = DateTime.UtcNow }
        };

        foreach (var katalog in kataloglar)
        {
            if (!await vt.Kataloglar.AnyAsync(x => x.PdfDosyaYolu == katalog.PdfDosyaYolu))
                vt.Kataloglar.Add(katalog);
        }
        await vt.SaveChangesAsync();
    }

    /// <summary>
    /// i:\modeller klasöründeki gerçek NRD kapak modellerini seed eder.
    /// Her çalışmada eksik olanları ekler, mevcutları atlar.
    /// </summary>
    private static async Task TohumlaGercekKapakModelleriAsync(VizitLink3DDbContext vt)
    {
        // Soft-deleted kayitlari da kontrol et; yoksa AciliKapakModelleriniSil silinmisleri yeniden ekler
        var mevcutKodlar = (await vt.KapakModelleri
            .Select(k => k.ModelKodu)
            .ToListAsync())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var mevcutSluglar = (await vt.KapakModelleri
            .Select(k => k.Slug)
            .ToListAsync())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var r = JsonSerializer.Serialize(VarsayilanRenkPaleti());
        var nitelikler = JsonSerializer.Serialize(new[] { "MDF", "Membran Kaplama", "UV Lake" });

        // Resim yolları: wwwroot/medya/kapaklar/ altında thumb_NNN.jpg ve yatay_NNN.png
        // GLB yolları:   wwwroot/medya/3d/ altında nrd_NNN.glb
        var modeller = new List<KapakModeli>
        {
            NrdKapak(100, "NRD 100", "nrd-100", "Membran", true,  false, r, nitelikler, true),
            NrdKapak(101, "NRD 101", "nrd-101", "Membran", false, false, r, nitelikler, false),
            NrdKapak(102, "NRD 102", "nrd-102", "Membran", false, false, r, nitelikler, true),
            NrdKapak(103, "NRD 103", "nrd-103", "Membran", false, false, r, nitelikler, true),
            NrdKapak(104, "NRD 104", "nrd-104", "Membran", false, false, r, nitelikler, true),
            NrdKapak(105, "NRD 105", "nrd-105", "Membran", true,  false, r, nitelikler, true),
            NrdKapak(106, "NRD 106", "nrd-106", "Membran", false, false, r, nitelikler, true),
            NrdKapak(107, "NRD 107", "nrd-107", "Membran", false, false, r, nitelikler, true),
            NrdKapak(108, "NRD 108", "nrd-108", "Membran", false, false, r, nitelikler, true),
            NrdKapak(109, "NRD 109", "nrd-109", "Membran", false, false, r, nitelikler, true),
            NrdKapak(110, "NRD 110", "nrd-110", "Membran", false, false, r, nitelikler, true),
            NrdKapak(111, "NRD 111", "nrd-111", "Membran", false, false, r, nitelikler, true),
            NrdKapak(112, "NRD 112", "nrd-112", "Membran", true,  false, r, nitelikler, true),
            NrdKapak(113, "NRD 113", "nrd-113", "Membran", false, false, r, nitelikler, true),
            NrdKapak(114, "NRD 114", "nrd-114", "Membran", false, false, r, nitelikler, true),
            NrdKapak(116, "NRD 116", "nrd-116", "Membran", false, false, r, nitelikler, false),
            NrdKapak(117, "NRD 117", "nrd-117", "Membran", false, false, r, nitelikler, false),
            NrdKapak(118, "NRD 118", "nrd-118", "Membran", false, false, r, nitelikler, true),
            NrdKapak(120, "NRD 120", "nrd-120", "Membran", false, false, r, nitelikler, true),
            NrdKapak(121, "NRD 121", "nrd-121", "Membran", false, false, r, nitelikler, true),
            NrdKapak(122, "NRD 122", "nrd-122", "Membran", false, false, r, nitelikler, false),
            NrdKapak(123, "NRD 123", "nrd-123", "Membran", false, false, r, nitelikler, false),
            NrdKapak(124, "NRD 124", "nrd-124", "Membran", false, false, r, nitelikler, true),
            NrdKapak(125, "NRD 125", "nrd-125", "Membran", false, false, r, nitelikler, true),
            NrdKapak(126, "NRD 126", "nrd-126", "Membran", false, false, r, nitelikler, false),
            NrdKapak(127, "NRD 127", "nrd-127", "Membran", false, false, r, nitelikler, false),
            NrdKapak(128, "NRD 128", "nrd-128", "Membran", false, false, r, nitelikler, true),
            NrdKapak(129, "NRD 129", "nrd-129", "Membran", false, false, r, nitelikler, false),
            NrdKapak(130, "NRD 130", "nrd-130", "Membran", false, false, r, nitelikler, true),
            NrdKapak(131, "NRD 131", "nrd-131", "Membran", false, false, r, nitelikler, false),
            NrdKapak(132, "NRD 132", "nrd-132", "Membran", false, false, r, nitelikler, false),
            NrdKapak(133, "NRD 133", "nrd-133", "Membran", false, false, r, nitelikler, false),
            NrdKapak(134, "NRD 134", "nrd-134", "Membran", false, false, r, nitelikler, true),
            NrdKapak(135, "NRD 135", "nrd-135", "Membran", false, false, r, nitelikler, true),
            NrdKapak(137, "NRD 137", "nrd-137", "Membran", false, false, r, nitelikler, false),
            NrdKapak(138, "NRD 138", "nrd-138", "Membran", false, false, r, nitelikler, false),
            NrdKapak(139, "NRD 139", "nrd-139", "Membran", false, false, r, nitelikler, false),
            NrdKapak(140, "NRD 140", "nrd-140", "Membran", false, false, r, nitelikler, false),
            NrdKapak(141, "NRD 141", "nrd-141", "Membran", false, false, r, nitelikler, false),
            NrdKapak(142, "NRD 142", "nrd-142", "Membran", false, false, r, nitelikler, false),
            NrdKapak(143, "NRD 143", "nrd-143", "Membran", false, false, r, nitelikler, false),
            NrdKapak(144, "NRD 144", "nrd-144", "Membran", false, false, r, nitelikler, true),
            NrdKapak(145, "NRD 145", "nrd-145", "Membran", false, false, r, nitelikler, false),
            NrdKapak(146, "NRD 146", "nrd-146", "Membran", false, false, r, nitelikler, false),
            NrdKapak(147, "NRD 147", "nrd-147", "Membran", false, false, r, nitelikler, true),
            NrdKapak(148, "NRD 148", "nrd-148", "Membran", false, false, r, nitelikler, false),
            NrdKapak(149, "NRD 149", "nrd-149", "Membran", false, false, r, nitelikler, true),
            NrdKapak(150, "NRD 150", "nrd-150", "Membran", false, false, r, nitelikler, true),
            NrdKapak(151, "NRD 151", "nrd-151", "Membran", false, false, r, nitelikler, true),
            NrdKapak(152, "NRD 152", "nrd-152", "Membran", false, false, r, nitelikler, true),
            NrdKapak(153, "NRD 153", "nrd-153", "Membran", false, false, r, nitelikler, true),
            NrdKapak(154, "NRD 154", "nrd-154", "Membran", false, false, r, nitelikler, true),
            NrdKapak(155, "NRD 155", "nrd-155", "Membran", false, false, r, nitelikler, false),
            NrdKapak(156, "NRD 156", "nrd-156", "Membran", false, false, r, nitelikler, true),
            NrdKapak(157, "NRD 157", "nrd-157", "Membran", false, false, r, nitelikler, true),
            NrdKapak(158, "NRD 158", "nrd-158", "Membran", false, false, r, nitelikler, true),
            NrdKapak(159, "NRD 159", "nrd-159", "Membran", false, false, r, nitelikler, false),
            NrdKapak(160, "NRD 160", "nrd-160", "Membran", false, false, r, nitelikler, true),
            NrdKapak(161, "NRD 161", "nrd-161", "Membran", false, false, r, nitelikler, true),
            NrdKapak(162, "NRD 162", "nrd-162", "Membran", false, false, r, nitelikler, true),
            NrdKapak(163, "NRD 163", "nrd-163", "Membran", false, false, r, nitelikler, false),
            NrdKapak(164, "NRD 164", "nrd-164", "Membran", false, false, r, nitelikler, true),
            NrdKapak(165, "NRD 165", "nrd-165", "Membran", false, false, r, nitelikler, false),
            NrdKapak(166, "NRD 166", "nrd-166", "Membran", false, false, r, nitelikler, true),
            NrdKapak(167, "NRD 167", "nrd-167", "Membran", false, false, r, nitelikler, true),
            NrdKapak(168, "NRD 168", "nrd-168", "Membran", false, false, r, nitelikler, false),
            NrdKapak(169, "NRD 169", "nrd-169", "Membran", false, false, r, nitelikler, false),
            // BOY KAPAKLAR serisi
            NrdKapakBoySeri("BOY-01", "NRD BOY KPK 01", "nrd-boy-kpk-01", r, nitelikler),
            NrdKapakBoySeri("BOY-02", "NRD BOY KPK 02", "nrd-boy-kpk-02", r, nitelikler),
            NrdKapakBoySeri("BOY-04", "NRD BOY KPK 04", "nrd-boy-kpk-04", r, nitelikler),
            NrdKapakBoySeri("BOY-05", "NRD BOY KPK 05", "nrd-boy-kpk-05", r, nitelikler),
            NrdKapakBoySeri("BOY-07", "NRD BOY KPK 07", "nrd-boy-kpk-07", r, nitelikler),
            NrdKapakBoySeri("BOY-08", "NRD BOY KPK 08", "nrd-boy-kpk-08", r, nitelikler),
            NrdKapakBoySeri("BOY-10", "NRD BOY KPK 10", "nrd-boy-kpk-10", r, nitelikler),
            NrdKapakBoySeri("BOY-11", "NRD BOY KPK 11", "nrd-boy-kpk-11", r, nitelikler),
            NrdKapakBoySeri("BOY-12", "NRD BOY KPK 12", "nrd-boy-kpk-12", r, nitelikler),
            NrdKapakBoySeri("BOY-13", "NRD BOY KPK 13", "nrd-boy-kpk-13", r, nitelikler),
            NrdKapakBoySeri("BOY-14", "NRD BOY KPK 14", "nrd-boy-kpk-14", r, nitelikler),
            NrdKapakBoySeri("BOY-15", "NRD BOY KPK 15", "nrd-boy-kpk-15", r, nitelikler),
            NrdKapakBoySeri("BOY-16", "NRD BOY KPK 16", "nrd-boy-kpk-16", r, nitelikler),
            NrdKapakBoySeri("BOY-22", "NRD BOY KPK 22", "nrd-boy-kpk-22", r, nitelikler),
            // CAM (camlı) serisi
            NrdKapakCamSeri("CAM-100", "NRD CAM 100", "nrd-cam-100", r, nitelikler),
            NrdKapakCamSeri("CAM-101", "NRD CAM 101", "nrd-cam-101", r, nitelikler),
            NrdKapakCamSeri("CAM-102", "NRD CAM 102", "nrd-cam-102", r, nitelikler),
            NrdKapakCamSeri("CAM-103", "NRD CAM 103", "nrd-cam-103", r, nitelikler),
            NrdKapakCamSeri("CAM-104", "NRD CAM 104", "nrd-cam-104", r, nitelikler),
            NrdKapakCamSeri("CAM-105", "NRD CAM 105", "nrd-cam-105", r, nitelikler),
            NrdKapakCamSeri("CAM-106", "NRD CAM 106", "nrd-cam-106", r, nitelikler),
            NrdKapakCamSeri("CAM-107", "NRD CAM 107", "nrd-cam-107", r, nitelikler),
            NrdKapakCamSeri("CAM-108", "NRD CAM 108", "nrd-cam-108", r, nitelikler),
            NrdKapakCamSeri("CAM-109", "NRD CAM 109", "nrd-cam-109", r, nitelikler),
            NrdKapakCamSeri("CAM-110", "NRD CAM 110", "nrd-cam-110", r, nitelikler),
            NrdKapakCamSeri("CAM-111", "NRD CAM 111", "nrd-cam-111", r, nitelikler),
        };

        var eklenecekler = modeller
            .Where(m => !mevcutKodlar.Contains(m.ModelKodu) && !mevcutSluglar.Contains(m.Slug))
            .GroupBy(m => m.Slug, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (eklenecekler.Any())
        {
            vt.KapakModelleri.AddRange(eklenecekler);
            await vt.SaveChangesAsync();
            Console.WriteLine($"[TOHUM] {eklenecekler.Count} yeni NRD kapak modeli eklendi.");
        }
    }

    /// <summary>
    /// NRD 100-169 serisi kapak modeli oluşturur.
    /// glbVarMi: i:\modeller klasöründe NRD NNN.glb dosyası var mı?
    /// </summary>
    private static KapakModeli NrdKapak(
        int numara, string ad, string slug, string kategori,
        bool oneCikan, bool yeni,
        string renkJson, string nitelikJson, bool glbVarMi)
    {
        var numStr = numara.ToString();
        var anaGorsel = "/medya/gold-katalog/sayfa-006-hero.png";
        var yatayGorsel = "/medya/gold-katalog/sayfa-010-hero.png";
        var modelDosya = glbVarMi ? $"medya/3d/nrd_{numStr}.glb" : null;

        var uygulamaGorseller = new List<string> { yatayGorsel, "/medya/goldbanyo/uretim.jpg" };

        return new KapakModeli
        {
            ModelAdi = ad,
            ModelKodu = $"NRD-{numStr}",
            Slug = slug,
            Kategori = kategori,
            ModelTuru = "Kapak",
            OneCikanMi = oneCikan,
            YeniMi = yeni,
            AnaGorselUrl = anaGorsel,
            ModelDosyaYolu = modelDosya,
            OnYazi = $"Norden {ad} membran kapak modeli.",
            Aciklama = $"VizitLink3D Norden serisi {ad} mobilya kapağı. Yüksek kaliteli MDF üzeri membran kaplama teknolojisi ile üretilmiştir.",
            MinYukseklik = 65,
            MaxYukseklik = 2400,
            MinGenislik = 65,
            MaxGenislik = 900,
            RenkSecenekleriJson = renkJson,
            NiteliklerJson = nitelikJson,
            UygulamaGorselleriJson = JsonSerializer.Serialize(uygulamaGorseller),
            SiraNo = numara,
            OlusturulmaTarihi = DateTime.UtcNow
        };
    }

    private static KapakModeli NrdKapakBoySeri(string kod, string ad, string slug, string renkJson, string nitelikJson)
    {
        // "nrd-boy-kpk-01" -> "NRD BOY KPK 01"
        var glbDosyaAdi = ad.ToUpperInvariant().Replace(" ", "%20");
        return new KapakModeli
        {
            ModelAdi = ad,
            ModelKodu = kod,
            Slug = slug,
            Kategori = "BoyKapak",
            ModelTuru = "Kapak",
            OneCikanMi = false,
            YeniMi = false,
            AnaGorselUrl = null,
            ModelDosyaYolu = $"medya/3d/{Uri.EscapeDataString(ad)}.glb",
            OnYazi = $"{ad} boy kapak modeli.",
            Aciklama = $"VizitLink3D Norden {ad} boy kapak serisi.",
            MinYukseklik = 65,
            MaxYukseklik = 2400,
            MinGenislik = 65,
            MaxGenislik = 900,
            RenkSecenekleriJson = renkJson,
            NiteliklerJson = nitelikJson,
            OlusturulmaTarihi = DateTime.UtcNow
        };
    }

    private static KapakModeli NrdKapakCamSeri(string kod, string ad, string slug, string renkJson, string nitelikJson)
    {
        return new KapakModeli
        {
            ModelAdi = ad,
            ModelKodu = kod,
            Slug = slug,
            Kategori = "Camli",
            ModelTuru = "Kapak",
            OneCikanMi = false,
            YeniMi = false,
            AnaGorselUrl = null,
            ModelDosyaYolu = $"medya/3d/{Uri.EscapeDataString(ad)}.glb",
            OnYazi = $"{ad} camlı kapak modeli.",
            Aciklama = $"VizitLink3D Norden {ad} camlı kapak serisi. Temperli cam ve MDF kombinasyonu.",
            MinYukseklik = 260,
            MaxYukseklik = 2400,
            MinGenislik = 260,
            MaxGenislik = 900,
            RenkSecenekleriJson = renkJson,
            NiteliklerJson = nitelikJson,
            OlusturulmaTarihi = DateTime.UtcNow
        };
    }

    private static async Task ProjeleriYenile2026Async(VizitLink3DDbContext vt)
    {
        // Mevcut tüm projeleri pasife al
        var eskiler = await vt.Projeler.ToListAsync();
        foreach (var e in eskiler) e.AktifMi = false;
        if (eskiler.Any()) await vt.SaveChangesAsync();

        var kategoriler = await vt.ProjeKategorileri.ToDictionaryAsync(k => k.Slug, k => k.Id);
        if (kategoriler.Count == 0) return;
        int Kat(string slug) => kategoriler.TryGetValue(slug, out var id) ? id : kategoriler.Values.First();

        var simdi = DateTime.UtcNow;

        vt.Projeler.AddRange(
            new Proje
            {
                Slug = "sertepe-195-daire-mutfak-dolabi",
                Baslik = "Sertepe İnşaat — 195 Daire Mutfak Donanımı",
                KisaAciklama = "Bursa Nilüfer'de 195 daireli rezidans projesi için lake kapak sistem tasarımı ve uygulaması.",
                Aciklama = "<p>Bursa Nilüfer'de inşa edilen 195 daireli lüks konut projesinde VizitLink3D imzalı lake mutfak dolapları kullanıldı. Her daire için özel ölçüde üretilen mat lake kapaklar, modern ve sade çizgisiyle projenin genel tasarım anlayışını yansıtmaktadır.</p><p>Üretimden teslimata kadar tüm süreçler VizitLink3D kalite standartları çerçevesinde yürütülmüş; montaj ekibimiz tüm birimlerde titiz bir uygulama gerçekleştirmiştir.</p>",
                KategoriId = Kat("mutfak"),
                MusteriAdi = "Sertepe İnşaat",
                MusteriSehir = "Bursa",
                MusteriLogo = "/medya/referanslar/sertepe-insaat.png",
                KapakResim = "/medya/mutfak/mutfak1a.jpg",
                OneCikanMi = true,
                SiraNo = 1,
                AktifMi = true,
                SeoBaslik = "Sertepe İnşaat 195 Daire Mutfak Projesi | VizitLink3D",
                SeoAciklama = "Bursa Nilüfer'de 195 daireli konut projesinde lake mutfak dolabı uygulaması.",
                ProjeTarihi = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                OlusturulmaTarihi = simdi
            },
            new Proje
            {
                Slug = "alpis-insaat-120-daire-lake-kapak",
                Baslik = "Alpış İnşaat — 120 Daire Premium Lake Sistem",
                KisaAciklama = "Istanbul Başakşehir projesinde 120 daire için yüksek parlak lake kapak ve kapı sistemi.",
                Aciklama = "<p>Istanbul Başakşehir'deki konut projesinde 120 dairenin mutfak ve yatak odaları VizitLink3D premium lake kapak sistemi ile donatıldı. Yüksek parlak (high-gloss) lake kapaklar, modern iç mimarinin vazgeçilmez unsuru haline gelmiştir.</p><p>Üretimden yerinde montaja kadar tüm süreçlerde kalite yönetim sistemimiz devreye girmiş, proje zamanında ve eksiksiz teslim edilmiştir.</p>",
                KategoriId = Kat("mutfak"),
                MusteriAdi = "Alpış İnşaat",
                MusteriSehir = "Istanbul",
                MusteriLogo = "/medya/referanslar/alpis-insaat.png",
                KapakResim = "/medya/mutfak/mutfak1b.jpg",
                OneCikanMi = true,
                SiraNo = 2,
                AktifMi = true,
                SeoBaslik = "Alpış İnşaat 120 Daire Lake Kapak Projesi | VizitLink3D",
                SeoAciklama = "Istanbul Başakşehir'de 120 daireli konut projesi için lake kapak ve kapı sistemi.",
                ProjeTarihi = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                OlusturulmaTarihi = simdi
            },
            new Proje
            {
                Slug = "ulu-cinar-16-villa-komple-kapi-kapak",
                Baslik = "Ulu Çınar — 16 Villa Komple Kapı & Kapak",
                KisaAciklama = "Bursa Mudanya'da 16 villanın tüm iç kapı ve mutfak kapak sistemi özel lake serisinden.",
                Aciklama = "<p>Bursa Mudanya'daki prestijli villa projesinde 16 konutun tüm iç kapıları ve mutfak kapak sistemleri VizitLink3D özel lake serisiyle donatıldı. Her villa için birbirinden farklı renk ve doku kombinasyonları uygulanarak kişisel bir estetik anlayış yaratıldı.</p><p>Hem lake mutfak kapakları hem de iç mekan kapı modelleri aynı kaplama rengi ve dokusuyla birbiriyle uyumlu üretilerek bütünleşik bir iç mimari dil oluşturuldu.</p>",
                KategoriId = Kat("mutfak"),
                MusteriAdi = "Ulu Çınar",
                MusteriSehir = "Bursa",
                MusteriLogo = "/medya/referanslar/ulu-cinar.png",
                KapakResim = "/medya/mutfak/mutfak1c.jpg",
                OneCikanMi = true,
                SiraNo = 3,
                AktifMi = true,
                SeoBaslik = "Ulu Çınar 16 Villa Komple Kapı ve Kapak Projesi | VizitLink3D",
                SeoAciklama = "Bursa Mudanya'da 16 villa için lake kapak ve iç kapı sistemi uygulaması.",
                ProjeTarihi = new DateTime(2025, 3, 20, 0, 0, 0, DateTimeKind.Utc),
                OlusturulmaTarihi = simdi
            },
            new Proje
            {
                Slug = "kumova-insaat-196-daire-membran-kapak",
                Baslik = "Kumova İnşaat — 196 Daire Membran Kapak",
                KisaAciklama = "Bursa Osmangazi'de 196 daireli projede ekonomik ve şık membran kapak çözümü.",
                Aciklama = "<p>Bursa Osmangazi'deki 196 daireli konut projesinde VizitLink3D membran kapak sistemi tercih edildi. Bütçe dostu yapısına karşın yüksek estetik sunan membran kapaklar, geniş renk skalasıyla her dairenin ihtiyacına özel konfigüre edildi.</p><p>Kısa üretim süreleri ve hızlı montaj kapasitesiyle proje belirlenen takvim dahilinde tamamlanmıştır.</p>",
                KategoriId = Kat("mutfak"),
                MusteriAdi = "Kumova İnşaat",
                MusteriSehir = "Bursa",
                MusteriLogo = "/medya/referanslar/kumova-insaat.png",
                KapakResim = "/medya/mutfak/mutfak2a.jpg",
                OneCikanMi = false,
                SiraNo = 4,
                AktifMi = true,
                SeoBaslik = "Kumova İnşaat 196 Daire Membran Kapak Projesi | VizitLink3D",
                SeoAciklama = "Bursa Osmangazi'de 196 daireli konut projesi için membran kapak uygulaması.",
                ProjeTarihi = new DateTime(2024, 11, 10, 0, 0, 0, DateTimeKind.Utc),
                OlusturulmaTarihi = simdi
            },
            new Proje
            {
                Slug = "bezek-mimarlik-150-daire-lake-kapi",
                Baslik = "Bezek Mimarlık — 150 Daire Lake Kapı Projesi",
                KisaAciklama = "Ankara Çankaya'da mimarlık bürosu öncülüğünde 150 daire için özel tasarım lake kapı.",
                Aciklama = "<p>Ankara Çankaya'da hayata geçirilen prestijli konut projesinde Bezek Mimarlık'ın özel tasarımlarına uygun lake kapı modelleri üretildi. Mimari projeyle bütünleşik kapı tasarımları, mekanların genel estetiğini tamamlayan özgün bir kimlik oluşturdu.</p><p>Her kapı modeli mimarın teknik çizimlerine göre özel ölçüde üretilmiş; yüzey işlemleri ve donanımlar birlikte belirlenmiştir.</p>",
                KategoriId = Kat("ofis"),
                MusteriAdi = "Bezek Mimarlık",
                MusteriSehir = "Ankara",
                MusteriLogo = "/medya/referanslar/bezek-mimarlik.png",
                KapakResim = "/medya/gold-katalog/sayfa-006-hero.png",
                OneCikanMi = true,
                SiraNo = 5,
                AktifMi = true,
                SeoBaslik = "Bezek Mimarlık 150 Daire Lake Kapı Projesi | VizitLink3D",
                SeoAciklama = "Ankara Çankaya'da 150 daireli proje için özel tasarım lake kapı uygulaması.",
                ProjeTarihi = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                OlusturulmaTarihi = simdi
            },
            new Proje
            {
                Slug = "sadriogullari-200-daire-mutfak-kapak",
                Baslik = "Sadrioğulları İnşaat — 200 Daire Mutfak",
                KisaAciklama = "Izmir Karşıyaka'da 200 daireli büyük ölçekli projede lake ve membran karma kapak sistemi.",
                Aciklama = "<p>Izmir Karşıyaka'da teslim edilen 200 daireli konut projesinde daire tipine göre lake ve membran kapak karması tercih edildi. Geniş dairelerde premium lake, standart dairelerde ekonomik membran kapaklar kullanılarak bütçe optimizasyonu sağlandı.</p><p>VizitLink3D'un kurumsal proje birimi tüm koordinasyonu üstlenmiş; sözleşme, üretim, lojistik ve montaj süreçleri tek noktadan yönetilmiştir.</p>",
                KategoriId = Kat("mutfak"),
                MusteriAdi = "Sadrioğulları İnşaat",
                MusteriSehir = "Izmir",
                MusteriLogo = "/medya/referanslar/sadriogullari-insaat.png",
                KapakResim = "/medya/mutfak/mutfak2b.jpg",
                OneCikanMi = false,
                SiraNo = 6,
                AktifMi = true,
                SeoBaslik = "Sadrioğulları İnşaat 200 Daire Mutfak Kapak Projesi | VizitLink3D",
                SeoAciklama = "Izmir Karşıyaka'da 200 daireli proje için lake ve membran karma kapak sistemi.",
                ProjeTarihi = new DateTime(2024, 8, 22, 0, 0, 0, DateTimeKind.Utc),
                OlusturulmaTarihi = simdi
            },
            new Proje
            {
                Slug = "yg-goktas-96-daire-lake-kapak-kapi",
                Baslik = "YG Göktaş İnş. — 96 Daire Kapak & Kapı",
                KisaAciklama = "Bursa Yıldırım'da 96 daireli projede lake mutfak kapağı ve ahşap görünümlü iç kapı kombinasyonu.",
                Aciklama = "<p>Bursa Yıldırım'daki 96 daireli konut projesinde mutfak kapakları ve iç kapılar VizitLink3D imzasıyla hayata geçirildi. Lake mutfak kapakları açık gri rengi ile modern bir görünüm sunarken, iç kapılarda ahşap desenli kaplama tercih edilerek sıcak bir atmosfer yaratıldı.</p><p>Aynı proje bünyesindeki kapak ve kapı siparişleri koordineli üretimle eş zamanlı teslim edilmiş, montaj süreci kısaltılmıştır.</p>",
                KategoriId = Kat("mutfak"),
                MusteriAdi = "YG Göktaş İnşaat",
                MusteriSehir = "Bursa",
                MusteriLogo = "/medya/referanslar/yg-goktas.png",
                KapakResim = "/medya/mutfak/mutfak2c.jpg",
                OneCikanMi = false,
                SiraNo = 7,
                AktifMi = true,
                SeoBaslik = "YG Göktaş İnşaat 96 Daire Kapak ve Kapı Projesi | VizitLink3D",
                SeoAciklama = "Bursa Yıldırım'da 96 daireli proje için lake kapak ve ahşap görünümlü kapı kombinasyonu.",
                ProjeTarihi = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                OlusturulmaTarihi = simdi
            },
            new Proje
            {
                Slug = "akar-insaat-60-daire-cam-kapakli-mutfak",
                Baslik = "Akar İnşaat — 60 Daire Camlı Lake Mutfak",
                KisaAciklama = "Bursa Nilüfer'de 60 daireli prestijli projede camlı lake kapak ve ahşap desen kombinasyonu.",
                Aciklama = "<p>Bursa Nilüfer'deki 60 daireli üst segment konut projesinde VizitLink3D'un camlı lake kapak serisi tercih edildi. Temperli cam eklemeli üst dolap kapakları ile mat lake alt dolap kapakları bir arada kullanılarak çağdaş ve ferah bir mutfak görünümü elde edildi.</p><p>Her daire için renk ve cam kombinasyonu müşteri tercihine göre özelleştirilmiş; üretim süresi boyunca projede görevlendirilen VizitLink3D proje koordinatörü tüm süreci birebir takip etmiştir.</p>",
                KategoriId = Kat("mutfak"),
                MusteriAdi = "Akar İnşaat",
                MusteriSehir = "Bursa",
                MusteriLogo = "/medya/referanslar/akar-insaat.png",
                KapakResim = "/medya/gold-katalog/sayfa-010-hero.png",
                OneCikanMi = true,
                SiraNo = 8,
                AktifMi = true,
                SeoBaslik = "Akar İnşaat 60 Daire Camlı Lake Mutfak Projesi | VizitLink3D",
                SeoAciklama = "Bursa Nilüfer'de 60 daireli proje için camlı lake kapak ve mat lake kombinasyonu.",
                ProjeTarihi = new DateTime(2025, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                OlusturulmaTarihi = simdi
            }
        );

        await vt.SaveChangesAsync();
    }

    // ─── FABRİKAMIZ MENÜ + SAYFA İÇERİKLERİ ───────────────────────────────────
    private static async Task FabrikaMenuVeSayfaEkleAsync(VizitLink3DDbContext vt)
    {
        // 1) Kurumsal alt menüye "Fabrikamız" ekle
        var kurumsal = await vt.MenuOgeleri
            .FirstOrDefaultAsync(m => m.Konum == "PublicHeader" && m.Baslik == "Kurumsal" && m.UstMenuId == null && !m.SilindiMi);
        if (kurumsal != null && !await vt.MenuOgeleri.AnyAsync(m => m.UstMenuId == kurumsal.Id && m.Url == "fabrikamiz" && !m.SilindiMi))
        {
            vt.MenuOgeleri.Add(new MenuOgesi { Baslik = "Fabrikamız", Url = "fabrikamiz", Sira = 5, Konum = "PublicHeader", UstMenuId = kurumsal.Id, AktifMi = true });
        }

        // 2) Fabrikamız hero slaytı
        if (!await vt.Slaytlar.AnyAsync(s => s.SayfaKodu == "fabrikamiz" && s.Dil == "tr" && !s.SilindiMi))
        {
            vt.Slaytlar.AddRange(
                new Slayt { Dil = "tr", SayfaKodu = "fabrikamiz", Baslik = "Fabrikamız", AltBaslik = "Modern Üretim Tesisi", Aciklama = "5.000 m² kapalı alanda 30 yıllık üretim deneyimi.", ArkaplanResim = "/medya/goldbanyo/uretim.jpg", SiraNo = 1, AktifMi = true },
                new Slayt { Dil = "en", SayfaKodu = "fabrikamiz", Baslik = "Our Factory", AltBaslik = "Modern Production Facility", Aciklama = "30 years of production experience in 5,000 m² closed area.", ArkaplanResim = "/medya/goldbanyo/uretim.jpg", SiraNo = 1, AktifMi = true }
            );
        }

        // 3) Gizlilik sayfası içeriğini 3dvizitlink.com.tr ile güncelle
        var gizlilikIcerik = await vt.SayfaIcerikleri
            .FirstOrDefaultAsync(s => s.Bolum == "gizlilik" && s.Anahtar == "SayfaIcerigi" && s.Dil == "tr");
        var gizlilikHtml = "<div class=\"gb-hukuki-sayfa\">" +
            "<h2>Gizlilik ve Kullanım Şartları</h2>" +
            "<p>3dvizitlink.com.tr internet sitesini kullanan ve alışveriş yapan müşterilerin gizliliğini korumak şirketimizin temel ilkeleri arasındadır. Bu politika, kişisel bilgilerinizin nasıl toplandığını, işlendiğini ve korunduğunu açıklamaktadır.</p>" +
            "<h3>Kişisel Bilgilerin Kullanımı</h3>" +
            "<p>Siteye üye olurken ya da alışveriş yaparken talep edilen isim, doğum tarihi, e-posta, kimlik numarası, adres, telefon ve IP adresi gibi kişisel veriler, Türkiye Cumhuriyeti yasaları ve 6698 sayılı KVKK kapsamında işlenmektedir.</p>" +
            "<p>Söz konusu veriler; sipariş tamamlama, müşteri hizmetleri ve yasal yükümlülüklerin yerine getirilmesi amacıyla kullanılmakta olup <strong>üçüncü taraflarla pazarlama amacıyla paylaşılmamaktadır</strong>.</p>" +
            "<h3>Güvenlik</h3>" +
            "<p>Site altyapısında 128-bit SSL şifrelemesi kullanılmaktadır. Kredi kartı bilgileri sisteme kaydedilmez; her işlemde bankanın güvenli ödeme altyapısı üzerinden doğrudan iletilir.</p>" +
            "<h3>Kullanıcı Hakları</h3>" +
            "<ul><li>E-posta adresiniz dışarıdan gelen tanıtım amaçlı mesajlar için kullanılmayacaktır.</li>" +
            "<li>Kişisel bilgileriniz, yasal zorunluluklar dışında idari ve yargı mercileriyle paylaşılmamaktadır.</li>" +
            "<li>Hesap bilgilerinize yalnızca siz erişebilirsiniz.</li>" +
            "<li>Üyelik formundaki zorunlu olmayan alanları doldurmak isteğe bağlıdır.</li>" +
            "<li>Kredi kartı bilgisi şirket sistemine hiçbir şekilde kaydedilmemektedir.</li></ul>" +
            "<h3>İletişim</h3>" +
            "<p>Kişisel verilerinizle ilgili başvurularınız için <a href=\"mailto:info@3dvizitlink.com.tr\">info@3dvizitlink.com.tr</a> adresine yazabilirsiniz.</p>" +
            "</div>";
        if (gizlilikIcerik == null)
            vt.SayfaIcerikleri.Add(new SayfaIcerigi { Bolum = "gizlilik", Anahtar = "SayfaIcerigi", Deger = gizlilikHtml, Dil = "tr" });
        else
            gizlilikIcerik.Deger = gizlilikHtml;

        // 4) Çerez politikası sayfası ekle
        var cerezBaslik = await vt.SayfaIcerikleri
            .FirstOrDefaultAsync(s => s.Bolum == "cerez-politikasi" && s.Anahtar == "SayfaBasligi" && s.Dil == "tr");
        var cerezIcerik = await vt.SayfaIcerikleri
            .FirstOrDefaultAsync(s => s.Bolum == "cerez-politikasi" && s.Anahtar == "SayfaIcerigi" && s.Dil == "tr");

        var cerezHtml = "<div class=\"gb-hukuki-sayfa\">" +
            "<h2>Çerez Politikası</h2>" +
            "<p>VizitLink3D olarak, web sitelerimizde ve üçüncü taraf platform uygulamalarımızda çerez kullanılmaktadır. Sitemize giriş yapmanız, çerez kullanımını kabul ettiğiniz anlamına gelir. Çerezleri tarayıcı ayarlarınızdan devre dışı bırakabilirsiniz.</p>" +
            "<h3>Çerez Nedir?</h3>" +
            "<p>Çerezler (cookie), bir web sitesini ziyaret ettiğinizde tarayıcınız aracılığıyla bilgisayarınıza ya da mobil cihazınıza gönderilen küçük metin dosyalarıdır. Ziyaretleriniz arasında kullanıcı bilgisi ve tercihlerini hatırlamak amacıyla kullanılırlar.</p>" +
            "<h3>Çerez Kullanım Amaçları</h3>" +
            "<ul>" +
            "<li>Site işlevselliğini ve performansını artırmak</li>" +
            "<li>Yeni özellikler sunmak</li>" +
            "<li>Site optimizasyonu için istatistiksel veri toplamak</li>" +
            "<li>Gezinme tercihlerini hatırlamak</li>" +
            "<li>En çok tıklanan bağlantılar, popüler sayfalar ve hata mesajları gibi kişisel olmayan analizler toplamak</li>" +
            "<li>5651 sayılı İnternet Kanunu kapsamındaki yasal yükümlülükleri yerine getirmek</li>" +
            "</ul>" +
            "<p>Tüm işlemler, 6698 sayılı Kişisel Verilerin Korunması Kanunu kapsamında gerçekleştirilmektedir.</p>" +
            "<h3>Çerez Türleri</h3>" +
            "<table style=\"width:100%;border-collapse:collapse;margin:16px 0;\">" +
            "<thead><tr style=\"background:var(--gb-bg-alt);\"><th style=\"padding:10px 14px;text-align:left;border:1px solid var(--mud-palette-divider);\">Çerez Türü</th><th style=\"padding:10px 14px;text-align:left;border:1px solid var(--mud-palette-divider);\">İşlevi</th></tr></thead>" +
            "<tbody>" +
            "<tr><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\"><strong>Oturum Çerezleri</strong></td><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\">Ziyaretleri oturumlara böler; kullanıcı sayfayı kapattığında silinir.</td></tr>" +
            "<tr><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\"><strong>Kalıcı Çerezler</strong></td><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\">Kullanıcı silene ya da son kullanma tarihi geçene kadar cihazda kalır.</td></tr>" +
            "<tr><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\"><strong>Zorunlu Çerezler</strong></td><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\">Sitenin düzgün çalışması için gereklidir; sahtekârlığı önler.</td></tr>" +
            "<tr><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\"><strong>İşlevsel / Analitik Çerezler</strong></td><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\">Kullanıcı tercihlerini ve site kullanım kalıplarını takip eder.</td></tr>" +
            "<tr><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\"><strong>İzleme Çerezleri</strong></td><td style=\"padding:10px 14px;border:1px solid var(--mud-palette-divider);\">Hedefli reklamlar için alan adları arasındaki tıklama ve ziyaretleri takip eder.</td></tr>" +
            "</tbody></table>" +
            "<h3>Çerez Yönetimi</h3>" +
            "<p>Tarayıcı ayarlarınız üzerinden çerezleri görüntüleyebilir, silebilir ya da engelleyebilirsiniz. Çerezleri devre dışı bırakmanız, bazı site özelliklerinin çalışmamasına yol açabilir.</p>" +
            "<ul>" +
            "<li><strong>Google Chrome:</strong> Ayarlar → Gizlilik ve Güvenlik → Çerezler</li>" +
            "<li><strong>Mozilla Firefox:</strong> Tercihler → Gizlilik ve Güvenlik → Çerezler</li>" +
            "<li><strong>Safari:</strong> Tercihler → Gizlilik → Çerezleri Engelle</li>" +
            "<li><strong>Internet Explorer / Edge:</strong> Araçlar → İnternet Seçenekleri → Gizlilik</li>" +
            "</ul>" +
            "<h3>İletişim</h3>" +
            "<p>Çerez politikamız hakkında sorularınız için <a href=\"mailto:info@3dvizitlink.com.tr\">info@3dvizitlink.com.tr</a> adresine yazabilirsiniz.</p>" +
            "</div>";

        if (cerezBaslik == null)
            vt.SayfaIcerikleri.Add(new SayfaIcerigi { Bolum = "cerez-politikasi", Anahtar = "SayfaBasligi", Deger = "Çerez Politikası | VizitLink3D", Dil = "tr" });
        else
            cerezBaslik.Deger = "Çerez Politikası | VizitLink3D";

        if (cerezIcerik == null)
            vt.SayfaIcerikleri.Add(new SayfaIcerigi { Bolum = "cerez-politikasi", Anahtar = "SayfaIcerigi", Deger = cerezHtml, Dil = "tr" });
        else
            cerezIcerik.Deger = cerezHtml;

        // 5) Footer'a çerez politikası linki ekle
        if (!await vt.MenuOgeleri.AnyAsync(m => m.Konum == "PublicFooterHizli" && m.Url == "cerez-politikasi" && !m.SilindiMi))
        {
            vt.MenuOgeleri.Add(new MenuOgesi { Baslik = "Çerez Politikası", Url = "cerez-politikasi", Sira = 8, Konum = "PublicFooterHizli", AktifMi = true });
        }

        await vt.SaveChangesAsync();
    }

    private static async Task TohumlaTemaSablonlariAsync(VizitLink3DDbContext vt)
    {
        // Mevcut kayıtları güncelle: gold aktif, diğerleri pasif
        var tumTemalar = await vt.TemaSablonlari.ToListAsync();
        foreach (var t in tumTemalar)
        {
            if (t.Slug == "gold")
            {
                t.AktifMi = true;
                t.VarsayilanMi = true;
            }
            else
            {
                t.AktifMi = false;
                t.VarsayilanMi = false;
                if (!t.Etiketler.Contains("eskitema"))
                    t.Etiketler = (t.Etiketler + ",eskitema").Trim(',');
            }
        }

        if (tumTemalar.Any())
        {
            await vt.SaveChangesAsync();
            return;
        }

        // Tablo boşsa seed ekle
        var temalar = new List<TemaSablonu>
        {
            new()
            {
                Kod = "GOLD", Ad = "Gold", Slug = "gold",
                Aciklama = "Altın yoğunluklu lüks tema — koyu zemin üzerinde parlayan altın, glassmorphism, Playfair Display.",
                Kaynak = "elle", GlassmorphismAktif = true, Premium = true, VarsayilanMi = true, AktifMi = true,
                Etiketler = "premium,glassmorphism,karanlik,altin,lux,varsayilan",
                RenklerJson = "{\"birincil\":\"#0a0a0a\",\"ikincil\":\"#1a1a1a\",\"vurgu\":\"#FFD700\",\"arkaPlan\":\"#0a0a0a\",\"metin\":\"#f5e6c8\"}",
                TipografiJson = "{\"baslikAilesi\":\"Playfair Display\",\"govdeAilesi\":\"Manrope\",\"boyutSkalaRatio\":1.333}",
                GeometriJson = "{\"koseSm\":2,\"koseMd\":4,\"koseLg\":8,\"koseXl\":16}",
                GlassmorphismJson = "{\"aktif\":true,\"blur\":\"20px\",\"bgOpacity\":0.08}",
                AnimasyonJson = "{\"hizi\":\"yavas\",\"tip\":\"dramatik-altin\"}",
                LayoutJson = "{\"header\":\"glassmorphism\",\"heroTipi\":\"fullscreen-slider-overlay\",\"kartStili\":\"glass-altin-glow\",\"sutunSayisi\":4}",
                AdAnahtar = "tema.gold.ad", AciklamaAnahtar = "tema.gold.aciklama",
                AdVarsayilanTr = "Gold", AdVarsayilanEn = "Gold",
                AciklamaVarsayilanTr = "Altın yoğunluklu lüks tema", AciklamaVarsayilanEn = "Gold-rich luxury theme"
            }
        };

        vt.TemaSablonlari.AddRange(temalar);
        await vt.SaveChangesAsync();
    }

}
