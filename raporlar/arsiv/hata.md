# HATA TESHISI VE COZUM PLANI — Sistem Calismadi

> Tarih: 2026-05-16
> Yontem: `dotnet build` + `dotnet run` canli calistirildi, loglar ve port/PID
> kanitlari toplandi. Asagidaki bulgular tahmin degil, dogrudan cikti.

---

## 1. OZET (3 cumle)

1. **Build SORUNSUZ** — `dotnet build Desadoor.slnx` → `0 Uyari, 0 Hata`.
2. Sistemin "calismama" sebebi **port cakismasi**: eski `dotnet run` surecleri
   olmedi, 5015 ve 5013'u hala tutuyor; yeni instance "address already in use"
   ile patliyor.
3. Ek **gizli bug**: DbContext'te yeni DbSet'ler acildi ama migration alinmadi;
   yeni tablolar DB'de yok ("No migrations were applied. The database is already
   up to date.") → yeni binary calisinca urun/RAL/malzeme sayfalari runtime
   hatasi verir.

---

## 2. KANITLAR (canli cikti)

### 2.1 Build yesil
```
Olusturma basarili oldu.
    0 Uyari
    0 Hata
```
Sonuc: `eksilermd.md` §1'deki "build kirik / UrunParcaEslemesi.cs CS0234"
tespiti **artik gecersiz** — o hata zaten giderilmis. (Plan guncellenmeli.)

### 2.2 Asil hata — port cakismasi (API startup logu)
```
[18:43:29 INF] No migrations were applied. The database is already up to date.
[18:43:30 ERR] Hosting failed to start
System.IO.IOException: Failed to bind to address http://127.0.0.1:5015:
address already in use.
 ---> AddressInUseException ... SocketException (10048)
Unhandled exception. ... Failed to bind to address http://127.0.0.1:5015
```

### 2.3 Portu tutan zombi surecler (PID kaniti)
```
Port 5015 -> PID 31352  Desadoor.Api   (baslangic 18:39:44)
Port 5013 -> PID 21964  dotnet         (baslangic 18:40:15)

Acik dotnet surecleri: 31352, 4848, 7868, 21964, 26648, 30716, 34520
```
Yorum: Arka planda baslatilan ilk API/UI surecleri "failed" bildirimine ragmen
gercekte **portu birakmadan** ayakta kaldi + uzerine yeni denemeler birikti
(7+ dotnet sureci). Bu yuzden her yeni `dotnet run` 10048 ile dusuyor.

### 2.4 Migration uyumsuzlugu (gizli)
- Bu oturumda `DesadoorDbContext.cs`'te 20 DbSet acildi (Urunler, RalRenkleri,
  Malzemeler, KaplamaSecenekleri, UrunUcBoyutParcalari, ...).
- Startup logu: `No migrations were applied. The database is already up to date.`
- Yani EF yeni tablolari OLUSTURMADI; `desadoor.db` eski semada.
- Sonuc: calisan eski binary (PID 31352) sorun gostermez (eski derleme), ama
  GUNCEL binary calistiginda `Urunler`/`RalRenkleri` vb. sorgulari
  "no such table" ile patlar.

---

## 3. KOK NEDEN ANALIZI

| # | Kok neden | Etki | Kanit |
|---|---|---|---|
| KN-1 | Eski `dotnet run` surecleri sonlandirilmadan birikti | Yeni instance 5015/5013'e baglanamiyor → "sistem calismadi" | §2.2, §2.3 |
| KN-2 | DbContext DbSet'leri acik ama migration yok | Guncel binary'de urun/RAL/malzeme uclari runtime "no such table" | §2.4 |
| KN-3 | `eksilermd.md` "build kirik" bulgusu bayat | Yanlis oncelik (asil sorun build degil) | §2.1 |

KN-1 birincil (sistemin hic ayaga kalkmama sebebi). KN-2 ikincil ama
duzeltilmeden urun sistemi calismaz. KN-3 sadece dokuman guncelligi.

---

## 4. COZUM PLANI (sirayla)

### Adim 1 — Port temizligi (KN-1) [ZORUNLU, ILK]
- [ ] Tum eski Desadoor surecleri kapat:
  ```
  Get-Process dotnet,Desadoor.Api -ErrorAction SilentlyContinue |
    Stop-Process -Force
  ```
  veya yalniz portu tutanlar:
  ```
  Get-NetTCPConnection -LocalPort 5015,5013 -State Listen |
    %{ Stop-Process -Id $_.OwningProcess -Force }
  ```
- [ ] `Get-NetTCPConnection -LocalPort 5015,5013` ile portlarin BOS oldugunu
  dogrula.
- Not: `BASLA.bat` zaten port temizligi yapiyor (satir 11-12); tek baslatma
  kanali olarak o kullanilabilir. Birden fazla paralel `dotnet run`
  baslatilmamali.

### Adim 2 — Migration hizalama (KN-2) [ZORUNLU]
- [ ] DB yedek: `Yedekler/db/desadoor_YYYYMMDD_urun_oncesi.db`.
- [ ] `UrunParcaEslemesi.cs` saf POCO mu dogrula (build yesil oldugu icin
  muhtemelen tamam — yine de gozden gecir).
- [ ] Migration olustur:
  ```
  dotnet ef migrations add UrunOmurgasiEklendi --project Desadoor.Api
  dotnet ef database update --project Desadoor.Api
  ```
- [ ] `__EFMigrationsHistory`'de yeni kaydin ve yeni tablolarin
  (`Urunler`, `RalRenkleri`, `Malzemeler` ...) olustugunu dogrula.
- Alternatif (FAZ A'ya hazir degilse): DbContext DbSet acmalarini GECICI
  geri al ki guncel binary eski semayla uyumlu kalsin. Onerilen: migration'i
  yap, geri alma.

### Adim 3 — Tek kanaldan baslat + dogrula
- [ ] Portlar bos + migration tamamken API'yi baslat (tek instance).
- [ ] UI'yi baslat (tek instance).
- [ ] Canli kontrol:
  ```
  GET http://localhost:5015/api/urunler   → Content-Type application/json
  GET http://localhost:5013/             → 200
  ```
  `text/html` donerse kontrolcu eksik (yenipaneksil.md FAZ B).

### Adim 4 — Dokuman guncelle
- [ ] `eksilermd.md` §1 "build kirik" maddesini "GIDERILDI (2026-05-16,
  build 0 hata)" olarak isaretle.
- [ ] `yenipaneksil.md` FAZ A1/A2: build zaten yesil — A1 sadece dogrulama
  adimi; asil ilk is **port disiplini + migration** (Adim 1-2).

---

## 5. TEKRARI ONLEME

- Ayni anda birden fazla `dotnet run` baslatma. Tek kanal: `BASLA.bat`
  (port temizligi + tek API + tek UI).
- Arka plan sureci "failed" bildirse bile portu tutmus olabilir — yeniden
  baslatmadan once portu kontrol et / oldur.
- DbContext'te entity/DbSet degisikligi yapildiginda AYNI commit'te migration
  uret; migration'siz binary calistirma.
- Startup logunda `No migrations were applied` + model degismisse bu bir
  uyaridir; `database update` yapilmadan ilerleme.

---

## 6. DURUM OZETI

| Konu | Durum |
|---|---|
| `dotnet build Desadoor.slnx` | YESIL (0 hata) |
| `UrunParcaEslemesi.cs` | Build'i kirmiyor (eksilermd bayat) |
| Port 5015 / 5013 | Zombi sureclerce tutulu → temizlenmeli (Adim 1) |
| DbContext yeni DbSet'ler | Acik, migration YOK → Adim 2 |
| Sistemin calismamasi | Kok neden: KN-1 port cakismasi |
| Sonraki net is | Adim 1 → Adim 2 → Adim 3 |
