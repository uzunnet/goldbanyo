# 3D Konfigüratör — Admin Studio

> **Proje:** Gold Banyo / VizitLink3D
> **Tarih:** 20 Temmuz 2026
> **Durum:** Onay Bekliyor

---

## 1. Genel Bakış

Admin Studio, Gold Banyo yönetim panelinin içinde yer alan **3D model yönetim arayüzüdür**. Yetkili kullanıcılar bu arayüz üzerinden:

- 3D model (GLB/GLTF) yükler
- Parçaları seçer ve metadatalarını düzenler
- Hareket parametrelerini ayarlar
- Renk/malzeme tanımlar
- Varyantlar oluşturur
- Screenshot/video alır
- Public görünümü test eder

### Erişim Yolu
```
/admin/studio                    → Ana sayfa (ürün listesi)
/admin/studio/urun/:id           → Tek ürün editörü
/admin/studio/urun/:id/parca/:pid → Parça metadata editörü
/admin/studio/test/:id           → Public görünüm test modu
```

### Yetki Gereksinimleri
- **Admin Studio erişimi:** `Rol == "Admin" || Rol == "Editör"`
- **Model yükleme:** `Yetki == "ModelYukleme"`
- **Metadata düzenleme:** `Yetki == "MetadataDuzenleme"`
- **Silme:** `Yetki == "ModelSilme"` (soft delete)

---

## 2. Sayfa Bileşenleri

### 2.1 Ürün Listesi Sayfası (`/admin/studio`)

```
┌─────────────────────────────────────────────────────┐
│  Admin Studio — 3D Model Yönetimi                    │
├─────────────────────────────────────────────────────┤
│  [+ Yeni Ürün]  [Filtre: Tüm Kategoriler ▾]  [Ara] │
├─────────────────────────────────────────────────────┤
│  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐          │
│  │ 📦  │ │ 📦  │ │ 📦  │ │ 📦  │ │ 📦  │          │
│  │Ürün1│ │Ürün2│ │Ürün3│ │Ürün4│ │Ürün5│          │
│  │3 par│ │5 par│ │2 par│ │8 par│ │4 par│          │
│  │ ✏️  │ │ ✏️  │ │ ✏️  │ │ ✏️  │ │ ✏️  │          │
│  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘          │
│                                                     │
│  Sayfa 1/3  ‹ 1 2 3 ›                              │
└─────────────────────────────────────────────────────┘
```

**Özellikler:**
- Grid görünümü (varsayılan) + Liste görünümü
- Sürükle-bırak ile sıralama
- Toplu iş: Sil, Kategori değiştir, Export
- Arama: Ürün adı, kodu, kategorisine göre

### 2.2 Ürün Editörü (`/admin/studio/urun/:id`)

```
┌──────────────────────────────────────────────────────┐
│  Ürün: [Banyo Dolabı Gold Series]  [Kaydet] [Sil]    │
├──────────────────────┬───────────────────────────────┤
│                      │  Ürün Bilgileri                │
│                      │  ┌─────────────────────────┐  │
│     3D VIEWPORT      │  │ Ad: [Banyo Dolabı     ] │  │
│                      │  │ Kod: [BD-GOLD-001     ] │  │
│   ┌──────────┐       │  │ Kategori: [Banyo Dolabı]│  │
│   │  3D Model│       │  │ Aktif: [✓]              │  │
│   │  (orbit) │       │  └─────────────────────────┘  │
│   │          │       │                               │
│   └──────────┘       │  Parça Listesi                │
│                      │  ┌─────────────────────────┐  │
│  [Orbit] [Zoom]      │  │ 1. Kapak Sol     [✏️][🗑]│  │
│  [Reset] [Screenshot]│  │ 2. Kapak Sağ     [✏️][🗑]│  │
│                      │  │ 3. Çekmece Üst   [✏️][🗑]│  │
│  Parça: [Seçili: 0]  │  │ 4. Çekmece Alt   [✏️][🗑]│  │
│                      │  │ 5. Ayna           [✏️][🗑]│  │
│                      │  └─────────────────────────┘  │
│                      │  [+ Yeni Parça]               │
│                      │  [+ Model Yeniden Yükle]       │
├──────────────────────┴───────────────────────────────┤
│  [Varyantlar]  [Test Modu]  [AI Asistan]             │
└──────────────────────────────────────────────────────┘
```

### 2.3 Parça Metadata Editörü (`/admin/studio/urun/:id/parca/:pid`)

```
┌──────────────────────────────────────────────────────┐
│  Parça: [Kapak Sol]  [Kaydet] [Sıfırla]              │
├──────────────────────┬───────────────────────────────┤
│                      │  Genel Bilgiler                │
│     3D VIEWPORT      │  ┌─────────────────────────┐  │
│   (parça highlight)  │  │ Parça Adı:              │  │
│                      │  │   TR: [Kapak Sol      ] │  │
│   ┌──────────┐       │  │   EN: [Left Door      ] │  │
│   │  Highlight│       │  │ Parça Kodu: [PK-001  ] │  │
│   │  edilmiş  │       │  │ Kategori: [Kapak ▾   ] │  │
│   │  parça    │       │  └─────────────────────────┘  │
│   └──────────┘       │                               │
│                      │  Konum & Boyut                 │
│  Pivot Noktası:      │  ┌─────────────────────────┐  │
│  [x] [-15.0]         │  │ X: [-15.0] Y: [0] Z:[0]│  │
│  [y] [0.0]           │  │ Rotasyon: [0°] [0°] [0°]│  │
│  [z] [0.0]           │  │ Ölçü: 40×80×2 cm        │  │
│                      │  └─────────────────────────┘  │
│                      │                               │
│                      │  Malzeme & Renk               │
│                      │  ┌─────────────────────────┐  │
│                      │  │ Malzeme: [MDF ▾]        │  │
│                      │  │ Renk: [■ #1A1A27]       │  │
│                      │  │ Metalness: [0.1]         │  │
│                      │  │ Roughness: [0.8]         │  │
│                      │  └─────────────────────────┘  │
│                      │                               │
│                      │  Hareket                      │
│                      │  ┌─────────────────────────┐  │
│                      │  │ Tür: [Donme ▾]          │  │
│                      │  │ Eksen: [0, 1, 0]        │  │
│                      │  │ Açı Limiti: [120°]      │  │
│                      │  │ Hız: [400ms]            │  │
│                      │  │ Ease: [easeInOut ▾]     │  │
│                      │  └─────────────────────────┘  │
│                      │                               │
│                      │  Fiyat & Stok                 │
│                      │  ┌─────────────────────────┐  │
│                      │  │ Ek Fiyat: [0.00 ₺]      │  │
│                      │  │ Stok Kodu: [STK-001   ] │  │
│                      │  │ Gizli: [ ]               │  │
│                      │  └─────────────────────────┘  │
├──────────────────────┴───────────────────────────────┤
│  Bağımlılıklar: [Parça 2, Parça 3] [+ Ekle]          │
│  Açıklama: [MDF malzemeli, sol kapak paneli...]      │
│  Referans Görseller: [📎 Yükle]                      │
└──────────────────────────────────────────────────────┘
```

---

## 3. API Endpoint'leri

### 3.1 Ürün CRUD

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| `GET` | `/api/admin/studio/urunler` | Ürün listesi (sayfalı, filtreli) |
| `GET` | `/api/admin/studio/urunler/:id` | Ürün detayı + parçalar |
| `POST` | `/api/admin/studio/urunler` | Yeni ürün oluştur |
| `PUT` | `/api/admin/studio/urunler/:id` | Ürün güncelle |
| `DELETE` | `/api/admin/studio/urunler/:id` | Ürün sil (soft delete) |

### 3.2 Model Yükleme

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| `POST` | `/api/admin/studio/urunler/:id/model` | GLB/GLTF yükle (multipart) |
| `DELETE` | `/api/admin/studio/urunler/:id/model` | Model dosyasını kaldır |
| `GET` | `/api/admin/studio/urunler/:id/model/bilgi` | Model istatistikleri (boyut, parça sayısı) |

### 3.3 Parça Metadata

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| `GET` | `/api/admin/studio/urunler/:id/parcalar` | Tüm parçaları listele |
| `GET` | `/api/admin/studio/urunler/:id/parcalar/:pid` | Parça detayı |
| `POST` | `/api/admin/studio/urunler/:id/parcalar` | Yeni parça ekle |
| `PUT` | `/api/admin/studio/urunler/:id/parcalar/:pid` | Parça güncelle |
| `DELETE` | `/api/admin/studio/urunler/:id/parcalar/:pid` | Parça sil |
| `PUT` | `/api/admin/studio/urunler/:id/parcalar/sira` | Parça sırasını güncelle |

### 3.4 Varyantlar

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| `GET` | `/api/admin/studio/urunler/:id/varyantlar` | Varyantları listele |
| `POST` | `/api/admin/studio/urunler/:id/varyantlar` | Yeni varyant oluştur |
| `PUT` | `/api/admin/studio/urunler/:id/varyantlar/:vid` | Varyant güncelle |
| `DELETE` | `/api/admin/studio/urunler/:id/varyantlar/:vid` | Varyant sil |

### 3.5 Yardımcı Endpoint'ler

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| `POST` | `/api/admin/studio/screenshot` | Screenshot al (POST body: sahne ayarları) |
| `POST` | `/api/admin/studio/test-modu` | Public görünümü test et |
| `GET` | `/api/admin/studio/malzemeler` | Malzeme enum listesi |
| `GET` | `/api/admin/studio/hareket-turleri` | Hareket türü enum listesi |

---

## 4. Admin Studio Akış Diyagramı

```
Admin → Ürün Oluştur
  │
  ▼
GLB Yükle → Model Parse → Parçaları Otomatik Tespit Et
  │
  ▼
Parça Listesi Oluştur (varsayılan metadata ile)
  │
  ▼
Admin Her Parça İçin Metadata Düzenle
  ├── Konum ayarla (3D viewport'ta sürükle)
  ├── Malzeme/renk seç
  ├── Hareket parametrelerini tanımla
  ├── Fiyat bilgisi ekle
  └── Bağımlılıkları belirle
  │
  ▼
Varyant Oluştur (isteğe bağlı)
  ├── Farklı renk kombinasyonları
  ├── Farklı parça setleri
  └── Farklı fiyatlandırma
  │
  ▼
Test Modu → Public Görünümü Kontrol Et
  │
  ▼
Kaydet → DB + Dosya Sistemi Güncellenir
  │
  ▼
Public Viewer'a Yansır
```

---

## 5. Dosya Yapısı

```
wwwroot/medya/3d-modeller/
├── {tenantId}/
│   ├── {urunId}/
│   │   ├── model.glb              # Ana model
│   │   ├── model-compressed.glb   # Sıkıştırılmış (CDN)
│   │   ├── thumbnail.jpg          # Küçük resim
│   │   ├── variants/
│   │   │   ├── siyah.glb
│   │   │   └── beyaz.glb
│   │   └── screenshots/
│   │       ├── on.jpg
│   │       └── yandan.jpg
```

---

## 6. UI Bileşenleri (Razor/Blazor)

| Bileşen | Dosya | Açıklama |
|---------|-------|----------|
| `StudioAnaSayfa` | `Admin/Studio/StudioAnaSayfa.razor` | Ürün listesi |
| `UrunEditör` | `Admin/Studio/UrunEditör.razor` | Ürün +3D viewport |
| `ParcaEditör` | `Admin/Studio/ParcaEditör.razor` | Parça metadata formu |
| `ModelYukleyici` | `Admin/Studio/ModelYukleyici.razor` | Drag-drop GLB yükleme |
| `Viewport3D` | `Admin/Studio/Viewport3D.razor` | Three.js viewport |
| `HareketAyarlayici` | `Admin/Studio/HareketAyarlayici.razor` | Hareket parametreleri |
| `MalzemePaneli` | `Admin/Studio/MalzemePaneli.razor` | PBR malzeme editörü |
| `VaryantYonetici` | `Admin/Studio/VaryantYonetici.razor` | Varyant CRUD |
| `TestModu` | `Admin/Studio/TestModu.razor` | Public görünüm testi |

---

## 7. Performans Hedefleri

| Metrik | Hedef |
|--------|-------|
| Viewport açılış süresi | ≤1 saniye |
| Parça seçim tepkisi | ≤16ms |
| Metadata kaydetme | ≤500ms |
| Model yükleme (10MB) | ≤3 saniye |
| Screenshot alma | ≤2 saniye |
| Concurrent admin kullanıcı | ≥10 |

---

## 8. Onay

- [ ] Ustam onayı
- [ ] UI/UX tasarımı
- [ ] API kontratı onayı
- [ ] Güvenlik incelemesi
