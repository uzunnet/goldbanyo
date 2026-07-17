# Faz 0 — Ürün Bağlantısı Envanteri

**Tarih:** 2026-05-16  
**Durum:** ✅ Tam — Ana mimarisi doğru  
**Hedef:** Mevcut sistemi bozmadan bağlantı haritasını çıkarmak

---

## ✅ Ürün Merkez Mimarisi (TAMAM)

### Ana Entity'ler (23 adet)

| Entity | Dosya | Durum | Not |
|--------|-------|-------|-----|
| `Urun` | `Urunler/Urun.cs` | ✅ | İD, Ad, Slug, KategoriId, Açıklama, Aktif |
| `UrunKategori` | `Urunler/UrunKategori.cs` | ✅ | Self-referencing (alt kategori), Slug |
| `UrunYerellestirme` | `Urunler/UrunYerellestirme.cs` | ✅ | TR/EN başlık ve açıklama |
| `UrunMedya` | `Urunler/UrunMedya.cs` | ✅ | FK Urun, Medya, SiraNo |
| `UrunUcBoyutModeli` | `Urunler/UrunUcBoyutModeli.cs` | ✅ | GLB/GLTF dosyası referansı |
| `UrunUcBoyutParcasi` | `Urunler/UrunUcBoyutParcasi.cs` | ✅ | Hotspot, material, renderable part |
| `RenkKatalogu` | `Renkler/RenkKatalogu.cs` | ✅ | RAL kodu, Hex, Ad, EğimSayısı |
| `RalRengi` | `Renkler/RalRengi.cs` | ✅ | Standart RAL katalog (2000+) |
| `UrunParcaRenkSecenegi` | `Urunler/UrunParcaRenkSecenegi.cs` | ✅ | Ürün parçasına uygulanabilir renkler |
| `Malzeme` | `Malzemeler/Malzeme.cs` | ✅ | Yüzey tipi, doku, hassasiyet |
| `UrunParcaMalzemeSecenegi` | `Urunler/UrunParcaMalzemeSecenegi.cs` | ✅ | Parçaya uygulanabilir malzemeler |
| `KaplamaSecenegi` | `Malzemeler/KaplamaSecenegi.cs` | ✅ | Opsiyonel kaplama alt katmanı |
| `UrunAilesi` | `Urunler/UrunAilesi.cs` | ✅ | Ürünü gruplama (Kapı vs Kapak) |
| `UrunParcaGrubu` | `Urunler/UrunParcaGrubu.cs` | ✅ | 3D modeldeki parça gruplaması |
| `UrunParcaEslemesi` | `UrunParcaEslemesi.cs` | ✅ | 3D parça ↔ Özellik eşleşmesi |
| `MusteriKonfigurasyonu` | `Urunler/MusteriKonfigurasyonu.cs` | ✅ | Kaydedilmiş konfigürasyon JSON |
| `MusteriKonfigurasyonParcasi` | `Urunler/MusteriKonfigurasyonParcasi.cs` | ✅ | Parça başı renk/malzeme seçimi |
| `TeklifIstegi` | `Urunler/TeklifIstegi.cs` | ✅ | Müşteri teklif isteği + konfigürasyon |
| `TeklifIstegiParcasi` | `Urunler/TeklifIstegiParcasi.cs` | ✅ | Teklifin ürün parçaları |
| `UrunKonfigurasyonKurali` | `Urunler/UrunKonfigurasyonKurali.cs` | ✅ | A=Kırmızı ise B=Parlak yüzey |
| `UrunKonfigurasyonSablonu` | `Urunler/UrunKonfigurasyonSablonu.cs` | ✅ | Ön-tasarlanmış konfigürasyon setleri |
| `Medya` | `Medya/Medya.cs` | ✅ | Dosya, Hash, Tip, Kaynak |
| `MedyaKullanim` | `Medya/MedyaKullanim.cs` | ✅ | Medya → Ürün referans takibi |

---

## ✅ DTO Katmanı (Mevcut)

| DTO | Dosya | Kullanım | Durum |
|-----|-------|----------|-------|
| `KapakModeliDto` | `KapakModeliDto.cs` | Ziyaretçi (liste/detay) | ✅ |
| `UrunDto` | Frontend Models | Admin form | ⚠️ Eksik (custom DTO yok) |
| `UrunKonfiguratorDto` | ⚠️ Eksik | 3D + konfigürasyon | ❌ |
| `RenkSeciciDto` | Frontend Models | Renk listesi | ⚠️ Eksik |
| `TeklifDto` | ⚠️ Eksik | PDF teklif içeriği | ❌ |

**Eksik DTO'lar:**
- `UrunOzetDto` (liste sütunları: İD, Ad, Slug, Ana Görsel URL)
- `UrunDetayDto` (tüm özellikler + ilişkiler)
- `UrunAdminDto` (yönetim formu)
- `UrunFormDto` (form girdileri)
- `UrunKonfiguratorDto` (3D + renk + malzeme + ölçü)
- `RenkSeciciDto` (RAL + uygulanan renkler)
- `TeklifDto` (PDF içeriği)

---

## ✅ Admin Sayfaları (Mevcut)

| Sayfa | Dosya | Durum | Fonksiyon |
|-------|-------|-------|----------|
| Ürün Yönetimi | `UrunYonetimi.razor(.cs)` | ✅ | Liste, Sil, Arama |
| Ürün Formu | `UrunFormu.razor(.cs)` | ✅ Kısmi | Yeni/Düzenle (eksik bölümler) |
| Ürün Kategorisi | `UrunKategoriYonetimi.razor(.cs)` | ✅ | Tree view, CRUD |
| Renk Yönetimi | `RenkYonetimi.razor` | ⚠️ Eksik `.cs` | RAL katalog seçim |
| Malzeme Yönetimi | `MalzemeYonetimi.razor` | ⚠️ Eksik `.cs` | CRUD |
| Medya Havuzu | `MedyaGalerisi.razor(.cs)` | ✅ | Yükle, Sil, Listele |

---

## ✅ Frontend Sayfaları (Mevcut)

| Sayfa | Dosya | Durum | Not |
|-------|-------|-------|-----|
| Ürün Listesi | `UrunListesi.razor` | ⚠️ Eksik | Hardcoded mock veri |
| Ürün Detay | `KapakDetay.razor(.cs)` | ✅ | 3D + RAL entegrasyonu mevcut |
| Kategori Filtresi | `KapiModelleri.razor(.cs)` | ✅ | Kategoriye göre grup |
| 3D Viewer | `UcBoyutGoruntuleyici.razor` | ✅ | Three.js wrapper |
| Renk Seçici | `RenkSecici.razor(.cs)` | ✅ | RAL entegrasyonu |
| Teklif Formu | ⚠️ Eksik | — | Müşteri teklif isteği |
| Konfigürasyon Paylaş | ⚠️ Eksik | — | `/k/{id}` link |

---

## 🔴 Kopuk Bağlantılar ve Hardcoded Veri

### Hardcoded Ürün Verisi
- ✅ `KapakDetay.razor`: Mock `kapaklar` array (8 kapı örneği) **→ API'ye bağla**
- ✅ `AnaSayfa.razor`: One çıkan ürünler hardcoded **→ API'ye bağla**
- ⚠️ `Projeler.razor`: Proje listesi hardcoded **→ API'ye bağla**

### Hardcoded Görsel Yolları
- `wwwroot/kapaklar/` (12 PNG dosyası)
- `wwwroot/mobilya/` (5 PNG dosyası)
- `wwwroot/projeler/` (6 PNG dosyası)
- **→ Medya.cs hash referansı olmalı, dosya yolu değil**

### Hardcoded 3D Modeller
- `wwwroot/models/kapi_*.glb` (9 dosya)
- **→ UrunUcBoyutModeli.cs → Medya.cs foreign key**

### Seed Verisi
- ✅ `TohumVerisi.cs`: 40+ kapi seed (db'ye yazılmış)
- ✅ `TohumVerisi.cs`: 5 kategori seed
- ✅ `TohumVerisi.cs`: 10 renk seed (RAL)

---

## 🔍 API Endpoint Haritası (Mevcut)

| Endpoint | Kontrolcü | DTO | Durum |
|----------|-----------|-----|-------|
| `GET /api/urunler` | `UrunKontrolcu` | — | ✅ 200 döner, hardcoded liste |
| `GET /api/urunler/{id}` | `UrunKontrolcu` | — | ✅ DB'den çeker |
| `GET /api/urunler/{id}/medialar` | `UrunKontrolcu` | — | ⚠️ Eksik |
| `GET /api/urunler/{id}/3d` | `UcBoyutModelKontrolcu` | — | ✅ GLB dosya döner |
| `GET /api/renkler` | `RenkKontrolcu` | — | ✅ |
| `GET /api/malzemeler` | `MalzemeKontrolcu` | — | ⚠️ Eksik |
| `POST /api/teklif/pdf` | `TeklifKontrolcu` | — | ✅ QuestPDF ile PDF üretir |
| `POST /api/teklif/konfigurasyonu` | `TeklifKontrolcu` | — | ⚠️ Eksik |
| `POST /api/konfigurasyonlar/kaydet` | `KonfigurasyonKontrolcu` | — | ⚠️ Eksik |

---

## 📊 Bağlantı Akışı (İdeal vs Mevcut)

### İdeal Akış (Hedef)
```
1. Admin: UrunFormu → API POST /api/urunler (UrunAdminDto)
2. DB: Urun + UrunYerellestirme + UrunMedya + UrunUcBoyutModeli
3. Frontend: GET /api/urunler/slug/{slug} → UrunDetayDto
4. 3D: UrunKonfiguratorDto → RenkSecici → PDF Teklif
```

### Mevcut Akış
```
1. Admin: UrunYonetimi.razor → API POST /api/urunler/{id}
2. DB: Urun tablosu güncellenme (eksik ilişki kaydı)
3. Frontend: Hardcoded mock veri + DB'den çekilen verinin karışımı
4. 3D: UcBoyutGoruntuleyici.razor → RenkSecici → Manuel QuestPDF çağrısı
```

**Fark:** Medya, 3D, Renk ilişkileri frontend'de kurula kurula, API'ye bağlı değil.

---

## ✅ DB Yedeği

| Dosya | Boyut | Tarih | Durum |
|-------|-------|-------|-------|
| `Yedekler/db/VIZITLINK3D_20260514.bak` | 850 KB | 2026-05-14 | ✅ |

---

## 🎯 Sonraki Adım

1. **Faz 1:** `UrunOzetDto` + `UrunDetayDto` + `UrunKonfiguratorDto` oluştur
2. **API:** Eksik endpoint'leri tamamla (medya, 3D, konfigürasyon)
3. **Admin:** Form bölümlemeleri kontrol et (Temel, Medya, 3D, Renkler, Malzemeler)
4. **Frontend:** Hardcoded veriyi API çağrısıyla değiştir

---

*Envanter tamamlandı — Faz 1'e geçilme hazır.*
