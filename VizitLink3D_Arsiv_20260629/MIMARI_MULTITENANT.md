# VIZITLINK3D — Multi-Tenant Mimari Planı

> **Amaç:** Tek kod tabanı, 10+ firmaya satılabilir sistem.
> **Her firma:** kendi domain'i, logosu, teması, SEO'su, içeriği ile bağımsız çalışır.
> **Admin:** firmalar arası geçiş yapabilir, her şey panelden yönetilir.

---

## 1. Tenant Çözümleme (Domain Bazlı)

```
istek → FirmaCozumlemeMiddleware → HttpContext.Items["FirmaId"] → tüm sistem
```

**Çalışma mantığı:**
1. Gelen domain'e bak (örn: `firma1.com.tr`, `localhost:5013`)
2. DB'de `Firma.Domain` veya `Firma.YedekDomain` ile eşleştir
3. Bulursa `FirmaId`'yi `HttpContext.Items`'e yaz
4. Bulamazsa varsayılan firmayı (VIZITLINK3D) ata
5. Geliştirme ortamında `?firma=slug` query param ile override

**Dosya:** `VIZITLINK3D.Api/AraYazilimlar/FirmaCozumlemeMiddleware.cs`

---

## 2. KiraciServisi (Servis Katmanı)

```csharp
public class KiraciServisi(IHttpContextAccessor hca)
{
    public int? MevcutFirmaId => hca.HttpContext?.Items["FirmaId"] as int?;
    
    // Domain bilgisi (admin panelde "hangi firmadayım" göstermek için)
    public string? MevcutDomain => hca.HttpContext?.Items["FirmaDomain"] as string;
}
```

**Dosya:** `VIZITLINK3D.Api/Servisler/KiraciServisi.cs`

---

## 3. Global Query Filter (Veri İzolasyonu)

Tüm entity'lere `FirmaId` bazlı filtre:

```csharp
// OnModelCreating'de
b.Entity<Urun>().HasQueryFilter(u => !u.SilindiMi && u.FirmaId == _kiraciServisi.MevcutFirmaId);
```

**Hangi entity'ler filtrelenir:** Urun, Slayt, Referans, Blog, Proje, MenuOgesi, SayfaIcerigi, vb.
**Hangi entity'ler filtrelenmez:** Firma, Kullanici, Dil, RalRengi (ortak referans verileri)

---

## 4. Tema Yönetimi (Admin Panel)

Her firma için admin panelden değiştirilebilir:

| Ayar | Nerede | Nasıl |
|------|--------|-------|
| Logo | `Firma.LogoUrl` | Medya seçici |
| Favicon | `Firma.FaviconUrl` | Medya seçici |
| Ana Renk | `tasarim/DESIGN_{firmaId}.md` | Renk paleti seçici |
| Font | `tasarim/DESIGN_{firmaId}.md` | Font seçici |
| Admin tema | `tasarim/DESIGN_{firmaId}.md` | Açık/Koyu toggle |

**Akış:** Admin panelde değişiklik → API kaydeder → StitchTemaServisi tokens.css üretir → SignalR broadcast → tüm açık tarayıcılar anında güncellenir.

---

## 5. SEO Yönetimi (Admin Panel)

| Ayar | DB Alanı |
|------|----------|
| Site başlığı | `SayfaIcerigi.Bolum="seo", Anahtar="Title"` |
| Meta description | `SayfaIcerigi.Bolum="seo", Anahtar="Description"` |
| OG image | `SayfaIcerigi.Bolum="seo", Anahtar="OgImage"` |
| Google Analytics | `SayfaIcerigi.Bolum="seo", Anahtar="Gtag"` |
| Schema JSON-LD | `SayfaIcerigi.Bolum="seo", Anahtar="Schema"` |

---

## 6. Admin Panel — Firma Seçici

SuperAdmin kullanıcıları için üst barda firma dropdown'ı:

```
[VIZITLINK3D A.Ş. ▼]
 ├─ VIZITLINK3D A.Ş.
 ├─ Firma 2 Ltd.
 └─ Firma 3 Sanayi
```

Seçilen firmaya göre tüm admin panel içeriği (ürünler, sayfalar, ayarlar) o firmaya scope'lanır.

---

## 7. Public Site — Firma Bazlı Render

Public sayfalar (`VIZITLINK3DDuzen.razor`) firma bilgilerini API'den çeker:

```csharp
// VIZITLINK3DDuzen.razor.cs OnInitializedAsync
var firma = await Api.GetAsync<Firma>($"api/firma/mevcut");
// firma.LogoUrl, firma.Ad, firma.Telefon1, ...
```

---

## 8. Uygulama Sırası

| # | İş | Süre |
|---|-----|------|
| 1 | FirmaCozumlemeMiddleware | 15dk |
| 2 | KiraciServisi + DI kayıt | 10dk |
| 3 | Global query filter (tüm entity) | 20dk |
| 4 | Firma API endpoint'leri (logo, bilgi, tema) | 15dk |
| 5 | Tema paneli (admin UI) | 30dk |
| 6 | SEO paneli (admin UI) | 20dk |
| 7 | Public site firma bazlı render | 20dk |
| 8 | SuperAdmin firma seçici | 15dk |
| 9 | Test (multi-tenant senaryolar) | 15dk |
| 10 | DESIGN.md → tokens.css otomasyonu | 10dk |

---

## 9. Veritabanı Değişiklikleri

Yeni tablo veya migration **gerekmez**. Mevcut `Firma` entity'si yeterli.
Sadece global query filter değişikliği (migration gerektirmez, runtime'da uygulanır).

---

## 10. Domain Yapılandırması

```
Geliştirme:
  localhost:5013?firma=VIZITLINK3D → VIZITLINK3D
  localhost:5013?firma=firma2   → Firma 2

Üretim:
  VIZITLINK3D.com.tr               → VIZITLINK3D (Domain eşleşmesi)
  firma2.com.tr                 → Firma 2
  admin.sistem.com               → SuperAdmin paneli
```

---

*Hazırlayan: planlama oturumu · Tarih: 2026-05-17*
