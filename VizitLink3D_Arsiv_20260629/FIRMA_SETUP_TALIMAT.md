# Firma 2 (VIZITLINK3D) - Admin Panelden Setup Talimatı

## 📋 Kullanılacak Bilgiler (goldbanyom.com.tr'den çekildi)

### Firma Bilgileri
```
Firma 1 (Mevcut): Goldbanyo
Firma 2 (Eklenecek): VIZITLINK3D (Demo)

Logo: https://www.goldbanyom.com.tr/wp-content/uploads/2020/06/goldlogo.png
```

### İletişim Bilgileri

**Üretim Tesisi:**
- Adres: Çankırı Yolu 8. km Büğdüz Mah. 24. Sok. No: 4 Akyurt Ankara
- Tel: +90 312 847 55 22 / +90 312 847 55 99 / +90 312 847 51 42
- E-posta: info@goldbanyom.com.tr

**Showroom:**
- Adres: Rüzgarlı Ege Sk. Rüzgarlı İş Merkezi No:15/23 Ankara
- Tel: +90 312 309 06 88 / +90 312 309 06 48

### Sosyal Medya
- Facebook: facebook.com/gold.banyo
- Instagram: instagram.com/gold.banyom/
- LinkedIn: linkedin.com/company/goldbanyo

### Açıklama
Türkiye'nin lider banyo mobilyası üreticisi. 35+ ülkede hizmet, 600+ satış noktası.

### Ürün Kategorileri
1. Gold Exclusive - Sofistike Modeller
2. Gold Premium - Çoklu Stil Seçenekleri
3. Gold Trend - Kanıtlanmış Favoriler
4. Gold Standart - Standartları Yeniden Tanımlayan

---

## 🔧 Admin Panelden Yapılacak Adımlar

### Adım 1: Firma 2 Oluşturma
1. Admin Panel → Firma Yönetimi
2. "Yeni Firma" butonuna tıkla
3. Aşağıdaki bilgileri gir:

```
Ad: VIZITLINK3D
Slug: VIZITLINK3D
Domain: VIZITLINK3D.local (dev) / VIZITLINK3D.example.com (prod)
Yedek Domain: www.VIZITLINK3D.local
Durum: Aktif
```

### Adım 2: Firma Ayarları
1. Firma > Ayarlar
2. Temel Bilgiler:
   - Site Başlığı: "VIZITLINK3D | Banyo Mobilyası"
   - Logo URL: /img/VIZITLINK3D-logo.svg
   - Favicon URL: /img/VIZITLINK3D-favicon.ico

3. İletişim Bilgileri:
   - E-posta: info@VIZITLINK3D.com
   - Telefon 1: +90 312 847 55 22
   - Telefon 2: +90 312 847 55 99
   - Adres: Çankırı Yolu 8. km Büğdüz Mah. 24. Sok. No: 4 Akyurt Ankara

4. Sosyal Medya:
   - Facebook: https://facebook.com/gold.banyo
   - Instagram: https://instagram.com/gold.banyom/
   - LinkedIn: https://linkedin.com/company/goldbanyo

### Adım 3: Sayfalar ve İçerik
Admin Panelden aşağıdaki sayfaları ekle:

#### Ana Sayfa
- Başlık: "VIZITLINK3D - Banyo Mobilyası"
- İçerik: Goldbanyo websitesinden kopyalanan açıklama

#### Hakkımızda
- İçerik: "Türkiye'nin lider banyo mobilyası üreticisi..."
- Uzun açıklama: 10.000 m² üretim tesisi, 35+ ülke, 600+ satış noktası

#### Ürünler
- 4 Kategori oluştur:
  1. Gold Exclusive
  2. Gold Premium
  3. Gold Trend
  4. Gold Standart

#### İletişim
- E-posta: info@VIZITLINK3D.com
- Telefon: +90 312 847 55 22
- Adres: Çankırı Yolu 8. km...

### Adım 4: Menü Yapılandırması
Üst Menü (Header):
- Ana Sayfa → /
- Hakkımızda → /hakkimizda
- Ürünler → /urunler
  - Gold Exclusive → /urunler/exclusive
  - Gold Premium → /urunler/premium
  - Gold Trend → /urunler/trend
  - Gold Standart → /urunler/standart
- İletişim → /iletisim

Alt Menü (Footer):
- Hakkımızda → /hakkimizda
- Gizlilik Politikası → /gizlilik
- Kullanım Şartları → /sartlar
- İletişim → /iletisim

### Adım 5: Tema Ayarları
- Site Teması: Modern
- Renk Paleti:
  - Ana Renk: #111111
  - İkincil Renk: #D5A642
  - Vurgu: #E8C86A

---

## 📸 Logo ve Resim Yükleme

1. Admin Panel → Medya Havuzu
2. Aşağıdaki dosyaları yükle:

```
wwwroot/medya/VIZITLINK3D/
├── logo.svg
├── logo-light.svg
├── favicon.ico
├── ana-sayfa-hero.jpg
├── urunler/
│   ├── exclusive/
│   ├── premium/
│   ├── trend/
│   └── standart/
└── referanslar/
    ├── proje-1.jpg
    └── proje-2.jpg
```

---

## ✅ Kontrol Listesi

- [ ] Firma 2 (VIZITLINK3D) oluşturuldu
- [ ] Firma Ayarları kaydedildi
- [ ] Ana Sayfa içeriği eklendi
- [ ] Hakkımızda sayfası eklendi
- [ ] Ürün kategorileri oluşturuldu
- [ ] Menüler yapılandırıldı
- [ ] Logo ve resimler yüklendi
- [ ] Tema ayarları yapıldı
- [ ] Firma 2 siteye erişim test edildi (localhost:5013?firma=VIZITLINK3D)

---

## 🔗 Multi-Tenant Tespit

### Development (Localhost)
```
http://localhost:5013/?firma=goldbanyo  → Firma 1
http://localhost:5013/?firma=VIZITLINK3D   → Firma 2
```

### Production (Domain)
```
https://goldbanyom.com.tr    → Firma 1 (Domain mapping)
https://VIZITLINK3D.com.tr      → Firma 2 (Domain mapping)
```

---

## 📝 Notlar

- Firma 1 (Goldbanyo) mevcut ve aktif, yeni firma onun üzerindeki ayarlar ile başlayabilir
- Firma 2 için ayrı veritabanı kullanılacak: `Yedekler/db/VIZITLINK3D.db`
- Multi-tenant yapı FirmaCozumlemeMiddleware tarafından otomatik yönetilir
- Admin panelden her firma kendi içeriğini bağımsız yönetebilir
