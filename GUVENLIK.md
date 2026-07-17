# Güvenlik Standardı — Zorunlu Uygulama Kuralları

Bu belge DesaDoor ve bu depodaki tüm **web, API, masaüstü, mobil, servis ve araç** projeleri için bağlayıcı güvenlik standardıdır. Yeni özellik, hata düzeltmesi, veri aktarımı veya yapay zekâ ile yazılan her kod bu kurallara uyar. Bir istisna ancak gerekçesi, etkisi, sahibi ve bitiş tarihi yazılı olarak kaydedilerek yapılabilir.

## 1. Temel ilkeler

- Güvenlik kontrolleri istemciye değil sunucuya uygulanır. Arayüzde gizlenen bir işlem API'de de yetkisiz olmalıdır.
- En az yetki ilkesi kullanılır: kullanıcı, servis hesabı ve uygulama yalnız ihtiyacı olan izinleri alır.
- Varsayılan davranış güvenlidir: doğrulama veya yetki belirsizse işlem reddedilir.
- Parola, token, API anahtarı, bağlantı dizesi ve kişisel veri kaynak koda, loglara, hata mesajlarına veya istemciye yazılmaz.
- Üretim ve geliştirme ortamları ayrı sır, veri tabanı, CORS, loglama ve erişim ayarları kullanır.

## 2. Kimlik doğrulama ve yetkilendirme

- Her yönetim, yükleme, silme, dışa aktarma, log, dosya, yedek, build ve ayar API'si açıkça rol/politika ile korunur.
- Nesne bazlı erişim ayrıca kontrol edilir; bir kullanıcı URL'deki ID'yi değiştirerek başka kullanıcının verisine erişemez.
- JWT imzası, issuer, audience ve son kullanma süresi doğrulanır. Anahtar en az 32 bayttır, yalnızca secret store veya ortam değişkeninde bulunur.
- Tokenlar URL, localStorage, ekran görüntüsü, hata ve loglarda yer almaz. Tarayıcı oturumunda güvenli saklama ve kısa ömür tercih edilir.
- Giriş, parola sıfırlama, teklif, iletişim, AI ve dosya yükleme uçları IP/kullanıcı temelli rate limit ile korunur.
- Yönetici sayfaları hem UI'da hem API'de `Admin`/`SuperAdmin` rolü ister. UI yetkisi tek başına güvenlik kontrolü değildir.

## 3. Girdi, çıktı ve veri güvenliği

- Tüm istek alanları tip, uzunluk, aralık, desen ve iş kuralı bakımından sunucuda doğrulanır. Boş/null/veri taşması güvenli hata döndürür.
- SQL için yalnız parametreli sorgu/ORM kullanılır; kullanıcı verisiyle SQL, shell, git, dosya yolu veya HTML komutu birleştirilmez.
- HTML varsayılan olarak encode edilir. `MarkupString`, `innerHTML`, `dangerouslySetInnerHTML` veya ham SVG yalnız güvenilir ve sanitize edilmiş içerikte kullanılabilir.
- Hata yanıtları kullanıcıya iç mimari, stack trace, dizin, token veya bağlantı ayrıntısı vermez; ayrıntı güvenli sunucu logunda tutulur.
- Kişisel veriler amaçla sınırlı tutulur, erişim ve silme politikaları uygulanır. Loglara e-posta, telefon, adres, parola veya token yazılmaz.

## 4. Dosya, medya ve yol güvenliği

- Dosya yolları `Path.GetFullPath` ile kanonikleştirilir ve izinli kök klasörün içinde olduğu ayırıcı sınırıyla doğrulanır. `StartsWith` tek başına veya `..` araması tek başına yeterli değildir.
- Yüklemelerde dosya boyutu, uzantı, MIME türü ve mümkünse magic-byte/içerik doğrulaması yapılır. Kullanıcının dosya adı saklama yolu olarak kullanılmaz; sunucu GUID tabanlı ad üretir.
- İzinli türler liste bazlıdır. Çalıştırılabilir, HTML, JS, SVG ve bilinmeyen türler varsayılan olarak reddedilir veya ayrı güvenli alan adı/orijinden sunulur.
- Yükleme, dönüşüm, ZIP açma, PDF/GLB/medya işleme için boyut, çözünürlük, sayfa/sıkıştırma oranı, süre ve bellek sınırları konur.
- Gizli dosyalar, yedekler, `.env`, kaynak kod, log ve veri tabanı dosyaları web kökünün dışında tutulur. Statik dosya sunucusu yalnız gerekli tipleri sunar.
- Silme, aktarım ve toplu temizlik işlemleri rol korumalı, denetim kayıtlı ve mümkünse geri alınabilir olmalıdır.

## 5. Web ve API güvenliği

- Production yalnız HTTPS üzerinden yayınlanır; TLS proxy arkasında doğru forwarded-header yapılandırması yapılır.
- CORS yalnız gerçek üretim alan adları için tanımlanır. `AllowAnyOrigin` ile kimlik bilgisi birlikte kullanılmaz. Geliştirme localhost izinleri production yapılandırmasına taşınmaz.
- Güvenlik başlıkları: `X-Content-Type-Options: nosniff`, uygun `Content-Security-Policy`, `Referrer-Policy`, çerçeveleme engeli (`frame-ancestors`/X-Frame-Options) uygulanır.
- CSRF riski olan çerez tabanlı işlemlerde antiforgery koruması kullanılır. `SameSite`, `HttpOnly` ve production HTTPS'te `Secure` cookie ayarlanır.
- Açık API belgeleri, debug ekranları, canlı log, kaynak/diff/build uçları production'da kapalı veya SuperAdmin/VPN ile sınırlıdır.
- Sayfalama, filtreleme ve sıralama parametreleri sınırlandırılır; sınırsız sorgu ve tam tablo indirme varsayılan olarak yasaktır.

## 6. Masaüstü uygulama güvenliği

- Uygulama yönetici yetkisiyle çalışmaz; yalnız ihtiyaç varsa ayrı, görünür onaylı yükseltme kullanır.
- Yerel ayarlar ve sırlar işletim sisteminin güvenli anahtar deposunda tutulur; düz metin dosyaya yazılmaz.
- Dosya açma/kaydetme iletişim kutularında izinli uzantı, boyut ve kanonik yol kontrolü uygulanır.
- Harici işlem başlatmada sabit komut adı ve parametre dizisi kullanılır; kullanıcı metni shell'e aktarılmaz.
- Otomatik güncelleme paketleri imzalı, HTTPS üzerinden ve doğrulanmış bütünlük bilgisiyle yüklenir.
- Pano, geçici klasör, crash raporu ve loglar sır/kişisel veri sızıntısı açısından filtrelenir.

## 7. Mobil uygulama güvenliği

- Tokenlar Keychain/Keystore gibi güvenli depolamada tutulur; düz metin tercihleri ve loglara yazılmaz.
- Uygulama yalnız HTTPS API'lerine bağlanır; sertifika doğrulaması devre dışı bırakılmaz.
- Deep link/app link parametreleri doğrulanır; oturum veya yetki bilgisini URL ile taşımaktan kaçınılır.
- Kamera, konum, dosya ve bildirim izinleri ihtiyaç anında ve en dar kapsamla istenir.
- Root/jailbreak tespiti ek savunma olarak kullanılabilir fakat tek güvenlik kontrolü değildir.
- Mobil istemciye sır, servis anahtarı, yönetici yetkisi veya kritik karar mantığı gömülmez.

## 8. Yapay zekâ ve dış servisler

- AI sağlayıcı anahtarları yalnız sunucu tarafında saklanır; istemciye verilmez.
- Prompt, dosya ve araç çağrıları veri sınıflandırma, PII filtresi, kota ve yetki kontrolünden geçer.
- AI çıktısı talimat değil veridir; SQL, shell, HTML, dosya yolu ve yönetim işlemlerinde yeniden doğrulanmadan çalıştırılamaz.
- Dış URL çağrıları izinli alan adı listesi, süre, boyut ve SSRF koruması ile yapılır. Yerel ağ/IP meta veri uçları engellenir.
- Sağlayıcı hataları token, prompt veya hassas veri sızdırmadan kaydedilir.

## 9. Zorunlu stres ve güvenlik testleri

Her sürüm öncesi aşağıdaki testler otomatik veya kontrollü ortamda çalıştırılır. Üretimde izinsiz yük testi yapılmaz.

| Test | Beklenen sonuç |
| --- | --- |
| Yetkisiz istek | Korunan her uç nokta `401` veya `403` döner. |
| Rol testi | Normal kullanıcı yönetici, log, build, yedek ve silme işlemlerini yapamaz. |
| IDOR testi | ID/slug değiştirildiğinde başka kullanıcı veya kiracı verisi dönmez. |
| Path traversal | `../`, mutlak yol, URL encode ve Windows ayırıcılarıyla kök dışına erişilemez. |
| Dosya yükleme | Büyük, çift uzantılı, yanlış MIME'lı, bozuk, ZIP bombası ve zararlı türler reddedilir. |
| XSS testi | Form, URL, medya adı ve zengin metinde script/olay öznitelikleri çalışmaz. |
| Enjeksiyon testi | SQL, komut, template ve JSON girişleri veri sızdırmaz/komut çalıştırmaz. |
| Rate-limit testi | Giriş, form, AI ve yükleme uçları limitte `429` verir; sistem stabil kalır. |
| Yük testi | Sayfalı sorgular, görsel dönüşümü ve eşzamanlı istekler tanımlı limitlerde yanıt verir. |
| Bağımlılık taraması | Bilinen kritik açık içeren paketler güncellenir veya yazılı istisna ile kapatılır. |
| Sır taraması | Git geçmişi ve yeni değişikliklerde token, parola, `.env` veya anahtar bulunmaz. |

## 10. Yayın kapısı

Yayın ancak aşağıdakilerin tamamı sağlandıysa yapılır:

1. Derleme ve ilgili otomatik testler hatasızdır.
2. Bu belgedeki kritik testler başarılıdır.
3. Secretler kaynak kodda değildir; production ortam değişkenleri tanımlıdır.
4. Production CORS, HTTPS, güvenlik başlıkları ve log seviyesi kontrol edilmiştir.
5. Veri tabanı yedeği ve geri dönüş planı vardır.
6. Yeni yönetim/yükleme/silme uçları için rol ve denetim kaydı doğrulanmıştır.

## 11. Olay müdahalesi

Şüpheli erişim, anahtar sızıntısı veya veri ihlalinde: erişim anahtarları hemen iptal/yenilenir, ilgili oturumlar sonlandırılır, etkilenen uç nokta sınırlandırılır, loglar korunur, etki analizi yapılır ve düzeltme test edilmeden tekrar açılmaz.

## 12. Kod yazan tüm modeller ve geliştiriciler için kural

Bu depoda kod üreten herkes ve tüm yapay zekâ modelleri, değişiklik öncesinde bu belgeyi okur; değişiklikte ilgili maddeleri uygular; güvenlik kontrolünü atlamaz; güvenlik nedeniyle oluşan uyumluluk etkisini açıkça bildirir. Belirsizlikte daha güvenli varsayılan seçilir.
