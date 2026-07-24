# 07 — Kurulum ve Çalıştırma Rehberi

> **Hedef kitle:** Projeyi ilk kez açan bir geliştirici.  
> **Kapsam:** Gereksinimlerden ilk çalıştırmaya, BFF güvenlik mimarisinden sık karşılaşılan sorunlara kadar adım adım rehber.

---

## 1. Gereksinimler

| Araç | Minimum Sürüm | Doğrulama Komutu |
|---|---|---|
| .NET SDK | **10.0** | `dotnet --version` |
| PowerShell | **7.0+** | `pwsh --version` |
| Tarayıcı | Chrome 90+ / Firefox 90+ / Edge 90+ | — |
| İşletim Sistemi | Windows 10+, macOS 12+, Linux (glibc ≥2.31) | — |

> ⚠ **Önemli:** .NET 8.x veya 9.x bu projeyi **derleyemez**. `dotnet --version` çıktısı `10.0.xxx` olmalıdır.

**Ek araçlar (opsiyonel):**

| Araç | Amaç |
|---|---|
| `dotnet ef` CLI | Migration yönetimi (`dotnet tool install -g dotnet-ef`) |
| `curl` / Postman | API endpoint testi |

---

## 2. Proje Yapısı

VizitLink3D Konfigurator, **bağımsız 3D SaaS** mimarisine sahip iki ayrı .NET uygulamasından oluşur:

| Proje | Port | Framework | Açıklama |
|---|---|---|---|
| `VizitLink3D.Konfigurator` | **5114** | Blazor Interactive Server + MudBlazor 8.x | Kullanıcı arayüzü (BFF), cookie auth |
| `VizitLink3D.Konfigurator.Api` | **5116** | ASP.NET Core Web API + EF Core 10 + SQLite | İş mantığı, veritabanı, dosya servisleri |

**Klasör yapısı (özet):**

```
VizitLink3D.Konfigurator/           ← UI (BFF)
├── Components/                     ← Razor bileşenleri
├── Layout/
│   ├── AdminDuzen.razor           ← Admin panel layout (MudBlazer provider'lar)
│   └── BosDuzen.razor             ← Public sayfa layout
├── Pages/
│   ├── Admin/                      ← Admin sayfaları
│   └── Public/                     ← Public sayfalar
├── Servisler/                      ← ApiIstemcisi, KimlikServisi, DilServisi
├── App.razor                       ← Import map, CSP, font
└── Program.cs                      ← BFF oturum endpoint'leri, güvenlik başlıkları

VizitLink3D.Konfigurator.Api/       ← API
├── AraYazilimlar/
│   └── BffGuvenlikFilter.cs       ← X-Konfigurator-Bff-Anahtari doğrulama
├── Moduller/
│   ├── Kimlik/                     ← Giriş, şifre sıfırlama, kullanıcı
│   ├── Modeller/                   ← Model CRUD, GLB yükleme/doğrulama
│   └── Sistem/                     ← Sağlık kontrolü
├── VeriTabani/
│   └── KonfiguratorDbContext.cs    ← EF Core DbContext (SQLite)
├── appsettings.json                ← Ana yapılandırma
└── Program.cs                      ← Migration, seed, rate limiting, middleware
```

---

## 3. İlk Çalıştırma (Adım Adım)

### 3.1 Konfigürasyon Dosyaları

#### `appsettings.json` (ana yapılandırma — API tarafı)

```json
{
  "ConnectionStrings": {
    "KonfiguratorVeriTabani": "Data Source=Konfigurator.db"
  },
  "IlkYonetici": {
    "KullaniciAdi": "admin",
    "Sifre": "GoldBanyo2024!",
    "Eposta": "admin@vizitlink3d.com.tr"
  },
  "BffGuvenlik": {
    "Anahtar": "VizitLink3D_BFF_Guvenlik_Anahtari_2024_Gelistirme"
  },
  "Eposta": {
    "Sunucu": "",
    "Port": "587",
    "KullaniciAdi": "",
    "AppSifresi": "",
    "GonderenAdres": ""
  },
  "SifreSifirlama": {
    "UygulamaUrl": ""
  },
  "GlbYukleme": {
    "MaxDosyaBoyutuMb": 30
  }
}
```

| Ayar Bölümü | Açıklama |
|---|---|
| `ConnectionStrings` | SQLite veritabanı dosya yolu (`Konfigurator.db`) |
| `IlkYonetici` | İlk çalıştırmada otomatik oluşturulan yönetici hesabı |
| `BffGuvenlik` | BFF-API arası paylaşılan gizli anahtar (bkz. §3.2) |
| `Eposta` | Şifre sıfırlama e-postaları için SMTP ayarları |
| `SifreSifirlama` | Şifre sıfırlama bağlantısı için uygulama URL'i |
| `GlbYukleme` | GLB model dosyası için maksimum boyut (MB) |

#### `appsettings.json` (UI/BFF tarafı)

```json
{
  "ApiAyarlari": {
    "BaseUrl": "https://api.3dvizitlink.com.tr"
  },
  "UygulamaAyarlari": {
    "Port": 5114,
    "Proje": "VizitLink3D.Konfigurator (Bagimsiz)",
    "Versiyon": "P03-B"
  },
  "BffGuvenlik": {
    "Anahtar": "VizitLink3D_BFF_Guvenlik_Anahtari_2024_Gelistirme"
  }
}
```

#### `appsettings.Development.json` (her iki projede)

**UI (5114) — Development override:**
```json
{
  "ApiAyarlari": {
    "BaseUrl": "http://localhost:5116/"
  },
  "UygulamaAyarlari": {
    "Port": 5114
  },
  "BffGuvenlik": {
    "Anahtar": "VizitLink3D_BFF_Guvenlik_Anahtari_2024_Gelistirme"
  }
}
```

**API (5116) — Development override:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "BffGuvenlik": {
    "Anahtar": "VizitLink3D_BFF_Guvenlik_Anahtari_2024_Gelistirme"
  }
}
```

> ℹ️ Development override'lar şunları ezmez:
> - `ConnectionStrings` → `appsettings.json`'daki SQLite bağlantısı aynen kalır
> - `IlkYonetici` → seed işlemi aynı kullanıcı adı/şifre ile çalışır
> - `GlbYukleme` → maksimum dosya boyutu aynı kalır

---

### 3.2 User Secrets (KRİTİK)

> ⚠ **BU ADIM ATLANIRSA UYGULAMA ÇALIŞMAZ.**  
> UI tarafında "Yapılandırma hatası" alırsınız.

**Neden gerekli?**

`appsettings.json` içindeki `BffGuvenlik:Anahtar` değeri **geliştirme ortamında production değeriyle çakışabilir** veya geliştirici makineler arasında farklılık gösterebilir. .NET'in **user-secrets** mekanizması, her geliştiricinin kendi makinesinde bu değeri ayrı ayrı tanımlamasına olanak tanır. Bu değer `appsettings.json`'a gömülmez, kaynak kontrole (Git) gönderilmez.

**Her iki proje için AYNI değeri tanımlayın.** İki projedeki `BffGuvenlik:Anahtar` değeri birebir aynı olmalıdır; aksi halde BFF → API istekleri `401` ile reddedilir.

```powershell
# UI projesi (BFF)
dotnet user-secrets set "BffGuvenlik:Anahtar" "VizitLink3D_BFF_Guvenlik_Anahtari_2024_Gelistirme" --project VizitLink3D.Konfigurator

# API projesi
dotnet user-secrets set "BffGuvenlik:Anahtar" "VizitLink3D_BFF_Guvenlik_Anahtari_2024_Gelistirme" --project VizitLink3D.Konfigurator.Api
```

**Doğrulama:**

```powershell
dotnet user-secrets list --project VizitLink3D.Konfigurator
dotnet user-secrets list --project VizitLink3D.Konfigurator.Api
```

Her ikisi de `BffGuvenlik:Anahtar = VizitLink3D_BFF_Guvenlik_Anahtari_2024_Gelistirme` göstermelidir.

> 💡 **İyi uygulama:** Geliştiriciler kendi makinelerinde farklı bir anahtar kullanmak isterse, user-secrets ile kendi değerlerini tanımlar. Production'da bu değer ortam değişkeni (environment variable) ile override edilir.

---

### 3.3 İlk Yönetici Hesabı

`appsettings.json` içindeki `IlkYonetici` bölümü:

```json
"IlkYonetici": {
  "KullaniciAdi": "admin",
  "Sifre": "GoldBanyo2024!",
  "Eposta": "admin@vizitlink3d.com.tr"
}
```

**Seed (bootstrap) mantığı — `Program.cs` içinde:**

1. Uygulama başlarken `db.Database.MigrateAsync()` çağrılır (EF Core migration'ları otomatik uygulanır).
2. Migration sonrası `IlkYonetici:KullaniciAdi` değeri okunur.
3. Veritabanında bu kullanıcı adıyla bir kayıt **yoksa** yeni yönetici oluşturulur:
   - `Rol = "Yonetici"`
   - `AktifMi = true`
   - Şifre **BCrypt** ile hash'lenir (work factor: 12)
   - `OlusturulmaTarihi = DateTime.UtcNow`
4. Veritabanında **zaten varsa** → atlanır (tekrar eklenmez).
5. `IlkYonetici:KullaniciAdi` veya `IlkYonetici:Sifre` **boşsa** → seed atlanır, log'a bilgi yazılır.

> ⚠ **Güvenlik notu:** Production ortamında `appsettings.json` içinde sabit şifre barındırmayın. `IlkYonetici` değerlerini ortam değişkenleri (environment variables) ile override edin:
>
> ```powershell
> $env:IlkYonetici__KullaniciAdi = "admin"
> $env:IlkYonetici__Sifre = "GucluVeUzunSifre!"
> ```

---

### 3.4 Çalıştırma

İki ayrı terminal penceresi açın. **ÖNCE API'yi başlatın** — UI, API'ye bağlanır; API hazır değilse UI hata verir.

#### Terminal 1: API (Port 5116)

```powershell
cd VizitLink3D.Konfigurator.Api
dotnet run --urls http://localhost:5116
```

Başarılı başlatma çıktısı:

```
[KONFIGURATOR.API] Veritabani: Konfigurator.db
[KONFIGURATOR.API] http://localhost:5116 adresinde dinleniyor...
```

#### Terminal 2: UI / BFF (Port 5114)

```powershell
cd VizitLink3D.Konfigurator
dotnet run --urls http://localhost:5114
```

Başarılı başlatma çıktısı:

```
[KONFIGURATOR] VizitLink3D Studio Bagimsiz Runtime — http://localhost:5114
[KONFIGURATOR] Public 3D Viewer   — http://localhost:5114/
[KONFIGURATOR] Public API modeller — http://localhost:5114/api/public/modeller
[KONFIGURATOR] Admin Giris        — http://localhost:5114/admin
[KONFIGURATOR] Admin Dashboard    — http://localhost:5114/admin/dashboard
[KONFIGURATOR] Admin Modeller     — http://localhost:5114/admin/modeller
[KONFIGURATOR] Saglik             — http://localhost:5114/saglik
```

---

### 3.5 Doğrulama

#### Sağlık kontrolü

```powershell
# API sağlık (5116)
curl http://localhost:5116/saglik
```

Beklenen yanıt:

```json
{
  "basariliMi": true,
  "veri": "Calisiyor",
  "mesaj": "API saglikli."
}
```

```powershell
# UI sağlık (5114)
curl http://localhost:5114/saglik
```

Beklenen yanıt:

```json
{
  "durum": "calisiyor",
  "port": 5114,
  "proje": "VizitLink3D.Konfigurator (Bagimsiz)",
  "versiyon": "P03-B"
}
```

#### Admin giriş

```powershell
curl -X POST http://localhost:5116/api/kimlik/giris `
  -H "Content-Type: application/json" `
  -d '{"KullaniciAdi":"admin","Sifre":"GoldBanyo2024!"}'
```

Beklenen yanıt:

```json
{
  "basariliMi": true,
  "mesaj": "Giris basarili.",
  "veri": {
    "kullaniciId": 1,
    "kullaniciAdi": "admin",
    "rol": "Yonetici"
  }
}
```

#### Tarayıcı ile giriş

1. Tarayıcıda `http://localhost:5114/admin` adresine gidin.
2. Kullanıcı adı: `admin`
3. Şifre: `GoldBanyo2024!`
4. Başarılı giriş → `http://localhost:5114/admin/dashboard` sayfasına yönlendirilirsiniz.

---

## 4. BFF Güvenlik Mimarisi (Özet)

```
┌─────────────┐        X-Konfigurator-Bff-Anahtari        ┌─────────────┐
│   Tarayıcı   │ ──── HTTPS ──── │    UI (5114)    │ ───── Header ───── │  API (5116)  │
│             │   Cookie Auth    │                 │   Server-to-Server  │              │
└─────────────┘                  └─────────────────┘                     └──────────────┘
```

| Katman | Açıklama |
|---|---|
| **Tarayıcı ↔ UI** | Cookie tabanlı kimlik doğrulama (`HttpOnly`, `SameSite=Lax`, `Secure`) |
| **UI ↔ API** | `X-Konfigurator-Bff-Anahtari` header'ı ile server-to-server doğrulama |
| **API** | `BffGuvenlikFilter` attribute'ü ile korunan endpoint'ler |

### 4.1 `BffGuvenlikFilter` Çalışma Mantığı

```csharp
// YonetimModellerKontrolcu.cs
[ApiController]
[Route("api/yonetim/modeller")]
[ServiceFilter(typeof(BffGuvenlikFilter))]   // ← Bu attribute BFF güvenliğini etkinleştirir
public class YonetimModellerKontrolcu : ControllerBase { ... }
```

Filtre sırasıyla şu kontrolleri yapar:

1. **Yapılandırma kontrolü:** `BffGuvenlik:Anahtar` boş → `503 Service Unavailable`
2. **Header varlığı:** `X-Konfigurator-Bff-Anahtari` header'ı eksik → `401 Unauthorized`
3. **Değer eşleşmesi:** Header değeri ≠ beklenen anahtar → `401 Unauthorized`

> ⚠ **Önemli:** Açık endpoint'ler (örn. `api/kimlik/giris`, `api/kimlik/sifre-sifirlama-istegi`) `[ServiceFilter(typeof(BffGuvenlikFilter))]` ile işaretlenmez — bunlar **public** endpoint'lerdir. Yönetim endpoint'leri (`api/yonetim/*`) BFF güvenlik filtresi ile korunur.

### 4.2 CSRF Koruması

UI (5114) tarafında tüm `POST` işlemleri için ASP.NET Core **AntiforgeryToken** doğrulaması yapılır. Bu sayede:

- Tarayıcıda `POST /oturum/giris`, `/oturum/cikis`, `/oturum/sifre-sifirlama-istegi` gibi endpoint'lere gönderilen formlar CSRF saldırılarına karşı korunur.
- Antiforgery doğrulaması başarısız olursa `400 Bad Request` döner.

### 4.3 Rate Limiting

API tarafında endpoint bazlı hız sınırlandırması uygulanır:

| Politikası | Endpoint | Limit | Pencere |
|---|---|---|---|
| `giris` | `POST /api/kimlik/giris` | 5 istek | 1 dakika |
| `sifre-sifirlama-istegi` | `POST /api/kimlik/sifre-sifirlama-istegi` | 3 istek | 15 dakika |
| `sifre-yenile` | `POST /api/kimlik/sifre-yenile` | 5 istek | 15 dakika |
| `modelyukleme` | `POST /api/yonetim/modeller/yukle` | 10 istek | 1 dakika |
| `yonetim` | `api/yonetim/modeller/*` | 30 istek | 1 dakika |
| `yonetim-parcalar` | `api/yonetim/parcalar/*` | 30 istek | 1 dakika |

Aşım durumunda `429 Too Many Requests` döner.

### 4.4 JWT Kullanılmaz

Bu projede **JWT token yoktur.** Kimlik doğrulama:

- **Tarayıcı tarafında:** Cookie (`HttpOnly`, `SameSite=Lax`)
- **Server-to-server:** Paylaşılan gizli anahtar (`X-Konfigurator-Bff-Anahtari` header)

Bunun avantajları:
- XSS ile token çalınamaz (cookie `HttpOnly`)
- Token yenileme (refresh) döngüsü yok — cookie sliding expiration ile yönetilir
- API'nin JWT doğrulama altyapısına ihtiyacı yok

---

## 5. 3D Viewer ve CSP

### 5.1 Three.js Entegrasyonu

Proje **Three.js 0.170.0** sürümünü CDN üzerinden yükler (jsdelivr). Sürüm bilinçli olarak **pinned** (sabitlenmiş) tutulur; `@latest` kullanılmaz — böylece CDN güncellemesi uygulamayı kıramaz.

```html
<!-- App.razor içinde -->
<script type="importmap">
{
  "imports": {
    "three": "https://cdn.jsdelivr.net/npm/three@0.170.0/build/three.module.js",
    "three/addons/": "https://cdn.jsdelivr.net/npm/three@0.170.0/examples/jsm/"
  }
}
</script>
```

**Import map** sayesinde Razor bileşenlerinde bare specifier (çıplak tanımlayıcı) kullanılabilir:

```javascript
// JS modül içinde:
import * as THREE from 'three';                  // ← bare specifier, import map ile çözülür
import { OrbitControls } from 'three/addons/';   // ← addons yolu
```

### 5.2 Content Security Policy (CSP)

UI (5114), Blazor Interactive Server + MudBlazor + Three.js için optimize edilmiş dar bir CSP politikası uygular. CSP, `Program.cs` içinde middleware olarak tanımlanır:

| CSP Direktifi | İzin Verilen Kaynaklar |
|---|---|
| `default-src` | `'self'` |
| `script-src` | `'self'`, `https://cdn.jsdelivr.net`, import map SHA-256 hash |
| `style-src` | `'self'`, `'unsafe-inline'`, `https://fonts.googleapis.com` |
| `font-src` | `'self'`, `https://fonts.gstatic.com` |
| `img-src` | `'self'`, `data:`, `blob:` |
| `connect-src` | `'self'`, `ws:` (Blazor SignalR) |
| `worker-src` | `'self'`, `blob:` |
| `frame-ancestors` | `'none'` |

> ℹ️ Google Fonts, `Guvenlik:GoogleFontsEtkin` ayarı `false` yapılarak devre dışı bırakılabilir.

---

## 6. Medya / GLB Yükleme

### 6.1 Dosya Depolama

- Tüm 3D modeller `wwwroot/medya/3d-modeller/` altında saklanır.
- Dosya adları **UUID** formatındadır (örn. `a1b2c3d4e5f6...glb`). Orijinal dosya adı veritabanında ayrı bir alanda tutulur.
- API tarafında `/medya` path'i altında `UseStaticFiles` ile sunulur.
- **Sadece `.glb` uzantılı dosyalara** izin verilir; diğer uzantılar `404` döner.

### 6.2 Desteklenen Format

| Format | Uzantı | Durum |
|---|---|---|
| glTF Binary | `.glb` | ✅ Desteklenir |
| glTF JSON + ayrı dosyalar | `.gltf` + `.bin` | ❌ Desteklenmez |
| OBJ, FBX, STL, USDZ | — | ❌ Desteklenmez |

### 6.3 GLB Magic Byte Doğrulaması

Yüklenen her dosya, `GlbDosyaServisi.SihirliBaytDogrula()` ile doğrulanır:

1. **Dosya uzunluğu** ≥ 12 bayt olmalı
2. **Magic** (bayt 0-3): `67 6C 54 46` = ASCII `glTF`
3. **Version** (bayt 4-7): uint32 little-endian = `2`
4. **Declared total length** (bayt 8-11): dosyanın gerçek boyutuyla eşleşmeli

Tüm kontroller geçilirse dosya SHA-256 hash'i hesaplanarak diske yazılır.

### 6.4 Dosya Boyutu Sınırı

- Maksimum: **30 MB** (`GlbYukleme:MaxDosyaBoyutuMb` ile yapılandırılır)
- Sınır aşılırsa: `413 Request Entity Too Large` veya doğrulama hatası

### 6.5 İçerik Türü (Content-Type)

GLB dosyaları `model/gltf-binary` MIME türü ile sunulur. Yükleme sırasında istemciden `Content-Type` doğru gelmese bile, API tarafında `model/gltf-binary` fallback olarak atanır.

---

## 7. Sık Karşılaşılan Sorunlar

| Sorun | Belirti | Neden | Çözüm |
|---|---|---|---|
| **Yapılandırma hatası** | UI başlangıcında hata sayfası | `BffGuvenlik:Anahtar` boş | `dotnet user-secrets set` ile anahtarı tanımlayın (§3.2) |
| **401 Yetkisiz erişim** | Admin panel API çağrılarında 401 | BFF anahtarı iki projede farklı | Her iki projede **aynı** `BffGuvenlik:Anahtar` değerini kullanın |
| **503 Service Unavailable** | API'den 503 yanıtı | API tarafında `BffGuvenlik:Anahtar` tanımlı değil | API projesinde user-secrets veya appsettings'te anahtarı kontrol edin |
| **3D preview yüklenmiyor** | Siyah ekran / boş canvas | CSP `script-src` Three.js CDN'e izin vermiyor | CSP'de `https://cdn.jsdelivr.net` ve import map hash'inin doğru olduğunu kontrol edin |
| **Model yüklenemedi** | Upload başarısız / "Geçersiz GLB" hatası | Dosya GLB formatında değil veya bozuk | Dosyanın glTF Binary (`.glb`) olduğundan emin olun. [Khronos validator](https://github.khronos.org/glTF-Validator/) ile doğrulayın |
| **"Missing MudPopoverProvider"** | Dialog/popover açılmıyor, console'da hata | Layout'ta MudBlazor provider bileşenleri eksik | `AdminDuzen.razor` veya `BosDuzen.razor` içinde aşağıdakilerin var olduğunu kontrol edin: `<MudPopoverProvider />`, `<MudDialogProvider />`, `<MudSnackbarProvider />` |
| **SQLite hatası** | `SQLite Error 14: 'unable to open database file'` | Klasör yazma izni yok veya dosya kilitli | Proje kök klasörüne yazma izniniz olduğunu kontrol edin. Önceki `dotnet run` sürecini durdurun |
| **Port çakışması** | `Failed to bind to address http://localhost:5114` | Port zaten kullanımda | Başka bir sürecin portu kullanmadığından emin olun: `netstat -ano \| findstr :5114` |
| **Migration hatası** | `There is already an object named '...' in the database` | Migration'lar tutarsız | `Konfigurator.db` dosyasını silin (geliştirme ortamında!), uygulamayı yeniden başlatın — migration'lar sıfırdan uygulanır |
| **UI API'ye bağlanamıyor** | Admin giriş başarısız / "Sunucu hatası" | API çalışmıyor veya yanlış port | API'nin 5116 portunda çalıştığını doğrulayın. `appsettings.Development.json`'da `ApiAyarlari:BaseUrl` = `http://localhost:5116/` olduğundan emin olun |
| **dotnet ef komutu bulunamadı** | `'dotnet-ef' is not recognized` | EF Core CLI tool yüklü değil | `dotnet tool install -g dotnet-ef` komutunu çalıştırın |

---

## 8. Geliştirme İpuçları

### 8.1 Migration Ekleme

Veritabanı şemasında değişiklik yaptığınızda:

```powershell
# Yeni migration oluştur
dotnet ef migrations add MigrasyonAdi --project VizitLink3D.Konfigurator.Api
```

Migration'lar uygulama başlangıcında otomatik uygulanır (`db.Database.MigrateAsync()`). Manuel uygulamak gerekirse:

```powershell
dotnet ef database update --project VizitLink3D.Konfigurator.Api
```

> ⚠ **KESİNLİKLE YAPILMAMASI GEREKENLER:**
> - Veritabanı dosyasını (`Konfigurator.db`) manuel silmeyin (geliştirme hariç)
> - Migration'ları geri almayın (`database update PreviousMigration` çalıştırmayın)
> - Mevcut entity/tablo silmeyin — sadece **ekleme** yapın

### 8.2 DB'yi Sıfırlamadan Şema Değişikliği

`Konfigurator.db` dosyasını koruyarak şema değişikliği yapmak için:

1. Yeni entity/model ekleyin
2. `KonfiguratorDbContext`'e `DbSet<T>` property ekleyin
3. `dotnet ef migrations add YeniMigrationAdi --project VizitLink3D.Konfigurator.Api` çalıştırın
4. Uygulamayı yeniden başlatın — migration otomatik uygulanır

### 8.3 Yeni Modül Ekleme (Vertical Slice)

Proje **Vertical Slice** mimarisini kullanır. Yeni bir modül eklemek için:

```
Moduller/
└── YeniModulAdi/          ← Klasör adı ASCII, PascalCase
    ├── Kontrolcu/          ← API Controller
    ├── Dtolar/             ← Request/Response DTO'lar
    ├── Modeller/           ← Entity'ler
    ├── Servisler/          ← İş mantığı servisleri
    └── Dogrulayicilar/     ← FluentValidation doğrulayıcılar
```

### 8.4 Yeni Sayfa Ekleme

- **Admin sayfası:** `Pages/Admin/YeniSayfa.razor` + `Pages/Admin/YeniSayfa.razor.cs` (partial class)
- **Public sayfa:** `Pages/Public/YeniSayfa.razor` + `Pages/Public/YeniSayfa.razor.cs`

> ⚠ **Kurallar:**
> - `.razor` içinde `<style>` etiketi kullanmayın
> - `.razor` içinde `@code { }` bloğu kullanmayın — her sayfa **partial class** (`*.razor.cs`)
> - Hardcoded metin yerine `DilServisi.T("anahtar", "Varsayılan metin")` kullanın
> - Renk/font için `tokens.css` değişkenlerini kullanın (`var(--ana-renk)`)

### 8.5 Debugging

```powershell
# API debugging
cd VizitLink3D.Konfigurator.Api
dotnet run --urls http://localhost:5116

# UI debugging
cd VizitLink3D.Konfigurator
dotnet run --urls http://localhost:5114
```

- Browser DevTools (F12) → Console sekmesi: Blazor/JS hataları
- Browser DevTools → Network sekmesi: API çağrıları, BFF header'ı
- `appsettings.Development.json` ile `Microsoft.EntityFrameworkCore` log seviyesini `Debug` yaparak SQL sorgularını görebilirsiniz

### 8.6 Production Derlemesi

```powershell
# API
dotnet publish VizitLink3D.Konfigurator.Api -c Release -o ./yayin/api

# UI
dotnet publish VizitLink3D.Konfigurator -c Release -o ./yayin/ui
```

> ℹ️ Production'da `BffGuvenlik:Anahtar` değeri ortam değişkeni ile verilmelidir:
> ```powershell
> $env:BffGuvenlik__Anahtar = "production-guvenli-uzun-anahtar"
> ```

---

## 9. Hızlı Başlangıç Özeti (Checklist)

İlk kez çalıştıran bir geliştirici için tüm adımların özeti:

```
[ ] .NET 10 SDK kurulu → dotnet --version
[ ] PowerShell 7+ kurulu → pwsh --version
[ ] dotnet-ef tool kurulu → dotnet tool install -g dotnet-ef (opsiyonel)
[ ] User secrets tanımlandı (her iki proje, AYNI değer) → §3.2
[ ] Terminal 1: cd VizitLink3D.Konfigurator.Api && dotnet run --urls http://localhost:5116
[ ] Terminal 2: cd VizitLink3D.Konfigurator && dotnet run --urls http://localhost:5114
[ ] curl http://localhost:5116/saglik → {"basariliMi":true,...}
[ ] curl http://localhost:5114/saglik → {"durum":"calisiyor",...}
[ ] Tarayıcı: http://localhost:5114/admin → admin / GoldBanyo2024! → Dashboard
```

---

*Versiyon: 1.0 — Temmuz 2026*  
*İlgili dokümanlar: `00_MIMARI_KARARLAR.md`, `01_YOL_HARITASI.md`, `05_LISANS_TENANT_GUVENLIK.md`*
