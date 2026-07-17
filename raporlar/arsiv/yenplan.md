# YENPLAN - VIZITLINK3D Endustriyel Sistem Toparlama Plani

> Olusturulma: 2026-05-16  
> Amac: VIZITLINK3D sistemini parca parca calisan ekrandan, admin-DB-frontend-3D-medya baglantilari tek omurgaya oturan endustriyel urun yonetimi sistemine cevirmek.  
---

## 1. Ana Teshis

Sistem teknik olarak derleniyor ve lokal smoke testte aciliyor. Asil sorun calisma zamani baslatmasindan cok mimari baglantilarin kopuk olmasi.

Mevcut durum:

- Admin ekranlari, frontend sayfalari, medya havuzu ve 3D viewer var.
- Ancak bu parcala birbirine tam bagli degil.
- Admin tarafinda girilen veri her zaman frontend tarafinda ayni sekilde gorunmuyor.
- 3D model, urun, renk, malzeme, olcu ve medya arasinda net endustriyel iliski yok.
- Bazi bilgiler DB'den, bazi bilgiler seed/eski sabit veri/dosya yolu mantigindan geliyor.

Hedef:

Admin panelde tanimlanan bir urun, kategori, medya, 3D model, renk, malzeme ve teknik bilgi tek veri omurgasindan frontend'e aksin.

---

## 2. Temel Mimari Karar

Urun merkezli mimariye gecilecek.

Ana omurga:

```text
Kategori
  -> Urun
      -> UrunYerellestirme
      -> UrunMedya
      -> UrunUcBoyutModeli
      -> UrunRenkSecenegi
      -> UrunMalzemeSecenegi
      -> UrunOlcuSecenegi
      -> UrunTeknikOzellik
      -> UrunHotspot
      -> UrunTeklifKurali
```


---

## 3. Faz 0 - Dondurma ve Envanter

Durum: Bekliyor  
Hedef: Mevcut sistemi bozmadan baglanti haritasini cikarmak.

Yapilacaklar:

- [ ] Aktif admin sayfalarini listele.
- [ ] Aktif ziyaretci sayfalarini listele.
- [ ] Urun/kapak/kapi/kategori DTO ve entity listesini cikar.
- [ ] Medya entity, medya servisleri ve mevcut dosya yolu kullanimlarini listele.
- [ ] 3D model kullanan tum sayfa/bilesen/servisleri listele.
- [ ] Hardcoded urun, gorsel, 3D model, kategori ve fiyat alanlarini isaretle.
- [ ] Hangi endpoint hangi sayfa tarafindan kullaniliyor haritasini cikar.
- [ ] Mevcut DB yedegi al.

Cikis:

- `raporlar/urun-baglanti-envanteri.md`
- `raporlar/admin-frontend-api-haritasi.md`
- DB yedegi

Basari kriteri:

- Hangi veri nereden geliyor net gorulecek.
- Silinecek, korunacak ve donusturulecek alanlar ayrilacak.

---

## 4. Faz 1 - Urun Veri Modeli Omurgasi

Durum: Bekliyor  
Hedef: Endustriyel urun tanimini DB seviyesinde netlestirmek.

Yapilacaklar:

- [ ] Mevcut `KapakModeli`, `KapiKategorisi`, `KapiModeliResim` ve ilgili DTO'lari analiz et.
- [ ] "Kapak modeli" ile "urun" ayrimi netlestir.
- [ ] Tek ana urun modeli sec: `Urun` veya mevcut `KapakModeli` genisletme.
- [ ] Urun ana alanlarini belirle:
  - Ad
  - Slug
  - KategoriId
  - KisaAciklama
  - Aciklama
  - AktifMi
  - OneCikanMi
  - SiraNo
  - AnaMedyaId
  - UcBoyutModelMedyaId
  - TeknikOzelliklerJson
  - Seo alanlari
- [ ] Urun yerellestirme yapisini standardize et.
- [ ] Urun-medya iliski tablosunu tasarla.
- [ ] Urun-3D iliski tablosunu tasarla.
- [ ] Renk, malzeme, olcu ve hotspot tablolarini tasarla.
- [ ] Soft delete, audit, ASCII tablo/sutun kurallarini uygula.
- [ ] Migration planini parcalara bol.

Onerilen tablolar:

- `Urunler`
- `UrunYerellestirmeleri`
- `UrunMedyalari`
- `UrunUcBoyutModelleri`
- `UrunRenkSecenekleri`
- `UrunMalzemeSecenekleri`
- `UrunOlcuSecenekleri`
- `UrunHotspotlari`
- `UrunTeklifKurallari`

Basari kriteri:

- Bir urun tum medya, 3D, renk, malzeme ve olcu bilgilerine DB iliskisiyle ulasacak.
- Dosya yolu hardcoded ana kaynak olmayacak.

---

## 5. Faz 2 - API ve Servis Katmani

Durum: Bekliyor  
Hedef: Admin ve frontend ayni API omurgasini kullansin.

Yapilacaklar:

- [ ] Urun modulu Vertical Slice yapisina tasinacak.
- [ ] Public sorgular:
  - `GET /api/urunler`
  - `GET /api/urunler/slug/{slug}`
  - `GET /api/urunler/kategori/{slug}`
  - `GET /api/urunler/{id}/konfigurator`
- [ ] Admin komutlari:
  - Urun olustur
  - Urun guncelle
  - Urun soft delete
  - Medya bagla/cikar
  - 3D model bagla
  - Renk/malzeme/olcu secenegi yonet
  - Hotspot yonet
- [ ] Her endpoint `Cevap<T>` donecek.
- [ ] Kontrolculerde business logic temizlenecek.
- [ ] FluentValidation dogrulayicilari yazilacak.
- [ ] Salt-okunur sorgularda `AsNoTracking` kullanilacak.
- [ ] Detay sorgularinda `AsSplitQuery` kullanilacak.
- [ ] DTO'lar admin ve public olarak ayrilacak.

DTO ayrimi:

- `UrunOzetDto`
- `UrunDetayDto`
- `UrunAdminDto`
- `UrunFormDto`
- `UrunKonfiguratorDto`
- `UrunMedyaDto`
- `UrunUcBoyutDto`

Basari kriteri:

- Admin ile frontend ayni urun kaydini farkli DTO'larla kullanacak.
- Mock/sabit urun verisi kalmayacak.

---

## 6. Faz 3 - Admin Panel Uyumlastirma

Durum: Bekliyor  
Hedef: Admin panel gercek endustriyel urun yonetim merkezi olacak.

Yapilacaklar:

- [ ] Admin urun liste ekranini gercek API'ye bagla.
- [ ] Urun formunu bolumlere ayir:
  - Temel bilgiler
  - Yerellestirme
  - Galeri ve medya
  - 3D model
  - Renk secenekleri
  - Malzeme secenekleri
  - Olcu secenekleri
  - Hotspotlar
  - SEO
  - Yayin durumu
- [ ] MedyaSecici her gorsel ve 3D alaninda kullanilacak.
- [ ] `.glb`, `.gltf`, `.hdr` dosyalari medya havuzundan secilecek.
- [ ] 3D model icin parca/material adlari admin tarafinda tanimlanacak.
- [ ] Renk secenekleri RAL katalogu ile baglanacak.
- [ ] Admin kaydetme sonrasi frontend onizleme linki verilecek.
- [ ] Form metinleri `DilServisi.T()` ile duzenlenecek.
- [ ] `.razor` icinde `@code` ve `<style>` olmayacak.

Basari kriteri:

- Admin bir urunu bastan sona tanimlayabilecek.
- Admin kaydettigi urunu frontend detay sayfasinda ayni haliyle gorecek.

---

## 7. Faz 4 - Frontend Uyumlastirma

Durum: Bekliyor  
Hedef: Ziyaretci tarafi sadece API'den gelen yayinlanmis urunleri gostersin.

Yapilacaklar:

- [ ] Ana sayfa one cikan urunleri API'den cekecek.
- [ ] Kapi/kapak liste sayfalari kategori ve filtre bilgilerini API'den cekecek.
- [ ] Urun detay sayfasi `slug` ile tek kaynaktan beslenecek.
- [ ] Galeri, ana gorsel, teknik tablo, renkler, malzemeler ve 3D viewer ayni `UrunDetayDto` veya `UrunKonfiguratorDto` ile dolacak.
- [ ] Hardcoded gorsel, model, kategori, fiyat ve aciklama temizlenecek.
- [ ] Dil ve SEO alanlari yerellestirme tablosundan gelecek.
- [ ] Bos veri durumlari profesyonel sekilde ele alinacak.

Basari kriteri:

- Admin urunu pasif yaparsa frontend'de gorunmeyecek.
- Admin gorsel/model degistirirse frontend aninda ayni kaynaktan yeni veriyi gosterecek.

---

## 8. Faz 5 - 3D Konfigurator Baglantisi

Durum: Bekliyor  
Hedef: 3D viewer urune bagli konfigurator haline gelecek.

Yapilacaklar:

- [ ] Her urun icin 3D model medya kaydi secilecek.
- [ ] 3D model parca/material adlari DB'de tutulacak.
- [ ] Renk uygulanabilir parcalar tanimlanacak.
- [ ] RAL renk secenekleri urunle baglanacak.
- [ ] Malzeme/yuzey secenekleri urunle baglanacak.
- [ ] Olcu secenekleri min/max/kademeli olarak tanimlanacak.
- [ ] Hotspot koordinatlari ve aciklamalari urune baglanacak.
- [ ] Konfigurasyon state'i JSON olarak uretilecek.
- [ ] PDF teklif endpoint'i bu konfigurasyonla beslenecek.
- [ ] Paylasilabilir konfigurasyon linki icin tasarim yapilacak.

Onerilen konfigurator akisi:

```text
UrunDetay
  -> UrunKonfiguratorDto alir
  -> 3D model yuklenir
  -> Renk/malzeme/olcu secilir
  -> Konfigurasyon JSON olusur
  -> Teklif/PDF/Paylasim bu JSON ile uretilir
```

Basari kriteri:

- 3D model dosyasi, renk secici ve urun bilgisi birbirinden kopuk olmayacak.
- Urune ait olmayan renk/malzeme/olcu secilemeyecek.

---

## 9. Faz 6 - Medya Havuzu Entegrasyonu

Durum: Bekliyor  
Hedef: Gorsel, PDF, video ve 3D dosyalari tek medya havuzundan yonetilsin.

Yapilacaklar:

- [ ] Urun formlarindaki dosya inputlari MedyaSecici ile degistirilecek.
- [ ] MedyaKullanim kaydi otomatik tutulacak.
- [ ] Silinmek istenen medya kullaniliyorsa uyarilacak.
- [ ] `.glb`, `.gltf`, `.hdr`, `.webp`, `.jpg`, `.png`, `.pdf` tipleri net ayrilacak.
- [ ] Ana gorsel, galeri, teknik PDF, 3D model ayni medya havuzu mantigiyla secilecek.
- [ ] Medya detay panelinde kullanim listesi gorunecek.

Basari kriteri:

- Bir dosyanin hangi urunlerde kullanildigi gorulecek.
- Rastgele dosya yolu yazma aliskanligi bitecek.

---

## 10. Faz 7 - Test ve Dogrulama

Durum: Bekliyor  
Hedef: Baglantilarin kopmadigini testlerle garanti etmek.

Minimum test gruplari:

- [ ] Urun olusturma validasyon testleri
- [ ] Urun-medya iliski testleri
- [ ] Urun-3D model iliski testleri
- [ ] Public liste sadece aktif urunleri dondurur testi
- [ ] Admin guncelleme frontend detay sonucunu etkiler testi
- [ ] Renk/malzeme/olcu konfigurasyon testi
- [ ] Soft delete sonrasi urun public listede yok testi
- [ ] Medya kullanimda ise silme engeli testi
- [ ] PDF teklif konfigurasyon verisiyle uretilir testi
- [ ] Smoke test: API, UI, urun liste, urun detay, admin giris

Basari kriteri:

- Build yesil.
- Testler yesil.
- Admin -> DB -> API -> Frontend -> 3D akisi manuel testten gececek.

---


- [ ] `VIZITLINK3D.Ortak` build context icinde gorunur olacak.
- [ ] Production UI `localhost:5015` yerine relative API/proxy kullanacak.
- [ ] CDN bagimliliklari yerel dosyaya alinacak veya kontrollu CSP yazilacak.
- [ ] Production smoke test otomatiklesecek.

---

## 12. Kritik Yasak ve Dikkat Listesi

- [ ] Razor icinde `@code` yazilmayacak.
- [ ] Razor icinde `<style>` yazilmayacak.
- [ ] Ekran metinleri `DilServisi.T()` ile olacak.
- [ ] Renk/font/bosluk CSS token kullanacak.
- [ ] Kontrolculerde try-catch olmayacak.
- [ ] Fiziksel delete yapilmayacak.
- [ ] Migration oncesi DB yedegi alinacak.
- [ ] DB tablo/sutun adlarinda Turkce karakter olmayacak.
- [ ] Secret/config anahtarlari production'da environment variable olacak.
- [ ] Harici JS kutuphaneleri wrapper servis uzerinden kullanilacak.

---

## 13. Oncelikli Is Sirasi

1. Faz 0 envanter ve yedek.
2. Urun veri modeli karari.
3. Urun-medya-3D iliski modeli.
4. API DTO ve servis omurgasi.
5. Admin urun formu.
6. Frontend liste/detay baglantisi.
7. 3D konfigurator baglantisi.
8. Testler.

---

## 14. Gunluk Takip Sablonu

```text
Tarih:
Calisilan faz:
Tamamlananlar:
Bulunan sorunlar:
Alinan kararlar:
DB yedegi:
Build durumu:
Test durumu:
Sonraki adim:
```

---

## 15. Nihai Kabul Kriteri

Bir admin kullanicisi su akisi tek panelden tamamlayabiliyorsa plan basarili sayilir:

```text
Yeni urun olustur
  -> kategori sec
  -> TR/EN metin gir
  -> ana gorsel sec
  -> galeri sec
  -> 3D model sec
  -> renk/malzeme/olcu tanimla
  -> hotspot ekle
  -> yayina al
  -> frontend detay sayfasinda ayni urunu gor
  -> 3D konfigurator calissin
  -> PDF teklif uretilsin
```

Bu akista hardcoded veri, kopuk dosya yolu, mock veri veya elle DB duzenleme olmayacak.
