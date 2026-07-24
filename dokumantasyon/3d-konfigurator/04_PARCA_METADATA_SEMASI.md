# 3D Konfigüratör — Parça Metadata Şeması

> **Proje:** Gold Banyo / VizitLink3D
> **Tarih:** 20 Temmuz 2026
> **Durum:** Onay Bekliyor

---

## 1. Genel Bakış

Bu doküman, 3D konfigüratördeki her parçanın **metadata yapısını** tanımlar. Metadata, parçanın fiziksel özelliklerini, hareket yeteneklerini, fiyat bilgisini ve bağımlılıklarını kapsar.

---

## 2. Veritabanı Şeması

### 2.1 Tablolar

```sql
-- 3D Ürünler (ana tablo)
CREATE TABLE Urun3D (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TenantId        UUID NOT NULL REFERENCES Tenant(Id),
    Ad              VARCHAR(200) NOT NULL,
    Slug            VARCHAR(200) NOT NULL,
    Aciklama        TEXT,
    Kategori        VARCHAR(100) NOT NULL,
    Aktif           BOOLEAN DEFAULT true,
    Sira            INTEGER DEFAULT 0,
    OlusturulmaTarihi TIMESTAMP DEFAULT NOW(),
    GuncellenmeTarihi TIMESTAMP,
    SilindiMi       BOOLEAN DEFAULT false,
    UNIQUE(TenantId, Slug)
);

-- 3D Parçalar
CREATE TABLE Parca3D (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Urun3DId        UUID NOT NULL REFERENCES Urun3D(Id),
    TenantId        UUID NOT NULL REFERENCES Tenant(Id),
    ParcaKodu       VARCHAR(50) NOT NULL,
    Ad_TR           VARCHAR(200) NOT NULL,
    Ad_EN           VARCHAR(200),
    Kategori        VARCHAR(50) NOT NULL,
    Sira            INTEGER DEFAULT 0,
    Aktif           BOOLEAN DEFAULT true,
    Gizli           BOOLEAN DEFAULT false,
    OlusturulmaTarihi TIMESTAMP DEFAULT NOW(),
    GuncellenmeTarihi TIMESTAMP,
    SilindiMi       BOOLEAN DEFAULT false,
    UNIQUE(TenantId, ParcaKodu)
);

-- Parça Konum & Boyut
CREATE TABLE ParcaGeometry (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Parca3DId       UUID NOT NULL REFERENCES Parca3D(Id) ON DELETE CASCADE,
    KonumX          DECIMAL(10,4) DEFAULT 0,
    KonumY          DECIMAL(10,4) DEFAULT 0,
    KonumZ          DECIMAL(10,4) DEFAULT 0,
    RotasyonX       DECIMAL(10,4) DEFAULT 0,
    RotasyonY       DECIMAL(10,4) DEFAULT 0,
    RotasyonZ       DECIMAL(10,4) DEFAULT 0,
    OlcumEn         DECIMAL(10,4) NOT NULL,
    OlcumBoy        DECIMAL(10,4) NOT NULL,
    OlcumDerinlik   DECIMAL(10,4) NOT NULL,
    PivotX          DECIMAL(10,4) DEFAULT 0,
    PivotY          DECIMAL(10,4) DEFAULT 0,
    PivotZ          DECIMAL(10,4) DEFAULT 0
);

-- Parça Malzeme & Renk
CREATE TABLE ParcaMalzeme (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Parca3DId       UUID NOT NULL REFERENCES Parca3D(Id) ON DELETE CASCADE,
    MalzemeTuru     VARCHAR(50) NOT NULL,
    VarsayilanRenk  VARCHAR(7) NOT NULL,
    Metalness       DECIMAL(3,2) DEFAULT 0.0,
    Roughness       DECIMAL(3,2) DEFAULT 0.5,
    Opaklik         DECIMAL(3,2) DEFAULT 1.0,
    NormalMapURL    VARCHAR(500),
    AlbedoMapURL    VARCHAR(500),
    ORMMapURL       VARCHAR(500)
);

-- Parça Renk Seçenekleri
CREATE TABLE ParcaRenkSecenekleri (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Parca3DId       UUID NOT NULL REFERENCES Parca3D(Id) ON DELETE CASCADE,
    RenkKodu        VARCHAR(7) NOT NULL,
    RenkAdi         VARCHAR(100) NOT NULL,
    EkFiyat         DECIMAL(10,2) DEFAULT 0,
    Sira            INTEGER DEFAULT 0,
    Aktif           BOOLEAN DEFAULT true
);

-- Parça Hareket Tanımı
CREATE TABLE ParcaHareket (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Parca3DId       UUID NOT NULL REFERENCES Parca3D(Id) ON DELETE CASCADE,
    HareketTuru     VARCHAR(50) NOT NULL,
    EksenX          DECIMAL(5,2) DEFAULT 0,
    EksenY          DECIMAL(5,2) DEFAULT 1,
    EksenZ          DECIMAL(5,2) DEFAULT 0,
    AciLimiti       DECIMAL(5,2) DEFAULT 90,
    MesafeLimiti    DECIMAL(10,2),
    HizMs           INTEGER DEFAULT 400,
    EaseFonksiyonu  VARCHAR(50) DEFAULT 'easeInOut',
    VarsayilanDurum VARCHAR(20) DEFAULT 'kapali',
    OzelAnimasyonURL VARCHAR(500)
);

-- Parça Fiyat Bilgisi
CREATE TABLE ParcaFiyat (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Parca3DId       UUID NOT NULL REFERENCES Parca3D(Id) ON DELETE CASCADE,
    TabanFiyat      DECIMAL(10,2) DEFAULT 0,
    ParaBirimi      VARCHAR(3) DEFAULT 'TRY',
    KDVOrani        DECIMAL(5,2) DEFAULT 20,
    IndirimOrani    DECIMAL(5,2) DEFAULT 0,
    StokKodu        VARCHAR(50),
    StokMiktari     INTEGER
);

-- Parça Bağımlılıkları
CREATE TABLE ParcaBagimlilik (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    KaynakParcaId   UUID NOT NULL REFERENCES Parca3D(Id),
    HedefParcaId    UUID NOT NULL REFERENCES Parca3D(Id),
    BagimlilikTuru  VARCHAR(50) NOT NULL,
    kosul           JSONB,
    UNIQUE(KaynakParcaId, HedefParcaId)
);

-- Parça Referans Görselleri
CREATE TABLE ParcaGorsel (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Parca3DId       UUID NOT NULL REFERENCES Parca3D(Id) ON DELETE CASCADE,
    URL             VARCHAR(500) NOT NULL,
    Tip             VARCHAR(50) NOT NULL,
    Sira            INTEGER DEFAULT 0,
    Aktif           BOOLEAN DEFAULT true
);

-- Parça Açıklamaları (çok dilli)
CREATE TABLE ParcaAciklama (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Parca3DId       UUID NOT NULL REFERENCES Parca3D(Id) ON DELETE CASCADE,
    DilKodu         VARCHAR(5) NOT NULL,
    Baslik          VARCHAR(200),
    Aciklama        TEXT,
    UNIQUE(Parca3DId, DilKodu)
);
```

### 2.2 İndeksler

```sql
CREATE INDEX idx_urun3d_tenant ON Urun3D(TenantId) WHERE SilindiMi = false;
CREATE INDEX idx_urun3d_kategori ON Urun3D(TenantId, Kategori) WHERE SilindiMi = false;
CREATE INDEX idx_urun3d_slug ON Urun3D(TenantId, Slug) WHERE SilindiMi = false;
CREATE INDEX idx_parca3d_urun ON Parca3D(Urun3DId) WHERE SilindiMi = false;
CREATE INDEX idx_parca3d_kategori ON Parca3D(TenantId, Kategori) WHERE SilindiMi = false;
CREATE INDEX idx_parca_hareket ON ParcaHareket(Parca3DId);
CREATE INDEX idx_parca_bagimlilik_kaynak ON ParcaBagimlilik(KaynakParcaId);
CREATE INDEX idx_parca_bagimlilik_hedef ON ParcaBagimlilik(HedefParcaId);
```

---

## 3. Enum Tipleri

### 3.1 ParcaKategorisi

```csharp
public enum ParcaKategorisi
{
    AltUrun = 0,        // Ana ürune ait alt bileşenler (kapak, çekmece)
    Aksesuar = 10,      // Aksesuarlar (askı, tutamak, stop)
    Dekor = 20,         // Dekoratif elemanlar (ayna, raf)
    Mekanik = 30,       // Mekanik parçalar (menteşe, ray, piston)
    Elektrik = 40,      // Elektrik bileşenleri (LED, priz)
    Plumbing = 50,      // Tesisat bileşenleri (batarya, sifon)
    Dolgu = 60,         // Dolgu malzemeleri
    Diger = 99          // Diğer
}
```

### 3.2 HareketTuru

```csharp
public enum HareketTuru
{
    HicYok = 0,         // Sabit nesne, hareket yok
    Donme = 10,         // Eksen etrafında döndürme (kapı menteşesi)
    Kayma = 20,         // Düzlem üzerinde kaydırma (sürgülü kapak)
    Cekme = 30,         // Doğrusal çekme (çekmece)
    Katlanma = 40,      // Katlanma animasyonu (katlanır kapı)
    YukariAsagi = 50,   // Dikey hareket (kaldırılabilir panel)
    Salınım = 60,       // Salınım hareketi
    Ozel = 99           // Özel animasyon (spline yol, keyframe)
}
```

### 3.3 MalzemeTuru

```csharp
public enum MalzemeTuru
{
    MDF = 0,
    MDFC = 1,           // MDF-C (su geçirmez)
    Sunta = 5,
    Masif = 10,         // Doğal ahşap
    KompaktLaminat = 15,
    HPL = 20,           // High Pressure Laminate
    Celik = 30,
    Aluminyum = 35,
    Cam = 40,
    Akrilik = 45,
    Seramik = 50,
    Mermer = 55,
    Epoksi = 60,
    PVC = 65,
    ABS = 70,
    Diger = 99
}
```

### 3.4 BagimlilikTuru

```csharp
public enum BagimlilikTuru
{
    Gereklidir = 0,     // Bu parça olmadan ana ürün eksik
    Opsiyonel = 10,     // İsteğe bağlı
    Alternatif = 20,    // Başka bir parçanın yerine kullanılabilir
    Engelleyici = 30,   // Bu parça varken diğeri eklenemez
    Sıra = 40           // Sıralı montaj (önce A sonra B)
}
```

### 3.5 VaryantTipi

```csharp
public enum VaryantTipi
{
    Renk = 0,           // Farklı renk kombinasyonu
    Malzeme = 10,       // Farklı malzeme
    Boyut = 20,         // Farklı boyut
    Ozellik = 30,       // Farklı özellik seti
    Kombinasyon = 50    // Birden fazla faktörün kombinasyonu
}
```

---

## 4. C# Model Sınıfları

```csharp
namespace VizitLink3D.Core.Models;

public class ParcaMetadata
{
    public Guid Id { get; set; }
    public Guid Urun3DId { get; set; }
    public string ParcaKodu { get; set; } = string.Empty;
    public string Ad_TR { get; set; } = string.Empty;
    public string? Ad_EN { get; set; }
    public ParcaKategorisi Kategori { get; set; }
    public int Sira { get; set; }
    public bool Aktif { get; set; } = true;
    public bool Gizli { get; set; }
    
    // Geometry
    public ParcaGeometry Geometry { get; set; } = new();
    
    // Malzeme
    public ParcaMalzemeData Malzeme { get; set; } = new();
    
    // Hareket
    public ParcaHareketData? Hareket { get; set; }
    
    // Fiyat
    public ParcaFiyatData? Fiyat { get; set; }
    
    // Bağımlılıklar
    public List<ParcaBagimlilikData> Bagimliliklar { get; set; } = new();
    
    // Görseller
    public List<ParcaGorselData> Gorseller { get; set; } = new();
    
    // Açıklamalar
    public Dictionary<string, ParcaAciklamaData> Aciklamalar { get; set; } = new();
}

public class ParcaGeometry
{
    public Vector3 Konum { get; set; }
    public Vector3 Rotasyon { get; set; }
    public Vector3 Olculer { get; set; }
    public Vector3 Pivot { get; set; }
}

public class Vector3
{
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal Z { get; set; }
}

public class ParcaMalzemeData
{
    public MalzemeTuru Turu { get; set; }
    public string VarsayilanRenk { get; set; } = "#808080";
    public decimal Metalness { get; set; }
    public decimal Roughness { get; set; } = 0.5m;
    public decimal Opaklik { get; set; } = 1.0m;
    public string? NormalMapURL { get; set; }
    public string? AlbedoMapURL { get; set; }
    public string? ORMMapURL { get; set; }
    public List<RenkSecenekData> RenkSecenekleri { get; set; } = new();
}

public class RenkSecenekData
{
    public string RenkKodu { get; set; } = string.Empty;
    public string RenkAdi { get; set; } = string.Empty;
    public decimal EkFiyat { get; set; }
    public int Sira { get; set; }
}

public class ParcaHareketData
{
    public HareketTuru Turu { get; set; }
    public Vector3 Eksen { get; set; } = new() { Y = 1 };
    public decimal AciLimiti { get; set; } = 90;
    public decimal? MesafeLimiti { get; set; }
    public int HizMs { get; set; } = 400;
    public string EaseFonksiyonu { get; set; } = "easeInOut";
    public string VarsayilanDurum { get; set; } = "kapali";
    public string? OzelAnimasyonURL { get; set; }
}

public class ParcaFiyatData
{
    public decimal TabanFiyat { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public decimal KDVOrani { get; set; } = 20;
    public decimal IndirimOrani { get; set; }
    public string? StokKodu { get; set; }
    public int? StokMiktari { get; set; }
}

public class ParcaBagimlilikData
{
    public Guid HedefParcaId { get; set; }
    public BagimlilikTuru Turu { get; set; }
    public string? Kosul { get; set; } // JSON conditions
}

public class ParcaGorselData
{
    public string URL { get; set; } = string.Empty;
    public string Tip { get; set; } = "referans";
    public int Sira { get; set; }
}

public class ParcaAciklamaData
{
    public string? Baslik { get; set; }
    public string? Aciklama { get; set; }
}
```

---

## 5. JSON API Şeması

### 5.1 Parça Metadata JSON

```json
{
  "parcaId": "pk-001",
  "parcaKodu": "BD-GOLD-KS-001",
  "ad": {
    "tr": "Kapak Sol",
    "en": "Left Door"
  },
  "kategori": "AltUrun",
  "sira": 1,
  "aktif": true,
  "gizli": false,
  "geometry": {
    "konum": { "x": -15.0, "y": 0.0, "z": 0.0 },
    "rotasyon": { "x": 0.0, "y": 0.0, "z": 0.0 },
    "olculer": { "x": 40.0, "y": 80.0, "z": 2.0 },
    "pivot": { "x": -20.0, "y": 0.0, "z": 0.0 }
  },
  "malzeme": {
    "turu": "MDF",
    "varsayilanRenk": "#1A1A27",
    "metalness": 0.1,
    "roughness": 0.8,
    "opaklik": 1.0,
    "renkSecenekleri": [
      { "renkKodu": "#1A1A27", "renkAdi": "Siyah", "ekFiyat": 0 },
      { "renkKodu": "#C8952A", "renkAdi": "Altın", "ekFiyat": 500 },
      { "renkKodu": "#ffffff", "renkAdi": "Beyaz", "ekFiyat": 0 }
    ]
  },
  "hareket": {
    "turu": "Donme",
    "eksen": { "x": 0, "y": 1, "z": 0 },
    "aciLimiti": 120,
    "hizMs": 400,
    "easeFonksiyonu": "easeInOut",
    "varsayilanDurum": "kapali"
  },
  "fiyat": {
    "tabanFiyat": 0,
    "paraBirimi": "TRY",
    "kdvOrani": 20,
    "stokKodu": "STK-BD-KS"
  },
  "bagimliliklar": [
    {
      "hedefParcaId": "pk-002",
      "turu": "Gereklidir",
      "kosul": null
    }
  ],
  "gorseller": [
    {
      "url": "/medya/3d-modeller/t1/u1/gorseller/kapak-sol-on.jpg",
      "tip": "referans",
      "sira": 0
    }
  ],
  "aciklamalar": {
    "tr": {
      "baslik": "Sol Kapak Paneli",
      "aciklama": "MDF malzemeli, soft-close menteşeli sol kapak paneli. 18mm kalınlıkta."
    },
    "en": {
      "baslik": "Left Door Panel",
      "aciklama": "MDF material, soft-close hinged left door panel. 18mm thickness."
    }
  }
}
```

---

## 6. Doğrulama Kuralları

### 6.1 Zorunlu Alanlar

| Alan | Zorunlu | Koşul |
|------|---------|-------|
| `ParcaKodu` | Evet | Tenant içinde benzersiz, regex: `^[A-Z0-9\-]+$` |
| `Ad_TR` | Evet | 3-200 karakter |
| `Kategori` | Evet | Geçerli enum değeri |
| `Geometry.Olculer` | Evet | Tüm eksenler > 0 |
| `Malzeme.Turu` | Evet | Geçerli enum değeri |
| `Malzeme.VarsayilanRenk` | Evet | Geçerli hex rengi |

### 6.2 Koşullu Alanlar

| Alan | Koşul |
|------|-------|
| `Hareket` | `HareketTuru != HicYok` ise zorunlu |
| `Hareket.AciLimiti` | `HareketTuru == Donme` ise zorunlu |
| `Hareket.MesafeLimiti` | `HareketTuru == Kayma || Cekme` ise zorunlu |
| `Fiyat.StokKodu` | Stok takibi yapılıyorsa zorunlu |

### 6.3 İş Kuralları

```csharp
// Parça kodu benzersizliği
if (await db.Parca3D.AnyAsync(p => 
    p.TenantId == tenantId && 
    p.ParcaKodu == parcaKodu && 
    p.Id != mevcutParcaId))
{
    throw new BusinessException("Bu parça kodu zaten kullanılıyor.");
}

// Bağımlılık döngü kontrolü
if (BagimlilikDongusuVarMi(parcaId, hedefParcaId))
{
    throw new BusinessException("Bağımlılık döngüsü tespit edildi.");
}

// Pivot noktası parça sınırları içinde olmalı
if (pivot.X < 0 || pivot.X > olculer.X ||
    pivot.Y < 0 || pivot.Y > olculer.Y ||
    pivot.Z < 0 || pivot.Z > olculer.Z)
{
    throw new BusinessException("Pivot noktası parça sınırları içinde olmalıdır.");
}

// Hareket açı limiti mantıklı aralıkta olmalı
if (hareket.AciLimiti < 0 || hareket.AciLimiti > 360)
{
    throw new BusinessException("Açı limiti 0-360 arasında olmalıdır.");
}
```

---

## 7. Migration Sırası

1. `Urun3D` tablosu oluştur
2. `Parca3D` tablosu oluştur (FK: Urun3D)
3. `ParcaGeometry` tablosu oluştur (FK: Parca3D)
4. `ParcaMalzeme` tablosu oluştur (FK: Parca3D)
5. `ParcaRenkSecenekleri` tablosu oluştur (FK: Parca3D)
6. `ParcaHareket` tablosu oluştur (FK: Parca3D)
7. `ParcaFiyat` tablosu oluştur (FK: Parca3D)
8. `ParcaBagimlilik` tablosu oluştur (FK: Parca3D x2)
9. `ParcaGorsel` tablosu oluştur (FK: Parca3D)
10. `ParcaAciklama` tablosu oluştur (FK: Parca3D)
11. İndeksleri oluştur
12. Seed data (varsayılan malzemeler, renk paletleri)

---

## 8. Onay

- [ ] Ustam onayı
- [ ] DBA incelemesi
- [ ] EF Core migration testi
- [ ] Seed data onayı
