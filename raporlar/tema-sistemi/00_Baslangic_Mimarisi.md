# Tema Sistemi Başlangıç Mimarisi

**Proje:** Gold Banyo (goldbanyo.com.tr)
**Tarih:** Haziran 2026
**Durum:** Faz 1 tamamlandı, Faz 2 devam ediyor

---

## 1. Felsefe

> **Tema = Farklı Bir Site**

Tema değişimi sadece renk değişimi **değildir**. Her tema şu katmanların **hepsini** birlikte değiştirir:

| Katman | Değişim kapsamı |
|---|---|
| Renk paleti | Birincil, ikincil, vurgu, arka plan, metin, durum renkleri |
| Tipografi | Başlık/gövde/vurgu/mono ailesi, ağırlık, harf aralığı, boyut skala ratio |
| Geometri | Köşe yuvarlaklığı (sm/md/lg/xl), border kalınlığı ve stili |
| Gölgeler | Drop shadow, glow shadow, vurgu gölgesi |
| Glassmorphism | Aktif/pasif, blur, opaklık, border opaklığı |
| Animasyon | Hız (hızlı/normal/yavaş), cubic-bezier, hover yüksekliği, scroll reveal |
| Layout | Header stili, footer tipi, hero sunumu, kart stili, sütun sayısı |
| İkon seti | Material Icons / Phosphor / Lucide |
| Boşluk ritmi | xs/sm/md/lg/xl/2xl/3xl |

---

## 2. Mevcut Durum

### Aktif Temalar

| # | Slug | Kaynak | Durum |
|---|---|---|---|
| 1 | `gold` | Elle (varsayılan) | Aktif, premium, glassmorphism |
| 2 | `aurelian-onyx` | Stitch import | Aktif, premium, glassmorphism |

### Hedeflenen Tema Sayısı: 20+

10 placeholder tema zaten tanımlı (midnight-noir, marble-rose, copper-bronze, vb.) — manifest ve CSS şablonları hazır.

---

## 3. Hedef Klasör Yapısı

```
[Proje Kökü]/
├── tasarim/                                    ← DESIGN.md kaynakları
│   ├── DESIGN.md
│   ├── DESIGN_aurelian.md
│   └── DESIGN_gold.md                         ← (ileride)
│
├── VizitLink3D.Api/
│   ├── Moduller/Tema/
│   │   ├── Servisler/
│   │   │   ├── CokluTemaServisi.cs             ← tema katalog, yükleme, SignalR
│   │   │   └── StitchTemaServisi.cs            ← Stitch MCP import
│   │   └── AlanModelleri/                      ← (ileride)
│   ├── Kontrolculer/Sistem/
│   │   ├── TemaKontrolcu.cs                    ← /api/tema endpoints
│   │   └── FirmaTemaKontrolcu.cs               ← /api/firma-tema endpoints
│   ├── Hubs/TemaHub.cs                         ← SignalR tema broadcast
│   └── wwwroot/css/temalar/                    ← tema CSS dosyaları
│       ├── _sistem/                            ← ortak (tüm temalar)
│       │   ├── ortak-bilesenler.css
│       │   ├── animasyon-ortak.css
│       │   └── efektler-ortak.css
│       ├── gold/                               ← varsayılan tema
│       │   ├── manifest.json
│       │   ├── tokens.css
│       │   ├── bilesenler.css
│       │   └── animasyonlar.css
│       ├── aurelian-onyx/
│       └── ... (20+ placeholder)
│
├── VizitLink3D.UI/
│   ├── Bilesenler/Tema/
│   │   └── TemaSecici.razor                    ← frontend tema seçici
│   ├── Pages/Admin/
│   │   ├── TemaYonetimi.razor                  ← super admin tema yönetimi
│   │   └── TemaYonetimi.razor.cs
│   └── wwwroot/css/
│       ├── sistem/                             ← temel CSS + modül CSS
│       │   ├── temeller/degiskenler.css
│       │   └── moduller/vizitlink3d.css
│       └── temalar/                            ← (API ile aynı tema dosyaları)
│
└── raporlar/tema-sistemi/                      ← bu belgeler
    ├── 00_Baslangic_Mimarisi.md
    ├── 01_Dosya_Haritasi.md
    ├── kanit-gorseller/
    └── ham-veri/
```

---

## 4. Veri Modeli

### 4.1 TemaSablonu (hedef DB şeması)

```csharp
public class TemaSablonu
{
    public long Id { get; set; }
    public string Kod { get; set; } = "";            // AURELIAN_ONYX
    public string Ad { get; set; } = "";             // Aurelian Onyx
    public string Slug { get; set; } = "";           // aurelian-onyx
    public string Aciklama { get; set; } = "";
    public string Kaynak { get; set; } = "elle";     // varsayilan | stitch | manuel | elle
    public string? StitchProjeId { get; set; }
    public bool GlassmorphismAktif { get; set; }
    public bool Premium { get; set; }
    public decimal Fiyat { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public string? ThumbnailUrl { get; set; }
    public bool Aktif { get; set; } = true;
    public bool VarsayilanMi { get; set; }
    public string Etiketler { get; set; } = "";
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public int Versiyon { get; set; } = 1;
    public bool SilindiMi { get; set; }

    // Görsel kimlik (manifest'ten deserialize)
    public string RenklerJson { get; set; } = "{}";
    public string TipografiJson { get; set; } = "{}";
    public string GeometriJson { get; set; } = "{}";
    public string GolgelerJson { get; set; } = "{}";
    public string GlassmorphismJson { get; set; } = "{}";
    public string AnimasyonJson { get; set; } = "{}";
    public string LayoutJson { get; set; } = "{}";
    public string IkonSeti { get; set; } = "Material Icons";

    // Çeviri anahtarları
    public string AdAnahtar { get; set; } = "";
    public string AciklamaAnahtar { get; set; } = "";
    public string EtiketlerAnahtar { get; set; } = "";
    public string AdVarsayilanTr { get; set; } = "";
    public string AdVarsayilanEn { get; set; } = "";
    public string AciklamaVarsayilanTr { get; set; } = "";
    public string AciklamaVarsayilanEn { get; set; } = "";
}
```

### 4.2 FirmaTemaAtama

```csharp
public class FirmaTemaAtama
{
    public long Id { get; set; }
    public int FirmaId { get; set; }
    public long TemaSablonId { get; set; }
    public bool Aktif { get; set; } = true;
    public DateTime AtamaTarihi { get; set; } = DateTime.UtcNow;
    public string? OzelDegiskenlerJson { get; set; }
}
```

### 4.3 TemaRevizyonu

```csharp
public class TemaRevizyonu
{
    public long Id { get; set; }
    public long TemaSablonId { get; set; }
    public int Versiyon { get; set; }
    public string KaynakTipi { get; set; } = "";    // elle | stitch | manuel
    public string? HamDesignMd { get; set; }
    public string UretilenManifestJson { get; set; } = "{}";
    public string? Notlar { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
```

---

## 5. Runtime Akışı

```
[Firma acılır]
    → FirmaTemaAtama'dan aktif tema bulunur
    → manifest.json okunur
    → data-tema-id (yeni) + data-site-tema (geriye uyum) set edilir
    → tokens.css + bilesenler.css + animasyonlar.css lazy load edilir
    → CSS değişkenleri anında devreye girer (sayfa yenilenmez)
    → Değişiklik varsa SignalR ile broadcast yapılır
```

---

## 6. Admin Akışları

| Ekran | Açıklama | Durum |
|---|---|---|
| Tema Katalog | Tüm temaları listele (thumbnail + ad + premium + etiketler) | Mevcut (kısmi) |
| Tema Detay | Seçili temanın tüm alanlarını göster | Mevcut |
| Tema Önizleme | iframe ile canlı önizleme (farklı cihaz boyutları) | Hedef |
| Firmaya Tema Ata | Firma bazlı tema seçimi ve kaydetme | Mevcut (kısmi) |
| Stitch'ten İçe Aktar | Proje ID → otomatik manifest + CSS üretimi | Mevcut (kısmi) |
| Tema Revizyonları | Versiyon geçmişi ve geri alma | Hedef |

---

## 7. Stitch Entegrasyonu

```
Stitch Proje ID gir
    → stitch_get_project ile DESIGN.md çek
    → Ham dosyayı tema_revizyonlari'na kaydet
    → Manifest.json üret (renk, tipografi, geometri, glassmorphism, animasyon, layout)
    → tokens.css üret (CSS değişkenleri)
    → bilesenler.css üret (component varyasyonları)
    → animasyonlar.css üret (keyframes)
    → Admin önizleme → onay → firmaya ata
```

---

## 8. Eski Tema Yasagi

Eski tema adlari yeni sistemde kullanilmayacak:

- `goldbanyo`
- `goldbanyo-karanlik`
- `gold-luxury-dark`
- `altin-siyah`

Tek aktif Gold Banyo site tema slug'i:

- `gold`

| Katman | Eski | Yeni |
|---|---|---|
| **Token** | `--vizit-primary`, `--vizit-accent`, vb. | `--tema-birincil`, `--tema-vurgu`, vb. |
| **Attribute** | `data-site-tema="gold"` | `data-tema-id="gold"` |
| **Class** | `.vizit-navbar`, `.gb-urun-kart` | `.navbar`, `.urun-kart` |

Her tokens.css dosyası eski alias'ları da tanımlar → mevcut sayfalar bozulmaz.

---

## 9. Tema Değişimi Güvenliği

- **Lazy load:** Sadece seçili temanın CSS dosyaları yüklenir
- **Cache:** `sessionStorage` ile aynı tema tekrar yüklenmez
- **Rollback:** Hata olursa eski temaya geri dönülür
- **Flicker önleme:** Geçiş sırasında hafif karartma overlay'i
- **State koruma:** Scroll pozisyonu, form değerleri, modal durumu korunur
- **SignalR broadcast:** Tüm açık sekmeler同一 anda güncellenir

---

## 10. Faz Planı

### Faz 1 — Tamamlandı
- Aktif dosyaları tek merkezde belgeleme
- Dağınık kanıt/test dosyalarını toplama
- `gold` temasını temel slug yapma
- `manifest.json` yapısını netleştirme

### Faz 2 — Devam Ediyor
- Tema veritabanı modellerini ekleme (TemaSablonu, FirmaTemaAtama, TemaRevizyonu)
- Admin katalog ekranını dinamik hale getirme
- Tema seçimini firma bazlı çalıştırma
- `FirmaTemaKontrolcu` ile `CokluTemaServisi` tam uyum
- `data-site-tema` ve `data-tema-id` birlikte yönetimi

### Faz 3 — Planlanan
- Stitch import akışını açma
- Her tema için ayrı `DESIGN.md` kaynak klasörü
- Layout ve görsel varyasyonları şablona bağlama
- Super admin tema ekleme formu (elle / stitch / manuel CSS)

### Faz 4 — Gelecek
- 20 tema kataloğunu tamamlama
- Önizleme, rollback, revizyon ve yayın akışını bitirme
- Eklenti mekanizması (firma bazlı genişleme)
- Plugin sistemi ile izole çalıştırma

---

## 11. Kritik Kurallar

1. **Tema = farklı site** — sadece renk değişikliği yeterli değil
2. **Kod değişikliği yok** — yeni tema eklemek için sadece dosya + DB satırı
3. **Geriye uyumlu** — eski `data-site-tema`, `--vizit-*`, `.vizit-*` hâlâ çalışır
4. **CSS değişkenleri zorunlu** — hardcoded renk/font/şekil yasak
5. **Tema-özgü dosyalar** — `_sistem/` ortak dosyalarına tema özel değer yazılmaz
6. **Dil-bağımsız CSS** — tema adı/açıklaması `DilServisi.T()` ile gösterilir
7. **Font Türkçe destekli** — Latin Extended veya Unicode tam olmalı
8. **SignalR broadcast** — tema değişimi tüm sekmelere anında yansır

---

*Versiyon: 2.0 — Haziran 2026 | Gold Banyo Tema Sistemi Başlangıç Mimarisi*
