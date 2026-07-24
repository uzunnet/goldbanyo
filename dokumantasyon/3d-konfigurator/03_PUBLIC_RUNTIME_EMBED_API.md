# 3D Konfigüratör — Public Runtime, Embed & API

> **Proje:** Gold Banyo / VizitLink3D
> **Tarih:** 20 Temmuz 2026
> **Durum:** Onay Bekliyor

---

## 1. Genel Bakış

Bu doküman, 3D konfigüratörün **halka açık** üç bileşenini tanımlar:

1. **Public Viewer** — Ziyaretçilerin ürün sayfalarında kullandığı interaktif 3D görüntüleyici
2. **Embed API** — Üçüncü parti sitelerin 3D konfigüratörü gömmesi
3. **Public API** — Dış servislerin model ve metadata verilerine erişimi

---

## 2. Public Viewer

### 2.1 Erişim
```
/urun/{slug}/3d           → Tam sayfa 3D görünüm
/urun/{slug}              → Ürün sayfasında嵌入 3D widget
```

### 2.2 Özellikler

| Özellik | Açıklama |
|---------|----------|
| Orbit Kontrol | Mouse/touch ile 360° döndürme |
| Zoom | Scroll/pinch ile yakınlaşma |
| Parça Seçimi | Tıklama ile parça bilgisi |
| Renk Değiştirme | Seçili parçalar için renk seçici |
| Malzeme Görünümü | PBR malzeme önizleme |
| Hareket Demo | Kapı açma, çekmece çekme gibi etkileşimler |
| Screenshot | Görüntüyü JPEG olarak kaydetme |
| Paylaş | URL + OpenGraph thumbnail |
| Tam Ekran | Tam ekran modu |
| AR Deneme | (Gelecek) WebXR ile gerçek ortama yerleştirme |

### 2.3 Performans Optimizasyonu

#### Model Yükleme Stratejisi
```
1. sayfa ilk yükleme
   └── Thumbnail göster (JPEG, ≤50KB)
   └── Three.js'i lazy-load (code splitting)

2. Kullanıcı "3D'yi Aç" tıklarsa
   └── LOD 0 (high) yükle — mesafe ≤5m
   └── Arka planda LOD 1 (medium)预备

3. Kullanıcı modeli döndürürken
   └── LOD 0 aktif
   └── Yakınlaşma >10m → LOD 1'e geç
   └── Yakınlaşma >20m → LOD 2'ye geç
```

#### Sıkıştırma
| Yöntem | Oran | Uyumluluk |
|--------|------|-----------|
| Draco | %60-80 | Tüm modern tarayıcılar |
| Meshopt | %50-70 | Chrome, Firefox, Edge |
| Brotli (HTTP) | %15-20 | CDN tarafı |

#### Cache Stratejisi
```
Model dosyaları:
  Cache-Control: public, max-age=31536000, immutable
  ETag: "{hash}"
  Vary: Accept-Encoding

Metadata:
  Cache-Control: public, max-age=3600
  ETag: "{hash}"
```

### 2.4 Public Viewer Bileşen Yapısı

```
PublicViewer.razor
├── ViewerHeader.razor          # Ürün adı, fiyat, kategori
├── Viewport3D.razor            # Three.js viewport
│   ├── OrbitControls           # Kamera kontrolü
│   ├── ParcaSecici             # Raycaster + highlight
│   ├── HareketOynatici         # Animasyon kontrolü
│   └── RenkDegistirici         # Renk paleti overlay
├── ParcaBilgiPaneli.razor      # Seçili parça detayı
├── PaylasimButonlari.razor     # Screenshot, URL paylaş
└── TamEkranButonu.razor        # Fullscreen API
```

---

## 3. Embed API (Güvenli Token Tabanlı)

> ⚠ **ÖNEMLİ:** API anahtarı ASLA frontend/istemci tarafında bulunmaz.
> Tüm embed akışı **sunucu-tanımlı token** ile çalışır. Token **key içermez**,
> 5 dakika geçerlidir ve DataProtection ile şifrelenir.

### 3.1 Mimari Akış

```
┌──────────────────┐    1. POST /api/entegrasyon/konfigurator/{slug}/embed-oturum
│  Müşteri Backendi │    Header: X-Konfigurator-Anahtari (SunucuEntegrasyonu scope)
│  (API Key saklar) │─────────────────────────────────────────────────────────────▶┐
└──────────────────┘                                                               │
        │                                                                          │
        │  2. iframeUrl döner                                                      │
        │  (time-limited token)                                                    │
        │◀─────────────────────────────────────────────────────────────────────────┘
        │
        ▼
┌──────────────────┐    3. <iframe src="/konfigurator/embed/{token}">
│  Müşteri Sitesi  │─────────────────────────────────────────────────────────────▶┐
│  (Browser)       │                                                               │
└──────────────────┘                                                               │
        │                                                                          │
        │  4. Token doğrulanır                                                     │
        │  CSP frame-ancestors + no-referrer eklenir                               │
        │  Konfigüratör HTML döndürülür                                            │
        │◀─────────────────────────────────────────────────────────────────────────┘
        │
        ▼
   Konfigüratör iframe içinde görüntülenir
```

### 3.2 Adım Adım Entegrasyon

#### Adım 1: API Anahtarı Oluşturma (Admin Panel)
- Kapsam: `SunucuEntegrasyonu` (server-to-server endpoint'ler için)
- İzin Verilen Domainler: Embed'in gösterileceği müşteri domain'i (örn. `["https://musteri-sitesi.com"]`)
- Anahtar sadece **sunucu tarafında** saklanır

#### Adım 2: Embed Token Alma (Sunucu-Sunucu)

Müşterinin backend'i, kendi API anahtarı ile aşağıdaki endpoint'i çağırır:

```
POST /api/entegrasyon/konfigurator/{slug}/embed-oturum
Header: X-Konfigurator-Anahtari: vt3d_xxx...
Body:
{
  "hedefOrigin": "https://musteri-sitesi.com"
}

Response (200):
{
  "basariliMi": true,
  "veri": {
    "iframeUrl": "/konfigurator/embed/xxx...",
    "gecerlilikSaniye": 300
  }
}
```

**Güvenlik kontrolleri (backend):**
- API anahtarı SunucuEntegrasyonu kapsamını içermeli
- API anahtarının FirmaId'si tenant izolasyonunu sağlar
- HedefOrigin, API anahtarındaki izinli domain'ler ile exact match doğrulanır
- Ürün slug'ı tenant altında var mı kontrol edilir

#### Adım 3: iframe Embed (İstemci)

Müşteri sitesi, backend'den aldığı token'ı kullanarak iframe oluşturur:

```html
<script src="https://goldbanyo.com.tr/js/konfigurator-widget.js"></script>
<div id="vizitlink3d-konfigurator"></div>
<script>
  // Token backend'den alınır — frontend'de YOK
  VizitLink3D.EmbedKonfigurator({
    embedToken: 'ALINAN_TOKEN_BURAYA',
    hedefElementId: 'vizitlink3d-konfigurator',
    genislik: '100%',
    yukseklik: '600px'
  });
</script>
```

**Widget otomatik olarak:**
- iframe sandbox attribute: `allow-scripts allow-same-origin allow-forms allow-popups`
- iframe referrerpolicy: `no-referrer`
- Token sadece iframe içindeki sessionStorage'da tutulur (sayfa kapanınca kaybolur)
- CSP frame-ancestors header'ı backend tarafından eklenir

### 3.3 Endpoint Referansı

#### Entegrasyon: POST /api/entegrasyon/konfigurator/{slug}/embed-oturum

| Başlık | Değer |
|--------|-------|
| Amaç | Embed iframe için time-limited token oluşturur |
| Kimlik | X-Konfigurator-Anahtari (SunucuEntegrasyonu scope) |
| Rate Limit | Entegrasyon (ayrı havuz) |
| Tenant | API anahtarı FirmaId'si |
| İstek | `{ "hedefOrigin": "https://musteri-sitesi.com" }` |
| Başarılı | 200: `{ iframeUrl, gecerlilikSaniye }` |
| Hatalı | 401: geçersiz anahtar, 403: scope/domain hatası |

#### Embed Sayfası: GET /konfigurator/embed/{token}

| Başlık | Değer |
|--------|-------|
| Amaç | iframe içinde açılacak konfigüratör sayfası |
| Token | DataProtection ile şifrelenmiş, 5 dk geçerli |
| CSP | `frame-ancestors {hedefOrigin};` |
| Referrer-Policy | `no-referrer` |
| X-Content-Type-Options | `nosniff` |
| Başarılı | 200: HTML sayfa (konfigüratör içeriği) |
| Hatalı | 200: HTML hata sayfası (token geçersiz/süresi dolmuş) |

#### Token Veri API: POST /api/embed/konfigurator/token/{token}/veri

| Başlık | Değer |
|--------|-------|
| Amaç | Widget JS'nin iframe içinden veri alması için |
| Token | DataProtection ile doğrulanır |
| CSP | `frame-ancestors {hedefOrigin};` |
| Başarılı | `Cevap<PublicKonfiguratorDto>` |
| Hatalı | 401/404 |

### 3.4 Güvenlik Detayları

| Kontrol | Açıklama |
|---------|----------|
| **Token içeriği** | FirmaId + UrunSlug + HedefOrigin + Nonce + ZamanDamgası |
| **Token süresi** | 5 dakika (time-limited DataProtection) |
| **Token'da KEY yok** | API anahtarı veya hash token payload'ında bulunmaz |
| **Origin doğrulama** | Referer header → token'daki HedefOrigin exact match |
| **CSP frame-ancestors** | Sadece token'daki hedef origin, fallback: `'none'` |
| **no-referrer** | Referrer-Policy: no-referrer + meta referrer content |
| **Sandbox** | `allow-scripts allow-same-origin allow-forms allow-popups` |
| **Tenant izolasyonu** | FirmaId token'a gömülür, ürün verisi ile karşılaştırılır |
| **Log yasağı** | Token/key/API anahtarı log, console, storage'a yazılmaz |
| **Insecure query yok** | API anahtarı URL/query string'de ASLA gönderilmez |

### 3.5 Hata Kodları

| HTTP | Hata | Anlamı |
|------|------|--------|
| 401 | Geçersiz API anahtarı | X-Konfigurator-Anahtari eksik/hatalı |
| 403 | API anahtarı yetkili değil | Scope veya domain uyuşmazlığı |
| 400 | Hedef origin geçersiz | URL format hatası veya path/query içeriyor |
| 200 | Token süresi dolmuş | iframe sayfasında hata mesajı gösterilir |
| 200 | Origin uyuşmazlığı | Referer ile token origin eşleşmezse |

---

## 4. Public API

### 4.1 Endpoint'ler

#### Ürün Listesi
```
GET /api/public/urunler
Query: sayfa=1&sayfaBoyutu=20&kategori=banyo-dolabi&arama=gold

Response:
{
  "basarili": true,
  "veri": {
    "elemanlar": [
      {
        "id": "guid",
        "ad": "Banyo Dolabı Gold Series",
        "slug": "banyo-dolabi-gold",
        "kategori": "Banyo Dolabı",
        "thumbnailUrl": "/medya/3d-modeller/t1/u1/thumbnail.jpg",
        "modelUrl": "/medya/3d-modeller/t1/u1/model-compressed.glb",
        "parcaSayisi": 8,
        "varyantSayisi": 3,
        "fiyat": 15999.00,
        "paraBirimi": "TRY"
      }
    ],
    "toplam": 45,
    "sayfa": 1,
    "sayfaBoyutu": 20
  }
}
```

#### Ürün Detayı
```
GET /api/public/urunler/:slug

Response:
{
  "basarili": true,
  "veri": {
    "id": "guid",
    "ad": "Banyo Dolabı Gold Series",
    "slug": "banyo-dolabi-gold",
    "aciklama": "Modern tasarım, altın detaylar...",
    "kategori": "Banyo Dolabı",
    "olculer": { "en": 120, "boy": 180, "derinlik": 45 },
    "modelUrl": "/medya/3d-modeller/t1/u1/model-compressed.glb",
    "thumbnailUrl": "/medya/3d-modellers/t1/u1/thumbnail.jpg",
    "parcalar": [
      {
        "id": "pk-001",
        "ad": "Kapak Sol",
        "konum": { "x": -15, "y": 0, "z": 0 },
        "malzeme": "MDF",
        "varsayilanRenk": "#1A1A27",
        "mevcutRenkler": ["#1A1A27", "#C8952A", "#ffffff"],
        "hareketTuru": "donme"
      }
    ],
    "varyantlar": [
      {
        "id": "v1",
        "ad": "Siyah-Altın",
        "renkSema": { "ana": "#1A1A27", "vurgu": "#C8952A" }
      }
    ]
  }
}
```

#### Parça Detayı
```
GET /api/public/urunler/:slug/parcalar/:pid

Response:
{
  "basarili": true,
  "veri": {
    "id": "pk-001",
    "ad": "Kapak Sol",
    "malzeme": "MDF",
    "renk": "#1A1A27",
    "olculer": { "en": 40, "boy": 80, "derinlik": 2 },
    "hareketTuru": "donme",
    "hareketParametreleri": {
      "eksen": [0, 1, 0],
      "aciLimiti": 120,
      "hizMs": 400
    }
  }
}
```

### 4.2 Rate Limiting

| Endpoint Kategorisi | Limit | Pencere |
|---------------------|-------|---------|
| Ürün listesi/detay | 1000 | 5 dakika |
| Model dosyası | 200 | 5 dakika |
| Parça detay | 500 | 5 dakika |
| Embed sayfası | 100 | 1 saat |

### 4.3 Caching

| Kaynak | Süre | Strateji |
|--------|------|----------|
| Ürün listesi | 5 dk | CDN cache + stale-while-revalidate |
| Ürün detay | 15 dk | CDN cache |
| Model dosyası | 1 yıl | Immutable, content-hash |
| Metadata | 1 saat | CDN cache + ETag |

---

## 5. Three.js Motor Konfigürasyonu

### 5.1 Public Viewer Varsayılanları

```javascript
const publicViewerConfig = {
  renderer: {
    antialias: true,
    alpha: false,
    powerPreference: 'high-performance',
    maxPixelRatio: 2,
    toneMapping: THREE.ACESFilmicToneMapping,
    toneMappingExposure: 1.0
  },
  camera: {
    fov: 45,
    near: 0.1,
    far: 1000,
    pozisyon: [5, 3, 5],
    hedef: [0, 0, 0]
  },
  controls: {
    enableDamping: true,
    dampingFactor: 0.05,
    minDistance: 1,
    maxDistance: 20,
    maxPolarAngle: Math.PI / 2,
    enablePan: true,
    rotateSpeed: 0.5
  },
  lighting: {
    ambient: { color: 0xffffff, intensity: 0.4 },
    directional: [
      { color: 0xffffff, intensity: 0.8, position: [5, 10, 5] },
      { color: 0xffffff, intensity: 0.3, position: [-5, 5, -5] }
    ],
    environment: 'studio' // HDRI environment map
  },
  loader: {
    draco: true,
    meshopt: true,
    maxTextureSize: 2048,
    progressive: true
  }
};
```

### 5.2 Embed Varsayılanları (Sandbox)

```javascript
const embedConfig = {
  ...publicViewerConfig,
  renderer: {
    ...publicViewerConfig.renderer,
    powerPreference: 'default', // Embed'de daha az kaynak
    maxPixelRatio: 1.5
  },
  security: {
    allowDownload: false,
    allowFullscreen: true,
    allowAR: false,
    sandbox: true
  }
};
```

---

## 6. Hata Yönetimi

| Hata Kodu | HTTP | Mesaj | Çözüm |
|-----------|------|-------|-------|
| `MODEL_BULUNAMADI` | 404 | "3D model bulunamadı" | Ürün ID kontrol |
| `MODEL_YUKLENEMEDI` | 500 | "Model yüklenirken hata oluştu" | Dosya bütünlüğü kontrol |
| `TENANT_IZINSIZ` | 403 | "Bu modele erişim yetkiniz yok" | Tenant doğrulama |
| `RATE_LIMIT` | 429 | "Çok fazla istek. Lütfen bekleyin." | Rate limit aşımı |
| `EMBED_YASAK` | 403 | "Embed izni verilmemiş" | Domain izni kontrol |
| `FORMAT_HATALI` | 400 | "Geçersiz dosya formatı" | Sadece GLB/GLTF |

---

## 7. Monitoring ve Metrikler

| Metrik | Hedef | Ölçüm |
|--------|-------|-------|
| Model yükleme süresi (p50) | ≤2s | CDN logs |
| Model yükleme süresi (p95) | ≤5s | CDN logs |
| FPS (mobil) | ≥30 | RUM (Real User Monitoring) |
| FPS (desktop) | ≥60 | RUM |
| Bounce rate (3D sayfası) | ≤40% | Analytics |
| Embed görüntülenme | Günlük | API logs |
| Hata oranı | ≤0.1% | Sentry |

---

## 8. Onay

- [ ] Ustam onayı
- [ ] API kontratı onayı
- [ ] Güvenlik incelemesi (CSP, rate limit)
- [ ] CDN yapılandırması
- [ ] Monitoring kurulumu
