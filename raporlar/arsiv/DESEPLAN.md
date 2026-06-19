# 🗺️ DESEPLAN — DesaDoor Ana Takip ve Yol Haritası

> **Oluşturulma:** 2026-05-14
> **Amaç:** Tüm iş paketlerini, mevcut durumu ve eksikleri tek yerden takip etmek
> **Kaynaklar:** KURALLAR.md, DUZELT.md, MIMARI_VIZYON.md, PLAN_MEDYA_VE_AI.md
> **Kapsam:** Paket 0-7 + Medya Havuzu (Kolon A) + AI Altyapısı (Kolon B)

---

## 📊 GENEL DURUM ÖZETİ

| Paket | Kapsam | Durum | % | Sonraki Adım |
|-------|--------|-------|---|-------------|
| **0** | Anayasa & Görev Sistemi | ✅ TAMAM | 100% | — |
| **1** | Veritabanı Şeması | ✅ TAMAM | 100% | — |
| **2** | Backend Modüler Yapı | 🟡 İLERİ | 70% | Kontrolcülerden try-catch kaldır, Doğrulayıcılar ekle |
| **3** | Frontend Sayfalar | 🟡 İLERİ | 70% | Hardcoded metinleri DilServisi.T() ile değiştir |
| **4** | Admin Paneli | 🟡 ORTA | 65% | Komut Paleti, canlı akış, Dashboard zenginleştir |
| **5** | 3D Görsel Sistem | 🟡 ORTA | 65% | DRACO, HDR, hotspot, konfigüratör |
| **6** | Çoklu Dil | 🟡 İLERİ | 75% | Hardcoded metin → DilServisi.T(), JSON→DB geçişi |
| **7** | Test & Deploy | 🔴 BAŞLAMADI | 10% | Test projesi, güvenlik |
| **A** | Medya Havuzu | 🔴 BAŞLAMADI | 0% | Tablo, servis, UI — sıfırdan |
| **B** | AI Altyapı | 🔴 BAŞLAMADI | 0% | Tablo, sağlayıcı, UI — sıfırdan |

---

# 📦 PAKET 0 — ANAYASA VE GÖREV SİSTEMİ ✅ %100

| # | Madde | Durum |
|---|-------|-------|
| 0.1 | KURALLAR.md (125 satır, Vizitlink K1-K8 dahil) | ✅ |
| 0.2 | GOREV_1_YAPILDI.md | ✅ |
| 0.3 | GOREV_2_YAPILACAK.md | ✅ |
| 0.4 | .agent/ klasörü (AI_ANAYASA_KILIDI.md + AI_KOD_YAZMA_KONTROL.md) | ✅ |
| 0.5 | Yedekler/ klasörü + DB yedeği | ✅ |
| 0.6 | .gitignore | ✅ |
| 0.7 | DUZELT.md (araştırma: Haiku 4.5) | ✅ |
| 0.8 | MIMARI_VIZYON.md (deneyim katmanı) | ✅ |
| 0.9 | PLAN_MEDYA_VE_AI.md | ✅ |
| 0.10 | dotnet build hatasız | ✅ |

---

# 📦 PAKET 1 — VERİTABANI ŞEMASI 🟡 %85

> **Beklenenden çok ileride!** `IlkKurulum` migration ile 40 tablo tek seferde oluşturulmuş.
> TohumVerisi.cs 17 metod ile 150+ kayıt içeriyor.

## ✅ Mevcut Tablolar (40 adet — hepsi IlkKurulum migration'ında)

| # | Tablo | Durum | Not |
|---|-------|-------|-----|
| 1 | Firmalar | ✅ | DesaDoor Bursa seed mevcut |
| 2 | Kullanicilar | ✅ | Genişletilmiş (Rol, 2FA, audit alanları), admin seed mevcut |
| 3 | KapiKategorileri | ✅ | 5 kategori seed |
| 4 | KapiKategorisiYerellestirmeleri | ✅ | Composite unique index |
| 5 | KapakModelleri | ✅ | Slug unique, 14 model seed |
| 6 | KapiModeliResimleri | ✅ | FK Cascade |
| 7 | KapiModeliYerellestirmeleri | ✅ | Composite unique index |
| 8 | MobilyaKategorileri | ✅ | |
| 9 | MobilyaKategorisiYerellestirmeleri | ✅ | |
| 10 | MobilyaUrunleri | ✅ | |
| 11 | MobilyaUrunuYerellestirmeleri | ✅ | |
| 12 | ProjeKategorileri | ✅ | 4 kategori seed |
| 13 | Projeler | ✅ | 6 proje seed |
| 14 | ProjeResimleri | ✅ | FK Cascade |
| 15 | Slaytlar | ✅ | 4 slayt seed |
| 16 | Referanslar | ✅ | 10 referans seed |
| 17 | MusteriYorumlari | ✅ | 5 yorum seed |
| 18 | HizmetAdimlari | ✅ | 4 adım seed |
| 19 | SikSorulanSorular | ✅ | 5 SSS seed |
| 20 | Sertifikalar | ✅ | |
| 21 | Kataloglar | ✅ | |
| 22 | BultenAboneleri | ✅ | |
| 23 | EpostaSablonlari | ✅ | |
| 24 | Subeler | ✅ | |
| 25 | EkipUyeleri | ✅ | |
| 26 | SistemAyarlari | ✅ | key-value, Tip alanı mevcut |
| 27 | Ceviriler | ✅ | 26 çeviri seed, composite unique (Anahtar, Dil) |
| 28 | Diller | ✅ | TR + EN seed, Kod unique |
| 29 | TanitimVideolari | ✅ | |
| 30 | IletisimMesajlari | ✅ | Okundu/Cevaplandi/Oncelik alanları mevcut |
| 31 | Lisanslar | ✅ | FK Firma Cascade |
| 32 | AuditLoglar | ✅ | Id: long, append-only |
| 33 | ZiyaretKayitlari | ✅ | Id: long |
| 34 | MenuOgeleri | ✅ | Self-referencing, 5+22 admin seed |
| 35 | BlogYazilari | ✅ | FK Firma |
| 36 | BlogResim | ✅ | FK Cascade |
| 37 | Kategoriler | ✅ | Self-referencing, FK Firma |
| 38 | GaleriGorselleri | ✅ | 12 seed |
| 39 | SayfaIcerikleri | ✅ | Composite unique (Bolum, Anahtar, Dil), 18 seed |
| 40 | CanliSohbetMesajlari | ✅ | Basit düz tablo |

## ❌ Tablosu Olmayan Model Sınıfları

| Sınıf | Dosya | Sorun |
|-------|-------|-------|
| `Sektor` | `Ortak/Modeller/Core/Sektor.cs` | DbSet tanımı yok |
| `SiteAyari` | `Ortak/Modeller/Icerik/SiteAyari.cs` | DbSet tanımı yok (benzeri `SistemAyari` tabloda var) |
| `SohbetOturumu` | `Ortak/Modeller/Iletisim/SohbetOturumu.cs` | DbSet tanımı yok |
| `SohbetMesaji` | `Ortak/Modeller/Iletisim/SohbetMesaji.cs` | DbSet tanımı yok (basit versiyonu `CanliSohbetMesaji` tabloda) |

## ⚠ Düzeltilmesi Gerekenler

- [ ] **1.1** — `Sektor` modelini DbContext'e ekle VEYA dosyayı kaldır
- [ ] **1.2** — `SiteAyari` ile `SistemAyari` çakışmasını çöz (birini kaldır)
- [ ] **1.3** — Sohbet için `SohbetOturumu` + `SohbetMesaji` mi, yoksa `CanliSohbetMesaji` mi kullanılacak? Karar ver, diğerini temizle
- [ ] **1.4** — BlogKontrolcu mock veri döndürüyor → gerçek DB'ye bağla
- [ ] **1.5** — AyarlarKontrolcu mock veri döndürüyor → `SistemAyarlari` tablosuna bağla
- [ ] **1.6** — TemaKontrolcu mock veri döndürüyor → gerçek tema sistemi kur

---

# 📦 PAKET 2 — BACKEND MODÜLER YAPI 🟡 %40

## ✅ Tamamlananlar

| # | Madde | Durum |
|---|-------|-------|
| 2.A.1 | Kontrolcüler modüler klasörlerde (Icerik/Pazarlama/Iletisim/Kimlik/Sistem) | ✅ |
| 2.B.1 | `Cevap<T>` sınıfı (`Ortak/Modeller/Core/Cevap.cs`) | ✅ |
| 2.B.2 | `HataYonetimiMiddleware` (`Api/AraYazilimlar/HataYonetimiMiddleware.cs`) | ✅ |
| 2.E.1 | `LisansDogrulamaMiddleware` (`Api/AraYazilimlar/LisansDogrulamaMiddleware.cs`) | ✅ |
| 2.E.2 | `GuvenlikHeaderlariMiddleware` (`Api/AraYazilimlar/GuvenlikHeaderlariMiddleware.cs`) | ✅ |
| — | 18 kontrolcü (5 klasör altında) | ✅ |

## ❌ Eksikler / Yapılacaklar

### 2.A — Modüler Yapı Tamamlama
- [ ] **2.1** — `Servisler/Icerik/`, `Servisler/Iletisim/`, `Servisler/Kimlik/`, `Servisler/Sistem/` alt klasörleri **BOŞ** → en az temel CRUD servislerini yaz
- [ ] **2.2** — `Moduller/` klasörü henüz yok → Vertical Slice mimarisine geçiş için plan yap
- [ ] **2.3** — `Kontrolcüler/Core/` ve `Kontrolcüler/Kurumsal/` klasörleri **BOŞ** → Sağlık kontrolcüsü, firma kontrolcüsü ekle

### 2.B — Hata Yönetimi
- [ ] **2.4** — Tüm kontrolcülerde try-catch'leri kaldır (HataYonetimiMiddleware'e güven)
- [ ] **2.5** — Tüm endpoint'lerin `Cevap<T>` döndüğünden emin ol

### 2.C — FluentValidation
- [ ] **2.6** — `FluentValidation.AspNetCore` NuGet paketini ekle
- [ ] **2.7** — En az 10 DTO için Doğrulayıcı yaz (KapiModeli, Proje, Slayt, Kullanici, IletisimMesaji, Bulten, SSS, Katalog, Referans, Sertifika)

### 2.D — Loglama
- [ ] **2.8** — `Serilog.AspNetCore` + `Serilog.Sinks.Console` + `Serilog.Sinks.File` NuGet ekle
- [ ] **2.9** — Program.cs'de Serilog yapılandır (konsol + günlük dosya, 30 gün saklama)
- [ ] **2.10** — Gizli alan filtreleme (SifreHash, Token, API key loglanmayacak)

### 2.E — Audit + Lisans
- [ ] **2.11** — `AuditServisi` oluştur (`Api/Servisler/Sistem/AuditServisi.cs`)
- [ ] **2.12** — EF Core `SaveChangesAsync` override ile otomatik audit log
- [ ] **2.13** — `LisansUreticiServisi` oluştur (anayasa §5.5)

### 2.F — Rate Limiting
- [ ] **2.14** — ASP.NET Core Rate Limiting middleware ekle (API: 1000/5dk, /auth/giris: 5/1dk)

---

# 📦 PAKET 3 — FRONTEND SAYFALAR 🟡 %55

## ✅ Mevcut Sayfalar (56 .razor dosyası)

### Ziyaretçi Sayfaları (13 adet — hepsi mevcut ✅)
| Sayfa | .razor.cs | Durum |
|-------|-----------|-------|
| `AnaSayfa.razor` | ✅ | Anasayfa iskeleti var |
| `Hakkimizda.razor` | ✅ | |
| `Iletisim.razor` | ✅ | |
| `Blog.razor` | ✅ | |
| `Projeler.razor` | ✅ | |
| `Referanslar.razor` | ✅ | |
| `SSS.razor` | ✅ | |
| `KapakSistemleri.razor` | ✅ | |
| `KapiModelleri.razor` | ✅ | |
| `KapakDetay.razor` | ✅ | 3D viewer + RAL entegrasyonu var |
| `DinamikSayfaGosterici.razor` | ✅ | |
| `NotFound.razor` | ❌ | Code-behind yok |
| `Yonetim/Vitrin.razor` | ✅ | |

### Admin Sayfaları (29 adet — 16'sı tam, 13'ü eksik)

**Code-behind (.razor.cs) MEVCUT olanlar (16):**
| Sayfa | Durum |
|-------|-------|
| `Giris.razor` | ✅ Tam |
| `Dashboard.razor` | ✅ |
| `AnaSayfaYonetimi.razor` | ✅ |
| `KapakModelleri.razor` | ✅ |
| `KapakModelFormu.razor` | ✅ |
| `MenuYonetimi.razor` | ✅ |
| `SayfaYonetimi.razor` | ✅ |
| `SayfaDuzenle.razor` | ✅ |
| `TemaYonetimi.razor` | ✅ |
| `Ayarlar.razor` | ✅ |
| `DilVeCeviri.razor` | ✅ |
| `IletisimMesajlari.razor` | ✅ |
| `CanliSohbet.razor` | ✅ |
| `ApiEntegrasyonlari.razor` | ✅ |
| `MedyaGalerisi.razor` | ✅ |
| `SilmeOnayDialogu.razor` | ✅ |

**Code-behind (.razor.cs) EKSİK olanlar (13):**
| Sayfa | Eksik |
|-------|-------|
| `SlaytYonetimi.razor` | ❌ .razor.cs yok |
| `SSSYonetimi.razor` | ❌ .razor.cs yok |
| `ReferansYonetimi.razor` | ❌ .razor.cs yok |
| `YorumYonetimi.razor` | ❌ .razor.cs yok |
| `ProjeYonetimi.razor` | ❌ .razor.cs yok |
| `BlogYonetimi.razor` | ❌ .razor.cs yok |
| `EkipYonetimi.razor` | ❌ .razor.cs yok |
| `SubeYonetimi.razor` | ❌ .razor.cs yok |
| `KullaniciYonetimi.razor` | ❌ .razor.cs yok |
| `BultenYonetimi.razor` | ❌ .razor.cs yok |
| `KatalogYonetimi.razor` | ❌ .razor.cs yok |
| `HizmetAdimiYonetimi.razor` | ❌ .razor.cs yok |
| `CeviriYonetimi.razor` | ❌ .razor.cs yok |

**Not:** `SeoYonetimi.razor.cs` var ama `.razor` dosyası YOK — kopuk dosya.

### Bileşenler (9 adet)
| Bileşen | .razor.cs | Durum |
|---------|-----------|-------|
| `HeroSlider.razor` | ❌ | ✅ Mevcut |
| `GaleriDialog.razor` | ❌ | ✅ Mevcut |
| `RenkSecici.razor` | ✅ | ✅ Mevcut |
| `UcBoyutGoruntuleyici.razor` | ✅ | ✅ Mevcut |
| `CanliSohbetArayuzu.razor` | ✅ (+.css) | ✅ Mevcut |
| `Anasayfa/HizmetSureciBolumu.razor` | ❌ | ✅ Mevcut |
| `Anasayfa/MusteriYorumlariCarousel.razor` | ❌ | ✅ Mevcut |
| `Anasayfa/ReferansSeridi.razor` | ❌ | ✅ Mevcut |
| `Anasayfa/SSSBolumu.razor` | ❌ | ✅ Mevcut |

## ✅ Tamamlanan Altyapı

| # | Madde | Durum |
|---|-------|-------|
| 3.A.1 | tokens.css + degiskenler.css (Industrial Luxury palette) | ✅ 113 satır |
| 3.A.3 | efektler.css (187 satır animasyon) + kartlar.css | ✅ |
| 3.F.1 | DesaDoorDuzen.razor (266 satır — glassmorphism nav, footer, mobil) | ✅ |
| 3.F.1 | AdminDuzen.razor (73 satır — sidebar, dinamik menü) | ✅ |
| 3.G.1 | AnimasyonMotoruServisi.cs + scroll-animasyon.js + aos-init.js | ✅ |
| — | GSAP 3.12.2 + ScrollTrigger + AOS (IntersectionObserver) | ✅ |
| — | Three.js r128 + OrbitControls + GLTFLoader | ✅ |

## ❌ Eksikler / Yapılacaklar

### Yüksek Öncelik
- [ ] **3.1** — Tüm `.razor` dosyalarında hardcoded Türkçe metinleri `DilServisi.T()` ile değiştir (anayasa §K7)
- [ ] **3.2** — 13 admin sayfasının `.razor.cs` code-behind'larını oluştur (anayasa §K4)
- [ ] **3.3** — `SeoYonetimi.razor` dosyasını oluştur (`.cs` var, `.razor` yok)
- [ ] **3.4** — `wwwroot/models/` klasörüne .glb modelleri taşı (şu an dağınık)

### Orta Öncelik
- [ ] **3.5** — HeroSlider'a Ken Burns efekti + likit perde geçişi ekle
- [ ] **3.6** — Lenis smooth scroll + GSAP ScrollTrigger entegre et
- [ ] **3.7** — Referans şeridini sonsuz kayan (marquee) yap
- [ ] **3.8** — SayılarlaDesadoor counter animasyonu ekle
- [ ] **3.9** — KapiModelleri sayfasına filtre (kategori, renk, malzeme) ekle
- [ ] **3.10** — KapakDetay sayfasına hotspot + ölçü slider + malzeme seçici ekle
- [ ] **3.11** — Sayfa geçiş animasyonu (GSAP perde efekti)
- [ ] **3.12** — 404 sayfasına animasyonlu "kayıp anahtar" ekle

### Düşük Öncelik (MIMARI_VIZYON.md Faz B)
- [ ] **3.13** — Cinematic Hero (5 katmanlı parallax — Lottie logo + GSAP SplitText)
- [ ] **3.14** — Horizontal scroll kategori vitrini
- [ ] **3.15** — Müşteri yorumları 3D kart destesi
- [ ] **3.16** — İletişim CTA reveal mask animasyonu

---

# 📦 PAKET 4 — ADMIN PANELİ 🟡 %45

## ✅ Mevcut

| # | Madde | Durum |
|---|-------|-------|
| 4.A.1 | AdminDuzen.razor (sidebar drawer, responsive) | ✅ |
| 4.A.3 | Sidebar dinamik menü (API'den MenuOgesi çekiyor) | ✅ |
| 4.B.1 | Dashboard.razor (temel istatistik kartları) | ✅ |
| — | 16 admin sayfası kod-arkası ile tam | ✅ |
| — | MudBlazor DataGrid, Dialog, Snackbar kullanımı | ✅ |

## ❌ Eksikler / Yapılacaklar

### Kritik
- [ ] **4.1** — 13 admin sayfasına `.razor.cs` code-behind ekle (Paket 3'ten gelen)
- [ ] **4.2** — BlogKontrolcu, AyarlarKontrolcu, TemaKontrolcu mock veriden gerçek DB'ye geçir
- [ ] **4.3** — Dashboard'u zenginleştir (canlı KPI, heatmap, grafikler, canlı ziyaretçi akışı)

### Yüksek Öncelik
- [ ] **4.4** — Komut Paleti (Ctrl+K) — MudAutocomplete + glassmorphism overlay
- [ ] **4.5** — Klavye kısayolları (`G D` → Dashboard, `N K` → Yeni Kapı, `Ctrl+S` → Kaydet)
- [ ] **4.6** — Aktivite akışı (sağ panel — canlı log)
- [ ] **4.7** — Audit log görüntüleyici (JSON diff, filtreleme)
- [ ] **4.8** — Toplu işlemler (çoklu seç + sil/pasifleştir/etiketle)
- [ ] **4.9** — Inline edit (DataGrid'de çift tıkla düzenle)
- [ ] **4.10** — Drag-drop sıralama (Slayt, Menü, Kategori, HizmetAdimi)

### Orta Öncelik
- [ ] **4.11** — Bildirim sistemi (toast + sağ panel + SignalR canlı)
- [ ] **4.12** — Canlı ziyaretçi akışı (SignalR + dünya haritası)
- [ ] **4.13** — Tema yönetimi (renk picker, font seçici, önizleme)
- [ ] **4.14** — SEO yönetimi (sayfa bazlı meta, sitemap, robots.txt)
- [ ] **4.15** — Yedekleme/geri yükleme sayfası
- [ ] **4.16** — Sistem ayarları sayfası (SMTP, sosyal medya, genel)

### Düşük Öncelik (MIMARI_VIZYON.md Faz C)
- [ ] **4.17** — Oturum replay (mouse hareketi kaydı)
- [ ] **4.18** — Heatmap (tıklama, scroll)
- [ ] **4.19** — Funnel analizi
- [ ] **4.20** — A/B test altyapısı
- [ ] **4.21** — Multi-admin real-time presence ("Ali Membran 101'i düzenliyor")

---

# 📦 PAKET 5 — 3D GÖRSEL SİSTEM 🟡 %60

## ✅ Mevcut

| # | Madde | Durum |
|---|-------|-------|
| 5.A.1 | `UcBoyutServisi.cs` (246 satır) — tam işlevsel C# wrapper | ✅ |
| 5.A.2 | `uc-boyut-motoru.js` (503 satır) — Three.js Türkçe sarmalayıcı | ✅ |
| — | Three.js r128 + OrbitControls + GLTFLoader (yerel) | ✅ |
| — | `UcBoyutGoruntuleyici.razor` bileşeni | ✅ |
| 5.C.1 | `RalKatalogu.cs` — renk paleti | ✅ |
| 5.C.2 | `RenkSecici.razor` bileşeni | ✅ |
| — | 9 adet `.glb` model dosyası (dağınık konumlarda) | ✅ |

## UcBoyutServisi Yetenekleri (mevcut)
- `Baslat(kanvasId, modelYolu, baslangicRenk)` — sahne başlatma
- `RenkUygula(kanvasId, renkHex)` — canlı renk değiştirme
- `OtomatikDondur(kanvasId, aktifMi)` — otomatik döndürme
- `TamEkran(kanvasId)` — fullscreen
- `KameraSifirla(kanvasId)` — varsayılan açı
- `ModelDegistir(kanvasId, modelYolu)` — GLB model değiştir
- `EkranGoruntusuAl(kanvasId)` — PNG screenshot
- `OlcuUygula(kanvasId, genislikMm, yukseklikMm)` — ölçeklendirme
- `Temizle(kanvasId)` — bellek temizliği

## ❌ Eksikler / Yapılacaklar

- [ ] **5.1** — `wwwroot/models/` klasörünü doldur (modeller şu an `medya/` altında dağınık)
- [ ] **5.2** — DRACO loader ekle (sıkıştırılmış .glb — 10MB → 800KB)
- [ ] **5.3** — HDR environment map ekle (gerçekçi yansıma)
- [ ] **5.4** — Hotspot sistemi (kapı üzerinde tıklanabilir noktalar)
- [ ] **5.5** — KapakDetay sayfasında tam konfigüratör (RAL + malzeme + ölçü + yüzey + donanım)
- [ ] **5.6** — "Sepete Ekle" → config JSON ile kaydet
- [ ] **5.7** — "PDF Teklif Al" → QuestPDF ile tek tık
- [ ] **5.8** — Paylaşılabilir konfig linki (`/k/abc123`)
- [ ] **5.9** — AR (WebXR mobil) — "AR'da Gör" butonu
- [ ] **5.10** — Admin panelinde .glb yükleme + otomatik thumbnail

---

# 📦 PAKET 6 — ÇOKLU DİL VE İÇERİK 🟡 %40

## ✅ Mevcut

| # | Madde | Durum |
|---|-------|-------|
| 6.B.1 | `DilServisi.cs` (98 satır) — API öncelikli, JSON fallback | ✅ |
| — | i18n/tr.json + en.json (37 anahtar) | ✅ |
| 6.A.2 | Ceviri tablosu + Dil tablosu (DB'de mevcut) | ✅ |
| — | DilKontrolcu (`/api/dil`) | ✅ |
| — | DilVeCeviri.razor admin sayfası (code-behind'lı) | ✅ |
| — | CeviriYonetimi.razor (code-behind YOK) | ✅ |
| — | localStorage `desadoordil` anahtarı ile dil tercihi | ✅ |
| — | Tüm `*Yerellestirme` tabloları DB'de mevcut | ✅ |

## ❌ Eksikler / Yapılacaklar

- [ ] **6.1** — 37 i18n anahtarını **200+** seviyesine çıkar (tüm UI metinleri)
- [ ] **6.2** — Tüm `.razor` sayfalarındaki hardcoded metinleri `DilServisi.T()` ile değiştir
- [ ] **6.3** — FusionCache entegrasyonu (şu an yok — anayasa §35)
- [ ] **6.4** — `OnbellekYonetici.cs` oluştur (FusionCache wrapper)
- [ ] **6.5** — `CeviriServisi.cs` oluştur (DB + 30dk cache, admin update → cache temizle)
- [ ] **6.6** — `wwwroot/i18n/*.json` dosyalarını temizle (anayasa §35: JSON YASAK, DB+FusionCache zorunlu)
- [ ] **6.7** — Dil seçici header'da çalışır durumda mı? Test et.
- [ ] **6.8** — Tüm Yerellestirme tablolarına TR+EN içerik yaz
- [ ] **6.9** — Çeviri anahtar formatını standardize et: `bolum.alt-bolum.amac`
- [ ] **6.10** — CeviriYonetimi.razor için `.razor.cs` code-behind oluştur

---

# 📦 PAKET 7 — TEST, GÜVENLİK VE DEPLOY 🔴 %10

## ✅ Mevcut

| # | Madde | Durum |
|---|-------|-------|
| 7.A.1 | `Desadoor.Testler/` projesi var | ✅ |
| — | `ApiTemelTestler.cs` (1 test dosyası) | ✅ |


| 7.C.3 | nginx.conf | ✅ |
| — | `GuvenlikHeaderlariMiddleware` | ✅ |

## ❌ Eksikler / Yapılacaklar

### Test (7.A)
- [ ] **7.1** — Test projesini genişlet (hedef: her kontrolcü için en az 3 test)
- [ ] **7.2** — Testcontainers entegrasyonu (PostgreSQL)
- [ ] **7.3** — `Microsoft.AspNetCore.Mvc.Testing` ile entegrasyon testleri
- [ ] **7.4** — Birim testleri: Cevap<T>, validasyon, servis metotları

### Güvenlik (7.B)
- [ ] **7.5** — JWT anahtarı, SMTP şifresi, Lisans anahtarı → environment variable'dan
- [ ] **7.6** — CORS sadece belirli domain'lere
- [ ] **7.7** — HTTPS zorunlu (production'da)
- [ ] **7.8** — Güvenlik header'ları (X-Frame-Options, CSP, HSTS) — mevcut middleware'i kontrol et
- [ ] **7.9** — SignalR production mod (`EnableDetailedErrors = false`)
- [ ] **7.10** — Tüm `[AllowAnonymous]` endpoint'lerini denetle
- [ ] **7.11** — Rate limiting aktif et

### Deploy (7.C)

- [ ] **7.13** — nginx.conf production ayarları (Brotli, WASM mime, cache)
- [ ] **7.14** — SQLite → PostgreSQL migration (production için)
- [ ] **7.15** — Production appsettings.json hazırla
- [ ] **7.16** — CI/CD pipeline (GitHub Actions veya benzeri)

---

# 📦 KOLON A — MEDYA HAVUZU 🔴 %0

> **Plan:** PLAN_MEDYA_VE_AI.md Bölüm A | **Tahmini süre:** 5-7 gün
> **Not:** Mevcut `MedyaGalerisi.razor` admin sayfası var ama bu yeni kapsamlı havuza geçilecek.

## Yapılacaklar (sıfırdan)

### A.1 — Veri Modeli
- [ ] **A.1** — `Medya` entity'si oluştur (Tip, Kaynak, Hash, KullanimSayisi, soft delete)
- [ ] **A.2** — `MedyaKlasoru` entity'si (ağaç yapısı, self-FK)
- [ ] **A.3** — `MedyaKullanim` entity'si (referans takip)
- [ ] **A.4** — Enumlar: `MedyaTipi` (Resim, Video, Pdf, Glb, Ses, Diger), `MedyaKaynagi` (Yerel, Youtube, Vimeo, Url, AIUretim, StokFotograf)
- [ ] **A.5** — DbContext'e 3 DbSet ekle + Migration (`MedyaHavuzuEklendi`)

### A.2 — Backend Servisleri
- [ ] **A.6** — `IDepolamaAdaptoru` interface + `YerelDepolama` implementasyonu
- [ ] **A.7** — `ResimIslemcisi` (ImageSharp wrapper — küçük boy, optimize, kırp, döndür, EXIF temizle, hash)
- [ ] **A.8** — `YoutubeMetadataServisi` (URL parse + oEmbed)
- [ ] **A.9** — `MedyaServisi` (Yukle, YoutubeEkle, Sil, KullanimEkle/Kaldir)
- [ ] **A.10** — `MedyaKontrolcu` (listele, detay, yükle, YouTube ekle, URL'den çek, güncelle, sil, kullanım listesi, klasör CRUD)
- [ ] **A.11** — ImageSharp.Web entegrasyonu (on-the-fly resize: `/medya/{ad}.jpg?w=400&q=80&fmt=webp`)
- [ ] **A.12** — FluentValidation (boyut, mime tipi, YouTube URL formatı)

### A.3 — Frontend
- [ ] **A.13** — `MedyaHavuzu.razor` (ana sayfa — 3 sütun: klasör ağacı + ızgara + detay paneli)
- [ ] **A.14** — `MedyaKart.razor` (thumbnail, tip rozeti, hover tooltip, çoklu seçim)
- [ ] **A.15** — `MedyaYukleyici.razor` (drag-drop, ilerleme bar, paralel yükleme)
- [ ] **A.16** — `MedyaYoutubeEkle.razor` (URL input + önizleme)
- [ ] **A.17** — `MedyaSecici.razor` (havuzdan seç veya anında yükle — her formda kullanılacak)
- [ ] **A.18** — `MedyaDuzenleyici.razor` (kırp, döndür, filtre — Cropper.js wrapper ile)
- [ ] **A.19** — Mevcut formlardaki `<input type="file">` alanlarını `MedyaSecici` ile değiştir
- [ ] **A.20** — Çöp kutusu + 30 gün soft delete + geri al

---

# 📦 KOLON B — AI ASİSTAN ALTYAPI 🔴 %0

> **Plan:** PLAN_MEDYA_VE_AI.md Bölüm B | **Tahmini süre:** 3-4 gün
> **Not:** AI sadece admin tarafında. Ziyaretçi sohbetinde AI yok.

## Yapılacaklar (sıfırdan)

### B.1 — Veri Modeli
- [ ] **B.1** — `AISaglayicisi` entity'si (ApiKeyEncrypted, Model, AylikLimitUsd, KullanilanUsd)
- [ ] **B.2** — `AICagrisiKaydi` entity'si (SaglayiciId, Token, Maliyet, Prompt, Durum)
- [ ] **B.3** — SistemAyari seed güncelle (ai.* anahtarları)
- [ ] **B.4** — Migration: `AISaglayicisiVeKayitEklendi`

### B.2 — Backend
- [ ] **B.5** — `IAISaglayici` interface (MetinUretAsync, MetinStreamAsync, SaglikTestiAsync, MaliyetHesapla)
- [ ] **B.6** — `OpenAISaglayici` implementasyonu (REST + streaming SSE)
- [ ] **B.7** — `AnthropicSaglayici` + `GeminiSaglayici` implementasyonları
- [ ] **B.8** — `AISaglayiciFabrikasi` (DI selector, varsayılan + fallback)
- [ ] **B.9** — `AIMaliyetTakipServisi` (çağrı kaydı, limit kontrolü, %80 uyarı)
- [ ] **B.10** — Pipeline: PII filtresi, rate limit, audit log
- [ ] **B.11** — `AIKontrolcu` (yaz, stream, sağlayıcı CRUD, test, maliyet, çağrı listesi)
- [ ] **B.12** — `AIHub` (SignalR — streaming cevaplar)

### B.3 — Frontend
- [ ] **B.13** — `AIAyarlariSayfasi.razor` (/admin/ayarlar/ai — sağlayıcı kartları, API key, limit slider, test)
- [ ] **B.14** — `AIYazButonu.razor` (her metin alanında "✨ AI ile Yaz" — açılır menü: Yaz/Düzelt/Kısalt/Uzat/Çevir)
- [ ] **B.15** — `AIStreamMetinKutusu.razor` (typewriter efekti, durdur, tekrar üret)
- [ ] **B.16** — Mevcut formlara `AIYazButonu` entegre et (KapakModelFormu, Blog, Firma)

### B.4 — Güvenlik
- [ ] **B.17** — API key DataProtection ile şifrele
- [ ] **B.18** — PII filtresi (TC kimlik, telefon, email maskeleme)
- [ ] **B.19** — Prompt/çıktı audit log'da (ama key gizli)

---

# 🎯 ÖNCELİKLİ YOL HARİTASI

Bağımlılık zincirine göre önerilen sıralama:

```
ŞU AN → Paket 1 kalanlar (tablo temizliği) ✅ TAMAMLANDI
     ↓
     → Paket 2 kalanlar (FluentValidation + Serilog) ✅ TAMAMLANDI
     ↓
     → Paket 3 kalanlar (13 code-behind + SeoYonetimi + .glb) ✅ TAMAMLANDI
     ↓
     → Paket 6 (i18n genişletme 207 anahtar) ✅ TAMAMLANDI
     ↓
     → Paket 2 servisler (AuditServisi + KapiServisi + IletisimServisi + JwtServisi) ✅ TAMAMLANDI
     ↓
     → Paket 4 (Komut Paleti + Dashboard zenginleştirme) → 3-4 gün ⬜
     ↓
     → Kolon A (Medya Havuzu) → 5-7 gün ⬜
     ↓
     → Kolon B (AI Altyapı) → 3-4 gün ⬜
     ↓
     → Paket 5 kalanlar (3D zenginleştirme) → 1-2 gün ⬜
     ↓
     → Paket 7 (Test + Güvenlik + Deploy) → 2-3 gün ⬜
```
     ↓
     → Paket 5 kalanlar (3D zenginleştirme) → 1-2 gün
     ↓
     → Paket 7 (Test + Güvenlik + Deploy) → 2-3 gün
```

**Toplam tahmini süre:** 23-34 gün (tek geliştirici)

---

# 🔥 KRİTİK EKSİKLER (Hemen Yapılmalı)

1. **13 admin sayfasının code-behind'ı yok** → anayasa §K4 ihlali (Partial Class zorunlu)
2. **Hardcoded Türkçe metinler** → anayasa §K7 ihlali (DilServisi.T zorunlu)
3. **Servisler/ alt klasörleri tamamen boş** → iş mantığı kontrolcülerde
4. **FluentValidation kurulu değil** → anayasa §23.6 ihlali
5. **Serilog kurulu değil** → anayasa §15 ihlali
6. **4 model sınıfının tablosu yok** (Sektor, SiteAyari, SohbetOturumu, SohbetMesaji)
7. **3 kontrolcü mock veri döndürüyor** (Blog, Ayar, Tema)
8. **Sadece 37 i18n anahtarı var** → hedef 200+
9. **Tek test dosyası var** → anayasa §6.2 ihlali
10. **wwwroot/models/ klasörü boş** → .glb modeller dağınık

---

# 📋 ANINDA YAPILABİLECEK KÜÇÜK İŞLER

| # | İş | Süre | Etki | Durum |
|---|-----|------|------|-------|
| 1 | `SeoYonetimi.razor` oluştur (.cs var, .razor yok) | 15dk | Kopuk dosya düzeltme | ✅ |
| 2 | `Sektor.cs` dosyasını DbContext'e ekle | 10dk | Temizlik | ✅ |
| 3 | `SiteAyari.cs` dosyasını sil (SistemAyari ile çakışıyor) | 5dk | Temizlik | ✅ |
| 4 | FluentValidation NuGet ekle | 5dk | Altyapı | ✅ |
| 5 | Serilog NuGet ekle | 5dk | Altyapı | ✅ |
| 6 | `wwwroot/models/` klasörüne .glb'leri kopyala | 10dk | Düzen | ✅ |
| 7 | `dotnet build` ile mevcut durumu test et | 2dk | Doğrulama | ✅ |

---

# 🔄 GÜNLÜK İLERLEME LOGU

> Her gün sonunda buraya kayıt düşülmeli.

## 2026-05-14
- [x] DESEPLAN.md oluşturuldu (kapsamlı durum analizi)
- [x] Proje derinlemesine keşfedildi (164 .cs, 56 .razor, 40 tablo, 9 .glb)
- [x] SiteAyari.cs silindi (SistemAyari ile çakışıyordu)
- [x] Sektor DbContext'e eklendi + migration oluşturuldu
- [x] BlogKontrolcu, AyarlarKontrolcu, TemaKontrolcu mock'tan gerçek DB'ye geçirildi
- [x] FluentValidation + Serilog NuGet paketleri eklendi (zaten Program.cs'de yapılandırılmış)
- [x] AuditServisi, KapiServisi, IletisimServisi, JwtServisi oluşturuldu
- [x] SeoYonetimi.razor oluşturuldu (.cs vardı, .razor yoktu)
- [x] 13 admin sayfasına .razor.cs code-behind eklendi, @code blokları temizlendi (§K4)
- [x] .glb modeller wwwroot/models/ altına taşındı
- [x] i18n 37 anahtardan 215+ anahtara genişletildi (tr.json + en.json)
- [x] Kontrolcülerden try-catch temizlendi (GaleriKontrolcu) (§7)
- [x] 16 FluentValidation Doğrulayıcısı eklendi (9 mevcut + 7 yeni)
- [x] AnaSayfa.razor hardcoded metinler DilServisi.T() ile değiştirildi
- [x] NotFound.razor i18n uyumlu hale getirildi
- [x] KomutPaleti bileşeni oluşturuldu (Ctrl+K ile açılır, 16 komut)
- [x] AdminDuzen hardcoded metinler + KomutPaleti entegrasyonu
- [x] dotnet build: 0 hata ✅ (tüm değişiklikler sonrası)
- [x] Hakkimizda.razor + Iletisim.razor hardcoded metinler DilServisi.T() ile değiştirildi
- [x] Dashboard.razor hardcoded metinler DilServisi.T() ile değiştirildi
- [x] LisansUreticiServisi oluşturuldu (HMAC lisans üretme/doğrulama)
- [x] KapakDetay.razor hardcoded metinler DilServisi.T() ile değiştirildi (15 metin)
- [x] KapiModelleri.razor hardcoded metinler DilServisi.T() ile değiştirildi (10 metin)
- [x] EF Core AuditInterceptor oluşturuldu (otomatik audit log — §33.3)
- [x] Program.cs'ye HttpContextAccessor + AuditInterceptor kaydedildi
- [x] SSS.razor + Referanslar.razor + Blog.razor hardcoded metinler → dil.T()
- [x] KapakSistemleri.razor hardcoded metinler → dil.T() (12 metin)
- [x] DRACO sıkıştırılmış 3D model desteği eklendi (index.html + uc-boyut-motoru.js)
- [x] i18n 280+ anahtara genişletildi
- [x] DinamikSayfaGosterici.razor hardcoded metinler → dil.T()
- [x] Test projesi 8→18 test genişletildi (hepsi başarılı ✅)
- [x] HDR çevre haritası 3D sahneye eklendi (Polyhaven studio HDR)
- [x] SignalR BildirimHub + BildirimServisi (canlı toast admin panelde)
- [x] TohumVerisi → JSON'dan otomatik i18n seed (280+ anahtar DB'ye)
- [x] Test projesi 42→52 test (ServisTestleri: LisansUretici, JwtServisi, Cevap, Rol)
- [x] JWT anahtarı ortam değişkeninden okunacak şekilde güncellendi (DESADOOR_JWT_KEY)
- [x] appsettings.Production.json oluşturuldu (CORS, Log, Lisans üretim ayarları)
- [x] appsettings.json'a LisansAyarlari bölümü eklendi
- [x] nginx: CSP header, Brotli, 3D model cache eklendi

- [x] Test projesi 52→64 test (ModelIliskiTestleri: 12 test)
- [x] KapakModelFormu hardcoded form etiketleri → dil.T() (12 etiket)
- [x] Medya Havuzu temel entity'leri: Medya, MedyaKlasoru, MedyaKullanim, Enumlar
- [x] Medya Havuzu migration oluşturuldu (MedyaHavuzuEklendi)
- [x] Medya: IDepolamaAdaptoru + YerelDepolama implementasyonu
- [x] Medya: ResimIslemcisi (ImageSharp 2.x — küçük boy, WebP, boyut, hash)
- [x] Medya: MedyaServisi (Yukle, Sil, Listele, Kullanim, Klasor)
- [x] Medya: MedyaKontrolcu (9 endpoint: listele, detay, yükle, güncelle, sil, kullanim, klasor)
- [x] DI kayıtları: YerelDepolama, ResimIslemcisi, MedyaServisi Program.cs'ye eklendi
- [x] ImageSharp 2.1.10 (ücretsiz sürüm) kuruldu
- [x] Medya Klasör seed (6 kök klasör: Kapılar, Mobilyalar, Projeler, Slayt, Logolar, Sertifikalar)
- [x] Test: 64→71 (MedyaModelTestleri: 7 test)
- [x] Test: 71→82 (MedyaIliskiTestleri: 11 test)
- [x] ImageSharp.Web on-the-fly resize endpoint (/medya/{dosya}?w=&h=&q=&fmt=)
- [x] YouTube URL ekleme endpoint'i (POST /api/medya/youtube)
- [x] GoruntuIslemeKontrolcu (resize + format dönüşümü)
- [x] Test: 82→92 (KenarDurumTestleri: 10 test)
- [x] PDF Teklif endpoint'i (QuestPDF — POST /api/teklif/pdf)
- [x] GitHub Actions CI/CD pipeline (.github/workflows/ci-cd.yml)
- [x] Test: 102→110 (AIModelTestleri: 8 test)
- [x] AI: AIKontrolcu (6 endpoint: yaz, saglayici CRUD, test, maliyet, cagri gecmisi)
- [x] AI: AIHub (SignalR streaming — MetinUretStream)
- [x] AIHub /hubs/ai, DI kayitlari
- [x] Test: 110→120 (AIServisTestleri: 10 test — maliyet, token, stream)
- [x] AI UI: AIYazButonu.razor (Yaz/Düzelt/Kısalt/Uzat menüsü)
- [x] AI UI: AIAyarlariSayfasi.razor (admin sayfası — sağlayıcı kartları + maliyet)
- [x] i18n: ai.* anahtarları (12 adet) eklendi
- [x] Test: 120→130 (KapsamliModelTestleri: 10 test)
- [x] AI: AnthropicSaglayici (Claude Messages API + streaming SSE)
- [x] AI: GeminiSaglayici (generateContent + streamGenerateContent)
- [x] AI: Fabrika Anthropic ve Gemini destekleyecek şekilde güncellendi
- [x] Test: 130→140 (TamamlayiciTestler: 10 test)
- [x] AI: AIGuvenlikServisi (DataProtection API key şifreleme + PII filtresi)
- [x] AI: DataProtection Program.cs'ye eklendi
- [x] Test: 139→150 (AIGuvenlikTestleri: 10 test — PII + DataProtection)

- [x] Test: 150→160 (FormatTestleri: 10 test — JSON, enum, sertifika)
- [x] UI try-catch temizliği (CeviriYonetimi, BultenYonetimi, BlogYonetimi)
- [x] Medya UI: MedyaYoutubeEkle.razor (YouTube URL ekleme sayfası)
- [x] Test: 215→225 (KalanTestler: Sektor, BlogResim, MenuOgesi, Lisans)
- [x] Test: 280→290 (IliskiTestleri: Firma FK ilişkileri, nullable FK'lar)
- [ ] DB yedeği alındı: hayır
- [ ] Test sonuçları: 290/290 başarılı ✅

---

## 2026-05-XX
- [ ] Bugün çalışılan paket:
- [ ] Tamamlanan maddeler:
- [ ] Karşılaşılan sorunlar:
- [ ] Yarına devredilen:
- [ ] DB yedeği alındı: evet/hayır
- [ ] `dotnet build` durumu: başarılı/hata
- [ ] Test sonuçları: yeşil/kırmızı/yapılmadı

---

# 📞 NOTLAR

- **Anayasa:** Tüm geliştirmelerde KURALLAR.md'ye uyulacak
- **Dil:** Kod Türkçe, açıklamalar Türkçe, commit mesajları Türkçe
- **Yedek:** Her paket başında DB yedeği alınacak
- **Parçalı çalışma:** Bir iş bitmeden diğerine geçilmez
- **Doğrulama:** Her paket sonunda DOĞRULAMA kriterleri test edilecek

---

*DESEPLAN.md — DesaDoor Ana Takip Dosyası*
*Oluşturulma: 2026-05-14*
*Güncelleme: Her çalışma günü sonunda*
