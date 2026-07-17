# 🛠 İŞÇİ MODEL GÖREV LİSTESİ

> **Hedef:** Bu klasördeki iskelet `.md` dosyalarını **endüstriyel seviyede** dolduracak işçi AI modeli için brief'ler.
> **Kural:** Ustam plan + iskelet hazırladı. İşçi model **kod yazmadan önce** AGENTS.md ve 00_PROJE_BILGISI okur, sonra atanan dosyayı doldurur.

---

## 📋 DURUM ÖZETİ

| Dosya | Durum | İşçi'ye Atanacak |
|---|---|---|
| `AGENTS.md` | ✅ TAMAM | — |
| `README.md` | ✅ TAMAM | — |
| `.claude/CLAUDE.md` | ✅ TAMAM | — |
| `.cursor/rules.mdc` | ✅ TAMAM | — |
| `.github/copilot-instructions.md` | ✅ TAMAM | — |
| `AjanKurallari/00_PROJE_BILGISI.md` | ✅ TEMPLATE TAMAM | — |
| `AjanKurallari/01_BASLA.md` | ✅ TAMAM | — |
| `AjanKurallari/02_CSharp_Disiplini.md` | ✅ TAMAM | — |
| `AjanKurallari/03_Razor_MudBlazor_Blazor10.md` | ✅ TAMAM | — |
| `AjanKurallari/04_CSS_Tema_Stitch_Entegrasyonu.md` | ✅ TAMAM | — |
| `AjanKurallari/05_Veritabani_EFCore10.md` | 🟡 İSKELET | **İşçi #1** |
| `AjanKurallari/06_API_Servisler_MediatR.md` | 🟡 İSKELET | **İşçi #2** |
| `AjanKurallari/07_Guvenlik_Passkey_JWT.md` | 🟡 İSKELET | **İşçi #3** |
| `AjanKurallari/08_Performans_Cache_Render.md` | 🟡 İSKELET | **İşçi #4** |
| `AjanKurallari/09_Coklu_Platform_Web_Mobil_Masa.md` | 🟡 İSKELET | **İşçi #5** |
| `AjanKurallari/10_Test_Derleme_Pipeline.md` | 🟡 İSKELET | **İşçi #6** |
| `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md` | ✅ TAMAM | — |

---

## 🌐 EVRENSEL İŞÇİ KURALLARI (Hepsine Geçerli)

### Ön Okuma Zorunlu (Görev Başlangıcında)
1. `AGENTS.md` (kök)
2. `AjanKurallari/00_PROJE_BILGISI.md` (template — placeholder'lar görülür)
3. `AjanKurallari/01_BASLA.md` (indeks)
4. `AjanKurallari/02_CSharp_Disiplini.md` (kod örnek standardı)
5. `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md` (yasaklar)

### Yazım Standardı (TÜM İŞÇİLER)
- **Dil:** %100 Türkçe açıklama. Kod örnekleri C# 14 / Razor / CSS.
- **Frontmatter:** Mevcut iskeletteki `name`, `description`, `status` korunur. `status: TAMAM` yapılır.
- **Marka-nötr:**
  - ❌ `KapiModeli`, `KapiKategorisi`, `VIZITLINK3D.com.tr`, `bronz`, `kapı`, `mobilya`
  - ✅ `Urun`, `Kategori`, `Musteri`, `Siparis`, `Fatura`, `[PROJE_ADI]`, `[FIRMA_ADI]`, `[URL_BIRINCIL]`
- **Yer Tutucu Sözdizimi:** `[TEMA.ANA_RENK]`, `[00_PROJE_BILGISI.port_api]` (çift köşeli parantez içinde dot notation)
- **Kod örnekleri:** Minimum 5-10 örnek. Hem yanlış (`❌`) hem doğru (`✅`) gösterilir.
- **Boyut:** 400-800 satır arası (1500 satır limiti var ama o kadar değil).
- **Format:** Markdown başlık hiyerarşisi (`##`, `###`), tablolar, kod blokları (` ```csharp `, ` ```razor `, ` ```css `).
- **Bağlantılar:** Diğer kural dosyalarına relatif link (`[02_CSharp_Disiplini.md](02_CSharp_Disiplini.md)`).
- **Son bölüm:** Her dosya 📋 **Öz-Denetim** checklist'i ile biter (madde sayısı brief'te).
- **Versiyon:** `Versiyon: 1.0 | Tarih: 2026-05-14` footer.

### Kalite Beklentisi
- **Endüstriyel seviye:** Notion/Linear/Stripe dokümantasyonu kalitesinde
- **Aksiyon odaklı:** Her bölüm "ne yap, ne yapma" tarzında
- **Hata önleyici:** Yanlış örnekler + neden yanlış + doğrusu
- **Latest tech:** .NET 10, C# 14, Blazor 10, EF Core 10 yeni özellikleri vurgulanır
- **NuGet wrapper isimleri:** Türkçe sarmalayıcı standardı

---

## 📄 İŞÇİ GÖREVLERİ (Tek Tek)

### 👷 İşçi #1 — `05_Veritabani_EFCore10.md`

**Süre tahmini:** 1-1.5 saat
**Önkoşul okuma:** AGENTS.md, 02_CSharp_Disiplini.md, 04_CSS_Tema_Stitch.md (entity örnekleri için)

**Doldur:**
1. **Yasaklar bölümü** (8 madde, kod örnekleri ❌/✅)
2. **İsimlendirme** (tablo, sütun, index `IX_*`, FK `FK_*`)
3. **Entity Tasarımı** — `Urun` örneği ile audit alanları + soft delete + JsonIgnore + nav property
4. **DbContext** — DbSet, OnModelCreating, query filter, MaxLength, cascade Restrict
5. **Migration Workflow** — bash adımlar (yedek → migration add → script önizle → update → test → yedek)
6. **Migration Adlandırma** — Türkçe ✅/İngilizce ❌ örnekler (5-10 tane)
7. **Index Stratejisi** — slug unique, eposta unique, hash partial (PostgreSQL JSONB), composite
8. **Sorgu Kuralları** — async, N+1, AsNoTracking, AsSplitQuery, projection, ExecuteUpdateAsync, BulkExtensions
9. **Soft Delete** — Global Query Filter + IgnoreQueryFilters
10. **JSON Column** — `TeknikOzelliklerJson` örneği, EF.Functions.JsonContains
11. **Yedek Politikası** — Hangfire RecurringJob, `00_PROJE_BILGISI.yedek.saat` referansı
12. **Performans İpuçları Tablosu** (sorun → çözüm)
13. **Öz-denetim** — 18 madde

**Referans için kullanılabilir (brand-spesifik içerik VAR — generic'e dönüştür!):**
`i:\desedoorweb\Yedekler\ajan_modelleri_yedek_20260514\AjanModelleri\04_Veritabani_EFCore.md`

---

### 👷 İşçi #2 — `06_API_Servisler_MediatR.md`

**Süre:** 1.5-2 saat
**Önkoşul:** AGENTS.md, 02_CSharp, 05_Veritabani

**Doldur:**
1. **Vertical Slice Klasör Yapısı** — `Moduller/Urun/{Komutlar, Sorgular, Dtolar, Dogrulayicilar, Servisler, Kontrolcu}` görseli
2. **Route Standardı** — REST tablo (GET/POST/PUT/DELETE)
3. **Cevap<T> Zarfı** — kod örnek + nasıl kullanılır
4. **MediatR CQRS** — Komut/Sorgu record, IRequestHandler, kontrolcü 3 satır
5. **Pipeline Behaviors** — `ValidationBehavior`, `LoggingBehavior`, `CachingBehavior`, `AuditBehavior` (her birinin kod iskeleti)
6. **FluentValidation Dogrulayici** — `UrunOlusturDogrulayici` örneği, async kural (`MustAsync`)
7. **Mapster** — config + ProjectToType + AutoMapper YASAK gerekçe
8. **HataYonetimiMiddleware** — kod iskeleti (CorrelationId, dev/prod davranış, Cevap<T> sarma)
9. **Kontrolcü Disiplini** — örnek 3-satırlık endpoint
10. **API Versioning** — `Asp.Versioning.Mvc.ApiExplorer` config
11. **Rate Limiting** — endpoint başına `[EnableRateLimiting("ad")]`
12. **OpenAPI** — `AddOpenApi()` (ASP.NET 10 yerleşik), Swashbuckle YASAK gerekçe
13. **SignalR Hub** — `UrunHub` örneği, Türkçe metot, MessagePack
14. **Öz-denetim** — 15 madde

---

### 👷 İşçi #3 — `07_Guvenlik_Passkey_JWT.md`

**Süre:** 2 saat
**Önkoşul:** AGENTS.md, 02_CSharp, 06_API

**Doldur:**
1. **Yasaklar** — 8 madde alarm seviyesi
2. **JWT Bearer** — config'ten süre okuma, refresh token rotation, JwtServisi wrapper
3. **BCrypt** — work factor 00_PROJE_BILGISI'ten, hash + verify + client'ta YASAK
4. **2FA TOTP** — OtpNet, QR code üret, kullanım akışı
5. **Passkey (Blazor 10 YENİ)** — WebAuthn, kayıt akışı, giriş akışı (`Microsoft.AspNetCore.Identity` entegrasyonu)
6. **[JsonIgnore] Zorunlu Liste** — 9 alan tablo + nedenler
7. **CORS** — config'ten `url_birincil` + `url_yedek`, üretim/dev ayrımı
8. **Güvenlik Header'ları** — `GuvenlikHeaderlariMiddleware` (HSTS, X-Frame, CSP, vb.)
9. **Rate Limiting** — politikalar (genel 1000/5dk, /giris 5/dk)
10. **Input Validation** — 06_'a referans
11. **XSS** — IcerikTemizleyici wrapper kullanımı
12. **SignalR Güvenlik** — JWT + EnableDetailedErrors false
13. **API Key Şifreleme** — DataProtection örneği (AISaglayicisi.ApiKeyEncrypted)
14. **Audit Log** — append-only entity (`AuditLog`), HMAC zincir hash, EF interceptor
15. **Gizli Bilgi Yönetimi** — user-secrets, env, .env yasağı
16. **PII Filtresi** — log enricher TC/telefon maskele
17. **Endpoint Güvenlik Seviyeleri** — [AllowAnonymous] kara liste
18. **Öz-denetim** — 14 madde

---

### 👷 İşçi #4 — `08_Performans_Cache_Render.md`

**Süre:** 1.5 saat
**Önkoşul:** AGENTS.md, 02_CSharp, 03_Razor, 05_Veritabani

**Doldur:**
1. **FusionCache** — config Program.cs, L1+L2 Redis, stampede, fail-safe örneği
2. **OnbellekYonetici Wrapper** — kod tam
3. **Cache Stratejisi Tablosu** — anahtar formatı (`urun:liste:{filtreHash}`, `ceviri:{dil}`), TTL
4. **EF Performans** — örneklerle (slow query → fix)
5. **N+1 Önleme** — ProjectToType (Mapster) + Include + AsSplitQuery
6. **Blazor Render** — Virtualize örneği, @key, ShouldRender (dikkatli)
7. **[PersistentState]** — Blazor 10 yeni özellik, kod örnek
8. **Lazy Loading Sayfalar** — `.csproj` config, route bazlı
9. **Asset Preloading** — Blazor 10 otomatik (açıklama)
10. **ImageSharp.Web** — `/img/foo.jpg?w=400&fmt=webp` örneği, Program.cs config
11. **SignalR + MessagePack** — Program.cs config, performans kazancı sayısal
12. **Compression** — Brotli/Gzip nginx + Kestrel
13. **Hedef Metrikler Tablosu** — LCP/FID/CLS/Lighthouse hedefleri
14. **Profil Çıkarma** — dotnet-counters, MiniProfiler kullanımı
15. **Öz-denetim** — 12 madde

---

### 👷 İşçi #5 — `09_Coklu_Platform_Web_Mobil_Masa.md`

**Süre:** 2 saat
**Önkoşul:** AGENTS.md, 03_Razor, 08_Performans

**Doldur:**
1. **Platform Matrisi Tablosu** — Web/Mobil/Tablet/Kiosk/Masaüstü teknoloji eşleştirme
2. **Paylaşılan `*.Ortak`** — DTO, validation, enum tek yerde + dependency rules
3. **Web (Blazor WASM)** — `Interactive Auto` render mode, varsayılan stack
4. **Mobil (.NET MAUI Blazor Hybrid)** — `.csproj` config, paylaşılan Razor, kamera/GPS/barkod servisleri
5. **Masaüstü (WPF + WebView2 Blazor Hybrid)** — proje template, sistem tepsisi, ClickOnce/MSIX
6. **PWA** — Service Worker, `manifest.json`, install prompt, offline-first cache strategy
7. **Admin Layout (3 Sütun)** — MudDrawer + content + sağ aktivite panel kod iskeleti
8. **Responsive Stratejisi** — `03_Razor` §6'ya referans + ekstra Touch optimizasyon
9. **Offline Senkronizasyon** — yerel SQLite + WolverineFx queue + sync API
10. **MudBlazor Alternatifleri Detaylı Tablo** — 7 lib, bileşen sayısı, lisans, güçlü/zayıf
11. **Platform-Spesifik Kod Ayrımı** — `#if ANDROID`, partial class file naming convention
12. **Push Notification** — Web Push (VAPID), Windows Toast, MAUI Firebase wrapper
13. **Test Stratejisi** — bUnit, MAUI, WPF, Playwright E2E
14. **Build & Deploy** — `dotnet publish` Web, `.aab` Android, `.msix` Windows
15. **Öz-denetim** — 13 madde

---

### 👷 İşçi #6 — `10_Test_Derleme_Pipeline.md`

**Süre:** 1.5 saat
**Önkoşul:** AGENTS.md, tüm 02-09

**Doldur:**
1. **Test Projesi İskeleti** — `[Proje].Testler` csproj, sln referansı, NuGet
2. **5 Test Standardı** — örnek `UrunKontrolcuTestleri` ile 5 senaryo
3. **Testcontainers** — `Testcontainers.PostgreSql` setup, fixture, IClassFixture
4. **WebApplicationFactory** — Custom factory, DI override (mock email, vb.)
5. **bUnit** — `UrunKart` bileşeni testi, render + event tetikleme
6. **Playwright** — E2E örnek (login → ürün ekle → görüldü mü), headless CI
7. **MAUI/WPF Test** — UI thread, mock pattern
8. **Pre-Commit Kontrol Listesi** — build, test, .env, hardcoded scan
9. **DB Yedek Protokolü** — bash adımlar, otomatik Hangfire
10. **GitHub Actions** — `.github/workflows/ci.yml` örneği (build matrix, test)
11. **Coolify Deploy** — nixpacks.json, env vars
12. **Kod Kalite Gate** — SonarQube veya CodeQL config + coverage hedefi
13. **Performans Test** — k6 / NBomber örnek scenarios
14. **Smoke Test** — production sonrası /api/health vb. kritik endpoint kontrolü
15. **Öz-denetim** — 12 madde

---

## 🔄 İŞ AKIŞI (Her İşçi)

```
1. AGENTS.md + ön okuma dosyaları oku
2. Atanan iskelet dosyayı oku
3. Bu brief'te kendi bölümüne git
4. Doldur (yukarıdaki yazım standardına uy)
5. status: TAMAM yap (frontmatter)
6. 99_YASAKLAR'a yeni yasak çıktıysa ekle (Ustam onayı gerek)
7. Tamamlandı bildirimi: "✓ İşçi #N — 0X_DosyaAdi.md tamamlandı, {satır} satır"
```

---

## 📊 KALİTE KONTROL (Tamamlananlar)

Her işçi dosyasını teslim ettikten sonra **gözden geçirme**:

```
[ ] Frontmatter doğru (name, description, status: TAMAM)
[ ] Marka-nötr (VIZITLINK3D / kapı / bronz YOK)
[ ] %100 Türkçe açıklama
[ ] Kod örnekleri C# 14 / .NET 10 / Blazor 10
[ ] Wrapper isimleri kullanıldı
[ ] ❌/✅ örnek dengesi (her major konsept)
[ ] Diğer dosyalara link verildi
[ ] Öz-denetim checklist eklendi
[ ] 400-800 satır arası
[ ] UTF-8 encoding (Türkçe karakter bozulmamış)
```

---

## 🎯 SONUÇ (Tüm İşçiler Bitince)

Master şablon hazır → `C:\Sablonlar\AjanSablonu\` her yeni projeye kopyalanır.
Sadece `00_PROJE_BILGISI.md` doldurulur, gerisi AYNEN kalır.

---

*Bu görev listesi Ustam tarafından hazırlandı. İşçi modeller kendi alanlarını doldurur.*
*Versiyon: 1.0 | 2026-05-14*
