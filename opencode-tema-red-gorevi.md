# Opencode Tema RED Gorevi

## Karar

Durum: RED.

Sebep: Tema dosyalari olusturulmus ve build geciyor, fakat frontend tema deneyimi uygulanmamis. Sayfa Gold/Stitch tasarimi gibi degil; ham bloklar, buyuk bosluklar, eksik ikon render'i, 404 kaynaklar ve eski icerik izleri var.

## Kontrol Kanitlari

- `dotnet build VizitLink3D.Api/VizitLink3D.Api.csproj`: 0 hata, 2 NuGet guvenlik uyarisi.
- `dotnet build VizitLink3D.UI/VizitLink3D.UI.csproj`: 0 hata, 4 NuGet guvenlik uyarisi.
- `http://localhost:3113/`: 200.
- `http://localhost:5115/openapi/v1.json`: 200.
- `http://localhost:3113/admin/giris`: 200.
- DevTools Console: `@import rules at the top` issue var.
- DevTools Console: 17 adet 404 resource hatasi var.
- Screenshot: `I:\goldbanyo_web\tmp\gold-tema-kontrol-20260702.png`.

## Gorulen Temel Hatalar

1. Tema sadece renk gibi kalmis; Stitch'teki lüks/endustriyel layout uygulanmamis.
2. Ana sayfada bilesenler ham akiyor, grid/section ritmi bozuk, buyuk bos alan var.
3. `arrow_forward` ikon olarak degil metin olarak gorunuyor.
4. Gold header var ama body Gold/Stitch sahne tasarimi degil.
5. `Kapak Sistemleri` frontend menude duruyor; Gold Banyo frontend hedefinde banyo mobilyasi menusu olmali.
6. CSS'te `@import` sirasi hatasi var.
7. 404 veren 17 kaynak bulunup duzeltilmeli.
8. `gold/bilesenler.css` ve `gold/animasyonlar.css` icinde token disi dogrudan `rgba`, `#000` gibi degerler var.
9. Layout ve yorumlarda eski `VizitLink3D` izleri var.
10. Admin ayri kalmali; admin icinde eski teknik alanlar olabilir, frontend tema duzeltmesine karistirilmayacak.

## M3 Supervisor Emri

Bu gorevde yeni mimari icat etme. Mevcut Gold template'i gercekten ekrana uygula. Kucuk ve kanitli duzeltmeler yap. Her alt model kendi sahiplik alanindan cikmasin.

Zorunlu okuma:
- `AGENTS.md`
- `AjanKurallari/00_PROJE_BILGISI.md`
- `AjanKurallari/04_CSS_Tema_Stitch_Entegrasyonu.md`
- `AjanKurallari/12_Token_Optimizasyonu_Alt_Ajan_Kullanimi.md`
- `AjanKurallari/13_Tema_Sablon_Sistemi.md`
- `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md`
- `düzeltmegold.md`
- `opencode-kontrol-yeni-plan.md`
- `opencode-tema-red-gorevi.md`

## Alt Model Gorevleri

### 1. explore / 404 ve DOM envanteri

Kod yazma.

Bul:
- 17 adet 404 hangi dosya/gorsel/font/css/js isteginden geliyor?
- Sayfada `arrow_forward` neden metin olarak gorunuyor?
- Ana sayfada hangi bilesenler CSS class bekliyor ama stil almiyor?
- Gold template CSS dosyalari browser'a yukleniyor mu?
- `data-tema-id`, `data-site-tema`, `data-tema-mod` DOM'da gercekten var mi?

Teslim:
- Her bulgu icin `kaynak dosya:satir`.
- 5 satiri gecmeyen ozet.

### 2. coder-hizli / 404 ve ikon duzeltmesi

Sahiplik:
- Eksik asset pathleri.
- Ikon render sorunu.
- CSS import sirasi.

Yap:
- 404 veren kaynaklari gercek dosyaya bagla veya fallback ekle.
- `arrow_forward` gibi Material icon metinleri ikon olarak render edilsin.
- CSS `@import` kurallari dosyanin en ustune gelsin.

Dokunma:
- Admin sayfalari.
- Tema servis mimarisi.

Kanit:
- DevTools Console'da 404 yok.
- `@import rules at the top` issue yok.
- Ikonlar metin olarak gorunmuyor.

### 3. yazici / Gold CSS'i gercek tema haline getir

Sahiplik:
- `VizitLink3D.UI/wwwroot/css/temalar/gold/tokens.css`
- `VizitLink3D.UI/wwwroot/css/temalar/gold/bilesenler.css`
- `VizitLink3D.UI/wwwroot/css/temalar/gold/animasyonlar.css`
- Gerekirse `VizitLink3D.UI/wwwroot/css/temalar/_sistem/*`

Yap:
- Ana sayfa bolumleri icin gercek section layout yaz: hero, stats, bento, zanaat, surec, akilli/hareketli, projeler, katalog CTA, bayi, yorum, SSS, iletisim CTA.
- Buyuk bosluklari kaldir.
- Responsive desktop/mobile davranisini duzelt.
- Hover, reveal, glow, parallax hissi CSS ile calissin.
- Token disi renk/font/bosluk kullanma.
- `rgba(...)`, `#000`, sabit `px/rem` tekrarlarini mumkun oldugunca tokena al.

Dokunma:
- Razor icine `<style>` ekleme.
- Admin CSS'ini etkileme.

Kanit:
- Desktop screenshot.
- Mobile screenshot.
- `rg -n "!important|#000|rgba\\(" gold css` sonucunu raporla ve gerekcesiz kalanlari sifirla.

### 4. sayfa-uygulama / Ana sayfa Stitch akisini gercekten uygula

Sahiplik:
- `VizitLink3D.UI/Pages/AnaSayfa.razor`
- `VizitLink3D.UI/Pages/AnaSayfa.razor.cs`
- `VizitLink3D.UI/Bilesenler/Anasayfa/*`
- `VizitLink3D.UI/Bilesenler/Stitch/*`
- `VizitLink3D.UI/Layout/VizitLink3DDuzen.razor.cs` sadece frontend menu icin.

Yap:
- Ana sayfa 4 Stitch ana sayfasinin tek birleşik akisi gibi gorunsun.
- Ham metin akisi degil, section/grid/sahne yapisi olsun.
- Frontend menuden `Kapak Sistemleri` kaldir veya Gold Banyo urun/koleksiyon yapisina cevir.
- Eski `VizitLink3D` yorum/metinlerini frontendden temizle.
- Gold Banyo disi Desadoor/kapi/mutfak/villa icerigi alma.
- Hardcoded metinleri `DilServisi.T(...)` ile kullan.

Kanit:
- `rg -n "Kapak Sistemleri|kapi-modelleri|VizitLink3D kurumsal|Desadoor|mutfak|Villa|villa" VizitLink3D.UI/Pages VizitLink3D.UI/Layout VizitLink3D.UI/Bilesenler` sonucunda frontend icin kritik kalinti yok.
- Browser screenshot Stitch/Gold luks sahne olarak gorunuyor.

### 5. QA/test / Kabul kontrolu

Kod yazma.

Calistir:
- `dotnet build VizitLink3D.Api/VizitLink3D.Api.csproj`
- `dotnet build VizitLink3D.UI/VizitLink3D.UI.csproj`
- `http://localhost:3113/`
- `http://localhost:5115/openapi/v1.json`
- DevTools Console
- Desktop screenshot
- Mobile screenshot

Kabul icin zorunlu:
- Build 0 hata.
- `3113` ve `5115` 200.
- Console error yok.
- 404 yok.
- DOM: `data-tema-id="gold"`, `data-site-tema="gold"`, `data-tema-mod="koyu"` veya secili mod var.
- Tema acik/koyu degisiyor.
- Sayfa ham HTML akisi gibi degil, Gold/Stitch tasarimi gibi.
- Admin giris sayfasi aciliyor ve admin tema sistemi bozulmuyor.

## Final Rapor Formati

Her alt model 5 satiri gecmeyecek:

```text
Yapilan:
Degisen:
Kanit:
Kalan risk:
Karar:
```

M3 final karari:
- KABUL
- DUZELTME GEREKLI
- RED

Bu kontrol icin mevcut karar: RED.
