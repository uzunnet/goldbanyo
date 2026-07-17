# Firma DÃ¶nÃ¼ÅŸÃ¼m Rehberi

Bu proje baÅŸka firmalara uyarlanÄ±rken marka bilgisi ve ilk tohum verisi tek merkezden yÃ¶netilir.

## Tek Kaynak Dosyalar

1. `AjanKurallari/00_PROJE_BILGISI.md`
   - Proje adÄ±, domain, iletiÅŸim, tema ve modÃ¼l kararlarÄ±.

2. `VizitLink.Api/VeriTabani/FirmaProfili.cs`
   - DB'ye yazÄ±lacak firma, lisans ve yÃ¶netici e-posta bilgileri.
   - Yeni firma iÃ§in Ã¶nce burasÄ± gÃ¼ncellenir.

3. `VizitLink.Api/appsettings.json`
   - Temiz DB dosya adÄ±.
   - Ã–rnek: `goldbanyo.db`, `yenifirma.db`.

4. `VizitLink.UI/wwwroot/css/sistem/temeller/degiskenler.css`
   - Marka renkleri token olarak gÃ¼ncellenir.
   - BileÅŸen CSS'lerinde doÄŸrudan renk yazÄ±lmaz.

5. `VizitLink.UI/wwwroot/manifest.json`
   - Program adÄ± `3DVizitLink` olarak kalÄ±r; firma adÄ± burada yazÄ±lmaz.

## Yeni Firma Kurulum AkÄ±ÅŸÄ±

1. Eski DB yedeÄŸini `Yedekler/db/` altÄ±na al.
2. `00_PROJE_BILGISI.md` iÃ§inde `proje_adi` deÄŸerini `3DVizitLink` bÄ±rak, firma alanlarÄ±nÄ± yeni firmaya gÃ¶re doldur.
3. `FirmaProfili.cs` deÄŸerlerini yeni firmaya gÃ¶re doldur.
4. `appsettings.json` iÃ§indeki DB adÄ±nÄ± yeni firmaya Ã§evir.
5. Tema token'larÄ±nÄ± firma markasÄ±na gÃ¶re gÃ¼ncelle.
6. Yeni Ã¼rÃ¼n/firma verileri gelene kadar eski sektÃ¶r seed'lerini Ã¼retimde yayÄ±nlama.
7. `dotnet build` ve `dotnet test` Ã§alÄ±ÅŸtÄ±r.
8. API ve UI smoke test yap.

## Gold Banyo BaÅŸlangÄ±Ã§ Profili

- Domain: `goldbanyom.com.tr`
- SektÃ¶r: banyo mobilyasÄ±
- DB: `goldbanyo.db`
- Ana renkler: siyah, gold, krem
- Program adÄ±: `3DVizitLink`
- ÃœrÃ¼n iÃ§eriÄŸi: resmi site serileriyle baÅŸlatÄ±ldÄ±, yeni Ã¼rÃ¼n/3D dosyalarÄ± geldikÃ§e geniÅŸletilecek.

## Sonraki Ä°yileÅŸtirme

Mevcut `TohumVerisi.cs` iÃ§inde hÃ¢lÃ¢ eski firmaya Ã¶zel demo iÃ§erikler bulunuyor. KalÄ±cÄ± Ã§Ã¶zÃ¼m:

- `FirmaProfili` sadece kimlik taÅŸÄ±r.
- ÃœrÃ¼nler, slaytlar, haberler, SSS ve sayfa iÃ§erikleri ayrÄ± `FirmaIcerikPaketi` sÄ±nÄ±flarÄ±na bÃ¶lÃ¼nÃ¼r.
- Her yeni site iÃ§in sadece yeni paket seÃ§ilir.
- Namespace/proje adÄ± deÄŸiÅŸimi en son fazda yapÄ±lÄ±r.

