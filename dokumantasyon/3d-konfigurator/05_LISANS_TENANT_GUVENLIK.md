# 3D Konfigüratör — Lisans, Tenant & Güvenlik

> **Proje:** Gold Banyo / VizitLink3D
> **Tarih:** 20 Temmuz 2026
> **Durum:** Onay Bekliyor

---

## 1. Genel Bakış

Bu doküman, 3D konfigüratörün **çok kiracılı (multi-tenant)** yapısını, **lisans yönetimini** ve **güvenlik önlemlerini** tanımlar.

---

## 2. Multi-Tenant Mimari

### 2.1 Tenant Tespiti

```
İstek Gelir
    │
    ├─ Domain tabanlı tespit (varsayılan)
    │   goldbanyo.com.tr → TenantId: "gold-banyo"
    │   abc-mobilya.com.tr → TenantId: "abc-mobilya"
    │
    ├─ Header tabanlı tespit (API için)
    │   X-Tenant-Id: "gold-banyo"
    │
    └─ Subdomain tabanlı tespit (isteğe bağlı)
        goldbanyo.vizitlink3d.com.tr → TenantId: "gold-banyo"
```

### 2.2 Tenant Yapısı

```csharp
public class Tenant
{
    public Guid Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? AltAlanAdi { get; set; }
    
    // Lisans
    public LisansTuru LisansTuru { get; set; }
    public DateTime LisansBaslangic { get; set; }
    public DateTime? LisansBitis { get; set; }
    public int MaksimumUrun3D { get; set; } = 50;
    public int MaksimumModelMB { get; set; } = 500;
    public int MaksimumAPIIstegiGunluk { get; set; } = 10000;
    
    // Tema
    public string? TemaKodu { get; set; }
    public string? LogoURL { get; set; }
    public string? AnaRenk { get; set; }
    
    // Durum
    public bool Aktif { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; }
}
```

### 2.3 Tenant İzolasyon Katmanları

#### Katman 1: Veritabanı İzolasyonu

```csharp
// Her sorguda TenantId filtresi
public async Task<List<Urun3D>> UrunleriListele(Guid tenantId)
{
    return await db.Urun3D
        .Where(u => u.TenantId == tenantId && !u.SilindiMi)
        .ToListAsync();
}

// Global query filter (EF Core)
modelBuilder.Entity<Urun3D>().HasQueryFilter(u => u.TenantId == _tenantId);
modelBuilder.Entity<Parca3D>().HasQueryFilter(p => p.TenantId == _tenantId);
```

#### Katman 2: Dosya Sistemi İzolasyonu

```
wwwroot/medya/3d-modeller/
├── gold-banyo/           # Tenant: Gold Banyo
│   ├── urun-1/
│   └── urun-2/
├── abc-mobilya/          # Tenant: ABC Mobilya
│   └── urun-1/
```

```csharp
public string ModelYoluOlustur(Guid tenantId, Guid urunId, string dosyaAdi)
{
    var tenantKlasoru = Path.Combine("wwwroot", "medya", "3d-modeller", tenantId.ToString());
    Directory.CreateDirectory(tenantKlasoru);
    return Path.Combine(tenantKlasoru, urunId.ToString(), dosyaAdi);
}
```

#### Katman 3: Cache İzolasyonu

```csharp
public string CacheKeyOlustur(Guid tenantId, string tip, string anahtar)
{
    return $"vizitlink3d:{tenantId}:{tip}:{anahtar}";
}

// Örnek: vizitlink3d:gold-banyo:urun:slug-banyo-dolabi
```

#### Katman 4: API İzolasyonu

```csharp
// Middleware: Her istekte TenantId extracts
public class TenantMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = await TenantTespitEt(context);
        context.Items["TenantId"] = tenantId;
        await _next(context);
    }
    
    private async Task<Guid> TenantTespitEt(HttpContext context)
    {
        // 1. Header'dan kontrol
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenant))
        {
            return Guid.Parse(headerTenant!);
        }
        
        // 2. Domain'den kontrol
        var domain = context.Request.Host.Host;
        var tenant = await _tenantService.DomainIleBul(domain);
        
        if (tenant == null)
            throw new UnauthorizedAccessException("Tenant bulunamadı.");
        
        return tenant.Id;
    }
}
```

#### Katman 5: JWT Token İzolasyonu

```csharp
public class JwtTokenServisi
{
    public string TokenOlustur(Guid tenantId, Guid kullaniciId, string roller)
    {
        var claims = new[]
        {
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("kullanici_id", kullaniciId.ToString()),
            new Claim("roller", roller),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        // Token içinde tenantId mutlaka olmalı
        // Tenant'lar arası geçiş engellenir
    }
}
```

### 2.4 Tenant Bağlımlılık Kontrolü

```csharp
public class TenantYetkiKontrolcu
{
    public async Task<bool> UrunErisilebilirMi(Guid tenantId, Guid urunId)
    {
        var urun = await db.Urun3D.FindAsync(urunId);
        
        if (urun == null || urun.SilindiMi)
            return false;
        
        // Kullanıcının tenant'ı ile ürünün tenant'ı aynı mı?
        return urun.TenantId == tenantId;
    }
    
    public async Task<bool> LisansLimitIciMi(Guid tenantId, string islemTipi)
    {
        var tenant = await db.Tenant.FindAsync(tenantId);
        
        return islemTipi switch
        {
            "urun_ekle" => await db.Urun3D
                .CountAsync(u => u.TenantId == tenantId) < tenant.MaksimumUrun3D,
            
            "model_yukle" => await ModelBoyutuHesapla(tenantId) < tenant.MaksimumModelMB * 1024 * 1024,
            
            "api_istek" => await GunlukAPIIstegiHesapla(tenantId) < tenant.MaksimumAPIIstegiGunluk,
            
            _ => true
        };
    }
}
```

---

## 3. Lisans Yönetimi

### 3.1 Lisans Tipleri

| Lisans | Fiyat Aralığı | Ürün Limiti | Model Limiti | API Limiti | Embed |
|--------|---------------|-------------|--------------|-----------|-------|
| **Free** | ₺0/ay | 5 ürün | 100MB | 1.000/gün | ❌ |
| **Starter** | ₺999/ay | 50 ürün | 1GB | 10.000/gün | ✅ |
| **Professional** | ₺2.999/ay | 200 ürün | 5GB | 50.000/gün | ✅ |
| **Enterprise** | ₺9.999/ay | Sınırsız | 20GB | 200.000/gün | ✅ |
| **Custom** | Görüşme | Görüşme | Görüşme | Görüşme | ✅ |

### 3.2 Lisans Kontrolleri

```csharp
public class LisansKontrolcu
{
    public async Task LisansKontrolu(Guid tenantId, string islem)
    {
        var tenant = await db.Tenant.FindAsync(tenantId);
        
        if (tenant == null || !tenant.Aktif)
            throw new LisansException("Tenant pasif veya bulunamadı.");
        
        if (tenant.LisansBitis.HasValue && tenant.LisansBitis < DateTime.UtcNow)
            throw new LisansException("Lisans süresi dolmuş.");
        
        switch (islem)
        {
            case "urun_ekle":
                var mevcutUrun = await db.Urun3D
                    .CountAsync(u => u.TenantId == tenantId && !u.SilindiMi);
                if (mevcutUrun >= tenant.MaksimumUrun3D)
                    throw new LisansException(
                        $"Ürün limiti dolmuş ({mevcutUrun}/{tenant.MaksimumUrun3D}). " +
                        $"Lütfen yükseltme yapın.");
                break;
                
            case "embed_kullanim":
                if (tenant.LisansTuru == LisansTuru.Free)
                    throw new LisansException(
                        "Embed özelliği Free lisanslarda desteklenmiyor.");
                break;
        }
    }
}
```

### 3.3 Lisans Durumu API

```csharp
[HttpGet("api/admin/lisans-durumu")]
public async Task<Cevap<LisansDurumu>> LisansDurumu()
{
    var tenantId = HttpContext.GetTenantId();
    var tenant = await db.Tenant.FindAsync(tenantId);
    
    var durum = new LisansDurumu
    {
        LisansTuru = tenant.LisansTuru,
        BaslangicTarihi = tenant.LisansBaslangic,
        BitisTarihi = tenant.LisansBitis,
        GunKaldi = tenant.LisansBitis.HasValue 
            ? (int)(tenant.LisansBitis.Value - DateTime.UtcNow).TotalDays 
            : -1,
        Kullanim = new LisansKullanim
        {
            UrunAdet = await db.Urun3D.CountAsync(u => u.TenantId == tenantId),
            UrunLimit = tenant.MaksimumUrun3D,
            ModelBoyutMB = await ModelBoyutuHesapla(tenantId),
            ModelLimitMB = tenant.MaksimumModelMB,
            GunlukAPIIstek = await GunlukAPIIstegiHesapla(tenantId),
            GunlukAPILimit = tenant.MaksimumAPIIstegiGunluk
        }
    };
    
    return Cevap<LisansDurumu>.Basarili(durum);
}
```

---

## 4. Güvenlik Önlemleri

### 4.1 JWT Kimlik Doğrulama

```csharp
// Token yapılandırması
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
```

### 4.2 Yetkilendirme

```csharp
public enum Roller
{
    Ziyaretci = 0,      // Public viewer
    Kullanici = 10,     // Giriş yapmış kullanıcı
    Editör = 20,        // Metadata düzenleme
    Admin = 30,         // Tam yetki
    SuperAdmin = 40     // Tenant yönetimi
}

// Yetki politikaları
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudioErisim", policy =>
        policy.RequireRole("Admin", "Editör", "SuperAdmin"));
    
    options.AddPolicy("ModelYukleme", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));
    
    options.AddPolicy("TenantYonetim", policy =>
        policy.RequireRole("SuperAdmin"));
    
    options.AddPolicy("EmbedErisim", policy =>
        policy.RequireAuthenticatedUser());
});
```

### 4.3 CORS Politikası

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("EmbedCORS", policy =>
    {
        policy.WithOrigins(
                "https://goldbanyo.com.tr",
                "https://www.goldbanyo.com.tr",
                "https://*.goldbanyo.com.tr"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
    
    options.AddPolicy("PublicAPI", policy =>
    {
        policy.AllowAnyOrigin() // Public API için
            .WithMethods("GET")
            .WithHeaders("Authorization", "X-Tenant-Id");
    });
});
```

### 4.4 Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    // IP bazlı - genel
    options.AddFixedWindowLimiter("Genel", opt =>
    {
        opt.PermitLimit = 1000;
        opt.Window = TimeSpan.FromMinutes(5);
    });
    
    // IP bazlı - giriş
    options.AddFixedWindowLimiter("Giris", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
    });
    
    // Tenant bazlı - API
    options.AddFixedWindowLimiter("TenantAPI", opt =>
    {
        opt.PermitLimit = 10000;
        opt.Window = TimeSpan.FromHours(24);
        opt.QueueLimit = 100;
    });
    
    // IP bazlı - Embed
    options.AddFixedWindowLimiter("Embed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromHours(1);
    });
});
```

### 4.5 Input Doğrulama

```csharp
// FluentValidation ile DTO doğrulama
public class ParcaMetadataDogrulayici : AbstractValidator<ParcaMetadataGuncellemeIstegi>
{
    public ParcaMetadataDogrulayici()
    {
        RuleFor(x => x.ParcaKodu)
            .NotEmpty().WithMessage("Parça kodu boş olamaz.")
            .MaximumLength(50).WithMessage("Parça kodu en fazla 50 karakter olabilir.")
            .Matches("^[A-Z0-9\\-]+$").WithMessage("Parça kodu sadece büyük harf, rakam ve tire içerebilir.");
        
        RuleFor(x => x.AdTR)
            .NotEmpty().WithMessage("Türkçe ad boş olamaz.")
            .MinimumLength(3).WithMessage("Türkçe ad en az 3 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Türkçe ad en fazla 200 karakter olabilir.");
        
        RuleFor(x => x.Kategori)
            .IsInEnum().WithMessage("Geçersiz kategori.");
        
        RuleFor(x => x.Geometry)
            .NotNull().WithMessage("Geometry bilgisi zorunludur.")
            .ChildRules(geometry =>
            {
                geometry.RuleFor(g => g.Olculer)
                    .NotNull().WithMessage("Ölçüler zorunludur.");
                geometry.RuleFor(g => g.Olculer.X)
                    .GreaterThan(0).WithMessage("En ölçüsü sıfırdan büyük olmalıdır.");
                geometry.RuleFor(g => g.Olculer.Y)
                    .GreaterThan(0).WithMessage("Boy ölçüsü sıfırdan büyük olmalıdır.");
                geometry.RuleFor(g => g.Olculer.Z)
                    .GreaterThan(0).WithMessage("Derinlik ölçüsü sıfırdan büyük olmalıdır.");
            });
        
        RuleFor(x => x.Malzeme)
            .NotNull().WithMessage("Malzeme bilgisi zorunludur.")
            .ChildRules(malzeme =>
            {
                malzeme.RuleFor(m => m.Turu)
                    .IsInEnum().WithMessage("Geçersiz malzeme türü.");
                malzeme.RuleFor(m => m.VarsayilanRenk)
                    .NotEmpty().WithMessage("Varsayılan renk boş olamaz.")
                    .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Geçersiz renk kodu (örnek: #1A1A27).");
            });
    }
}
```

### 4.6 XSS Koruması

```csharp
// Razor'da otomatik HTML encoding
@Html.Raw(Model.Aciklama) // ❌ YASAK - XSS açığı
@Model.Aciklama           // ✅ DOĞRU - otomatik encode

// API tarafında HTML temizleme
using Ganss.Xss;
var sanitizer = new HtmlSanitizer();
sanitizer.AllowedTags.Clear(); // Hiçbir HTML etiketine izin yok
var temizMetin = sanitizer.Sanitize(kirliMetin);
```

### 4.7 SQL Injection Koruması

```csharp
// ❌ YASAK - SQL Injection açığı
var query = $"SELECT * FROM Urun3D WHERE Ad = '{arama}'";

// ✅ DOĞRU - Parameterized query
var query = "SELECT * FROM Urun3D WHERE Ad = @arama";
var results = await db.Urun3D.FromSqlRaw(query, new SqlParameter("@arama", arama)).ToListAsync();

// ✅ DOĞRU - EF Core LINQ
var results = await db.Urun3D
    .Where(u => u.Ad.Contains(arama))
    .ToListAsync();
```

### 4.8 Dosya Yükleme Güvenliği

```csharp
public class DosyaYuklemeServisi
{
    private readonly string[] _izinliFormatlar = { ".glb", ".gltf" };
    private readonly string[] _izinliMimeTipler = { "model/gltf-binary", "model/gltf+json" };
    private readonly long _maxDosyaBoyutu = 30 * 1024 * 1024; // 30MB
    
    public async Task<string> DosyaYukle(Guid tenantId, Guid urunId, IFormFile dosya)
    {
        // 1. Format kontrolü
        var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
        if (!_izinliFormatlar.Contains(uzanti))
            throw new BusinessException("Sadece GLB/GLTF dosyaları yüklenebilir.");
        
        // 2. MIME tipi kontrolü
        if (!_izinliMimeTipler.Contains(dosya.ContentType))
            throw new BusinessException("Geçersiz dosya türü.");
        
        // 3. Boyut kontrolü
        if (dosya.Length > _maxDosyaBoyutu)
            throw new BusinessException($"Dosya boyutu {_maxDosyaBoyutu / 1024 / 1024}MB'ı aşamaz.");
        
        // 4. Dosya adı temizleme
        var temizAd = Path.GetFileName(dosya.FileName)
            .Replace(" ", "_")
            .Replace("\"", "")
            .Replace("'", "");
        
        // 5. Tenant bazlı kayıt
        var dosyaYolu = Path.Combine(
            "wwwroot", "medya", "3d-modeller",
            tenantId.ToString(), urunId.ToString(), temizAd);
        
        Directory.CreateDirectory(Path.GetDirectoryName(dosyaYolu)!);
        
        using var stream = new FileStream(dosyaYolu, FileMode.Create);
        await dosya.CopyToAsync(stream);
        
        return dosyaYolu;
    }
}
```

---

## 5. Embed Güvenliği

### 5.1 CSP (Content Security Policy)

```
Content-Security-Policy:
  default-src 'self';
  script-src 'self' 'unsafe-inline' 'unsafe-eval'; // Three.js için gerekli
  style-src 'self' 'unsafe-inline';
  img-src 'self' data: blob:;
  media-src 'self' blob:;
  connect-src 'self' https://goldbanyo.com.tr;
  font-src 'self';
  object-src 'none';
  frame-ancestors 'self' *.goldbanyo.com.tr;
  base-uri 'self';
  form-action 'self';
```

### 5.2 Sandbox

```html
<iframe
  src="https://goldbanyo.com.tr/embed/..."
  sandbox="allow-scripts allow-same-origin allow-popups"
  allow="autoplay; fullscreen"
></iframe>
```

### 5.3 PostMessage Güvenliği

```javascript
// Embed tarafından
window.parent.postMessage({
  tip: 'parcaSecildi',
  veri: { parcaId: 'pk-001' }
}, 'https://goldbanyo.com.tr'); // ✅ Specific origin

// ❌ YASAK
window.parent.postMessage(data, '*'); // Tüm originlere gönderim
```

---

## 6. Audit Log

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? KullaniciId { get; set; }
    public string Islem { get; set; } = string.Empty;
    public string Kaynak { get; set; } = string.Empty;
    public string? Detay { get; set; }
    public string? oncekiDeger { get; set; }
    public string? yeniDeger { get; set; }
    public string IPAdresi { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime Zaman { get; set; } = DateTime.UtcNow;
    public bool Basarili { get; set; } = true;
    public string? HataMesaji { get; set; }
}

// Audit log oluşturma
public class AuditLogServisi
{
    public async Task LogOlustur(
        Guid tenantId,
        Guid? kullaniciId,
        string islem,
        string kaynak,
        object? detay = null,
        object? onceki = null,
        object? yeni = null)
    {
        var log = new AuditLog
        {
            TenantId = tenantId,
            KullaniciId = kullaniciId,
            Islem = islem,
            Kaynak = kaynak,
            Detay = detay?.ToJson(),
            oncekiDeger = onceki?.ToJson(),
            yeniDeger = yeni?.ToJson(),
            IPAdresi = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString()
        };
        
        await db.AuditLog.AddAsync(log);
        await db.SaveChangesAsync();
    }
}
```

### Audit Loglanması Gereken İşlemler

| İşlem | Öncelik | Detay |
|-------|---------|-------|
| Model yükleme/silme | Yüksek | Dosya adı, boyutu |
| Metadata değiştirme | Yüksek | Değişen alanlar, eski/yeni değer |
| Renk/malzeme değiştirme | Orta | Değişiklik detayı |
| Hareket parametresi değiştirme | Orta | Eksen, açı, hız |
| Embed erişimi | Düşük | IP, domain, ürün |
| API isteği | Düşük | Endpoint, yanıt süresi |
| Lisans değişikliği | Yüksek | Eski/yeni lisans |
| Tenant ayar değişikliği | Yüksek | Değişen ayarlar |

---

## 7. Şifreleme

### 7.1 Hassas Veriler

| Veri | Şifreleme | Saklama |
|------|----------|---------|
| JWT Token Key | AES-256 | appsettings (production: env variable) |
| API Key'ler | BCrypt hash | DB'de hashlenmiş |
| Şifreler | BCrypt (work factor: 12) | DB'de hashlenmiş |
| Model dosyaları | TLS (iletim) | Diskte şifrelenmemiş |

### 7.2 BCrypt Kullanımı

```csharp
public class SifreServisi
{
    public string SifreHashle(string sifre)
    {
        return BCrypt.Net.BCrypt.HashPassword(sifre, workFactor: 12);
    }
    
    public bool SifreDogrula(string sifre, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(sifre, hash);
    }
}
```

---

## 8. GDPR Uyumluluğu

| Hak | Uygulama |
|-----|----------|
| Bilgi Edinme | Gizlilik politikası sayfası |
| Erişim Hakkı | `/api/admin/kullanici-verileri` |
| Düzeltme Hakkı | `/api/admin/profil-guncelle` |
| Silinme Hakkı | `/api/admin/hesap-sil` (soft delete + 30 gün) |
| Taşınabilirlik | `/api/admin/disari-aktar` (JSON export) |
| İtiraz | Veri işlenmeyi durdurma |

---

## 9. Monitoring ve Alarm

| Metrik | Eşik | Alarm |
|--------|------|-------|
| Başarısız giriş denemesi | 5/dk (IP) | E-posta + Slack |
| Rate limit aşımı | 10/dk | E-posta |
| Tenant veri sızıntısı denemesi | 1 (hemen) | SMS + Slack |
| Hatalı API isteği | >%5 (1 dk) | E-posta |
| Yüksek hata oranı | >%10 (5 dk) | SMS + Slack |
| Lisans süresi dolma | 7 gün kala | E-posta |
| Disk kullanımı | >%80 | E-posta |

---

## 10. Güvenlik Kontrol Listesi

### Deploy Öncesi

- [ ] JWT key production ortamında güçlü mü?
- [ ] CORS sadece izinli domain'leri kapsıyor mu?
- [ ] Rate limiting aktif mi?
- [ ] CSP header'ı tanımlı mı?
- [ ] Sandbox aktif mi (iframe)?
- [ ] Tüm input'lar doğrulanıyor mu?
- [ ] SQL injection koruması aktif mi?
- [ ] XSS koruması aktif mi?
- [ ] Dosya yükleme limitleri tanımlı mı?
- [ ] Audit logging aktif mi?
- [ ] Error handler stack trace göstermiyor mu?
- [ ] HTTPS zorunlu mu?
- [ ] HSTS aktif mi?
- [ ] Secure cookie flags tanımlı mı?

### Periyodik Kontroller

- [ ] Aylık: Güvenlik açığı taraması
- [ ] Üç aylık: Penetrasyon testi
- [ ] Altı aylık: Lisans gözden geçirme
- [ ] Yıllık: Güvenlik politikası güncelleme

---

## 11. Onay

- [ ] Ustam onayı
- [ ] Güvenlik ekibi incelemesi
- [ ] Penetrasyon testi planı
- [ ] GDPR uyumluluk incelemesi
- [ ] Monitoring kurulumu
