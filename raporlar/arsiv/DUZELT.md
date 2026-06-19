# 🛠️ DESADOOR DÜZELT.md — SİSTEM DÜZELTME VE TAMAMLAMA TAKİP DOSYASI

> **Hazırlayan (Araştırma):** Claude Haiku 4.5
> **Uygulanacak:** Başka model (Opus / Sonnet) veya geliştirici
> **Proje:** I:\desedoorweb (DesaDoor — Kapı/Mobilya Kurumsal Site)
> **Anayasa:** I:\desedoorweb\KURALLAR.md (uyulacak)
> **Referans Site:** https://www.desadoor.com.tr/
> **Tarih:** 2026-05-14

---

## 📋 NASIL KULLANILIR

1. Her ana başlık (▶) bir **iş paketidir**
2. Alt maddeler (☐) tamamlandıkça **[x]** yapılır
3. Her iş paketi sonunda **DOĞRULAMA** maddesi çalıştırılır → geçerse → bir sonraki pakete geçilir
4. Anayasa §0.4 gereği **parçalı çalışma zorunlu** — bir paket bitmeden diğerine geçilmez
5. Her iş paketi başında **DB yedeği** alınır (anayasa §6.1)

---

## 🎯 MEVCUT DURUM ÖZETİ (Tespit Edilen)

### Çalışıyor (✅)
- .NET 10 + Blazor WASM altyapı kurulu
- MudBlazor 9.4 + Extensions kurulu (anayasa §12.1 uyumlu)
- tokens.css yapısı (temeller/bilesenler/moduller) var (§12.3 uyumlu)
- Three.js 3D motor scripti var (§28 hazır)
- AOS + GSAP animasyon kütüphaneleri kurulu
- API port **5015**, UI port **5013** çalışıyor
- SQLite desadoor.db dosyası var
- 7 temel tablo migration ile oluşturulmuş
- Admin paneli 14 sayfa iskeleti var (Pages/Admin/)
- SignalR SohbetHub kurulu
- JWT BCrypt auth altyapısı var

### Eksik / Hatalı (❌)
- KURALLAR.md, GOREV dosyaları YOK (anayasa §0 ihlal)
- `.agent/` klasörü YOK (§0.1 ihlal)
- 18+ kritik tablo eksik (içerik tabloları)
- Kontrolcüler düz klasörde — modüler değil (§9.4 ihlal)
- Razor sayfalarında hardcoded Türkçe metinler (§25 ihlal)
- i18n için JSON dosya kullanılıyor — DB+FusionCache olmalı (§35 ihlal)
- HataYonetimiMiddleware yok (§7 ihlal)
- Cevap<T> standardı uygulanmamış (§7.3)
- FluentValidation kurulu değil (§23.6)
- Lisans + Domain kilidi yok (§5)
- Audit Log yok (§33.3)
- Serilog yapılandırılmamış (§15)
- xUnit test projesi yok (§6.2)
- Yedekler/ klasörü yok (§6.1)
- 3D model dosyaları (wwwroot/models/) BOŞ
- Ürün/hizmet görselleri yok (sadece 1 fabrika fotoğrafı var)

---

# ▶ İŞ PAKETİ 0 — ANAYASA VE GÖREV SİSTEMİ KURULUMU
> **Tahmini süre:** 1 gün | **Öncelik:** KRİTİK | **Anayasa:** §0, §6.1, §18

## Yapılacaklar

### ☐ 0.1 — KURALLAR.md oluştur
- **Konum:** `I:\desedoorweb\KURALLAR.md`
- **İçerik:** Ustam'ın verdiği orijinal vizitlink anayasasını **DesaDoor için adapte et**:
  - "Vizitlink" → "DesaDoor" (uygun yerlerde)
  - Port 5005/5003 → **5015/5013** (DesaDoor portları)
  - Multi-tenant başlangıçta opsiyonel (DesaDoor tek firma ama altyapı SaaS-ready kalsın)
  - SQLite üretim → SQLite geliştirme + PostgreSQL üretim (mevcut yapıyı koru)
  - "Ustam Ahmet" hitabını koru
- **Önemli:** Anayasanın "DOSYA ASLA KISALTILAMAZ" kuralı (§0 sonu) — tam orijinal halini al, sadece DesaDoor adaptasyon notu olarak başa ekle:
  ```
  > **DesaDoor Adaptasyon Notu:** Bu anayasa Vizitlink SaaS için yazılmıştır.
  > DesaDoor projesi bu anayasanın kapı/mobilya kurumsal site adaptasyonudur.
  > Multi-tenant başlangıçta tek firma (DesaDoor) için çalışır ama altyapı SaaS-ready.
  ```
- **Doğrulama:** Dosya I:\desedoorweb kökünde, satır sayısı 2500+ olmalı

### ☐ 0.2 — GOREV_1_YAPILDI.md oluştur
- **Konum:** `I:\desedoorweb\GOREV_1_YAPILDI.md`
- **Format:**
  ```markdown
  # DesaDoor — Tamamlanan Görevler
  
  ## 2026-05-XX
  - [x] Anayasa (KURALLAR.md) kuruldu
  - [x] Görev takip dosyaları oluşturuldu
  ```
- **Doğrulama:** Dosya kökte var

### ☐ 0.3 — GOREV_2_YAPILACAK.md oluştur
- **Konum:** `I:\desedoorweb\GOREV_2_YAPILACAK.md`
- **İçerik:** Bu DUZELT.md'deki tüm iş paketlerini özet liste halinde aktar
- **Doğrulama:** Dosya kökte var, tüm fazlar listede

### ☐ 0.4 — .agent klasörü ve AI Kilit dosyaları
- **Klasör:** `I:\desedoorweb\.agent\`
- **Dosyalar:**
  - `AI_ANAYASA_KILIDI.md` — Anayasa §0 dahil katı yasaklar
  - `AI_KOD_YAZMA_KONTROL.md` — Kod yazmadan önce checklist
- **AI_ANAYASA_KILIDI.md içeriği (minimum):**
  ```markdown
  # AI ANAYASA KİLİDİ
  
  ## KIRMIZI ÇİZGİLER
  1. Python (*.py) veya dış terminal botları YASAK
  2. KURALLAR.md okunmadan kod yazılamaz
  3. MudBlazor dışında UI kütüphanesi YASAK
  4. Hardcoded Türkçe metin Razor'da YASAK (DilServisi.T kullan)
  5. Try-catch kontrolcüde YASAK (HataYonetimiMiddleware)
  6. Veritabanı sorgusunda Türkçe karakter sütun adı YASAK (Ş→S, İ→I)
  7. Sadece EF Core Code-First Migration ile DB değişikliği
  
  ## ZORUNLU
  - Tüm değişken/sınıf/dosya Türkçe (framework istisnası hariç)
  - Cevap<T> dönüş standardı
  - FluentValidation her DTO için
  - Her dosya max 1500 satır (§10.1)
  ```
- **Doğrulama:** .agent klasörü ve 2 dosya var

### ☐ 0.5 — Yedekler/ klasörü oluştur
- **Konum:** `I:\desedoorweb\Yedekler\`
- **Alt klasörler:** `db/`, `anayasa_yedek_2026XXXX/`
- **İçerik:** Mevcut desadoor.db dosyasını `Yedekler/db/desadoor_20260514_baslangic.db` olarak kopyala
- **Doğrulama:** Yedek dosyası var

### ☐ 0.6 — .gitignore güncelle
- Aşağıdaki kalıplar olmalı:
  ```
  bin/
  obj/
  *.db-shm
  *.db-wal
  .vs/
  appsettings.Production.json
  .env
  Yedekler/db/*.db
  ```
- **Doğrulama:** .gitignore içeriği güncel

## ✅ İŞ PAKETİ 0 DOĞRULAMA
- [ ] KURALLAR.md var, 2500+ satır
- [ ] GOREV_1_YAPILDI.md ve GOREV_2_YAPILACAK.md var
- [ ] .agent klasöründe 2 dosya var
- [ ] Yedekler/db/ klasöründe başlangıç DB yedeği var
- [ ] `dotnet build` hatasız geçer

---

# ▶ İŞ PAKETİ 1 — VERİTABANI ŞEMASINI ANAYASAYA UYDUR & GENİŞLET
> **Tahmini süre:** 3-5 gün | **Öncelik:** EN KRİTİK | **Anayasa:** §8, §4

## 1.A — Mevcut Modeller Anayasaya Uygunluk Kontrolü

### ☐ 1.A.1 — Mevcut 7 model dosyasını incele
**Dosyalar:**
- `Desadoor.Api/Modeller/KapakModeli.cs`
- `Desadoor.Api/Modeller/SayfaIcerigi.cs`
- `Desadoor.Api/Modeller/GaleriGorseli.cs`
- `Desadoor.Api/Modeller/YoneticiKullanici.cs`
- `Desadoor.Api/Modeller/CanliSohbetMesaji.cs`
- `Desadoor.Ortak/Modeller/Icerik/MenuOgesi.cs`
- `Desadoor.Ortak/Modeller/Icerik/BlogYazisi.cs` vb.

**Kontrol kriterleri:**
- [ ] Tüm property'ler Türkçe (PascalCase)
- [ ] [JsonIgnore] gerekli alanlarda var (§3.4): SifreHash, vb.
- [ ] DateTime alanlar `DateTime.UtcNow` kullanılıyor
- [ ] Soft delete için `SilindiMi`, `SilinmeTarihi` alanları
- [ ] `OlusturulmaTarihi`, `GuncellenmeTarihi`, `OlusturanKullaniciId` audit alanları
- [ ] FirmaId opsiyonel (SaaS-ready için, başlangıçta nullable)

### ☐ 1.A.2 — YoneticiKullanici → Kullanici olarak genişlet
- **Eklenecek alanlar:**
  - `Eposta` (unique)
  - `Telefon`
  - `Rol` (enum: SuperAdmin, Admin, Editor, Musteri)
  - `EmailDogrulandiMi`, `EmailDogrulamaToken`
  - `SonGirisTarihi`, `SonGirisIP`
  - `IkiAdimDogrulamaAktif`, `TotpAnahtari`
  - `PinHash` (kısa giriş için), `DesenHash`
  - `SifreSifirlamaToken`, `TokenGecerlilikTarihi`
  - `AktifMi`, `KilitlendiMi`, `BasarisizGirisDenemesi`
- **JsonIgnore zorunlu:** SifreHash, PinHash, DesenHash, SifreSifirlamaToken, TokenGecerlilikTarihi, TotpAnahtari, EmailDogrulamaToken
- **Migration adı:** `KullaniciModeliGenisletildi`

## 1.B — Eksik Tabloları Ekle (Sıralı)

### ☐ 1.B.1 — Firma (SaaS-ready, başlangıçta tek kayıt)
**Konum:** `Desadoor.Ortak/Modeller/Core/Firma.cs` (mevcut, zenginleştir)
**Alanlar:**
```
Id, Slug, Ad, AciklamaKisa, Aciklama,
Domain (desadoor.com.tr), YedekDomain (www.desadoor.com.tr),
Logo, Favicon, Eposta, Telefon1, Telefon2, Whatsapp,
Adres, Sehir, Ilce, PostaKodu, Ulke,
Enlem, Boylam (harita için),
CalismaSaatleri, KurulusYili,
Twitter, Facebook, Instagram, YoutubeKanal, Pinterest, LinkedIn, TiktokKanal,
TasarimRengi1, TasarimRengi2, TasarimRengi3 (tema renkleri),
AktifMi, OlusturulmaTarihi
```
**Seed:** DesaDoor firma kaydı (Bursa Çalı adresi, tel: 0224 482 24 00, vb.)

### ☐ 1.B.2 — KapiKategorisi tablosu
**Konum:** `Desadoor.Ortak/Modeller/Sektorler/KapiKategorisi.cs`
**Alanlar:**
```
Id, Slug ("membran", "lake", "laminant", "melamin", "kaplama"),
Ad, Aciklama, KapakResim, Ikon, SiraNo, AktifMi,
SeoBaslik, SeoAciklama, SeoAnahtarKelimeler,
OlusturulmaTarihi, GuncellenmeTarihi
```
**+ KapiKategorisiYerellestirme** tablosu (Dil, Ad, Aciklama, SeoBaslik...)
**Seed:** 5 kategori (Membran, Lake, Laminant, Melamin, Kaplama)

### ☐ 1.B.3 — KapiModeli (mevcut KapakModeli → genişlet)
**Mevcut KapakModeli.cs'i bu yapıya dönüştür:**
```
Id, Slug, Ad, KisaAciklama, Aciklama,
KategoriId (FK → KapiKategorisi),
KapakResim, GaleriResimleri (1:N → KapiModeliResim),
UcBoyutluModelUrl (.glb dosyası),
RenkSecenekleri (RAL kodları JSON),
MalzemeSecenekleri (JSON),
OlcuStandart, OlcuOzelMi,
TeknikOzellikler (JSON: kalınlık, ağırlık, izolasyon, vb.),
SertifikalarJson,
KullanimAlanlari (iç kapı, dış kapı, banyo, vb.),
FiyatBilgi (opsiyonel — vitrin için),
SiraNo, OneCikan, YeniMi, AktifMi,
SeoBaslik, SeoAciklama, SeoAnahtarKelimeler,
OlusturulmaTarihi, GuncellenmeTarihi
```
**+ KapiModeliYerellestirme** + **KapiModeliResim** tabloları
**Seed:** Her kategoriden 3-5 örnek model (toplam 15-25 model)

### ☐ 1.B.4 — MobilyaKategorisi + MobilyaUrunu
**Alanlar:** KapiKategorisi/KapiModeli ile aynı yapı
**Kategoriler:** Mutfak Dolapları, Mutfak Kapak, Banyo Dolapları, Duvar Panelleri, TV Üniteleri, Vestiyer
**Yerelleştirme + Resim tabloları ekle**

### ☐ 1.B.5 — Proje + ProjeKategorisi + ProjeResim
**Konum:** `Desadoor.Ortak/Modeller/Icerik/Proje.cs`
**ProjeKategorisi:** Mutfak, Banyo, Yatak Odası, Ofis, Taç, Aksesuar (6 kategori)
**Proje alanları:**
```
Id, Slug, Baslik, KisaAciklama, Aciklama,
KategoriId, MusteriAdi, MusteriSehir, ProjeTarihi,
KapakResim, OneCikanMi, SiraNo, AktifMi,
SeoBaslik, SeoAciklama,
OlusturulmaTarihi, GuncellenmeTarihi
```
**+ ProjeYerellestirme + ProjeResim (1:N — galeri için)**

### ☐ 1.B.6 — Slayt (Hero Slider)
**Konum:** `Desadoor.Ortak/Modeller/Icerik/Slayt.cs`
**Alanlar:**
```
Id, Baslik, AltBaslik, Aciklama,
ArkaplanResim (büyük), ArkaplanResimMobil,
ButonMetni1, ButonLink1, ButonMetni2, ButonLink2,
AnimasyonTipi (fade/slide/zoom enum),
GecisHizi (ms), GosterimSuresi (ms),
MetinHizalama (sol/orta/sağ), MetinRengi,
SiraNo, AktifMi, BaslangicTarihi, BitisTarihi (zaman bazlı yayın),
OlusturulmaTarihi
```
**+ SlaytYerellestirme**
**Seed:** 4 slayt (desadoor.com.tr'deki gibi — "Her mekana her yaşama özel kapılar" vb.)

### ☐ 1.B.7 — Referans (TV kanalları + Müşteri logoları)
**Konum:** `Desadoor.Ortak/Modeller/Icerik/Referans.cs`
**Alanlar:**
```
Id, Ad, Logo, Tip (enum: Medya, Musteri, Tedarikçi, Sertifika),
WebSite, Aciklama, SiraNo, AktifMi,
OlusturulmaTarihi
```
**Seed:** Show TV, ATV, Kanal D, TRT, Eczacıbaşı + 10 müşteri logosu (placeholder)

### ☐ 1.B.8 — MusteriYorumu (Testimonial)
**Konum:** `Desadoor.Ortak/Modeller/Icerik/MusteriYorumu.cs`
**Alanlar:**
```
Id, MusteriAdi, MusteriUnvan, MusteriSehir,
Avatar, Yorum, Puan (1-5),
ProjeId (FK opsiyonel — hangi projeyle ilgili),
Onaylandi, OneCikan, SiraNo, AktifMi,
YorumTarihi, OlusturulmaTarihi
```
**+ MusteriYorumuYerellestirme**

### ☐ 1.B.9 — HizmetAdimi (4 Adımlı Süreç)
**Konum:** `Desadoor.Ortak/Modeller/Icerik/HizmetAdimi.cs`
**Alanlar:**
```
Id, Baslik, Aciklama, Ikon (FontAwesome class veya SVG),
AdimNo, SiraNo, AktifMi
```
**+ HizmetAdimiYerellestirme**
**Seed:** 4 adım (Ölçüm, Ön Tasarım, Detaylı Tasarım, Kurulum)

### ☐ 1.B.10 — SSS (Sıkça Sorulan Sorular)
**Konum:** `Desadoor.Ortak/Modeller/Icerik/SikSorulanSoru.cs`
**Alanlar:**
```
Id, Soru, Cevap, KategoriAdi (Genel/Ürün/Hizmet/Garanti),
SiraNo, AktifMi, GoruntulemeSayisi, FaydaliMi,
OlusturulmaTarihi
```
**+ SikSorulanSoruYerellestirme**

### ☐ 1.B.11 — Sertifika (Kalite Belgeleri)
**Alanlar:** Id, Ad, Aciklama, Resim, PdfDosya, VerilmeTarihi, GecerlilikTarihi, VerenKurum, SiraNo, AktifMi

### ☐ 1.B.12 — Katalog (PDF Dosyaları)
**Konum:** `Desadoor.Ortak/Modeller/Icerik/Katalog.cs`
**Alanlar:**
```
Id, Baslik, Aciklama, KapakResim, PdfDosyaYolu,
DosyaBoyutuMb, SayfaSayisi, Yil,
IndirilmeSayisi, SiraNo, AktifMi,
OlusturulmaTarihi
```
**Seed:** "DesaDoor Kapı Kataloğu 2024", "DesaDoor Kapak Kataloğu 2025"

### ☐ 1.B.13 — Bulten (Newsletter Aboneleri)
**Alanlar:** Id, Eposta, AdSoyad, AbonelikTarihi, IptalTarihi, AktifMi, DogrulamaToken, DogrulandiMi, KaynakSayfa, IP

### ☐ 1.B.14 — EpostaSablonu + EpostaKampanyasi
- Aboneye otomatik mail gönderim için
- Standart şablonlar: Hoş Geldiniz, Doğrulama, İletişim Cevabı, Newsletter

### ☐ 1.B.15 — Sube (Showroom)
**Alanlar:**
```
Id, Ad, Adres, Sehir, Ilce, Telefon, Eposta,
Enlem, Boylam, CalismaSaatleri,
Aciklama, Resimler (1:N),
SubeYetkilisi, SubeYetkilisiTelefon,
SiraNo, AktifMi
```

### ☐ 1.B.16 — EkipUyesi (Hakkımızda sayfası için)
**Alanlar:** Id, AdSoyad, Unvan, Bio, Resim, Linkedin, SiraNo, AktifMi

### ☐ 1.B.17 — SistemAyari (Site genel ayarlar)
**Alanlar:** Anahtar, Deger, Tip (string/int/bool/json), Aciklama
**Seed kayıtlar:**
- `site.baslik`, `site.aciklama`, `site.logo`
- `seo.varsayilanBaslik`, `seo.varsayilanAciklama`
- `iletisim.email`, `iletisim.telefon`
- `tema.anaRenk`, `tema.ikincilRenk`, `tema.fontAilesi`
- `sosyal.twitter`, `sosyal.instagram`, vb.

### ☐ 1.B.18 — Ceviri (FusionCache cache'lenecek)
**Konum:** `Desadoor.Ortak/Modeller/Icerik/Ceviri.cs`
**Alanlar:**
```
Id, Anahtar (ör: "ortak.kaydet"), Dil (tr/en/ar),
Deger, Bolum (ör: "anasayfa", "iletisim"),
OlusturulmaTarihi, GuncellenmeTarihi
```
**Composite Unique Index:** (Anahtar, Dil)
**Seed:** wwwroot/i18n/tr.json + en.json içindeki anahtarları DB'ye aktar

### ☐ 1.B.19 — Dil (Desteklenen diller)
**Alanlar:** Id, Kod (tr/en), Ad (Türkçe/English), Bayrak, SiraNo, VarsayilanMi, AktifMi
**Seed:** TR (varsayılan), EN

### ☐ 1.B.20 — TanitimVideo
**Alanlar:** Id, Baslik, Aciklama, VideoUrl (YouTube/Vimeo embed), KapakResim, SureSaniye, GoruntulemeSayisi, SiraNo, AktifMi

### ☐ 1.B.21 — IletisimMesaji (mevcut, zenginleştir)
**Eksik alanları ekle:**
- `Okundu`, `OkunmaTarihi`, `CevaplandiMi`, `CevapTarihi`, `CevapMetni`
- `OncelikSeviyesi` (Düşük/Normal/Yüksek/Acil enum)
- `Etiketler` (JSON)
- `IPAdresi`, `Tarayici`, `Cihaz` (analitik için)

### ☐ 1.B.22 — Lisans (anayasa §5)
**Konum:** `Desadoor.Ortak/Modeller/Core/Lisans.cs`
**Alanlar:** (anayasa §5.2'deki tam yapı)

### ☐ 1.B.23 — AuditLog (anayasa §33.3 — append-only)
**Konum:** `Desadoor.Ortak/Modeller/Core/AuditLog.cs`
**Alanlar:**
```
Id (long), ZamanDamgasi, CorrelationId,
KullaniciId, FirmaId, Eylem (ör: "Kapi.Olusturuldu"),
EskiDeger (JSON), YeniDeger (JSON),
IPAdresi, Tarayici, ImzaHash (bütünlük için)
```
**Önemli:** DELETE yetkisi olmamalı, sadece INSERT

### ☐ 1.B.24 — ZiyaretKaydi (Analytics)
**Alanlar:** Tarih, IP, Sayfa, Referer, Tarayici, Cihaz, Sehir, Ulke, OturumSuresi

### ☐ 1.B.25 — Online Satış için (FAZ İLERİSİ — opsiyonel)
- `Sepet`, `SepetKalemi`
- `Siparis`, `SiparisKalemi`, `SiparisDurumu` enum
- `OdemeKaydi`
- `KargoTakip`

## 1.C — DbContext Güncelle

### ☐ 1.C.1 — DesadoorDbContext.cs'e tüm DbSet'leri ekle
```csharp
public DbSet<Firma> Firmalar => Set<Firma>();
public DbSet<KapiKategorisi> KapiKategorileri => Set<KapiKategorisi>();
public DbSet<KapiKategorisiYerellestirme> KapiKategorisiYerellestirmeleri => Set<...>();
public DbSet<KapiModeli> KapiModelleri => Set<KapiModeli>();
public DbSet<KapiModeliResim> KapiModeliResimleri => Set<...>();
public DbSet<KapiModeliYerellestirme> KapiModeliYerellestirmeleri => Set<...>();
// ... (tüm yeni tablolar)
public DbSet<Lisans> Lisanslar => Set<Lisans>();
public DbSet<AuditLog> AuditLoglar => Set<AuditLog>();
public DbSet<Ceviri> Ceviriler => Set<Ceviri>();
public DbSet<Dil> Diller => Set<Dil>();
```

### ☐ 1.C.2 — OnModelCreating: İlişkiler, Index'ler
- Tüm Yerellestirme tabloları için (EntityId, Dil) composite unique index
- Slug alanları için unique index
- AuditLog için ImzaHash hesaplama
- BCrypt parola alanları için MaxLength

### ☐ 1.C.3 — TohumVerisi.cs zenginleştir
- 5 KapiKategorisi seed
- 20 KapiModeli seed (her kategoriden 4)
- 4 HizmetAdimi seed
- 4 Slayt seed (desadoor.com.tr referans metinleri)
- 10 SSS seed
- 5 Referans (Show TV, ATV, Kanal D, TRT, NTV)
- 5 MusteriYorumu seed
- 1 Firma (DesaDoor — Bursa Çalı)
- 2 Dil (TR, EN)
- Tüm Ceviri kayıtları (mevcut tr.json + en.json'dan aktarılacak)

## 1.D — Migration Oluştur

### ☐ 1.D.1 — Adım adım migration (parçalı — anayasa §0.4)
```bash
cd I:\desedoorweb\Desadoor.Api

# Adım 1: Kullanıcı genişletme
dotnet ef migrations add KullaniciModeliGenisletildi

# Adım 2: Firma + Lisans
dotnet ef migrations add FirmaVeLisansEklendi

# Adım 3: İçerik kategori sistemi
dotnet ef migrations add KapiVeMobilyaKategoriEklendi

# Adım 4: Ürün modelleri
dotnet ef migrations add KapiModeliVeYerellestirmeEklendi

# Adım 5: Pazarlama (slayt, referans, yorum, hizmet)
dotnet ef migrations add PazarlamaModulleriEklendi

# Adım 6: SSS, Katalog, Sertifika
dotnet ef migrations add BilgilendirmeModulleriEklendi

# Adım 7: Bülten, EpostaSablon, Sube
dotnet ef migrations add IletisimGenisletildi

# Adım 8: Audit, Ceviri, Dil
dotnet ef migrations add SistemIzlemeEklendi

# Her migration sonrası test
dotnet ef database update
```

## ✅ İŞ PAKETİ 1 DOĞRULAMA
- [ ] `dotnet ef migrations list` → 8+ yeni migration görünüyor
- [ ] `dotnet ef database update` → hatasız çalışıyor
- [ ] desadoor.db ~25+ tablo içeriyor (sqlite browser ile kontrol)
- [ ] Tüm Yerellestirme tablolarında unique index var
- [ ] Seed verisi yüklendi (Firma, Kategori, Slayt, vb. boş değil)
- [ ] DB yedeği `Yedekler/db/desadoor_20260514_paket1.db` alındı

---

# ▶ İŞ PAKETİ 2 — BACKEND MODÜLER YAPILANDIRMA
> **Tahmini süre:** 3-4 gün | **Öncelik:** YÜKSEK | **Anayasa:** §9.4, §7, §10

## 2.A — Klasör Yapısını Modüler Yap (§9.4)

### ☐ 2.A.1 — Kontrolcüleri grupla
**Hedef yapı:**
```
Desadoor.Api/Kontrolcüler/
├── Icerik/
│   ├── KapiKategorisiKontrolcu.cs
│   ├── KapiModeliKontrolcu.cs
│   ├── MobilyaKategorisiKontrolcu.cs
│   ├── MobilyaUrunuKontrolcu.cs
│   ├── ProjeKontrolcu.cs (mevcut yok — oluştur)
│   ├── BlogKontrolcu.cs (mevcut, taşı)
│   ├── SayfaIcerigiKontrolcu.cs (mevcut, taşı)
│   ├── KatalogKontrolcu.cs
│   └── SikSorulanSoruKontrolcu.cs
├── Pazarlama/
│   ├── SlaytKontrolcu.cs
│   ├── ReferansKontrolcu.cs
│   ├── MusteriYorumuKontrolcu.cs
│   └── HizmetAdimiKontrolcu.cs
├── Iletisim/
│   ├── IletisimKontrolcu.cs (mevcut, taşı)
│   ├── BultenKontrolcu.cs
│   ├── SohbetKontrolcu.cs (mevcut, taşı)
│   └── EpostaSablonuKontrolcu.cs
├── Kimlik/
│   ├── KimlikKontrolcu.cs (mevcut, taşı)
│   ├── KullaniciKontrolcu.cs
│   └── YetkiKontrolcu.cs
├── Kurumsal/
│   ├── FirmaKontrolcu.cs
│   ├── SubeKontrolcu.cs
│   ├── EkipKontrolcu.cs
│   ├── SertifikaKontrolcu.cs
│   └── HakkimizdaKontrolcu.cs
├── Sistem/
│   ├── AyarlarKontrolcu.cs (mevcut, taşı)
│   ├── DilVeCeviriKontrolcu.cs
│   ├── MedyaKontrolcu.cs
│   ├── DashboardKontrolcu.cs (mevcut, taşı)
│   ├── AuditLogKontrolcu.cs
│   ├── YedeklemeKontrolcu.cs
│   ├── SeoKontrolcu.cs
│   └── TemaKontrolcu.cs (mevcut, taşı)
└── Core/
    ├── SaglikKontrolcu.cs (/api/health)
    ├── SitemapKontrolcu.cs
    └── RobotsKontrolcu.cs
```

### ☐ 2.A.2 — Servisleri modüler yap
**Hedef yapı:**
```
Desadoor.Api/Servisler/
├── Iletisim/
│   ├── IEpostaServisi.cs + EpostaServisi.cs (MailKit wrapper)
│   ├── BultenServisi.cs
│   └── SohbetServisi.cs
├── Icerik/
│   ├── KapiServisi.cs
│   ├── ProjeServisi.cs
│   └── BlogServisi.cs
├── Sistem/
│   ├── ResimIslemcisi.cs (SixLabors.ImageSharp wrapper)
│   ├── OnbellekYonetici.cs (FusionCache wrapper)
│   ├── CeviriServisi.cs (DB + Cache)
│   ├── AuditServisi.cs
│   └── YedeklemeServisi.cs
└── Kimlik/
    ├── JwtServisi.cs
    ├── KullaniciServisi.cs
    └── LisansDogrulamaServisi.cs
```

## 2.B — Merkezi Hata Yönetimi (§7)

### ☐ 2.B.1 — Cevap<T> sınıfı oluştur
**Konum:** `Desadoor.Ortak/Modeller/Core/Cevap.cs` (mevcut, anayasa §7.3'e göre tamamla)

### ☐ 2.B.2 — HataYonetimiMiddleware
**Konum:** `Desadoor.Api/AraYazilimlar/HataYonetimiMiddleware.cs`
**Görev:**
- Tüm exception'ları yakala
- Production'da detay verme, dev'de stack trace dön
- CorrelationId ekle (her isteğe Guid)
- Audit log'a yaz
- Cevap<T>.Hata() formatında JSON dön

### ☐ 2.B.3 — Tüm controller'lardan try-catch'leri kaldır
- Kontrolcüler sadece iş mantığı yazsın
- Hatalar middleware'e gitsin

## 2.C — Validation (§23.6)

### ☐ 2.C.1 — FluentValidation NuGet ekle
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
```

### ☐ 2.C.2 — Her DTO için Dogrulayici sınıfı
**Örnek:** `Desadoor.Api/Dogrulayicilar/KapiModeliDogrulayici.cs`
```csharp
public class KapiModeliDogrulayici : AbstractValidator<KapiModeliDto>
{
    public KapiModeliDogrulayici()
    {
        RuleFor(x => x.Ad).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().Matches(@"^[a-z0-9-]+$");
        // ...
    }
}
```
**En azından şu DTO'lar için:** KapiModeli, KapiKategorisi, Proje, Slayt, MusteriYorumu, IletisimMesaji, Kullanici, Bulten, SikSorulanSoru

## 2.D — Loglama (§15)

### ☐ 2.D.1 — Serilog NuGet ekle
```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
```

### ☐ 2.D.2 — Program.cs'de Serilog yapılandır
- Konsol + Dosya (günlük rotasyon)
- 30 günlük log saklama
- CorrelationId enricher
- KullaniciId, FirmaId enricher (HttpContext'ten)

### ☐ 2.D.3 — Loglara YASAK alan kontrolü (§15)
- SifreHash, PinHash, JWT token, API key — log'da OLMAYACAK
- Yapısal log: `_logger.LogInformation("Kullanici girisi {KullaniciId} {Eposta}", id, eposta);`

## 2.E — Lisans ve Domain Kilidi (§5)

### ☐ 2.E.1 — LisansDogrulamaMiddleware
**Konum:** `Desadoor.Api/AraYazilimlar/LisansDogrulamaMiddleware.cs`
**Mantık:** anayasa §5.4 — domain kontrol + tarih kontrol + HMAC doğrula

### ☐ 2.E.2 — LisansUretici
**Konum:** `Desadoor.Api/Servisler/Kimlik/LisansUreticiServisi.cs`
**Mantık:** anayasa §5.5

### ☐ 2.E.3 — appsettings.json
```json
"LisansAyarlari": {
  "GizliAnahtar": "DESADOOR_HMAC_2026_SECRET_KEY_min_32char",
  "HardLockGunSayisi": 7
}
```

### ☐ 2.E.4 — Seed: DesaDoor için lisans kaydı
- BirincilDomain: "desadoor.com.tr"
- YedekDomain: "www.desadoor.com.tr"
- LisansTipi: "Omurboyu" veya 5 yıllık

## 2.F — Audit Log (§33.3)

### ☐ 2.F.1 — AuditServisi
**Konum:** `Desadoor.Api/Servisler/Sistem/AuditServisi.cs`
**Metotlar:**
- `KaydetAsync(eylem, eskiDeger, yeniDeger)`
- Otomatik: KullaniciId, FirmaId, IPAdresi HttpContext'ten
- ImzaHash: SHA256(prev_hash + record_data)

### ☐ 2.F.2 — EF Core SaveChangesAsync override
- Tüm entity değişikliklerini AuditLog'a yaz
- DbContext'te `OnSaveChanges` interceptor

## 2.G — Rate Limiting (§3.2, §23.5)

### ☐ 2.G.1 — Microsoft.AspNetCore.RateLimiting (NET 10 built-in)
- API genel: 1000 istek / 5 dakika
- /auth/giris: 5 istek / 1 dakika
- IP bazlı + opsiyonel kullanıcı bazlı

## ✅ İŞ PAKETİ 2 DOĞRULAMA
- [ ] Kontrolcüler modüler klasörlerde
- [ ] Hiçbir kontrolcüde try-catch yok (sadece middleware'de)
- [ ] Tüm endpoint'ler Cevap<T> dönüyor
- [ ] Serilog konsol ve dosyaya log yazıyor (`logs/gunluk-YYYYMMDD.log`)
- [ ] FluentValidation çalışıyor (boş ad gönderince 400 dönüyor)
- [ ] LisansDogrulamaMiddleware desadoor.com.tr için 200, başka domain için 403
- [ ] AuditLog tablosunda kayıt oluşuyor (test: bir kapı modeli oluştur)
- [ ] DB yedeği `paket2.db` alındı
- [ ] `dotnet test` çalışıyor (eğer test eklendiyse)

---

# ▶ İŞ PAKETİ 3 — FRONTEND ANIMASYONLU SAYFALAR (desadoor.com.tr Birebir)
> **Tahmini süre:** 5-7 gün | **Öncelik:** YÜKSEK | **Anayasa:** §12, §25, §28

## 3.A — Tasarım Sistemi Hazırlığı

### ☐ 3.A.1 — tokens.css zenginleştir
**Konum:** `Desadoor.UI/wwwroot/css/sistem/temeller/degiskenler.css`
**DesaDoor için renk paleti (Industrial Luxury):**
```css
:root {
  /* Ana renkler — Siyah/Beyaz/Bronze (desadoor.com.tr referans) */
  --renk-ana: #0a0a0a;
  --renk-ikinci: #c19b76;  /* Bronze */
  --renk-vurgu: #d4a574;
  --renk-arkaplan: #ffffff;
  --renk-arkaplan-koyu: #1a1a1a;
  --renk-metin: #2c2c2c;
  --renk-metin-acik: #6c6c6c;
  
  /* Tipografi */
  --font-baslik: 'Playfair Display', serif;
  --font-metin: 'Inter', sans-serif;
  --font-vurgu: 'Cormorant Garamond', serif;
  
  /* Boşluk */
  --bosluk-xs: 0.5rem;
  --bosluk-sm: 1rem;
  --bosluk-md: 1.5rem;
  --bosluk-lg: 2.5rem;
  --bosluk-xl: 4rem;
  
  /* Gölge */
  --golge-yumusak: 0 4px 20px rgba(0,0,0,0.08);
  --golge-orta: 0 10px 40px rgba(0,0,0,0.12);
  --golge-luks: 0 20px 60px rgba(193,155,118,0.15);
  
  /* Geçişler */
  --gecis-hizli: 0.2s ease;
  --gecis-orta: 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  --gecis-yavas: 0.8s cubic-bezier(0.4, 0, 0.2, 1);
  
  /* Breakpoint */
  --ekran-mobil: 480px;
  --ekran-tablet: 768px;
  --ekran-masaustu: 1280px;
}
```

### ☐ 3.A.2 — MudThemeProvider tokens.css ile besle
**Konum:** `Desadoor.UI/Bilesenler/TemaSaglayici.razor` (yeni)
**Görev:** MudTheme nesnesini CSS değişkenlerinden oluştur
- Anayasa §12.3 zorunluluğu

### ☐ 3.A.3 — Razor `<style>` etiketlerini temizle (§12.4)
- Tüm .razor dosyaları kontrol et — `<style>` blokları varsa global CSS'e taşı
- Modal, animasyon CSS'leri `wwwroot/css/sistem/bilesenler/` altına

## 3.B — Hero Slider (Anasayfa)

### ☐ 3.B.1 — HeroSlider bileşeni
**Konum:** `Desadoor.UI/Bilesenler/Anasayfa/HeroSlider.razor`
**Özellikler:**
- Tam ekran (100vh)
- API'den Slayt listesi çek
- Fade/Slide/Zoom animasyon (DB'den)
- Otomatik dönüş + manuel kontrol
- Mobilde farklı resim (ArkaplanResimMobil)
- 2 buton (CTA) desteği
- Metin animasyonu (GSAP)
- Pagination (alt noktalar)
- "Aşağı kaydır" oku (animasyonlu)

### ☐ 3.B.2 — Slayt'larda kullanılacak içerik (desadoor.com.tr referans)
**Seed metinler:**
1. "Her Mekana Her Yaşama Özel Kapılar" / "1992'den beri kalite ve estetik"
2. "Çok Boyutlu Şıklık" / "Modern mutfak kapak modellerimiz"
3. "Detaylarda Mükemmellik" / "Banyo dolapları için özel çözümler"
4. "Sanal Tur ile Keşfet" / "3D fabrika turumuza katılın"

## 3.C — Ana Sayfa Bölümleri

### ☐ 3.C.1 — Kategori Vitrini Bölümü
**Konum:** `Desadoor.UI/Bilesenler/Anasayfa/KategoriVitrini.razor`
**Görsel:** 5 kategori kart (Membran, Lake, Laminant, Melamin, Kaplama)
**Animasyon:** AOS scroll reveal (`data-aos="fade-up"`, stagger 100ms)
**Hover:** Resim zoom + overlay metin

### ☐ 3.C.2 — Öne Çıkan Projeler Bölümü
**Konum:** `Desadoor.UI/Bilesenler/Anasayfa/OneCikanProjeler.razor`
**Görsel:** Masonry grid, 6 proje
**Animasyon:** AOS fade-in
**Tıklayınca:** Lightbox açılır

### ☐ 3.C.3 — Hizmet Süreci Bölümü (4 Adımlı)
**Konum:** `Desadoor.UI/Bilesenler/Anasayfa/HizmetSureciBolumu.razor`
**Görsel:** Yatay timeline (mobilde dikey)
**Animasyon:** Scroll'a göre çizgi dolma + adımlar görünme

### ☐ 3.C.4 — Müşteri Yorumları Carousel
**Konum:** `Desadoor.UI/Bilesenler/Anasayfa/MusteriYorumlariCarousel.razor`
**Özellikler:**
- MudCarousel veya custom GSAP
- Avatar + isim + yıldız + yorum
- Otomatik geçiş

### ☐ 3.C.5 — Referans Şeridi (Sonsuz Kayan)
**Konum:** `Desadoor.UI/Bilesenler/Anasayfa/ReferansSeridi.razor`
**Özellikler:**
- CSS animation: `marquee infinite`
- Show TV, ATV, Kanal D, TRT logoları yan yana
- Hover'da durdurma

### ☐ 3.C.6 — Sayılarla DesaDoor (Counter)
**Konum:** `Desadoor.UI/Bilesenler/Anasayfa/SayilarlaDesadoor.razor`
**İçerik:** 1992'den beri, 1600+ proje, 1620+ müşteri, 5 kategori
**Animasyon:** Sayı sayma efekti (görünür olunca)

### ☐ 3.C.7 — Blog Şeridi (Son 3 Yazı)
**Konum:** `Desadoor.UI/Bilesenler/Anasayfa/BlogSeridi.razor`

### ☐ 3.C.8 — İletişim CTA Bölümü
**Konum:** `Desadoor.UI/Bilesenler/Anasayfa/IletisimCTA.razor`
**Görsel:** Büyük arkaplan + "Hemen Randevu Al" butonu + telefon

## 3.D — Kategori ve Ürün Sayfaları

### ☐ 3.D.1 — KapiModelleri.razor zenginleştir
**Özellikler:**
- Üstte hero başlık
- Filtre kenar çubuğu (kategori, renk, malzeme, fiyat)
- Grid view (3-4 sütun)
- Hover'da hızlı bakış
- AOS staggered fade-in
- Sonsuz scroll veya sayfalama
- URL filtreleri (`?kategori=membran&renk=ahsap`)

### ☐ 3.D.2 — KapakDetay.razor (Ürün Detay) — KRİTİK
**Bölümler:**
- 3D Model Viewer (üstte tam genişlik)
- Galeri (yan tarafta thumbnail'lar)
- Ürün başlık + kısa açıklama
- **RAL Renk Seçici** (mevcut RenkSecici.razor'ı kullan)
- Malzeme seçici
- Ölçü girişi (mm)
- Teknik özellikler tablosu
- Sertifikalar
- "İletişime Geç" + "Randevu Al" + "Sepete Ekle" (3D config JSON ile §28.3)
- Aşağıda: benzer ürünler

### ☐ 3.D.3 — Projeler.razor
**Özellikler:**
- Filtre: kategori (Mutfak/Banyo/Y.Odası/Ofis/Taç/Aksesuar)
- Masonry layout
- Lightbox galeri (her projede çoklu resim)

### ☐ 3.D.4 — ProjeDetay.razor (yeni)
**Bölümler:**
- Slider/galeri
- Proje bilgileri (müşteri, şehir, tarih)
- Açıklama
- Kullanılan ürünler (linkli)
- İlgili projeler

## 3.E — Diğer Sayfalar

### ☐ 3.E.1 — Hakkimizda.razor zenginleştir
- 1992'den beri timeline
- Vizyon/Misyon kartları
- Fabrika resimleri (galeri)
- Ekip üyeleri
- Sertifikalar

### ☐ 3.E.2 — Iletisim.razor
- Harita embed (Google Maps — Bursa Çalı)
- İletişim bilgileri (tel, eposta, adres)
- İletişim formu (FluentValidation)
- Sosyal medya linkleri
- Çalışma saatleri

### ☐ 3.E.3 — SSS.razor (akordion)
- Kategorilere göre gruplu
- Açılır/kapanır animasyon (GSAP)
- Arama kutusu

### ☐ 3.E.4 — Blog.razor + BlogDetay.razor (yeni)
- Grid liste
- Detay sayfa (resim, içerik, paylaş)

### ☐ 3.E.5 — Referanslar.razor
- TV kanalları + Müşteri logoları
- Hover'da renk değişimi

### ☐ 3.E.6 — Katalog.razor (yeni — PDF indirme)
- Katalog kartları
- PDF önizleme (mevcut Gotho.BlazorPdf paketi)
- İndirme sayacı

### ☐ 3.E.7 — Galeri.razor (yeni — Pinterest masonry)

### ☐ 3.E.8 — Subelerimiz.razor (yeni)
- Liste + harita

## 3.F — Düzen (Layout) ve Navigasyon

### ☐ 3.F.1 — DesaDoorDuzen.razor zenginleştir
- Üstte ince haber bandı (telefon, sosyal medya)
- Logo + ana menü + dil seçici + sepet + arama ikonu
- Sticky header (scroll'da küçülür)
- Mobilde hamburger menü (drawer)
- Footer (büyük — logo, hızlı linkler, sosyal medya, newsletter, telif)

### ☐ 3.F.2 — MenuOgesi dinamik render (§57.1)
- Hardcoded `<NavLink>` YASAK
- API'den menü çek + MenuServisi cache
- Drop-down submenu desteği

### ☐ 3.F.3 — DilSecici (header'da)
- TR / EN bayraklı dropdown
- Seçim sonrası tüm metinler değişir

## 3.G — Animasyonlar (Anayasa §28 — GSAP/AOS)

### ☐ 3.G.1 — AnimasyonMotoruServisi tamamla
**Konum:** `Desadoor.UI/Servisler/AnimasyonMotoruServisi.cs` (mevcut, zenginleştir)
**Metodlar:**
- `SayfaGirisAnimasyonAsync()` — fade-in + slight slide
- `SayfaCikisAnimasyonAsync()` — fade-out
- `ScrollAnimasyonBaslatAsync()` — AOS init
- `MetinYazmaAnimasyonAsync(elementId, text)` — typewriter
- `SayiSaymaAnimasyonAsync(elementId, hedefSayi)` — counter

### ☐ 3.G.2 — Smooth scroll
- GSAP ScrollTo
- Tüm anchor link'lerde smooth davranış

### ☐ 3.G.3 — Page transition
- App.razor'da route değişimi yakalanır
- GSAP ile fade-out → fade-in

## ✅ İŞ PAKETİ 3 DOĞRULAMA
- [ ] AnaSayfa açılınca hero slider çalışıyor (4 slayt geçişli)
- [ ] Scroll yapınca AOS animasyonları tetikleniyor
- [ ] Referans şeridi sonsuz kayıyor
- [ ] KapiModelleri sayfasında filtreler çalışıyor
- [ ] KapakDetay sayfasında 3D viewer açılıyor (placeholder model)
- [ ] RAL renk seçici tıklanınca model rengi değişiyor
- [ ] Tüm sayfalarda hardcoded metin YOK (tüm metinler DilServisi.T())
- [ ] Mobil/tablet/masaüstü responsive ✅
- [ ] DB yedeği `paket3.db` alındı

---

# ▶ İŞ PAKETİ 4 — ADMIN PANELİ (MudBlazor — anayasa §12, §57)
> **Tahmini süre:** 4-5 gün | **Öncelik:** YÜKSEK

## 4.A — Admin Layout

### ☐ 4.A.1 — AdminDuzen.razor zenginleştir
**Özellikler:**
- MudLayout + MudAppBar + MudDrawer
- Responsive (260px → 72px → bottom bar — §57.4)
- Üst bar: logo, arama, bildirimler, kullanıcı menüsü, çıkış
- Sol menü: dinamik MenuOgesi listesinden
- Dark/light tema toggle (MudThemeProvider)

### ☐ 4.A.2 — Komut Paleti (Ctrl+K) — §57.3
**Konum:** `Desadoor.UI/Bilesenler/Admin/KomutPaleti.razor`
- MudAutocomplete + glassmorphism overlay
- Fuzzy arama: menü, sayfa, müşteri, mesaj, ürün
- Klavye navigasyonu (↑↓ Enter Esc)

### ☐ 4.A.3 — Sidebar dinamik menü (§57.1)
- API'den MenuOgesi çek (Rol filtreli)
- Aktif menü vurgusu
- Daraltma/genişletme animasyonu

## 4.B — Dashboard

### ☐ 4.B.1 — Dashboard.razor zenginleştir
**Widget'lar:**
- İstatistik kartları: Toplam Ziyaretçi, Mesaj, Newsletter Abone, Bugünkü Sipariş
- LiveCharts grafiği: Son 30 günlük ziyaret
- Son 5 iletişim mesajı
- Popüler ürünler
- Sistem durumu (DB boyut, log boyut, son yedek)

## 4.C — CRUD Sayfaları (Her tablo için)

**Her CRUD sayfası şu standarda uymalı:**
- Üstte MudBreadcrumbs
- Üstte sağda "Yeni Ekle" butonu
- MudDataGrid (sıralama, filtreleme, sayfalama)
- Satır işlemleri: Düzenle, Sil, Önizle, Sırala
- Toplu işlemler (checkbox + dropdown)
- Sil onayı (SilmeOnayDialogu.razor mevcut — kullan)

### ☐ 4.C.1 — Slayt Yönetimi (drag-drop sıralama)
- MudSortable veya BlazorSortable
- Resim önizleme
- Animasyon tipi seçimi
- Yayın zaman aralığı

### ☐ 4.C.2 — KapiKategorisi + KapiModeli Yönetimi
- Resim yükleme (multiple — galeri)
- 3D model dosyası yükleme (.glb)
- RAL renk seçici (çoklu)
- Malzeme listesi
- Teknik özellikler JSON editor
- Dil sekmesi (TR/EN — Yerellestirme tablosuna kayıt)

### ☐ 4.C.3 — MobilyaKategorisi + MobilyaUrunu Yönetimi

### ☐ 4.C.4 — ProjeKategorisi + Proje Yönetimi
- Galeri yükleme (drag-drop sıralama)
- Müşteri bilgileri

### ☐ 4.C.5 — Referans Yönetimi (logolar)

### ☐ 4.C.6 — MusteriYorumu Yönetimi
- Onay kuyruğu
- Toplu onaylama
- Spam tespiti (opsiyonel)

### ☐ 4.C.7 — HizmetAdimi Yönetimi (drag-drop sıralama)

### ☐ 4.C.8 — SSS Yönetimi
- Kategori bazlı
- Sıralama

### ☐ 4.C.9 — Katalog (PDF) Yönetimi
- PDF yükleme
- Kapak resmi otomatik üretim (ilk sayfadan)

### ☐ 4.C.10 — Blog Yönetimi
- Zengin metin editör (MudExRichTextEditor — kurulu)
- Resim galeri
- SEO alanları
- Etiket sistemi

### ☐ 4.C.11 — Sertifika Yönetimi

### ☐ 4.C.12 — Sube Yönetimi
- Harita üzerinde konum seçimi

### ☐ 4.C.13 — EkipUyesi Yönetimi

### ☐ 4.C.14 — Bulten Aboneleri
- Liste, ihraç (CSV), abonelikten çıkarma

### ☐ 4.C.15 — IletisimMesajlari (gelen kutusu UI)
- Okunmamış vurgusu
- Yıldız
- Etiket
- Yanıtla (e-posta gönderim)
- Atama (kime atandı)

### ☐ 4.C.16 — Dil ve Çeviri Yönetimi
- Sol: anahtar listesi
- Sağ: TR/EN/AR sütunları (inline edit)
- Eksik çeviri vurgusu
- Toplu içe/dışa aktarma (Excel/CSV)

### ☐ 4.C.17 — Menü Yönetimi (drag-drop ağaç) — §57.6
- jsTree veya MudTreeView
- Yeni ekle, düzenle, sil, rol ata

### ☐ 4.C.18 — Tema Yönetimi
- Renk picker (3 ana renk)
- Font seçici
- Önizleme paneli
- "Hazır Şablonlar" galerisi

### ☐ 4.C.19 — SEO Yönetimi
- Sayfa bazlı meta tags
- Sitemap önizleme
- robots.txt editor
- Google Search Console entegrasyon yer tutucu

### ☐ 4.C.20 — Sistem Ayarları
- SMTP ayarları (test gönderim butonu)
- Sosyal medya hesapları
- Genel site ayarları
- Yedekleme ayarları

### ☐ 4.C.21 — Audit Log Görüntüleyici
- Filtrelenebilir liste (kullanıcı, tarih, eylem)
- JSON diff görüntüleyici (eski → yeni)

### ☐ 4.C.22 — Yedekleme/Geri Yükleme
- Manuel yedek al
- Yedek listesi
- Yedek indir
- Yedek geri yükle (onay ile)

### ☐ 4.C.23 — Kullanıcı/Rol Yönetimi

### ☐ 4.C.24 — Canlı Sohbet Yönetim (mevcut, zenginleştir)
- Online ziyaretçi listesi
- Sohbet geçmişi
- AI otomatik yanıt ayarları

## ✅ İŞ PAKETİ 4 DOĞRULAMA
- [ ] /admin/giris → giriş yapılabiliyor (admin / desadoor2024)
- [ ] Dashboard açılıyor, widget'lar veri gösteriyor
- [ ] Tüm 24+ CRUD sayfası çalışıyor (en azından liste + ekleme)
- [ ] Ctrl+K Komut Paleti açılıyor, arama yapılıyor
- [ ] Mobilde admin paneli kullanılabilir
- [ ] Bir kapı modeli ekleyince anasayfada görünüyor
- [ ] DB yedeği `paket4.db` alındı

---

# ▶ İŞ PAKETİ 5 — 3D GÖRSEL SİSTEM (Anayasa §28)
> **Tahmini süre:** 3-4 gün | **Öncelik:** ORTA-YÜKSEK

## 5.A — UcBoyutMotoru Wrapper Tamamla

### ☐ 5.A.1 — UcBoyutServisi.cs zenginleştir
**Konum:** `Desadoor.UI/Servisler/UcBoyutServisi.cs` (mevcut)
**Metodlar:**
- `SahneBaslatAsync(string canvasId, string modelUrl)`
- `ModeliYukleAsync(string modelUrl)` — .glb/.gltf/.obj
- `RenkUygulaAsync(string materyalAdi, string ralKodu)`
- `MalzemeUygulaAsync(string materyalAdi, string dokuUrl)`
- `OlcuGuncelleAsync(double genislikMm, double yukseklikMm)`
- `KameraVarsayilanAsync()`
- `ResimAlAsync()` — screenshot (canvas → base64 PNG)
- `OtomatikDondurBaslatAsync(bool aktif)`

### ☐ 5.A.2 — uc-boyut-motoru.js geliştir
**Konum:** `Desadoor.UI/wwwroot/js/uc-boyut-motoru.js` (mevcut)
- Three.js + OrbitControls + GLTFLoader (kurulu)
- DRACOLoader ekle (sıkıştırılmış model desteği)
- Aydınlatma sistemi (HDR environment map)
- AO (Ambient Occlusion) shadow

## 5.B — 3D Model Dosyaları

### ☐ 5.B.1 — wwwroot/models/ klasörüne placeholder modeller
- `kapi-membran-orneği.glb`
- `kapi-lake-orneği.glb`
- `mutfak-dolap-orneği.glb`
- Bunlar geçici — gerçek modeller fotoğraf veya ücretsiz GLB modellerinden

### ☐ 5.B.2 — Model upload (Admin panel)
- KapiModeli formunda .glb yükleme alanı
- Boyut kontrolü (max 10 MB)
- Otomatik thumbnail oluşturma (Three.js render → PNG)

## 5.C — RAL Renk Sistemi (mevcut zenginleştir)

### ☐ 5.C.1 — RalKatalogu.cs incele
**Konum:** `Desadoor.UI/RalKatalogu.cs` (mevcut)
- 213 RAL kodu olmalı

### ☐ 5.C.2 — RenkSecici.razor zenginleştir
**Konum:** `Desadoor.UI/Bilesenler/RenkSecici.razor` (mevcut)
- Kategoriye göre filtre (Sarı, Mavi, Yeşil, Kahve, vb.)
- Arama
- Favori RAL kodları (localStorage)

## 5.D — Sepete Ekleme (3D Config JSON — §28.3)

### ☐ 5.D.1 — KapakDetay'da "Sepete Ekle" butonu
**Konum:** `Desadoor.UI/Pages/KapakDetay.razor.cs`
**Mantık:**
```csharp
var konfig = new {
    urunId = model.Id,
    konfigurasyon = new {
        ralKodu = secilenRal,
        malzeme = secilenMalzeme,
        genislikMm = girilenGenislik,
        yukseklikMm = girilenYukseklik
    }
};
await SepetServisi.EkleAsync(konfig);
```

## 5.E — WebXR (Opsiyonel — İLERİ FAZA)
- `ArtirilmisGerceklikServis.cs`
- "AR'da Gör" butonu (mobilde)
- Three.js WebXR API entegrasyonu

## ✅ İŞ PAKETİ 5 DOĞRULAMA
- [ ] KapakDetay sayfasında 3D viewer açılıyor
- [ ] Model döndürülebiliyor (OrbitControls)
- [ ] RAL renk değiştirilince model rengi anında değişiyor
- [ ] Ölçü girince viewer yeniden ölçeklenir
- [ ] Screenshot butonu PNG indiriyor
- [ ] Admin paneli .glb yüklemeyi destekliyor

---

# ▶ İŞ PAKETİ 6 — ÇOKLU DİL VE İÇERİK
> **Tahmini süre:** 3-5 gün | **Öncelik:** ORTA | **Anayasa:** §25, §35

## 6.A — DilServisi DB+Cache (§35)

### ☐ 6.A.1 — OnbellekYonetici.cs (FusionCache wrapper)
**Konum:** `Desadoor.Api/Servisler/Sistem/OnbellekYonetici.cs`
**NuGet:** `ZiggyCreatures.FusionCache`
**Metodlar:**
- `GetirVeyaOlusturAsync<T>(string anahtar, Func<Task<T>> uretici, TimeSpan? sure)`
- `SilAsync(string anahtar)`
- `SilDesenAsync(string desen)` — ör: "ceviri:*"

### ☐ 6.A.2 — CeviriServisi (API tarafı)
**Konum:** `Desadoor.Api/Servisler/Sistem/CeviriServisi.cs`
- DB'den çek + 30dk cache
- Admin update edince cache temizle

### ☐ 6.A.3 — DilKontrolcu güncelle
- GET `/api/dil/ceviriler/{dil}` → tüm çeviriler JSON döner
- POST `/api/dil/ceviri` → çeviri ekle/güncelle (admin)

## 6.B — DilServisi (UI tarafı)

### ☐ 6.B.1 — DilServisi.cs zenginleştir
**Konum:** `Desadoor.UI/Servisler/DilServisi.cs` (mevcut)
- App başlangıcında API'den TR ve EN çevirileri çek
- `T(string anahtar, string varsayilan = "")` metodu
- `DilDegistir(string yeniDil)` → localStorage + event
- Component'lar dil değişince yeniden render olsun

### ☐ 6.B.2 — wwwroot/i18n/*.json dosyalarını sil
- Migration ile DB'ye aktardıktan sonra fiziksel dosyaları kaldır
- Eski yöntem (JSON) ARTIK YASAK (§35)

## 6.C — Tüm Razor Sayfalarında DilServisi.T()

### ☐ 6.C.1 — Hardcoded metinleri tara
- Tüm `.razor` dosyaları
- Her gözüken Türkçe metin → `@DilServisi.T("anahtar", "varsayilan")` ile değiştir

**Örnek:**
```razor
@* ❌ ÖNCE *@
<h1>Kapı Modelleri</h1>

@* ✅ SONRA *@
<h1>@DilServisi.T("kapi.modeller.baslik", "Kapı Modelleri")</h1>
```

### ☐ 6.C.2 — Çeviri anahtar yapısı
**Format:** `bolum.alt-bolum.amaç`
**Örnekler:**
- `ortak.kaydet`, `ortak.iptal`, `ortak.sil`, `ortak.duzenle`
- `menu.anasayfa`, `menu.kurumsal`, `menu.kapi-modelleri`
- `anasayfa.hero.baslik1`, `anasayfa.hero.altbaslik1`
- `iletisim.form.ad`, `iletisim.form.eposta`

### ☐ 6.C.3 — Seed: tüm anahtarlar TR + EN
- Migration ile DB'ye yaz
- En az 200 anahtar (tüm UI metinleri için)

## 6.D — İçerik Yerelleştirme

### ☐ 6.D.1 — KapiModeli, KapiKategorisi, Proje, Slayt, vb. tablolar için
- Her ana tablonun `*Yerellestirme` tablosuna içerik yaz (TR + EN)
- API endpoint'leri dil parametresi alacak: `/api/kapi/modeller?dil=tr`

## ✅ İŞ PAKETİ 6 DOĞRULAMA
- [ ] /admin/dil-ve-ceviri sayfasında 200+ anahtar görünüyor
- [ ] Header'daki dil seçici çalışıyor (TR ↔ EN)
- [ ] Tüm sayfalardaki metinler dil değişimine tepki veriyor
- [ ] wwwroot/i18n/ klasöründe JSON dosya KALMADI
- [ ] FusionCache çalışıyor (ikinci istek <5ms)

---

# ▶ İŞ PAKETİ 7 — TEST, GÜVENLİK VE DEPLOY HAZIRLIĞI
> **Tahmini süre:** 2-3 gün | **Öncelik:** YÜKSEK | **Anayasa:** §6, §3, §17

## 7.A — Test Projesi

### ☐ 7.A.1 — Desadoor.Testler projesi oluştur
```bash
cd I:\desedoorweb
dotnet new xunit -n Desadoor.Testler
dotnet sln Desadoor.slnx add Desadoor.Testler/Desadoor.Testler.csproj
```

### ☐ 7.A.2 — Testcontainers kurulumu
```xml
<PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.7" />
```

### ☐ 7.A.3 — Her kontrolcü için 5 test (§23.10)
**Örnek: KapiModeliKontrolcuTestleri.cs**
1. Başarılı senaryo — geçerli veri ile 200 döner
2. Boş veri — 400 döner
3. Yetkisiz — 401/403 döner
4. Başka firma verisi — 404 veya filtrelendi
5. Geri dönüş — başka endpoint hâlâ çalışıyor

## 7.B — Güvenlik Sertleştirme (§3)

### ☐ 7.B.1 — Production appsettings.json kontrol
- [ ] JWT anahtarı env variable'dan alınıyor
- [ ] SMTP şifresi env variable'dan
- [ ] LisansAyarlari:GizliAnahtar env variable'dan
- [ ] CORS sadece desadoor.com.tr ve www.desadoor.com.tr

### ☐ 7.B.2 — HTTPS zorunlu (production)
- `RequireHttpsMetadata = true`
- HSTS aktif

### ☐ 7.B.3 — Güvenlik header'ları
- X-Frame-Options: DENY
- X-Content-Type-Options: nosniff
- Content-Security-Policy
- Referrer-Policy: strict-origin-when-cross-origin

### ☐ 7.B.4 — SignalR production
- `EnableDetailedErrors = false`

### ☐ 7.B.5 — Backdoor kontrolü
- Tüm `[AllowAnonymous]` endpoint'lerini denetle
- Sadece şunlarda olmalı: `/api/kimlik/giris`, `/api/iletisim/mesaj-gonder`, `/api/bulten/abone-ol`, public read endpoint'leri

## 7.D — Deploy Kontrol Listesi (§17 — uygula)

### ☐ 7.D.1 — Anayasa §17'deki tüm maddeleri kontrol et
- [ ] Backdoor şifresi yok
- [ ] BCrypt hash log'a yazılmıyor
- [ ] JWT env variable
- [ ] CORS spesifik
- [ ] RequireHttpsMetadata = true
- [ ] SignalR EnableDetailedErrors = false
- [ ] SifreHash [JsonIgnore]
- [ ] LİSANS sistemi aktif
- [ ] PostgreSQL üretimde
- [ ] Son migration çalıştırıldı
- [ ] DB yedeği alındı
- [ ] dotnet test → tümü yeşil
- [ ] Kritik endpoint'ler test edildi

## ✅ İŞ PAKETİ 7 DOĞRULAMA
- [ ] `dotnet test` → tümü yeşil
- [ ] HTTPS test sertifikası ile site açılıyor
- [ ] Güvenlik tarama (OWASP ZAP) — kritik bulgu yok
- [ ] Production DB yedeği alındı

---

# 📊 SON KONTROL — TÜM PAKETLERİN ÖZETİ

| Paket | Süre | Durum | DB Yedek | Test |
|-------|------|-------|----------|------|
| 0 — Anayasa & Görev | 1 gün | ☐ | - | - |
| 1 — Veritabanı | 3-5 gün | ☐ | ☐ | ☐ |
| 2 — Backend Modüler | 3-4 gün | ☐ | ☐ | ☐ |
| 3 — Frontend Animasyon | 5-7 gün | ☐ | ☐ | ☐ |
| 4 — Admin Paneli | 4-5 gün | ☐ | ☐ | ☐ |
| 5 — 3D Görsel | 3-4 gün | ☐ | ☐ | ☐ |
| 6 — Çoklu Dil | 3-5 gün | ☐ | ☐ | ☐ |
| 7 — Test & Deploy | 2-3 gün | ☐ | ☐ | ☐ |
| **TOPLAM** | **25-35 gün** | | | |

---

# 🚨 ANAYASA UYUMLULUK CHECKLIST (FINAL)

Tüm paketler tamamlandığında bu liste FULL olmalı:

- [ ] §0 — KURALLAR.md, .agent klasörü, görev dosyaları var
- [ ] §1 — .NET 10 + Blazor WASM + MudBlazor (✅ kurulu)
- [ ] §2 — Tüm kod %100 Türkçe (sadece framework istisnaları)
- [ ] §3 — JWT, BCrypt, [JsonIgnore], rate limiting, CORS
- [ ] §4 — Multi-tenant FirmaId filtreleri (opsiyonel ama altyapı hazır)
- [ ] §5 — Lisans + Domain kilidi aktif
- [ ] §6 — Test protokolü çalışıyor, yedek alınıyor
- [ ] §7 — HataYonetimiMiddleware + Cevap<T>
- [ ] §8 — EF Core Code-First, Türkçe tablolar (ASCII)
- [ ] §9.4 — Modüler klasör hiyerarşisi
- [ ] §10.1 — Dosya max 1500 satır (büyükleri partial class)
- [ ] §10.2 — DRY (kod tekrarı yok)
- [ ] §11 — Wrapper kullanımı (JS doğrudan çağrı yasak)
- [ ] §12 — MudBlazor + tokens.css + Razor `<style>` yok
- [ ] §13 — Modül aktivasyon kontrolü (opsiyonel)
- [ ] §15 — Serilog yapısal log, gizli alan yok
- [ ] §16 — Türkçe commit mesajları
- [ ] §17 — Deploy kontrol listesi tamam
- [ ] §18 — Kök dizin temiz
- [ ] §23 — FusionCache, QuestPDF, MailKit, FluentValidation, Mapster, MediatR
- [ ] §25 — Hardcoded metin yok, DilServisi.T() her yerde
- [ ] §28 — 3D motoru wrapper ile (UcBoyutMotoru)
- [ ] §33.3 — Audit Log append-only
- [ ] §35 — Ceviri DB+FusionCache (JSON yok)
- [ ] §44 — Modüler dosya mimarisi
- [ ] §46 — CSS tokens.css merkezi
- [ ] §57 — Dinamik MenuOgesi sistemi, Command Palette

---

# 📞 ARAŞTIRMA NOTLARI (Haiku'dan)

## Tespit edilen kritik noktalar:

1. **Mevcut KapakModeli.cs** — sadece KapakModeli adıyla var, anayasa "KapiModeli" daha doğru olur. Veya iki ayrı tablo: KapakModeli (mutfak kapağı) + KapiModeli (iç/dış kapı)
2. **CanliSohbetMesaji** — mevcut, ama SohbetOturumu tablosuyla bağlanmalı
3. **wwwroot/i18n/tr.json** — içeriğini incele ve DB'ye aktarma migration'ını yaz
4. **DesadoorDuzen.razor** — Layout mevcut ama footer/header eksik
5. **DesaDoor renk paleti** — site siyah/beyaz/bronze. Industrial Luxury anayasa renkleri (#c19b76) tam uyum
6. **Üç boyutlu görüntüleme** — Three.js r128 kurulu, OrbitControls + GLTFLoader hazır
7. **Anayasa §31 (Yerel AI/Ollama)** — DesaDoor için opsiyonel, ileri faza alınabilir
8. **Anayasa §38 (Split Payments) ve §40 (Dynamic Pricing)** — DesaDoor kurumsal site, e-ticaret minimal olduğu için skip edilebilir
9. **Anayasa §27 (Meta-Platform)** — DesaDoor tek firma, ama altyapı SaaS-ready olmalı (FirmaId opsiyonel)
10. **Bugünkü test:** http://localhost:5013 UI çalışıyor, http://localhost:5015 API çalışıyor — temel iskelet hazır

## Önerilen sıralama (kritik yol):
**Paket 0 → 1 → 2 → 3 → 4 → 5 → 6 → 7**

Paket 3 (Frontend) başlamadan önce mutlaka:
- Paket 1 (DB tablolar)
- Paket 2 (Backend API)
- En azından KapiKategorisi, KapiModeli, Slayt, HizmetAdimi, Referans, MusteriYorumu için endpoint'ler hazır olmalı

---

# 🔄 GÜNLÜK İLERLEME LOGU

> Her gün sonunda buraya kayıt düşülmeli (anayasa §6.3, §19)

## 2026-05-XX
- [ ] Bugün hangi paket üzerinde çalışıldı:
- [ ] Tamamlanan maddeler:
- [ ] Karşılaşılan sorunlar:
- [ ] Yarına devredilen:
- [ ] DB yedeği alındı: evet/hayır
- [ ] Test sonuçları: yeşil/kırmızı/yapılmadı

---

**Bu DUZELT.md dosyası, başka modelin (Opus/Sonnet) takip ederek sistematik olarak düzelteceği detaylı yol haritasıdır. Her madde tikleneceği zaman:**
1. **DB yedeği alın** (anayasa §6.1)
2. **Değişikliği uygulayın**
3. **dotnet build hatasız mı?** kontrol edin
4. **DOĞRULAMA kriterini test edin**
5. **GOREV_1_YAPILDI.md'ye ekleyin**
6. **Türkçe commit atın** (anayasa §16)

---

*Hazırlık tarihi: 2026-05-14*
*Araştırma: Claude Haiku 4.5*
*Uygulama: Başka model veya geliştirici*
*Anayasa: I:\desedoorweb\KURALLAR.md (Vizitlink v11.0 — DesaDoor adaptasyonu)*
