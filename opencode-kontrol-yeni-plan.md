# Opencode Kontrol Sonucu ve Yeni Dar Duzeltme Plani

## 1. Test Sonucu

Kontrol tarihi: 2026-07-02

Yapilan testler:
- `dotnet build VizitLink3D.Api/VizitLink3D.Api.csproj`: basarili, 0 hata, 2 NuGet guvenlik uyarisi.
- `dotnet build VizitLink3D.UI/VizitLink3D.UI.csproj`: basarili, 0 hata, 4 NuGet guvenlik uyarisi.
- `http://localhost:5115/openapi/v1.json`: 200.
- `http://localhost:5113/`: 200.
- `http://localhost:3113/`: hata, port dinlemiyor.
- `VizitLink3D.UI/wwwroot` icinde `_framework`, `_content`, `.br`, `.gz` publish artigi bulunmadi.

Karar:
- Build geciyor ama is KABUL degil.
- Sebep: 3113 port hedefi tutmuyor, gold tema default degil, acik/koyu mod uygulanmiyor, ana sayfada hardcoded/sahte icerik ve eski VizitLink3D/kapi izleri kalmis.

## 2. Kritik Bulgular

1. Port uyumsuzlugu
   - Dinleyen UI portu: `5113`
   - Beklenen/kullanici hedefi: `3113`
   - API: `5115`
   - Yapilacak: UI port karari tek olsun. Bu plan 3113'u hedef kabul eder.

2. Gold tema default degil
   - `VizitLink3D.Api/Moduller/Tema/Servisler/CokluTemaServisi.cs:23`
   - `VARSAYILAN_TEMA = "aurelian-onyx"`
   - `VizitLink3D.UI/Layout/VizitLink3DDuzen.razor.cs:161-162`
   - Layout ilk render'da `data-tema-id` ve `data-site-tema` degerini zorla `aurelian-onyx` yapiyor.

3. Acik/koyu mod eksik uygulanmis
   - `gold/tokens.css` icinde `data-tema-mod="acik"` var.
   - Layout tarafinda `data-tema-mod` set edildigine dair kanit yok.
   - TemaSecici `gold` biliyor ama layout bunu default kabul etmiyor.

4. Gold CSS kurallara tam uymuyor
   - `gold/animasyonlar.css` icinde `@keyframes` bloklari `:root[data-tema-id="gold"]` icine yazilmis.
   - `gold/bilesenler.css` icinde cok sayida `!important` var.
   - Bu, `04_CSS_Tema_Stitch_Entegrasyonu.md` yasaklarina aykiri.

5. Icerik hedefi tutmuyor
   - `VizitLink3D.UI/Pages/AnaSayfa.razor.cs` icinde hardcoded metinler var.
   - Sahte proje ornekleri var: Malibu, Dubai, Londra, Ocean View Villa, Royal Suite, Mayfair Residence.
   - Fallback gorseller eski `/images/vizitlink3d/proje-*.jpg` yoluna dusuyor.
   - `VizitLink3D.UI/Layout/VizitLink3DDuzen.razor.cs:381` icinde `Kapı Modelleri / kapi-modelleri` menusu duruyor.
   - `KapiModelleri`, `KapakSistemleri`, `KapakDetay` gibi sayfalar frontend menude veya route'ta Gold Banyo hedefini kirabilir.

6. Calisma agaci riski
   - Git status cok kirli.
   - Eski `Desadoor.*` dosyalarinda buyuk silmeler var.
   - `VizitLink3D.*` projeleri untracked gorunuyor.
   - Yeni ajanlar genis temizlik veya silme yapmayacak.

## 3. Yeni Opencode Gorevi

M3 supervisor bu kez sadece asagidaki dar duzeltmeleri yaptiracak. Yeni mimari icat edilmeyecek.

```text
Proje: I:\goldbanyo_web
Ana hedef: Mevcut Opencode cikisini kabul seviyesine getir. Kod tabaninda buyuk silme/refactor yapma.

Zorunlu okuma:
AGENTS.md
AjanKurallari/00_PROJE_BILGISI.md
AjanKurallari/04_CSS_Tema_Stitch_Entegrasyonu.md
AjanKurallari/12_Token_Optimizasyonu_Alt_Ajan_Kullanimi.md
AjanKurallari/13_Tema_Sablon_Sistemi.md
AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md
düzeltmegold.md
opencode-kontrol-yeni-plan.md

Genel yasak:
- Desadoor silmelerine dokunma.
- Admin layout ve admin sayfalarini bozma.
- `.razor` icinde `<style>` veya `@code` ekleme.
- Yeni JS/CSS framework ekleme.
- Hardcoded renk/font/bosluk ekleme.
- Kullanici onayi olmadan DB migration ekleme.
```

## 4. Alt Model Gorevleri

### Ajan 1 - coder-hizli / Port ve calisma duzeltmesi

Sahiplik:
- `VizitLink3D.UI/Properties/launchSettings.json` varsa
- `VizitLink3D.Api/Properties/launchSettings.json` varsa
- `VizitLink3D.Api/Program.cs`
- `VizitLink3D.UI/wwwroot/appsettings.json`
- `AjanKurallari/00_PROJE_BILGISI.md` sadece port karari gerekiyorsa

Gorev:
- UI lokal portunu `3113` yap.
- API `5115` kalsin.
- CORS `http://localhost:3113` ve gerekirse `http://localhost:5113` icin development'ta izinli olsun.
- Uretimde `AllowAnyOrigin()` kullanma.

Kanıt:
- `http://localhost:3113/` 200.
- `http://localhost:5115/openapi/v1.json` 200.
- CORS preflight `3113 -> 5115` basarili.

### Ajan 2 - coder-agir / Gold tema varsayilan ve mod uygulama

Sahiplik:
- `VizitLink3D.Api/Moduller/Tema/Servisler/CokluTemaServisi.cs`
- `VizitLink3D.UI/Layout/VizitLink3DDuzen.razor.cs`
- `VizitLink3D.UI/Bilesenler/Tema/TemaSecici.razor`

Gorev:
- Varsayilan frontend tema `gold` olsun.
- Layout ilk render'da `aurelian-onyx` zorlamasin.
- `data-tema-id="gold"`, `data-site-tema="gold"`, `data-tema-mod="koyu"` default gelsin.
- TemaSecici acik/koyu mod secimini localStorage + DOM attribute ile uygulasin.
- Admin tarafinda `AdminDuzen` etkilenmesin.

Kanıt:
- Browser DOM'da root attribute'leri gorunsun.
- Gold koyu/acik mod gecisi sayfayi bozmadan calissin.

### Ajan 3 - yazici / Gold CSS kural temizligi

Sahiplik:
- `VizitLink3D.UI/wwwroot/css/temalar/gold/tokens.css`
- `VizitLink3D.UI/wwwroot/css/temalar/gold/bilesenler.css`
- `VizitLink3D.UI/wwwroot/css/temalar/gold/animasyonlar.css`

Gorev:
- `@keyframes` bloklarini `:root` disina al.
- `!important` kullanımlarini kaldir; specificity ile coz.
- `bilesenler.css` icinde dogrudan renk/font/bosluk ekleme; token kullan.
- `data-tema-mod="acik"` ve `data-tema-mod="koyu"` modlarini koru.

Kanıt:
- `rg -n "!important|@keyframes" gold css` sonucu `!important` yok, keyframes top-level.
- Browser'da hero/header/kart hover animasyonlari calisir.

### Ajan 4 - sayfa-uygulama / Ana sayfa ve menu icerik temizligi

Sahiplik:
- `VizitLink3D.UI/Pages/AnaSayfa.razor`
- `VizitLink3D.UI/Pages/AnaSayfa.razor.cs`
- `VizitLink3D.UI/Layout/VizitLink3DDuzen.razor.cs`
- Gerekirse sadece frontend menuden kaldirmak icin ilgili route/menu kayitlari

Gorev:
- Ana sayfadaki sahte global proje fallbacklerini kaldir.
- `/images/vizitlink3d/proje-*.jpg` fallbacklerini Gold Banyo medya yollarina veya bos/fallback-safe gorsellere cevir.
- `Kapı Modelleri / kapi-modelleri` frontend menuden kalksin; Gold Banyo menu urunler, katalog, projeler, referanslar, bayiler, iletisim etrafinda kalsin.
- Hardcoded gorunen metinleri `DilServisi.T(...)` veya DB icerik fallbacklerine tasimaya basla.
- Stitch 4 ana sayfa fikri tek ana sayfada kalsin ama icerik Gold Banyo disina cikmasin.

Kanıt:
- `rg` ile Malibu/Dubai/Londra/Ocean View/Royal Suite/Mayfair ve `/images/vizitlink3d/proje` bulunmaz.
- Ana sayfa 200 ve gorseller 404 vermiyor.

### Ajan 5 - QA/test

Sahiplik:
- Kod yazma yok.

Gorev:
- Build, port, browser ve icerik kontrolu yap.

Zorunlu kanıt:
- API build: 0 hata.
- UI build: 0 hata.
- `http://localhost:3113/`: 200.
- `http://localhost:5115/openapi/v1.json`: 200.
- Browser console kritik error yok.
- Network kritik istekler 200.
- DOM: `data-tema-id="gold"` ve `data-tema-mod` var.
- Ana sayfada eski kapi/desadoor/vizitlink/sahte global proje icerigi yok.
- Admin giris/admin layout frontend tema degisiminden etkilenmiyor.

## 5. Kabul Karari

Bu tur sonunda M3 sadece su uc karardan birini verecek:

- KABUL: Tum kanitlar tamam.
- DUZELTME GEREKLI: Build/port calisir ama tema/icerik eksik.
- RED: Build bozuk, portlar acilmiyor veya admin bozulmus.

Mevcut durum: `DUZELTME GEREKLI`.
