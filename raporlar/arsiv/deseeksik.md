# VIZITLINK3D Eksik Tamamlama Plani

> Amaç: VIZITLINK3D sistemini endustriyel seviyede, dinamik, canli efektli, animasyonlu ve yonetilebilir bir admin paneline tasimak.
> Bu dosya, projede calisacak modellerin hatasiz uygulama yapmasi icin net is emri ve kabul kriteridir.

## 0. Zorunlu Okuma Sirasi

Kod yazmadan once su dosyalar sirayla okunacak:

1. `AGENTS.md`
2. `AjanKurallari/00_PROJE_BILGISI.md`
3. `AjanKurallari/03_Razor_MudBlazor_Blazor10.md`
4. `AjanKurallari/04_CSS_Tema_Stitch_Entegrasyonu.md`
5. `AjanKurallari/09_Coklu_Platform_Web_Mobil_Masa.md`
6. `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md`

Bu is kapsaminda aktif hedef:

- Proje: VIZITLINK3D
- Tema: Industrial Luxury
- Admin giris: `/admin/giris`
- Admin ana alan: `/admin/dashboard`
- UI port: `5013`
- API port: `5015`
- UI kutuphanesi: MudBlazor
- Dil: Turkce isimlendirme ve `DilServisi.T()`
- Stil: `wwwroot/css/sistem/` ve token tabanli CSS

## 1. Mevcut Sistem Ozeti

Sistemde su guclu altyapi zaten var:

- `VIZITLINK3D.Api`, `VIZITLINK3D.UI`, `VIZITLINK3D.Ortak`, `VIZITLINK3D.Testler` ayrimi mevcut.
- Admin altinda cok sayida yonetim sayfasi mevcut.
- `AdminDuzen.razor` ve `AdminDuzen.razor.cs` var.
- `KomutPaleti`, `CanliSohbetArayuzu`, `BildirimServisi`, `AnimasyonMotoruServisi` mevcut.
- `tokens.css` ve `wwwroot/css/sistem/` klasor hiyerarsisi mevcut.
- SignalR tarafinda `SohbetHub`, `BildirimHub`, `AIHub` mevcut.
- Medya, AI, iletisim, icerik, tema ve dashboard API parcalari mevcut.

Bu temel korunacak; ana is, bu yapinin endustriyel admin deneyimine donusturulmesidir.

## 2. Kritik Eksikler

### 2.1 Admin Duzeni Endustriyel Seviyede Degil

Mevcut `VIZITLINK3D.UI/Layout/AdminDuzen.razor` tek sol menulu duzen kullaniyor. Hedef, `09_Coklu_Platform_Web_Mobil_Masa.md` dosyasindaki 3 sutunlu admin standardidir:

- Sol: 260px tam menu, tablet 72px mini menu, mobil drawer.
- Orta: sayfa icerigi, sabit toolbar ve akilli baslik alani.
- Sag: 320px canli aktivite akisi, bildirimler, audit log, sohbet ve sistem olaylari.
- Mobil: bottom nav + hamburger + sag panel bottom sheet davranisi.

Kabul kriteri:

- `AdminDuzen.razor` inline style icermeyecek.
- Tum metinler `DilServisi.T()` ile gelecek.
- Admin layout, masaustu/tablet/mobil icin ayri davranis gosterecek.
- Sag canli panel SignalR ile veri alacak.
- Sayfa icerigi kart icine hapsedilmeyecek; duzen tam genislikli profesyonel admin arayuzu gibi davranacak.

### 2.2 Inline Stil ve Hardcoded Renk Cok Fazla

Tespit edilen ornekler:

- `VIZITLINK3D.UI/Layout/AdminDuzen.razor`
- `VIZITLINK3D.UI/Bilesenler/RenkSecici.razor`
- `VIZITLINK3D.UI/Bilesenler/HeroSlider.razor`
- `VIZITLINK3D.UI/Bilesenler/GaleriDialog.razor`
- `VIZITLINK3D.UI/Pages/Yonetim/Vitrin.razor`
- `VIZITLINK3D.UI/Pages/Vitrin/PiedraKonfigurator.razor`
- `VIZITLINK3D.UI/Pages/Test/PiedraAnaliz.razor`
- `VIZITLINK3D.UI/Bilesenler/Medya/MedyaSecici.razor`

Yapilacak:

- Inline `style` kullanimlari CSS class yapisina tasinacak.
- Hardcoded `#fff`, `#000`, `rgba(...)`, `px`, `rem` degerleri token veya merkezi class ile degistirilecek.
- Admin icin tum stiller `VIZITLINK3D.UI/wwwroot/css/sistem/moduller/yonetim.css` ve gerekli bilesen CSS dosyalarinda toplanacak.
- Dinamik renk zorunluysa yalniz gercek veri rengi icin kullanilacak. Ornek: RAL renk kutucugu `background-color: @renk.HexKod` kabul edilebilir, ama boyut, border, shadow inline olmayacak.

Kabul kriteri:

- Admin layout ve admin sayfalarinda inline style kalmayacak.
- CSS icinde `!important` kullanimi azaltilecek, zorunlu kalanlar gerekcelendirilecek.
- Hardcoded renkler `var(--...)` tokenlariyla degistirilecek.

### 2.3 Razor Partial Class Ihlalleri Var

Asagidaki bilesenlerde `@code` blogu tespit edildi:

- `VIZITLINK3D.UI/Bilesenler/GaleriDialog.razor`
- `VIZITLINK3D.UI/Bilesenler/HeroSlider.razor`
- `VIZITLINK3D.UI/Bilesenler/Anasayfa/SSSBolumu.razor`
- `VIZITLINK3D.UI/Bilesenler/Anasayfa/MusteriYorumlariCarousel.razor`
- `VIZITLINK3D.UI/Bilesenler/Anasayfa/ReferansSeridi.razor`
- `VIZITLINK3D.UI/Bilesenler/Anasayfa/HizmetSureciBolumu.razor`

Yapilacak:

- Her `@code` blogu ayni isimli `.razor.cs` partial class dosyasina tasinacak.
- `.razor` dosyasi sadece markup icerecek.
- Namespace ve class adlari Turkce proje standardina uygun olacak.

Kabul kriteri:

- `rg -n "@code" VIZITLINK3D.UI -g *.razor -g !obj/**` sonuc vermeyecek.

### 2.4 Canli Efekt ve Animasyon Altyapisi Eksik Kullaniliyor

Mevcut `AnimasyonMotoruServisi` var ama:

- Hatalari sessiz yutuyor.
- Admin layout tarafinda endustriyel animasyon katmani net degil.
- GSAP/Lenis/Lottie/Three.js gibi harici animasyonlar icin admin tarafinda standart class ve wrapper sozlesmesi yok.

Yapilacak:

- `AnimasyonMotoruServisi` admin icin genisletilecek.
- Dogrudan JS cagri yerine wrapper metotlari kullanilacak.
- Admin icin efekt paketleri tanimlanacak:
  - sayfa giris animasyonu
  - drawer ac/kapa gecisi
  - komut paleti acilis animasyonu
  - bildirim pulse efekti
  - canli aktivite satiri reveal animasyonu
  - dashboard metrik sayac animasyonu
  - hover manyetik buton etkisi
  - skeleton shimmer
  - bos durum micro interaction
- Kullanici `prefers-reduced-motion` tercihinde animasyonlar kisilacak.

Kabul kriteri:

- Animasyonlar admin deneyimini guclendirecek ama veri girisini yavaslatmayacak.
- Form yazarken, tablo filtrelerken, grid kullanirken hareketler odagi bozmayacak.
- Tum JS interop wrapper servis uzerinden olacak.

### 2.5 Admin Dashboard "Canli Operasyon Merkezi" Olmali

Mevcut dashboard gelistirilecek.

Hedef bolumler:

- Gunluk ziyaret, mesaj, medya, blog, proje ve teklif ozetleri.
- Canli ziyaretci akisi.
- Son admin islemleri.
- Son mesajlar ve sohbet kuyrugu.
- Sistem sagligi: API, DB, depolama, SignalR.
- AI kullanim ozeti.
- Icerik eksikleri: bos SEO, eksik gorsel, pasif sayfa, ceviri eksigi.
- Hizli eylemler: yeni blog, yeni slayt, medya yukle, tema duzenle, ceviri ekle.

Kabul kriteri:

- Dashboard ilk ekranda operasyon durumunu anlatacak.
- Sahte/sabit veri varsa ayrica yorumla isaretlenecek ve API entegrasyonu icin is listesine alinacak.
- `DateTime.Now` yerine `DateTime.UtcNow` kullanilacak.
- Her metin `DilServisi.T()` ile gelecek.

### 2.6 Admin Sayfalari Ortak Desenlere Tasinali

Admin altindaki CRUD sayfalari ayni deneyimi vermeli.

Standart sayfa iskeleti:

- Ust baslik alani: ikon, baslik, aciklama, ana eylem.
- Filtre/arama toolbar'i.
- `MudDataGrid` veya uygun MudBlazor liste bileseni.
- Kayit dialogu veya yan panel formu.
- Silme islemi icin `EndustriyelOnayDialogu`.
- Kayit basari/hata mesajlari icin `ISnackbar`.
- Loading skeleton.
- Bos durum.
- Hata durumu.

Kabul kriteri:

- Her admin sayfasinda ortak tasarim dili olacak.
- Her formda dogrulama olacak.
- Her liste filtrelenebilir/siralanabilir olacak.
- Her kritik islem onay isteyecek.

### 2.7 Sag Canli Aktivite Paneli Eksik

Yeni bilesenler olusturulacak:

- `VIZITLINK3D.UI/Bilesenler/Admin/AktiviteAkisi.razor`
- `VIZITLINK3D.UI/Bilesenler/Admin/AktiviteAkisi.razor.cs`
- `VIZITLINK3D.UI/Bilesenler/Admin/BildirimZili.razor`
- `VIZITLINK3D.UI/Bilesenler/Admin/BildirimZili.razor.cs`
- `VIZITLINK3D.UI/Bilesenler/Admin/SistemDurumuKartlari.razor`
- `VIZITLINK3D.UI/Bilesenler/Admin/SistemDurumuKartlari.razor.cs`

API/SignalR hedefleri:

- `BildirimHub` admin bildirimlerini yayinlayacak.
- Audit log kayitlari sag panele akacak.
- Sohbet mesajlari sag panele duserek admini uyandiracak.

Kabul kriteri:

- Sag panel acilip kapanabilir olacak.
- Yeni olay geldiginde satir animasyonla belirecek.
- Bildirim sayaci gercek veriyle guncellenecek.
- Mobilde sag panel bottom sheet gibi davranacak.

### 2.8 Tema Sistemi Tam Endustriyel Degil

Mevcut token dosyalari var ama hardcoded renkler ve `--admin-*` degerleri tam proje konfiginden turemiyor.

Yapilacak:

- `AjanKurallari/00_PROJE_BILGISI.md` tema degerleri ile `degiskenler.css` uyumlu hale getirilecek.
- `tokens.css` sadece `@import` icerecek sekilde korunacak.
- Admin ve public tema ayrimi token seviyesinde netlestirilecek.
- Dark mode icin `[data-tema="koyu"]` admin degiskenleri tamamlanacak.
- Tema yonetimi sayfasi canli onizleme sunacak.

Kabul kriteri:

- Renk, font, bosluk, golge ve gecis degerleri token uzerinden gelecek.
- Admin tema degisimi sayfa yenilemeden gorsel olarak yansiyacak.

### 2.9 Bootstrap Kalintilari Temizlenmeli

`VIZITLINK3D.UI/wwwroot/lib/bootstrap/` altinda Bootstrap dosyalari mevcut.

Yapilacak:

- Bootstrap'in projede gercekten kullanilip kullanilmadigi kontrol edilecek.
- Kullanilmiyorsa referanslar kaldirilacak ve dosyalar temizlenecek.
- Kullaniliyorsa MudBlazor disi UI kutuphanesi yasağı geregi Ustam onayi alinmadan kullanilmayacak.

Kabul kriteri:

- MudBlazor disinda UI kutuphanesi aktif olmayacak.

### 2.10 Debug Log ve Sessiz Hata Yutma Temizlenmeli

Tespit edilen riskler:

- `Console.WriteLine("DEBUG...")` kullanimlari var.
- Bazi `catch { }` bloklari hatayi sessizce yutuyor.
- `AnimasyonMotoruServisi`, `ApiIstemcisi`, `AdminDuzen`, sohbet ve bazi public sayfalarda bu desen goruluyor.

Yapilacak:

- UI tarafinda gerekli yerlerde `ILogger<T>` veya kullaniciya anlamli `ISnackbar` bildirimi kullanilacak.
- Gercekten opsiyonel hatalarda yorum ile gerekce yazilacak.
- Token, sifre, hassas veri loglanmayacak.

Kabul kriteri:

- Debug `Console.WriteLine` kalmayacak.
- Sessiz `catch { }` bloklari gerekcesiz kalmayacak.

## 3. Oncelikli Uygulama Sirasi

### Faz 1: Kural Temizligi

1. `@code` bloklarini `.razor.cs` dosyalarina tasi.
2. Admin layout inline style temizligini yap.
3. Hardcoded renk/font/bosluklari tokenlara bagla.
4. Debug log ve sessiz catch bloklarini duzenle.
5. Bootstrap referanslarini kontrol et.

### Faz 2: Admin Iskeleti

1. `AdminDuzen` icin 3 sutunlu endustriyel layout uygula.
2. Sag aktivite panelini ekle.
3. Responsive davranisi tamamla.
4. Komut paletini admin ust barda birinci sinif arac haline getir.
5. Bildirim zili ve canli sayaçlari SignalR ile bagla.

### Faz 3: Canli Efekt Katmani

1. `yonetim.css` icinde admin animasyon siniflarini olustur.
2. `AnimasyonMotoruServisi` icinde admin animasyon wrapper metotlarini ekle.
3. `prefers-reduced-motion` destegini ekle.
4. Dashboard metrik sayac ve aktivite reveal animasyonlarini uygula.
5. Dialog, drawer, hover ve loading micro interaction standardini oturt.

### Faz 4: Dashboard ve CRUD Standardizasyonu

1. Dashboard'u operasyon merkezine cevir.
2. Admin CRUD sayfalarini ortak baslik/toolbar/grid/dialog desenine tasi.
3. Loading, empty, error state bilesenleri ekle.
4. Tum formlara FluentValidation bagla.
5. Her sayfa icin en az 5 test ekle.

### Faz 5: Kalite ve Dogrulama

1. `dotnet build` hatasiz calismali.
2. `dotnet test` hatasiz calismali.
3. UI port `5013` uzerinden admin sayfalari manuel kontrol edilmeli.
4. Masaustu, tablet, mobil ekran goruntusu alinmali.
5. Animasyonlar, metin tasmalari ve layout kaymalari kontrol edilmeli.

## 4. Tasarim Kalite Standardi

Admin paneli su hissi vermeli:

- Endustriyel: net, ciddi, hizli, is odakli.
- Luxury: koyu zemin, altin vurgu, kaliteli bosluk, dusuk gurultu.
- Canli: bildirim, aktivite, sohbet, sistem sagligi anlik akar.
- Dinamik: tablo, filtre, komut paleti ve panel gecisleri hizli tepki verir.
- Profesyonel: veri yogun ama okunabilir; gereksiz sus yok.

Yapilmayacaklar:

- Landing page gibi hero bolumleri admin ana ekrana koyma.
- Karti kart icine koyma.
- Tek renkli, agir mor/mavi veya sadece siyah tasarima saplanma.
- Inline stil ile hizli cozum uretme.
- Form ve tablo ergonomisini animasyon ugruna bozma.

## 5. Yeni Bilesen Onerileri

Olusturulmasi onerilen ortak admin bilesenleri:

- `AdminSayfaBasligi`
- `AdminAracCubugu`
- `AdminIstatistikKarti`
- `AdminBosDurum`
- `AdminYukleniyorIskeleti`
- `AdminHataDurumu`
- `AktiviteAkisi`
- `BildirimZili`
- `SistemDurumuKartlari`
- `HizliEylemPaneli`
- `CanliMetrikKarti`

Her `.razor` dosyasinin `.razor.cs` partial class dosyasi olacak.

## 6. Dosya Hedefleri

Ana dosyalar:

- `VIZITLINK3D.UI/Layout/AdminDuzen.razor`
- `VIZITLINK3D.UI/Layout/AdminDuzen.razor.cs`
- `VIZITLINK3D.UI/wwwroot/css/sistem/moduller/yonetim.css`
- `VIZITLINK3D.UI/wwwroot/css/sistem/bilesenler/animasyon.css`
- `VIZITLINK3D.UI/wwwroot/css/sistem/bilesenler/efektler.css`
- `VIZITLINK3D.UI/Servisler/AnimasyonMotoruServisi.cs`
- `VIZITLINK3D.UI/Servisler/BildirimServisi.cs`
- `VIZITLINK3D.UI/Bilesenler/KomutPaleti.razor`
- `VIZITLINK3D.UI/Pages/Admin/Dashboard.razor`
- `VIZITLINK3D.UI/Pages/Admin/Dashboard.razor.cs`

Ilk temizlenecek bilesenler:

- `VIZITLINK3D.UI/Bilesenler/GaleriDialog.razor`
- `VIZITLINK3D.UI/Bilesenler/HeroSlider.razor`
- `VIZITLINK3D.UI/Bilesenler/RenkSecici.razor`
- `VIZITLINK3D.UI/Bilesenler/Medya/MedyaSecici.razor`
- `VIZITLINK3D.UI/Bilesenler/Anasayfa/*.razor`

## 7. Model Uygulama Kurallari

Bu dosyayi uygulayacak model:

- Once ilgili dosyayi okuyacak, sonra kucuk ve test edilebilir degisiklik yapacak.
- Kullanici degisikliklerini geri almayacak.
- Yeni JS is mantigi yazmayacak; gerekirse wrapper servisi kullanacak.
- `.razor` icine `@code` veya `<style>` koymayacak.
- Admin metinlerini hardcoded yazmayacak; `DilServisi.T()` kullanacak.
- CSS degerlerini token ile yazacak.
- API yanitlarinda `Cevap<T>` desenini bozmayacak.
- Hata yonetimini kontrolcu icinde try-catch ile dagitmayacak.
- Her faz sonunda build/test calistiracak.

## 8. Test ve Kontrol Komutlari

Kural taramalari:

```powershell
rg -n "@code" VIZITLINK3D.UI -g *.razor -g !obj/**
rg -n "<style|Style=|style=|!important|#[0-9A-Fa-f]{3,8}|rgb\\(|rgba\\(" VIZITLINK3D.UI -g *.razor -g *.css -g !obj/**
rg -n "Console\\.WriteLine|DEBUG|catch \\{ \\}" VIZITLINK3D.UI VIZITLINK3D.Api -g *.cs -g !obj/**
rg -n "DateTime\\.Now|\\.Result|\\.Wait\\(" VIZITLINK3D.UI VIZITLINK3D.Api VIZITLINK3D.Ortak -g *.cs -g *.razor -g !obj/**
```

Derleme ve test:

```powershell
dotnet build VIZITLINK3D.slnx
dotnet test VIZITLINK3D.slnx
```

Manuel UI kontrol:

```powershell
dotnet run --project VIZITLINK3D.Api/VIZITLINK3D.Api.csproj
dotnet run --project VIZITLINK3D.UI/VIZITLINK3D.UI.csproj
```

Tarayici hedefleri:

- `http://localhost:5013/admin/giris`
- `http://localhost:5013/admin/dashboard`
- `http://localhost:5013/admin/medya-havuzu`
- `http://localhost:5013/admin/canli-sohbet`
- `http://localhost:5013/admin/tema-yonetimi`

## 9. Nihai Kabul Kriterleri

Is tamamlanmis sayilmaz, ta ki:

- Admin paneli 3 sutunlu endustriyel layout'a gecene kadar.
- Sag canli aktivite paneli calisana kadar.
- Dashboard gercek operasyon merkezi hissi verene kadar.
- Inline style ve hardcoded renkler kritik yuzeylerden temizlenene kadar.
- `.razor` icinde `@code` kalmayana kadar.
- Mobil/tablet/masaustu gorunumleri bozulmadan calisana kadar.
- Animasyonlar canli ama ergonomik olana kadar.
- `dotnet build` ve `dotnet test` basarili olana kadar.
- Her yeni ozellik icin en az 5 test eklenene kadar.

## 10. Ilk Is Emri

Bir sonraki model su sirayla baslamali:

1. `AdminDuzen.razor` icindeki inline style ve hardcoded metinleri temizle.
2. `AdminDuzen.razor.cs` icindeki debug loglari ve sessiz hata desenlerini duzenle.
3. `yonetim.css` icinde admin layout tokenlarini ve responsive 3 sutun yapisini kur.
4. `AktiviteAkisi` ve `BildirimZili` bilesenlerini ekle.
5. Dashboard'u bu yeni duzene bagla.
6. Build/test calistir, sonuc raporla.

