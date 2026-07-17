# VIZITLINK3D 3D Model ve Endustriyel Urun Konfigurator Plani

> Amaç: VIZITLINK3D icin GLB/GLTF 3D modelleri, PDF katalogdan cikan urun gorsellerini, RAL renklerini, parca bazli malzeme/renk secimini ve admin tarafindan tam yonetilen urun detay sayfalarini tek endustriyel sisteme toplamak.
> Bu dosya, projede calisacak modellerin hatasiz uygulama yapmasi icin kapsamli is emri ve kabul kriteridir.

## 0. Zorunlu Okuma Sirasi

Kod yazmadan once su dosyalar sirayla okunacak:

1. `AGENTS.md`
2. `AjanKurallari/00_PROJE_BILGISI.md`
3. `AjanKurallari/03_Razor_MudBlazor_Blazor10.md`
4. `AjanKurallari/04_CSS_Tema_Stitch_Entegrasyonu.md`
5. `AjanKurallari/05_Veritabani_EFCore10.md`
6. `AjanKurallari/06_API_Servisler_MediatR.md`
7. `AjanKurallari/08_Performans_Cache_Render.md`
8. `AjanKurallari/10_Test_Derleme_Pipeline.md`
9. `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md`

Aktif proje bilgisi:

- Proje: VIZITLINK3D
- Sektor: kapi/mobilya
- Tema: Industrial Luxury
- Admin: `/admin`
- UI port: `5013`
- API port: `5015`
- 3D dosya tipi: `.glb`, `.gltf`
- Medya dosya tipleri: resim, PDF, GLB, video
- UI kutuphanesi: MudBlazor
- 3D motor: Three.js, sadece `UcBoyutServisi` wrapper uzerinden
- Dil: `DilServisi.T()` ve DB/FusionCache tabanli ceviri
- Stil: `wwwroot/css/sistem/` altinda token tabanli CSS

## 1. Mevcut Durum

Projede 3D icin kullanilabilecek temel parcalar var:

- `VIZITLINK3D.UI/Servisler/UcBoyutServisi.cs`
  - GLB/GLTF model baslatma
  - model degistirme
  - ekran goruntusu alma
  - parca analizi
  - parca gorunurluk
  - parca renk
  - parca malzeme
  - kapak derece/acilma
  - isik ayari
  - parca secim callback
- `VIZITLINK3D.UI/Bilesenler/UcBoyutGoruntuleyici.razor`
  - gorsel/3D tab gecisi
  - kamera sifirlama
  - otomatik dondurme
  - tam ekran
- `VIZITLINK3D.UI/Pages/Vitrin/PiedraKonfigurator.razor`
  - banyo dolabi icin deneysel parca secimi
  - parca renk/malzeme
  - kapak acisi
  - isik ayari
- `VIZITLINK3D.UI/Bilesenler/RenkSecici.razor`
  - RAL benzeri renk secimi var fakat islevsel urun/parca baglantisi eksik
- `VIZITLINK3D.Api/Moduller/Medya/Servisler/MedyaServisi.cs`
  - resim, PDF, GLB yukleme tipi taniniyor
- `VIZITLINK3D.Api/Moduller/Medya/Kontrolcu/PdfTeklifKontrolcu.cs`
  - teklif PDF uretimi var fakat urun konfigurasyonu ve 3D ekran goruntusu ile tam entegre degil

Eksik olan ana parca:

- Urun aileleri, 3D modeller, PDF kaynaklari, resimler, parcalar, RAL renkleri, malzemeler, template ve dil yapisi tek veri modeliyle bagli degil.

## 2. Hedef Urun Aileleri

Sistem ilk etapta su endustriyel urun ailelerini destekleyecek:

1. Dusakabin
2. Banyo dolabi
3. Vestiyer
4. Kapi
5. Dolap kapagi / mobilya kapagi
6. Tamamlayici aksesuar

Her urun ailesi admin tarafinda ayarlanabilir olacak. Yeni urun ailesi eklemek icin kod degistirmek zorunda kalinmayacak; yalniz yeni parca sablonu ve urun tipi tanimlanacak.

## 3. Urun Ailesi Parca Sablonlari

### 3.1 Dusakabin

Parca gruplari:

- Cam panel
- Sabit cam
- Surme cam
- Menteseli cam
- Aluminyum profil
- Kose profil
- Ray
- Kulp
- Menteşe
- Conta
- Tekne opsiyonu

Renk/malzeme kurallari:

- Cam: seffaf, füme, bronz, desenli, buzlu
- Aluminyum: krom, mat siyah, gold, satine, antrasit
- Kulp: metal renkleri, RAL veya hazir kaplama
- Conta: genelde sabit malzeme, musteri degistiremez

### 3.2 Banyo Dolabi

Parca gruplari:

- Ana govde
- Kapak sol/sag
- Cekmece onleri
- Ust tabla
- Lavabo
- Musluk
- Ayna
- Aydinlatma
- Ayak
- Kulp
- Raf

Renk/malzeme kurallari:

- Govde ve kapak: RAL renkleri, ahsap doku, lake, membran, mat/parlak
- Ust tabla: mermer, porselen, kompakt, ahsap, renk secenekleri
- Lavabo: porselen, mat beyaz, parlak beyaz, siyah
- Musluk/kulp: krom, gold, siyah, bronz
- Ayna: standart, ledli, yuvarlak, dikdortgen

### 3.3 Vestiyer

Parca gruplari:

- Govde
- Kapaklar
- Ayna
- Askilik
- Raf
- Cekmece
- Oturma alani
- Kulp
- Ayak

Renk/malzeme kurallari:

- Govde/kapak: RAL, ahsap doku, lake, mat/parlak
- Ayna: normal, füme, bronz
- Kulp/askilik: metal kaplama

### 3.4 Kapi

Parca gruplari:

- Kapi kanadi
- Kasa
- Pervaz
- Koseme / doseme uyum bolgesi
- Kaplama yuzeyi
- Cam bolme
- Kulp
- Kilit
- Menteşe
- Esik

Renk/malzeme kurallari:

- Kapi kanadi: RAL, lake, ahsap kaplama, membran, panel doku
- Kasa/pervaz: ayni renk veya kontrast
- Kulp/menteşe/kilit: metal kaplama
- Cam bolme: seffaf, buzlu, füme, bronz

### 3.5 Dolap Kapagi / Mobilya Kapagi

Parca gruplari:

- Kapak yuzeyi
- Cerceve
- Panel
- Kenar bant
- Kulp
- Menteşe
- Dekoratif cizgi

Renk/malzeme kurallari:

- Kapak yuzeyi: RAL, ahsap, lake, membran, akrilik, mat/parlak
- Kenar bant: ayni renk veya kontrast
- Kulp: metal, gizli kulp, profil kulp

## 4. Admin Panel Hedefleri

Admin panelinden her sey ayarlanabilir olacak:

- Urun ailesi tanimi
- Urun kategorisi
- Urun karti
- Urun detay template'i
- Urun resimleri
- PDF kaynaklari
- PDF'den cikan urun resimleri
- GLB/GLTF model dosyasi
- 3D model onizleme resmi
- Model parca analizi
- Parca isimlerini urun parca gruplarina esleme
- Hangi parca musteri tarafindan degistirilebilir?
- Hangi parca hangi RAL renklerini kullanabilir?
- Hangi parca hangi malzemeleri kullanabilir?
- Hangi parca gorunur/gizlenebilir?
- Hangi parca acilip kapanabilir?
- Hangi parca olcuye gore scale edilir?
- Hangi kombinasyonlar yasak?
- Varsayilan renk, malzeme, isik, kamera ve animasyon ayarlari
- Detay sayfasi animasyon template'i
- Teklif formu alanlari
- PDF teklif cikti template'i
- SEO ve coklu dil alanlari

## 5. Musteri Deneyimi Akisi

### 5.1 Urun Listeleme

Ilk ekranda urunler listelenecek:

- Kategori filtreleri: dusakabin, banyo dolabi, vestiyer, kapi, kapak
- Urun kartinda ana gorsel
- 3D var etiketi
- PDF kaynakli katalog etiketi
- Renk sayisi
- Malzeme sayisi
- One cikan / yeni etiketi
- Hizli onizleme

Kabul kriteri:

- Liste responsive olacak.
- Urun kartlari kaymayacak.
- Resimler lazy loading ile gelecek.
- 3D modeli olmayan urunde 3D butonu gosterilmeyecek.

### 5.2 PDF Kaynakli Urun Detayi

Admin PDF yuklediginde sistem:

- PDF'i medya havuzuna kaydedecek.
- PDF sayfalarini analiz edecek.
- PDF icindeki urun gorsellerini cikartacak.
- Her gorseli medya olarak kaydedecek.
- Admin'e "urun olustur" veya "mevcut urune bagla" secenegi verecek.
- PDF sayfa no ve kaynak bilgisini urun detaya baglayacak.

Not:

- PDF okuma ve resim cikarma is mantigi C# wrapper servis ile yapilacak.
- Python veya harici script yazilmayacak.
- Tercih edilen C# kutuphaneler icin wrapper: `PdfIcerikCozumleyici`.
- Kutuphane dogrudan controller veya Razor icinden cagrilmayacak.

### 5.3 Animasyonlu Detay Sayfasi

Detay sayfasinda:

- Urun gorsel hero alani
- PDF'den gelen gorseller galerisi
- 3D model sekmesi
- Parca secilebilir 3D model
- Renk/malzeme secim paneli
- Olcu secimi
- Aksesuar secimi
- Anlik fiyat/teklif ozeti
- Ekran goruntusu alma
- Teklif iste butonu

Kabul kriteri:

- Tum metinler `DilServisi.T()` ile gelecek.
- Animasyonlar wrapper uzerinden olacak.
- Mobilde 3D sahne ve secim paneli ergonomik olacak.
- 3D model yuklenirken skeleton/loading gosterilecek.
- `prefers-reduced-motion` tercihine uyulacak.

## 6. Veri Modeli Plani

Yeni modeller `VIZITLINK3D.Ortak/Modeller/Urunler/` altinda toplanacak. Tablo ve sutun adlari ASCII olacak.

### 6.1 Ana Entityler

Onerilen entityler:

- `UrunAilesi`
- `UrunKategori`
- `Urun`
- `UrunYerellestirme`
- `UrunMedya`
- `UrunPdfKaynagi`
- `PdfSayfaGorseli`
- `UrunUcBoyutModeli`
- `UrunUcBoyutParcasi`
- `UrunParcaGrubu`
- `UrunParcaEslemesi`
- `RenkKatalogu`
- `RalRengi`
- `Malzeme`
- `KaplamaSecenegi`
- `UrunParcaRenkSecenegi`
- `UrunParcaMalzemeSecenegi`
- `UrunKonfigurasyonSablonu`
- `UrunKonfigurasyonKurali`
- `MusteriKonfigurasyonu`
- `MusteriKonfigurasyonParcasi`
- `TeklifIstegi`
- `TeklifIstegiParcasi`

### 6.2 Urun

Zorunlu alanlar:

- `Id`
- `Slug`
- `Kod`
- `Ad`
- `KisaAciklama`
- `Aciklama`
- `UrunAilesiId`
- `UrunKategoriId`
- `AnaGorselMedyaId`
- `VarsayilanUcBoyutModeliId`
- `AktifMi`
- `OneCikanMi`
- `YeniMi`
- `SiraNo`
- `SeoBaslik`
- `SeoAciklama`
- `OlusturulmaTarihi`
- `GuncellenmeTarihi`
- `SilindiMi`
- `SilinmeTarihi`

Index:

- `IX_Urunler_Slug_Unique`
- `IX_Urunler_UrunAilesiId_AktifMi`
- `IX_Urunler_UrunKategoriId_AktifMi`

### 6.3 UrunUcBoyutModeli

Alanlar:

- `Id`
- `UrunId`
- `MedyaId`
- `ModelYolu`
- `OnizlemeMedyaId`
- `ModelTipi` (`Glb`, `Gltf`)
- `DosyaBoyutuByte`
- `Versiyon`
- `VarsayilanMi`
- `KameraAyarJson`
- `IsikAyarJson`
- `CevreAyarJson`
- `ModelAnalizJson`
- `AktifMi`
- `OlusturulmaTarihi`
- `GuncellenmeTarihi`
- `SilindiMi`

### 6.4 UrunUcBoyutParcasi

Bu tablo GLB icindeki mesh/grup adlarini adminin anlayacagi parca isimlerine baglar.

Alanlar:

- `Id`
- `UrunUcBoyutModeliId`
- `MeshAdi`
- `GorunenAd`
- `ParcaGrubuId`
- `SecilebilirMi`
- `RenklenebilirMi`
- `MalzemeDegisebilirMi`
- `GizlenebilirMi`
- `HareketliMi`
- `HareketTipi` (`KapakAc`, `CekmeceAc`, `Surme`, `Donme`, `Yok`)
- `MinDeger`
- `MaxDeger`
- `VarsayilanDeger`
- `VarsayilanRenkId`
- `VarsayilanMalzemeId`
- `SiraNo`
- `AktifMi`

Index:

- `IX_UrunUcBoyutParcalari_UrunUcBoyutModeliId_MeshAdi_Unique`
- `IX_UrunUcBoyutParcalari_ParcaGrubuId`

### 6.5 RAL Renk Modeli

Mevcut `RenkSecici` islevsel hale getirilecek ve DB kaynakli calisacak.

Entityler:

- `RenkKatalogu`
- `RalRengi`

`RalRengi` alanlari:

- `Id`
- `KatalogId`
- `Kod` (`RAL 9016`)
- `Ad`
- `HexKod`
- `Grup`
- `YuzeyTipi` (`Mat`, `Parlak`, `Saten`, `Metal`)
- `AktifMi`
- `SiraNo`

Kabul kriteri:

- Renkler hardcoded listeden degil API'den gelecek.
- Admin renk ekleyebilecek, duzenleyebilecek, pasife alabilecek.
- Urun parcalarinda hangi RAL renkleri kullanilabilir secilecek.
- Müşteri sadece adminin izin verdigi renkleri gorecek.

### 6.6 Malzeme ve Kaplama

Entityler:

- `Malzeme`
- `KaplamaSecenegi`

Malzeme ornekleri:

- Cam
- Aluminyum
- Metal
- Ayna
- Porselen
- Ahsap
- MDF
- Lake
- Membran
- Akrilik
- Kompakt

Kaplama ornekleri:

- Krom
- Mat siyah
- Gold
- Bronz
- Satine
- Antrasit
- Seffaf cam
- Füme cam
- Buzlu cam
- Ahsap doku

## 7. API Modulleri

Yeni moduller Vertical Slice yapisinda kurulacak.

### 7.1 Urunler Modulu

Konum:

- `VIZITLINK3D.Api/Moduller/Urunler/`

Endpointler:

- `GET /api/urunler`
- `GET /api/urunler/{id}`
- `GET /api/urunler/slug/{slug}`
- `POST /api/urunler`
- `PUT /api/urunler/{id}`
- `DELETE /api/urunler/{id}`
- `GET /api/urunler/{id}/detay-sayfasi`

Her endpoint `Cevap<T>` donecek.

### 7.2 3D Model Modulu

Konum:

- `VIZITLINK3D.Api/Moduller/UcBoyut/`

Endpointler:

- `POST /api/uc-boyut/modeller/yukle`
- `GET /api/uc-boyut/modeller/{id}`
- `POST /api/uc-boyut/modeller/{id}/analiz-sonucu`
- `PUT /api/uc-boyut/modeller/{id}/parca-eslemeleri`
- `GET /api/uc-boyut/urun/{urunId}/konfigurator`
- `POST /api/uc-boyut/konfigurasyon`
- `POST /api/uc-boyut/konfigurasyon/{id}/ekran-goruntusu`

Not:

- GLB analizi istemci tarafinda Three.js ile yapilabilir, fakat sonuc API'ye kaydedilecek.
- Sunucu tarafinda dosya tipi, boyut, mime ve guvenlik kontrolleri yapilacak.

### 7.3 PDF Katalog Modulu

Konum:

- `VIZITLINK3D.Api/Moduller/PdfKatalog/`

Endpointler:

- `POST /api/pdf-katalog/yukle`
- `POST /api/pdf-katalog/{id}/cozumle`
- `GET /api/pdf-katalog/{id}/sayfalar`
- `POST /api/pdf-katalog/gorseller/{id}/urune-bagla`
- `POST /api/pdf-katalog/gorseller/{id}/urun-olustur`

Wrapper servisleri:

- `IPdfIcerikCozumleyici`
- `PdfIcerikCozumleyici`
- `IPdfGorselCikarici`
- `PdfGorselCikarici`

### 7.4 Renk ve Malzeme Modulu

Konum:

- `VIZITLINK3D.Api/Moduller/Renkler/`
- `VIZITLINK3D.Api/Moduller/Malzemeler/`

Endpointler:

- `GET /api/renkler/ral`
- `POST /api/renkler/ral`
- `PUT /api/renkler/ral/{id}`
- `DELETE /api/renkler/ral/{id}`
- `GET /api/malzemeler`
- `POST /api/malzemeler`
- `PUT /api/malzemeler/{id}`

## 8. Admin Sayfalari

Yeni veya yenilenecek admin sayfalari:

- `VIZITLINK3D.UI/Pages/Admin/UrunYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Admin/UrunDuzenle.razor`
- `VIZITLINK3D.UI/Pages/Admin/UrunAilesiYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Admin/UrunKategoriYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Admin/UcBoyutModelYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Admin/UcBoyutParcaEsleme.razor`
- `VIZITLINK3D.UI/Pages/Admin/PdfKatalogYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Admin/RalRenkYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Admin/MalzemeYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Admin/KonfigurasyonSablonuYonetimi.razor`
- `VIZITLINK3D.UI/Pages/Admin/TeklifSablonuYonetimi.razor`

Her sayfanin `.razor.cs` partial class dosyasi olacak.

Kural:

- `.razor` icinde `@code` yok.
- `.razor` icinde `<style>` yok.
- Inline style yok.
- Tum metinler `DilServisi.T()` ile.
- Admin sayfalarinda `@attribute [Authorize(Roles = "Admin")]`.

## 9. Admin 3D Model Yukleme Akisi

1. Admin urun olusturur veya mevcut urunu acar.
2. GLB/GLTF dosyasini medya havuzuna yukler.
3. Sistem dosya tipini, boyutunu ve mime tipini dogrular.
4. Model 3D onizleme alaninda acilir.
5. Admin "Modeli analiz et" der.
6. Three.js wrapper modelin mesh/grup adlarini cikarir.
7. Mesh listesi admin panelinde tablo olarak gosterilir.
8. Admin her mesh'i parca grubuna baglar.
9. Her parca icin su alanlari ayarlar:
   - gorunen ad
   - secilebilir mi?
   - renklenebilir mi?
   - malzeme degisebilir mi?
   - gizlenebilir mi?
   - hareketli mi?
   - hareket tipi
   - varsayilan renk
   - izinli RAL renkleri
   - izinli malzemeler
10. Admin kaydeder.
11. Musteri detay sayfasinda sadece izin verilen secenekleri gorur.

Kabul kriteri:

- Mesh adlari admin tarafinda kaybolmayacak.
- Bir GLB modeli yeni versiyonla degistirilirse eski parca eslemeleri korunmaya calisilacak.
- Eslesmeyen meshler admin panelinde uyarilacak.

## 10. Musteri Konfiguratoru

Yeni bilesenler:

- `VIZITLINK3D.UI/Bilesenler/Urunler/UrunListeKart.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/UrunDetayHero.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/UrunMedyaGalerisi.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/UrunKonfigurator.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/UcBoyutParcaPaneli.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/ParcaRenkPaneli.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/MalzemeSecimPaneli.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/KonfigurasyonOzeti.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/TeklifIstegiFormu.razor`

Konfigurator ozellikleri:

- Parca tiklayinca panelde secili parca gorunecek.
- Parca icin izinli RAL renkleri listelenecek.
- Parca icin izinli malzemeler listelenecek.
- Hareketli parca varsa slider veya preset butonlar gelecek.
- Kapi/kapi kapagi icin acilma animasyonu.
- Dusakabin icin surme/menteşe animasyonu.
- Banyo dolabi icin cekmece/kapak acilma animasyonu.
- Vestiyer icin kapak/cekmece/ayna gorunum secimi.
- Secimler URL veya kayitli konfigurasyon olarak saklanabilecek.
- Kullanici teklif isterken secim ozeti API'ye gidecek.

## 11. Template ve Dil Yapisi

### 11.1 Detay Template

Admin urun ailesine gore detay template sececek:

- `Endustriyel3D`
- `KatalogGorselAgirlikli`
- `MinimalTeklif`
- `TeknikOzellikAgirlikli`
- `BanyoKonfigurator`
- `DusakabinKonfigurator`
- `KapiKonfigurator`
- `KapakKonfigurator`

Template ayarlari DB'de tutulacak:

- hero aktif mi?
- 3D ilk acilsin mi?
- gorsel mi 3D mi varsayilan?
- teknik ozellik bolumu aktif mi?
- PDF kaynak bolumu aktif mi?
- benzer urunler aktif mi?
- teklif formu aktif mi?
- animasyon tipi
- renk paneli konumu
- mobil panel davranisi

### 11.2 Dil Anahtar Standardi

Ornek anahtarlar:

- `urun.liste.baslik`
- `urun.detay.3d-gorunum`
- `urun.detay.gorseller`
- `urun.detay.teklif-iste`
- `urun.konfigurator.parca-sec`
- `urun.konfigurator.renk-sec`
- `urun.konfigurator.malzeme-sec`
- `urun.konfigurator.olcu-sec`
- `urun.konfigurator.konfigurasyon-ozeti`
- `admin.urunler.baslik`
- `admin.uc-boyut.model-yukle`
- `admin.uc-boyut.parca-esle`
- `admin.pdf-katalog.cozumle`
- `admin.ral-renk.baslik`

Kabul kriteri:

- Razor icinde hardcoded Turkce metin kalmayacak.
- Default metinler `DilServisi.T("anahtar", "Varsayilan")` ile yazilacak.
- Ceviri JSON dosyasi kullanilmayacak.

## 12. PDF'den Urun Cikarma Plani

PDF akisi:

1. Admin PDF yukler.
2. PDF medya olarak kaydedilir.
3. Cozumleme kuyruga alinir.
4. Sayfa gorselleri uretilir.
5. Sayfa icindeki gorseller ayrilir.
6. OCR veya metin katmani varsa urun kodu/ad tahmini alinir.
7. Admin onay ekraninda:
   - gorsel kirpma
   - urun adi
   - urun kodu
   - kategori
   - urun ailesi
   - mevcut urune bagla
   - yeni urun olustur
8. Kaydedilen gorseller urun medya galerisine eklenir.

Not:

- OCR kullanilacaksa harici servis dogrudan cagrilmayacak, wrapper servis olacak.
- AI ile urun bilgisi tahmini yapilacaksa `AI` modulu ve `AIGuvenlikServisi` uzerinden gidecek.
- Admin onayi olmadan otomatik yayina alinmayacak.

## 13. Teklif ve Cikti Sistemi

Teklif istegi su verileri kaydedecek:

- UrunId
- KonfigurasyonId
- Secilen parcalar
- Secilen RAL renkleri
- Secilen malzemeler
- Olculer
- 3D ekran goruntusu
- PDF kaynak bilgisi
- Musteri ad soyad
- Telefon
- Eposta
- Not
- OlusturulmaTarihi

PDF teklif ciktisi:

- Marka bilgisi
- Urun resmi
- 3D ekran goruntusu
- Secim ozeti
- RAL kodlari
- Malzeme listesi
- Olculer
- Musteri bilgileri
- Uyari metinleri

Mevcut `PdfTeklifKontrolcu` yeniden tasarlanacak:

- Controller 3 satir kuralina cekilecek.
- `Cevap<T>` disiplini uygulanacak.
- QuestPDF kullanimi wrapper/servis icine alinacak.
- `DateTime.Now` yerine `DateTime.UtcNow` kullanilacak.
- Hardcoded renk/fontlar tema token veya ayar uzerinden beslenecek.

## 14. 3D Teknik Gereksinimler

GLB model kabul kurallari:

- Dosya boyutu `00_PROJE_BILGISI.depolama.max_glb_mb` sinirini asmayacak.
- Model dosya adlari guvenli hale getirilecek.
- Mesh adlari mumkunse anlamli olmasi icin admin uyarilacak.
- Draco/KTX2 optimizasyonu desteklenecek.
- Model onizleme resmi otomatik uretilebilecek.
- Ekran goruntusu urun galerisine kaydedilebilecek.

Three.js tarafinda gereken wrapper fonksiyonlari:

- `model_parca_listesi_al`
- `parca_vurgula`
- `parca_vurgu_temizle`
- `parca_renk`
- `parca_malzeme`
- `parca_gorunurluk`
- `parca_hareket`
- `kamera_preset_uygula`
- `sahne_ayar_uygula`
- `ekran_goruntusu_al`
- `model_optimizasyon_bilgisi_al`

Blazor tarafinda bu fonksiyonlar sadece `UcBoyutServisi` ile cagrilacak.

## 15. Guvenlik ve Dosya Kontrolleri

- GLB, PDF ve resim yuklemeleri yetkili admin endpointlerinden yapilacak.
- Dosya uzantisi tek basina yeterli sayilmayacak; mime ve icerik kontrol edilecek.
- Dosya adlari normalize edilecek.
- Dosya yolu traversal riskine kapali olacak.
- Yuklenen dosyalar dogrudan calistirilabilir script icermemeli.
- PDF icindeki link/script/ekler pasif hale getirilecek veya yok sayilacak.
- Hassas veri loglanmayacak.
- Admin islemleri audit log'a yazilacak.

## 16. Performans Plani

- Urun listeleme `AsNoTracking` ve projection ile calisacak.
- Urun detay verisi cache'lenecek.
- RAL renk listesi cache'lenecek.
- GLB dosyalari CDN/static cache ile servis edilecek.
- Resimler ImageSharp.Web parametreleri ile optimize edilecek.
- 3D model lazy load olacak; liste sayfasinda GLB yuklenmeyecek.
- Detayda once resim, sonra 3D model yuklenecek.
- Mobilde dusuk kalite model veya azaltimli texture secenegi olacak.
- Buyuk parca listeleri `Virtualize` ile gosterilecek.

## 17. Test Plani

Her ana ozellik icin minimum 5 test:

### 17.1 Urun API Testleri

- Urun olustur basarili
- Zorunlu alan eksikse dogrulama hatasi
- Slug unique kontrolu
- Yetkisiz admin islemi reddedilir
- Listeleme aktif urunleri getirir

### 17.2 3D Model Testleri

- GLB medya tipi dogru kaydedilir
- Model urune baglanir
- Parca esleme kaydedilir
- Izinli RAL listesi dogru doner
- Silinen model public detayda gorunmez

### 17.3 PDF Katalog Testleri

- PDF yukleme basarili
- PDF cozumleme kaydi olusur
- Sayfa gorseli urune baglanir
- Admin onayi olmadan urun yayinlanmaz
- Hatalı PDF guvenli hata doner

### 17.4 RAL ve Malzeme Testleri

- RAL rengi olusturulur
- Hex format dogrulanir
- Parca renk izinleri kaydedilir
- Pasif renk musteri panelinde gorunmez
- Malzeme izinleri parca bazli filtrelenir

### 17.5 Blazor/bUnit Testleri

- Urun karti gorseli ve 3D etiketi gosterir
- Konfigurator izinli renkleri gosterir
- Parca secimi paneli acar
- Malzeme degisimi callback tetikler
- Teklif formu secim ozetiyle render olur

### 17.6 Playwright E2E

- Admin giris yapar, GLB yukler, parca esler
- Admin PDF yukler, gorseli urune baglar
- Musteri urun listesinde urunu gorur
- Musteri detayda 3D modeli acar
- Musteri renk/malzeme secip teklif ister

## 18. Uygulama Fazlari

### Faz 1: Veri ve Medya Temeli

1. `Urunler` model klasorunu olustur.
2. Urun, urun ailesi, kategori, urun medya, 3D model entitylerini ekle.
3. RAL ve malzeme entitylerini ekle.
4. DbContext ve migration hazirla.
5. Medya GLB/PDF kontrollerini guclendir.

### Faz 2: Admin Urun ve RAL Yonetimi

1. Urun yonetim sayfasi.
2. Urun duzenleme sayfasi.
3. RAL renk yonetimi.
4. Malzeme/kaplama yonetimi.
5. Urun-parca-renk/malzeme izinleri.

### Faz 3: 3D Model Yonetimi

1. GLB yukleme.
2. Model onizleme.
3. Mesh/parca analiz sonucu alma.
4. Parca esleme ekrani.
5. Kamera/isik/animasyon presetleri.
6. Model versiyonlama.

### Faz 4: PDF Katalog Entegrasyonu

1. PDF yukleme.
2. PDF sayfa/gorsel cikarimi.
3. Admin onay ekrani.
4. Gorselden urun olusturma.
5. Gorseli mevcut urune baglama.

### Faz 5: Public Urun Liste ve Detay

1. Urun listeleme sayfasi.
2. Urun detay sayfasi.
3. Medya galerisi.
4. 3D model linki/sekmesi.
5. Animasyonlu detay template sistemi.

### Faz 6: Musteri Konfiguratoru

1. Parca secim paneli.
2. RAL renk secimi.
3. Malzeme secimi.
4. Olcu ve aksesuar secimi.
5. Konfigurasyon ozeti.
6. Teklif istegi.

### Faz 7: Teklif ve Raporlama

1. Teklif kaydi.
2. 3D ekran goruntusu kaydi.
3. PDF teklif uretimi.
4. Admin teklif listesi.
5. Musteri takip durumu.

### Faz 8: Test, Performans, UI Kalite

1. Unit/integration/bUnit/E2E testleri.
2. Build/test dogrulama.
3. Masaustu/tablet/mobil ekran kontrolu.
4. 3D sahne bos kalma kontrolu.
5. GLB yukleme performans kontrolu.

## 19. Ilk Is Emri

Bir sonraki model su sirayla baslamali:

1. `VIZITLINK3D.Ortak/Modeller/Urunler/` altinda yeni entity taslaklarini olustur.
2. `VIZITLINK3D.Api/Moduller/Urunler/`, `UcBoyut`, `Renkler`, `Malzemeler` vertical slice klasorlerini kur.
3. RAL renklerini DB kaynakli hale getir ve mevcut `RenkSecici` bilesenini API verisine bagla.
4. `UcBoyutModelYonetimi` ve `UcBoyutParcaEsleme` admin sayfalarini tasarla.
5. Mevcut `PiedraKonfigurator` kodunu genelleserek `UrunKonfigurator` bilesenine donustur.
6. PDF katalog icin wrapper servis arayuzlerini ekle.
7. Build/test calistir ve eksikleri raporla.

## 20. Nihai Kabul Kriterleri

Sistem tamamlanmis sayilmaz, ta ki:

- Admin GLB yukleyip urune baglayana kadar.
- Admin 3D model parcalarini analiz edip esleyene kadar.
- Admin her parca icin RAL/malzeme izinlerini ayarlayana kadar.
- RAL renkleri DB'den gelip islevsel sekilde 3D parcaya uygulanana kadar.
- PDF'den cikan gorseller urun detayina baglanana kadar.
- Musteri urun listesinde gorselleri gorup detaya girebilene kadar.
- Detay sayfasinda animasyonlu 3D model linki/sekmesi calisana kadar.
- Dusakabin, banyo dolabi, vestiyer, kapi ve kapak icin parca sablonlari tanimlanana kadar.
- Musteri secimlerini teklif istegine donusturene kadar.
- Dil/template sistemi hardcoded metinsiz calisana kadar.
- Mobil/tablet/masaustu deneyimi bozulmadan calisana kadar.
- `dotnet build` ve `dotnet test` basarili olana kadar.

