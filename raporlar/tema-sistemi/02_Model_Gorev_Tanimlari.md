# Model Gorev Tanimlari

Bu belge coklu model calismasinda dogrudan kopyalanip kullanilacak net gorev metinlerini icerir.

## Model 1

Gorev: Tema sistemi envanteri cikar.

Yapilacaklar:

- projedeki aktif tema akisini bul
- hangi `cs`, `razor`, `css`, `js`, `DESIGN.md`, `manifest.json` dosyalari gercekten tema sistemine bagli listele
- kullanilmayan veya sadece test ve kanit olan dosyalari ayir
- `gold`, `aurelian-onyx` ve yasaklanmis eski tema adlari arasindaki farki raporla

Cikti:

- kisa mimari ozet
- aktif dosya listesi
- gereksiz dosya listesi
- riskler

## Model 2

Gorev: Tema adlari ve alias yapisini normalize et.

Yapilacaklar:

- sistemde tek standart tema slug yapisi kur
- eski tema adlarini desteklemek yerine yasaklanacak alanlari tespit et
- `data-site-tema` ve gerekiyorsa `data-tema-id` kullanimini analiz et
- kirilmadan gecis plani cikar

Cikti:

- onerilen standart slug listesi
- yasaklanacak eski ad listesi
- hangi dosyada hangi degisiklik yapilmali
- uygulanacak migration sirasi

## Model 3

Gorev: 20 temaya olceklenecek tema klasor mimarisini tasarla.

Yapilacaklar:

- her tema icin zorunlu dosyalari tanimla
- `manifest`, `tokens`, `bilesenler`, `animasyonlar`, `layout` ayrimini netlestir
- ortak sistem dosyalari ile tema-ozel dosyalari ayir
- admin ve runtime kullanim akislarini dusun

Cikti:

- klasor agaci
- tema dosya sozlesmesi
- ortak ve tema-ozel katman listesi
- 20 tema icin genisleme kurallari

## Model 4

Gorev: Admin tema yonetim sistemi tasarla.

Yapilacaklar:

- admin panelde tema katalogu, onizleme, aktif etme, firma atama, taslak ve yayin akislarini tasarla
- hangi API endpointlerin lazim oldugunu belirle
- hangi DB tablolarinin lazim oldugunu belirle
- tema degisince canli guncellemenin nasil olacagini tarif et

Cikti:

- ekran listesi
- endpoint listesi
- entity ve tablo listesi
- kullanici akisi

## Model 5

Gorev: Stitch entegrasyon akisinin teknik tasarimini cikar.

Yapilacaklar:

- Stitch'ten gelen `DESIGN.md` veya benzeri ciktiyi bizim sisteme nasil alacagimizi tarif et
- ham kaynak, normalize edilmis manifest, uretilen css katmanlarini ayir
- hata durumunda fallback mantigini yaz
- admin onayli import akislarini oner

Cikti:

- Stitch import pipeline
- donusum adimlari
- fallback plani
- guvenlik ve versiyonlama notlari

## Model 6

Gorev: Tema = farkli site felsefesine uygun gorsel varyasyon standardi uret.

Yapilacaklar:

- sadece renk degil hangi katmanlarin degismesi gerektigini net tanimla
- hero, kart, navbar, footer, buton, bosluk, ikon, animasyon ve tipografi icin varyasyon eksenleri cikar
- her tema icin doldurulacak bir sablon format oner

Cikti:

- varyasyon matrisi
- tema tasarim checklisti
- tema olusturma sablonu

## Zorunlu Not

Eski tema adlari artik yeni islerde kullanilmayacak:

- `goldbanyo`
- `goldbanyo-karanlik`
- `gold-luxury-dark`
- `altin-siyah`

Yeni islerde aktif Gold Banyo tema slug'i:

- `gold`
