# 🔥 DESADOOR — DUDAK UÇUKLATAN SİSTEM VİZYONU

> **Hedef:** Sıradan kurumsal site değil — sektörde **referans gösterilen**, ziyaretçinin "vay be" dediği, adminin "her şeyi görüyorum" hissi yaşadığı endüstriyel platform.
> **Tarih:** 2026-05-14
> **Anayasa:** [KURALLAR.md](KURALLAR.md)
> **Önceki plan:** [DUZELT.md](DUZELT.md) (temel iş paketleri — bu dosya onun üstüne **deneyim katmanı** ekler)

---

## 🎯 FELSEFE

**3 cümlede vizyon:**
1. **Ziyaretçi** siteye girdiği ilk 3 saniyede **"bu marka ciddi"** der.
2. **Admin** panele girdiği an **"her şeyi avucumda tutuyorum"** hisseder.
3. **Müşteri** sohbet botuyla konuşurken **"insan mı yapay zekâ mı?"** diye düşünür.

---

# 🌟 BÖLÜM 1 — ZİYARETÇİ DENEYİMİ (Cinematic Front-End)

## 1.1 — Hero Açılış (İlk 3 Saniye Vurucu)

### Katmanlı Parallax Hero
```
Katman 1 (arka): Bulanık fabrika video loop (kontrast düşürülmüş)
Katman 2 (orta): Bronz parçacık efekti (Three.js particles — yavaş süzülen)
Katman 3 (ön):   Lottie animasyonlu DesaDoor logo
Katman 4 (metin): GSAP SplitText — harf harf yazılan slogan
Katman 5 (CTA):   Magnetic buton (cursor yaklaşınca çekilir)
```

**Slogan animasyonu:**
- "Her Mekana" → 0.0s harf harf belirir
- "Her Yaşama" → 0.4s belirir
- "**Özel Kapılar**" → 0.8s parlayan bronz vurgu (text gradient + glow)

**Scroll göstergesi:** Aşağıda animasyonlu chevron — fare yaklaşınca "kayma" efekti.

### Hero Slider Geçişleri
- **Ken Burns efekti** (her slayt yavaş zoom + pan)
- **Geçiş:** Likit perde (SVG mask animasyon — yukarıdan aşağı dalga)
- Mobilde **dikey kaydırmalı** (TikTok hissi) — her bölüm tam ekran snap

## 1.2 — Scroll Sahneleri (Sinema Gibi)

**Lenis Smooth Scroll** + **GSAP ScrollTrigger** kombinasyonu:

| Bölüm | Efekt |
|-------|-------|
| Kategori Vitrini | **Horizontal scroll** — kategori kartları yana kayar |
| Sayılarla Desadoor | Counter animasyon + arka planda paralaks fabrika resmi |
| Hizmet Süreci | **Pin** — bölüm yapışır, scroll'la 4 adım sırayla aydınlanır |
| Müşteri Yorumları | 3D döner küp veya kart deste karıştırma |
| Referans Şeridi | **Sonsuz marquee** + hover'da renk değişimi |
| Blog Şeridi | Kartlar **3D tilt** (vanilla-tilt) — cursor takip |
| İletişim CTA | **Reveal mask** — arka plan resmi karelerden açılır |

## 1.3 — Kapı Detay Sayfası (FLAGSHIP DENEYİM)

### Sol: 3D Sahne (Sticky)
- **Three.js** + HDR environment map (gerçekçi yansıma)
- **DRACO** sıkıştırılmış .glb (10MB → 800KB)
- **Otomatik döndürme** — kullanıcı dokununca durur
- **Hotspots** — kapı üzerinde tıklanabilir noktalar ("Menteşe detayı", "Kilit", "Yüzey kaplama")
- **Auto-fit zoom** — RAL renk değişince hafif zoom-in efekti

### Sağ: Konfigüratör (Liquid UI)
```
RAL Renk Paleti:      [● ● ● ● ●]  (213 renk — kategori filtreli)
Malzeme:              ○ Membran  ● Lake  ○ Laminant
Genişlik:             [— 800 mm —]  slider
Yükseklik:            [— 2100 mm —]
Yüzey:                ○ Mat ● Yarı Mat ○ Parlak
Donanım:              [çoklu seçim chip'ler]
```
Her değişiklikte **3D model anında güncellenir** (smooth tween — 400ms).

### Aksiyon Çubuğu (Sticky alt)
- **"📸 Anlık Görüntü"** — canvas → PNG indir
- **"📐 PDF Teklif Al"** — QuestPDF ile tek tık (konfig + iletişim formu)
- **"🛒 Sepete Ekle"** — config JSON ile kaydet
- **"🔗 Paylaş"** — kısa link (`/k/abc123` → tam konfig açılır)
- **"📱 AR'da Gör"** — mobilde WebXR (telefonu kapıya tut, gerçek boyutta görsün)

## 1.4 — Mikro-Etkileşimler

- **Cursor**: Bağlamsal değişir — link üstünde büyür, video üstünde "play", 3D üstünde "döndür"
- **Buton hover**: Bronz parıltı geçişi (gradient sweep)
- **Form input**: Floating label + ::after success tick animasyonu
- **Toast bildirimleri**: Glassmorphism + slide-from-top + auto-dismiss progress bar
- **Sayfa geçişi**: GSAP perde efekti (siyah → bronz çizgi → açılma)
- **404**: Animasyonlu "kayıp anahtar" sahnesi + Lottie

## 1.5 — Performans Hedefleri

| Metrik | Hedef |
|--------|-------|
| LCP | < 1.8s |
| FID | < 100ms |
| CLS | < 0.05 |
| Lighthouse Performance | 90+ |
| 3D model yükleme | < 2s (DRACO + lazy) |

**Teknikler:** Image CDN (WebP/AVIF on-the-fly), Output Cache, Brotli compression, Critical CSS inline, Route-based code splitting.

---

# 🦾 BÖLÜM 2 — ADMIN PANELİ (Kaslı, Her Şeyi Bilen)

## 2.1 — Genel Felsefe

Notion + Linear + Vercel Dashboard karışımı.
**3 sütun yapı:** Sol menü (daraltılabilir) | Orta içerik | Sağ aktivite/bildirim akışı (canlı).

## 2.2 — Dashboard (Komuta Merkezi)

### Üst Satır — Canlı KPI Kartları (animasyonlu sayaçlar)
```
┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐
│ 👁 ŞU AN   │ │ 📨 BUGÜN   │ │ 💬 AÇIK    │ │ 📞 BEKLEYEN │
│   ONLINE   │ │  ZIYARET   │ │  SOHBET    │ │  RANDEVU    │
│    47      │ │    1,284   │ │     3      │ │     8       │
│ ▲ %12     │ │ ▲ %23     │ │ • canlı   │ │ ! acil    │
└────────────┘ └────────────┘ └────────────┘ └────────────┘
```

### Orta — Heat Map (Saat × Gün)
Hangi saatte hangi günde en çok ziyaret? **GitHub contribution graph** tarzı renkli grid.

### Sol Aşağı — Canlı Ziyaretçi Akışı
```
🟢 14:32  Anonim - İstanbul/Kadıköy - Chrome/Windows
          📄 /kapi-modelleri/membran-101
          ⏱ 2dk 14sn — 3 sayfa gezdi
          🛤 Geliş: Google "membran kapı fiyat"

🟢 14:31  Ali Demir (kayıtlı) - Ankara - iPhone Safari
          📄 /projeler/villa-bursa
          🛒 sepete 2 ürün ekledi
          
🟡 14:28  Anonim - Bursa - Chrome - 5dk önce ayrıldı
          📄 /iletisim (form yarıda bıraktı ⚠)
```

### Sağ Aşağı — Dünya Haritası
Anlık ziyaretçi noktaları (pulse animasyon) + ülke bazlı ısı haritası (Mapbox GL).

### Alt — Trend Grafikleri (LiveCharts)
- Son 30 gün ziyaret (alan grafik, gradient dolgu)
- En çok görüntülenen 10 ürün (yatay bar)
- Dönüşüm hunisi: Ziyaret → Ürün Görüntüleme → Sepet → İletişim

## 2.3 — Aktivite Akışı (Sağ Panel — Her Sayfada)

Twitter timeline gibi canlı:
```
🟢 ŞIMDI
└ Ayşe (Editör) "Membran 101" ürününü güncelledi
   ↳ Renk seçenekleri: +3 RAL kodu eklendi

⏱ 2dk önce
└ Yeni iletişim mesajı: Mehmet Yılmaz (Ankara)
   ↳ Konu: "Villa için kapı teklifi"
   ↳ [Hızlı Cevapla] [Atla]

⏱ 8dk önce
└ Newsletter aboneliği: ali@example.com (Bursa)
   ↳ Kaynak: /anasayfa footer

⏱ 15dk önce
└ Sistem: Otomatik DB yedeği alındı (4.2 MB)
```

**Filtre:** Tümü | Yönetici işlemleri | Ziyaretçi | Sistem | Hata

## 2.4 — Audit Log Görüntüleyici (Adli Tıp Seviyesi)

Her veri değişikliği kaydedilir + **JSON diff görüntüleyici**:
```
📝 Ali Demir | 14:32 | 192.168.1.5 (Bursa) | Chrome/Windows
   Eylem: KapiModeli.Guncellendi
   ID: 47 | Slug: membran-101
   
   ┌─ ESKİ ────────────┬─ YENİ ─────────────┐
   │ Fiyat: 1500 ₺    │ Fiyat: 1750 ₺     │
   │ Stok: 50         │ Stok: 75          │
   │ Aktif: false     │ Aktif: true       │
   └───────────────────┴────────────────────┘
   
   [Geri Al] [Detay] [İmza Doğrula ✓]
```

**Özellikler:**
- Append-only (silme yok — anayasa §33.3)
- HMAC zincir imza (manipülasyon kanıtı)
- Filtre: kullanıcı, tarih aralığı, eylem türü, IP, etkilenen kayıt
- Export: CSV/PDF (mahkeme delili olarak kullanılabilir)

## 2.5 — Ziyaretçi Analitik (Detaylı Dedektif)

### Oturum Replay
Her ziyaretçinin **mouse hareketi + tıklama + scroll** kaydedilir (KVKK uyumlu — IP maskelenir):
- Video gibi izlenebilir (1x/2x/4x hız)
- Heatmap (en çok tıklanan noktalar)
- "Rage click" tespiti (kullanıcı sinirli mi?)
- Form abandonment (hangi alanda bıraktı?)

### Funnel Analizi
```
Anasayfa (10,000) ────► %42
   ↓
Kapı Modelleri (4,200) ────► %38
   ↓
Detay Sayfa (1,600) ────► %15
   ↓
Sepete Ekle (240) ────► %52
   ↓
İletişim Formu (125) ────► %78
   ↓
Form Gönderildi (98)
```

### Kohort
Hangi hafta gelen kullanıcı kaç kez geri döndü? Retention matrix.

## 2.6 — Hangi Sayfa Tıklandı (Click Tracking)

Her tıklama kaydedilir + **görsel heatmap**:
- Sayfa screenshot üzerine sıcak noktalar
- En çok tıklanan butonlar/linkler sıralaması
- Ölü zonlar (hiç tıklanmayan)
- A/B test desteği (iki versiyon + dönüşüm karşılaştırma)

## 2.7 — Komut Paleti (Ctrl+K) — Süper Güç

```
┌─────────────────────────────────────────┐
│ 🔍 ne aramak istiyorsun?               │
├─────────────────────────────────────────┤
│ 📄 Sayfa: Kapı Modelleri               │
│ 👤 Kullanıcı: ali@example.com          │
│ 📦 Ürün: Membran 101                   │
│ 💬 Mesaj: "villa için teklif" (3 sonuç)│
│ ⚡ Eylem: Yeni Slayt Ekle              │
│ ⚡ Eylem: Yedek Al                     │
│ ⚙ Ayar: SMTP Yapılandırma              │
│ 📊 Rapor: Bu hafta ziyaret              │
└─────────────────────────────────────────┘
↑↓ gez · Enter aç · ⌘+Enter yeni sekme
```

**Klavye:** ↑↓ Enter Esc — tüm aksiyonlar mouse'suz.

## 2.8 — Kısayollar (Power User)

| Tuş | Eylem |
|-----|-------|
| `G` `D` | Dashboard'a git |
| `G` `K` | Kapılar |
| `G` `P` | Projeler |
| `G` `M` | Mesajlar |
| `N` `K` | Yeni Kapı |
| `N` `P` | Yeni Proje |
| `Ctrl+S` | Kaydet |
| `Ctrl+Z` | Geri al (son 10 işlem) |
| `?` | Kısayol cheat sheet |

## 2.9 — Bildirimler (Çok Kanallı)

**Tetik koşulları:**
- Yeni iletişim mesajı → toast + sağ panel + (opsiyonel) Telegram bot
- Form abandoned + iletişim bıraktı → "geri arama önerisi"
- Stok kritik → uyarı
- Sistem hatası → admin'e push (PWA notification)
- Yüksek trafik anomali → "Şu an siteye %300 fazla ziyaretçi var"

**Kanallar:** Tarayıcı bildirim, Email, Telegram bot, PWA push, SignalR canlı toast.

## 2.10 — Inline Edit + Toplu İşlem

- **Çift tıkla düzenle** her DataGrid hücresinde
- **Toplu seç + işlem**: Sil, Pasifleştir, Etiket Ata, Kategori Değiştir, Export
- **Undo toast**: "5 kayıt silindi [Geri Al]" (5sn süre)
- **Drag-drop sıralama**: Slayt, Menü, Kategori, HizmetAdimi

## 2.11 — Multi-Yönetici Real-Time

SignalR ile **canlı varlık**:
- "Ali şu an `Membran 101` kaydını düzenliyor 🟢"
- Aynı kaydı 2 kişi açınca uyarı + kilit
- Yazılan değişiklikler **canlı görünür** (Google Docs benzeri)
- Sohbet eklentisi: admin'ler kendi aralarında not düşebilir

## 2.12 — Dashboard Özelleştirme

- Widget'ları **drag-drop** taşı/yeniden boyutlandır
- Kullanıcı bazlı kaydet
- 3 hazır şablon: "Pazarlama Odaklı", "Operasyon Odaklı", "Yönetici Özeti"

## 2.13 — Tema ve Görsellik

- **3 hazır tema:** Bronz (varsayılan), Mat Siyah, Açık
- **Tasarım dili:** Glassmorphism + bronz aksanlar + ince çizgiler
- **Animasyonlar:** Spring physics (Framer Motion ruhu — MudBlazor + custom CSS)
- **İkonlar:** Lucide veya Phosphor (tutarlı line-icon set)
- **Tipografi:** Inter (UI) + JetBrains Mono (kod/log)

---

## 2.14 — 🪄 AI ASİSTANLARI (Admin İçi — API Tabanlı)

> **Not:** AI **sadece admin tarafında**, içerik üretme + yardım için. Ziyaretçi sohbetinde AI **yoktur**.
> Sağlayıcılar: OpenAI / Claude / Gemini — admin panel'den seçilebilir.

### 2.14.1 — Sağlayıcı Yönetimi (Admin → Ayarlar → AI Sağlayıcıları)

```
┌──────────────────────────────────────┐
│ AI Sağlayıcıları                     │
├──────────────────────────────────────┤
│ ● OpenAI       [API Key: ●●●●●●3a4f] │
│   Model: gpt-4o-mini                 │
│   Aylık limit: 100$  | Kullanılan: 23$│
│   [Test Et] [Kaydet]                 │
│                                      │
│ ○ Anthropic Claude  [yapılandır]    │
│ ○ Google Gemini     [yapılandır]    │
│                                      │
│ Varsayılan: OpenAI                   │
│ Yedek (fallback): Claude             │
└──────────────────────────────────────┘
```

**DB tablosu:** `AISaglayicisi` — Ad, ApiKey (şifreli), Model, AylikLimitUsd, AktifMi, SiraNo, KullanilanUsd, SonKullanim.

**Maliyet izleme:** Her çağrı `AICagrisiKaydi` tablosuna düşer (token, $$ , kullanıcı, kullanım amacı). Limit aşılırsa otomatik durdur.

### 2.14.2 — "Yaz Bana" Butonu (Her Metin Alanında)

Her zengin metin alanının üstünde **✨ AI ile Yaz** butonu:

```
Açıklama
┌──────────────────────────────────────┐
│ ✨ AI ile Yaz  |  Kısalt  |  Uzat   │
│                |  Düzelt  |  Çevir  │
├──────────────────────────────────────┤
│ [metin alanı]                        │
└──────────────────────────────────────┘
```

**Yetenekler:**
- **Yaz:** Boş alana içerik üret ("Membran kapı için ürün açıklaması yaz")
- **Düzelt:** Yazım/dilbilgisi
- **Kısalt / Uzat:** İçerik boyu ayarla
- **Resmilik tonu:** Resmi / Samimi / Pazarlama
- **Stil:** Maddeli / Akıcı / SEO odaklı

**Streaming:** Cevap **gerçek zamanlı yazılır** (typewriter efekti — SignalR ile).

### 2.14.3 — Firma Bilgileri AI Sihirbazı

Admin → Firma → **"✨ AI ile Doldur"** tıklayınca:
- Web sitenizden (varsa) bilgi çeker
- Eksik alanları otomatik doldurur (slogan, hakkımızda, misyon, vizyon)
- SEO meta tags otomatik oluşturur
- Sosyal medya hesaplarını web'den arar önerir
- Çalışma saatleri, harita konumu önerir
- Admin **her öneriyi onaylar/reddeder/düzenler**

### 2.14.4 — SEO AI Asistanı

Her sayfa/ürün için **"SEO Skoru"** + AI önerileri:

```
SEO Skoru: 67/100  🟡
├─ ✅ Başlık uzunluğu uygun (52 karakter)
├─ ⚠ Meta açıklama çok kısa (84 → 150+ olmalı)
│   [✨ AI ile Genişlet]
├─ ❌ Alt etiketler eksik (3 resim)
│   [✨ Tümüne AI ile alt yaz]
├─ ⚠ Anahtar kelime yoğunluğu düşük
│   [✨ Öneriler]
└─ ✅ Slug temiz
```

**Toplu SEO operasyonu:** "150 ürünün tümüne meta açıklama üret" → arka planda iş kuyruğu + ilerleme bar.

### 2.14.5 — Dil/Çeviri AI Asistanı

Çeviri yönetim sayfası:
```
Anahtar              | TR              | EN  | AR
─────────────────────┼─────────────────┼─────┼─────
anasayfa.hero.baslik | Her mekana...   | —   | —
anasayfa.hero.alt    | 1992'den...     | —   | —
                     [✨ Tüm eksikleri AI ile çevir]
```

**Özellikler:**
- DeepL benzeri kaliteli çeviri (API üzerinden)
- Bağlam farkındalığı: anahtar adı (örn `urun.fiyat`) çeviriye ipucu verir
- **Marka sözlüğü**: "DesaDoor", "Membran" gibi terimler aynen kalır
- Toplu çeviri: 200 anahtarı tek seferde
- Çeviri belleği: aynı metin tekrar çevrilmez

### 2.14.6 — Ürün Açıklaması AI Üretici

Kapı/Mobilya ekleme formunda:
1. **Resim yükle** → AI resmi inceler ("Bu bir membran iç kapı, koyu ceviz desenli")
2. **"✨ Açıklama Üret"** → 3 farklı uzunlukta seçenek (kısa/orta/uzun)
3. **Teknik özellik tablosu** otomatik (malzeme, kalınlık, izolasyon önerileri)
4. **Kullanım alanı önerileri** (iç kapı / banyo / yatak odası)
5. **Etiketler** otomatik

### 2.14.7 — Blog AI Yazarı

- Konu başlığı + anahtar kelimeler ver → tam blog yazısı üretir
- Önce **outline** (içindekiler) → admin onaylar → sonra tam metin
- Görsel önerileri (Unsplash API entegre — telifsiz)
- SEO meta + kapak resmi alt + sosyal medya post taslakları

### 2.14.8 — İletişim Mesajı AI Cevap Önerisi

Gelen iletişim formu mesajına **admin cevap yazarken**:
- AI **3 cevap taslağı** önerir
- Müşteri sorusuna göre uygun ton (resmi/samimi)
- Önceki yazışmaları bağlam olarak kullanır
- Admin **mutlaka onaylar/düzenler** — direkt gönderim YOK

### 2.14.9 — Görsel AI (Opsiyonel)

- **Alt metin üret** (resim → "Modern koyu ceviz membran iç kapı, paslanmaz çelik kulplu")
- **Otomatik etiketle** (kategori önerisi resimden)
- **Arka plan kaldır** (rembg/Cloudinary API)
- **Renk çıkar** (resimden ana renkleri al → tema önerisi)

### 2.14.10 — Komut Paleti AI Modu

Ctrl+K içinde `?` ile başla:
```
? bana son 7 günde en çok ziyaret edilen 5 ürünü göster
? membran kategorisindeki tüm ürünlere %10 zam yap
? bu hafta yeni iletişim mesajlarını özetle
```
→ AI komutu **eylem**a çevirir (SQL/MediatR komutu) + admin **onaylar** sonra yürütülür (DESTRUCTIVE eylemler için her zaman onay).

### 2.14.11 — Güvenlik ve Sınırlar

- **API key şifreli** DB'de (ASP.NET DataProtection)
- **Rate limit** (kullanıcı başına saatte X çağrı)
- **Kayıtlı prompt geçmişi** (audit log — kim ne sordu)
- **PII filtresi** — kişisel veri AI'ye gönderilmez (otomatik maskeleme)
- **Çıktı moderasyonu** — uygunsuz içerik üretilirse engellenir
- **Hiç yapılmaz:** Otomatik yayın. AI üretir → **insan mutlaka onaylar** → yayınlanır.

---

## 2.15 — 📡 CANLI DİNAMİK SİSTEM (Her Şey Real-Time)

**SignalR + reactive backend** ile her şey **anlık**:

### Canlı Akışlar
- 🟢 **Ziyaretçi sayısı** — her panelin üstünde sabit, gerçek zamanlı
- 🟢 **Yeni mesaj** geldi → toast + ses + sayaç +1
- 🟢 **Yeni form** dolduruldu → dashboard kartı animasyonlu güncellenir
- 🟢 **Stok değişimi** → diğer admin'lerin ekranı eş zamanlı yenilenir
- 🟢 **Audit log** akışı → sağ panel sürekli düşer

### Hot Reload Ayarlar
Admin → Ayarlar değiştirince:
- **Sayfa yenilemeye gerek YOK**
- Tüm açık admin oturumlarına SignalR broadcast → ayar canlı uygulanır
- Ziyaretçi tarafına yansır (örn. yeni slayt eklendi → 30sn içinde anasayfa otomatik yenilenir)

### Otomatik Yenileme (Polling YOK)
- Liste sayfaları SignalR ile **push** alır
- Yeni kayıt → tablonun üstüne **kayma efektiyle** eklenir (vurgulu — 2sn yeşil glow)
- Silme → kayıt fade-out
- Düzenleme → satır kısa süre vurgu (sarı glow)

### Canlı İstatistikler
- Counter widget'lar **akışkan** (sayı yumuşak geçer, sıçramaz)
- Grafikler **akar** (yeni veri sağdan girer, eski sol kayar)
- Heatmap noktaları **anlık** parlar

---

## 2.16 — 🖼 MEDYA HAVUZU (Resim/Video Kütüphanesi — Merkezi)

> **Felsefe:** Her yerde her şeye resim/video eklenir. Hepsi **tek havuzdan** beslenir.
> Bir kez yükle → her yerde kullan. Sil/güncelle → her yerden senkron.

### 2.16.1 — DB Şeması

**Medya tablosu:**
```
Id, FirmaId, Tip (Resim/Video/Pdf/Glb),
Ad, OrijinalAd, DosyaYolu, MiniaturYolu,
Boyut (bytes), Genislik, Yukseklik, Sure (saniye — video),
MimeTipi, Hash (SHA256 — duplicate tespiti),
KaynakTip (Yerel/Youtube/Vimeo/Url),
KaynakUrl (YouTube ise embed URL),
AltMetin, Aciklama, Etiketler (JSON),
KlasorId (opsiyonel — albüm/klasör),
KullanimSayisi (kaç yerde referans var — silmeden önce uyar),
YukleyenKullaniciId, OlusturulmaTarihi, GuncellenmeTarihi
```

**MedyaKullanim tablosu (referans takip):**
```
Id, MedyaId, EntiteAdi (KapiModeli/Proje/Slayt vb.),
EntiteId, AlanAdi (KapakResim/GaleriResim/Avatar),
SiraNo, OlusturulmaTarihi
```

Bu sayede: "Bu resmi sileceksin ama 3 üründe ve 1 projede kullanılıyor — emin misin?"

**MedyaKlasoru:**
```
Id, FirmaId, ParentId (ağaç), Ad, Slug,
Ikon, Renk, SiraNo, OlusturulmaTarihi
```

### 2.16.2 — Havuz Arayüzü (Notion/Figma Tarzı)

```
┌─ KLASÖRLER ──┬────────────────────────────────────┐
│ 📁 Tümü (842)│ 🔍 ara...  🏷 etiket  📅 tarih   │
│ 📁 Kapılar   │                                    │
│  └ Membran   │ ┌──┐ ┌──┐ ┌──┐ ┌──┐ ┌──┐ ┌──┐ │
│  └ Lake      │ │🖼│ │🖼│ │🎬│ │🖼│ │🎬│ │🖼│ │
│  └ Laminant  │ └──┘ └──┘ └──┘ └──┘ └──┘ └──┘ │
│ 📁 Mobilyalar│  3  ▲   YT   1     YT   2          │
│ 📁 Projeler  │                                    │
│ 📁 Slayt     │ [+ Yükle] [+ YouTube] [+ URL]      │
│ 📁 Sertifika │ [📁 Yeni klasör] [✨ AI etiketle] │
│ 📁 Logolar   │                                    │
│ + Klasör Ekle│ Seçili: 3 dosya                   │
│              │ [Taşı] [Etiketle] [Sil] [İndir]   │
└──────────────┴────────────────────────────────────┘
```

**Özellikler:**
- **Drag-drop yükleme** (klasör üzerine bırak)
- **Çoklu seçim** (Ctrl/Shift + click, lasso seçim)
- **Önizleme paneli** (sağ tıkla → büyük önizleme)
- **Detay paneli** (alt metin, etiket, kullanım listesi)
- **Hash duplicate tespiti** — aynı dosya 2. kez yüklenirse uyarı + mevcut'a yönlendir
- **Lazy load** (virtualize — 10.000+ resim olsa bile akıcı)
- **Etiket bulutu** — filtreleme için

### 2.16.3 — Yükleme Kanalları

#### 1) Yerel Yükleme
- Drag-drop / dosya seç / yapıştır (clipboard'dan resim — Ctrl+V)
- **Toplu** (ZIP yükle → otomatik açılır)
- **İlerleme bar** + paralel yükleme
- **Otomatik işleme:** WebP/AVIF dönüşüm, küçük resim üret, EXIF temizle (gizlilik)
- **Boyut kontrolü:** Max 20MB resim, 500MB video (ayarlardan)

#### 2) YouTube
```
🎬 YouTube'dan Ekle
┌──────────────────────────────────────┐
│ https://youtube.com/watch?v=xxx     │
│ [Ekle]                              │
└──────────────────────────────────────┘
✓ Başlık otomatik: "DesaDoor Fabrika Turu"
✓ Süre: 3:42
✓ Kapak resmi otomatik (maxresdefault.jpg)
✓ Embed URL kaydedildi
```
- Sadece **referans** kaydedilir (videoyu indirmeyiz — YouTube oynatır)
- Kapak resmi havuza otomatik kopyalanır
- Vimeo / Dailymotion da aynı mantık

#### 3) URL'den
- Web'deki bir resmi URL ile çek (sunucu side fetch → havuza kopyalar)
- Telif uyarısı gösterilir

#### 4) AI Üretim (Opsiyonel — Bölüm 2.14 ile bağlı)
- Stable Diffusion / DALL-E API → prompt yaz → resim üret → havuza düşer
- Stok fotoğraf entegre (Unsplash/Pexels API — telifsiz)

### 2.16.4 — Resim Seçici (Her Yerde Aynı Komponent)

Form alanına resim eklerken **MedyaSecici.razor** açılır:

```
┌─ Resim Seç ─────────────────────────────┐
│ [Havuzdan] [Yeni Yükle] [YouTube] [URL] │
├─────────────────────────────────────────┤
│ 🔍 ara...                               │
│ Klasör: [Kapılar/Membran ▼]            │
│                                         │
│ ┌──┐ ┌──┐ ┌──┐ ┌──┐ ┌──┐ ┌──┐         │
│ │🖼│ │🖼│✓│🖼│ │🖼│ │🖼│ │🖼│        │
│ └──┘ └──┘ └──┘ └──┘ └──┘ └──┘         │
│                                         │
│ Seçili: kapi-membran-101.webp           │
│ [İptal] [Kullan]                        │
└─────────────────────────────────────────┘
```

- **Çoklu seçim** (galeri için sıralı)
- **Hızlı kırpma** (seçince crop modal — sıraka, kare, geniş ekran preset)
- **AI alt metin** ✨ butonu (görmeyenler için)

### 2.16.5 — Düzenleme (Görsel Editor — Inline)

Havuzdaki resme tıklayınca veya formda **✏ Düzenle**:
- Kırp / Döndür / Çevir
- Parlaklık / Kontrast / Doygunluk
- Filtre (siyah-beyaz, sepia, vintage)
- Yazı ekle (logo/watermark)
- Sıkıştırma kalitesi
- **Yeni sürüm kaydet** veya **üzerine yaz**

Tüm kullanım yerleri **otomatik güncellenir** (referans sistemi sayesinde).

### 2.16.6 — Silme Mantığı

Sil tıklayınca:
```
⚠ "kapi-membran-101.webp" silinecek

Bu medya 4 yerde kullanılıyor:
  • KapiModeli #47 (KapakResim)
  • KapiModeli #51 (GaleriResim)
  • Slayt #3 (Arkaplan)
  • Blog #12 (KapakResim)

[ ] Kullanılan yerlerden de kaldır
[ ] Sadece havuzdan kaldır (referanslar kırılır)
[İptal]  [Sil]
```

- **Soft delete** (anayasa §8) — `SilindiMi` true olur, 30 gün çöp kutuda
- **Çöp kutusu** klasörü (30 gün sonra fiziksel sil)
- **Geri al** — silinen dosyayı çöp kutusundan geri çağır

### 2.16.7 — Storage Adaptörü (Ayarlanabilir)

Admin → Ayarlar → Depolama:
```
Depolama Sağlayıcı:
  ● Yerel disk        (wwwroot/medya/)
  ○ MinIO (S3)        [yapılandır]
  ○ Cloudflare R2     [yapılandır]
  ○ AWS S3            [yapılandır]
  ○ Azure Blob        [yapılandır]
```
**Provider değişince** mevcut dosyalar arka planda **migrate edilir** (iş kuyruğu).

### 2.16.8 — CDN ve On-the-fly İşlem

**ImageSharp.Web** ile URL parametreleri:
```
/medya/kapi-101.jpg              → orijinal
/medya/kapi-101.jpg?w=400        → 400px genişlik
/medya/kapi-101.jpg?w=400&q=80   → kalite
/medya/kapi-101.jpg?fmt=webp     → format dönüşüm
/medya/kapi-101.jpg?w=400&fit=crop&pos=center
```
Cache'lenir + CDN-friendly. Mobil otomatik küçük versiyon.

---

## 2.17 — ⚙ AYARLAR SİSTEMİ (Her Şey Yapılandırılabilir)

> **Felsefe:** Hardcoded hiçbir şey yok. Her şey **DB'de SistemAyari** kaydı.
> Admin değiştirir → SignalR hot-reload → sayfa yenilemeden uygulanır.

### SistemAyari Tablosu
```
Id, Anahtar, Deger, Tip (string/int/bool/json),
Bolum (Genel/SEO/Sosyal/Modul/Limit/Tema/AI/Medya),
AciklamaTr, AciklamaEn, GorunurMu,
DegisimTarihi, DegistirenId
```

### Örnek Anahtarlar
```
modul.blog.aktif          = true
modul.sohbet.aktif        = true
modul.3dgoruntu.aktif     = true
modul.cokludil.aktif      = true
modul.eticaret.aktif      = false
limit.medya.maxBoyutMb    = 20
limit.medya.maxVideoMb    = 500
ai.saglayici              = openai
ai.aylikLimitUsd          = 100
gorunum.tema              = bronz
gorunum.font              = inter
seo.varsayilanBaslik      = "DesaDoor — ..."
iletisim.email            = info@desadoor.com.tr
sosyal.instagram          = @desadoor
yedek.otomatikGunde       = 1
medya.depolama            = yerel   (yerel/minio/s3/r2)
```

### Ayar UI
- Kategorilere ayrılmış sekme (Genel/SEO/Sosyal/Modüller/Limit/Tema/AI/Medya/Yedek)
- Her ayarın yanında `?` tooltip (AciklamaTr)
- Değiştirince **canlı uygulanır** (SignalR hot-reload — bkz. 2.15)
- **Geri al** — ayar değişiklik geçmişi (audit log entegre)
- **Export/Import** — JSON ile yedek/geri yükleme

### Modül Aktivasyon (Anayasa §13)
- Her modülün `aktif` ayarı var
- Kapalıyken hem admin menüsünde, hem ziyaretçi tarafında **görünmez**
- Tek tık aç/kapa — kod değişmez

### FirmaId Altyapısı (İlerisi İçin Hazır — Şu An Kullanılmıyor)
- Anayasa §4 gereği tüm tablolarda `FirmaId` (nullable) alanı var
- Şu an tek firma (DesaDoor) — varsayılan değer ile çalışır
- İleride ihtiyaç olursa multi-tenant açılır — şimdi SaaS gerekmiyor

---

# 💬 BÖLÜM 3 — CANLI DESTEK SOHBETİ (İnsan Temsilci — Bot AI YOK)

> **Not:** Ziyaretçi sohbetinde AI/bot **kullanılmaz**. Tüm sohbetler **gerçek temsilci** tarafından yanıtlanır.
> AI sadece **admin** tarafında içerik üretmek için (bkz. Bölüm 2.14).

## 3.1 — Mimari (Saf SignalR)

```
Ziyaretçi mesaj atar
      ↓
[SignalR Hub — SohbetHub]
      ↓
[Sohbet havuzuna düşer]
      ↓
[Müsait temsilci panelinde toast + ses]
      ↓
Temsilci alır → karşılıklı yazışma
```

Temsilci yoksa:
- Mesai dışı şablonu: "Şu an mesai dışındayız. Mesajınızı bırakın, en geç X saatte döneriz."
- Otomatik form: Ad + Telefon + Konu → IletisimMesaji tablosuna kayıt → admin'e mail.

## 3.2 — Ziyaretçi Tarafı

**Açılış:** Sağ alttan zıplayarak çıkan balon + "Merhaba 👋 Size nasıl yardımcı olabiliriz?" preview.

**Açık hali:** Glassmorphism panel + bronz aksanlar.
- Ziyaretçi mesajı: sağda, bronz balon
- Temsilci mesajı: solda, koyu cam balon + temsilci avatarı + isim ("Ayşe — DesaDoor")
- Typing indicator (gerçek — temsilci yazarken canlı tetiklenir)
- Okundu işareti (çift tik)

**Pratikleştirme — AI değil, hazır şablonlar:**
- **Quick replies (chip butonlar)** ziyaretçiye: "Fiyat sormak istiyorum", "Randevu", "Katalog indir", "Adres/yol tarifi"
- Bu butonlar **statik** — tıklayınca öntanımlı metin gönderilir veya direkt aksiyon (katalog PDF linki) çalışır
- Müsaitlik durumu: yeşil/kırmızı nokta + "X dakika içinde dönüyoruz"

**Zenginlik (AI'sız):**
- Ürün kartı paylaşımı (temsilci paneldeki "ürün gönder" butonuyla)
- Dosya/resim yükleme (ziyaretçi → temsilci, temsilci → ziyaretçi)
- PDF katalog gönderme
- Randevu linki ("Şu saatlerde müsaitiz: ...")

## 3.3 — Temsilci Paneli (Admin İçinde Inbox)

```
┌──────────────┬─────────────────────┬──────────────┐
│ AKTİF SOHBET │ MESAJLAŞMA          │ ZİYARETÇİ    │
│              │                     │ PROFİL       │
│ 🟢 Mehmet (2)│ Mehmet: Membran     │              │
│ 🟡 Anonim    │   kapı fiyat?       │ Ad: Mehmet   │
│ ⚪ Ayşe (5)  │                     │ Şehir: Bursa │
│              │ Ali: Merhaba,       │ Geldiği:     │
│              │   ölçü nedir?       │ /membran-101 │
│              │                     │              │
│              │ [Yaz...........] [▶]│ GEÇMİŞ       │
│              │ 📎 📦 📅 ⭐         │ 3 mesaj      │
└──────────────┴─────────────────────┴──────────────┘
```

**Özellikler:**
- **Hazır cevap şablonları** (kategorize: Selamlama, Fiyat, Randevu, Veda) — Ctrl+1..9 ile hızlı ekle
- **Notlar** (sadece temsilciler görür — ziyaretçi göremez)
- **Etiket** (Potansiyel Satış / Şikayet / Bilgi / Spam)
- **Atama** — "Bu sohbet Ali'ye atandı"
- **Ürün gönder butonu** — modal'dan ürün seç → kart olarak yollanır
- **Geçmiş sohbet** — aynı IP/email daha önce yazdıysa görünür
- **Ses bildirim** + masaüstü notification yeni mesajda
- **Devir** — temsilci başka temsilciye sohbeti devredebilir

## 3.4 — Mesai ve Otomatik Yanıtlar (Statik, AI Değil)

**Yönetilebilir mesai takvimi** (admin → ayarlar):
- Pzt-Cum 09:00-18:00 açık
- Cumartesi 09:00-13:00
- Pazar kapalı

**Mesai dışı / temsilci yokken:**
- Şablon mesaj otomatik gönderilir (sabit metin — admin düzenler)
- "İletişim formu" şeklinde alanlar açılır
- Form gönderildiğinde IletisimMesaji tablosuna düşer + admin'e mail

**Yoğunlukta:**
- Sıraya alma: "X. sıradasınız, ortalama bekleme süresi Y dakika"

## 3.5 — Analitik

- Ortalama yanıt süresi (temsilci bazlı)
- Sohbet sayısı / gün
- Memnuniyet anketi (sohbet sonunda 1-5 yıldız + opsiyonel yorum)
- En çok sorulan konular (etiket bazlı raporlama)

---

# 🏗 BÖLÜM 4 — TEKNİK MİMARİ (Temiz, Modüler, Ölçeklenebilir)

## 4.1 — Vertical Slice (Modül Bazlı)

```
Desadoor.Api/Moduller/
├── Kapilar/
│   ├── Komutlar/      KapiOlusturKomutu.cs, KapiOlusturIsleyici.cs
│   ├── Sorgular/      KapiListeleSorgusu.cs, KapiDetaySorgusu.cs
│   ├── Dtolar/        KapiDto.cs, KapiOzetDto.cs
│   ├── Dogrulayicilar/ KapiOlusturDogrulayici.cs
│   ├── Profil/        KapiMapsterProfil.cs
│   └── Kontrolcu/     KapiKontrolcu.cs (3 satırlık, mediator.Send())
├── Mobilyalar/  (aynı yapı)
├── Projeler/
├── Pazarlama/   (Slayt, Referans, Yorum, HizmetAdimi)
├── Kurumsal/    (Firma, Sube, Ekip, Sertifika)
├── Iletisim/    (Mesaj, Bulten, Sohbet)
├── Kimlik/      (Giris, Kayit, Rol, Yetki)
└── Sistem/      (Audit, Lisans, Ceviri, Yedekleme, Tema, Ayar)
```

**Avantaj:** Bir özellik = bir klasör. Ekip paralel çalışır. Test kolay.

## 4.2 — CQRS + MediatR Pipeline

Her istek otomatik geçer:
```
İstek → [Validation] → [Authorization] → [Logging] → [Cache] → [Handler] → [Audit] → Yanıt
```

Kontrolcü:
```csharp
[HttpPost]
public async Task<Cevap<KapiDto>> Olustur(KapiOlusturKomutu komut)
    => await _mediator.Send(komut);
```

## 4.3 — Cache Stratejisi

- **L1 (Memory)** — sık erişilen (çeviri, ayar, kategori liste)
- **L2 (Redis)** — paylaşımlı (oturum, output cache)
- **FusionCache** her ikisini yönetir + stampede protection + jitter

## 4.4 — Görsel Sunucu

**ImageSharp.Web** — on-the-fly:
```
/img/kapi-101.jpg?w=400&q=80&format=webp
```
Cache + CDN-friendly. Mobilde otomatik küçük versiyon.

## 4.5 — Observability

| Araç | Görev |
|------|-------|
| **Serilog** | Yapısal log (JSON) |
| **Seq** | Log görselleştirme + arama |
| **OpenTelemetry** | Trace + metric (gelecek için) |
| **Health Checks** | `/api/health` (DB, Redis, disk, SMTP) |
| **Sentry** (ops) | Frontend hata izleme |

## 4.6 — Güvenlik Katmanları

1. **WAF** (Cloudflare önünde)
2. **Rate limiting** (per IP + per user)
3. **CSP + güvenlik header'ları** (zaten kurulu)
4. **JWT + Refresh token rotation**
5. **2FA** (TOTP — Google Authenticator)
6. **Lisans + domain kilit** (HMAC)
7. **Audit log** (append-only + HMAC zincir)
8. **Honeypot** form alanları (bot tuzakları)
9. **CAPTCHA v3** (görünmez, skor bazlı)

---

# 📋 BÖLÜM 5 — UYGULAMA YOL HARİTASI

DUZELT.md'deki 8 paketin üstüne **deneyim katmanı**:

## Faz A — Temel (DUZELT.md Paket 0-2 zaten planlanmış)
- [x] Anayasa + görev dosyaları
- [ ] **Migration'lar oluştur** (kritik eksik!)
- [ ] Vertical Slice klasör yapısına geçiş
- [ ] MediatR + FluentValidation + Serilog kurulumu

## Faz B — Görsel Şahane Front-End (DUZELT.md Paket 3 + bu vizyon)
- [ ] Lenis + GSAP ScrollTrigger + Lottie
- [ ] Cinematic Hero (5 katmanlı parallax)
- [ ] Tüm scroll sahneleri
- [ ] KapakDetay flagship deneyim
- [ ] Mikro etkileşimler

## Faz C — Kaslı Admin Panel
- [ ] Dashboard komuta merkezi (KPI + heatmap + dünya haritası)
- [ ] Canlı ziyaretçi akışı (SignalR)
- [ ] Audit log adli tıp arayüzü
- [ ] Oturum replay + heatmap
- [ ] Komut paleti Ctrl+K
- [ ] Klavye kısayolları
- [ ] Real-time presence (multi-admin)

## Faz C+ — Admin AI Asistanları (API Tabanlı)
- [ ] AISaglayicisi tablosu + şifreli API key saklama
- [ ] AICagrisiKaydi (maliyet izleme + limit)
- [ ] "✨ AI ile Yaz" buton komponenti (her metin alanına)
- [ ] Streaming cevap (SignalR + typewriter)
- [ ] Firma AI sihirbazı
- [ ] SEO AI asistanı (skor + öneri + toplu)
- [ ] Çeviri AI (toplu + marka sözlüğü)
- [ ] Ürün açıklaması AI (resim→metin)
- [ ] Blog AI yazarı
- [ ] İletişim cevap önerisi
- [ ] Komut paleti `?` AI modu

## Faz C++ — Canlı Dinamik Sistem
- [ ] SignalR hub'ları (Ziyaretci, Audit, Liste güncellemeleri)
- [ ] Reactive tablo komponenti (push güncelleme + animasyon)
- [ ] Hot-reload ayarlar (broadcast)
- [ ] Akışkan counter + canlı grafik

## Faz D — Canlı Destek (İnsan Temsilci — Bot AI YOK)
- [ ] SignalR SohbetHub geliştirme (mevcut, zenginleştir)
- [ ] Ziyaretçi sohbet UI (glassmorphism + quick reply chip'ler)
- [ ] Temsilci inbox paneli (3 sütun)
- [ ] Hazır cevap şablonları + Ctrl+1..9 kısayollar
- [ ] Mesai takvimi + mesai dışı şablonu
- [ ] Sıraya alma + atama + devir
- [ ] Memnuniyet anketi + analitik

## Faz E — 3D + Konfigüratör (DUZELT.md Paket 5 + zenginleştirme)
- [ ] DRACO loader + HDR environment
- [ ] Hotspot sistemi
- [ ] AR (WebXR mobil)
- [ ] PDF teklif (QuestPDF)
- [ ] Paylaşılabilir konfig link

## Faz F — Analitik + İzleme
- [ ] Click tracking + heatmap
- [ ] Funnel + cohort analiz
- [ ] Form abandonment tespiti
- [ ] A/B test altyapısı

## Faz G — Sertleştirme (DUZELT.md Paket 7)
- [ ] Test suite genişletme
- [ ] WAF + 2FA
- [ ] Performans optimizasyon (Lighthouse 90+)

---

# 🎬 SONUÇ

Bu dokuman **DUZELT.md'yi tamamlayan deneyim katmanıdır**.
- DUZELT.md → "**Ne** yapılacak" (tablolar, endpoint'ler, sayfalar)
- MIMARI_VIZYON.md → "**Nasıl şahane** yapılacak" (efekt, animasyon, his)

**İlk somut adım önerisi:**
1. DUZELT.md Paket 1 migration'larını çalıştır (DB temel)
2. Vertical Slice klasör yapısına geç (Paket 2 yerine bu)
3. Faz B Hero deneyimi prototipi (görünür sonuç → motivasyon)

Sonra ister Faz C admin, ister Faz D sohbet — paralel ekip varsa aynı anda.

---

*Tarih: 2026-05-14*
*Anayasa uyumlu: [KURALLAR.md](KURALLAR.md)*
*Tamamlayıcı: [DUZELT.md](DUZELT.md)*
