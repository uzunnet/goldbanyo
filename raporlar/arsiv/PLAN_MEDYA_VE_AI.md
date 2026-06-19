# 📋 PLAN — MEDYA HAVUZU VE AI ASİSTANLARI UYGULAMA PLANI

> **Amaç:** [MIMARI_VIZYON.md](MIMARI_VIZYON.md) Bölüm 2.14 (AI Asistanları) ve 2.16 (Medya Havuzu) bölümlerini **adım adım** uygulanabilir göreve dönüştürmek.
> **Önkoşul:** [DUZELT.md](DUZELT.md) Paket 0 tamamlanmış (KURALLAR, .agent, Yedekler ✅). Paket 1 migration'ları **henüz yok** — bu plan migration aşaması üstüne kurulur.
> **Anayasa:** [KURALLAR.md](KURALLAR.md) §0, §4, §8, §10, §11, §15, §35
> **Tarih:** 2026-05-14

---

## 🎯 İKİ KOLON, TEK PLAN

Bu plan iki bağımsız özelliği kapsar — sırayla yapılır:

| Kolon | Özellik | Tahmini Süre |
|-------|---------|--------------|
| **A** | Medya Havuzu (resim/video/YouTube + havuz UI) | 5-7 gün |
| **B** | AI Sağlayıcı Yönetimi + "✨ AI ile Yaz" temel altyapı | 3-4 gün |

**Sıra önerisi:** Önce **A (Medya)** — çünkü AI'nın da görsel üretim/alt metin gibi kısımları havuza yazacak. Sonra **B (AI)**.

---

# 🧱 KOLON A — MEDYA HAVUZU

## A.0 — Hazırlık (yarım gün)

### A.0.1 — DB Yedek Al (anayasa §6.1)
- `Yedekler/db/desadoor_YYYYMMDD_medya_oncesi.db`

### A.0.2 — Klasör İskeleti
```
Desadoor.Ortak/Modeller/Medya/
├── Medya.cs
├── MedyaKlasoru.cs
├── MedyaKullanim.cs
└── Enumlar.cs   (MedyaTipi, MedyaKaynagi)

Desadoor.Api/Moduller/Medya/     (vertical slice — anayasa §9.4)
├── Komutlar/
├── Sorgular/
├── Dtolar/
├── Dogrulayicilar/
├── Servisler/   (DepolamaAdaptoru, ResimIslemcisi, YoutubeMetadataServisi)
└── Kontrolcu/   (MedyaKontrolcu.cs)

Desadoor.UI/Bilesenler/Medya/
├── MedyaHavuzu.razor       (ana havuz sayfası)
├── MedyaSecici.razor       (her yerde kullanılan picker)
├── MedyaKart.razor         (tek bir medya kartı)
├── MedyaDetayPanel.razor   (sağ panel — detay/düzenle)
├── MedyaYukleyici.razor    (drag-drop yükleyici)
├── MedyaYoutubeEkle.razor  (YouTube URL ekleme modal)
└── MedyaDuzenleyici.razor  (kırp/döndür/filtre)
```

---

## A.1 — Veri Modeli (1 gün)

### A.1.1 — Medya entity'si
**Alanlar:**
- `Id, FirmaId?, Tip (enum), Kaynak (enum)`
- `Ad, OrijinalAd, DosyaYolu, MiniaturYolu`
- `KaynakUrl` (YouTube embed URL)
- `BoyutByte, Genislik, Yukseklik, SureSaniye, MimeTipi`
- `Hash` (SHA256 — duplicate tespiti)
- `AltMetin, Aciklama, EtiketlerJson`
- `KlasorId?` (FK)
- `KullanimSayisi` (cache — performance için)
- `YukleyenKullaniciId, SilindiMi, SilinmeTarihi`
- `OlusturulmaTarihi, GuncellenmeTarihi`

**Enum tipi:** `Resim, Video, Pdf, Glb, Ses, Diger`
**Enum kaynak:** `Yerel, Youtube, Vimeo, Url, AIUretim, StokFotograf`

### A.1.2 — MedyaKlasoru
- Ağaç yapısı (`UstKlasorId` self-FK)
- `Ad, Slug, Ikon, Renk, SiraNo, AktifMi`
- Seed: "Tümü" (kök), alt klasörler: Kapılar/Mobilyalar/Projeler/Slayt/Logolar/Sertifikalar

### A.1.3 — MedyaKullanim (referans takip)
- `MedyaId, EntiteAdi, EntiteId, AlanAdi, SiraNo`
- Index: `(EntiteAdi, EntiteId)` + `(MedyaId)`

### A.1.4 — DbContext + Migration
- `DesadoorDbContext`'e 3 DbSet
- Unique index: `Medya.Hash` (kısmi — null hariç)
- Migration adı: `MedyaHavuzuEklendi`
- `dotnet ef database update` test

### A.1.5 — TohumVerisi
- 5 kök klasör seed (Kapılar, Mobilyalar, Projeler, Slayt, Logolar)

---

## A.2 — Backend Servisleri (1.5 gün)

### A.2.1 — IDepolamaAdaptoru (Storage abstraction)
- Interface: `YukleAsync`, `SilAsync`, `GetirAsync`, `UrlOlustur`
- 4 implementation:
  - `YerelDepolama` (wwwroot/medya/)
  - `MinioDepolama` (S3-uyumlu — opsiyonel)
  - `R2Depolama` (Cloudflare R2 — opsiyonel)
  - `S3Depolama` (AWS — opsiyonel)
- Hangisi aktif: `SistemAyari` (`medya.depolama` anahtarından)

### A.2.2 — ResimIslemcisi (ImageSharp wrapper — anayasa §11)
- `KucukBoyutOlustur` (200x200 thumbnail)
- `OlcuOptimize` (WebP/AVIF dönüşüm)
- `Kirp, Dondur, Cevir, Filtre` (görsel düzenleme)
- `EksifTemizle` (gizlilik)
- `HashHesapla` (SHA256)

### A.2.3 — YoutubeMetadataServisi
- URL → video ID parse
- oEmbed API ile: başlık, süre, kapak resmi
- Kapak resmini havuza otomatik kopyala (orijinal medyanın MiniaturYolu olarak)

### A.2.4 — MedyaServisi (orchestrator)
- `YukleAsync(stream, ad, klasorId, kullaniciId)` — tüm boru hattı
  - Hash → duplicate kontrol
  - Storage'a yaz
  - Thumbnail üret
  - Metadata oku
  - DB'ye kaydet
- `YoutubeEkleAsync(url, klasorId)`
- `SilAsync(id, kullanimYerleriniDeKaldir: bool)` — soft delete
- `KullanimEkleAsync(medyaId, entiteAdi, entiteId, alanAdi, siraNo)`
- `KullanimKaldirAsync(...)`

### A.2.5 — MedyaKontrolcu (vertical slice)
Endpoint'ler:
- `GET  /api/medya?klasor=&etiket=&q=&tip=` — listele (paginated)
- `GET  /api/medya/{id}` — detay
- `POST /api/medya/yukle` — yerel yükleme (multipart)
- `POST /api/medya/youtube` — YouTube URL ekle
- `POST /api/medya/url` — URL'den çek
- `PUT  /api/medya/{id}` — güncelle (alt metin, etiket, klasör)
- `DEL  /api/medya/{id}?cascade=bool` — sil
- `GET  /api/medya/{id}/kullanim` — nerelerde kullanılıyor
- `POST /api/medya/klasor` — yeni klasör
- `GET  /api/medya/klasorler` — ağaç yapı

### A.2.6 — Görsel Sunucu (ImageSharp.Web)
- `Program.cs`'e `AddImageSharp()` + endpoint
- URL: `/medya/{ad}.jpg?w=400&q=80&fmt=webp` — on-the-fly resize
- Cache: 7 gün

### A.2.7 — FluentValidation Dogrulayicilari
- `MedyaYukleDogrulayici` (boyut, mime tipi)
- `YoutubeEkleDogrulayici` (URL formatı)

---

## A.3 — Frontend (Havuz UI) (2-3 gün)

### A.3.1 — MedyaHavuzu.razor (ana sayfa — /admin/medya)
**Layout 3 sütun:**
- Sol: Klasör ağacı (MudTreeView)
- Orta: Izgara görünüm (MudGrid + virtualize)
- Sağ: Detay paneli (seçili medya bilgi/düzenle)

**Üst bar:**
- Arama, filtre (tip, tarih, etiket), seçim modu
- Butonlar: `+ Yükle`, `+ YouTube`, `+ URL`, `+ Klasör`

**Alt bar (seçim sonrası):**
- "3 dosya seçildi" + [Taşı] [Etiketle] [Sil] [İndir]

### A.3.2 — MedyaKart.razor
- Thumbnail (lazy load)
- Tip rozeti (YT/PDF/3D ikonu sağ üst)
- Hover: hızlı bilgi tooltip
- Çift tık: detay aç
- Çoklu seçim: Ctrl/Shift+click

### A.3.3 — MedyaYukleyici.razor
- Drag-drop zone (MudFileUpload)
- İlerleme bar (paralel yükleme)
- Hata/duplicate uyarı

### A.3.4 — MedyaYoutubeEkle.razor
- URL input + Önizleme (oEmbed çekildikten sonra)
- Onayla → havuza ekle

### A.3.5 — MedyaSecici.razor (her yerde kullanılan)
- Modal/dialog komponenti
- Mevcut havuzdan seç **veya** anında yükle
- `[Parameter] EventCallback<int> SecilenId`
- `[Parameter] bool CokluSecim`
- `[Parameter] MedyaTipi[]? FiltreTipler`

### A.3.6 — MedyaDuzenleyici.razor
- Cropper.js wrapper (anayasa §11 — JS direkt çağrı yasak)
- Kırp / döndür / filtre / parlaklık
- "Yeni sürüm" veya "üzerine yaz" seçimi

### A.3.7 — Mevcut Formlara Entegrasyon
- KapiModeli formu → KapakResim alanı `MedyaSecici` ile değiştirilir
- Slayt formu → ArkaplanResim
- Proje formu → galeri (çoklu)
- Blog formu → KapakResim
- Sertifika formu → Resim
- Tüm hardcoded `<input type="file">` alanları → `MedyaSecici`

---

## A.4 — Test ve Doğrulama (yarım gün)

### A.4.1 — Manuel Test Senaryoları
- [ ] 10 resim yükle (tekli + toplu)
- [ ] Aynı dosyayı 2. kez yükle → duplicate uyarı
- [ ] YouTube URL ekle → kapak otomatik geldi
- [ ] Klasör oluştur, dosya taşı
- [ ] Bir resmi 3 farklı yerde kullan, sil → uyarı listesi
- [ ] Resmi kırp → kullanım yerleri otomatik güncellendi
- [ ] Çöp kutusu → geri al

### A.4.2 — Performans Test
- 1000 resim seed et → havuz <300ms açılıyor (virtualize)
- Thumbnail 50KB altı

### A.4.3 — Yedek Al
- `Yedekler/db/desadoor_YYYYMMDD_paket_medya.db`

---

# 🪄 KOLON B — AI ASİSTAN ALTYAPI

> **Kapsam:** Bu plan **temel altyapıyı** kurar. Spesifik özellikler (SEO AI, Çeviri AI, Blog yazarı) sonraki iterasyonda.

## B.0 — Hazırlık (yarım gün)

### B.0.1 — DB Yedek Al

### B.0.2 — Klasör İskeleti
```
Desadoor.Ortak/Modeller/AI/
├── AISaglayicisi.cs
├── AICagrisiKaydi.cs
└── Enumlar.cs   (AISaglayiciTipi, AICagriDurumu)

Desadoor.Api/Moduller/AI/
├── Servisler/
│   ├── IAISaglayici.cs           (interface)
│   ├── OpenAISaglayici.cs
│   ├── AnthropicSaglayici.cs
│   ├── GeminiSaglayici.cs
│   ├── AISaglayiciFabrikasi.cs   (DI selector)
│   └── AIMaliyetTakipServisi.cs
├── Dtolar/
├── Komutlar/
└── Kontrolcu/   (AIKontrolcu.cs)

Desadoor.UI/Bilesenler/AI/
├── AIYazButonu.razor       (her metin alanında "✨ AI ile Yaz")
├── AIStreamMetinKutusu.razor  (canlı streaming gösterimi)
└── AIAyarlariSayfasi.razor    (admin → ayarlar → AI)
```

---

## B.1 — Veri Modeli (yarım gün)

### B.1.1 — AISaglayicisi
- `Id, Tip (enum: OpenAI, Anthropic, Gemini), Ad`
- `ApiKeyEncrypted` (ASP.NET DataProtection ile şifreli)
- `Model` (gpt-4o-mini, claude-haiku, gemini-flash vb.)
- `AylikLimitUsd, KullanilanUsd` (counter)
- `SonSifirlamaTarihi` (aylık reset)
- `AktifMi, SiraNo`
- `EkBaslik` (özel header'lar JSON)
- `OlusturulmaTarihi, GuncellenmeTarihi`

### B.1.2 — AICagrisiKaydi (audit + maliyet)
- `Id, SaglayiciId, KullaniciId, KullanimAmaci`
  - Amac örnek: `MetinYaz, MetinDuzelt, SeoUret, Cevir, AciklamaUret, BlogYaz`
- `IstekTokenSayisi, CevapTokenSayisi, ToplamMaliyetUsd`
- `Prompt` (kısaltılmış 500 karakter — audit için)
- `Durum (Basarili/Hata/LimitAsildi)`
- `HataMesaji?`
- `SureMs, OlusturulmaTarihi`

### B.1.3 — SistemAyari Yeni Anahtarlar
- `ai.varsayilan.saglayici` = "OpenAI"
- `ai.varsayilan.model` = "gpt-4o-mini"
- `ai.fallback.saglayici` = "Anthropic"
- `ai.toplam.aylikLimitUsd` = "100"
- `ai.kullanici.gunlukLimitCagri` = "50"
- `ai.streaming.aktif` = "true"

### B.1.4 — Migration
- Adı: `AISaglayicisiVeKayitEklendi`
- 2 tablo eklenir, SistemAyari seed güncellenir

---

## B.2 — Backend (1-1.5 gün)

### B.2.1 — IAISaglayici Interface
```
Task<AIYanit> MetinUretAsync(AIIstek istek, CancellationToken iptal);
IAsyncEnumerable<string> MetinStreamAsync(AIIstek istek, ...);
Task<bool> SaglikTestiAsync();
decimal MaliyetHesapla(int istekToken, int cevapToken);
```

### B.2.2 — Sağlayıcı Implementasyonları
- `OpenAISaglayici` — REST + streaming SSE
- `AnthropicSaglayici` — Claude Messages API
- `GeminiSaglayici` — Google AI Studio API
- Her birinde rate limit, retry (Polly), timeout

### B.2.3 — AISaglayiciFabrikasi
- DI'dan istenen tipi seç
- Varsayılan + fallback yönetimi
- API key DataProtection ile decrypt

### B.2.4 — AIMaliyetTakipServisi
- Her çağrıdan sonra `AICagrisiKaydi` yaz
- Aylık limit kontrolü (limit dolduysa çağrı reddedilir)
- Kullanıcı bazlı günlük limit
- Limit %80 doldu → admin'e uyarı (SignalR toast)

### B.2.5 — Pipeline Middleware
- Her AI çağrısı için:
  - **PII filtresi** (TC kimlik, telefon, email maskele)
  - **Rate limit** (kullanıcı/saat)
  - **Audit log** (her çağrı kaydedilir)
- Anayasa §15 — token/key/prompt loglarda **gizli**

### B.2.6 — AIKontrolcu
- `POST /api/ai/yaz` (body: amac, prompt, baglam) → cevap
- `POST /api/ai/stream` (SignalR hub'a yönlendirir — canlı yazma)
- `GET  /api/ai/saglayicilar` (admin — şifreli key görünmez)
- `POST /api/ai/saglayici` (admin — yeni/güncelle)
- `POST /api/ai/saglayici/{id}/test` (sağlık testi)
- `GET  /api/ai/maliyet` (admin — bu ay kullanım)
- `GET  /api/ai/cagrilar` (admin — audit listesi)

### B.2.7 — SignalR AI Hub
- `AIHub` — streaming cevaplar için
- `MetinStreamBaslat(istek)` → her token geldiğinde client'a push

---

## B.3 — Frontend (1 gün)

### B.3.1 — AIAyarlariSayfasi.razor
**Konum:** `/admin/ayarlar/ai`
- Sağlayıcı listesi (kart görünüm)
- API key giriş (mask: `••••••3a4f`)
- Model dropdown (sağlayıcıdan gelir)
- Aylık limit slider
- "Test Et" butonu (sağlık testi)
- Bu ay kullanım grafiği
- Çağrı geçmişi (son 100)

### B.3.2 — AIYazButonu.razor (her yerde)
- `[Parameter] string Amac` (MetinYaz, SeoUret vs.)
- `[Parameter] EventCallback<string> CevapGeldi`
- `[Parameter] string? Baglam` (mevcut metin)
- Açılır mini menü: Yaz / Düzelt / Kısalt / Uzat / Çevir
- Tıklayınca AI çağrısı + streaming gösterim

### B.3.3 — AIStreamMetinKutusu.razor
- Typewriter efekti (her token geldikçe yazılır)
- "Durdur" butonu (iptal token)
- "Tekrar Üret" butonu

### B.3.4 — Mevcut Form Entegrasyonu (örnek 3 tane)
- KapiModeli formu → Açıklama alanı yanında `AIYazButonu`
- BlogYazisi formu → İçerik
- Firma formu → Aciklama

---

## B.4 — Test ve Doğrulama (yarım gün)

### B.4.1 — Manuel Test
- [ ] OpenAI sağlayıcısı ekle, test et → 200
- [ ] Yanlış key ile → uyarı
- [ ] Aylık limit 1$'a düşür → çağrı reddediliyor
- [ ] AI ile Yaz → streaming canlı geliyor
- [ ] Audit log'da çağrı kaydı var

### B.4.2 — Güvenlik Kontrolü
- [ ] API key DB'de şifreli mi? (DataProtection)
- [ ] Loglarda key/prompt görünmüyor mu? (anayasa §15)
- [ ] PII filtresi çalışıyor mu? (test: "TC 12345..." → maskeleniyor)

### B.4.3 — Yedek Al

---

# 📊 ÖZET — PROGRAM AKIŞI

```
[Paket 1 Migration'ları] ← ÖNCE BU (DUZELT.md — kritik eksik)
        ↓
[Kolon A — Medya Havuzu]  5-7 gün
    A.0 hazırlık → A.1 model → A.2 backend → A.3 UI → A.4 test
        ↓
[Kolon B — AI Altyapı]  3-4 gün
    B.0 hazırlık → B.1 model → B.2 backend → B.3 UI → B.4 test
        ↓
[Sonraki Iterasyon — AI Özelleştirmeleri]
    SEO AI, Çeviri AI, Ürün açıklama AI, Blog AI yazarı, vb.
```

**Toplam tahmini süre:** 8-11 gün (tek geliştirici, full-time)

---

# ⚠ DİKKAT EDİLECEKLER

1. **Migration önce** — Paket 1 migration'ları olmadan bu çalışmaz
2. **Vertical Slice** — modüller `Moduller/Medya/` ve `Moduller/AI/` altında (anayasa §9.4)
3. **JS direkt çağrı YASAK** (anayasa §11) — Cropper/Three.js hep wrapper üzerinden
4. **Türkçe isimlendirme** — `MedyaServisi`, `AISaglayicisi` (anayasa §2)
5. **JsonIgnore** — `Medya.Klasor`, `AISaglayicisi.ApiKeyEncrypted` (anayasa §3.4)
6. **try-catch kontrolcüde YOK** — HataYonetimiMiddleware'e gider (anayasa §7)
7. **API key loglara yazılmaz** (anayasa §15)
8. **Soft delete** — `SilindiMi` (anayasa §8) — fiziksel silme 30 gün sonra
9. **FirmaId** her tabloda var, şimdi nullable (anayasa §4)
10. **Her aşamada DB yedeği** (anayasa §6.1)

---

# 🎬 KARAR NOKTASI

Bu plan iki ayrı uygulama:
- **Şimdi başla A (Medya)** → görsel sonuç hızlı
- **Önce B (AI)** → admin verimlilik daha çabuk artar
- **İkisini paralel** → tek geliştirici için riskli, ayrı insanlar gerek

**Önerim:** Önce A, sonra B. A bittiğinde B'nin AI üretim çıktıları doğal olarak havuza akar.

**Karar bekleniyor:** Hangi kolondan başlayalım? A mı, B mi?

---

*Tarih: 2026-05-14*
*Bağlı dokümanlar: [MIMARI_VIZYON.md](MIMARI_VIZYON.md), [DUZELT.md](DUZELT.md), [KURALLAR.md](KURALLAR.md)*
