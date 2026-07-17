# Gold Banyo Gold Tema Planı

## Durum Özeti

Gold tema isteği birkaç kez verilmiş olmasına rağmen sonuç tutarlı üretilmemiş. İnceleme sonunda sorun tek bir yerden değil, üç ayrı kırılmadan geliyor:

1. `StitchTemaServisi` varsayılan olarak `tasarim/DESIGN.md` okuyor.
2. Mevcut `DESIGN.md` içeriği Gold Banyo yerine `Aurelian Onyx` karakteri taşıyor.
3. Canlı API başlangıcı SQLite migration kilidine takıldığı için tema düzelse bile kullanıcı tarafta sonuç görülemiyor.

## Neden Olmadı

### 1. Yanlış tasarım kaynağı

`VizitLink3D.Api/Moduller/Tema/Servisler/StitchTemaServisi.cs` önce ortak `DESIGN.md` dosyasını okuyordu. Bu dosya Gold Banyo gold showroom dili yerine koyu Aurelian / Onyx mantığında yazılmış. Sonuç olarak model her importta yanlış ruh haline çekiliyor.

### 2. Tema varlıkları ile katalog akışı kopuktu

Dosya sisteminde bulunan bazı frontend tema klasörleri veritabanı ve API katalog akışına tam yansımıyordu. Bu yüzden klasör var olsa da seçici veya doğrulama katmanı onları her zaman geçerli tema gibi görmüyordu.

### 3. Canlı test ortamı kilitleniyordu

`dotnet run --project VizitLink3D.Api/VizitLink3D.Api.csproj` çıktısında şu kök hata yakalandı:

- `SQLite Error 5: 'database is locked'`
- hata noktası: [Program.cs](I:\goldbanyo_web\VizitLink3D.Api\Program.cs:124)

Bu satırda uygulama açılışta `MigrateAsync()` çalıştırıyor. Eski veya paralel API süreçleri veritabanı kilidi bırakınca `5115` portunda API hiç ayağa kalkmıyor. Kullanıcı da bunu tema bozukluğu gibi görüyor.

## Bu Turda Uygulanan Düzeltmeler

1. `StitchTemaServisi` için varsayılan kaynak seçimi Gold Banyo lehine güncellendi.
2. Yeni ana kaynak dosyası eklendi:
   [DESIGN_goldbanyo_gold.md](I:\goldbanyo_web\tasarim\DESIGN_goldbanyo_gold.md)
3. Tema klasörlerinin dosya sistemi ile katalog akışı arasındaki bağ önceki kod düzeltmeleriyle güçlendirildi.
4. Canlı testte gerçek kök hata belgelendi: tema render kırığı değil, API başlangıç kilidi.

## Canlı Test Sonucu

04 Temmuz 2026 testinde:

- UI geliştirme sunucusu `3113` portunda dinliyor.
- API `5115` portunda ayağa kalkamadı.
- Doğrudan çalışma çıktısı `SQLite Error 5: database is locked` verdi.

Bu nedenle tema uçları canlıdan tam doğrulanamadı; önce API kilidi temiz başlangıçla çözülmeli.

## Alt Modellere Verilecek Kesin Emir

### Görev hedefi

Gold Banyo için admin temasını bozmadan, sadece ziyaretçi sitesi için `gold` temel frontend template sistemini üret ve doğrula.

### Mutlak kurallar

1. Admin teması ayrı kalacak, frontend teması ayrı çalışacak.
2. Varsayılan tasarım kaynağı olarak sadece [DESIGN_goldbanyo_gold.md](I:\goldbanyo_web\tasarim\DESIGN_goldbanyo_gold.md) kullanılacak.
3. `Aurelian`, `Onyx`, `Desadoor`, kapı/door içerikleri Gold Banyo içerik alanına taşınmayacak.
4. Tema sadece renk değil; layout, tipografi, animasyon, section ritmi, hover, kart dili ve CTA davranışıyla birlikte uygulanacak.
5. Çıktı hattı sabit olacak:
   `DESIGN_goldbanyo_gold.md -> manifest.json -> tokens.css -> bilesenler.css -> animasyonlar.css -> Blazor sayfa/component`
6. Slug temel olarak `gold` olacak. Türevler varsa `gold-light`, `gold-dark` gibi açık isimli varyantlar kullanılacak.

### Beklenen teknik çıktı

1. Ana sayfa Gold Banyo showroom diliyle yeniden hizalanacak.
2. Ürün liste ve ürün detay sayfaları animasyonlu, canlı ve katalogla uyumlu olacak.
3. Firma bazlı frontend template seçimi çalışacak.
4. Admin panel sadece atama yapacak; görünüşü etkilenmeyecek.
5. `data-site-tema` doğru değişecek.

## Sonraki Teknik Adımlar

1. `vizitlink3d.db` kilidini tutan süreçleri temizle.
2. Gerekirse startup migration davranışını geliştirme modunda tek süreç güvenli hale getir.
3. API `5115` ayağa kalkınca `/api/tema/kapsam?kapsam=site` ve `/api/firma-tema` uçlarını doğrula.
4. Tarayıcıda `3113` üzerinden ana sayfa, ürün liste ve detay sayfalarını görsel test et.
5. Son aşamada Gold Banyo Stitch referansları ile animasyon ve section geçişlerini sıkılaştır.
