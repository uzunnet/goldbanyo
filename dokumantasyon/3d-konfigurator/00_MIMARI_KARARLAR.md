# 3D Konfigüratör — Mimari Kararlar

> **Proje:** Gold Banyo / VizitLink3D
> **Tarih:** 20 Temmuz 2026
> **Durum:** Onay Bekliyor

---

## 1. Karar Özeti

| # | Karar | Seçim | Gerekçe |
|---|-------|-------|---------|
| M-01 | 3D Motor | **Three.js** devam | Mevcut yatırım, topluluk desteği, glTF/GLB natif destek, WebGPU yol haritası |
| M-02 | Çekirdek Mimari | **Shared Core** (ortak motor kütüphanesi) | Admin, public ve embed ortamları aynı motor kodunu paylaşır; bakım maliyeti düşer |
| M-03 | Kullanım Ayrımı | **Admin Studio / Public Viewer / Embed API** | Her ortamın sorumluluğu, izni ve performans profili farklıdır |
| M-04 | Model Depolama | **Central Model Storage + Hybrid** | Merkezi model havuzu (TENANT bazlı izole) + istemci tarafı lazy-load; bant genişliği ve cache dengesi |
| M-05 | Tenant Yapısı | **Domain-bazlı multi-tenant** | Her kiracı kendi domainiyle erişir; veri izolasyonu `TenantId` ile sağlanır |
| M-06 | API / MCP Ayrımı | **REST API** (genel) + **MCP Protocol** (AI asistan) | Public tüketim REST, AI asistan MCP üzerinden entegre çalışır |
| M-07 | Ürün Tipi Hareket | **Genel Ürün Tipi Hareket Yetenekleri** | Sadece banyo değil; kapı, dolap, tezgah gibi tüm mobilya tiplerini kapsayan esnek sistem |
| M-08 | Admin Metadata | **Zengin Metadata Editörü** | Parçalar için konum, malzeme, renk, fiyat, ölçü, bağımlılık gibi metadatalar admin panelinden yönetilir |

---

## 2. Mimari Detay

### 2.1 Three.js Motoru

```
┌─────────────────────────────────────────┐
│            THREE.WebGLRenderer           │
│  ┌───────────┐  ┌────────────────────┐  │
│  │  Sahne     │  │  Yükleme (GLTF)   │  │
│  │  Yönetimi  │  │  Draco + Meshopt  │  │
│  └───────────┘  └────────────────────┘  │
│  ┌───────────┐  ┌────────────────────┐  │
│  │  Işık      │  │  Kamera Kontrol   │  │
│  │  Sistemi   │  │  OrbitControls    │  │
│  └───────────┘  └────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │  Raycaster — Parça Seçim / Hover │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

**Teknik Gerekçeler:**
- glTF 2.0/GLB natif destek (draco + meshopt sıkıştırma)
- WebGPU geçişine hazır (`WebGPURenderer` deneysel)
- PBR malzeme sistemi (metallic-roughness workflow)
- Ormansızlaştırma (instancing) desteği
-Post-processing zinciri (SSAO, bloom, outline)

### 2.2 Shared Core — Ortak Motor Kütüphanesi

```
vizitlink3d.core/
├── engine/           # Three.js sarmalayıcı, sahne, kamera, ışık
├── loader/           # GLTF/GLB yükleme, Draco, meshopt
├── interaction/      # Raycaster, parça seçimi, hover,拖拽
├── animation/        # Geçiş animasyonları, parçalı hareket
├── metadata/         # Parça metadata okuma/yazma
├── export/           # Screenshot, video, glTF export
└── types/            # Ortak TypeScript/C# tipleri
```

**Paylaşım Stratejisi:**
- `vizitlink3d.core` NuGet paketi (backend) + npm paketi (frontend) olarak yayınlanır
- Admin Studio, Public Viewer ve Embed API — aynı paketi farklı yapılandırmalarla kullanır
- Sürüm uyumsuzluğu: semantik versioning (`MAJOR.MINOR.PATCH`)

### 2.3 Admin Studio / Public Viewer / Embed API Ayrımı

| Katman | Erişim | Yetki | Performans Profili |
|--------|--------|-------|-------------------|
| **Admin Studio** | `/admin/studio` | Yetkili kullanıcı (JWT) | Tam metadata düzenleme, model yükleme, sahne oluşturma, test |
| **Public Viewer** | `/urun/:slug/3d` | Herkes (oturumsuz) | Okunabilir, optimizasyonlu, CDN cache'li, lazy-load |
| **Embed API** | `<iframe>` / JS SDK | Üçüncü parti siteler | Sandbox, CSP kısıtlı, rate-limit'li, minimal UI |

### 2.4 Central Model Storage + Hybrid

```
┌─────────────────────────────────────────────┐
│           Merkezi Model Deposu               │
│  wwwroot/medya/3d-modeller/{tenant}/         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ .glb     │  │ .glb     │  │ .glb     │  │
│  │ (ana     │  │ (varyant │  │ (parça   │  │
│  │  model)  │  │  A)      │  │  seti)   │  │
│  └──────────┘  └──────────┘  └──────────┘  │
└─────────────────────┬───────────────────────┘
                      │
         ┌────────────┼────────────┐
         ▼            ▼            ▼
   ┌──────────┐ ┌──────────┐ ┌──────────┐
   │  Admin   │ │  Public  │ │  Embed   │
   │  Studio  │ │  Viewer  │ │  API     │
   │  (PLL)   │ │  (CDN)   │ │  (Cache) │
   └──────────┘ └──────────┘ └──────────┘
```

**Hybrid Stratejisi:**
- **Sunucu tarafı:** Model dosyaları merkezi depoda, CDN üzerinden sunulur
- **İstemci tarafı:** Three.js üzerinde lazy-load, LOD (Level of Detail), progressive mesh
- **Cache:** Tarayıcı cache (immutable headers) + uygulama içi LRU cache
- **Bant genişliği:** Draco/meshopt sıkıştırma ile %60-80 küçültme

### 2.5 Tenant (Multi-Tenant)

```
domain → tenant tespit → {TenantId}
    │
    ├── Model isolasyonu: /3d-modeller/{tenantId}/
    ├── Metadata izolasyonu: DB'de TenantId filtresi
    ├── Tema izolasyonu: Tenant-specific tema tokenları
    └── Rate limit: Tenant bazlı API kotaları
```

**İzolasyon Katmanları:**
1. **Dosya sistemi:** Her tenant kendi model klasöründe
2. **Veritabanı:** Tüm sorgularda `WHERE TenantId = @current`
3. **Cache:** Tenant-namespace'li cache key'leri
4. **API:** JWT claim'inden `TenantId` çıkarılır; her endpoint'te doğrulanır

### 2.6 API / MCP Ayrımı

| Protokol | Kullanım | Örnek |
|----------|----------|-------|
| **REST API** | Genel CRUD, model listesi, metadata okuma | `GET /api/3d/urunler/{id}` |
| **MCP Protocol** | AI asistan entegrasyonu,自然 dil sorguları | "Banyo dolabını siyah yap" → MCP → metadata güncelle |
| **WebSocket (SignalR)** | Gerçek zamanlı güncelleme, admin paneli canlı senkron | Admin değişikliği → Public viewer'a anlık yansıma |

### 2.7 Genel Ürün Tipi Hareket Yetenekleri

Sistem sadece banyo mobilyası değil, **tüm mobilya tiplerini** destekler:

| Ürün Tipi | Hareket Yeteneği | Açıklama |
|-----------|-----------------|----------|
| **Kapı** | Açma/kapama, sürgülü, katlanır | Menteşe pivot noktası, ray üzerinde kayma |
| **Dolap** | Kapak açma, çekmece çekme, sürgülü | İç raf görünürlüğü, lamba senaryosu |
| **Tezgah** | Lavabo yerleşimi, batarya konumu | Modular bileşen birleştirme |
| **Ayna** | Çerçeve seçimi, aydınlatma efekti | LED şerit animasyonu |
| **Raf** | Modüler genişletme, askı | Snap-to-grid yerleşim |
| **Banyo dolabı** | Kapak + çekmece + ayna kombinasyonu | Konfigüratörde çok parçalı |

**Hareket Sistemi Mimarisi:**
```
HareketTuru enum:
  ├── HicYok           // Sabit nesne
  ├── Donme             // Eksen etrafında döndürme (kapı menteşesi)
  ├── Kayma             // Düzlem üzerinde kaydırma (sürgülü kapak)
  ├── Cekme             // Doğrusal çekme (çekmece)
  ├── Katlanma          // Katlanma animasyonu (katlanır kapı)
  └── Ozel              // Özel animasyon (spline yol, keyframe)
```

Her parça için `HareketParametreleri` tanımlanır:
- `Eksen`: Hareket ekseni (Vector3)
- `AçıLimiti`: Maksimum açı/derece
- `Hız`: Animasyon süresi (ms)
- `EaseFonksiyonu`: easing tipi (easeInOut, spring, vb.)

### 2.8 Admin Metadata Düzenleyici

Admin Studio'da her3D parça için şu metadatalar düzenlenir:

| Alan | Tip | Zorunlu | Açıklama |
|------|-----|---------|----------|
| `ParcaAdi` | string | Evet | Görünür ad (TR/EN) |
| `ParcaKodu` | string | Evet | Benzersiz SKU/ kod |
| `Kategori` | enum | Evet | Alt ürün, aksesuar, dekor, mekanik |
| `Konum` | Vector3 | Evet | Ana modele göre offset |
| `Rotasyon` | Quaternion | Hayır | Varsayılan döndürme |
| `Olculer` | Bounds | Evet | Bounding box (en, boy, yükseklik) |
| `Malzeme` | enum | Evet | MDF, ahşap, metal, cam, akrilik |
| `Renk` | hex/enum | Evet | Varsayılan renk + renk paleti |
| `Fiyat` | decimal | Hayır | Ek fiyat bilgisi |
| `StokKodu` | string | Hayır | Stok takip kodu |
| `HareketTuru` | enum | Evet | Hareket yeteneği |
| `HareketParametreleri` | JSON | Koşullu | Hareket detayları |
| `Gizlilik` | bool | Hayır | Admin-only görünür mü? |
| `VaryantBagimliligi` | Guid[] | Hayır | Hangi varyantlarda görünür |
| `Aciklama` | string | Hayır | Detaylı açıklama |
| `Gorseller` | URL[] | Hayır | 2D referans görselleri |

---

## 3. Reddedilen Alternatifler

| Alternatif | Red Sebebi |
|-----------|------------|
| Unity WebGL | Ağır payload (~5MB), lisans kısıtlamaları, mobil performans sorunları |
| Babylon.js | Three.js'e göre daha küçük topluluk, Gold Banyo ekibinin Three.js deneyimi |
| Custom Canvas 2D | 3D perspektif yok, gölge/ışık desteği yok |
| Sunucu taraflı render (headless) | Yüksek maliyet, gecikme, gerçek zamanlı etkileşim imkansız |
| Sadece banyo odaklı | Gelecekte kapı/dolap ekleneceği için genel yapı gerekli |

---

## 4. Bağımlılıklar

| Bağımlılık | Versiyon | Amaç |
|-----------|----------|------|
| Three.js | ^0.170+ | 3D motor |
| glTF-Transform | ^4+ | glTF manipülasyonu |
| @dimforge/rapier3d | ^0.14+ | Fizik motoru (isteğe bağlı, parçalı montaj) |
| NuGet: VizitLink3D.Core | 1.x | Backend shared core |
| npm: @vizitlink3d/core | 1.x | Frontend shared core |

---

## 5. Onay

- [ ] Ustam onayı
- [ ] Teknik ekibin incelemesi
- [ ] Performans testi planı
- [ ] Güvenlik gözden geçirmesi
