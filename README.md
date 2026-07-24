# Gold Banyo — Banyo Dolabı Kurumsal Web Platformu

**.NET 10 + Blazor WASM + MudBlazor + SQLite** kurumsal web platformu.

## Hızlı Başlangıç

```bash
# Derle
dotnet build

# API çalıştır (port 5115)
cd VizitLink3D.Api && dotnet run

# Admin UI çalıştır (port 5113)
cd VizitLink3D.UI && dotnet run

# 3D Konfigüratör Runtime çalıştır (port 5114)
cd VizitLink3D.Konfigurator && dotnet run

# Veya tek komutla
.\BASLA.bat
```

## Test

```bash
# Tüm testleri çalıştır
dotnet test VizitLink3D.Testler/VizitLink3D.Testler.csproj
```

## Migration

```bash
# Yeni migration oluştur
dotnet ef migrations add MigrationAdi --project VizitLink3D.Api/VizitLink3D.Api.csproj

# Veritabanını güncelle
dotnet ef database update --project VizitLink3D.Api/VizitLink3D.Api.csproj
```

## Proje Yapısı

| Klasör | Açıklama |
|--------|----------|
| `VizitLink3D.Api/` | ASP.NET Core Web API (port 5115) |
| `VizitLink3D.UI/` | Blazor WASM Admin Paneli (port 5113) |
| `VizitLink3D.Konfigurator/` | 3D Konfigüratör Runtime (port 5114) |
| `VizitLink3D.Ortak/` | Paylaşılan modeller |
| `VizitLink3D.Testler/` | xUnit test projesi |

### Local Port Haritası

| Port | Servis |
|------|--------|
| 5113 | Admin UI (Blazor WASM) |
| 5114 | 3D Konfigüratör Runtime (Three.js) |
| 5115 | API |

### Admin Studio Geliştirme Bridge (Geçici)

> ⚠ **GEÇİCİ ÇÖZÜM** — Sadece development ortamında çalışır. Production'da `/admin` 404 döner.

`VizitLink3D.Konfigurator` (port 5114) üzerinde `/admin` endpoint'i, port 5113'teki
Admin Studio'yu (`/admin/konfigurator-studio`) tam ekran iframe ile embed eder.

- **URL:** `http://localhost:5114/admin`
- **CSP:** Development'ta `frame-src 'self' http://localhost:5113` eklenir; production self kalır.
- **Sandbox:** `allow-scripts allow-same-origin allow-forms allow-popups allow-modals` (Blazor WASM + auth çalışır).
- **İlerleme:** Gerçek ayrı admin host taşıması (bağımsız port/admin domain) sonraki pakette yapılacaktır.

## Teknoloji

- .NET 10
- Blazor WebAssembly
- MudBlazor
- Entity Framework Core (SQLite)
- Three.js (3D ürün görüntüleyici)
- SignalR (canlı sohbet + bildirim + AI streaming)
- FluentValidation
- Serilog

## 3D Konfigüratör Runtime (VizitLink3D.Konfigurator)

Bağımsız minimal ASP.NET Core host. `VizitLink3D.UI/wwwroot/goldbanyo/` klasörünü
root `/` olarak servis eder. Admin UI'den tamamen bağımsızdır; `/goldbanyo/`
alt yol Blazor fallback sorununu ortadan kaldırır.

> **Geçiş notu:** Bu host şu anda `VizitLink3D.UI/wwwroot/goldbanyo/` klasörünü
> referans alır. Gelecekte 3D asset'ler (model, doku, HDR) medya havuzuna
> (`wwwroot/medya/3d-modeller/`) taşındığında PhysicalFileProvider yolu
> güncellenecektir.

## Kurallar

Tüm geliştirme kuralları için: [AGENTS.md](AGENTS.md) ve [AjanKurallari/](AjanKurallari/)
