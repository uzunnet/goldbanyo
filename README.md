# Gold Banyo — Banyo Dolabı Kurumsal Web Platformu

**.NET 10 + Blazor WASM + MudBlazor + SQLite** kurumsal web platformu.

## Hızlı Başlangıç

```bash
# Derle
dotnet build

# API çalıştır (port 5115)
cd VizitLink3D.Api && dotnet run

# UI çalıştır (port 5113)
cd VizitLink3D.UI && dotnet run

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
| `VizitLink3D.UI/` | Blazor WebAssembly (port 5113) |
| `VizitLink3D.Ortak/` | Paylaşılan modeller |
| `VizitLink3D.Testler/` | xUnit test projesi |

## Teknoloji

- .NET 10
- Blazor WebAssembly
- MudBlazor
- Entity Framework Core (SQLite)
- Three.js (3D ürün görüntüleyici)
- SignalR (canlı sohbet + bildirim + AI streaming)
- FluentValidation
- Serilog

## Kurallar

Tüm geliştirme kuralları için: [AGENTS.md](AGENTS.md) ve [AjanKurallari/](AjanKurallari/)
