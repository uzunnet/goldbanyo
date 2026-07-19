# VIZITLINK3D → VizitLink3D Yeniden Adlandırma Planı

> Marka: **3DVizitLink** | Kod: **VizitLink3D**  
> Tarih: 2026-06-26  
> Kapsam: ~450 dosya, ~4600+ satır, 37 dosya/klasör adı

---

## Özet Tablo

| Kategori | Dosya | Eylem |
|---|---|---|
| Çözüm dosyası | `VIZITLINK3D.slnx` | Yeniden adlandır + içerik güncelle |
| Proje klasörleri (4) | `VIZITLINK3D.Api/` vb. | Klasör + .csproj yeniden adlandır |
| Eski VizitLink klasörleri (4) | `VizitLink.Api/` vb. | Sil (sadece log/build artifakt) |
| C# namespace (400+ satır) | ~366 .cs dosyası | `VIZITLINK3D.*` → `VizitLink3D.*` |
| Razor @using (270+ satır) | ~80 .razor dosyası | `VIZITLINK3D.*` → `VizitLink3D.*` |
| _Imports.razor | 1 dosya, 11 satır | Namespace güncelle |
| VIZITLINK3DDbContext | 150+ kullanım | `VIZITLINK3DDbContext` → `VizitLink3DDbContext` |
| .csproj içeriği | 5 dosya | ProjectReference yolları |
| appsettings.json | 1 dosya | DB yolu, JWT, HMAC |
| docker-compose.yml | 1 dosya | Servis adları, env, volume |
| Dockerfile (2) | Api + UI | `dotnet` komut satırları |
| entrypoint.sh | 1 dosya | DB yolu, DLL adı |
| CSS dosyaları (3 kaynak) | `VIZITLINK3D.css` | Dosya adı + iç referans |
| Razor dosyası | `VIZITLINK3DDuzen.razor` | `VizitLink3DDuzen.razor` |
| API route'lar (11) | `[Route("api/VIZITLINK3D/...")]` | `api/vizitlink3d/...` |
| Hardcoded string (6) | `Slug == "VIZITLINK3D"` | `vizitlink3d` |
| JS localStorage | 4 anahtar | `VIZITLINK3D_*` → `vizitlink3d_*` |
| Program.cs (Api) | 1 dosya, 18 satır | Log yolu, DB yolu, env |
| Görseller/logo | `VIZITLINK3D-logo.svg` vb. | İsim değişikliği |

---

## Aşama 1: Eski VizitLink Klasörlerini Temizle

Mevcut `VizitLink.*` klasörleri sadece log/build artifakt içeriyor, kaynak kod yok.

```powershell
Remove-Item -Recurse -Force VizitLink.Api, VizitLink.Ortak, VizitLink.Testler, VizitLink.UI
```

---

## Aşama 2: Proje Klasörlerini Yeniden Adlandır

```powershell
Rename-Item VIZITLINK3D.Api VizitLink3D.Api
Rename-Item VIZITLINK3D.Ortak VizitLink3D.Ortak
Rename-Item VIZITLINK3D.Testler VizitLink3D.Testler
Rename-Item VIZITLINK3D.UI VizitLink3D.UI
Rename-Item VIZITLINK3D.slnx VizitLink3D.slnx
```

---

## Aşama 3: .csproj Dosyalarını Güncelle

### VizitLink3D.slnx
```xml
<Project Path="VizitLink3D.Ortak/VizitLink3D.Ortak.csproj" />
<Project Path="VizitLink3D.Api/VizitLink3D.Api.csproj" />
<Project Path="VizitLink3D.Testler/VizitLink3D.Testler.csproj" />
<Project Path="VizitLink3D.UI/VizitLink3D.UI.csproj" />
```

### VizitLink3D.Api.csproj
- `<ProjectReference Include="..\VizitLink3D.Ortak\VizitLink3D.Ortak.csproj" />`

### VizitLink3D.UI.csproj
- `<ProjectReference Include="..\VizitLink3D.Ortak\VizitLink3D.Ortak.csproj" />`

### VizitLink3D.Testler.csproj
- `<ProjectReference Include="..\VizitLink3D.Api\VizitLink3D.Api.csproj" />`

### TestJson.csproj
- `<ProjectReference Include="..\VizitLink3D.Ortak\VizitLink3D.Ortak.csproj" />`

---

## Aşama 4: C# Namespace Değişiklikleri (Toplu)

**Tüm .cs dosyalarında** şu replace'ler uygulanır:

| Eski | Yeni |
|---|---|
| `namespace VIZITLINK3D.Api` | `namespace VizitLink3D.Api` |
| `namespace VIZITLINK3D.Ortak` | `namespace VizitLink3D.Ortak` |
| `namespace VIZITLINK3D.Testler` | `namespace VizitLink3D.Testler` |
| `namespace VIZITLINK3D.UI` | `namespace VizitLink3D.UI` |
| `using VIZITLINK3D.Api` | `using VizitLink3D.Api` |
| `using VIZITLINK3D.Ortak` | `using VizitLink3D.Ortak` |
| `using VIZITLINK3D.UI` | `using VizitLink3D.UI` |
| `using VIZITLINK3D.Testler` | `using VizitLink3D.Testler` |

---

## Aşama 5: Razor Dosyalarında Değişiklikler

### _Imports.razor (11 satır)
```
@using VizitLink3D.UI
@using VizitLink3D.UI.Layout
@using VizitLink3D.UI.Pages
@using VizitLink3D.UI.Servisler
@using VizitLink3D.UI.Bilesenler
@using VizitLink3D.UI.Bilesenler.Admin
@using VizitLink3D.UI.Bilesenler.Anasayfa
@using VizitLink3D.UI.Bilesenler.Urunler
@using VizitLink3D.UI.Models
@using VizitLink3D.Ortak.Modeller
@using VizitLink3D.Ortak.Modeller.Urunler
```

### Tüm .razor dosyalarında
- `@inject VIZITLINK3D.UI.Servisler.DilServisi` → `@inject VizitLink3D.UI.Servisler.DilServisi`
- `<VIZITLINK3D.UI.Bilesenler.` → `<VizitLink3D.UI.Bilesenler.`
- `@layout VIZITLINK3D.UI.Layout.` → `@layout VizitLink3D.UI.Layout.`

### VIZITLINK3DDuzen.razor → VizitLink3DDuzen.razor
- Dosya adı değişikliği
- İçerik: `VIZITLINK3D` string'leri → `vizitlink3d`
- `<VIZITLINK3D.UI.Bilesenler.CanliSohbetArayuzu />` → `<VizitLink3D.UI.Bilesenler.CanliSohbetArayuzu />`

---

## Aşama 6: VIZITLINK3DDbContext Değişikliği

- Dosya adı: `VIZITLINK3DDbContext.cs` → `VizitLink3DDbContext.cs`
- Sınıf adı: `VIZITLINK3DDbContext` → `VizitLink3DDbContext`
- Tüm DI kayıtları, migration'lar ve kullanımlar güncellenir

---

## Aşama 7: Yapılandırma Dosyaları

### appsettings.json
```json
{
  "VeriTabani": { "Yol": "vizitlink3d.db" },
  "Jwt": {
    "Anahtar": "VizitLink3DGizliAnahtar2024!Guvenliyir#XYZ987",
    "Yayinci": "VizitLink3D.Api",
    "Izleyici": "VizitLink3D.UI"
  },
  "LisansAyarlari": {
    "GizliAnahtar": "VIZITLINK3D_HMAC_2026_SECRET_KEY_min_32char"
  }
}
```

### docker-compose.yml
- Servis adları: `vizitlink3d-api`, `vizitlink3d-ui`
- Container adları: `vizitlink3d-api`, `vizitlink3d-ui`
- Dockerfile yolları: `VizitLink3D.Api/Dockerfile`, `VizitLink3D.UI/Dockerfile`
- Env: `VIZITLINK3D_JWT_KEY`
- DB yolu: `vizitlink3d_v2.db`
- Volume: `vizitlink3d_data_v2`

### Program.cs (Api)
- Log yolu: `logs/vizitlink3d-.log`
- DB yolu: `vizitlink3d.db`
- Env: `VIZITLINK3D_JWT_KEY`

---

## Aşama 8: API Route'lar

Tüm controller'larda:
```
[Route("api/VIZITLINK3D/...")] → [Route("api/vizitlink3d/...")]
```

---

## Aşama 9: Hardcoded String'ler

| Konum | Eski | Yeni |
|---|---|---|
| TohumVerisi.cs | `Slug == "VIZITLINK3D"` | `Slug == "vizitlink3d"` |
| MenuKontrolcu.cs | `"VIZITLINK3D"` | `"vizitlink3d"` |
| LisansKontrolcu.cs | `"VIZITLINK3D"` | `"vizitlink3d"` |
| Razor dosyaları | `"VIZITLINK3D"` | `"vizitlink3d"` |
| JS localStorage | `VIZITLINK3D_token` | `vizitlink3d_token` |
| JS localStorage | `VIZITLINK3Ddil` | `vizitlink3dil` |
| JS localStorage | `VIZITLINK3D_admin_tema` | `vizitlink3d_admin_tema` |
| JS localStorage | `VIZITLINK3DAnimasyon` | `vizitlink3dAnimasyon` |

---

## Aşama 10: CSS Dosyaları

1. `VIZITLINK3D.css` → `vizitlink3d.css` (3 kopya: UI wwwroot, UI publish, Api wwwroot)
2. `tokens.css` içi: `@import './moduller/VIZITLINK3D.css'` → `@import './moduller/vizitlink3d.css'`
3. CSS sınıfları: `desa-*` → `vizit-*` (tüm dosyalarda ~200+ sınıf)
4. CSS değişkenleri: `--desa-*` → `--vizit-*` (tüm dosyalarda)

---

## Aşama 11: Statik Dosyalar / Görseller

- `VIZITLINK3D-logo.svg` → `vizitlink3d-logo.svg`
- `VIZITLINK3D-logo-light.svg` → `vizitlink3d-logo-light.svg`
- `VIZITLINK3D-icon.svg` → `vizitlink3d-icon.svg`
- `VIZITLINK3D_default.png` → `vizitlink3d_default.png`
- PDF kataloglar: `VIZITLINK3D-kapi-2026.pdf` → `vizitlink3d-kapi-2026.pdf`

---

## Aşama 12: .gitignore ve DB Dosyaları

- `VIZITLINK3D.db` → `vizitlink3d.db` (gitignore'da güncelle)
- `VIZITLINK3D_corrupt.db` → temizle

---

## Aşama 13: Dockerfile Güncellemeleri

### VizitLink3D.Api/Dockerfile
```dockerfile
COPY VizitLink3D.Api/VizitLink3D.Api.csproj ...
RUN dotnet restore VizitLink3D.Api/VizitLink3D.Api.csproj
...
CMD ["dotnet", "VizitLink3D.Api.dll"]
```

### VizitLink3D.UI/Dockerfile
```dockerfile
COPY VizitLink3D.UI/VizitLink3D.UI.csproj ...
RUN dotnet restore VizitLink3D.UI/VizitLink3D.UI.csproj
...
CMD ["dotnet", "VizitLink3D.UI.dll"]
```

---

## Aşama 14: entrypoint.sh

```bash
dotnet VizitLink3D.Api.dll --urls "http://+:8080"
```

---

## Aşama 15: 00_PROJE_BILGISI.md Güncellemesi

```yaml
proje_adi: "VizitLink3D"
firma_adi: "3DVizitLink A.Ş."
url_birincil: "3dvizitlink.com.tr"
...
```

---

## Uygulama Sırası

1. **Önce** tüm dosyaları oku (mevcut hali)
2. **DB yedeği al**
3. Aşama 1-2: Klasör temizliği ve yeniden adlandırma
4. Aşama 3-6: .csproj, namespace, Razor, DbContext
5. Aşama 7-9: Config, API route, hardcoded string
6. Aşama 10-15: CSS, statik dosyalar, Docker
7. **Derleme testi** (`dotnet build`)
8. **Çalışma testi** (API + UI başlat)

---

## Riskler

1. **Migration'lar** — Mevcut migration'larda `VIZITLINK3DDbContext` referansı var, yeni migration'larda `VizitLink3DDbContext` kullanılır
2. **CSS kırılması** — `desa-*` sınıf adları 200+ yerde, toplu replace dikkatli yapılmalı
3. **DB dosyası** — Mevcut `VIZITLINK3D.db` adı değişir, eski DB korunmalı
4. **Docker cache** — Eski image'lar temizlenmeli

---

*Bu plan onaylandıktan sonra scriptlerle uygulanacaktır.*
