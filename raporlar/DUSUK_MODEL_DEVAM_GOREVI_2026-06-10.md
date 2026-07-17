# VIZITLINK3D Devam Gorevi - Dusuk Model Icin

Tarih: 2026-06-10
Hedef: Daha dusuk maliyetli bir modelin projeyi bozmadan devam ettirebilmesi icin net gorev listesi.

## Mevcut Durum

- UI: `http://localhost:5013`
- API: `http://localhost:5015`
- Cozum: .NET 10 Blazor WebAssembly + API.
- Zorunlu ilk okuma: `AGENTS.md`, `AjanKurallari/00_PROJE_BILGISI.md`, goreve gore uzman dosya, `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md`.

## Bu Turda Yapilan Duzeltmeler

1. Urun detay route degisimi duzeltildi.
   - Dosya: `VIZITLINK3D.UI/Pages/UrunDetay.razor.cs`
   - Sorun: `/urun/a` sayfasindan detay icindeki baska urune tiklayinca Blazor ayni component instance uzerinde kalabiliyor, `OnInitializedAsync` tekrar calismadigi icin eski urun/model ekranda kalabiliyordu.
   - Cozum: `OnParametersSetAsync` ile `Slug` degisimi izleniyor. Slug degisince urun verisi, listeler, secili parca, renk/malzeme durumu ve 3D sahne sifirlaniyor.

2. 3D model okunurlugu iyilestirildi.
   - Dosya: `VIZITLINK3D.UI/wwwroot/js/uc-boyut-motoru.js`
   - Sorun: Ozellikle RAL 9016 gibi beyaz/acik renklerde kapak motifleri, freze cizgileri ve parca ayrimlari silik gorunuyordu.
   - Cozum: GLB ve parametrik model meshlerine hafif teknik kenar cizgisi eklendi. Isik dengesi daha az ambient, daha belirgin ana/kenar isik olacak sekilde ayarlandi. RAL malzemede `envMapIntensity` ve roughness dengelendi.

3. 3D JS cache kirildi.
   - Dosya: `VIZITLINK3D.UI/wwwroot/index.html`
   - Degisim: `js/uc-boyut-motoru.js?v=2` -> `v=3`

4. Urun kartlarinda kirik placeholder gorseli duzeltildi.
   - Dosyalar:
     - `VIZITLINK3D.UI/Bilesenler/Urunler/UrunListeKart.razor`
     - `VIZITLINK3D.UI/Bilesenler/Urunler/UrunListeKart.razor.cs`
     - `VIZITLINK3D.UI/wwwroot/css/sistem/bilesenler/kartlar.css`
   - Sorun: `/medya/placeholder-urun.jpg` yoktu ve 404 donuyordu.
   - Cozum: Gorsel yoksa artik `<img>` basilmiyor; sistem ici placeholder yuzeyi gosteriliyor.

5. Urunler sayfasindaki demo gorsel 404 duzeltildi.
   - Dosya: `VIZITLINK3D.UI/Pages/Urunler.razor`
   - Degisim: Olmayan `/medya/placeholder-urun.jpg` yerine mevcut `/medya/katalog/503/503-y.png` kullanildi.

## Test Sonuclari

- `node --check VIZITLINK3D.UI/wwwroot/js/uc-boyut-motoru.js`: Basarili.
- `dotnet build VIZITLINK3D.UI/VIZITLINK3D.UI.csproj --no-restore`: Basarili.
  - Mevcut uyari: `OpenMcdf 3.1.3` NU1902.
  - Mevcut uyari: `SharpCompress 0.46.3` NU1902.
- `dotnet build VIZITLINK3D.Api/VIZITLINK3D.Api.csproj --no-restore`: Basarili.
- `dotnet test VIZITLINK3D.Testler/VIZITLINK3D.Testler.csproj --no-restore`: 428 test basarili, 0 hata.
- HTTP smoke:
  - `/`, `/urunler`, `/urun/nrd-004`, `/katalog`, `/sertifikalar`: 200.
  - `api/urunler?dil=tr`, `api/VIZITLINK3D/kataloglar`, `api/menu/konum/AnaMenu`: 200.
  - Model dosyalari kontrol edildi:
    - `/models/katalog/04/nrd-boy-kpk-04.glb`: 200.
    - `/models/katalog/01/nrd-boy-kpk-01.glb`: 200.
  - Yeni JS:
    - `/js/uc-boyut-motoru.js?v=3`: 200 ve `TeknikKenarCizgisi` iceriyor.

## Bilinen Kalan Riskler

1. Browser plugin bu turda baslatilamadi.
   - Hata: in-app browser runtime baslangicinda sandbox kaynakli kopma.
   - Bu nedenle tam gorsel screenshot QA yapilamadi. HTTP/build/test kontrolleri yapildi.

2. Admin ve frontend CSS genel borcu var.
   - Statik taramada eski dosyalarda `!important`, hardcoded renk ve inline style kullanimi var.
   - Ozellikle `VIZITLINK3D.Api/wwwroot/css/...` icinde eski publish/static kopyalar da gorunuyor.
   - Yeni eklenen CSS bu kurali bozmayacak sekilde yazildi, fakat tum proje temiz degil.

3. Urun modelleri farkli GLB dosyalari donduruyor, fakat bazi katalog modelleri geometrik olarak cok benzer.
   - Ornek:
     - `nrd-004` -> `/models/katalog/04/nrd-boy-kpk-04.glb`
     - `nrd-001` -> `/models/katalog/01/nrd-boy-kpk-01.glb`
     - `nrd-124` -> `/models/katalog/124/nrd-124.glb`
   - Eger kullanici "hala ayni gorunuyor" derse model dosyalarinin gercek farklilik kalitesi incelenmeli.

4. Urun gorsel eksikleri veri kaynakli.
   - Bazi urunlerde `AnaGorselMedyaId` null.
   - UI kirik gorsel gostermeyecek hale getirildi, fakat admin/veri tarafinda gercek gorsel atanmasi gerekiyor.

## Dusuk Model Icin Sirali Gorevler

1. Once build ve test calistir.
   - `dotnet build VIZITLINK3D.UI/VIZITLINK3D.UI.csproj --no-restore`
   - `dotnet build VIZITLINK3D.Api/VIZITLINK3D.Api.csproj --no-restore`
   - `dotnet test VIZITLINK3D.Testler/VIZITLINK3D.Testler.csproj --no-restore`

2. Urun detay route testini yap.
   - `http://localhost:5013/urun/nrd-004` ac.
   - Detaydaki "Benzer Urunler", "En Cok Gezilenler" veya "Musterilerin Dikkatini Cekenler" kartindan baska urune tikla.
   - Beklenen: URL, baslik, urun kodu ve 3D model degismeli.

3. 3D gorsel kalite testini yap.
   - RAL 9016, RAL 9010 ve koyu bir RAL rengi sec.
   - Beklenen: Kapak cizgileri/freze/motif ayrimlari beyazda da secilmeli.
   - Eger cizgiler fazla koyu gorunurse `uc-boyut-motoru.js` icinde `opacity: 0.2` degerini 0.12-0.16 araligina cek.

4. Kirik gorsel testi yap.
   - `http://localhost:5013/urunler`
   - DevTools Network ile 404 gorsel var mi kontrol et.
   - Beklenen: `placeholder-urun.jpg` istegi hic olmamali.

5. Admin dinamiklik kontrolu yap.
   - Admin urun yonetiminden bir urune ana gorsel ve 3D model bagla.
   - Public `/urunler` ve `/urun/{slug}` sayfalarinda guncelleme gorunmeli.
   - PDF/katalog/sertifika icin ham PDF linki yerine `/pdf-gosterici?...` kullanildigini dogrula.

6. Dil kontrolu yap.
   - Ana menude dil dropdown acilmali.
   - `tr` ve `en` degisiminde urun liste/detay API `?dil=` parametresiyle yenilenmeli.
   - Hardcoded yeni metin ekleme; Razor metinleri `dil.T(...)` ile olmalidir.

7. CSS borcunu ayri is olarak temizle.
   - `rg "!important|style=|#[0-9A-Fa-f]{3,6}" VIZITLINK3D.UI -n -g "*.razor" -g "*.css"`
   - Tumunu tek seferde degil, modul modul temizle.
   - `tokens.css` degiskenleri kullan.

## Degistirilen Dosyalar

- `VIZITLINK3D.UI/Pages/UrunDetay.razor.cs`
- `VIZITLINK3D.UI/wwwroot/js/uc-boyut-motoru.js`
- `VIZITLINK3D.UI/wwwroot/index.html`
- `VIZITLINK3D.UI/Bilesenler/Urunler/UrunListeKart.razor`
- `VIZITLINK3D.UI/Bilesenler/Urunler/UrunListeKart.razor.cs`
- `VIZITLINK3D.UI/wwwroot/css/sistem/bilesenler/kartlar.css`
- `VIZITLINK3D.UI/Pages/Urunler.razor`

## Devam Ederken Dikkat

- `.razor` icinde `@code` veya `<style>` ekleme.
- Yeni ekran metni ekliyorsan `dil.T("anahtar", "Varsayilan")` kullan.
- CSS renk/font/bosluk icin token kullan.
- DB degisikligi yapilacaksa once `Yedekler/db/` altina yedek al.
- Kullanici degisikliklerini geri alma.
