# Stitch Referans Paketi

Kaynak proje: https://stitch.withgoogle.com/projects/13800263520330366969

Bu klasor Gold Banyo ziyaretci sitesi icin Stitch MCP'den indirilen referans pakettir. Admin panel tema sistemi icin kullanilmaz.

## Klasorler

- `html/`: Stitch ekranlarinin indirilen HTML/Markdown ciktisi.
- `screenshots/`: Ekran gorsel referanslari.
- `screens-index.json`: Indirilen ekranlarin id, ad ve indirme durumlari.
- `README.md`: Bu not.

## Tema Ayrimi

- `aurelian-onyx`: gece/koyu frontend tema varyanti.
- `aurelian-daylight`: gunduz/acik frontend tema varyanti.
- `AdminTema`: admin panel icindir, bu paketten etkilenmemelidir.
- `SiteTema`: ziyaretci site temasini belirler.

## Uygulama Notu

Bu dosyalar dogrudan Blazor sayfasi olarak kopyalanmayacak. Once Stitch import hatti icin kaynak kabul edilir:

`DESIGN.md -> manifest.json -> tokens.css -> bilesenler.css -> animasyonlar.css -> Blazor sayfa/bilesen esleme`

Kabulde `http://localhost:3113/` ziyaretci site, `http://localhost:5115/` API olarak dogrulanir.

## Yeni Uretilen Birlesik Ekran

- `html/Gold_Banyo_Sinematik_Ana_Sayfa_Deneyimi.html`: Stitch API ile uretilen, ana sayfa varyantlarini tek canli/dinamik deneyimde birlestiren referans ekran.
- ScreenId: `8f9993b6151d41629e8616a1d16e06a7`
- Not: Bu HTML Tailwind/CDN referanslari icerebilir; uygulamaya birebir kopyalanmayacak, proje kurallarina uygun Blazor + tokens.css + sistem CSS yapisina cevrilecektir.

## Guncel Final Ana Sayfa Kaynagi

- `html/Gold_Banyo_Desadoor_Akis_Referansli_Final_Ana_Sayfa.html`: GUNCEL ANA KAYNAK. Desadoor sadece akis/sahne/animasyon referansi olarak kullanildi; icerik Gold Banyo menu ve konularina gore kurgulandi.
- `screenshots/Gold_Banyo_Desadoor_Akis_Referansli_Final_Ana_Sayfa.png`: Guncel final ekran gorseli.
- ScreenId: `b7ee3d1f41604a88821ad48cdc2cdec4`
- Uyari: `Sikca Sorulanlar`, `Merak Ettikleriniz`, kapi/door veya Desadoor metinleri alinmayacak. Ana sayfa bolumleri Gold Banyo menusuyle sinirli kalacak.
