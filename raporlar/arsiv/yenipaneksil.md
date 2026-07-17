# YENIPANEKSIL — Birlesik Endustriyel Urun + 3D Konfigurator Uygulama Plani

> Olusturulma: 2026-05-16
> Kaynak: `yenplan.md` (mimari vizyon) + `eksilermd.md` (build/eksik denetimi) karsilastirildi,
> uzerine **canli sistem incelemesi** (API/DB/UI uctan uca test) eklendi.
> Amac: Baska bir modelin tek basina, sirayla uygulayabilecegi net is emri.
> Kapsam karari (kullanici): Yeni Urunler/3D sistemi TAMAMLANACAK; eski Kapak sistemi
> zamanla bu omurgaya gocecek. Tek kaynak: her urun ayni yerden gelir.

---

## 0. ONCE OKU (kod yazmadan)

1. `AGENTS.md`
2. `AjanKurallari/00_PROJE_BILGISI.md`
3. `AjanKurallari/05_Veritabani_EFCore10.md`
4. `AjanKurallari/06_API_Servisler_MediatR.md`
5. `AjanKurallari/03_Razor_MudBlazor_Blazor10.md`
6. `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md`

Sabitler: UI 5013, API 5015, .glb/.gltf, MudBlazor, `Cevap<T>`, `DilServisi.T()`,
soft delete (`SilindiMi`), DB sutun adi ASCII, kontrolcude try-catch yok.

---

## 1. GERCEK TESHIS (canli dogrulanmis — iki planin ustune)

`yenplan.md` "baglantilar kopuk" diyor; `eksilermd.md` "build kirik + PDF/konfig eksik"
diyor. Canli test ile **asil kok neden** tespit edildi ve ikisi de bunu net yazmamis:

| Bulgu | Kanit | Durum |
|---|---|---|
| **DbContext'te Urunler DbSet'leri yorum satiriydi** → tablo yok | `VIZITLINK3DDbContext.cs` 76-99 yorum | Bu oturumda ACILDI (asagi bak) |
| `UrunParcaEslemesi.cs` build kiriyor | `eksilermd` §1.1, CS0234/CS0246 | DUZELTILECEK (ilk is) |
| `/api/urunler`, `/api/renkler/ral`, `/api/malzemeler` JSON degil, **SPA fallback HTML** donuyor | Canli: `Content-Type: text/html` | Kontrolcu eksik/dogrulanmali |
| `/api/uc-boyut/modeller` JSON donuyor ama veri bos `[]`, parca uclari yok | Canli + `UcBoyutModelKontrolcu.cs` | Parca endpoint eklenecek |
| Eski calisan sistem: `/api/kapak-modelleri` 42KB gercek JSON | Canli | Korunacak, gocecek |
| Demo varlik mevcut | `I:\KApaklar` 8 GLB, `I:\websitesi` yuzlerce gorsel | Seed icin kullanilacak |
| Urun domaini = **kapak (mobilya/dolap kapagi)** + aile bazli kapi/dolap/dusakabin | GLB adlari: 402, kapak1-4 | Aile sablonu gerekli |

**Bu oturumda yapilan degisiklik (devam noktasi):**
- `VIZITLINK3DDbContext.cs`: Urunler/Renkler/Malzemeler DbSet'leri ACILDI
  (`UrunAilesileri, UrunKategorileri, Urunler, UrunYerellestirmeleri, UrunMedyalari,
  UrunUcBoyutParcalari, UrunParcaGruplari, UrunParcaEslemeleri, RalRenkleri,
  RenkKataloglari, Malzemeler, KaplamaSecenekleri, UrunParcaRenkSecenekleri,
  UrunParcaMalzemeSecenekleri, UrunKonfigurasyonSablonlari, UrunKonfigurasyonKurallari,
  TeklifIstekleri, TeklifIstegiParcalari, UrunPdfKaynaklari, PdfSayfaGorselleri`).
  `using Renkler; using Malzemeler;` eklendi. OnModelCreating'e Urun slug unique +
  soft-delete query filter'lari eklendi.
- **HENUZ migration alinmadi, build dogrulanmadi.** Sonraki model buradan devam eder.

---

## 2. URUN AILESI → PARCA SABLONU (kullanici netlestirmesi)

Her urun ayni `Urun` tablosundan gelir; ailesine gore parca sablonu farklidir.
3D Parca Esleme her zaman **o urunun 3D modeline** baglanir (urune ait).

| Aile | Tipik 3D parcalari (mesh → GorunenAd) | Renklenebilir | Malzeme degisir |
|---|---|---|---|
| **Kapak** (mobilya/dolap kapagi) | Kapak yuzeyi, Cerceve, Panel, Kenar bant, Kulp, Mentese | yuzey/cerceve | yuzey |
| **Kapi** | Kapi kanadi, Kasa, Pervaz, Cerceve, Cam bolme, Kapi kolu, Mentese, Kilit, Esik | kanat/kasa/pervaz | kanat/cam |
| **Dolap / Banyo** | Govde, Kapak sol/sag, Cekmece, Ust tabla, Lavabo, Musluk, Ayna, Ust dolap, Ayak, Kulp, Raf | govde/kapak | tabla/lavabo |
| **Dusakabin** | Cam panel, Sabit cam, Surme cam, Aluminyum profil, Kose profil, Ray, Kulp, Conta, Tekne | cam/profil | cam/profil |

Kural: parca sablonu **kod degil veri**. `UrunParcaGrubu` + `UrunUcBoyutParcasi`
ile DB'de tanimlanir; yeni aile eklemek icin kod degismez.

---

## 3. TEK OMURGA (yenplan.md mimarisi sadelestirilmis)

```
UrunKategori
 └─ Urun  (Slug, Kod, Ad, UrunAilesiId, AktifMi, OneCikan, SiraNo, AnaGorselMedyaId, VarsayilanUcBoyutModeliId, Seo)
     ├─ UrunYerellestirme        (TR/EN metin)
     ├─ UrunMedya                (ana gorsel + galeri — Medya havuzundan)
     ├─ UrunUcBoyutModeli        (GLB/GLTF + analiz JSON + kamera/isik)
     │    └─ UrunUcBoyutParcasi  (mesh→GorunenAd, secilebilir/renklenebilir/hareketli)
     │         ├─ UrunParcaRenkSecenegi     (parca → izinli RAL)
     │         └─ UrunParcaMalzemeSecenegi  (parca → izinli Malzeme/Kaplama)
     ├─ UrunKonfigurasyonSablonu (detay sablonu + varsayilanlar)
     ├─ UrunKonfigurasyonKurali  (yasak/zorunlu kombinasyon)
     └─ TeklifIstegi / TeklifIstegiParcasi
RenkKatalogu → RalRengi    |    Malzeme → KaplamaSecenegi
```

Mock/sabit veri yok. Dosya yolu ana kaynak degil; medya/DB ana kaynak.

---

## 4. FAZ SIRASI (her faz bitince build + canli dogrula)

### FAZ A — Build'i yesile cek (P0, ONCE BU)
1. `VIZITLINK3D.Ortak/Modeller/UrunParcaEslemesi.cs` saf POCO yap:
   - `Microsoft.EntityFrameworkCore` ve `VIZITLINK3D.Ortak.Modeller.Audit` using KALDIR.
   - `EntityBase` kalitimini kaldir; alanlari acikca yaz (Id, FK'lar, audit alanlari).
   - Navigation `UrunUcBoyutParcasi`'ya `[JsonIgnore]`; ayni namespace (`...Urunler`).
2. `dotnet build VIZITLINK3D.slnx` → yesil olana kadar baska ise gecme.
3. DB yedek: `Yedekler/db/VIZITLINK3D_YYYYMMDD_urun_oncesi.db`.
4. Migration: `dotnet ef migrations add UrunOmurgasiEklendi --project VIZITLINK3D.Api`
   sonra `dotnet ef database update --project VIZITLINK3D.Api`.
   (DbSet'ler bu oturumda acildi; snapshot ile uyum kontrol et.)

Kabul: build yesil, `dotnet ef database update` hatasiz, yeni tablolar olusuyor.

### FAZ B — API kontrolculeri (eksilermd P1/P2 + canli eksik)
Mevcut desen: `ControllerBase` + `VIZITLINK3DDbContext` + `Cevap<T>`
(ornek: `UcBoyutModelKontrolcu.cs`). Eksik/dogrulanacak kontrolculer
`VIZITLINK3D.Api/Moduller/Urunler/` ve `.../Malzemeler/`:
- `UrunlerKontrolcu`  → `GET api/urunler`, `GET api/urunler/{id}`,
  `GET api/urunler/slug/{slug}`, `GET api/urunler/{id}/uc-boyut-modelleri`,
  `POST/PUT/DELETE api/urunler` (soft delete).
- `UrunAilesiKontrolcu` (`api/urun-ailesi`), `UrunKategoriKontrolcu` (`api/urun-kategorileri`).
- `RalRenkKontrolcu` (`api/renkler/ral`), `RenkKataloguKontrolcu`.
- `MalzemeKontrolcu` (`api/malzemeler`), `KaplamaKontrolcu` (`api/kaplamalar`,
  `api/malzemeler/{id}/kaplamalar`).
- `UcBoyutModelKontrolcu`'ya PARCA uclari ekle (su an YOK, UI cagiriyor):
  - `GET  api/uc-boyut/modeller/{id}/parcalar`
  - `POST api/uc-boyut/modeller/{id}/parcalar`
  - `PUT  api/uc-boyut/modeller/parcalar/{id}`
  - `DELETE api/uc-boyut/modeller/parcalar/{id}`
  - `POST api/uc-boyut/modeller/{id}/analiz-sonucu` (mesh listesi → parca taslagi)
- `KonfigurasyonKontrolcu` mevcut; `api/konfigurasyon/{id}` + `/parcalar` teklif
  formuyla uyumlu dogrula (eksilermd §3.2).
- `UrunParcaRenkSecenegi` / `UrunParcaMalzemeSecenegi` uclari: parca bazli izinli
  RAL/malzeme (public konfiguratore yansiyacak).

Kural: kontrolcude try-catch yok (HataYonetimiMiddleware), her uc `Cevap<T>`,
salt-okunur sorgu `AsNoTracking`, detay `AsSplitQuery`, route lowercase-cogul.
Dogrulama: her uc `Content-Type: application/json` donmeli (HTML fallback DEGIL).

### FAZ C — Demo seed (hicbir yer bos gelmesin)
`TohumVerisi.cs`'e idempotent (`if (!vt.X.Any())`) seed ekle:
1. GLB kopya: `I:\KApaklar\*.glb` → `VIZITLINK3D.Api/wwwroot/medya/ucboyut/`
   (dosya adlarini ASCII slug yap: `402duz.glb`, `kapak1.glb` ...).
2. Gorsel kopya: `I:\websitesi\*` → `VIZITLINK3D.Api/wwwroot/medya/urunler/`
   (ana gorsel + galeri; ad ASCII).
3. `RenkKatalogu` + `RalRengi` (min ~24 yaygin RAL: 9016, 9010, 7016, 9005 ...).
4. `Malzeme` (Membran, Lake, Laminant, Akrilik, Cam, Aluminyum, MDF) +
   `KaplamaSecenegi` (Mat, Parlak, Yari Mat, Krom, Siyah, Gold).
5. `UrunAilesi` (Kapak, Kapi, Dolap/Banyo, Dusakabin) +
   `UrunKategori`.
6. `Urun` (her aileden en az 1-2; GLB'lere bagli) + `UrunMedya` + `UrunUcBoyutModeli`.
7. `UrunParcaGrubu` + `UrunUcBoyutParcasi` (her urunun GLB mesh'lerine gore;
   §2 sablonuna gore) + `UrunParcaRenkSecenegi`/`UrunParcaMalzemeSecenegi`.
8. `UrunKonfigurasyonSablonu` (aile basina) + ornek `UrunKonfigurasyonKurali`.

Mesh adlari bilinmiyorsa: yukleme sonrasi Three.js `model_analiz_et` ile cikan
adlari `analiz-sonucu` ucuna kaydet; seed bunlari kullanir veya admin esler.

Kabul: admin Urun/RAL/Malzeme/Parca-Esleme ve public liste/detay BOS gelmez.

### FAZ D — Admin formu sekmeli (eksilermd P0.4/P0.5)
Tek urun formu (`UrunDuzenle` / `KapakModelFormu` yerine birlestir), sekmeler:
`Temel Bilgi | Yerellestirme | Gorseller | 3D Model | Renk/Kaplama | Parca Esleme | SEO | Yayin`.
- Gorseller: ana gorsel + galeri → `MedyaSecici` (local yukle + havuzdan sec).
- 3D Model: GLB local yukle → API/medya kaydi → donen URL `ModelYolu`'na yaz →
  `UcBoyutGoruntuleyici` aninda yeniden baslat → hata varsa snackbar+panel.
- Parca Esleme: model analiz → mesh listesi → her mesh'i `GorunenAd`+gruba bagla,
  renklenebilir/malzeme/hareket + izinli RAL/malzeme sec. **Hangi urune aitse o
  modele baglanir** (UrunId zinciri korunur).
- Konfigurasyon Sablon/Kural yonetimi ayni urune bagli, ayri sekme/sayfa.
- Tum metin `DilServisi.T()`, `.razor` icinde `@code`/`<style>` yok, inline style yok.

### FAZ E — Frontend uyum (yenplan Faz 4-5)
- `Urunler.razor` liste + `UrunDetay.razor` slug ile tek kaynak (`UrunDetayDto`/
  `UrunKonfiguratorDto`).
- 3D viewer urune bagli: parca sec → izinli RAL/malzeme → 3D aninda guncellenir →
  urune ait olmayan secenek gosterilmez.
- Pasif urun frontend'de gorunmez; admin degisikligi ayni kaynaktan yansir.
- Footer/menu admin'den yonetilebilir, slug ASCII (`kapi-modelleri`).

### FAZ F — Test + temizlik (eksilermd P3)
- En az 5 test/ozellik: urun CRUD, urun-3D iliski, parca esleme, izinli RAL/malzeme
  filtresi, soft delete public listede yok, teklif konfig ile uretilir.
- `eval` kaldir (`CanliSohbet.razor.cs` → wrapper), `DateTime.Now`→`UtcNow`,
  inline style → `wwwroot/css/sistem/`, hardcoded metin → `DilServisi.T()`.
- Smoke: API+UI ayaga kalkar, urun liste/detay/admin giris, GLB yukle→onizle→esle.

---

## 5. NIHAI KABUL (yenplan §15 + kullanici)

Bir SuperAdmin tek panelden sunu hardcoded/mock olmadan yapabilmeli:
```
Yeni urun → aile sec (Kapak/Kapi/Dolap/Dusakabin) → TR/EN metin →
ana gorsel + galeri (havuz) → GLB yukle → model analiz → parcalari esle
(o urune bagli) → parcalara izinli RAL/malzeme/kaplama → konfig sablon+kural →
yayina al → frontend detayda ayni urun → 3D konfigurator izinli secimlerle calisir →
PDF teklif konfig verisiyle uretilir
```
Ek kabul (eksilermd): `dotnet build/test VIZITLINK3D.slnx` yesil; tum yeni API uclari
JSON (HTML fallback degil) doner; hicbir admin/public ekran bos gelmez.

---

## 6. SONRAKI MODEL ICIN NET ILK 3 IS

1. **FAZ A**: `UrunParcaEslemesi.cs` POCO + build yesil + DB yedek + migration
   `UrunOmurgasiEklendi` + `database update`. (DbSet'ler ACILDI, snapshot uyumla.)
2. **FAZ B**: Eksik kontrolculer (`UrunlerKontrolcu`, `RalRenkKontrolcu`,
   `MalzemeKontrolcu`, `KaplamaKontrolcu`) + `UcBoyutModelKontrolcu` parca uclari;
   her uc canli `application/json` dogrula.
3. **FAZ C**: `TohumVerisi`'ne demo seed (I:\KApaklar GLB + I:\websitesi gorsel +
   RAL + malzeme + aile/urun/parca esleme); admin ve public ekranlar dolu gelsin.

Build yesile donmeden FAZ D-E-F'ye girme. Her faz sonunda
`yenplan.md` §14 gunluk takip sablonunu doldur.

---

## 7. ZIHNIYET: STOK KART / URETIM MANTIGI (kullanici netlestirmesi)

Bu sistem bir e-ticaret degil, bir **uretim/stok kart** sistemidir. Mantik:

```
1) HAM GLB YUKLE  (sisteme urun olarak ham 3D model girer)
        ↓
2) PARCALARA AYIR  (model mesh'leri parcalara bolunur)
        ↓
3) PARCA TANIMLA   (her parca: ad, kategori-aile, renklenebilir mi,
                     malzeme/kaplama, hareket: surgu/acilan/sabit)
        ↓
4) KAPLAMA + RAL + MALZEME AYARLA  (bu urune/parcaya izinli olanlar)
        ↓
5) URUN KAYIT BOLUMU  (olcu, olceklik, kapak acis yonu, varyant)
        ↓
6) ON IZLEME (admin)  → ne kaydettigini gor
        ↓
7) YAYINLA → musteri urunu 3D + seceneklerle gorur
```

Aileler ayni zamanda **kategori**: Kapak, Kapi, Dolap, Dusakabin, Vestiyer
(ve "herhangi bir sey" — sistem aile eklemeye acik, kod degismeden).

**Parca isimlendirme kurali (onemli):**
- GLB icinde mesh adlari **anlamli/duzgun** ise (`lavabo`, `ayna`, `musluk`)
  → o isimler oldugu gibi kullanilir.
- Degilse → sistem `obje1, obje2...` gibi gecici ad verir; admin urun yuklerken
  bu adlari **degistirir** (`obje3 → "Lavabo Dolabi"`).
- Parca sayisi aileye gore degisir, ornek:
  - Dusakabin: ayna, ayna dolabi, ayna cercevesi, lavabo, lavabo dolabi,
    musluk, kapaklar (~7 parca)
  - Banyo dolabi: ~6 parca
  - Kapi: ~3 parca (kanat, kasa, kol/mentese)
  - Kapak: ~1 parca (kapak yuzeyi)
- Malzeme havuzu parca tipine gore: krom, metal, plastik, porselen, cam, ahsap,
  lake, membran, aluminyum. Urun olustugunda parca → izinli malzeme bilinir.

**FAZ C'ye ek (ilk somut hedef):** Once **2 ornek urun** uctan uca calissin
(1 Kapak + 1 Dusakabin veya Banyo dolabi). Dosyadan dogrudan yuklenir, listede
gorunur, parcalari ayrilmis, renk/malzeme secilebilir, admin onizlemede ne
kaydettigini net gorur. Bu 2 urun "referans sablon" olur; gerisi cogaltilir.

---

## 8. DINAMIK SAHNE / AYARLAR (endustriyel admin — aile bazli)

Admin'de cok kapsamli, **canli** bir "3D Sahne Ayarlari" bolumu olacak.
Ayar degisince sahneye **aninda** uygulanir (kayit-yenile yok).

Ayarlanacaklar (her **aile** icin ayri profil — banyo, dusakabin, dolap, kapi
farkli varsayilan):
- Isik siddeti / aci / sayisi
- Golge yumusakligi ve yogunlugu
- Zemin rengi + zemin tipi (mat/parlak/yansimali)
- Arka plan rengi / gradient / HDR ortam haritasi
- Ayna/cam parlamasi icin **HDR yukleme** (ayna dolabi, dusakabin cami)
- Kamera: baslangic acisi, zoom min/max, otomatik donme
- Ton/expozisyon, parlama (bloom) **filtreleme** — isik patlamasi urun
  uzerinde patlamamali, kontrollu olmali

**Akilli arka plan kurali (kullanici ozel istegi):**
Arka/zemin rengi **sabit olmamali**, urunun rengine gore otomatik ayarlanmali:
- Urun beyaz/acik → arka plan koyulasir (urun kaybolmasin)
- Urun siyah/koyu → arka plan acilir
- Bu otomatik kontrast davranisi ayarlardan acik/kapali + esik ile yonetilir.

Hedef his: model **canli, gercekci ve guzel** gorunsun; yapay/yanmis isik,
duz/cansiz zemin olmasin. Tum sahne parametreleri DB'de (`SistemAyari` veya
`UrunKonfigurasyonSablonu` JSON alanlari), 3D motor (`UcBoyutServisi`/Three.js
wrapper) bunlari okur.

---

## 9. MUSTERI DENEYIMI SENARYOSU (uctan uca)

```
[Ana sayfa]
  → Once URUN GORSELLERI sergilenir (galeri/grid, hizli ve sik)
  → Urun karti: ad, kisa aciklama, "3D var" rozeti

[Urun detay — tek sayfa, REFRESH YOK]
  → Ust: urun gorselleri (ana + galeri)
  → Urun bilgileri: ozellikler, kullanildigi yerler, aciklama, teknik tablo
  → "3D Izle" → AYNI SAYFADA, sayfa yenilenmeden 3D model acilir
       - Sag/yan panel: RENK (izinli RAL), KAPLAMA, MALZEME secimi
       - Parca secilince o parcaya izinli secenekler gelir
       - Yakinlas / uzaklas (zoom), 360 donme, kamera sifirla
       - Ayna/cam parlamasi (HDR), aile bazli sahne ayari uygulanir
       - Secim degisince 3D aninda guncellenir (urune ait olmayan secenek yok)
  → Altinda: "En cok izlenen", "Yeni urunler", "En son baktiklariniz"
  → "Teklif Iste": secili konfigurasyon (parca+renk+malzeme+olcu) ile
       TeklifIstegi olusur; PDF teklif bu konfigurasyon verisiyle uretilir
```

Bu senaryo FAZ E'nin (frontend uyum) somut kabul akisidir. "En son
baktiklariniz" icin hafif client-side gecmis (localStorage veya
oturum) + populer/yeni icin API siralama.

---

## 10. SENARYO ZENGINLESTIRME (eklenen kurgu — uygulama detayina ipucu)

- **Admin akisi (oyunlastirilmis dogrulama):** GLB yuklenince sistem "X parca
  buldum, isimlerini onayla/duzelt" der → admin parcalari isimler → her parca
  icin renk/malzeme/hareket secer → "Onizle" sekmesinde dondurup ne kaydettigini
  gorur → "Yayinla" der. Yanlis/eksik parca varsa uyarir (mesh esleserse yesil,
  eslesmezse sari).
- **Uretim karti hissi:** Her urun bir "kart": sol ust ham GLB, sagda parca
  agaci, altta varyant matrisi (olcu x renk x acis yonu). Bu bir katalog
  sayfasi degil, bir **uretim tanim karti** gibi gozukur.
- **2 referans urun:** (1) "Duz Kapak 402" — tek parca, RAL + mat/parlak;
  (2) "Aria Banyo Dolabi" — govde+kapak+lavabo+musluk+ayna+ust dolap, parca
  bazli farkli malzeme (porselen lavabo, krom musluk, cam ayna). Bu ikisi
  tum sistemin canli kanit-i (proof) olur.
- **His hedefi:** Musteri 3D'yi acinca "vay" demeli; admin paneli acinca
  "her seyi buradan yonetiyorum" hissetmeli (MIMARI_VIZYON.md felsefesiyle
  uyumlu, ama once calisan omurga, sonra sov).

> Not: Bu bolum kurgu/senaryodur; FAZ D-E ve §8 ayarlar bunu teknik kabul
> kriterine cevirir. Kod yazan model: once §6 ilk 3 is, sonra §7 iki referans
> urun, sonra §8-9 deneyim.

---

## 11. VERI SOZLUGU VE ALAN TUZAKLARI (kod yazmadan once oku)

Canli incelemede tespit edilen, kod yazani yanıltacak gercek tuzaklar:

- **`UrunUcBoyutModeli` iki ayri yol alani tasiyor:** `ModelDosyaYolu` VE
  `ModelYolu` (+ `MedyaId`, `MedyaId` long). UI (`UrunDetay.razor.cs`,
  `KapakModelFormu`) **`ModelYolu`** okuyor; `UcBoyutModelKontrolcu` yukleme
  ucu **`ModelYolu`**'na yaziyor ama `KapakModelFormu` bazen `ModelDosyaYolu`
  bekliyor. KARAR: tek alan otorite olsun (`ModelYolu`), digeri ya kaldirilsin
  ya her zaman ayni deger yazilsin. Migration'da veri kaybi olmasin.
- **DTO'lar mevcut ama bos/yarim:** `UrunOzetDto, UrunDetayDto,
  UrunKonfiguratorDto, UrunAdminDto` dosyalari var. Public uclar bunlari
  donmeli (entity degil) — `Urun` entity'sinde `[JsonIgnore] UrunAilesi`
  oldugu icin entity dogrudan donulurse aile bilgisi gitmez. Mapster ile DTO.
- **`UrunUcBoyutParcasi` audit alani yok** (OlusturulmaTarihi/SilindiMi yok) —
  soft-delete query filter EKLENMEMELI (yoksa EF patlar). Parca silme = fiziksel
  veya parent uzerinden; karar netlestirilip dokumante edilsin.
- **Klasor adi tutarsiz:** `Moduller/Urunler/Kontrolcüler` (Turkce 'ü'),
  baska yerde `Kontrolcu`, `Kontrolculer`. Yeni kontrolculer mevcut klasore
  konsun, YENI klasor adi turetilmesin (route cakismasi/karisiklik riski).
- **Cift `MedyaKontrolcu`:** `Moduller/Medya/Kontrolcu/` ve
  `Moduller/Medya/Kontrolculer/` altinda iki tane var — ayni route'a iki
  controller cakisma riski. Birlestir veya route ayir, sonra dogrula.

---

## 12. ESKI KAPAK → YENI URUN GOCU (kayipsiz gecis)

Canli site su an `KapakModeli` + `/api/kapak-modelleri` (42KB gercek veri)
uzerinde calisiyor. Yeni omurgaya gecis kademeli olmali, site kesintiye
girmemeli:

1. Yeni `Urun` omurgasi calisir hale gelene kadar eski sistem AYAKTA kalir.
2. Tek seferlik **gocum servisi** (`KapakGocServisi`): her `KapakModeli` →
   `Urun` (aile=Kapak) + `UrunMedya` (mevcut resimler) + varsa `UrunUcBoyutModeli`.
   Idempotent (tekrar calisinca cogaltmaz; `Kod`/`Slug` ile eslesme).
3. Frontend rotalari once yeni `Urun` kaynagina bakar, yoksa eski sisteme
   fallback (gecis suresince).
4. Goc dogrulanip site yeni kaynaktan sorunsuz calisinca eski uclar
   `[Obsolete]` isaretlenir, sonra kaldirilir. Veri silinmez (soft).

Kabul: goc sonrasi public liste/detay ESKI ile birebir ayni gorunur, sonra
yeni ozellikler (3D konfig) uzerine eklenir.

---

## 13. ADMIN KABUK EKSIKLERI (eksilermd P0.1–P0.7 ozeti — atlanmasin)

yenipaneksil ana akisi urun/3D'ye odakli; ama eksilermd'deki su admin kabuk
eksikleri de kapsama dahil (FAZ D ile paralel, ayri is paketi):

- **Ust banner:** aktif sayfa basligi + breadcrumb + rol/kullanici (JWT'den) +
  gercek bildirim; statik "Yönetici/admin" kaldirilsin.
- **Menu yetki/hiyerarsi:** `GerekliRol, SuperAdminGerekliMi, YetkiAnahtari,
  KilitliMi, SistemMenusuMu` alanlari; tree view; soft delete; konum ayrimi
  (`PublicHeader, PublicFooterHizli, PublicFooterKategori, PublicMobil,
  AdminSol, AdminUst`).
- **Dinamik footer/menu:** footer ve kategori linkleri de API'den; sabit
  kategori (Membran/Lake...) kaldirilsin; slug ASCII (`kapi-modelleri`).
- **SuperAdmin:** tum menu/modul/urun/parca/RAL/malzeme/kullanici yonetimi,
  modul aktif/pasif, rol atama, cop kutusu/geri al; Admin sadece yetkili
  oldugunu gorur.
- **Yetki:** admin uclar `[Authorize(Roles="Admin")]`; musteri teklif POST
  `[AllowAnonymous]`; public read aciktir. JWT'den kullanici alma TODO'su
  kapatilsin (MedyaKontrolcu vb.).

---

## 14. DIGER KAPSAM (PDF, dil, real-time, medya)

- **PDF katalog (eksilermd §3.1):** `PdfIcerikCozumleyici`/`PdfGorselCikarici`
  hala placeholder (`SayfaSayisi = 0`). C# PDF wrapper ile gercek sayfa/gorsel
  cikarimi; admin onay ekraninda sayfa gorselleri; gorseli urune bagla. (P1)
- **Dil/ceviri seed:** her yeni modul (urun, 3D, RAL, malzeme, kaplama, teklif,
  ayarlar) icin `Ceviri` seed anahtarlari; Razor'da hardcoded metin yok,
  `DilServisi.T("anahtar","Varsayilan")`.
- **Real-time (vizyon):** §8 sahne ayari degisince acik admin oturumlarina ve
  gerekirse frontend'e SignalR ile yansisin (kayit-yenile yok). MessagePack.
- **Medya havuzu:** GLB/HDR/gorsel `MedyaSecici` ile secilsin; `MedyaKullanim`
  referans kaydi tutulsun (silmeden once "su urunlerde kullaniliyor" uyarisi).
- **ImageSharp guvenlik:** build duzelince `SixLabors.ImageSharp` guvenli
  surume guncellensin.

---

## 15. DOGRULAMA YONTEMI (SPA fallback tuzagi) + TEST MATRISI

**Kritik tuzak:** API, Blazor'u host ettigi icin **olmayan `/api/...` rotasi
404 degil, 200 + `index.html` (HTML) doner.** Status 200'e bakip "endpoint
var" sanma. Her uc icin **Content-Type** kontrol et:

```
curl -i http://localhost:5015/api/urunler   → Content-Type application/json OLMALI
(text/html donerse controller YOK / route yanlis)
```

**Minimum test matrisi (≥5/ozellik, gercek PostgreSQL/SQLite — in-memory yok):**
- Urun CRUD + slug unique + zorunlu alan validasyon
- Urun → 3D model → parca iliski; parca esleme kaydi
- Parca bazli izinli RAL/malzeme filtresi (urune ait olmayan secilemez)
- Soft delete sonrasi public listede yok
- Teklif konfigurasyon verisiyle olusur + PDF uretilir
- Goc: KapakModeli → Urun birebir (sayim + ornek karsilastirma)
- Smoke: API+UI ayaga kalkar, liste/detay/admin giris, GLB yukle→onizle→esle
- Her yeni uc icin "JSON donuyor (HTML degil)" entegrasyon testi

---

## 16. NIHAI KONTROL LISTESI (kod yazan model her faz sonu isaretler)

```
[ ] dotnet build VIZITLINK3D.slnx yesil
[ ] dotnet test VIZITLINK3D.slnx yesil
[ ] DB yedek alindi (her migration oncesi)
[ ] Yeni uclar JSON donuyor (Content-Type dogrulandi, HTML degil)
[ ] Hicbir admin/public ekran bos gelmiyor (seed dolu)
[ ] ModelYolu/ModelDosyaYolu tek otorite, veri kaybi yok
[ ] Eski Kapak sistemi gocten once ayakta, goc kayipsiz
[ ] Razor'da @code/<style>/inline-style yok, metin DilServisi.T()
[ ] Kontrolcude try-catch yok, Cevap<T> donuyor
[ ] DB sutun adlari ASCII, soft delete, audit alanlari
[ ] 2 referans urun (Kapak + Banyo/Dusakabin) uctan uca calisiyor
[ ] §8 dinamik sahne ayari sahneye aninda uygulaniyor
[ ] eval kaldirildi, DateTime.Now → UtcNow
```

---

## 17. HER FAZDA GECERLI CAPRAZ KURALLAR (istisnasiz)

Asagidaki TODO'nun **her** maddesinde su kurallar zorunlu — ayri madde degil,
sabit cerceve:

- **Dinamik ve canli:** hicbir sey hardcoded degil; veri DB'den, ayar
  degisince SignalR ile sahneye/admin'e aninda yansir (kayit-yenile yok).
- **Dil:** kullaniciya gorunen tum metin `DilServisi.T("anahtar","Varsayilan")`;
  TR varsayilan + EN ceviri seed. Ham Ingilizce teknik terim gosterilmez
  (gerekirse tooltip).
- **CSS:** renk/font/bosluk yalniz `wwwroot/css/sistem/tokens.css` degiskeni
  (`var(--...)`); `.razor` icinde `<style>`/inline-style yok, `!important`/ID
  selektor yok.
- **MudBlazor:** UI yalniz MudBlazor bileseni + proje template/duzen deseni;
  baska UI kutuphanesi yok; `.razor` icinde `@code` yok (partial `.razor.cs`).
- **Sistem uyumu:** mevcut klasor/route/isim desenine uy (§11), yeni desen
  turetme; `Cevap<T>`, soft delete, ASCII DB, kontrolcude try-catch yok.

---

## 18. ANA TODO LISTESI — SIRAYLA, ATLAMA YOK

> Kod yazan model: bu listeyi kendi gorev takibine (TaskCreate/todo) **birebir
> aktar**, sirayla yap, her madde bitince isaretle. Bir madde bitmeden sonrakine
> GECME. Atlama/birlestirme yok. §17 caprazi her maddede gecerli. Her FAZ
> sonunda build+canli dogrula, sonucu `yenplan.md` §14 sablonuna yaz.

**FAZ A — Build + DB temeli**
- [ ] A1. `VIZITLINK3D.Ortak/Modeller/UrunParcaEslemesi.cs` saf POCO (EF/Audit using kaldir, EntityBase kaldir, [JsonIgnore], dogru namespace)
- [ ] A2. `dotnet build VIZITLINK3D.slnx` → YESIL (yesil olmadan A3'e gecme)
- [ ] A3. DB yedek: `Yedekler/db/VIZITLINK3D_YYYYMMDD_urun_oncesi.db`
- [ ] A4. `dotnet ef migrations add UrunOmurgasiEklendi --project VIZITLINK3D.Api` (DbSet'ler acik — snapshot uyumla)
- [ ] A5. `dotnet ef database update --project VIZITLINK3D.Api` → hatasiz, tablolar olusuyor
- [ ] A6. §11 alan tuzaklari karari yaz (ModelYolu otorite, parca soft-delete yok, klasor adi, cift MedyaKontrolcu)

**FAZ B — API kontrolculeri**
- [ ] B1. `UrunlerKontrolcu` (liste, id, slug, {id}/uc-boyut-modelleri, POST/PUT/DELETE soft)
- [ ] B2. `UrunAilesiKontrolcu` (api/urun-ailesi) + `UrunKategoriKontrolcu` (api/urun-kategorileri)
- [ ] B3. `RalRenkKontrolcu` (api/renkler/ral) + `RenkKataloguKontrolcu`
- [ ] B4. `MalzemeKontrolcu` (api/malzemeler) + `KaplamaKontrolcu` (api/kaplamalar, api/malzemeler/{id}/kaplamalar)
- [ ] B5. `UcBoyutModelKontrolcu`'ya parca uclari (GET/POST {id}/parcalar, PUT/DELETE parcalar/{id}, analiz-sonucu)
- [ ] B6. Parca→izinli RAL/malzeme uclari (`UrunParcaRenkSecenegi`/`UrunParcaMalzemeSecenegi`)
- [ ] B7. `KonfigurasyonKontrolcu` teklif formuyla uyum dogrula
- [ ] B8. Her uc canli `Content-Type: application/json` (HTML degil) — §15 yontemiyle dogrula
- [ ] B9. DTO katmani (Mapster) — public uclar entity degil DTO doner

**FAZ C — Demo seed (bos ekran yok)**
- [ ] C1. GLB kopya `I:\KApaklar\*.glb` → `wwwroot/medya/ucboyut/` (ASCII ad)
- [ ] C2. Gorsel kopya `I:\websitesi\*` → `wwwroot/medya/urunler/` (ASCII ad)
- [ ] C3. `RenkKatalogu`+`RalRengi` seed (≥24 RAL)
- [ ] C4. `Malzeme`+`KaplamaSecenegi` seed
- [ ] C5. `UrunAilesi` (Kapak/Kapi/Dolap/Dusakabin/Vestiyer) + `UrunKategori`
- [ ] C6. **2 referans urun** uctan uca: (1) Duz Kapak 402 (2) Banyo/Dusakabin — `Urun`+`UrunMedya`+`UrunUcBoyutModeli`
- [ ] C7. `UrunParcaGrubu`+`UrunUcBoyutParcasi` (mesh→GorunenAd, §2 sablonu) + parca izinli RAL/malzeme
- [ ] C8. `UrunKonfigurasyonSablonu` (aile basina) + ornek `UrunKonfigurasyonKurali`
- [ ] C9. Canli: admin Urun/RAL/Malzeme/Parca-Esleme + public liste/detay BOS DEGIL

**FAZ D — Sekmeli admin formu**
- [ ] D1. Tek urun formu (Temel/Yerellestirme/Gorseller/3D Model/Renk-Kaplama/Parca Esleme/SEO/Yayin)
- [ ] D2. Gorseller + 3D `MedyaSecici` (local yukle + havuzdan sec), GLB yukle→URL→viewer yeniden baslat→hata paneli
- [ ] D3. Parca Esleme: analiz→mesh listesi→ad/grup/renk/malzeme/hareket + izinli RAL/malzeme; UrunId zinciri korunur
- [ ] D4. Konfig Sablon/Kural yonetimi (ayni urune bagli)
- [ ] D5. Admin kabuk eksikleri §13 (ust banner, menu yetki, dinamik footer, SuperAdmin) — paralel paket

**FAZ E — Frontend**
- [ ] E1. `Urunler.razor` liste + `UrunDetay.razor` slug tek kaynak (DTO)
- [ ] E2. Ayni sayfada 3D (refresh yok), yan panel izinli renk/kaplama/malzeme, zoom/360/HDR
- [ ] E3. §8 aile bazli sahne ayari + akilli arka plan uygulanir
- [ ] E4. En cok izlenen/yeni/son baktiklarin + teklif iste→PDF
- [ ] E5. Pasif urun gizli, eski Kapak gocu (§12) kayipsiz dogrula

**FAZ F — Test + temizlik**
- [ ] F1. §15 test matrisi (≥5/ozellik)
- [ ] F2. eval kaldir, DateTime.Now→UtcNow, inline style→css/sistem, hardcoded→DilServisi.T()
- [ ] F3. PDF katalog gercek (§14), ImageSharp guncelle
- [ ] F4. `dotnet build/test VIZITLINK3D.slnx` yesil + smoke
- [ ] F5. §16 nihai kontrol listesi tam isaretli


> Kural: Bir FAZ tamamlanmadan sonrakine gecme. Build kirikken ileri faza
> girme. Her madde tek tek, atlamasiz. §17 caprazi istisnasiz.
