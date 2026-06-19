# Kapi Modelleri Dinamik Yonetim Task Notu

## Tamamlananlar

- Admin icin `Kapı/Kapak Model Yönetimi` sayfasi eklendi.
- Frontend kapak ve kapi liste sayfalari `api/kapak-modelleri` kaynagindan dinamik veri okumaya baglandi.
- Detay sayfasi urun API yerine `KapakModeli` API kaydini kullanacak sekilde guncellendi.
- Detay sayfasinda ayni component icinde model degisince veri yeniden yukleniyor.
- Benzer modeller ve cok izlenenler API endpointleri eklendi.
- `I:\modeller\Yeni klasör\kapi-modelleri-duzenli` altindaki 164 gorsel publish edilebilir medya klasorune alindi.

## Sonraki Model Icin Notlar

- Daha dusuk maliyetli bir model kullanilacaksa once sadece dosya adi, kategori ve gorsel kalite siniflandirmasi yaptirilabilir.
- 3D model uretimi veya detayli semantik aciklama icin ayrica daha guclu model sadece secilen en iyi kayitlarda calistirilmali.
- Admin kaydi icin minimum veri: `ModelAdi`, `ModelKodu`, `ModelTuru`, `Kategori`, `AnaGorselUrl`, `OneCikanMi`, `YeniMi`.
- Toplu ice aktarma icin seed yapisi hazir: medya klasorune eklenen yeni gorseller eksik kayit olarak DB'ye islenebilir.

## Kontrol Listesi

- `dotnet build Desadoor.Api/Desadoor.Api.csproj --no-restore`
- `dotnet build Desadoor.UI/Desadoor.UI.csproj --no-restore`
- `/api/kapak-modelleri?modelTuru=Kapi&adet=3`
- `/api/kapak-modelleri/cok-izlenen?modelTuru=Kapi&adet=3`
- `/kapi-modelleri`
- `/kapi/{id}/{kod}`
- `/admin/kapak-modeli-yonetimi`
