# 3D Konfigüratör — Gerçek Durum ve Düzeltme Planı

> **Proje:** VizitLink3D Konfigurator (Bağımsız 3D SaaS Platformu)
> **Tarih:** 21 Temmuz 2026
> **Durum:** Tespit, Bağımsızlaştırma ve Düzeltme

---

# Hedef Mimari

İdeal (hedef) mimari şudur. Tüm düzeltme çalışmaları bu mimariye ulaşmak için yapılır.

```
┌─────────────────────────────────────────────────────────────────┐
│                        DOMAIN (DNS/SSL)                         │
│  goldbanyo.com.tr              vizitlink3d.com.tr               │
│  (Gold Banyo kurumsal site)    (SaaS 3D platformu)              │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────┴──────────────────────────────────────┐
│                      REVERSE PROXY                              │
│          nginx / YARP (port bazlı yönlendirme)                   │
└────┬──────────────────────────┬─────────────────────────────────┘
     │                          │
┌────┴──────────────┐ ┌────────┴─────────────────────────────────┐
│  GOLD BANYO       │ │  3D SAAS PLATFORMU (vizitlink3d.com.tr)  │
│  (kurumsal)       │ │  (tamamen bağımsız — 5113/5115'e 0       │
│                   │ │   runtime/request/asset/auth/DB bağımlı)  │
│ ┌─────────────┐   │ │                                          │
│ │   5113      │   │ │  ┌──────────────────────────────────┐    │
│ │ Gold Banyo  │   │ │  │       5114                       │    │
│ │ Ana Site    │   │ │  │ 3D SaaS UI (MudBlazor)           │    │
│ │ (MudBlazor) │   │ │  │ Kendi layout/CSS/JS/media       │    │
│ │ Portali     │   │ │  │  /        → Public Viewer       │    │
│ │ Kurumsal    │   │ │  │  /admin   → Admin Studio        │    │
│ │ CMS         │   │ │  │  /embed   → Embed API           │    │
│ │ Blog vb.    │   │ │  └──────────┬───────────────────────┘    │
│ └──────┬──────┘   │ │             │                            │
│        │          │ │             ▼                            │
│ ┌──────┴──────┐   │ │  ┌──────────────────────────────────┐    │
│ │   5115      │   │ │  │  5116                           │    │
│ │ Gold Banyo  │   │ │  │ VizitLink3D.Konfigurator.Api    │    │
│ │ REST API    │   │ │  │ Kendi DB/migration/kullanicilar │    │
│ │ Cevap<T>    │   │ │  │ Cevap<T>/MediatR/CQRS           │    │
│ │ MediatR     │   │ │  │ Cookie Auth (BFF)               │    │
│ │ CQRS        │   │ │  │ (SADECE 5114'e hizmet eder)    │    │
│ │ JWT Auth    │   │ │  └──────────────────────────────────┘    │
│ │ Firma       │   │ │                                          │
│ │ Multi-Firma │   │ │                                          │
│ └─────────────┘   │ │                                          │
└───────────────────┘ └──────────────────────────────────────────┘
                      ┌──────────────────────────┐     ┌──────────────────┐
                      │  VizitLink3D.Ortak       │ ◄──►│  Core Motor      │
                      │  (C# paylasimli proje)   │     │  (Three.js)      │
                      │  - Cevap<T>              │     │  - GLTF Loader   │
                      │  - DilServisi            │     │  - Raycaster     │
                      │  - Guvenlik              │     │  - Parça Seçim   │
                      └──────────────────────────┘     │  - Renk/Mat      │
                                                       │  - Animasyon     │
                                                       └──────────────────┘
```

## Port Görev Dağılımı (Hedef)

| Port | Uygulama | Sorumluluk | Kimlik |
|------|----------|------------|--------|
| **5113** | `VizitLink3D.UI` — Gold Banyo | Kurumsal site, portal, blog, CMS, iletişim, ürün kataloğu, Gold Banyo markasına ait 3D görüntüleme | **goldbanyo.com.tr** |
| **5114** | `VizitLink3D.Konfigurator` — 3D SaaS Platformu (TAMAMEN bağımsız) | Public Viewer (`/`), Admin Studio (`/admin`, standalone), Embed API (`/embed`), JS SDK hosting, Model Yönetimi. Kendi layout/CSS/JS/media klasörü ile çalışır. Gold Banyo'dan iframe/redirect/proxy/Razor bileşeni/CSS/JS/marka/logoya bağımlı DEĞİLDİR. **SADECE 5116 API'sine bağlanır.** | **vizitlink3d.com.tr** — bağımsız SaaS |
| **5115** | `VizitLink3D.Api` | REST API: MediatR/CQRS, JWT, Firma (multi-firma), Passkey, Veritabanı, Auth. **SADECE Gold Banyo (5113) hizmet verir. 3D endpoint'leri burada değildir.** | **goldbanyo.com.tr** — yalnız Gold Banyo |
| **5116** | `VizitLink3D.Konfigurator.Api` | **YENİ bağımsız 3D backend.** Kendi DB/migration/kullanıcıları. MediatR/CQRS, Cevap\<T\>, Cookie Auth (BFF), kendi veritabanı. 5115 API'si veya 5113 ile hiçbir runtime/request/asset/auth/DB bağımlılığı yoktur. | **vizitlink3d.com.tr** — 3D SaaS API |

## Hedef Akışlar

1. **Gold Banyo kullanıcısı** → `goldbanyo.com.tr` (5113) → Ürün sayfasında Gold Banyo'ya ait 3D Viewer → API çağrıları 5115'e gider
2. **SaaS müşterisi admini** → `vizitlink3d.com.tr/admin` (5114) → Kendi layout/CSS/JS/media'sı ile Studio → model/parça/renk yönetimi → API 5116
3. **SaaS ziyaretçisi** → `vizitlink3d.com.tr/` (5114) → Public Viewer (bağımsız tema) → API 5116
4. **Üçüncü parti site** → `<iframe src="vizitlink3d.com.tr/embed/...">` → Embed Runtime (bağımsız) → API 5116
5. **Kritik kural:** 5114, 5113'ten iframe, redirect, proxy, Razor bileşeni, CSS/JS, marka, logo veya PhysicalFileProvider ile static asset ALMAZ. **SADECE 5116 API'sine bağlanır. 5115 API'sine veya 5113'e hiçbir runtime/request/asset/auth/DB bağımlılığı yoktur.**

## Hedef Route Yapısı (5114)

| Route | Tip | Açıklama |
|-------|-----|----------|
| `/` | Public | Firma ana sayfası, ürün listesi, 3D Viewer |
| `/admin` | Standalone | Admin Studio (oturum açma, model/parça/yapılandırma yönetimi) |
| `/admin/studio` | Standalone | 3D model düzenleyici (Three.js) |
| `/embed` | Minimal | iframe için sıfır UI, sadece canvas |
| `/embed/{token}` | Minimal | Nonce/token korumalı embed oturumu |
| `/api/*` | (proxy 5116) | İsteğe bağlı proxy |

---

# Mevcut Gerçek Durum

Aşağıdaki tablo, projenin **bugün itibarıyla** (21 Temmuz 2026) gerçek durumunu göstermektedir.

## Port Bazında Durum

| Port | Mevcut Durum | Açıklama |
|------|-------------|----------|
| **5113** (Gold Banyo UI) | ✅ Çalışır durumda | MudBlazor ile kurumsal site, portal, CMS, blog, ürün kataloğu çalışıyor. 3D ile ilgili **Studio ve Public Viewer Razor sayfaları burada** barınıyor. `VizitLink3D.UI` projesinin ana instance'ı. |
| **5114** (3D SaaS) | ❌ Kritik teknik borç | `VizitLink3D.Konfigurator` — minimal ASP.NET static host. **PhysicalFileProvider** ile `VizitLink3D.UI/wwwroot/goldbanyo/` root `/` olarak servis edilir — bu, 5114'ün Gold Banyo statik dosyalarına **bağımlı** olduğu anlamına gelir. Root'ta Gold Banyo'ya ait `index.html` Three.js ekranı vardır. Admin sayfaları **yalnızca geliştirme amaçlı iframe bridge** ile 5113'teki Studio'yu (`/admin/konfigurator-studio`) gösterir. **Production'da `/admin` 404 döner.** Public route (`/`) için Three.js statik ekranı mevcuttur ancak API'ye bağlı runtime değildir. Embed route (`/embed`) tanımlı değildir. **5114 static runtime API çağrısı yapmaz. 5114'ün kendine ait bağımsız layout/CSS/JS/media klasörü YOKTUR.** 3D backend olarak **5115'e değil, yeni 5116 API'sine** bağlanacaktır. |
| **5115** (API) | ✅ Çalışır durumda | MediatR/CQRS, JWT auth, FluentValidation, Cevap\<T\>, migration altyapısı mevcut. Firma bazlı tenant izolasyonu (KiraciServisi) ve Passkey/WebAuthn altyapısı kısmen mevcut. **5115 artık 3D'ye hizmet vermez — sadece Gold Banyo (5113) içindir.** 3D'ye ait endpoint'ler yeni 5116 API'sine taşınacaktır. FusionCache paketi **projede bulunmamaktadır.** |
| **5116** (3D API) | ❌ Henüz yok | `VizitLink3D.Konfigurator.Api` — yeni bağımsız 3D backend projesi. Henüz oluşturulmamıştır. Kendi DB/migration/kullanıcıları, kendi auth (cookie/BFF), 5113/5115'e sıfır bağımlılık. |

## 5114 Detaylı Durum

| Bileşen | Durum | Detay |
|---------|-------|-------|
| `/` (public landing) | ⚠️ Kısmen | Static host olarak `wwwroot/goldbanyo/index.html` Three.js konfigüratörü çalışır — model/texture seçimi, HDR/PBR ayarları, parça seçimi, renk/malzeme/doku değiştirme gibi kullanıcı etkileşimleri mevcuttur. **Ancak** 5116 API'sine bağlı (tenant/admin metadata, dinamik ürün) runtime değildir; tüm veri yerel statik katalog/GLB/doku dosyalarından yüklenir. |
| `/admin` (standalone) | ❌ Yok (üretimde) | Geliştirmede iframe bridge ile 5113'teki Studio'yu gösterir. **Üretimde 404.** Gerçek standalone admin değildir. |
| `/embed` | ❌ Yok | Route tanımlı değil. Embed JS SDK hazırlığı yok. |
| Admin Studio Razor | ❌ 5114'te yok | Studio Razor bileşenleri **5113'te** (`VizitLink3D.UI` ana instance). 5114 bunlara iframe ile erişiyor (sadece Development). |
| Public Viewer Razor | ❌ 5114'te yok | Public Viewer Razor bileşenleri **5113'te**. 5114'te bağımsız bir kopyası yok. |
| 3D Static Runtime | ⚠️ Var ama kopuk | `wwwroot/goldbanyo/` altında Three.js ile ilgili statik dosyalar (HTML/JS) mevcut. Statik `index.html` Three.js konfigüratörü (model/texture seçimi, HDR/PBR ayarları, parça/material/doku değiştirme) katalog/GLB/doku dosyalarıyla yerel olarak çalışır. Ancak Razor sayfaları veya API entegrasyonu yoktur — 5116 API'den tenant/admin metadata ve dinamik ürün çekmez. |
| Admin JS (iframe bridge) | ⚠️ Çalışıyor (sadece Dev) | 5114 admin açıldığında, 5113'teki Studio'yu iframe'de gösteren bir köprü JS mevcut. Bu **geçici** ve **yanlış** çözüm. Production'da çalışmaz. |
| GLB Model Yönetimi | ❌ Seed/Import yok | 9 adet GLB dosyası `wwwroot/medya/3d-modeller/` altında mevcut ancak **DB seed verisi yok**. Admin panelinden model import/seed mekanizması çalışmıyor. Kullanıcı yeni GLB yükleyemiyor. |

## 5113'teki 3D Bileşenleri

3D Konfigüratör'e ait tüm Razor bileşenleri şu anda **5113** portunda barınıyor:

| Bileşen | Dosya | Açıklama |
|---------|-------|----------|
| Admin Studio | `VizitLink3D.UI/Pages/Admin/KonfiguratorStudio.razor` + `.razor.cs` | Model/parça/renk/metadata düzenleme sayfaları |
| Public Viewer | `VizitLink3D.UI/Pages/Konfigurator/KonfiguratorPublic.razor` + `.razor.cs` | Ürün sayfalarında gömülü 3D görüntüleyici |

Bu bileşenler **Gold Banyo markasına sıkıca bağlıdır**; MudBlazor temaları, layout'ları ve authorization yapısı Gold Banyo'ya aittir. 5114 bu bileşenleri KULLANMAYACAK, kendi bağımsız sürümlerini oluşturacaktır.

## Veritabanı Durumu

Gerçek entity adları kullanılmıştır. Aşağıda belirtilmeyen hiçbir varlık (ör. `ParcaRenk`, `ParcaKonum`, `ModelDosyasi`, `EmbedNonce` entity olarak) **projede bulunmamaktadır.**

> **⚠️ 5116 bağımsızlaştırması sonrası:** Aşağıdaki tablodaki 3D entity'leri (`UrunUcBoyutModeli`, `UrunUcBoyutParcasi`, `UrunUcBoyutSahneOnayari`, `MusteriKonfigurasyonu`, Embed Nonce mekanizması, `FirmaApiAnahtari`) **5115'ten 5116'ya taşınacaktır**. 5116 kendi DB'sine (`vizitlink3d-konfigurator.db`) ve kendi migration geçmişine sahip olacaktır. 5115'te sadece Gold Banyo'ya ait entity'ler (`Firma`, `Kullanici`, `MenuOgeleri`) kalacaktır.

| Varlık | Durum (5115) | Hedef (5116) | Detay |
|--------|-------------|--------------|-------|
| `Firma` | ✅ Mevcut (5115'te kalır) | Yok | Migration var, entity tanımlı. Multi-firma izolasyonu KiraciServisi ile yapılır. **5116 kendi kullanıcı sistemine sahiptir; Firma entity'sini kullanmaz.** |
| `Kullanici` | ✅ Mevcut (5115'te kalır) | Yok | Migration var, Gold Banyo auth altyapısı. 5116 kendi kullanıcılarına sahiptir. |
| `UrunUcBoyutModeli` | ✅ Mevcut (5116'ya taşınacak) | ✅ Yeni DB'de | Migration var. 3B ürün modeli entity'si. **5116'da kopyalanacak/taşınacak.** |
| `UrunUcBoyutParcasi` | ✅ Mevcut (5116'ya taşınacak) | ✅ Yeni DB'de | Migration var. 3B ürün parçaları entity'si. **5116'da kopyalanacak/taşınacak.** |
| `UrunUcBoyutSahneOnayari` | ⚠️ Kısmen (5116'ya taşınacak) | ✅ Yeni DB'de | Migration var. Sahne onay/metadata entity'si. **5116'da yeniden yazılacak.** |
| `FirmaApiAnahtari` | ✅ Mevcut (5115'te kalır) | Yok | Firma bazlı API anahtarı yönetimi. **5116 kendi API anahtarı yönetimine sahip olacak.** |
| `MusteriKonfigurasyonu` | ⚠️ Kısmen (5116'ya taşınacak) | ✅ Yeni DB'de | Migration var. Müşteri konfigürasyon kayıtları. **5116'da yeniden yazılacak.** |
| Embed Nonce Mekanizması | ⚠️ Kısmen (5116'ya taşınacak) | ✅ Yeni DB'de | `EmbedNonceDeposu` servisi ve `EmbedOturumNonceKaydi` entity'si mevcuttur. **5116'ya taşınacak.** |
| **Seed Data (GLB)** | ❌ Yok | ❌ Yok (yeni kurulacak) | 9 GLB dosyası için DB seed migration'ı yok. 5116'da kurulacak. |
| Migration Geçmişi | ✅ Mevcut | ❌ Yok (yeni kurulacak) | EF Core migration geçmişi 5115'te sağlıklı. 5116 sıfırdan migration geçmişine başlayacak. |

---

# Yanlış ve Eksik Kısımlar

## Kritik Yanlışlar

### Y1: 5114 PhysicalFileProvider ile Gold Banyo Static Asset Bağımlılığı (KRİTİK)

**Sorun:** `VizitLink3D.Konfigurator`/5114, `Program.cs`'te PhysicalFileProvider kullanarak `VizitLink3D.UI/wwwroot/goldbanyo/` dizinini root `/` olarak servis eder. Bu:
- 5114'ü Gold Banyo'nun statik dosyalarına (HTML, JS, CSS, GLB, texture) **sıkıca bağımlı** kılar
- 5114'ün bağımsız deploy edilmesini engeller (5113 olmadan çalışmaz)
- Gold Banyo marka/logosunu 5114'te zorunlu kılar
- SaaS müşterilerine generic/bağımsız bir ürün sunmayı imkânsız hâle getirir
- Asset versiyonlama ve cache politikasını 5113'e bağlar

**Olması gereken:** 5114, kendi `wwwroot/` dizini altında bağımsız layout/CSS/JS/media klasörüne sahip olmalıdır. PhysicalFileProvider ile dış projeye bağımlılık TAMAMEN KALDIRILMALIDIR.

### Y2: 5114 Static Runtime — Gold Banyo Static Kopyası, API'den Kopuk

**Sorun:** 5114, PhysicalFileProvider ile Gold Banyo'nun `wwwroot/goldbanyo/` dizinini servis eder. Buradaki Three.js statik dosyaları (HTML/JS/CSS) Gold Banyo markasına aittir. API endpoint'leri ile bağlantısı yoktur. Statik `index.html` doğrudan sunulur ancak arka uç entegrasyonu bulunmaz. 5114'ün kendine ait bağımsız bir web kökü (layout/CSS/JS/media) yoktur.

**Olması gereken:** 5114, Gold Banyo'nun statik dosyalarını KULLANMAMALIDIR. Kendi `wwwroot/` altında bağımsız layout, CSS, JS, medya klasörlerini oluşturmalıdır. Public Viewer ve Admin Studio sayfaları, **5116 API'sinden** model/metadata verilerini çekmelidir.

### Y3: Studio/Public Bileşenler Sadece 5113'te ve Gold Banyo'ya Bağımlı

**Sorun:** 3D konfigüratörün temel Razor bileşenleri (Admin Studio, Public Viewer) **sadece 5113'te** mevcut. Bu:
- Gold Banyo markasına sıkı bağımlılık yaratıyor
- SaaS müşterileri için generic tema uygulanamıyor
- Firma bazlı markalaştırma (logo, renk, font) mümkün olmuyor
- Bağımsız bir SaaS ürünü olarak paketlenemiyor
- 5114'ün bu bileşenleri kullanması mümkün değil (iframe bridge başarısız oldu)

**Olması gereken:** 5114, KENDİ bağımsız Public Viewer ve Admin Studio bileşenlerini sıfırdan yazmalıdır. Gold Banyo'ya özel sürüm 5113'te kalır. 5114, 5113'ten Razor bileşeni, layout, CSS/JS, marka veya logo KULLANMAZ.

### Y4: 5114'te Route Yapısı Eksik ve Gold Banyo Route'larına Bağımlı

**Sorun:** 5114'te `/admin` (gerçek standalone) ve `/embed` route'ları ya yok ya da yanlış implemente edilmiş. Root `/`, PhysicalFileProvider ile Gold Banyo'nun `index.html`'sini gösterir. 5114'ün kendine ait bağımsız route yapısı (Razor sayfaları, layout, auth) bulunmaz.

**Olması gereken:** `Program.cs`'te route'lar 5114'e özgü, bağımsız şekilde tanımlanmalıdır. Root `/` kendi Public Viewer'ına, `/admin` kendi Admin Studio'suna yönlendirmelidir. Gold Banyo route'ları veya dosyaları kullanılmamalıdır.

### Y5: 5115 API'si 3D ve Gold Banyo Arasında Paylaşılıyor (KRİTİK — KARAR DEĞİŞTİ)

**Sorun:** Mevcut yapıda 5115 API'si hem Gold Banyo'ya (5113) hem de 3D SaaS'a (5114) hizmet vermektedir. Bu:
- İki farklı ürünün (kurumsal site vs. SaaS platformu) aynı DB'yi, aynı kullanıcı tabanını ve aynı auth mekanizmasını (JWT) paylaşmasına yol açar
- 3D platformunun bağımsız ölçeklenmesini, deploy edilmesini ve version'lanmasını engeller
- Gold Banyo'daki bir değişikliğin 3D platformunu etkilemesine (ve tersi) neden olur
- İki ürünün farklı güvenlik politikaları uygulamasını imkânsız kılar

**Kullanıcı kararı:** 3D proje Gold Banyo'dan TAMAMEN bağımsızdır. 5115 API'si 3D tarafından kullanılmayacaktır. Yeni bağımsız backend `VizitLink3D.Konfigurator.Api`, port 5116, kendi DB/migration ve kendi kullanıcıları ile kurulacaktır.

**Olması gereken:** 
- 5115, SADECE Gold Banyo'ya (5113) hizmet verir
- 5116 (`VizitLink3D.Konfigurator.Api`) yeni bağımsız 3D backend olarak kurulur
- 5114, SADECE 5116'ya çağrı yapar
- 5116'nın 5113/5115'e hiçbir runtime/request/asset/auth/DB bağımlılığı yoktur
- İki API (5115 ve 5116) ayrı DB'ler, ayrı kullanıcı tabanları ve ayrı auth mekanizmaları kullanır

### Y6: İlk Yönetici Credentials için Güvenli Provisioning Yok (KRİTİK)

**Sorun:** İlk yönetici hesabının oluşturulması için güvenli bir mekanizma tanımlanmamıştır. Seed verisi, kod içinde sabit şifre veya log'a dökülen credential'lar güvenlik ihlali oluşturur.

**Olması gereken:**
- İlk yönetici credentials **ASLA** kod, seed veya log içinde olmayacak
- **Development:** .NET user-secrets veya ortam değişkeni ile ilk yönetici provisioning
- **Production:** Secret store (Azure Key Vault / AWS Secrets Manager / HashiCorp Vault)
- Oturum yönetimi: HttpOnly Secure SameSite cookie / BFF (Backend for Frontend)
- **JWT / localStorage KESİNLİKLE YASAK** — oturum sadece cookie tabanlı

## Önemli Eksikler

### E1: GLB Seed / Import Mekanizması Yok

Hedefte 5116 kendi DB'sine sahip olacağı için GLB seed mekanizması **5116'da** kurulacaktır.
- 9 adet GLB dosyası fiziksel olarak `wwwroot/medya/3d-modeller/` altında mevcut, ancak 5116 DB'sinde kayıt yok
- Admin panelinde GLB import/seed sayfası ve iş akışı yok
- `UrunUcBoyutModeli` ve `UrunUcBoyutParcasi` tabloları 5116 DB'sinde sıfırdan oluşturulacak
- Sonuç: 9 GLB dosyası kullanılamaz durumda

### E2: Embed Route ve Güvenlik Nonce Akışı Eksik

5116'da embed nonce mekanizması sıfırdan kurulacaktır (5115'teki kopyalanmayacak). Mevcut durum:
- 5114'te `/embed` route'u yok
- Nonce oluşturma/doğrulama API endpoint'i 5116'da yok
- iframe embed JS SDK'sı yazılmamış
- CSP header'ları embed için optimize edilmemiş

### E3: API Anahtarı ile Embed Doğrulama Akışı Eksik

5116 kendi API anahtarı yönetimine sahip olacaktır (5115'teki `FirmaApiAnahtari` kullanılmayacak). Mevcut durum:
- Embed isteklerinde API anahtarı doğrulama middleware'i yok
- Rate limiting embed endpoint'lerinde aktif değil
- Firma bazlı API anahtarı rotasyonu ve iptali için admin UI yok

### E4: 5114 + 5116 Bağımsız Uygulama Olarak Yapılandırması Tamamlanmamış

5114 (`VizitLink3D.Konfigurator`) ve 5116 (`VizitLink3D.Konfigurator.Api`) ayrı projelerdir ancak:
- 5116 projesi henüz oluşturulmamıştır
- 5114, 5115'e değil 5116'ya bağlanacak şekilde yapılandırılmamıştır
- 5116'nın kendi `appsettings.json`'ı, kendi DB bağlantı dizesi ve kendi port/domain ayarları yoktur
- Bağımsız deploy stratejisi yok
- SSL sertifikası, domain bağlantısı (vizitlink3d.com.tr) henüz yapılmamış

### E5: Test Altyapısı 3D Senaryoları İçin Eksik

- 5116 API'si için Testcontainers testleri yok
- 3D model yükleme/test etme testleri yok
- Embed nonce güvenlik testleri yok
- 5116 kullanıcı/auth (cookie tabanlı) testleri yazılmamış
- API anahtarı doğrulama testleri yok

---

# Korunacak Yapılar

Aşağıdaki yapılar **mevcut haliyle doğru** ve **korunmalıdır**. Düzeltme çalışmaları bunları bozmamalı, aksine üzerine inşa etmelidir.

## 1. API ve Backend Altyapısı (5115 — Gold Banyo) — ✅ Kendi Alanında Korunacak

> **Not:** 5115'teki aşağıdaki yapılar Gold Banyo için doğru ve korunmalıdır. 5116 benzer desenleri kullanabilir (MediatR, Cevap\<T\>, FluentValidation) ancak **kod/fiziksel bağımlılık yoktur** — 5116 sıfırdan yazılır.

| Bileşen | 5115 (Gold Banyo) | 5116 (3D SaaS) |
|---------|-------------------|----------------|
| **MediatR / CQRS / Vertical Slice** | ✅ Korunur | ✅ Aynı desen, sıfırdan yazılacak |
| **Cevap\<T\>** | ✅ Korunur | ✅ `VizitLink3D.Ortak`'tan alınır |
| **FluentValidation** | ✅ Korunur | ✅ Eklenecek |
| **Mapster** | ✅ Korunur | ✅ Eklenecek |
| **HataYonetimiMiddleware** | ✅ Korunur | ✅ Eklenecek |
| **DilServisi** | ✅ Korunur | ✅ Eklenecek (kendi DB'sinde) |
| **Audit Alanları** | ✅ Korunur | ✅ Eklenecek |
| **Soft Delete** | ✅ Korunur | ✅ Eklenecek |

## 2. Veritabanı Varlıkları — ⚠️ Ayrıştırıldı

> 3D entity'leri 5115'ten **5116'nın kendi DB'sine** taşınır. Gold Banyo entity'leri 5115'te kalır.

| Entity | Sahibi | 5115 | 5116 |
|--------|--------|------|------|
| `Firma` | Gold Banyo | ✅ Kalır | ❌ Kullanılmaz (5116 kendi kullanıcı sistemine sahip) |
| `Kullanici` | Gold Banyo | ✅ Kalır | ❌ Kullanılmaz (5116 kendi kullanıcıları) |
| `UrunUcBoyutModeli` | 3D SaaS | ➡️ Taşınır | ✅ Yeni DB'de kurulacak |
| `UrunUcBoyutParcasi` | 3D SaaS | ➡️ Taşınır | ✅ Yeni DB'de kurulacak |
| `UrunUcBoyutSahneOnayari` | 3D SaaS | ➡️ Taşınır | ✅ Yeni DB'de yeniden yazılacak |
| `FirmaApiAnahtari` | Gold Banyo | ✅ Kalır | ❌ Kullanılmaz (5116 kendi API anahtarı yönetimi) |
| `MusteriKonfigurasyonu` | 3D SaaS | ➡️ Taşınır | ✅ Yeni DB'de yeniden yazılacak |
| `MenuOgeleri` | Gold Banyo | ✅ Kalır | ❌ Kullanılmaz |

## 3. Güvenlik Altyapısı — ⚠️ Ayrıştırıldı

### 5115 (Gold Banyo) — ✅ Mevcut Haliyle Korunur

| Bileşen | Açıklama |
|---------|----------|
| **Passkey (FIDO2/WebAuthn)** | WebAuthnPublicKey alani entity'de mevcut; tam FIDO2 akisi denetlenmeli |
| **BCrypt Hash** | Şifre hash'leme |
| **JWT Access + Refresh Token** | Token tabanlı auth |
| **`[JsonIgnore]`** | Şifre/token/hash alanlarında zorunlu |
| **Firma Bazlı Auth** | Her kullanıcının firmasına göre yetkilendirme |

### 5116 (3D SaaS) — 🔄 Sıfırdan Kurulacak

| Bileşen | Yaklaşım |
|---------|----------|
| **Oturum Yönetimi** | **HttpOnly Secure SameSite cookie / BFF (Backend for Frontend)** — JWT/localStorage KESİNLİKLE YOK |
| **Auth Mekanizması** | Cookie tabanlı session auth; anti-CSRF token |
| **İlk Yönetici Provisioning** | **ASLA kod/seed/log içinde değil.** Development: .NET user-secrets/env secret. Production: secret store (Azure Key Vault / AWS Secrets Manager / HashiCorp Vault) |
| **Şifre Hash** | BCrypt (Ortak'tan) |
| **`[JsonIgnore]`** | Tüm hassas alanlarda |
| **API Anahtarı** | 5116'ya özgü, sıfırdan tasarlanacak |
| **Embed Nonce** | 5116'da sıfırdan tasarlanacak |

## 4. Mevcut 9 GLB Dosyası — ✅ Korunacak (Fiziksel)

Fiziksel dosyalar `wwwroot/medya/3d-modeller/` altında mevcut. 5116 DB seed mekanizması kurulana kadar olduğu gibi kalacak, **silinmeyecek, taşınmayacak**. 5116 kendi DB'sinde bu dosyaları referans alacak seed migration'ını oluşturacaktır.

## 5. 5113'teki Mevcut 3D Bileşenleri (Gold Banyo) — ⚠️ Olduğu Gibi Kalacak

5113'teki Studio (`KonfiguratorStudio.razor`) ve Public Viewer (`KonfiguratorPublic.razor`) Razor bileşenleri **silinmeyecek**. 5113, Gold Banyo markasına ait 3D gösterimi için bu bileşenleri kullanmaya devam edecek (5115 API'si üzerinden). 5114'teki yeni generic bileşenler **ayrı olarak yazılacak** (taşıma değil, yeniden yazım) ve 5116 API'sine bağlanacak.

## 6. Proje Yapısı ve Klasör Düzeni — ✅ Güncellenecek

| Klasör / Proje | Port | Açıklama |
|----------------|------|----------|
| `VizitLink3D.Api/` | 5115 | Gold Banyo REST API (SADECE 5113'e hizmet) |
| `VizitLink3D.UI/` | 5113 | Gold Banyo ana site (MudBlazor) |
| `VizitLink3D.Konfigurator/` | 5114 | Bağımsız 3D SaaS Platformu UI (kendi layout/CSS/JS/media) |
| `VizitLink3D.Konfigurator.Api/` | **5116** | **YENİ** — Bağımsız 3D SaaS Backend (kendi DB/kullanıcı/auth) |
| `VizitLink3D.Ortak/` | — | Paylaşılan C# projesi (Cevap\<T\>, DilServisi, Guvenlik vb.) |
| `VizitLink3D.Testler/` | — | Test projesi — xUnit + Testcontainers |
| `Moduller/` | — | Vertical Slice yapısı (5115 ve 5116'da ayrı ayrı) |
| `AjanKurallari/` | — | AI ajan kuralları |
| `dokumantasyon/` | — | Mimari kararlar ve planlama |
| `Yedekler/` | — | DB ve konfig yedekleri |

---

## Özet Tablo: Düzeltme Öncelikleri

| Öncelik | Madde | Etki | Zorluk |
|---------|-------|------|--------|
| 🔴 P0 | 5116 `VizitLink3D.Konfigurator.Api` projesini oluştur (kendi DB/migration/kullanıcı/auth) | Kritik — yeni backend ön koşulu | Yüksek |
| 🔴 P0 | 5114 Gold Banyo bağımlılığını kaldır (PhysicalFileProvider + iframe bridge + asset bağımlılığı) | Kritik — bağımsızlık ön koşulu | Yüksek |
| 🔴 P0 | Bağımsız 5114 route yapısı (kendi Public Viewer `/`, Admin `/admin`, Embed `/embed`) | Kritik — kullanılamaz durum | Orta |
| 🔴 P0 | 5116'da Cookie Auth (BFF) + ilk yönetici provisioning (user-secrets/env) | Kritik — güvenlik | Orta |
| 🟠 P1 | 5114 static runtime → 5116 API bağlantısı | Yüksek — kopuk durumda | Yüksek |
| 🟠 P1 | 5116'da GLB seed/import mekanizması | Yüksek — 9 model kullanılamıyor | Orta |
| 🟡 P2 | Studio Razor bileşenlerini 5114'e generic olarak yazma | Orta — SaaS bağımsızlığı | Çok Yüksek |
| 🟡 P2 | Embed route + nonce + JS SDK (5116'da) | Orta — embed ürün vaadi | Yüksek |
| 🟢 P3 | API anahtarı doğrulama middleware + rate limiting (5116'da) | Düşük — güvenlik katmanı | Düşük |
| 🟢 P3 | Testler (5116 API, embed, kullanıcı/auth, nonce) | Düşük — kalite güvencesi | Orta |
| 🟢 P3 | 5114 + 5116 bağımsız appsettings + deploy | Düşük — DevOps | Düşük |

---

# Kesin Uygulama Sırası ve Kabul Kriterleri

Bu bölüm, yukarıdaki düzeltme planını **adım adım, bağımlılık sırasıyla** uygulamak için tanımlanmıştır. Her aşama (A0–A8), bir önceki aşamanın çıktısına dayanır. Aşamalar arasında atlama yapılmaz.

---

## A0 — 5116 `VizitLink3D.Konfigurator.Api` Projesini Oluştur

| Başlık | Detay |
|--------|-------|
| **Amaç** | Yeni bağımsız 3D backend projesini (`VizitLink3D.Konfigurator.Api`) oluştur. Kendi DB/migration/kullanıcıları, kendi auth (cookie/BFF), 5113/5115'e sıfır bağımlılık. |
| **Bağımlılık** | Yok (ilk adım) |
| **Değişen Katman(lar)** | Yeni proje: `VizitLink3D.Konfigurator.Api/`, yeni `Program.cs`, kendi `appsettings.json`, kendi DB context/migration, kendi kullanıcı entity'si |
| **Yapılacaklar** | ① `dotnet new webapi` ile `VizitLink3D.Konfigurator.Api` projesini oluştur (port 5116) ② `VizitLink3D.Ortak` proje referansını ekle (Cevap\<T\>, DilServisi vb. için) ③ Kendi `appsettings.json`'ını oluştur (port 5116, kendi DB bağlantı dizesi `vizitlink3d-konfigurator.db`) ④ Kendi `Program.cs`'ini yaz (MediatR, FluentValidation, Mapster, HataYonetimiMiddleware) ⑤ Kendi DB context'ini oluştur (`KonfiguratorDbContext`) ⑥ İlk migration'ı ekle (3D entity'leri: `UrunUcBoyutModeli`, `UrunUcBoyutParcasi` vb.) ⑦ Cookie Auth (BFF) yapılandırmasını ekle — HttpOnly Secure SameSite cookie, anti-CSRF ⑧ İlk yönetici provisioning mekanizmasını ekle (Development: user-secrets/env secret; Production: secret store) — **ASLA kod/seed/log içinde değil** ⑨ `launchSettings.json`'da 5116 portunu ayarla |
| **Test URL** | `http://localhost:5116/api/health` (health check) |
| **Kabul Kriteri** | Proje `dotnet build` ile derlenir. `http://localhost:5116/api/health` 200 OK döndürür. DB migration'ı `vizitlink3d-konfigurator.db` üzerinde çalışır. Cookie auth yapılandırması hazırdır. İlk yönetici provisioning kod/seed/log içinde GEÇMEZ. 5115 API'sine veya 5113'e hiçbir proje referansı/NuGet/using yoktur. |

---

## A1 — 5114 Gold Banyo Bağımlılığını Kaldır

| Başlık | Detay |
|--------|-------|
| **Amaç** | 5114'ün (`VizitLink3D.Konfigurator`) Gold Banyo'ya olan TÜM bağımlılıklarını kaldır: PhysicalFileProvider ile Gold Banyo static asset servisi, iframe bridge ile 5113 Studio gösterme, Gold Banyo marka/logosu kullanımı. 5114 kendi bağımsız web köküne kavuşur. |
| **Bağımlılık** | A0 tamamlanmalı (5116 API hazır olmalı) |
| **Değişen Katman(lar)** | 5114 `Program.cs` (PhysicalFileProvider kaldır, kendi static file hosting'e geç), 5114 `wwwroot/` (kendi layout, CSS, JS, medya klasörleri oluştur), 5114 `Pages/Admin/` (iframe bridge kaldır), 5114 admin JS dosyaları |
| **Yapılacaklar** | ① `Program.cs`'ten PhysicalFileProvider ile Gold Banyo static dosya referansını TAMAMEN KALDIR ② 5114'ün kendi `wwwroot/` dizini altında bağımsız klasör yapısı oluştur: `css/sistem/`, `css/temalar/`, `js/`, `medya/` ③ iframe bridge JS dosyasını temizle ④ 5114 admin Razor sayfalarındaki `<iframe>` etiketlerini kaldır ⑤ Gold Banyo logosu/marka referanslarını temizle ⑥ 5114'teki API çağrılarını 5115 yerine **5116'ya** yönlendir ⑦ Sayfalar artık Gold Banyo'ya bağımlı olmadan açılıyor mu kontrol et |
| **Test URL** | `http://localhost:5114/` (kendi bağımsız sayfası), `http://localhost:5114/admin` (iframe'siz) |
| **Kabul Kriteri** | 5114'ün `Program.cs`'inde `PhysicalFileProvider` veya `VizitLink3D.UI/wwwroot/goldbanyo` referansı kalmaz. `5114/admin` açıldığında hiçbir iframe yüklenmez. Gold Banyo'ya ait logo/marka/css/js dosyası 5114 üzerinden servis edilmez. 5114 kendi wwwroot/ klasörüne sahiptir. 5113 portuna giden HTTP isteği olmaz. **Tüm API istekleri 5116'ya gider (5115'e değil).** |

---

## A2 — Bağımsız 5114 Public Site + Admin Host Temeli

| Başlık | Detay |
|--------|-------|
| **Amaç** | 5114'te (`VizitLink3D.Konfigurator` üzerinde), 5113'ten TAMAMEN bağımsız bir Blazor/MudBlazor uygulaması çalıştır. Hem public kullanıcı sayfası (`/`) hem de admin sayfası (`/admin`) kendi layout/CSS/JS/media'sı ile çalışsın. Gold Banyo temasından bağımsız, firma bazlı markalaşmaya hazır generic tema. |
| **Bağımlılık** | A1 tamamlanmalı (Gold Banyo bağımlılığı kalkmalı) |
| **Değişen Katman(lar)** | 5114 `Program.cs` (bağımsız route yapılandırması — Blazor component routing), 5114 `App.razor`, 5114 `MainLayout.razor`, 5114 `Pages/Public/` (yeni public sayfalar), 5114 `Pages/Admin/` (yeni admin sayfalar), 5114 `wwwroot/css/` (kendi tema/CSS), 5114 `appsettings.json` |
| **Yapılacaklar** | ① 5114 `Program.cs`'e bağımsız route yapısı ekle: `/` → Public Viewer, `/admin` → Admin Studio ② Generic admin layout oluştur (Gold Banyo logosu yerine VizitLink3D markalı) ③ Generic public layout oluştur ④ MudBlazor tema konfigürasyonu ekle ⑤ **5116 API'sine cookie auth (BFF) ile bağlanacak giriş sayfası yap** — JWT/localStorage KULLANILMAZ ⑥ Boş bir admin dashboard sayfası ekle ⑦ `appsettings.json`'da `"Port": 5114`, `"Uygulama": "3D-SaaS"` ve `"ApiBaseUrl": "http://localhost:5116"` anahtarlarını ayır ⑧ Kendi `tokens.css` ve temel CSS dosyalarını oluştur |
| **Test URL** | `http://localhost:5114/` (public landing), `http://localhost:5114/admin` (giriş sayfası), `http://localhost:5114/admin/dashboard` (giriş sonrası) |
| **Kabul Kriteri** | 5114/ ve 5114/admin aynı portta, ayrı layout'larla açılır. Hiçbir sayfada 5113 portuna ait referans (iframe, script, link, CSS, JS, medya) bulunmaz. Public sayfa kendi layout/CSS/JS'sini kullanır. Admin sayfası kendi layout/CSS/JS'sini kullanır. F12 Network sekmesinde tüm istekler **`localhost:5116`** API'sine gider, `localhost:5113` veya `localhost:5115`'e giden istek *yoktur*. Gold Banyo logosu/markası görünmez. Giriş sayfası JWT değil cookie auth kullanır. |

---

## A3 — 9 GLB Model 5116 DB Import / Seed

| Başlık | Detay |
|--------|-------|
| **Amaç** | Mevcut 9 adet GLB dosyasını (`wwwroot/medya/3d-modeller/`) **5116 DB'sine** seed et. `UrunUcBoyutModeli`, `UrunUcBoyutParcasi`, `UrunUcBoyutSahneOnayari` tabloları **5116'da** oluşturulur. Admin panelinden manuel import sayfası ekle. |
| **Bağımlılık** | A2 tamamlanmalı (admin host ayakta olmalı) |
| **Değişen Katman(lar)** | 5116 API (yeni seed migration'ı, import endpoint'leri), 5114 admin (import sayfası), 5116 DB (`vizitlink3d-konfigurator.db` — yeni seed migration'ı) |
| **Yapılacaklar** | ① Her GLB dosyası için `UrunUcBoyutModeli` seed migration'ı yaz (DosyaAdi, OlusturulmaTarihi) — **FirmaId yok, 5116 kendi kullanıcı sistemine sahip** ② GLB ile ilişkili `UrunUcBoyutParcasi` seed verisi ekle ③ Her ürünün parçaları ve sahne onayları için seed migration'ı yaz ④ 5114 admin'de "3D Model İçe Aktar" sayfası yap (dosya seçici + import butonu) ⑤ **5116'da** `POST /api/3d/model/import` endpoint'i ekle (GLB yükle + DB kaydı + dosya kopyalama) ⑥ Seed migration'ını 5116 DB'sine uygula |
| **Test URL** | `http://localhost:5116/api/3d/model/list` (seed sonrası liste), `http://localhost:5114/admin/3d/import` (import sayfası) |
| **Kabul Kriteri** | Seed migration'ı çalıştıktan sonra 5116 DB'sinde `UrunUcBoyutModeli` tablosunda 9 kayıt görünür. `GET /api/3d/model/list` (5116'da) 9 öğe döndürür. Admin import sayfasından yeni bir GLB yüklenebilir, yüklenen dosya `wwwroot/medya/3d-modeller/` altına kopyalanır ve 5116 DB'sine kaydedilir. **5115 DB'si değişmez.** |

---

## A4 — Mesh / Parça Metadata Editor (5116 API)

| Başlık | Detay |
|--------|-------|
| **Amaç** | 5114 admin studio'da, 3D model parçalarının (mesh) metadata'larını düzenlemek için bir editor sayfası oluştur. Parça adı, malzeme, renk seçenekleri, görünürlük, tıklanabilirlik gibi alanlar düzenlenebilir olsun. |
| **Bağımlılık** | A3 tamamlanmalı (seed verisi ile çalışılabilir) |
| **Değişen Katman(lar)** | 5114 admin (`Pages/Admin/Studio.razor` + `.cs`), **5116 API** (yeni endpoint'ler: `UrunUcBoyutParcasi` CRUD, metadata güncelleme), 5114 `wwwroot/js/` (Three.js mesh seçim JS'si — bağımsız kopya) |
| **Yapılacaklar** | ① 5114'te "3D Model Düzenleyici" sayfası oluştur (MudBlazor + Three.js canvas) ② **5116'da** `UrunUcBoyutParcasi` için CRUD endpoint'leri ekle: `GET/PUT /api/3d/parca/{id}`, `GET /api/3d/model/{id}/parcalar` ③ Three.js raycaster ile mesh tıklama/seçim işlevi yaz (JS tarafında) ④ Seçilen mesh'in metadata'sını düzenleme formu yap (parça adı, malzeme tipi, renk paleti) ⑤ Değişiklikleri **5116 API'sine** kaydet ⑥ Parça bazında renk değiştirme önizlemesi ekle |
| **Test URL** | `http://localhost:5114/admin/studio/{modelId}` (belirli bir modelin düzenleyicisi) |
| **Kabul Kriteri** | Admin Studio sayfasında 3D canvas yüklenir. GLB model görünür. Fare ile bir mesh parçasına tıklandığında parça seçilir (highlight edilir). Seçilen parçanın adı, malzeme tipi ve renk bilgisi formda görünür. Formdan renk değiştirildiğinde canvas'ta anlık önizleme güncellenir. "Kaydet" butonuna basıldığında `PUT /api/3d/parca/{id}` isteği **5116'ya** gider ve değişiklik kalıcı olur. |

---

## A5 — 5114 Public Runtime API Adapter (5116)

| Başlık | Detay |
|--------|-------|
| **Amaç** | 5114'teki bağımsız 3D runtime JS dosyalarını (kendi `wwwroot/js/`) **5116 API'sine** bağla. Public Viewer sayfası oluştur; model listesi, parça metadata'sı ve renk seçenekleri API'den çekilsin. Gold Banyo'nun statik dosyaları kullanılmaz, 5114'ün kendi JS motoru yazılır. |
| **Bağımlılık** | A3 tamamlanmalı (seed verisi olmalı), A4 tamamlanmalı (metadata düzenlenebilir olmalı) |
| **Değişen Katman(lar)** | 5114 `Pages/Public/Viewer.razor` (yeni), 5114 `wwwroot/js/public/` (API entegrasyon JS'si - bağımsız), 5114 `Program.cs` (`/` route'u), **5116 API** (model/parça endpoint'leri) |
| **Yapılacaklar** | ① 5114'te `/` (public landing) route'una bir Public Viewer Razor sayfası bağla ② Three.js canvas'ını sayfaya göm ③ 5114'ün kendi JS motor kodlarını (Gold Banyo'dan bağımsız) **5116 API'den** veri çekecek şekilde yaz ④ 5116 API üzerinden doğru model listesini getir ⑤ Public Viewer'da parça listesi, renk seçici, model değiştirme kontrolleri ekle ⑥ JS dosyalarını 5114'ün kendi wwwroot/js/ klasöründen referans al |
| **Test URL** | `http://localhost:5114/` (public viewer) |
| **Kabul Kriteri** | `http://localhost:5114/` açıldığında 3D canvas görünür. 5116 API'den model listesi çekilir (`GET /api/3d/model/list`). Kullanıcı bir modele tıkladığında GLB yüklenir ve canvas'ta görüntülenir. Parça seçilebilir, renk değiştirilebilir. Network sekmesinde tüm veri istekleri **`localhost:5116`**'ya gider. Statik JS dosyaları 5114 üzerinden yüklenir. |

---

## A6 — Hareket Metadata (Kapak / Sürgü / LED) — 5116 API

| Başlık | Detay |
|--------|-------|
| **Amaç** | 3D modellerdeki hareketli parçaların (kapak açma, sürgü çekme, LED yakma/söndürme) metadata'sını tanımla. Parça bazında animasyon dönüşümü (pozisyon, rotasyon, ölçek) ve LED için renk/parlaklık bilgisi ekle. |
| **Bağımlılık** | A4 tamamlanmalı (mesh editor çalışıyor olmalı), A5 tamamlanmalı (public viewer ayakta olmalı) |
| **Değişen Katman(lar)** | **5116 API** (hareket entity'si + migration), 5114 admin studio (hareket metadata editor UI), 5114 JS motor (animasyon motoru), 5116 DB (yeni migration) |
| **Yapılacaklar** | ① Hareket entity'si oluştur (ParcaId, HareketTipi [Kapak/Surgu/LED], TransformBilgisi (JSON), SureMilisaniye, Tetikleyici) ② Migration ekle (5116 DB'sine) ③ **5116'da** hareket CRUD endpoint'leri ekle ④ 5114 admin studio'ya "Hareket Ekle/Düzenle" paneli ekle ⑤ Three.js animasyon motoruna hareket oynatma desteği ekle ⑥ Public viewer'da hareketli parçaları göster/kullan |
| **Test URL** | `http://localhost:5114/admin/studio/{modelId}` (hareket düzenleme), `http://localhost:5114/` (public'te hareket testi) |
| **Kabul Kriteri** | Admin Studio'da bir parça seçilir, "Hareket Ekle" ile kapak açma animasyonu tanımlanır. Kaydedilir. Public viewer'da aynı parçaya tıklandığında kapak animasyonu oynar. LED tipi harekette renk ve parlaklık değişir. Animasyonlar akıcıdır. |

---

## A7 — Embed / Docker / Deploy

| Başlık | Detay |
|--------|-------|
| **Amaç** | 5114 embed route'unu çalıştır (`/embed/{token}`). Nonce güvenlik akışını 5116'da tamamla. Embed JS SDK'yı yaz. Docker Compose yapılandırmasını güncelle (5113+5115+5114+5116 dörtlü ayağa kalksın). Production deploy için gerekli konfigürasyonu tamamla. |
| **Bağımlılık** | A5 tamamlanmalı (public runtime API'ye bağlı), A6 tamamlanmalı (hareket metadata embed'de de çalışmalı) |
| **Değişen Katman(lar)** | 5114 `Program.cs` (`/embed` route), 5114 `Pages/Embed/Viewer.razor` (yeni — sadece canvas, sıfır UI), **5116 API** (nonce oluşturma/doğrulama endpoint'leri), 5114 `wwwroot/js/embed/` (JS SDK — bağımsız), DevOps (`docker-compose.yml`, `Dockerfile`), 5114 + 5116 `appsettings.Production.json` |
| **Yapılacaklar** | ① 5114'te `/embed/{token}` route'u ekle (sadece Three.js canvas içeren minimal sayfa) ② **5116'da** nonce akışını kur: nonce oluşturma (`POST /api/3d/embed/nonce`), doğrulama middleware'i ③ Embed JS SDK dosyasını yaz ④ Docker Compose'a 5114 ve **5116** instance'larını ekle (mevcut 5113 + 5115 + yeni 5114 + yeni 5116) ⑤ Production `appsettings.json` hazırla (domain, SSL, CORS) ⑥ CSP header'larını embed için optimize et |
| **Test URL** | `http://localhost:5114/embed/{nonce}` (embed viewer) |
| **Kabul Kriteri** | Embed route'u açıldığında sadece 3D canvas görünür (header, footer, menü yok). Nonce doğrulama çalışır: geçerli nonce ile model görüntülenir, geçersiz/nonce süresi dolmuş token ile hata döner. Embed JS SDK harici bir siteye gömülebilir. Docker Compose ile `docker compose up` çalıştırıldığında 5113, 5114, 5115 ve **5116** aynı anda ayağa kalkar. CORS sadece izin verilen domain'lere açıktır. |

---

## A8 — Medya Havuzu ve Eksik 21 GLB Takibi

| Başlık | Detay |
|--------|-------|
| **Amaç** | Mevcut 9 GLB'ye ek olarak eksik 21 GLB modelin (toplam 30) takibini yap. Medya havuzu kategorizasyonunu denetle. 3D model dosyalarının `wwwroot/medya/3d-modeller/` altında düzenli olduğunu doğrula. Eksik modellerin listesini çıkar ve 5116 seed hazırlığı yap. |
| **Bağımlılık** | A3 tamamlanmalı (seed mekanizması 5116'da çalışıyor olmalı) |
| **Değişen Katman(lar)** | Dokümantasyon (model envanteri), 5114 admin (model durum takip sayfası), Medya havuzu fiziksel düzeni |
| **Yapılacaklar** | ① 9 GLB'nin güncel envanterini çıkar (dosya adı, boyut) ② Eksik 21 GLB için "istenen model listesi" oluştur ③ 5114 admin'de "Model Durum Takibi" sayfası yap ④ Medya havuzu klasör yapısını denetle ⑤ Eksik modeller geldikçe 5116 seed mekanizmasına hazırlık ⑥ Fiziksel dosya ←→ 5116 DB kaydı tutarlılık raporu çıkart |
| **Test URL** | `http://localhost:5114/admin/3d/durum` (durum takip sayfası) |
| **Kabul Kriteri** | 9 mevcut modelin her biri için **5116 DB'sinde** kayıt vardır ve dosya fiziksel olarak yerindedir. Eksik 21 modelin listesi bir tabloda görünür. Admin durum sayfası her modelin durumunu gösterir. Toplu yükleme sayfası (batch import) çalışır. Yetim dosya veya yetim DB kaydı raporu sıfırdır. |

---

## Aşamalar Arası Bağımlılık Grafiği

```
A0 (5116 Konfigurator.Api projesini oluştur — kendi DB/migration/kullanici/auth)
 │
 ▼
A1 (5114 Gold Banyo bağımlılığını kaldır — PhysicalFileProvider + iframe + asset)
 │
 ▼
A2 (bağımsız 5114 public site + admin host temeli) ◄── A1 ön koşul
 │
 ├──► A3 (5116 DB'de 9 GLB seed/import) ◄── A2 admin host gerekli
 │      │
 │      ├──► A4 (mesh metadata editor — 5116 API) ◄── seed verisi gerekli
 │      │      │
 │      │      └──► A6 (hareket metadata — 5116 API) ◄── mesh editor gerekli
 │      │
 │      └──► A5 (public runtime API — 5116 API) ◄── seed verisi gerekli
 │             │
 │             └──► A6 (hareket — public görüntüleme) ◄── public viewer gerekli
 │
 └──► A7 (embed/Docker/deploy — 4 port) ◄── public viewer + hareket gerekli
 │
 └──► A8 (medya/21 GLB takibi — 5116 DB) ◄── seed mekanizması gerekli
```

**Not:** A8, A3'ün seed mekanizması çalıştıktan sonra başlayabilir; A4/A5/A6/A7 ile paralel yürütülebilir. A0→A1→A2→A3 sırası **kesindir**, atlanamaz. A4 ve A5, A3'ten sonra **paralel** başlatılabilir. A6, A4 ve A5'in her ikisini de gerektirir. A7, A5 ve A6'yı gerektirir.

---

# Yapılmayacaklar

Bu bölüm, düzeltme planı kapsamında **kesinlikle yapılmayacak** şeyleri listeler. Sebepsiz değil; her biri daha önce test edilmiş ve başarısız/yıkıcı/yanlış olduğu kanıtlanmış yaklaşımlardır.

| # | Yapılmayacak Şey | Gerekçe |
|---|------------------|---------|
| 1 | **5113 ana siteye (Gold Banyo) dokunmak** — 5113'teki 3D Razor sayfalarını değiştirmek, taşımak veya silmek | 5113 üretimde çalışıyor ve kurumsal siteye hizmet veriyor. 3D ile ilgili mevcut hali olduğu gibi kalır (5115 API'si üzerinden). Tüm yeni çalışma sadece 5114 + 5116'ya yapılır. |
| 2 | **5114'te Gold Banyo static asset, marka, logo, CSS/JS, layout veya Razor bileşeni KULLANMAK** — PhysicalFileProvider, iframe bridge, redirect, proxy, CSS import, JS referansı, logo görseli, layout kalıtımı ile Gold Banyo'ya ait herhangi bir kaynağı kullanmak | 5114 TAMAMEN bağımsız bir SaaS ürünüdür. Kendi layout/CSS/JS/media klasörüne sahip olmalıdır. Gold Banyo'dan SADECE teknoloji ve kalite kuralları (C#/.NET 10, Blazor, MudBlazor, CSS tokens, Türkçe adlandırma, güvenlik, test, medya havuzu, soft-delete) alınır. Fiziksel asset/logomarka bağımlılığı KESİNLİKLE YASAKTIR. |
| 3 | **GLB mesh yapısı, kapak mekanizması, pivot noktaları hakkında tahmin yürütmek** — "Bu GLB'nin kapak parçası şudur" gibi AI tahminiyle kod/metadata üretmek | GLB dosyaları analiz edilmemiştir. Mesh adları, hiyerarşi, kapak pivot'u gibi bilgiler **yalnızca Blender/3D görüntüleyici ile elle incelenerek** veya kullanıcıdan alınarak girilir. AI tahmini + elle düzeltme döngüsü saatler kaybettirir. |
| 4 | **5116'da JWT veya localStorage kullanmak** — 5116 oturum yönetimi HttpOnly Secure SameSite cookie / BFF olmalıdır | 5116'da JWT token veya localStorage **KESİNLİKLE YASAKTIR**. Tüm oturum cookie tabanlı olmalıdır. 5115'te JWT kullanımı devam edebilir (Gold Banyo'nun kendi tercihi). |
| 5 | **İlk yönetici credentials'ı kod, seed veya log içinde saklamak** — Development: .NET user-secrets/env secret. Production: secret store. | Kod/seed/log içinde sabit credential güvenlik ihlalidir. İlk yönetici provisioning her ortamda güvenli bir kanaldan yapılmalıdır. |
| 6 | **5116'nın 5115 API'sine veya 5113'e runtime/request/asset/auth/DB bağımlılığı olması** | İki sistem (Gold Banyo ve 3D SaaS) tamamen bağımsızdır. 5116 kendi DB'sine, kendi kullanıcılarına, kendi auth mekanizmasına sahiptir. Sadece `VizitLink3D.Ortak` C# paylaşımlı projesi kullanılabilir (compile-time bağımlılık, runtime değil). |
| 7 | **GitHub'a push yapmak, commit atmak, branch açmak** | Bu dosya sadece durum tespiti ve plandır. Uygulama aşamasında kod değişikliği gerektiğinde Ustam ayrıca talimat verir. Bu doküman üzerinde yapılan hiçbir değişiklik push gerektirmez. |

# Kullanıcıdan Gereken Gerçek Bilgiler

Aşağıdaki bilgiler **AI tarafından tahmin edilemez**. Kullanıcının (Ustam'ın) bizzat sağlaması gerekir. Bu bilgiler olmadan A3–A6 arası sağlıklı ilerleyemez.

| # | Gerekli Bilgi | Hangi Aşamada | Neden Gerekli |
|---|---------------|---------------|---------------|
| 1 | **9 modelin her biri için: slug, görsel (katalog fotoğrafı), renk kodları** | A3 (seed) öncesi | Seed migration'ı 5116 DB'sinde `UrunUcBoyutModeli` tablosuna teknik GLB referansının yanı sıra kullanıcıya gösterilecek ad, açıklama, görsel yolu gibi alanları da yazmalı. Slug URL'de kullanılır; görsel ürün kartında gösterilir; renk kodları seed'de kullanılır. |
| 2 | **Kapak mekanizması bilgisi: hangi mesh hangi yönde ne kadar açılıyor, pivot noktası** | A6 (hareket metadata) öncesi | AI, GLB dosyasını açıp mesh adlarına bakarak "bu bir kapaktır, X ekseninde döner" diyemez. Her model için Blender'da veya Three.js canvas'ında elle inceleme yapılıp pivot, rotasyon ekseni ve açı miktarı belirlenmelidir. |
| 3 | **Eksik 21 GLB model listesi: hangi ürünlere ait, dosya adları, öncelik sırası** | A8 (medya takibi) öncesi | Hangi 21 modelin beklendiği net değil. Müşteriden/tedarikçiden temin edilecek modellerin listesi + teslim takvimi gerekir. Öncelik sırası (önce çok satan ürünler) A8 planlamasını etkiler. |

> **Not:** Bu bilgilerin bir kısmı (örneğin model slug'ları ve renk kodları) önceki sohbetlerde veya dosyalarda kısmen geçmiş olabilir. Ancak bu dokümanda **kesin ve eksiksiz** bir liste olarak derlenmemiştir. Ustam'dan rica: yukarıdaki 3 madde için mevcut bilgileriniz varsa paylaşın; yoksa hangi format/sıklıkta teslim edebileceğinizi belirtin.

# Riskler ve Karar Noktaları

Aşağıdaki kararlar alınmadan veya riskler yönetilmeden ilerlemek ileride yıkıcı değişiklik gerektirebilir.

| # | Risk / Karar | Açıklama | Kime Bağlı |
|---|-------------|----------|------------|
| 1 | **5114 + 5116 bağımsız proje yapısı** | 5114 = `VizitLink3D.Konfigurator`, 5116 = `VizitLink3D.Konfigurator.Api`. A0 ile 5116 sıfırdan kurulacak, A1+A2 ile 5114 Gold Banyo'dan TAMAMEN bağımsız hale getirilecek. Kendi layout/CSS/JS/media klasörü, kendi appsettings.json, kendi deploy akışı olacak. Uzun vadede ayrı bir repo/CID gerekip gerekmediği değerlendirilecek. | Ustam — uzun vadeli repo/CID kararı |
| 2 | **5114 + 5116 VizitLink3D markası** | 5114 ve 5116, Gold Banyo logosu/markası kullanmaz. VizitLink3D kendi markasına sahiptir. 5116'nın kendi kullanıcı sistemi vardır (Gold Banyo'nun Firma/Kullanici entity'lerini kullanmaz). | Ustam — VizitLink3D marka kimliği kararı |
| 3 | **5113'teki mevcut 3D sayfaları uzun vadede ne olacak?** | Şimdilik olduğu gibi kalıyor (5115 API'si üzerinden). İleride: (a) 5113, 5114'teki public viewer'ı iframe ile gömebilir (embed SDK ile, 5116 üzerinden), (b) 5113'teki sayfalar silinebilir (AjanKurallari emirlerine aykırı, Ustam onayı gerekir), (c) olduğu gibi kalır. | Ustam — uzun vadeli karar |
| 4 | **GLB dosyalarının mesh yapısı ve adlandırması standart mı?** | Her modelin mesh adları farklı olabilir. A4 mesh editor'ün çalışabilmesi için bir adlandırma standardı önerilir. Mevcut GLB'lerde bu standart yoksa A3 seed'den önce yeniden export gerekebilir. | Ustam + tasarımcı — lojistik karar |
| 5 | **Embed özelliği MVP için zorunlu mu?** | A7 embed + Docker + deploy içeriyor. Eğer MVP'de embed gerekmiyorsa A7 hafifletilebilir (sadece Docker çalışsın). Bu, A7'nin kapsamını ve süresini doğrudan etkiler. | Ustam — roadmap kararı |
| 6 | **5116 ilk yönetici provisioning stratejisi** | Development'ta user-secrets/env secret, production'da secret store kullanılacak. Hangi secret store (Azure Key Vault / AWS Secrets Manager / HashiCorp Vault) tercih edildiği netleşmeli. | Ustam — altyapı kararı |

# İlk Uygulanacak Paket

**Kapsam:** A0 + A1 + A2 birlikte paketlenmiştir. 5116 backend olmadan 5114 bağımsızlaştırılamayacağı için üç aşama tek bir uygulama bloku olarak ele alınır.

| Başlık | Detay |
|--------|-------|
| **Paket Adı** | `P01-5116-Backend-ve-5114-Bagimsizlasma` |
| **Aşamalar** | A0 (5116 Konfigurator.Api kurulum) + A1 (5114 Gold Banyo bağımlılığını kaldır) + A2 (bağımsız 5114 public site + admin host temeli) |
| **Hedef** | 5116 (`VizitLink3D.Konfigurator.Api`) bağımsız backend projesini kurmak (kendi DB/migration/kullanıcı/auth). 5114'ü (`VizitLink3D.Konfigurator`) Gold Banyo'dan TAMAMEN ayırmak. PhysicalFileProvider ile 5113 static asset bağımlılığını kaldırmak. 5114'ün kendi layout/CSS/JS/media klasörünü oluşturmak. 5116 API'sine (5115 değil) bağlanan bağımsız Public Viewer (`/`) ve Admin Studio (`/admin`) temelini atmak. Cookie auth (BFF) ve ilk yönetici provisioning mekanizmasını kurmak. |
| **Başlama Koşulu** | Mevcut kod durumu (bu dokümanda tespit edilen) yeterlidir. Ek bilgi gerektirmez. |
| **Kullanıcıdan Girdi** | Başlangıç için: ilk yönetici e-posta adresi (user-secrets/env ile provision edilecek). VizitLink3D marka/tema kararları varsayılan değerlerle başlar, sonra değiştirilir. |
| **Çıktı** | 5116 API projesi (çalışır durumda, kendi DB'si, cookie auth, ilk yönetici provisioning); 5114 kendi wwwroot/ klasörü (css/, js/, medya/); 5114/public (bağımsız layout); 5114/admin (bağımsız MudBlazor arayüzü); Cookie auth giriş ekranı; boş dashboard; Gold Banyo ve 5115 referansı olmayan Program.cs; 5116'ya yönlendirilmiş API çağrıları |
| **Süre Tahmini** | AI ajan çalışması ile ~4-6 saat (paralel alt-ajan kullanımı ile) |
| **Kabul Kriteri** | A2 kabul kriteri: 5114/ ve 5114/admin aynı portta ayrı layout'larla açılır, 5113 referansı yoktur, tüm API istekleri **5116**'ya gider (5115'e değil), Gold Banyo logosu/markası görünmez. 5116 health check 200 döndürür. Cookie auth ile giriş yapılabilir. |

Bu paket onaylanır onaylanmaz, kod yazma aşamasına geçilir. Ustam onayı bekleniyor.

---

# Bağımsızlık Kabul Kontrol Listesi

Aşağıdaki kontrol listesi, `VizitLink3D.Konfigurator`/5114 + `VizitLink3D.Konfigurator.Api`/5116'nın Gold Banyo'dan tamamen bağımsız olduğunu doğrulamak için her aşamada (özellikle A0 sonrası) uygulanır. Tüm maddeler ✅ olmadan bağımsızlık sağlanmış sayılmaz.

## 1. Kaynak Ağaç Bağımsızlığı

| # | Kontrol | Açıklama | ✅ / ❌ |
|---|---------|----------|--------|
| K1 | `Program.cs`'te **PhysicalFileProvider** ile 5113/wwwroot referansı yok | `AddStaticFiles` veya `UseStaticFiles` Gold Banyo dizinine işaret etmemeli | |
| K2 | 5114 kendi `wwwroot/` dizinine sahip | Altında `css/sistem/`, `css/temalar/`, `js/`, `medya/` klasörleri mevcut | |
| K3 | 5114 `appsettings.json`'ı bağımsız | Port 5114, uygulama adı "3D-SaaS", kendi bağlantı dizesi | |
| K4 | `VizitLink3D.Konfigurator.csproj`, `VizitLink3D.UI` projesine proje referansı vermiyor | Sadece `VizitLink3D.Ortak` ve NuGet paketlerine bağımlı | |

## 2. Network Bağımsızlığı

| # | Kontrol | Açıklama | ✅ / ❌ |
|---|---------|----------|--------|
| K5 | 5114 açıldığında 5113 portuna giden HTTP isteği yok | F12 Network sekmesinde `localhost:5113` istekleri görülmemeli | |
| K6 | 5114 iframe ile 5113 sayfası göstermiyor | DOM'da `<iframe src="*5113*">` veya `<iframe src="*goldbanyo*">` bulunmamalı | |
| K7 | 5114, **5116 API'sine** doğrudan bağlanıyor (5115'e değil) | Tüm API istekleri `localhost:5116` veya `api.vizitlink3d.com`'a gider. `localhost:5115`'e istek yok | |
| K8 | Embed/redirect/proxy ile 5113 sayfasını sarmıyor | 301/302 redirect veya YARP proxy ile 5113 içeriği gösterilmez | |

## 3. Asset Bağımsızlığı

| # | Kontrol | Açıklama | ✅ / ❌ |
|---|---------|----------|--------|
| K9 | 5114'ün kendi `wwwroot/css/` dosyaları var | Gold Banyo CSS import'u veya link referansı içermez | |
| K10 | 5114'ün kendi `wwwroot/js/` dosyaları var | Gold Banyo JS referansı içermez | |
| K11 | 5114'ün kendi `wwwroot/medya/` klasörü var | Gold Banyo medya dosyalarına bağımlı değil | |
| K12 | 5114'ün kendi `tokens.css` dosyası mevcut | Renk/font/boşluk token'ları Gold Banyo'dan bağımsız | |

## 4. Marka / Kimlik Bağımsızlığı

| # | Kontrol | Açıklama | ✅ / ❌ |
|---|---------|----------|--------|
| K13 | 5114 sayfalarında Gold Banyo logosu kullanılmıyor | `<img src="*goldbanyo*">` veya Gold Banyo marka adı geçmez | |
| K14 | 5114 layout'u Gold Banyo layout'undan bağımsız | Kendi `MainLayout.razor` + tema yapısı mevcut | |
| K15 | 5114 sayfa başlığı / favicon VizitLink3D markalı | `<title>` ve favicon Gold Banyo değil, VizitLink3D | |
| K16 | 5114 tema renkleri Gold Banyo'dan farklı | tokens.css'teki `--ana-renk`, `--logo-renk` vb. Gold Banyo değil | |

## 5. Runtime Bağımsızlığı

| # | Kontrol | Açıklama | ✅ / ❌ |
|---|---------|----------|--------|
| K17 | 5114 + 5116 5113 ve 5115 olmadan bağımsız çalışabiliyor | Sadece 5114 + 5116 + 5116 DB ile sistem ayakta kalır. 5115 API'sine veya 5113'e ihtiyaç duymaz | |
| K18 | 5114 Razor bileşenleri (Public Viewer, Admin Studio) kendine ait | 5113'teki `.razor` dosyalarını kullanmaz, kopyasını veya import'unu yapmaz | |
| K19 | 5114 deploy edildiğinde Gold Banyo deploy'undan bağımsız | Ayrı domain, ayrı container/image, ayrı CI/CD pipeline | |
| K20 | 5114 hata sayfaları VizitLink3D markalı | 404/500 sayfaları Gold Banyo'ya yönlendirmez | |

## 6. 5116 API Bağımsızlığı

| # | Kontrol | Açıklama | ✅ / ❌ |
|---|---------|----------|--------|
| K21 | `VizitLink3D.Konfigurator.Api` projesi mevcut, derleniyor, çalışıyor | `dotnet build` başarılı, `http://localhost:5116/api/health` 200 döner | |
| K22 | 5116 kendi DB'sine sahip (`vizitlink3d-konfigurator.db`) | Migration'lar kendi DB'sinde çalışır, 5115 DB'sine dokunmaz | |
| K23 | 5116 kendi kullanıcı sistemine sahip (Gold Banyo Kullanici/Firma entity'lerini kullanmaz) | 5116 DB'sinde kullanıcı tablosu kendine aittir. 5115'teki `Kullanici` veya `Firma` tablosuna runtime erişimi yoktur | |
| K24 | 5116 oturum yönetimi HttpOnly Secure SameSite cookie / BFF | JWT token veya localStorage KULLANILMAZ. Cookie auth yapılandırması hazır ve çalışır. İlk yönetici provisioning kod/seed/log içinde DEĞİLDİR | |
| K25 | 5116'nın 5115 API'sine veya 5113'e proje referansı/NuGet bağımlılığı yok | Sadece `VizitLink3D.Ortak` (compile-time) ve framework NuGet paketlerine bağımlı | |
| K26 | 5116 embed olmadan, 5114 sadece 5116 ile ayakta kalabiliyor | 5114 → 5116 → 5116 DB zinciri çalışır. 5113/5115 kapalıyken 3D SaaS kullanılabilir | |

> **Kullanım:** Her kontrol maddesi için A0 sonrası ve her büyük aşama (A1, A2, ...) sonrasında test edilir. Tek bir ❌ tespitinde ilgili aşama durdurulur, bağımlılık giderilmeden bir sonraki aşamaya geçilmez.

---

*Bu dosya, VizitLink3D Konfigurator (Bağımsız 3D SaaS Platformu) bağımsızlaştırma planını belgelemektedir. **3D proje Gold Banyo'dan tamamen bağımsızdır.** 5115 API'si kullanılmaz; yeni bağımsız backend 5116 (`VizitLink3D.Konfigurator.Api`) kendi DB/migration/kullanıcıları ve cookie auth (BFF) ile kurulur. 5114, SADECE 5116'ya çağrı yapar. 5113/5115'e hiçbir runtime/request/asset/auth/DB bağımlılığı yoktur.*
