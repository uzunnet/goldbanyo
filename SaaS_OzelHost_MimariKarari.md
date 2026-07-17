# SaaS ve Ozel Host Mimari Karari

Bu proje tek kod tabaniyla gelisecek, fakat firma bazinda iki farkli yayin modeli destekleyecek:

1. **Paylasimli host / coklu firma**
   - Tek API, tek UI, tek veritabani veya firma izole semasi.
   - Tenant domain veya `?firma=slug` ile cozulur.
   - `FirmaCozumlemeMiddleware` ve `KiraciServisi` zorunlu giris noktalaridir.

2. **Ozel host / tek firma**
   - Ayni kod tabani, firmanin kendi hostunda yayinlanir.
   - Tek firma profili aktif olur.
   - DB dosyasi veya PostgreSQL veritabani firmaya ozeldir.
   - Guncellemeler merkezi repodan surumlu paket olarak gider.

## Ana Ilke

Kod firmaya gore fork edilmez. Firma farklari:

- `3DVizitLink` sabit program adÄ±
- `FirmaProfili`
- DB kayitlari
- medya dosyalari
- tema tokenlari
- modul lisanslari
- yayin ortami ayarlari

uzerinden yonetilir.

## Gerekli Temel Parcalar

### 1. Firma Profili

`VizitLink.Api/VeriTabani/FirmaProfili.cs`

Tek firma hostunda varsayilan firma buradan gelir. Paylasimli hostta bu profil sadece ilk kurulum/default firma icindir.

### 2. Tenant Cozumleme

`FirmaCozumlemeMiddleware`

- Domain eslesirse ilgili firma.
- Sadece yerel gelistirmede `?firma=slug`.
- Bulunamazsa `FirmaProfili.Slug`.

### 3. Veri Izolasyonu

Tum firmaya ait tablolarda `FirmaId` olmalidir:

- Urun
- UrunKategori
- UrunAilesi
- MenuOgesi
- SayfaIcerigi
- Slayt
- Haber
- Proje
- Referans
- Medya
- IletisimMesaji
- Ayarlar

Ortak referans veriler:

- Dil
- RalRengi
- Sistem sabitleri
- Global paket/sablonlar

### 4. Modul Sistemi

Her firma icin modul bayraklari DB'de tutulmali:

- Blog
- Galeri
- Medya Havuzu
- AI Asistan
- 3D Goruntu
- E-Ticaret
- Coklu Dil
- PWA
- Audit Log

UI menuleri ve API yetkileri bu modul bayraklarina bakmali.

### 5. Surum ve Migration

Her yayin paketi bir surum tasir:

- Uygulama surumu: `2026.06.26.1`
- DB surumu: uygulanan migration listesi
- Firma icerik paketi surumu

Ozel hostlarda otomatik migration calisabilir, ama once DB yedegi alinmalidir.

### 6. Tema ve Icerik Paketi

Her firma icin ayri icerik paketi olmali:

- Firma bilgisi
- Slaytlar
- Menuler
- Sayfa icerikleri
- SSS
- Haberler
- Urun aileleri
- Urunler
- Kataloglar

Kalici hedef: `TohumVerisi.cs` firma ozel metinleri direkt tasimaz; `FirmaIcerikPaketi` siniflarini cagirir.

### 7. Deploy Profilleri

Her yayin icin profil:

- `PaylasimliHost`
- `OzelHost`
- `Demo`
- `Development`

Profil; DB yolunu, domainleri, CORS listesini, lisans ayarini ve aktif firma davranisini belirler.

## Yol Haritasi

1. `FirmaProfili` merkezini tamamla.
2. Eski firma ozel seedleri `FirmaIcerikPaketi` yapisina bol.
3. Firmaya ait tum entity'lerde `FirmaId` kontrolunu tamamla.
4. Admin/PWA katmaninda firma yerine `3DVizitLink` adini koru.
4. `KiraciServisi` kullanmayan sorgulari tespit et.
5. Modul bayraklarini admin panelden yonetilebilir hale getir.
6. Ozel host ve paylasimli host icin ayri appsettings profilleri olustur.
7. Surum/migration/yedek akisini otomatiklestir.
8. Yeni firma kurulum sihirbazi ekle.

## Karar

Bu proje SaaS cekirdegi gibi gelisecek. Firma farklari kod fork'u ile degil, profil + tenant + modul + icerik paketi ile yonetilecek.

