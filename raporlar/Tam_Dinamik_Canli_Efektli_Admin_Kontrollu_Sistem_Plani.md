# DesaDoor Tam Dinamik Canli Efektli Admin Kontrollu Sistem Plani

> Bu raporun amaci: DesaDoor sitesinin her noktasini admin panelden yonetilebilir, canli guncellenebilir, efektli, animasyonlu ve cok dilli hale getirmek. Dusuk kod modeli bu dosyayi uygulama rehberi olarak kullanacak.

## 1. Ana Hedef

Site artik statik/fallback agirlikli olmayacak. Public site, admin panelden girilen veriye gore canli sekillenecek:

- Ust menu, alt menu, footer
- Hero slider ve tum gorseller
- Yazilar, basliklar, aciklamalar, CTA metinleri
- Animasyon tipi, gecikme, sure, yon, parallax, fade/slide/zoom efektleri
- Sayfa bolum sirasi, gorunurluk, arka plan, renk varyanti
- Dil bazli icerik
- SEO alanlari
- Butonlar, linkler, ikonlar
- Mobil/masaustu gorsel farklari
- Tema tokenlari ve admin secilebilir tasarim sablonlari

Kural: Public UI veriyi admin/API kaynakli alacak. Statik/fallback sadece API yoksa veya veri bos ise devreye girecek.

## 2. Mevcut Durumda Sorun

1. Ana sayfada veri geliyor gibi gorunuyor ama gorsel yollari kirik:
   - `https://desadoor.com.tr/upload/desadoor.png`
   - `medya/slayt/slayt-2.webp`
2. Dil degisimi hata uretiyor:
   - `MudMenu` duplicate key: `0_en`
3. Animasyon sistemi sayfa bolumlerine admin tarafindan bagli degil.
4. Resimlerin efektleri admin tarafindan secilemiyor.
5. Yazilarin animasyonlari admin tarafindan secilemiyor.
6. CSS tokenlari var ama tum sistem bunlara uymuyor.
7. `tokens.css` iki kez yukleniyor.
8. Admin dil secimi acilir menu degil, ilk 4 dil buton olarak gorunuyor.
9. Sayfa bolumleri tam dinamik degil; bazi bolumler Razor icinde sabit.
10. Public sayfa icerigi `MarkupString` ile basiliyor; guvenli ve parcalanmis blok sistemi yok.

## 3. Yeni Sistem Mantigi

Tum site su modele gore calismali:

```text
Admin Panel
  -> Sayfa
  -> Bolum
  -> Blok
  -> Icerik
  -> Medya
  -> Efekt
  -> Animasyon
  -> Dil
  -> Tema
  -> SEO
  -> Yayin Durumu

API
  -> Cevap<SayfaGorunumDto>

Public UI
  -> DinamikSayfaRenderer
  -> DinamikBolumRenderer
  -> DinamikBlokRenderer
  -> AnimasyonMotoru wrapper
  -> tokens.css
```

## 4. Veri Modeli Ihtiyaci

### 4.1 Sayfa

Sayfa modeli su alanlari tasimali:

- `Id`
- `Slug`
- `SayfaTipi`: `Anasayfa`, `Kurumsal`, `UrunListe`, `Dinamik`, `Iletisim`
- `Baslik`
- `Dil`
- `SeoBaslik`
- `SeoAciklama`
- `YayinDurumu`
- `Sira`
- `AktifMi`
- `SilindiMi`
- `OlusturulmaTarihi`
- `GuncellenmeTarihi`

### 4.2 Sayfa Bolumu

Her sayfa bolumlerden olusmali:

- `Id`
- `SayfaId`
- `BolumKodu`: `hero`, `kategori`, `surec`, `referans`, `galeri`, `cta`
- `BolumTipi`: `HeroSlider`, `KartGrid`, `MetinGorsel`, `Galeri`, `SSS`, `UrunListe`, `HtmlTemiz`
- `Baslik`
- `AltBaslik`
- `Aciklama`
- `Dil`
- `Sira`
- `AktifMi`
- `ArkaPlanTipi`: `Renk`, `Gorsel`, `Video`, `GradientToken`
- `ArkaPlanDegeri`
- `TemaVaryanti`: `Acik`, `Koyu`, `Gold`, `Cam`
- `CssSinifi`
- `AnimasyonProfiliId`
- `SilindiMi`

### 4.3 Icerik Blogu

Bolum icinde tekrarli bloklar olacak:

- `Id`
- `BolumId`
- `BlokTipi`: `Baslik`, `Paragraf`, `Buton`, `Gorsel`, `Video`, `Kart`, `IkonluMetin`, `Urun`, `Referans`
- `Baslik`
- `Metin`
- `ButonMetni`
- `ButonUrl`
- `Ikon`
- `MedyaId`
- `MedyaMobilId`
- `AltMetin`
- `Dil`
- `Sira`
- `AktifMi`
- `AnimasyonProfiliId`
- `EfektProfiliId`

### 4.4 Medya Efekti

Resimler sadece URL olmayacak; efekt ayari da admin'den gelecek:

- `EfektTipi`: `Yok`, `Parallax`, `ZoomHover`, `KenBurns`, `BlurReveal`, `MaskReveal`, `Glass`, `Tilt`
- `HoverEfekti`: `Yok`, `Zoom`, `Lift`, `Glow`, `DarkOverlay`
- `GirisAnimasyonu`: `Fade`, `SlideUp`, `SlideLeft`, `Scale`, `ClipReveal`
- `SureMs`
- `GecikmeMs`
- `EasingToken`
- `MobildeKapaliMi`

### 4.5 Animasyon Profili

Yazilar ve bolumler icin:

- `Ad`
- `AnimasyonTipi`: `FadeIn`, `SlideUp`, `SlideRight`, `ScaleIn`, `Stagger`, `RevealWords`, `Parallax`
- `SureMs`
- `GecikmeMs`
- `SiraGecikmesiMs`
- `Tetikleme`: `SayfaAcilisi`, `ScrollGorununce`, `Hover`, `Click`
- `TekrarCalissinMi`
- `MobilAktifMi`

## 5. Admin Panelde Olmasi Gereken Ekranlar

### 5.1 Sayfa Yonetimi

Admin buradan sayfa acacak:

- Slug
- Dil
- SEO
- Yayin durumu
- Sayfa bolumleri
- Onizleme
- Taslak/yayinda

### 5.2 Bolum Tasarlayici

Her sayfa icin surukle-birak siralama:

- Hero
- Urun grubu
- Metin + gorsel
- Galeri
- SSS
- CTA
- Referans
- Ozel HTML, sadece temizlenmis sekilde

### 5.3 Medya + Efekt Paneli

Her medya icin:

- Masaustu gorseli
- Mobil gorseli
- Alt metin
- Efekt secimi
- Hover efekti
- Parallax ayari
- Gorsel odak noktasi
- Yayin durumu

### 5.4 Animasyon Paneli

Hazir animasyon sablonlari:

- Luks Fade
- Endustriyel Slide
- Cam Reveal
- Yavas Parallax
- Kart Stagger
- Hero Ken Burns

Admin bu profili bolum veya bloga atayacak.

### 5.5 Tema Paneli

Admin renkleri token olarak sececek:

- Ana renk
- Ikincil renk
- Vurgu
- Arka plan
- Metin
- Admin tema varyanti
- Public tema varyanti

Kayit sonrasi:

```text
Admin tema kaydi
  -> Tema API
  -> degiskenler.css token uretimi
  -> SignalR TemaGuncellendi
  -> UI anlik yenileme
```

## 6. Public UI Render Sistemi

Yeni ana bilesenler:

- `DinamikSayfaRenderer.razor`
- `DinamikBolumRenderer.razor`
- `DinamikBlokRenderer.razor`
- `DinamikMedya.razor`
- `DinamikAnimasyonSarmalayici.razor`

Her bilesen partial class olacak. `.razor` icinde `@code` olmayacak.

### Render Akisi

```text
Slug alinir
Aktif dil alinir
api/sayfalar/gorunum/{slug}?dil=tr
SayfaGorunumDto gelir
Bolumler siraya gore render edilir
Her bolum animasyon + efekt profiliyle sarilir
Medya yollarinin varligi kontrol edilir
Eksik medya varsa admin fallback medyasi kullanilir
```

## 7. Canli Guncelleme

Admin bir icerik degistirdiginde public site F5 istemeden guncellenmeli.

Gerekenler:

- `IcerikHub`
- `TemaHub`
- `MenuHub`
- `DilHub`

Olaylar:

- `SayfaGuncellendi`
- `MenuGuncellendi`
- `TemaGuncellendi`
- `DilGuncellendi`
- `MedyaGuncellendi`

Public UI bu olaylari dinler:

- Mevcut slug tekrar cekilir
- Menu tekrar cekilir
- Tokenlar yenilenir
- `StateHasChanged` calisir

## 8. Dil Sistemi

Dil sistemi sadece arayuz metni degil, tum icerigi kapsayacak:

- Menu basliklari
- Footer linkleri
- Sayfa basligi
- Bolum basligi
- Blok metni
- Buton metni
- SEO
- Alt metin
- Admin form label

Fallback sirasi:

```text
Aktif dil
Varsayilan dil tr
Bos durum
```

Admin dil secimi:

- Public: dil dropdown veya kompakt segmented control
- Admin: acilir menu
- Desteklenen diller API'den gelmeli
- Aktif olmayan dil publicte gorunmemeli

## 9. CSS Sistemi

`tokens.css` tek giris olacak. Baska sistem CSS dosyasi `index.html` veya layout icinde tekrar yuklenmeyecek.

Yasaklar:

- Inline style
- `.razor` icinde `<style>`
- Hardcoded renk
- Hardcoded font
- Gereksiz `!important`

Gerekli tokenlar:

- `--ana-renk`
- `--ikincil-renk`
- `--vurgu-renk`
- `--arkaplan`
- `--metin`
- `--font-baslik`
- `--font-metin`
- `--bosluk-*`
- `--kose-*`
- `--animasyon-sure-*`
- `--golge-*`
- `--admin-*`
- `--desa-*` alias olarak kalabilir

## 10. Acil Duzeltmeler

Dusuk kod modeli once bunlari yapmali:

1. `DesaDoorDuzen.razor` duplicate `@key` hatasini duzelt.
   - `@key="@($"{oge.Id}_{_aktifDil}")"` yerine benzersiz key helper kullan.
   - `Id == 0` olursa `Url + Baslik + Sira + index + dil` kullan.

2. `tokens.css` tekrarini kaldir.
   - `DesaDoorDuzen.razor` icindeki `<link href="css/sistem/tokens.css">` sil.
   - `index.html` tek kaynak kalsin.

3. Admin dil secimini dropdown yap.
   - `AdminUstBanner.razor` icindeki `Take(4)` kalksin.
   - Tum aktif diller acilir menude gorunsun.

4. Kirik medya yollarini duzelt.
   - `upload/desadoor.png` yerine yerel `wwwroot/medya` altinda logo kullan.
   - `medya/slayt/slayt-2.webp` dosyasi yoksa seed/API bu yolu vermesin.

5. Hero ve bolum verilerini admin kontrollu yap.
   - Hardcoded hero metin/gorsel/CTA kalmasin.
   - Anasayfa `SayfaGorunumDto` ile gelsin.

6. Dil anahtarlarini tamamla.
   - TR/EN icin menu, footer, hero, CTA, admin label ve snackbar anahtarlari DB'de olmali.

## 11. Kabul Kriteri

Is bitmis sayilmasi icin:

1. Public ana sayfa tamamen admin verisinden render edilir.
2. Admin panelde hero gorseli degisince publicte degisir.
3. Admin panelde animasyon secilince publicte uygulanir.
4. Admin panelde dil metni degisince publicte ilgili dilde gorunur.
5. EN secilince sayfa hata vermez ve metinler Ingilizce olur.
6. Kirmizi hata bar cikmaz.
7. Kirik resim kalmaz.
8. `tokens.css` tek kez yuklenir.
9. Inline style yeni eklenmez.
10. Admin dil secimi acilir menu olarak calisir.
11. Public ve admin console error vermez.
12. `dotnet build` ve `dotnet test` basarili olur.

## 12. Dusuk Kod Modeline Net Talimat

- Bu is sadece metin cevirisi degil, tam dinamik site motoru isidir.
- Once veri modeli ve DTO'lari netlestir.
- Sonra admin formlarini yap.
- Sonra public renderer bilesenlerini bagla.
- Sonra animasyon/efekt profillerini uygula.
- Her fazdan sonra build/test calistir.
- AGENTS kurallarini ihlal eden pratik cozum ekleme.
