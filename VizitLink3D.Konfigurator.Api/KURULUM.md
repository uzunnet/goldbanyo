# Konfigurator API — Güvenli Şifre Sıfırlama Kurulumu

## Gmail App Password ile E-posta Gönderimi

### 1. Gmail App Password Oluşturma

1. Google Hesabınız → Güvenlik → 2 Adımlı Doğrulama (etkinleştirilmeli)
2. "Uygulama Şifreleri" → Uygulama: **Posta**, Cihaz: **Diğer (özel ad)**
3. Oluşturulan 16 haneli şifreyi kopyalayın (boşluksuz)

### 2. User Secrets ile Gmail Yapılandırması

> **ASLA `appsettings.json` veya `appsettings.Development.json` içine Gmail şifresi yazmayın!**
> Bu dosyalar git'e commit edilir. User Secrets kullanın.

```powershell
# Konfigurator.Api proje dizininde:
cd VizitLink3D.Konfigurator.Api

dotnet user-secrets set "Eposta:Sunucu" "smtp.gmail.com"
dotnet user-secrets set "Eposta:Port" "587"
dotnet user-secrets set "Eposta:KullaniciAdi" "sizinmailiniz@gmail.com"
dotnet user-secrets set "Eposta:AppSifresi" "xxxxxxxxxxxxxxxx"           # 16 haneli App Password
dotnet user-secrets set "Eposta:GonderenAdres" "sizinmailiniz@gmail.com"
dotnet user-secrets set "SifreSifirlama:UygulamaUrl" "https://konfigurator.local"
```

### 3. İlk Yönetici E-posta Bootstrap (Opsiyonel)

İlk yönetici `vizitadmin` için gerçek e-posta atamak isterseniz:

```powershell
dotnet user-secrets set "IlkYonetici:KullaniciAdi" "vizitadmin"
dotnet user-secrets set "IlkYonetici:Sifre" "GuvenliSifre1!"
dotnet user-secrets set "IlkYonetici:Eposta" "admin@gercekfirma.com"
```

> **Not:** `IlkYonetici:Eposta` değeri gizli anahtar (secret) olarak saklanır — ASLA loglanmaz.
> Sadece ilk çalıştırmada, kullanıcı adı eşleştiğinde ve mevcut e-posta `@konfigurator.local` ise güncellenir.

### 4. appsettings.json Şeması (Boş/Nonsecret Varsayılanlar)

```json
{
  "Eposta": {
    "Sunucu": "",
    "Port": "587",
    "KullaniciAdi": "",
    "AppSifresi": "",
    "GonderenAdres": ""
  },
  "SifreSifirlama": {
    "UygulamaUrl": ""
  },
  "IlkYonetici": {
    "KullaniciAdi": "",
    "Sifre": "",
    "Eposta": ""
  }
}
```

> `appsettings.json` **her zaman boş/nonsecret varsayılanları** içermelidir.
> Gerçek değerler `dotnet user-secrets` veya ortam değişkenleri (`Eposta__Sunucu`, `Eposta__AppSifresi` vb.) ile sağlanır.

### 5. Ortam Değişkenleri ile Kurulum (Alternatif)

```powershell
# Windows PowerShell
$env:Eposta__Sunucu = "smtp.gmail.com"
$env:Eposta__Port = "587"
$env:Eposta__KullaniciAdi = "sizinmailiniz@gmail.com"
$env:Eposta__AppSifresi = "xxxxxxxxxxxxxxxx"
$env:Eposta__GonderenAdres = "sizinmailiniz@gmail.com"
$env:SifreSifirlama__UygulamaUrl = "https://konfigurator.local"
```

```bash
# Linux/macOS
export Eposta__Sunucu="smtp.gmail.com"
export Eposta__Port="587"
export Eposta__KullaniciAdi="sizinmailiniz@gmail.com"
export Eposta__AppSifresi="xxxxxxxxxxxxxxxx"
export Eposta__GonderenAdres="sizinmailiniz@gmail.com"
export SifreSifirlama__UygulamaUrl="https://konfigurator.local"
```

### 6. Docker Compose Örneği

```yaml
services:
  konfigurator-api:
    environment:
      - Eposta__Sunucu=smtp.gmail.com
      - Eposta__Port=587
      - Eposta__KullaniciAdi=sizinmailiniz@gmail.com
      - Eposta__AppSifresi=${GMAIL_APP_SIFRESI}    # .env dosyasından
      - Eposta__GonderenAdres=sizinmailiniz@gmail.com
      - SifreSifirlama__UygulamaUrl=https://konfigurator.ornek.com
```

### 7. Güvenlik Notları

| Konu | Durum |
|---|---|
| Gmail App Password | **ASLA** kodda, logda, API yanıtında görünmez |
| Token | **SADECE** SHA256 hash DB'de; raw token sadece e-posta linkinde |
| Hesap tarama (enumeration) | Var olan/olmayan e-posta için **aynı** generic yanıt |
| Şifre politikası | En az 8 karakter, büyük harf, küçük harf, rakam, özel karakter |
| Token süresi | 15 dakika, tek kullanımlık |
| Rate limit (istek) | 3 istek / 15 dakika |
| Rate limit (yenileme) | 5 istek / 15 dakika |
| E-posta yapılandırması yoksa | Sessizce başarısız olur, bilgi sızdırmaz |

### 8. API Uç Noktaları

```http
POST /api/kimlik/sifre-sifirlama-istegi
Content-Type: application/json

{ "eposta": "kullanici@ornek.com" }

# Yanıt (HER ZAMAN):
# { "basariliMi": true, "mesaj": "E-posta adresiniz sistemde kayitli ise..." }
```

```http
POST /api/kimlik/sifre-yenile
Content-Type: application/json

{ "token": "...", "yeniSifre": "YeniSifre1!" }

# Başarılı:
# { "basariliMi": true, "mesaj": "Sifreniz basariyla yenilendi." }

# Başarısız:
# { "basariliMi": false, "mesaj": "Sifre sifirlama baglantisi gecersiz..." }
```
