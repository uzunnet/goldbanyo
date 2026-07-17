# VIZITLINK3D Proje Analizi - Admin ve Frontend Eksikleri

> Tarih: 2026-05-15
> Ikinci kontrol notu: Bu dosya yeniden kontrol edilerek guncellendi. Onceki analizde build geciyordu; yeni kontrolde build/test artik kiriliyor.

## 0. Yeniden Calistirilan Kontroller

Calistirilan komutlar:

```powershell
dotnet build VIZITLINK3D.slnx
dotnet test VIZITLINK3D.slnx
rg -n "return Cevap<.*>\.Basarili\(\[\]|Basarili\(null\)|Basarili\(new |TODO|throw new NotImplementedException|SayfaSayisi = 0|Task.CompletedTask" VIZITLINK3D.Api VIZITLINK3D.UI -g *.cs -g !obj/**
rg -n "api/urunAilesi|api/urun-ailesi|api/UrunAilesi|api/kategoriler|api/urun-kategorileri|api/teklifler|api/kaplamalar|api/konfigurasyon|api/uc-boyut|api/pdf-katalog" VIZITLINK3D.UI VIZITLINK3D.Api -g *.cs -g *.razor -g !obj/**
```

Guncel sonuc:

- `dotnet build VIZITLINK3D.slnx`: basarisiz.
- `dotnet test VIZITLINK3D.slnx`: basarisiz, build hatasi yuzunden test kosamiyor.
- Ana kirici dosya: `VIZITLINK3D.Ortak/Modeller/UrunParcaEslemesi.cs`.
- Onceki analizde uyumsuz gorunen bazi route'lar duzeltilmis:
  - `api/urun-ailesi` artik API ve UI tarafinda uyumlu gorunuyor.
  - `api/urun-kategorileri` artik API ve UI tarafinda uyumlu gorunuyor.
  - `api/teklifler` artik API ve UI tarafinda uyumlu gorunuyor.
  - `api/kaplamalar` ve `api/malzemeler/{id}/kaplamalar` endpointleri eklenmis.
  - `api/uc-boyut/modeller` route'u sade ve uyumlu hale getirilmis.
- Buna ragmen PDF katalog, konfigurasyon endpointleri, 3D parca akisinin tamamlanmasi ve frontend stil/dil temizligi hala eksik.

## 1. P0 - Build'i Kiran Yeni Hata

### 1.1 `UrunParcaEslemesi.cs` Derlemeyi Kiriyor

Dosya:

- `VIZITLINK3D.Ortak/Modeller/UrunParcaEslemesi.cs`

Build hatalari:

```text
CS0234: 'Audit' tur veya ad alani adi 'VIZITLINK3D.Ortak.Modeller' ad alaninda yok
CS0234: 'EntityFrameworkCore' tur veya ad alani adi 'Microsoft' ad alaninda yok
CS0246: 'EntityBase' turu veya ad alani adi bulunamadi
CS0246: 'UrunUcBoyutParcasi' turu veya ad alani adi bulunamadi
```

Mevcut dosya sorunu:

```csharp
using VIZITLINK3D.Ortak.Modeller.Audit;
using Microsoft.EntityFrameworkCore;

public class UrunParcaEslemesi : EntityBase
{
    public UrunUcBoyutParcasi UrunUcBoyutParcasi { get; set; } = null!;
}
```

Sorunlar:

- `VIZITLINK3D.Ortak.Modeller.Audit` namespace'i yok.
- `EntityBase` yok.
- `VIZITLINK3D.Ortak` projesi EF Core referansi tasimamali; `Microsoft.EntityFrameworkCore` using'i Ortak modelde gereksiz ve kurala aykiri.
- `UrunUcBoyutParcasi` tipi dogru namespace'te bulunmuyor ya da dosya bu namespace'i gormuyor.

Yapilacak:

1. `UrunParcaEslemesi` saf POCO hale getirilmeli.
2. `EntityBase` kalitimi kaldirilmali veya projede gercek bir ortak temel entity varsa dogru namespace ile kullanilmali.
3. `Microsoft.EntityFrameworkCore` using'i kaldirilmali.
4. Navigation property icin `[JsonIgnore]` kullanilmali.
5. `UrunUcBoyutParcasi` ile ayni namespace'e alinmali veya tam namespace duzeltilmeli.
6. Build tekrar calistirilmali.

Kabul kriteri:

- `dotnet build VIZITLINK3D.slnx` tekrar basarili olmali.
- `dotnet test VIZITLINK3D.slnx` tekrar calismali.

## 2. Ikinci Kontrolde Duzelmis Gecmis Bulgular

### 2.1 Urun Ailesi Route'u Duzelmis Gorunuyor

API:

- `VIZITLINK3D.Api/Moduller/Urunler/Kontrolculer/UrunAilesiKontrolcu.cs`
- Route: `api/urun-ailesi`

UI:

- `VIZITLINK3D.UI/Pages/Admin/UrunAilesiYonetimi.razor.cs`
- `VIZITLINK3D.UI/Pages/Urunler.razor.cs`
- Cagri: `api/urun-ailesi`

Durum:

- Onceki `api/urunAilesi` / `api/urun-ailesi` uyumsuzlugu buyuk olcude giderilmis.

Kalan kontrol:

- Controller gercek DB islemi yapiyor mu mutlaka build duzeldikten sonra endpoint testiyle dogrulanmali.

### 2.2 Urun Kategori Route'u Duzelmis Gorunuyor

API:

- `VIZITLINK3D.Api/Moduller/Urunler/Kontrolculer/UrunKategoriKontrolcu.cs`
- Route: `api/urun-kategorileri`

UI:

- `UrunKategoriYonetimi.razor.cs`
- `Urunler.razor.cs`
- Cagri: `api/urun-kategorileri`

Durum:

- Endpoint standardi artik daha tutarli.
- Soft delete bu controller'da uygulanmis.

### 2.3 Teklif Route'u Duzelmis Gorunuyor

API:

- `VIZITLINK3D.Api/Moduller/Urunler/Kontrolculer/TeklifKontrolcu.cs`
- Route: `api/teklifler`

UI:

- `TeklifYonetimi.razor.cs`
- `TeklifIstegiFormu.razor.cs`
- Cagri: `api/teklifler`

Durum:

- Route uyumu duzelmis.

Kritik not:

- Controller sinifinda `[Authorize(Roles = "Admin")]` class seviyesinde var, `POST` icin `[AllowAnonymous]` eklenmis. Musteri teklif formu icin dogru olabilir.
- Ancak teklif formunun kullandigi `api/konfigurasyon/{id}` endpointleri hala belirsiz.

### 2.4 Kaplama Endpointleri Eklenmis

API:

- `VIZITLINK3D.Api/Moduller/Malzemeler/Kontrolculer/KaplamaKontrolcu.cs`
- Route: `api/kaplamalar`
- Ek route: `/api/malzemeler/{malzemeId:int}/kaplamalar`

UI:

- `KaplamaYonetimi.razor.cs`
- Cagri: `api/kaplamalar`, `api/malzemeler/{id}/kaplamalar`

Durum:

- Onceki kaplama API eksigi kismen giderilmis.

Kalan eksik:

- `KaplamaKontrolcu.Olustur` sadece `AktifMi = true` atiyor, `OlusturulmaTarihi`, `SilindiMi`, audit alanlari ve validation kontrol edilmeli.
- Delete fiziksel degil, pasif yapiyor; bu iyi, fakat modelde `SilindiMi` varsa standart soft delete'e cekilmeli.

### 2.5 3D Upload UI Tarafi Gelismis Gorunuyor

Yeni taramada:

- `UcBoyutModelYonetimi.razor.cs` artik `PostMultipartAsync` ile `api/uc-boyut/modeller/yukle` cagiriyor.

Durum:

- Onceki "UI JSON gonderiyor, API dosya bekliyor" bulgusu guncellenmeli: UI tarafinda multipart icin adim atilmis.

Kalan kontrol:

- API gercek GLB medya kaydi yapiyor mu build duzeldikten sonra dogrulanmali.
- Dosya boyutu, mime, GLB uzanti, medya havuzu baglantisi test edilmeli.

## 3. Hala Kritik Olan API Eksikleri

### 3.1 PDF Katalog Hala Placeholder

Dosyalar:

- `VIZITLINK3D.Api/Moduller/PdfKatalog/Servisler/PdfIcerikCozumleyici.cs`
- `VIZITLINK3D.Api/Moduller/PdfKatalog/Servisler/PdfGorselCikarici.cs`
- `VIZITLINK3D.Api/Moduller/PdfKatalog/Kontrolculer/PdfKatalogKontrolcu.cs`

Tespit:

```csharp
// TODO: Gercek PDF isleme entegrasyonu
return new PdfCozumlemeSonucu { BasariliMi = true, SayfaSayisi = 0 };
```

Sorun:

- PDF yukleme ekrani olsa bile gercek PDF sayfa sayisi alinmiyor.
- PDF gorsel cikarimi calismiyor.
- `SayfaSayisi = 0` dondugu icin admin urune baglanacak katalog gorsellerini goremez.

Yapilacak:

1. C# tabanli PDF parser/render wrapper secilmeli.
2. `PdfIcerikCozumleyici` gercek sayfa sayisi donmeli.
3. `PdfGorselCikarici` sayfa/gorsel medya kaydi uretmeli.
4. Cozumleme sonucu DB'ye yazilmali.
5. Admin onay ekraninda sayfa gorselleri gorunmeli.

### 3.2 Konfigurasyon Endpointleri Eksik veya Belirsiz

UI cagrilari:

- `api/konfigurasyon/{KonfigurasyonId}`
- `api/konfigurasyon/{KonfigurasyonId}/parcalar`

Dosya:

- `VIZITLINK3D.UI/Bilesenler/Urunler/TeklifIstegiFormu.razor.cs`

Sorun:

- API route taramasinda `api/konfigurasyon` controller'i gorunmedi.
- Teklif formu konfigurasyon ozeti yukleyemezse musteri teklif akisi eksik kalir.

Yapilacak:

- `KonfigurasyonKontrolcu` eklenmeli veya UI mevcut dogru endpointlere cekilmeli.
- `MusteriKonfigurasyonu` ve `MusteriKonfigurasyonParcasi` DB'den okunmali.

### 3.3 3D Parca API Akisi Build Duzelmeden Dogrulanamiyor

UI cagrilari:

- `api/uc-boyut/modeller/{modelId}/parcalar`
- `api/uc-boyut/modeller/parcalar/{id}`

Kritik bagli hata:

- Build'i kiran `UrunParcaEslemesi.cs`, 3D parca/esleme model ailesine ait.

Yapilacak:

- Once model namespace/build hatasi giderilmeli.
- Sonra 3D parca CRUD endpointleri gercek DB ile test edilmeli.
- Model analiz sonucu parca listesine donusuyor mu kontrol edilmeli.

## 4. Frontend ve Admin Eksikleri

### 4.1 Inline Style Hala Cok Fazla

Tekrar kontrol edilen ana dosyalar:

- `VIZITLINK3D.UI/Pages/AnaSayfa.razor`
- `VIZITLINK3D.UI/Layout/VIZITLINK3DDuzen.razor`
- `VIZITLINK3D.UI/Layout/AdminDuzen.razor`
- `VIZITLINK3D.UI/Pages/Admin/*.razor`
- `VIZITLINK3D.UI/Pages/Vitrin/PiedraKonfigurator.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/*.razor`
- `VIZITLINK3D.UI/Bilesenler/Anasayfa/*.razor`
- `VIZITLINK3D.UI/Bilesenler/AI/*.razor`

Sorun:

- AGENTS kurallarina gore `.razor` icinde inline style yasak veya cok sinirli olmali.
- Tema token sistemi varken renk/font/bosluk dogrudan markup icinde veriliyor.

Yapilacak:

- Stil kurallari `wwwroot/css/sistem/` altina tasinmali.
- Dinamik renk swatch gibi zorunlu alanlar disinda inline style kalmamali.

### 4.2 Hardcoded Metinler Hala Var

Ornekler:

- Admin yeni ekranlarinda `Ara...`, `Malzeme Filtresi`, `Silme Onayı`, `Evet, Sil`, `Kayıt başarıyla silindi.` gibi metinler dogrudan yaziliyor.
- Tema, 3D, PDF, RAL, kaplama, teklif ekranlarinda dil anahtarlari eksik.

Yapilacak:

- Tum metinler `DilServisi.T("anahtar", "Varsayilan")` ile verilmeli.
- Yeni moduller icin ceviri seedleri eklenmeli.

### 4.3 `eval` Kullanimi Devam Ediyor

Dosya:

- `VIZITLINK3D.UI/Pages/Admin/CanliSohbet.razor.cs`

Sorun:

```csharp
JS.InvokeVoidAsync("eval", "var el = document.getElementById('sohbet-alani'); ...");
```

Yapilacak:

- `SohbetArayuzServisi` veya JS wrapper modulu yazilmali.
- Direkt `eval` kaldirilmali.

### 4.4 `DateTime.Now` Kullanimi Devam Ediyor

Tespit edilen dosyalar:

- `VIZITLINK3D.Api/Moduller/Medya/Kontrolcu/PdfTeklifKontrolcu.cs`
- `VIZITLINK3D.UI/Layout/VIZITLINK3DDuzen.razor`

Yapilacak:

- API tarafinda `DateTime.UtcNow`.
- UI footer icin UTC veya merkezi zaman servisi.

## 5. Paket ve Altyapi Notlari

### 5.1 ImageSharp Guvenlik Uyarisi Onceki Kontrolde Vardi

Onceki build/test ciktisinda:

- `SixLabors.ImageSharp 2.1.10` icin orta seviye guvenlik acigi uyarisi vardi.

Yeni kontrolde build erken kirildigi icin paket uyarilari gorunmedi.

Yapilacak:

- Build duzeldikten sonra paket uyarisi tekrar kontrol edilmeli.
- ImageSharp guvenli surume guncellenmeli.

### 5.2 TODO / Gecici Isler

Tespitler:

- `LisansDogrulamaMiddleware`: production lisans kontrolu TODO.
- `YoutubeMetadataServisi`: oEmbed API TODO.
- `MedyaKontrolcu`: JWT'den kullanici alma TODO.
- `MedyaHavuzu`: yeni klasor dialog TODO.
- `PdfIcerikCozumleyici`: gercek PDF isleme TODO.

Yapilacak:

- Bu TODO'lar issue/is paketi olarak acilmali.
- Production etkisi olanlar P0/P1'e alinmali.

## 6. Guncel Oncelik Listesi

### P0 - Ust Banner, Menu, SuperAdmin ve Urun Giris Sorunlari

Kullanici tarafindan ek bildirilen ve tekrar incelenen kritik eksikler:

1. Admin ust banner yapisi bozuk veya endustriyel seviyede degil.
2. Admin sol menu ve sag panel gorunuyor, fakat gercek rol/yetki/canli veri entegrasyonu eksik.
3. Urun/kapi/dolap girisinde model yuklenmesine ragmen model onizlemesi her zaman gorunmuyor.
4. Inglizce terimler var: `High Gloss`, `OK`, `SignalR`, `API`, bazi kategori/metinler ve teknik etiketler.
5. Resim logosu ve model logosu/ikonlari karisik; ana gorsel, medya havuzu, model dosyasi ve 3D onizleme ayrimi net degil.
6. Model ve resim dosyalari localden yuklenebilmeli, medyaya kaydolmali ve hemen onizlenmeli.
7. Frontend on yuz menu yapisi tamamen admin tarafindan ayarlanabilir olmali.
8. SuperAdmin panelde her seyi ekleyebilmeli, kaldirabilmeli, siralayabilmeli ve yetkilendirebilmeli.

### P0.1 Admin Ust Banner Eksigi

Dosya:

- `VIZITLINK3D.UI/Layout/AdminDuzen.razor`

Mevcut durum:

- AppBar var.
- Marka, Ctrl+K, canlı akis butonu, bildirim, siteye git ve cikis butonlari var.
- Sag aktivite paneli var ama icerigi yer tutucu.

Sorunlar:

- Ust banner sayfa baglamini gostermiyor: aktif sayfa basligi, breadcrumb, modül durumu ve hizli eylemler yok.
- Bildirim sayaci statik olabilir; gercek `BildirimHub`, audit log veya admin olaylariyla bagli oldugu dogrulanmadi.
- Kullanici alani sadece `Yönetici / admin` olarak sabit gorunuyor.
- Rol bilgisi, SuperAdmin/Admin ayrimi, profil menusu, yetki bazli buton gosterimi yok.
- Ust bar mobilde tasma, daralma veya ikon yigilmasi riski tasiyor.

Yapilacak:

- `AdminUstBanner` bileseni olusturulmali.
- Aktif sayfa basligi ve breadcrumb route/menu verisinden gelmeli.
- Kullanici bilgisi JWT/session servisinden gelmeli.
- SuperAdmin icin ayri rozet ve gelismis menuler gorunmeli.
- Bildirim zili gercek `BildirimHub` veya bildirim API'sine baglanmali.
- Ust banner kompakt, sabit, responsive ve tasmasiz olmali.

Kabul kriteri:

- Admin herhangi bir sayfadayken ust barda aktif modül, kullanici, rol, bildirim ve hizli eylem net gorunmeli.
- SuperAdmin ve Admin farkli yetkiyle farkli aksiyonlar gormeli.

### P0.2 Admin Menu Yapisi Hala Yetki ve Hiyerarsi Acisindan Eksik

Dosyalar:

- `VIZITLINK3D.UI/Layout/AdminDuzen.razor`
- `VIZITLINK3D.UI/Pages/Admin/MenuYonetimi.razor.cs`
- `VIZITLINK3D.Api/Kontrolcüler/Sistem/MenuKontrolcu.cs`

Mevcut durum:

- Admin menuleri `api/menu/konum/Admin` uzerinden dinamik geliyor.
- Menu yonetiminde ekle, duzenle, sil, durum degistir, sira degistir var.

Sorunlar:

- Menu yonetimi Baslik alanina `"— "` prefix'i ekleyerek alt menuyu gorsellestiriyor; bu veri ile gorunumu karistiriyor.
- Silme metni "kalici olarak silinecektir" diyor; kural soft delete olmali.
- Menu kaydinda `Konum`, `Ikon`, `UstMenuId`, `Sira`, `AktifMi` var ama rol/yetki bazli gorunurluk net degil.
- SuperAdmin tum menuleri ekleyip kaldirabilmeli; Admin yalniz yetkili oldugu menuleri gorebilmeli.
- Menu ikon secimi serbest text gibi; ikon secici veya kontrollu ikon listesi olmali.
- Menude hem Admin hem Public konumlari ayni yonetimde net ayrilmali.

Yapilacak:

- Menu modeline veya iliskili tabloya rol/yetki alanlari eklenmeli:
  - `GerekliRol`
  - `SuperAdminGerekliMi`
  - `YetkiAnahtari`
  - `KilitliMi`
  - `SistemMenusuMu`
- Menu yonetiminde agac/tree view kullanilmali.
- Drag/drop veya yukari/asagi sira degistirme gercek hiyerarsiye gore calismali.
- Soft delete uygulanmali.
- Public ve Admin menu konumu filtrelenebilir olmali.

Kabul kriteri:

- SuperAdmin admin panelinde tum menuleri ekleyip kaldirabilmeli.
- Admin sadece yetkili oldugu menu ve modulleri gormeli.
- Public on yuz menusu admin tarafindan siralanabilmeli ve pasife alinabilmeli.

### P0.3 Frontend On Yuz Menu Admin'den Tam Ayarlanabilir Olmali

Dosya:

- `VIZITLINK3D.UI/Layout/VIZITLINK3DDuzen.razor`

Mevcut durum:

- Masaustu ve mobil menu `_menuOgeleri` uzerinden dinamik geliyor.
- Ancak footer hizli baglantilar ve kategori linkleri sabit yazilmis.

Sorunlar:

- Header menu dinamik olsa bile footer menusu sabit.
- Footer kategori linkleri admin tarafindan yonetilemiyor.
- Linklerde yazim sorunu var: `kapı-modelleri` gibi Turkce karakterli URL kullanimi riskli.
- Kategoriler sabit: Membran, Lake, Laminant, Melamin, Kaplama.
- Blog, KVKK, gizlilik gibi bazi metinler hardcoded veya kismen hardcoded.
- On yuz menusu admin panelden "header/footer/mobil/CTA/sosyal" olarak tam ayrilmiyor.

Yapilacak:

- Menu konumlari netlestirilmeli:
  - `PublicHeader`
  - `PublicFooterHizli`
  - `PublicFooterKategori`
  - `PublicMobil`
  - `AdminSol`
  - `AdminUst`
- `VIZITLINK3DDuzen` footer linklerini de API'den almali.
- URL slug'lari ASCII olmali: `kapi-modelleri`, `kapak-sistemleri`.
- Admin menu yonetiminde konum filtresi ve onizleme olmali.

Kabul kriteri:

- On yuz header, mobil drawer ve footer menuleri admin panelden degistirildiginde frontend'e yansimali.
- Sabit kategori linki kalmamali.

### P0.4 Urun/Kapi/Dolap Girisinde Model Gorunmuyor

Dosya:

- `VIZITLINK3D.UI/Pages/Admin/KapakModelFormu.razor`

Mevcut durum:

- Ana gorsel icin `MudFileUpload` var.
- 3D model icin `MudFileUpload` var.
- `Model.ModelDosyaYolu` doluysa `UcBoyutGoruntuleyici` render ediliyor.

Sorunlar:

- 3D onizleme sadece `Model.ModelDosyaYolu` doluysa aciliyor; upload sonrasi yol local/medya URL olarak dogru set edilmiyorsa model gorunmez.
- GLB yukleme ile medya havuzu/API kaydi arasindaki sonuc net degil.
- Model gorunmezse hata mesaji yok.
- Viewer yuklenemeyen modelde sadece bos alan veya kalici loading gosterebilir.
- `UcBoyutGoruntuleyici` icinde model hata callback/status UI standardi zayif.
- Kapak/kapi formu eski `KapakModeliDto` ve yeni genel urun sistemi arasinda kalmis gorunuyor.

Yapilacak:

- Upload sonrasi API cevabindan dosya yolu kesin olarak `Model.ModelDosyaYolu` alanina yazilmali.
- Local yukleme akisi:
  - Dosya sec
  - API/medya havuzuna yukle
  - Donen local URL'i modele ata
  - 3D viewer'i yeniden baslat
  - Hata varsa snackbar ve hata paneli goster
- Ana gorsel ve 3D model alanlari gorsel olarak ayrilmali:
  - Ana gorsel
  - Galeri gorselleri
  - 3D model dosyasi
  - 3D onizleme
  - Model analiz/parca esleme
- `KapakModelFormu` yeni `UrunYonetimi` ve `UcBoyutModelYonetimi` ile cakismayacak sekilde yeniden konumlandirilmali.

Kabul kriteri:

- Admin kapi/dolap/kapak urunu girerken GLB localden yuklenir yuklenmez 3D onizleme gorunmeli.
- Model yuklenemezse sebep acikca gorunmeli.

### P0.5 Resim Logosu ve Model Logosu/Kutusu Karisik

Dosya:

- `KapakModelFormu.razor`
- `UcBoyutModelYonetimi.razor`
- `MedyaHavuzu.razor`

Sorun:

- Ana gorsel yukleme ve 3D model yukleme ayni formda ama ayrim yeterince net degil.
- Resim onizleme kutusu var, model onizleme kutusu var; ancak ikon/metin/yardimci aciklama yetersiz.
- Medya havuzundan secilen gorsel ile local yuklenen gorsel ayni akisa baglanmali.
- 3D model dosyasi icin medya havuzundan secme ve local yukleme ayrimi net olmali.

Yapilacak:

- Admin urun formunda sekmeler:
  - `Temel Bilgi`
  - `Gorseller`
  - `3D Model`
  - `Renkler ve Kaplamalar`
  - `SEO`
- Gorseller sekmesinde ana gorsel, galeri, PDF kaynak gorselleri.
- 3D sekmesinde GLB local yukle, medya havuzundan sec, onizle, analiz et.
- Her alan icin ayri ikon ve aciklama.

### P0.6 Ingilizce Terimler ve Teknik Etiketler Temizlenmeli

Tespit edilen ornekler:

- `High Gloss`
- `OK`
- `API`
- `SignalR`
- `Ctrl+K`
- `Blog` bazi yerlerde cevirisiz
- `Admin`, `SuperAdmin` rol adlari kullaniciya ham gosterilebilir
- `Model`, `Upload`, `GLB`, teknik terimler yardimsiz kalabilir

Yapilacak:

- Kullaniciya gorunen terimler Turkce olmalı:
  - `High Gloss` yerine `Yuksek Parlak`
  - `OK` yerine `Sorunsuz` veya `Cevrimici`
  - `API` gerekiyorsa `Servis`
  - `SignalR` gerekiyorsa `Canli Baglanti`
  - `Upload` yerine `Yukle`
- Teknik terimler gerekiyorsa tooltip ile aciklanmali.
- Tum metinler `DilServisi.T()` ile olmali.

Kabul kriteri:

- Admin ve public yuzde kullaniciya ham Ingilizce teknik terim gosterilmemeli.

### P0.7 SuperAdmin Her Seyi Yonetebilmeli

Gereken yetenekler:

- Tum admin menulerini ekle/sil/duzenle.
- Public header/footer/mobil menuleri yonet.
- Urun ailesi, kategori, urun, model, parca, RAL, malzeme, kaplama, PDF katalog, teklif, tema, dil, SEO ve kullanicilari yonet.
- Her modulu aktif/pasif yap.
- Admin kullanicilarina rol/yetki ata.
- Kritik sistem menulerini kilitle veya ac.
- Silinen kayitlari geri al.

Eksik alanlar:

- Yetki modeli menulerle tam bagli degil.
- Admin layout kullanici rolune gore kendini uyarlamiyor.
- SuperAdmin icin "tum paneli duzenleme modu" yok.

Yapilacak:

- Rol/yetki tablolari veya mevcut `Rol` enum'u genisletilmeli.
- Menu ve moduller yetki anahtari ile baglanmali.
- SuperAdmin gorunumu:
  - menu duzenleme kisayolu
  - modül aktif/pasif paneli
  - sistem menusu kilitleri
  - geri alma/cop kutusu
- Admin/SuperAdmin ayrimi UI'da net gorunmeli.

Kabul kriteri:

- SuperAdmin panelden bir menu eklediginde hem admin hem public menude, secilen konuma gore aninda gorunmeli.
- Admin yetkisi olmayan bolume girememeli.

### P0 - Build'i Geri Getir

1. `VIZITLINK3D.Ortak/Modeller/UrunParcaEslemesi.cs` derleme hatalarini gider.
2. Ortak projeden EF Core using'ini kaldir.
3. `EntityBase` veya audit namespace sorununu cozumle.
4. `UrunUcBoyutParcasi` namespace/konum sorununu duzelt.
5. `dotnet build VIZITLINK3D.slnx` calistir.
6. `dotnet test VIZITLINK3D.slnx` calistir.

### P1 - PDF ve Konfigurasyon Akisini Gercek Hale Getir

1. `PdfIcerikCozumleyici` gercek sayfa sayisi ve cozumleme yapsin.
2. `PdfGorselCikarici` gercek medya/gorsel kaydi uretsin.
3. `api/konfigurasyon` endpointlerini ekle veya UI'yi dogru endpointlere cek.
4. Teklif formunun konfigurasyon ozeti gercek veriyle gelsin.

### P2 - 3D Model ve Parca Esleme

1. GLB upload gercek medya kaydi yapsin.
2. Model analiz JSON'u DB'ye kaydedilsin.
3. Parca CRUD endpointleri gercek calissin.
4. Parca-RAL-malzeme-kaplama izinleri public konfiguratore yansin.

### P3 - Admin/Frontend Kalite

1. Inline style temizligi.
2. Hardcoded metinleri `DilServisi.T()` ile degistirme.
3. `eval` kaldirma.
4. `DateTime.Now` kaldirma.
5. Admin sayfalarini ortak endustriyel CRUD desenine tasima.

## 7. Guncel Kabul Kriterleri

Bu dosyadaki eksikler tamamlandi sayilmaz, ta ki:

- `dotnet build VIZITLINK3D.slnx` hatasiz calisana kadar.
- `dotnet test VIZITLINK3D.slnx` tekrar basarili olana kadar.
- PDF katalog yukleme/cozumleme gercek sonuc uretene kadar.
- `api/konfigurasyon` akisi teklif formuyla uyumlu calisana kadar.
- 3D model upload, analiz ve parca esleme DB ile calisana kadar.
- RAL, malzeme ve kaplama secimleri parca bazli public konfiguratore yansiyana kadar.
- Admin yeni modulleri hardcoded metin/inline style agirligindan temizlenene kadar.
- `eval` ve kritik `DateTime.Now` kullanimlari kaldirilana kadar.

## 8. Bir Sonraki Model Icin Net Ilk Is

1. Sadece `UrunParcaEslemesi.cs` ve gerekiyorsa ilgili namespace/model dosyalarina odaklan.
2. Build'i tekrar yesile cek.
3. Build yesile donmeden PDF/3D/UI refactor'a girme.
4. Build sonrasi `PdfKatalog` ve `Konfigurasyon` endpointlerini ele al.
