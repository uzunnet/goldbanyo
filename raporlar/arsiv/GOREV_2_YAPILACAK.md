# VIZITLINK3D — Yapılacak Görevler

> **Anayasa:** KURALLAR.md (Vizitlink v11.0 adaptasyonu)
> **Ana Plan:** DESEPLAN.md (güncel durum için)
> **Eski Plan:** DUZELT.md
> **Başlangıç:** 2026-05-14
> **Toplam Tahmini Süre:** 23-34 gün (güncellendi)

---

## 📊 GENEL DURUM (14.05.2026 keşif sonrası güncel)

| Paket | Kapsam | Süre | Durum | % |
|-------|--------|------|-------|---|
| 0 | Anayasa & Görev Sistemi | 1 gün | ✅ TAMAMLANDI | 100% |
| 1 | Veritabanı Şeması | 1-2 gün kaldı | 🟡 İLERİ (40 tablo mevcut) | 85% |
| 2 | Backend Modüler Yapı | 2-3 gün | 🟡 BAŞLANGIÇ | 40% |
| 3 | Frontend Sayfalar | 4-6 gün | 🟡 ORTA (56 razor mevcut) | 55% |
| 4 | Admin Paneli | 3-4 gün | 🟡 BAŞLANGIÇ | 45% |
| 5 | 3D Görsel Sistem | 1-2 gün | 🟡 ORTA | 60% |
| 6 | Çoklu Dil & İçerik | 2-3 gün | 🟡 BAŞLANGIÇ | 40% |
| 7 | Test, Güvenlik & Deploy | 2-3 gün | 🔴 BAŞLAMADI | 10% |
| A | Medya Havuzu | 5-7 gün | 🔴 BAŞLAMADI | 0% |
| B | AI Altyapı | 3-4 gün | 🔴 BAŞLAMADI | 0% |

---

## ▶ PAKET 0 — ANAYASA VE GÖREV SİSTEMİ ✅

### Tamamlananlar
- [x] 0.1 — KURALLAR.md mevcut, VIZITLINK3D adaptasyon notu ve K1-K8 kuralları eklendi
- [x] 0.2 — GOREV_1_YAPILDI.md güncellendi
- [x] 0.3 — GOREV_2_YAPILACAK.md (bu dosya) güncellendi
- [x] 0.4 — .agent/ klasörü oluşturuldu
- [x] 0.5 — Yedekler/ klasörü ve DB yedeği alındı
- [x] 0.6 — .gitignore oluşturuldu
- [x] Doğrulama: dotnet build hatasız

---

## ▶ PAKET 1 — VERİTABANI ŞEMASI 🟡 %85 (40 tablo mevcut!)

> **Not:** DUZELT.md yazıldığında sadece 7 tablo olduğu sanılıyordu.
> Keşif sonucu: `IlkKurulum` migration ile **40 tablo** zaten oluşturulmuş!
> TohumVerisi.cs 17 metod ile 150+ kayıt içeriyor.

### ✅ Mevcut Tablolar (40/40 — tamamı var)
- [x] Firma, Kullanici (genişletilmiş), KapiKategorisi + Yerellestirme
- [x] KapakModeli + KapiModeliResim + KapiModeliYerellestirme
- [x] MobilyaKategorisi + MobilyaUrunu + Yerellestirmeler
- [x] Proje + ProjeKategorisi + ProjeResim
- [x] Slayt, Referans, MusteriYorumu, HizmetAdimi
- [x] SikSorulanSoru, Sertifika, Katalog
- [x] BultenAbonesi, EpostaSablonu, Sube, EkipUyesi
- [x] SistemAyari, Ceviri, Dil, TanitimVideo
- [x] IletisimMesaji (zenginleştirilmiş), Lisans, AuditLog, ZiyaretKaydi
- [x] MenuOgesi, BlogYazisi + BlogResim, Kategori
- [x] GaleriGorseli, SayfaIcerigi, CanliSohbetMesaji

### ❌ Kalan İşler (5 madde)
- [ ] 1.1 — `Sektor.cs` modelini DbContext'e ekle VEYA dosyayı sil (tablosu yok)
- [ ] 1.2 — `SiteAyari.cs` ile `SistemAyari` çakışmasını çöz (birini sil)
- [ ] 1.3 — Sohbet: `SohbetOturumu`+`SohbetMesaji` mi `CanliSohbetMesaji` mi? Karar ver, diğerini temizle
- [ ] 1.4 — BlogKontrolcu mock veriden gerçek DB'ye geçir
- [ ] 1.5 — AyarlarKontrolcu + TemaKontrolcu mock veriden gerçek DB'ye geçir

---

## ▶ PAKET 2 — BACKEND MODÜLER 🟡 %40

### ✅ Tamamlananlar
- [x] 2.A.1 — Kontrolcüler modüler klasörlerde (Icerik/Pazarlama/Iletisim/Kimlik/Sistem) — 18 kontrolcü
- [x] 2.B.1 — Cevap<T> sınıfı mevcut
- [x] 2.B.2 — HataYonetimiMiddleware mevcut
- [x] 2.E.1 — LisansDogrulamaMiddleware mevcut
- [x] GuvenlikHeaderlariMiddleware mevcut

### ❌ Kalan İşler
- [ ] 2.1 — Servisler/ alt klasörlerini doldur (Icerik/Iletisim/Kimlik/Sistem — HEPSİ BOŞ)
- [ ] 2.2 — Moduller/ Vertical Slice klasör yapısına geçiş planı
- [ ] 2.3 — Kontrolcüler/Core/ + Kurumsal/ boş klasörlerini doldur
- [ ] 2.4 — Tüm kontrolcülerden try-catch'leri kaldır
- [ ] 2.5 — FluentValidation NuGet ekle + en az 10 DTO için Doğrulayıcı yaz
- [ ] 2.6 — Serilog NuGet ekle + Program.cs yapılandır (konsol + günlük dosya)
- [ ] 2.7 — Gizli alan log filtrelemesi (SifreHash, Token, API key)
- [ ] 2.8 — AuditServisi oluştur + EF SaveChanges interceptor
- [ ] 2.9 — LisansUreticiServisi oluştur
- [ ] 2.10 — Rate Limiting ekle

---

## ▶ PAKET 3 — FRONTEND SAYFALAR 🟡 %55 (56 .razor mevcut)

### ✅ Tamamlananlar
- [x] 3.A.1 — tokens.css + degiskenler.css (Industrial Luxury palette, 113 satır)
- [x] 3.A.3 — efektler.css (187 satır) + kartlar.css
- [x] 3.B.1 — HeroSlider bileşeni mevcut
- [x] 3.C.3 — HizmetSureciBolumu mevcut
- [x] 3.C.4 — MusteriYorumlariCarousel mevcut
- [x] 3.C.5 — ReferansSeridi mevcut
- [x] 3.F.1 — VIZITLINK3DDuzen (266 satır) + AdminDuzen (73 satır) mevcut
- [x] 3.G.1 — AnimasyonMotoruServisi + scroll-animasyon.js + aos-init.js mevcut
- [x] Tüm 13 ziyaretçi sayfası .razor olarak mevcut
- [x] 9 bileşen mevcut

### ❌ Kalan İşler
- [ ] 3.1 — Tüm .razor'larda hardcoded metin → DilServisi.T() (anayasa §K7)
- [ ] 3.2 — 13 admin sayfasına .razor.cs code-behind ekle (anayasa §K4)
- [ ] 3.3 — SeoYonetimi.razor oluştur (.cs var, .razor yok)
- [ ] 3.4 — wwwroot/models/ klasörüne .glb'leri taşı
- [ ] 3.5 — HeroSlider zenginleştir (Ken Burns, likit perde geçişi)
- [ ] 3.6 — KapiModelleri sayfasına filtre (kategori, renk, malzeme)
- [ ] 3.7 — KapakDetay zenginleştir (hotspot, ölçü slider, malzeme seçici)
- [ ] 3.8 — SayılarlaVIZITLINK3D counter animasyonu
- [ ] 3.9 — Lenis smooth scroll + GSAP ScrollTrigger
- [ ] 3.10 — Sayfa geçiş animasyonu

---

## ▶ PAKET 4 — ADMIN PANELİ 🟡 %45 (29 sayfa mevcut)

### ✅ Tamamlananlar
- [x] 4.A.1 — AdminDuzen responsive (sidebar drawer, 73 satır)
- [x] 4.A.3 — Sidebar dinamik menü (API'den MenuOgesi çekiyor)
- [x] 4.B.1 — Dashboard temel widget'lar
- [x] 16 admin sayfası code-behind ile tam

### ❌ Kalan İşler
- [ ] 4.1 — 13 admin sayfasına .razor.cs ekle (Slayt, SSS, Referans, Yorum, Proje, Blog, Ekip, Sube, Kullanici, Bulten, Katalog, HizmetAdimi, Ceviri)
- [ ] 4.2 — Komut Paleti (Ctrl+K) — MudAutocomplete + glassmorphism
- [ ] 4.3 — Klavye kısayolları (G D, N K, Ctrl+S, ?)
- [ ] 4.4 — Aktivite akışı (sağ panel canlı log)
- [ ] 4.5 — Dashboard zenginleştir (KPI, heatmap, grafik, canlı ziyaretçi)
- [ ] 4.6 — Audit log görüntüleyici (JSON diff, filtreleme)
- [ ] 4.7 — Toplu işlemler + inline edit + drag-drop sıralama
- [ ] 4.8 — Bildirim sistemi (SignalR canlı toast)
- [ ] 4.9 — Tema + SEO + Yedekleme yönetim sayfaları

---

## ▶ PAKET 5 — 3D GÖRSEL 🟡 %60

### ✅ Tamamlananlar
- [x] 5.A.1 — UcBoyutServisi.cs (246 satır) — tam işlevsel
- [x] 5.A.2 — uc-boyut-motoru.js (503 satır) — Three.js Türkçe sarmalayıcı
- [x] Three.js r128 + OrbitControls + GLTFLoader kurulu
- [x] UcBoyutGoruntuleyici.razor bileşeni mevcut
- [x] RalKatalogu.cs + RenkSecici.razor mevcut
- [x] 9 adet .glb model dosyası mevcut

### ❌ Kalan İşler
- [ ] 5.1 — wwwroot/models/ klasörünü doldur (modeller dağınık)
- [ ] 5.2 — DRACO loader ekle (sıkıştırılmış model desteği)
- [ ] 5.3 — HDR environment map (gerçekçi yansıma)
- [ ] 5.4 — Hotspot sistemi (kapı üzerinde tıklanabilir noktalar)
- [ ] 5.5 — KapakDetay tam konfigüratör (RAL + malzeme + ölçü + yüzey)
- [ ] 5.6 — PDF Teklif Al (QuestPDF) + Paylaş linki + AR görüntüleme

---

## ▶ PAKET 6 — ÇOKLU DİL 🟡 %40

### ✅ Tamamlananlar
- [x] 6.B.1 — DilServisi.cs (98 satır) — API öncelikli, JSON fallback
- [x] i18n/tr.json + en.json (37 anahtar)
- [x] Ceviri + Dil tabloları DB'de mevcut, seed'li
- [x] DilKontrolcu + DilVeCeviri.razor mevcut

### ❌ Kalan İşler
- [ ] 6.1 — 37 anahtarı 200+ seviyesine çıkar
- [ ] 6.2 — Tüm .razor'larda hardcoded metin → DilServisi.T()
- [ ] 6.3 — FusionCache entegrasyonu (OnbellekYonetici.cs)
- [ ] 6.4 — CeviriServisi.cs (DB + 30dk cache)
- [ ] 6.5 — wwwroot/i18n/*.json sil (anayasa §35: JSON YASAK)
- [ ] 6.6 — Tüm Yerellestirme tablolarına TR+EN içerik yaz

---

## ▶ PAKET 7 — TEST & GÜVENLİK & DEPLOY 🔴 %10

### ✅ Tamamlananlar
- [x] 7.A.1 — VIZITLINK3D.Testler projesi mevcut
- [x] 7.C.3 — nginx.conf mevcut
- [x] GuvenlikHeaderlariMiddleware mevcut

### ❌ Kalan İşler
- [ ] 7.1 — Test projesini genişlet (hedef: 50+ test)
- [ ] 7.2 — Testcontainers + WebApplicationFactory entegrasyonu
- [ ] 7.3 — JWT/SMTP/Lisans anahtarlarını env variable'a taşı
- [ ] 7.4 — CORS + HTTPS zorunlu + HSTS
- [ ] 7.5 — Rate limiting aktif et
- [ ] 7.6 — [AllowAnonymous] endpoint denetimi
- [ ] 7.8 — SQLite → PostgreSQL production migration
- [ ] 7.9 — CI/CD pipeline (GitHub Actions)

---

## ▶ KOLON A — MEDYA HAVUZU 🔴 %0 (sıfırdan)

> **Plan:** PLAN_MEDYA_VE_AI.md | **Süre:** 5-7 gün
> Mevcut MedyaGalerisi.razor var ama kapsamlı havuz değil.

- [ ] A.1 — Medya + MedyaKlasoru + MedyaKullanim entity'leri + Migration
- [ ] A.2 — IDepolamaAdaptoru + YerelDepolama implementasyonu
- [ ] A.3 — ResimIslemcisi (ImageSharp wrapper)
- [ ] A.4 — YoutubeMetadataServisi + MedyaServisi
- [ ] A.5 — MedyaKontrolcu (9 endpoint)
- [ ] A.6 — MedyaHavuzu.razor (3 sütun layout)
- [ ] A.7 — MedyaSecici.razor (her formda kullanılacak)
- [ ] A.8 — MedyaYukleyici + MedyaYoutubeEkle + MedyaDuzenleyici
- [ ] A.9 — Mevcut input file'ları MedyaSecici ile değiştir
- [ ] A.10 — ImageSharp.Web on-the-fly resize

---

## ▶ KOLON B — AI ALTYAPI 🔴 %0 (sıfırdan)

> **Plan:** PLAN_MEDYA_VE_AI.md | **Süre:** 3-4 gün
> AI sadece admin tarafında, ziyaretçi sohbetinde yok.

- [ ] B.1 — AISaglayicisi + AICagrisiKaydi entity'leri + Migration
- [ ] B.2 — IAISaglayici interface + 3 implementasyon (OpenAI, Anthropic, Gemini)
- [ ] B.3 — AISaglayiciFabrikasi + AIMaliyetTakipServisi
- [ ] B.4 — AIKontrolcu + AIHub (SignalR streaming)
- [ ] B.5 — AIAyarlariSayfasi.razor (/admin/ayarlar/ai)
- [ ] B.6 — AIYazButonu.razor (her metin alanında "✨ AI ile Yaz")
- [ ] B.7 — API key şifreleme (DataProtection) + PII filtresi

---

*Bu dosya DUZELT.md'den otomatik aktarılmıştır. Her paket tamamlandığında GOREV_1_YAPILDI.md'ye işlenir.*
