# DesaDoor — Premium Kapı ve Mobilya Kapak Sistemleri

**.NET 10 + Blazor WASM + MudBlazor + SQLite** kurumsal web platformu.

## Hızlı Başlangıç

```bash
# Derle
dotnet build Desadoor.slnx

# API çalıştır (port 5015)
cd Desadoor.Api && dotnet run

# UI çalıştır (port 5013)
cd Desadoor.UI && dotnet run

# Veya tek komutla
.\BASLA.bat
```

## Test

```bash
# Tüm testleri çalıştır (170+ test)
dotnet test Desadoor.Testler/Desadoor.Testler.csproj

# Belirli test dosyası
dotnet test Desadoor.Testler/Desadoor.Testler.csproj --filter "FullyQualifiedName~ApiTemelTestler"
```

## Migration

```bash
# Yeni migration oluştur
dotnet ef migrations add MigrationAdi --project Desadoor.Api/Desadoor.Api.csproj

# Veritabanını güncelle
dotnet ef database update --project Desadoor.Api/Desadoor.Api.csproj
```

## Proje Yapısı

| Klasör | Açıklama |
|--------|----------|
| `Desadoor.Api/` | ASP.NET Core Web API (port 5015) |
| `Desadoor.UI/` | Blazor WebAssembly (port 5013) |
| `Desadoor.Ortak/` | Paylaşılan modeller |
| `Desadoor.Testler/` | xUnit test projesi (180 test) |

## Paketler

| Paket | % |
|-------|---|
| Veritabanı Şeması | 100% ✅ |
| Backend Modüler Yapı | 92% |
| Frontend Sayfalar | 95% |
| Admin Paneli | 92% |
| 3D Görsel Sistem | 90% |
| Çoklu Dil & İçerik | 100% ✅ |
| Test, Güvenlik & Deploy | 82% |
| Medya Havuzu | 75% |
| AI Asistan Altyapı | 85% |

## Teknoloji

- .NET 10
- Blazor WebAssembly
- MudBlazor 9.4
- Entity Framework Core (SQLite)
- Three.js (3D ürün görüntüleyici)
- SignalR (canlı sohbet + bildirim + AI streaming)
- FluentValidation
- Serilog
- QuestPDF
- SixLabors.ImageSharp

## Anayasa

Tüm geliştirme kuralları için: [KURALLAR.md](KURALLAR.md)
Detaylı yol haritası: [DESEPLAN.md](DESEPLAN.md)
