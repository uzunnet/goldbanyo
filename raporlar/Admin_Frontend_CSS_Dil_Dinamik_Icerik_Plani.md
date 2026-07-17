# VIZITLINK3D Admin + Frontend CSS/Dil/Dinamik Icerik Onarim Plani

> Bu rapor dusuk kod modeline uygulanabilir is paketi olarak verilecektir. Kod yazmadan once zorunlu okuma sirasi:
> `AGENTS.md` -> `AjanKurallari/00_PROJE_BILGISI.md` -> `AjanKurallari/03_Razor_MudBlazor_Blazor10.md` -> `AjanKurallari/04_CSS_Tema_Stitch_Entegrasyonu.md` -> `AjanKurallari/06_API_Servisler_MediatR.md` -> `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md`.

## 1. Hedef

Admin paneldeki metin girisleri, form alanlari, butonlar, grid basliklari, sayfa basliklari ve bildirimler tek dil yapisina baglanacak. Frontend ust menu, footer, dinamik sayfalar ve icerik bolumleri admin panelden gelen veriye gore calisacak. CSS sistemi `tokens.css` ve `degiskenler.css` tokenlariyla tum admin + public UI'da ayni kaynaga baglanacak. Dil secimi admin panelde acilir menu olarak gorunecek ve secilen dil sayfa yenilemeden sisteme uygulanacak.

## 2. Mevcut Tespit

1. `VIZITLINK3D.UI/Servisler/DilServisi.cs` icinde `DilDegisti` eventi var, ancak tum sayfalar bu olaya abone degil. Bu yuzden dil degisimi her yerde anlik yansimiyor.
2. `VIZITLINK3D.UI/Layout/VIZITLINK3DDuzen.razor` public ust menu icin API'den menu aliyor, fakat masaustu ve mobil anahtar kullanimi tutarsiz: bazen `nav_{Baslik}`, bazen direkt `Baslik`.
3. `VIZITLINK3D.UI/Layout/AdminDuzen.razor` admin menusunu API'den aliyor, fakat dil secimi banner icine dagilmis; tum admin sayfalari dil degisimine standart abonelikle baglanmamis.
4. `VIZITLINK3D.UI/Layout/VIZITLINK3DDuzen.razor.cs` ve `VIZITLINK3D.UI/Layout/AdminDuzen.razor.cs` icinde MudTheme renk/font degerleri hardcoded. Bunlar `00_PROJE_BILGISI` + `tokens.css` disipliniyle uyumsuz.
5. `VIZITLINK3D.UI/Pages/DinamikSayfaGosterici.razor` icinde inline style ve `MarkupString` var. Bu hem CSS disiplinini hem de XSS guvenligini riskli hale getiriyor.
6. `VIZITLINK3D.UI/wwwroot/index.html` ayni sistem CSS dosyalarini hem tek tek hem de `tokens.css` uzerinden yukluyor. Bu tekrar ve stil cakismasi uretiyor.
7. `VIZITLINK3D.UI/wwwroot/css/sistem` altinda cok sayida hardcoded renk, px, rgba ve `!important` kullanimi var. En buyuk risk dosyalar: `moduller/VIZITLINK3D.css`, `moduller/yonetim.css`, `bilesenler/admin-tema.css`, `bilesenler/efektler.css`.
8. `VIZITLINK3D.Api/Kontrolcüler/Sistem/MenuKontrolcu.cs` menu endpointleri calisiyor, ancak controller dogrudan DB sorguluyor; uzun vadede Vertical Slice/MediatR standardina tasinmali.
9. `VIZITLINK3D.Api/Kontrolcüler/Pazarlama/AdminIcerikKontrolcu.cs` bazi yerlerde `IActionResult` ve fiziksel `Remove` kullaniyor. AGENTS kurallarina gore `Cevap<T>` ve soft delete zorunlu.
10. `VIZITLINK3D.Api/VeriTabani/TohumVerisi.cs` icinde menu, ceviri ve sayfa icerigi seedleri var; admin -> frontend dinamik akis icin bu seedlerin tam ve dil bazli olmasi gerekiyor.

## 3. Degistirilmeyecek Kurallar

- `.razor` icinde `<style>` ve `@code` yok.
- Ekran metinleri `DilServisi.T("anahtar", "Varsayilan")` disina cikmayacak.
- CSS renk/font/bosluk token ile yazilacak; yeni hardcoded `#hex`, `rgb`, `rgba`, `px` eklenmeyecek.
- UI MudBlazor ile kalacak.
- API cevaplari `Cevap<T>` olacak.
- Silme islemleri soft delete olacak.
- DB degisikligi gerekiyorsa once `Yedekler/db/` altina yedek, sonra EF migration.
- Test eklenmeden is tamam sayilmayacak.

## 4. Faz 1 - Dil Altyapisini Teklestir

### Yapilacaklar

1. `DilServisi` icin ortak kullanim standardi belirle:
   - Tum layout ve sayfalarda `DilServisi` global inject kullanilsin.
   - Dil degisince `DilDegisti` eventi tetiklensin.
   - Layoutlar ve aktif sayfalar event'e abone olup `StateHasChanged` cagrisi yapsin.
   - `Dispose` veya `IAsyncDisposable` icinde abonelik kaldirilsin.

2. Admin dil secimi:
   - Admin ust bannerda dil secimi acilir menu olarak gorunsun.
   - Desteklenen diller `DilServisi.DesteklenenDiller` listesinden gelsin.
   - Secilen dil `localStorage` + `DilServisi.DilDegistirAsync` ile kaydedilsin.
   - Dil degisince admin sol menu ve aktif sayfa metinleri yenilensin.

3. Anahtar standardi:
   - Public menu: `menu.public.<slug>.baslik`
   - Admin menu: `menu.admin.<slug>.baslik`
   - Admin sayfa: `admin.<modul>.<alan>`
   - Ortak: `ortak.kaydet`, `ortak.iptal`, `ortak.sil`, `ortak.ara`, `ortak.yukleniyor`

### Kontrol Edilecek Dosyalar

- `VIZITLINK3D.UI/Servisler/DilServisi.cs`
- `VIZITLINK3D.UI/Layout/VIZITLINK3DDuzen.razor`
- `VIZITLINK3D.UI/Layout/VIZITLINK3DDuzen.razor.cs`
- `VIZITLINK3D.UI/Layout/AdminDuzen.razor`
- `VIZITLINK3D.UI/Layout/AdminDuzen.razor.cs`
- `VIZITLINK3D.UI/Bilesenler/Admin/AdminUstBanner.razor`
- `VIZITLINK3D.Api/VeriTabani/TohumVerisi.cs`

### Kabul Kriteri

TR/EN secimi admin ve public tarafta sayfa yenilemeden tum gorunur metinleri degistirecek. Menuler, butonlar, form label/placeholder, grid title ve snackbar metinleri `DilServisi.T()` ile gorunecek.

## 5. Faz 2 - Admin Metin Girislerini Standartlastir

### Yapilacaklar

1. Tum admin sayfalarinda hardcoded label, placeholder, tab text, grid title, chip text ve buton metni taranacak.
2. Her metin `dil.T()` anahtarina alinacak.
3. Ortak form alanlari icin tekrar eden anahtarlar kullanilacak.
4. Admin form bilesenleri icin tek gorunum standardi olusturulacak:
   - `MudTextField`, `MudSelect`, `MudSwitch`, `MudNumericField`, `MudDataGrid`
   - `Variant="Variant.Outlined"` gibi kararlar moduller arasi ayni olacak.
5. Form alanlarinda ayni siniflar kullanilacak; sayfa bazli farklar CSS tokenlariyla cozulecek.

### Ilk Temizlenecek Sayfalar

- `VIZITLINK3D.UI/Pages/Admin/UrunYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Admin/UrunSihirbazi.razor`
- `VIZITLINK3D.UI/Pages/Admin/YorumYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Yonetim/*.razor`
- `VIZITLINK3D.UI/Bilesenler/Admin/*.razor`

### Kabul Kriteri

Admin panelde ayni tip metin girisleri ayni gorunmeli; `Label="Urun Adi"` gibi hardcoded metin kalmamali. Tum alanlar dil degisimine tepki vermeli.

## 6. Faz 3 - CSS Sistemini Token'a Bagla

### Yapilacaklar

1. `index.html` CSS yukleme duzeni sadelelestirilecek:
   - `css/sistem/tokens.css` tek sistem girisi olacak.
   - `reset.css`, `degiskenler.css`, `VIZITLINK3D.css`, `yonetim.css` gibi dosyalar ayrica tekrar yuklenmeyecek.
   - `css/app.css` sadece gercekten gerekli global app stilleri icin kalacak.

2. `degiskenler.css` tokenlari tamamlanacak:
   - AGENTS standardindaki genel tokenlar: `--ana-renk`, `--ikincil-renk`, `--vurgu-renk`, `--arkaplan`, `--metin`, `--font-baslik`, `--font-metin`, `--bosluk-*`, `--kose-*`
   - Mevcut `--desa-*` ve `--admin-*` tokenlari bu ana tokenlara alias olarak baglanacak.

3. CSS dosyalari parca parca temizlenecek:
   - `#fff`, `#000`, `#111`, `rgba(...)`, `font-family: 'Manrope'`, `padding: 16px` gibi degerler token'a tasinacak.
   - `!important` kullanimlari kaldirilacak; gerekiyorsa selector yapisi duzeltilecek.
   - ID selector kullanimi kaldirilacak veya class'a tasinacak.

4. `.razor` inline style temizligi:
   - `style="..."` olan yerler CSS class'a tasinacak.
   - Loading ekranlari, video container, PDF embed, icon size gibi degerler class ile yonetilecek.

### Ilk Temizlenecek Dosyalar

- `VIZITLINK3D.UI/wwwroot/index.html`
- `VIZITLINK3D.UI/wwwroot/css/sistem/tokens.css`
- `VIZITLINK3D.UI/wwwroot/css/sistem/temeller/degiskenler.css`
- `VIZITLINK3D.UI/wwwroot/css/sistem/moduller/VIZITLINK3D.css`
- `VIZITLINK3D.UI/wwwroot/css/sistem/moduller/yonetim.css`
- `VIZITLINK3D.UI/wwwroot/css/sistem/bilesenler/admin-tema.css`
- `VIZITLINK3D.UI/wwwroot/css/sistem/bilesenler/efektler.css`
- `VIZITLINK3D.UI/Pages/DinamikSayfaGosterici.razor`
- `VIZITLINK3D.UI/Layout/AdminDuzen.razor`

### Kabul Kriteri

`tokens.css` tek giris olur. Yeni sayfa veya bilesen eklenince stil sistemi kendiliginden ayni gorunur. Admin ve public ekranlar ayni marka paletinden beslenir.

## 7. Faz 4 - Admin Kaynakli Dinamik Frontend Icerigi

### Yapilacaklar

1. Public ust menu:
   - `api/menu/VIZITLINK3D` yerine konum standardi netlestirilecek: `PublicHeader`.
   - Alt menuler admin panelde ac/kapa, sira, ikon, URL, yeni sekme, dil anahtari ile yonetilecek.
   - Menu basligi direkt `Baslik` metniyle degil, varsa `DilAnahtari` ile cevrilecek.

2. Footer:
   - `PublicFooterHizli` ve `PublicFooterKategori` admin panelden yonetilecek.
   - Footer link basliklari da dil anahtari ile cevrilecek.

3. Dinamik sayfalar:
   - `DinamikSayfaGosterici` slug + aktif dil ile icerik cekecek.
   - API endpoint `api/VIZITLINK3D/sayfa-icerigi/{slug}?dil=tr` standardina cekilecek.
   - HTML icerik gerekiyorsa API tarafinda sanitize edilmis alan donmeli; UI tarafinda ham `MarkupString` sadece temizlenmis icerige uygulanmali.

4. Admin icerik formlari:
   - Sayfa icerigi, menu, slayt, SSS, referans, hizmet adimi, musteri yorumu alanlari dil bazli girilebilmeli.
   - Dil sekmesi veya dil dropdown'u ile TR/EN icerik girisi ayrilmali.
   - Eksik dilde fallback net olmali: once aktif dil, yoksa varsayilan `tr`, o da yoksa bos durum.

### Backend Kontrol Dosyalari

- `VIZITLINK3D.Api/Kontrolcüler/Sistem/MenuKontrolcu.cs`
- `VIZITLINK3D.Api/Kontrolcüler/Sistem/DilKontrolcu.cs`
- `VIZITLINK3D.Api/Kontrolcüler/Pazarlama/AdminIcerikKontrolcu.cs`
- `VIZITLINK3D.Api/Kontrolcüler/Pazarlama/IcerikKontrolcu.cs`
- `VIZITLINK3D.Api/VeriTabani/TohumVerisi.cs`
- `VIZITLINK3D.Ortak/Modeller/MenuOgesi.cs`
- `VIZITLINK3D.Ortak/Modeller/SayfaIcerigi.cs`
- `VIZITLINK3D.Ortak/Modeller/Ceviri.cs`

### Kabul Kriteri

Admin panelde menu veya sayfa icerigi degistirilince frontend ust menu, footer ve dinamik sayfa ilgili veriyi API'den alir. Statik fallback sadece API bos/kapaliysa devreye girer.

## 8. Faz 5 - API Disiplini ve Veri Guvenligi

### Yapilacaklar

1. `AdminIcerikKontrolcu` ve `MenuKontrolcu` endpointleri kural uyumuna alinacak:
   - `IActionResult` yerine `Cevap<T>`
   - Fiziksel `Remove` yerine `SilindiMi = true`
   - `OlusturulmaTarihi`, `GuncellenmeTarihi`, `SilinmeTarihi`
   - Admin yazma endpointlerinde `[Authorize]`
   - FluentValidation DTO

2. Mümkunse Vertical Slice'a kademeli tasinacak:
   - `Moduller/Sistem/Menu/Komutlar`
   - `Moduller/Sistem/Menu/Sorgular`
   - `Moduller/Sistem/Ceviri`
   - `Moduller/Pazarlama/Icerik`

3. Dinamik HTML guvenligi:
   - `IcerikTemizleyici` servisi kullanilacak.
   - Admin WYSIWYG kaydinda veya public cevapta sanitize uygulanacak.

### Kabul Kriteri

API endpointleri `Cevap<T>` standardinda, soft delete uyumlu, admin yazmalari yetkili, public okumalar guvenli olur.

## 9. Test ve Dogrulama Plani

### Otomatik

1. `dotnet build`
2. `dotnet test`
3. Dil servisi icin en az 5 test:
   - Baslangic dili okunur.
   - Dil degisince event tetiklenir.
   - Eksik anahtar fallback doner.
   - API bos donerse yerel fallback denenir.
   - Desteklenen diller bos ise tr/en varsayilan gelir.
4. Menu API icin en az 5 test:
   - PublicHeader kok menu gelir.
   - Alt menu sirali gelir.
   - Pasif/silinmis menu gelmez.
   - AdminSol super admin filtreleri dogru calisir.
   - Footer konumlari ayrilir.
5. Dinamik sayfa icin en az 5 test:
   - Slug + dil ile icerik gelir.
   - Eksik dilde tr fallback gelir.
   - Yok slug 404/hatali cevap verir.
   - Sanitize edilmis HTML doner.
   - Bos icerik bos durum olarak gorunur.

### Manuel

1. `http://localhost:5013/` ac.
2. Ust menu, alt menu, footer linkleri gorunuyor mu kontrol et.
3. Dil dropdown TR -> EN yap; sayfa yenilenmeden metinler degismeli.
4. `/admin` icinde dil dropdown gorunmeli ve admin menusu/metinleri degismeli.
5. Admin panelden menu basligi veya sayfa icerigi degistir; frontend'de API verisi yansimali.
6. Tarayici console ve network hatalari sifirlanmali.

## 10. Uygulama Sirasi

1. Dil servis standardi ve admin dil dropdown.
2. Admin hardcoded metin temizligi.
3. CSS token girisi ve `index.html` sadelelestirme.
4. Public menu/footer/dinamik sayfa veri akisi.
5. API soft delete + `Cevap<T>` + validation temizligi.
6. Testler ve canli dogrulama.

## 11. Dusuk Kod Modeline Net Talimat

- Tek seferde tum projeyi degistirme. Her fazdan sonra build/test calistir.
- Once layout ve servisleri duzelt, sonra sayfalari temizle.
- Yeni CSS dosyasi acmadan once mevcut `wwwroot/css/sistem` yapisini kullan.
- Yeni hardcoded metin, renk, bosluk, inline style ekleme.
- `MarkupString` gordugun yerde guvenlik notu dus ve sanitize akisini tamamla.
- Silme endpointlerinde `Remove` kullanma.
- Her degisiklikten sonra ilgili kabul kriterini isaretle.
