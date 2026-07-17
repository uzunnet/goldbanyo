# Gold Banyo Template Sistemi Duzeltme Plani

## 1. Amac

Bu planin amaci Gold Banyo frontendini tek seferlik sayfa duzeltmesi olarak degil, kurallara uygun template sistemi olarak toparlamaktir.

Hedef:
- Gold Banyo icin ana template: `gold`
- Gold icinde iki mod: `acik` ve `koyu`
- Gelecekte en az 20 farkli frontend template eklenebilir mimari
- Adminin kendi dinamik admin tema sistemi korunacak
- Frontend firma template atamasi mevcut tema yonetimi ile baglanacak
- Stitch tasarimlari `DESIGN.md -> manifest.json -> tokens.css -> bilesenler.css -> animasyonlar.css -> Blazor sayfalar` akisi ile uygulanacak

Referans kurallar:
- `AGENTS.md`
- `AjanKurallari/00_PROJE_BILGISI.md`
- `AjanKurallari/04_CSS_Tema_Stitch_Entegrasyonu.md`
- `AjanKurallari/12_Token_Optimizasyonu_Alt_Ajan_Kullanimi.md`
- `AjanKurallari/13_Tema_Sablon_Sistemi.md`
- `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md`

## 2. Su Anki Tespitler

Son port ve tarayici kontrolunde bulunanlar:
- UI build hatasi kaynagi: `wwwroot/_content`, `wwwroot/_framework`, `.br`, `.gz` gibi publish/build artiklari kaynak klasore girmisti.
- API CORS sadece `5113` kabul ediyordu; aktif UI portu `3113` oldugu icin browser API isteklerini kesiyordu.
- Ana sayfa olmayan `/images/vizitlink3d/hero-kapak.jpg` dosyasini istiyordu.
- API startup port acmadan once seed/medya baglama yaptigi icin `5115` gec aciliyor.
- Console hatalari temizlendi; kalan uyarilar CSS `@import` sirasi ve NuGet/analyzer warning seviyesinde.

Kalici cozum:
- Build/publish artiklari kaynaktan uzak tutulacak, `.gitignore` ve denetim eklenecek.
- Portlar `00_PROJE_BILGISI.md` ile uyumlu olacak; lokal testte `3113` kullanilacaksa CORS buna izin verecek.
- Ana sayfa gorselleri gercek Gold Banyo medya dosyalarina veya API medya kayitlarina baglanacak.
- Seed idempotent ve hizli hale getirilecek; startup port bind'i gereksizce geciktirmeyecek.

## 3. Kritik Sinir: Admin Bozulmayacak

Bu plan sadece frontend site template sistemidir.

Kurallar:
- Admin panelin kendi dinamik admin temasi aynen korunacak.
- Admin CSS tokenlari frontend template tokenlari ile karistirilmayacak.
- Admin icin `admin-tema`, `yonetim`, `MudBlazor` yonetim gorunumu ayni kalacak.
- Frontend template secimi admin panelinden yapilabilir ama adminin kendi gorunumu bu secimden etkilenmez.
- Frontend root attribute: `data-tema-id`
- Geriye uyumluluk attribute: `data-site-tema`
- Admin icin ayrik attribute gerekiyorsa `data-admin-tema` veya mevcut admin mekanizmasi korunur.

## 4. Hedef Klasor Yapisi

Frontend template dosyalari:

```text
VizitLink3D.UI/wwwroot/css/temalar/
  _sistem/
    ortak-bilesenler.css
    ortak-animasyonlar.css
    ortak-efektler.css
  gold/
    manifest.json
    tokens.css
    bilesenler.css
    animasyonlar.css
    ekran-goruntusu.jpg
  aurelian-onyx/
    manifest.json
    tokens.css
    bilesenler.css
    animasyonlar.css
    ekran-goruntusu.jpg
  gelecekteki-template-01/
  ...
```

Stitch kaynaklari:

```text
Stitch_Referanslar/13800263520330366969/
  DESIGN.md
  screens-index.json
  html/
  screenshots/
  manifest-kaynak.json
```

Kural:
- `wwwroot/css/sistem/tokens.css` global sistem girisi olarak kalabilir.
- Her yeni template kendi `wwwroot/css/temalar/{slug}` klasorunde izole olur.
- Bir template baska template'in class veya tokenini override etmez.
- Aktif template sadece `:root[data-tema-id="{slug}"]` ile uygulanir.

## 5. Gold Template Tanimi

Gold tek template olacak, icinde iki mod bulunacak:

```json
{
  "slug": "gold",
  "kod": "GOLD",
  "kaynak": "stitch",
  "stitchProjeId": "13800263520330366969",
  "modlar": ["acik", "koyu"],
  "varsayilanMod": "koyu",
  "layout": "gold-birlesik-anasayfa",
  "adminTemaDegil": true
}
```

Gold template'in kapsayacagi deneyim:
- Stitch'teki acik ve koyu tasarim ayni layout, farkli renk modu olarak uygulanacak.
- Ana sayfa 4 ayri Stitch ana sayfasinin en iyi bolumlerini tek akista birlestirecek.
- Desadoor sadece akis/animasyon referansi olarak kullanilacak; icerigi alinmayacak.
- Icerik Gold Banyo sitesi, PDF/katalog ve mevcut API verisi ile sinirli olacak.
- "Sikca Sorulanlar / Merak Ettikleriniz" gibi bolumler sadece Gold Banyo menusu ve sektor icerigi ile doldurulacak.
- Kapi, mutfak, villa kapisi, Desadoor, VizitLink3D gibi eski seed/placeholder icerikleri frontendde temizlenecek.

Gold ana sayfa sirasi:
1. Cinematic hero: Gold Banyo marka, banyo mobilyasi, gold/onyx atmosfer
2. Koleksiyon bento: Exclusive, Premium, Trend, Standart
3. Zanaat/craftsmanship bolumu
4. Butik proje sureci
5. Akilli ve hareketli koleksiyon bolumu
6. Projeler ve referanslar
7. Katalog CTA
8. Bayi/global guc
9. Musteri yorumlari
10. SSS
11. Teklif/iletisim CTA

## 6. Stitch Import Hatti

Zorunlu akis:

```text
Stitch project
  -> DESIGN.md
  -> manifest.json
  -> tokens.css
  -> bilesenler.css
  -> animasyonlar.css
  -> Blazor component/page mapping
  -> browser smoke test
  -> frontend template atama
```

Asamalar:

1. `DESIGN.md` alimi
   - Stitch projesinden tema adi, ekranlar, renkler, tipografi, motion ve layout notlari cikarilir.
   - Ekranlar `screens-index.json` ile listelenir.

2. `manifest.json` normalizasyonu
   - Renk, tipografi, geometri, glassmorphism, animasyon, layout, ikon seti, modlar ve kaynak bilgisi manifestte tutulur.
   - Tema adi/aciklamasi icin `adAnahtar`, `adVarsayilanTr`, `adVarsayilanEn`, `aciklamaAnahtar` kullanilir.

3. `tokens.css` uretimi
   - Hardcoded renk/font/bosluk yok.
   - `:root[data-tema-id="gold"]` kapsami kullanilir.
   - `data-tema-mod="acik"` ve `data-tema-mod="koyu"` varyantlari ayni template icinde tanimlanir.
   - Eski token aliaslari korunur: `--vizit-*`, gerekirse `--aureli-*`.

4. `bilesenler.css`
   - Header, footer, hero, kart, bento, buton, form, SSS, katalog, referans gibi component varyasyonlari burada olur.
   - `.razor` icinde `<style>` veya inline stil yok.

5. `animasyonlar.css`
   - Scroll reveal, shimmer, gold glow, parallax, hover lift, magnetic CTA gibi tema motion presetleri burada olur.
   - JS gerekiyorsa dogrudan koda gomulmez; mevcut wrapper servisleri uzerinden calisir.

6. Blazor esleme
   - `.razor` sadece markup.
   - Is mantigi `*.razor.cs`.
   - Metinler `DilServisi.T(...)`.
   - Gorseller API/medya havuzu veya template manifestinden gelir.

## 7. StitchTemaServisi ve CokluTemaServisi Birlesimi

`StitchTemaServisi` sorumlulugu:
- Stitch kaynaklarini okur.
- `DESIGN.md` ve ekran indeksinden `TemaSablonuPaketi` uretir.
- Manifest dogrular.
- `tokens.css`, `bilesenler.css`, `animasyonlar.css` icin dosya paketi hazirlar.
- Gerekirse ekran goruntusu/thumbnail baglar.
- Hata olursa global temayi bozmaz.

`CokluTemaServisi` sorumlulugu:
- Aktif template listesini okur.
- Firmaya atanmis frontend template'i bulur.
- `gold` template icin aktif modu belirler: `acik` veya `koyu`.
- CSS dosyalarini lazy-load edecek listeyi dondurur.
- `data-tema-id`, `data-site-tema`, `data-tema-mod` attribute degerlerini verir.
- Geriye uyumluluk ve rollback kontrolunu yapar.

Birlesim kontrati:

```text
StitchTemaServisi.ImportEt(projectId)
  -> TemaSablonuPaketi
  -> CokluTemaServisi.KaydetVeyaGuncelle(paket)
  -> FirmaTemaAtama ile frontendde aktif edilir
```

Kural:
- `StitchTemaServisi` import eder.
- `CokluTemaServisi` yayinlar/atar/yukler.
- Adminin kendi tema servisi bu akistan etkilenmez.

## 8. 20 Template Icin Gelecek Havuzu

Ilk hedef 20 template slug'i:

1. `gold`
2. `aurelian-onyx`
3. `ivory-champagne`
4. `marble-rose`
5. `noir-graphite`
6. `midnight-noir`
7. `copper-bronze`
8. `sage-stone`
9. `ocean-azure`
10. `ember-red`
11. `royal-purple`
12. `pearl-minimal`
13. `industrial-steel`
14. `warm-walnut`
15. `gallery-white`
16. `monolith-black`
17. `boutique-cream`
18. `architect-grid`
19. `soft-sand`
20. `crystal-lux`

Her template ayni dosya sozlesmesine uyar:

```text
manifest.json
tokens.css
bilesenler.css
animasyonlar.css
ekran-goruntusu.jpg
```

Her template icin zorunlu fark alanlari:
- Renk paleti
- Tipografi ailesi
- Tipografi skalasi
- Kose/border sekli
- Bosluk ritmi
- Header tipi
- Footer tipi
- Hero tipi
- Kart stili
- Animasyon hizi
- Hover davranisi
- Ikon seti

Yasak:
- 20 template sadece renk degistirmis kopyalar olmayacak.
- Her biri farkli site hissi verecek.

## 9. Veritabani ve Atama Plani

Kod yazilacak fazda DB tarafinda gerekli tablolar EF migration ile eklenecek veya mevcut yapilar genisletilecek.

Onerilen kavramlar:
- `TemaSablonu`
- `TemaSablonuDosyasi`
- `FirmaTemaAtama`
- `TemaSablonuCeviri`
- `TemaImportKaydi`

Atama akisi:

```text
TemaSablonu aktif
  -> FirmaTemaAtama
  -> CokluTemaServisi aktif template'i bulur
  -> UI data-tema-id/data-tema-mod ile yuklenir
```

Admin notu:
- Admin panel sadece bu atamayi yapar.
- Adminin kendi tema gorunumu degismez.

## 10. Icerik Temizleme Plani

Gold Banyo frontendde kalacak icerikler:
- Banyo mobilyalari
- Koleksiyonlar
- Katalog/PDF urunleri
- Bayiler
- Referanslar
- Projeler
- Haber
- SSS
- Iletisim
- Akilli/hareketli koleksiyon sayfalari

Temizlenecek/ayristirilacak icerikler:
- Desadoor metinleri
- Kapi modelleri menusu ve kapak/kapi terimleri
- Mutfak/ofis/villa kapisi ornekleri
- VizitLink3D placeholder metinleri
- Sahte global proje adlari Gold Banyo verisi degilse

Kural:
- Icerik DB/API'den gelir.
- Template sadece gorsel kimlik ve layout tasir.
- CSS icinde metin uretilmez.

## 11. Teknik Riskler ve Onlemler

1. Static asset artigi
   - Risk: `_content`, `_framework`, `.br`, `.gz` kaynakta kalirsa Blazor build veya browser bozulur.
   - Onlem: `.gitignore`, CI kontrolu, kaynak `wwwroot` temizligi.

2. CORS/port uyumsuzlugu
   - Risk: UI `3113`, config `5113`, API `5115`.
   - Onlem: Lokal origin listesi configten okunur; uretimde `AllowAnyOrigin` yok.

3. Admin tema bozulmasi
   - Risk: frontend tokenlari admin CSS'ini etkiler.
   - Onlem: frontend `data-tema-id`, admin ayrik scope.

4. Eski icerik sizmasi
   - Risk: Desadoor/kapi/mutfak metinleri Gold Banyo anasayfasinda gorunur.
   - Onlem: seed/API icerik envanteri, Gold whitelist, smoke snapshot kontrolu.

5. Tema sadece renk degisimi olur
   - Risk: 20 template gercek template olmaz.
   - Onlem: manifestte layout/geometri/motion zorunlu alanlari.

6. Dil sistemi bozulur
   - Risk: hardcoded tema adi/aciklama/metin.
   - Onlem: `DilServisi.T`, tema ceviri anahtarlari, font Latin Extended kontrolu.

7. Startup yavasligi
   - Risk: API port gec acilir, testler yanlis negatif verir.
   - Onlem: seed idempotent ve opsiyonel; agir medya baglama background job olur.

8. Harici gorsel bagimliligi
   - Risk: `goldbanyom.com.tr` resmi yuklenmez.
   - Onlem: medya havuzuna lokal kopya veya fallback.

## 12. Alt Model Gorev Dagilimi

M3 Supervisor:
- Tum gorevi boler.
- AGENTS ve tema kurallarini okundugunu dogrular.
- Build/browser kanitlarini toplar.

explore:
- Mevcut tema, CSS, menu, API icerik, seed ve medya yollarini envanterler.
- Desadoor/kapi/mutfak kalintilarini raporlar.

coder-agir:
- Template mimarisini, DB entity/migration, `StitchTemaServisi` ve `CokluTemaServisi` sozlesmesini uygular.
- Lazy load, rollback, firma atama akisini kurar.

coder-hizli:
- Tekil dosya duzeltmeleri yapar: CORS, hero path, CSS import sirasi, eksik gorsel, static asset temizligi.

yazici:
- `manifest.json`, `tokens.css`, `bilesenler.css`, `animasyonlar.css` dosyalarini kurala uygun uretir.
- Tema ceviri anahtarlarini ve dokumantasyonu yazar.

QA/test modeli:
- `dotnet build`, kritik testler, browser console, network 200, desktop/mobile screenshot kontrolu yapar.
- Admin sayfasinin frontend tema degisiminden etkilenmedigini dogrular.

## 13. Kabul Kriterleri

Teknik:
- `dotnet build VizitLink3D.Api` 0 hata.
- `dotnet build VizitLink3D.UI` 0 hata.
- `http://localhost:3113/` 200.
- `http://localhost:5115/openapi/v1.json` 200.
- Browser console error yok.
- Networkte kritik API istekleri 200.
- CORS preflight `3113 -> 5115` icin 204.
- `wwwroot` icinde publish artigi `_framework`, `_content`, `.br`, `.gz` kaynak olarak kalmaz.

Tema:
- Gold `acik` ve `koyu` modlari calisir.
- Template degisimi sadece renk degil; layout, tipografi, animasyon, kart, header/footer davranisi degisir.
- `gold` template manifest, tokens, bilesenler, animasyonlar dosyalari ile izoledir.
- En az 20 template icin klasor/manifest sozlesmesi hazirdir.

Icerik:
- Gold Banyo disi Desadoor/kapi/mutfak/villa kapi placeholderlari ana sayfada gorunmez.
- Menu Gold Banyo icerigine gore akar.
- PDF/katalog ve urun kategorileri Gold Banyo verisinden gelir.

Admin:
- Admin tema sistemi bozulmaz.
- Admin panel frontend template atamasini yapabilir ama kendi temasini degistirmez.

## 14. Uygulama Sirasi

Faz 1 - Temizlik ve saglam zemin:
- Static asset artiklarini kalici olarak kaynak disina al.
- `.gitignore`/CI kontrolu ekle.
- CORS configini port kurallarina bagla.
- Eksik hero/gorsel pathlerini duzelt.

Faz 2 - Gold template paketi:
- `gold/manifest.json`
- `gold/tokens.css`
- `gold/bilesenler.css`
- `gold/animasyonlar.css`
- `gold/ekran-goruntusu.jpg`

Faz 3 - Servis sozlesmesi:
- `StitchTemaServisi` import/normalize.
- `CokluTemaServisi` listele/yukle/ata.
- `TemaHub` varsa hot reload; yoksa lazy-load + localStorage.

Faz 4 - Icerik ve sayfa esleme:
- Ana sayfa 4 Stitch tasarimini tek Gold akisa birlestir.
- Urunler, katalog, iletisim, hareketli koleksiyon, akilli koleksiyon sayfalarini Gold template classlariyla esle.

Faz 5 - 20 template altyapisi:
- Manifest sozlesmesi sabitlenir.
- 20 template placeholder kaydi acilir.
- Her yeni template icin kod degisikligi gerekmeyecek hale getirilir.

Faz 6 - Dogrulama:
- Build/test.
- Browser console/network.
- Desktop/mobile screenshot.
- Admin etkilenmiyor kontrolu.

## 15. Opencode Icin Tek Gorev Metni

Gold Banyo projesinde frontend template sistemini kur. AGENTS.md, 00_PROJE_BILGISI, 04_CSS_Tema_Stitch_Entegrasyonu, 12_Token_Optimizasyonu, 13_Tema_Sablon_Sistemi ve 99_YASAKLAR dosyalarina uy. Adminin kendi dinamik admin temasini bozma; bu is sadece frontend site template sistemi. Gold icin `gold` template olustur, icinde `acik` ve `koyu` modlari olsun. Stitch project `13800263520330366969` tasarimlarini tek ana sayfa akisi olarak birlestir; Desadoor/kapi/mutfak icerigi alma, sadece Gold Banyo icerigi ve katalog/PDF/API verisi kullan. Akis `DESIGN.md -> manifest.json -> tokens.css -> bilesenler.css -> animasyonlar.css -> Blazor sayfa/component esleme -> browser test` olacak. `StitchTemaServisi` import/normalize, `CokluTemaServisi` frontend template listeleme/yukleme/atama isini yapacak. 20+ template eklenebilir klasor/manifest sozlesmesini hazirla. Build, port, CORS, browser console ve admin etkilenmiyor kaniti ile don. Sonucu 5 satiri gecmeyecek sekilde ozetle.

## 16. Opencode Alt Model Emirleri

Asagidaki metin Opencode M3 supervisor'a tek parca verilecek. M3 bu isi alt modellere bolerek takip edecek.

```text
M3 SUPERVISOR GOREVI

Proje: I:\goldbanyo_web
Ana hedef: Gold Banyo frontendini Stitch tasarimlarina gore `gold` template olarak kurmak, acik/koyu modlari calistirmak, 20+ template mimarisini hazirlamak ve adminin kendi dinamik admin tema sistemini bozmamak.

Zorunlu ilk okuma:
1. AGENTS.md
2. AjanKurallari/00_PROJE_BILGISI.md
3. AjanKurallari/12_Token_Optimizasyonu_Alt_Ajan_Kullanimi.md
4. AjanKurallari/04_CSS_Tema_Stitch_Entegrasyonu.md
5. AjanKurallari/13_Tema_Sablon_Sistemi.md
6. AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md
7. düzeltmegold.md

Kritik sinirlar:
- Admin tema sistemi degismeyecek, admin CSS/token/sayfa davranisi frontend template seciminden etkilenmeyecek.
- Frontend template scope: `data-tema-id`, `data-site-tema`, `data-tema-mod`.
- Admin scope mevcut yapida kalacak; gerekirse `data-admin-tema` ayrimi korunacak.
- `.razor` icinde `<style>` ve `@code` yok.
- Hardcoded renk/font/bosluk yok; CSS token kullan.
- Razor metinleri `DilServisi.T(...)` ile kalacak.
- Desadoor/kapi/mutfak/villa/VizitLink3D placeholder icerigi Gold Banyo frontendine sizmayacak.
- `AllowAnyOrigin()` kullanma.
- Kaynak `wwwroot` altinda `_framework`, `_content`, `.br`, `.gz` gibi publish artiklari kalmayacak.

ALT MODEL DAGILIMI

1) explore / envanter ajan
Sahiplik: sadece okuma ve rapor.
Oku ve raporla:
- Mevcut tema/CSS dosyalari nerede?
- `StitchTemaServisi`, `CokluTemaServisi`, tema secici, layout ve admin tema dosyalari nerede?
- Ana sayfa, urunler, katalog, iletisim, hareketli koleksiyon, akilli koleksiyon sayfalari hangi dosyalarda?
- Desadoor/kapi/mutfak/villa/VizitLink3D kalintilari nerede?
- Port/CORS/static asset sorunu hangi dosyalardan etkileniyor?
Kanıt: dosya yolu + kisa bulgu. Kod yazma.

2) coder-hizli / stabilizasyon ajan
Sahiplik:
- Port/CORS lokal config
- Eksik hero/medya pathleri
- CSS import sirasi
- Kaynak `wwwroot` statik artifact temizligi ve tekrarini onleyen kontrol
Yap:
- UI 3113 ve API 5115 lokal calismasi icin gerekli uyumu sagla.
- `goldbanyo.com.tr` veya medya havuzu kaynakli gercek Gold Banyo gorsellerini kullan; olmayan path birakma.
- Build/publish artiklari kaynak klasore tekrar girmesin.
Kanıt: build 0 hata, UI 200, API openapi 200, CORS preflight 204.

3) coder-agir / mimari ajan
Sahiplik:
- `StitchTemaServisi`
- `CokluTemaServisi`
- Tema paket kontrati
- Firma frontend template atama akisi
- Lazy-load, rollback, geriye uyumluluk
Yap:
- `StitchTemaServisi.ImportEt(projectId) -> TemaSablonuPaketi` akisini kur veya mevcut servise uyarla.
- `CokluTemaServisi.KaydetVeyaGuncelle(paket)` ve frontend aktif template yukleme/atama akisini netlestir.
- `gold` template icin `acik/koyu` mod secimini destekle.
- Gelecekte 20+ template icin kod degisikligi gerektirmeyen sozlesmeyi hazirla.
- Admin tema sistemine dokunma; sadece frontend atamasi admin tarafindan yapilabilir olsun.
Kanıt: degisen dosyalar, servis kontrati, build 0 hata, admin etkilenmiyor notu.

4) yazici / template dosya ajan
Sahiplik:
- `VizitLink3D.UI/wwwroot/css/temalar/gold/manifest.json`
- `tokens.css`
- `bilesenler.css`
- `animasyonlar.css`
- `ekran-goruntusu.jpg` veya fallback thumbnail
- 20 template placeholder manifest sozlesmesi
Yap:
- Stitch project `13800263520330366969` icindeki 4 ana sayfa tasarim mantigini tek Gold ana sayfa template akisi olarak birlestir.
- Gold `acik` ve `koyu` modlarini ayni template icinde tanimla.
- Tema sadece renk degisimi olmayacak: tipografi, layout, kart, header, footer, hover, motion, bosluk ritmi farkli olacak.
- CSS icinde metin uretme; metin Razor/DilServisi tarafinda kalacak.
Kanıt: dosya listesi, token kapsam selectorleri, acik/koyu mod calisma notu.

5) sayfa-uygulama ajan
Sahiplik:
- Ana sayfa Blazor markup + partial class
- Urunler, katalog, iletisim, akilli koleksiyon, hareketli koleksiyon sayfa eslemeleri
Yap:
- Ana sayfada 4 Stitch ana sayfasinin iyi bolumlerini tek akisa birlestir.
- Sira: hero, koleksiyon bento, zanaat, proje sureci, akilli/hareketli koleksiyon, projeler/referanslar, katalog CTA, bayi/global guc, yorumlar, SSS, teklif/iletisim CTA.
- Icerik Gold Banyo API/PDF/katalog verisine gore gelsin.
- Desadoor orneginden sadece animasyon/akis ilhami al, icerik alma.
Kanıt: sayfa dosyalari, browser screenshot, console temiz.

6) QA/test ajan
Sahiplik: dogrulama.
Calistir:
- `dotnet build VizitLink3D.Api`
- `dotnet build VizitLink3D.UI`
- API: `http://localhost:5115/openapi/v1.json`
- UI: `http://localhost:3113/`
- CORS preflight: `3113 -> 5115`
- Browser console ve network
- Desktop + mobile gorunum
- Admin sayfasi frontend template seciminden etkileniyor mu kontrolu
Kabul:
- Build 0 hata.
- UI/API 200.
- Console kritik error yok.
- Network kritik istekler 200.
- Gold `acik` ve `koyu` mod calisir.
- Admin kendi temasinda kalir.
- Gold Banyo disi placeholder icerik gorunmez.

M3 RAPOR FORMATI
Her alt model sonucu 5 satiri gecmeyecek:
- Yapilan:
- Degisen:
- Kanit:
- Risk:
- Sonraki:

M3 sonunda tek karar verir: KABUL / RED / DUZELTME GEREKLI.
```

## 17. Net Dosya Sahipligi

Opencode alt modelleri ayni dosyayi paralel degistirmeyecek. Sahiplik asagidaki gibi uygulanacak.

| Ajan | Yazma Sahipligi | Okuma Serbest | Dokunmayacak |
|---|---|---|---|
| `explore` | Yok | Tum proje | Hicbir dosya degistirmez |
| `coder-hizli` | `VizitLink3D.Api/Program.cs`, `VizitLink3D.Api/appsettings*.json`, `VizitLink3D.UI/wwwroot/appsettings.json`, statik asset ignore/temizlik dosyalari | `VizitLink3D.UI/wwwroot`, `VizitLink3D.Api/wwwroot` | Ana sayfa tasarimi, tema servis mimarisi |
| `coder-agir` | `VizitLink3D.Api/Moduller/Tema/Servisler/StitchTemaServisi.cs`, `VizitLink3D.Api/Moduller/Tema/Servisler/CokluTemaServisi.cs`, `VizitLink3D.Api/Kontrolculer/Sistem/TemaKontrolcu.cs`, `VizitLink3D.Api/Kontrolculer/Sistem/FirmaTemaKontrolcu.cs`, `VizitLink3D.Api/Hubs/TemaHub.cs`, `VizitLink3D.Ortak/Modeller/Tema/*` | Tema CSS ve UI sayfalari | Admin layout/CSS gorunumunu degistirmez |
| `yazici` | `VizitLink3D.UI/wwwroot/css/temalar/gold/*`, `VizitLink3D.UI/wwwroot/css/temalar/_sistem/*`, gerekiyorsa diger 20 template `manifest.json` duzeltmeleri | UI sayfalari ve mevcut tema klasorleri | API servis dosyalari, admin sayfalari |
| `sayfa-uygulama` | `VizitLink3D.UI/Pages/AnaSayfa.razor`, `VizitLink3D.UI/Pages/AnaSayfa.razor.cs`, `VizitLink3D.UI/Pages/Urunler.razor*`, `VizitLink3D.UI/Pages/UrunDetay.razor*`, `VizitLink3D.UI/Pages/KatalogSayfasi.razor*`, `VizitLink3D.UI/Pages/Iletisim.razor*`, `VizitLink3D.UI/Pages/AkilliKoleksiyon.razor*`, `VizitLink3D.UI/Pages/HareketliKoleksiyon.razor*`, `VizitLink3D.UI/Pages/SSS.razor*`, `VizitLink3D.UI/Pages/Projeler.razor*`, `VizitLink3D.UI/Pages/Referanslar.razor*`, `VizitLink3D.UI/Bilesenler/Anasayfa/*`, `VizitLink3D.UI/Bilesenler/Stitch/*` | Tema CSS, urun/katalog DTOlari | API tema servisleri, admin sayfalari |
| `QA/test` | Yok veya sadece test raporu/screenshot klasoru | Tum proje | Uygulama kodu degistirmez |

Admin koruma listesi:
- `VizitLink3D.UI/Layout/AdminDuzen.razor`
- `VizitLink3D.UI/Layout/AdminDuzen.razor.cs`
- `VizitLink3D.UI/Pages/Admin/*`
- Adminin mevcut CSS/token dosyalari

Bu dosyalar sadece M3 onayi ile ve admin etkilenmiyor kaniti uretilerek degistirilebilir.

## 18. Alt Model Teslim Sablonu

Her alt model sonucunu bu formatta verecek. `dosya:satir` kaniti yoksa teslim eksik sayilir.

```text
Yapilan: ...
Degisen: dosya:satir, dosya:satir
Kanıt: build/test/browser/screenshot sonucu
Risk: ...
Sonraki: ...
```

QA/test modeli icin zorunlu kanitlar:
- `dotnet build VizitLink3D.Api`: 0 hata
- `dotnet build VizitLink3D.UI`: 0 hata
- `http://localhost:5115/openapi/v1.json`: 200
- `http://localhost:3113/`: 200
- Browser console: kritik error yok
- Network: kritik API istekleri 200
- Admin: `/admin` veya mevcut admin giris sayfasi frontend `gold` seciminden etkilenmiyor
- Tema: `gold` + `acik/koyu` mod gorunur calisiyor
