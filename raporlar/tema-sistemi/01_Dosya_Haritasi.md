# Tema Sistemi Dosya Haritası

**Proje:** Gold Banyo (goldbanyo.com.tr)
**Tarih:** Haziran 2026
**Durum:** Aktif tema sistemi dosyalarının tam envanteri

---

## 1. Tema CSS Dosyaları (API wwwroot)

### Varsayılan Tema: gold

| Dosya | Yol | Açıklama |
|---|---|---|
| manifest.json | `VizitLink3D.Api/wwwroot/css/temalar/gold/manifest.json` | Tema kimliği, renk, tipografi, geometri, glassmorphism, animasyon, layout tanımı |
| tokens.css | `VizitLink3D.Api/wwwroot/css/temalar/gold/tokens.css` | `:root[data-tema-id="gold"]` kapsamındaki CSS değişkenleri |
| bilesenler.css | `VizitLink3D.Api/wwwroot/css/temalar/gold/bilesenler.css` | Tema-özgü component varyasyonları (kart, buton, navbar, footer) |
| animasyonlar.css | `VizitLink3D.Api/wwwroot/css/temalar/gold/animasyonlar.css` | Tema-özgü keyframe ve motion preset'leri |

### Ortak Sistem Dosyaları

| Dosya | Yol | Açıklama |
|---|---|---|
| admin-tema.css | `VizitLink3D.Api/wwwroot/css/sistem/bilesenler/admin-tema.css` | Admin paneli tema yönetim ekranı stili |
| admin-tema.css.gz | `VizitLink3D.Api/wwwroot/css/sistem/bilesenler/admin-tema.css.gz` | Gzip sıkıştırılmış versiyonu |
| admin-tema.css.br | `VizitLink3D.Api/wwwroot/css/sistem/bilesenler/admin-tema.css.br` | Brotli sıkıştırılmış versiyonu |

### Tema JS

| Dosya | Yol | Açıklama |
|---|---|---|
| tema.js | `VizitLink3D.Api/wwwroot/js/tema.js` | Tema yükleme, lazy load, cache, geçiş mantığı |
| tema.js.gz | `VizitLink3D.Api/wwwroot/js/tema.js.gz` | Gzip sıkıştırılmış |
| tema.js.br | `VizitLink3D.Api/wwwroot/js/tema.js.br` | Brotli sıkıştırılmış |

---

## 2. Tema CSS Dosyaları (UI wwwroot)

### gold Teması (UI tarafında)

| Dosya | Yol |
|---|---|
| tokens.css | `VizitLink3D.UI/wwwroot/css/temalar/gold/tokens.css` |
| bilesenler.css | `VizitLink3D.UI/wwwroot/css/temalar/gold/bilesenler.css` |
| animasyonlar.css | `VizitLink3D.UI/wwwroot/css/temalar/gold/animasyonlar.css` |
| manifest.json | `VizitLink3D.UI/wwwroot/css/temalar/gold/manifest.json` |

### Sistem CSS

| Dosya | Yol | Açıklama |
|---|---|---|
| degiskenler.css | `VizitLink3D.UI/wwwroot/css/sistem/temeller/degiskenler.css` | Global CSS değişkenleri (tema override ile güncellenir) |
| vizitlink3d.css | `VizitLink3D.UI/wwwroot/css/sistem/moduller/vizitlink3d.css` | Ana modül CSS'i |
| admin-tema.css | `VizitLink3D.UI/wwwroot/css/sistem/bilesenler/admin-tema.css` | Admin tema ekranı |

---

## 3. API Kod Dosyaları

### Tema Modülü

| Dosya | Yol | Açıklama |
|---|---|---|
| CokluTemaServisi.cs | `VizitLink3D.Api/Moduller/Tema/Servisler/CokluTemaServisi.cs` | Tema katalog, yükleme, DESIGN.md → tokens.css üretimi, SignalR broadcast |
| StitchTemaServisi.cs | `VizitLink3D.Api/Moduller/Tema/Servisler/StitchTemaServisi.cs` | Stitch MCP entegrasyonu, DESIGN.md parse |

### Kontrolcüler

| Dosya | Yol | Açıklama |
|---|---|---|
| TemaKontrolcu.cs | `VizitLink3D.Api/Kontrolculer/Sistem/TemaKontrolcu.cs` | `/api/tema` — tema ayarları, katalog, aktif tema seçimi |
| FirmaTemaKontrolcu.cs | `VizitLink3D.Api/Kontrolculer/Sistem/FirmaTemaKontrolcu.cs` | `/api/firma-tema` — firma bazlı tema okuma/güncelleme |

### Hub

| Dosya | Yol | Açıklama |
|---|---|---|
| TemaHub.cs | `VizitLink3D.Api/Hubs/TemaHub.cs` | SignalR tema değişimi broadcast |

### Veritabanı

| Dosya | Yol | Açıklama |
|---|---|---|
| TohumVerisi.cs | `VizitLink3D.Api/VeriTabani/TohumVerisi.cs` | Varsayılan tema verileri seed |
| FirmaTemaVeSayfaIcerigiTenant migration | `VizitLink3D.Api/Veri/Migrations/20260531191326_FirmaTemaVeSayfaIcerigiTenant.cs` | Tema-firma atama migration |

### Program.cs

| Dosya | Yol | Açıklama |
|---|---|---|
| Program.cs | `VizitLink3D.Api/Program.cs` | CokluTemaServisi, StitchTemaServisi DI kayıtları |

---

## 4. UI Kod Dosyaları

### Sayfalar

| Dosya | Yol | Açıklama |
|---|---|---|
| TemaYonetimi.razor | `VizitLink3D.UI/Pages/Admin/TemaYonetimi.razor` | Super admin tema yönetim sayfası |
| TemaYonetimi.razor.cs | `VizitLink3D.UI/Pages/Admin/TemaYonetimi.razor.cs` | Partial class (backing code) |

### Layout

| Dosya | Yol | Açıklama |
|---|---|---|
| VizitLink3DDuzen.razor | `VizitLink3D.UI/Layout/VizitLink3DDuzen.razor` | Ana layout — tema CSS yükleme, `data-tema-id` set etme |
| VizitLink3DDuzen.razor.cs | `VizitLink3D.UI/Layout/VizitLink3DDuzen.razor.cs` | Partial class |

### Bileşenler (hedef)

| Dosya | Yol | Açıklama |
|---|---|---|
| TemaSecici.razor | `VizitLink3D.UI/Bilesenler/Tema/TemaSecici.razor` | Header tema seçici dropdown (hedef) |

---

## 5. Tasarım Kaynakları

| Dosya | Yol | Açıklama |
|---|---|---|
| DESIGN.md | `tasarim/DESIGN.md` | Varsayılan tema tasarım dokümanı |
| DESIGN_aurelian.md | `tasarim/DESIGN_aurelian.md` | Aurelian Onyx teması tasarım dokümanı |

---

## 6. Kural Dosyaları

| Dosya | Yol | Açıklama |
|---|---|---|
| 13_Tema_Sablon_Sistemi.md | `AjanKurallari/13_Tema_Sablon_Sistemi.md` | Tema sistemi kuralları (1449 satır, ZORUNLU) |
| 04_CSS_Tema_Stitch.md | `AjanKurallari/04_CSS_Tema_Stitch_Entegrasyonu.md` | CSS tema ve Stitch entegrasyonu |
| 00_PROJE_BILGISI.md | `AjanKurallari/00_PROJE_BILGISI.md` | Proje konfigürasyonu (tema renkleri, fontları) |

---

## 7. Rapor ve Kanıt Dosyaları

### Kanıt Görselleri

Konum: `raporlar/tema-sistemi/kanit-gorseller/2026-06-30/`

| Dosya | Tür |
|---|---|
| admin_giris.png | Admin giriş ekranı |
| admin_giris_current.png | Mevcut admin girişi |
| admin_giris_dark.png | Karanlık admin girişi |
| admin_giris_new.png | Yeni admin girişi |
| admin_tema_klasik.png | Klasik tema admin |
| admin_tema_sablonlari.png | Tema şablonları admin |
| admin_dashboard_dark.png | Karanlık dashboard |
| admin_dashboard_new.png | Yeni dashboard |
| admin-logo-test.png | Logo test |
| anasayfa_full.png | Anasayfa tam sayfa |
| anasayfa_full_v2.png | Anasayfa v2 |
| site_anasayfa.png | Site anasayfa |
| test_admin_giris.png | Test admin girişi |
| test_admin_giris_son.png | Test admin son |
| test_admin_giris_son2.png | Test admin son2 |
| test_admin_giris_final.png | Test admin final |
| test_gold_tema.jpeg | Gold tema test |
| test_image.png | Genel test |
| test_theme_check.png | Tema kontrol |
| test_urunler_aurelian*.jpeg | Aurelian tema ürün testleri (v2, v3, v4, final) |
| test_urunler_default.jpeg | Varsayılan tema ürün test |
| test_referanslar.png | Referanslar test |
| test_ana_sayfa.png | Ana sayfa test |
| kontrol_istakip.png | İstakip kontrol |
| istakip_final.png | İstakip final |
| eksik_kontrol.png | Eksik kontrol |
| urun_yonetimi_dark.png | Karanlık ürün yönetimi |
| dashboard-test.png | Dashboard test |

### Ham Veri

Konum: `raporlar/tema-sistemi/ham-veri/2026-06-30/`

| Dosya | Açıklama |
|---|---|
| urunler_api.json | Ürünler API çıktısı |
| temp_prod2.json | Geçici üretim verisi |
| api_output.txt.err | API hata çıktısı |
| api.log | API log |
| api_startup.log | API başlatma log |
| ui.log | UI log |

---

## 8. Hedef Klasör Yapısı (Tamamlanmamış)

```
VizitLink3D.Api/wwwroot/css/temalar/
├── _sistem/                          ← ORTAK (henüz oluşturulmadı)
│   ├── ortak-bilesenler.css
│   ├── animasyon-ortak.css
│   └── efektler-ortak.css
│
├── gold/                             ← AKTİF (4 dosya var)
├── aurelian-onyx/                    ← AKTİF (Stitch import — taşınacak)
│
├── midnight-noir/                    ← placeholder (oluşturulacak)
├── marble-rose/                      ← placeholder
├── copper-bronze/                    ← placeholder
├── sage-stone/                       ← placeholder
├── ocean-azure/                      ← placeholder
├── ember-red/                        ← placeholder
├── royal-purple/                     ← placeholder
├── ivory-champagne/                  ← placeholder
└── noir-graphite/                    ← placeholder
```

---

## 9. Mevcut Izinli Tema Adları

`FirmaTemaKontrolcu.cs` içinde tanımlı:

```csharp
private static readonly HashSet<string> IzinliTemalar = new(StringComparer.OrdinalIgnoreCase)
{
    "endustri-karanlik",
    "klasik-aydinlik",
    "altin-siyah",
    "modern-gri",
    "komuta-mavi",
    "windows-11",
    "gold",
    "aurelian-onyx",
    "gold"
};
```

---

## 10. CokluTemaServisi Kataloğu

`CokluTemaServisi.cs` içinde statik olarak tanımlı:

| Ad | Başlık | Glassmorphism | Design Dosyası |
|---|---|---|---|
| aurelian-onyx | Aurelian Onyx | Var | DESIGN_aurelian.md |
| gold | Gold | Var | DESIGN.md |

> Not: Bu katalog statik — DB'den dinamik yülemeye geçiş Faz 2'de planlandı.

---

## 11. Bir Sonraki Refactor Hedefi

1. **Tema adları ve alias yapısı** — eski adları normalize et, `data-site-tema` → `data-tema-id` geçişi
2. **Admin tema kataloğunu DB'den besle** — statik `TumTemalar` listesi yerine `TemaSablonu` tablosu
3. **`_sistem/` ortak CSS klasörünü oluştur** — ortak-bilesenler, animasyon-ortak, efektler-ortak
4. **`aurelian-onyx` için manifest + CSS klasörlerini taşı** — mevcut konumundan `/wwwroot/css/temalar/{slug}/` altına
5. **TemaSecici bileşenini tamamla** — dropdown, lazy load, SignalR, premium kontrol
6. **TemaYonetimi admin sayfasını DB tabanlı yap** — tema listesi, ekleme, düzenleme, silme

---

*Versiyon: 2.0 — Haziran 2026 | Gold Banyo Tema Sistemi Dosya Haritası*
