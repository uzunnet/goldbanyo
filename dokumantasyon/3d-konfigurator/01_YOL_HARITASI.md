# 3D Konfigüratör — Yol Haritası

> **Proje:** Gold Banyo / VizitLink3D
> **Tarih:** 20 Temmuz 2026
> **Durum:** Onay Bekliyor

---

## Genel Bakış

3D Konfigüratör projesi **6 aşamada** ve **3milestone**'ta planlanmıştır. Her aşama kendi içinde bağımsız test edilebilir ve deploy edilebilir.

```
Aşama 1 → Aşama 2 → Aşama 3 → Aşama 4 → Aşama 5 → Aşama 6
[Core]    [Admin]   [Public]   [Embed]   [AI/MCP]  [Scale]
  M1 ──────────── M2 ──────────── M3 ──────── Final
```

---

## Aşama 1 — Core Motor (Shared Library)

**Süre:** 3 hafta
**Milestone:** M1 — Motor Kütüphanesi Hazır

### Amaç
`vizitlink3d.core` shared kütüphanesini oluşturmak; Three.js motorunu sarmalayarak Admin, Public ve Embed ortak altyapısını kurmak.

### Görevler

| # | Görev | Sorumlu | Tahmini |
|---|-------|---------|---------|
| 1.1 | `vizitlink3d.core` proje yapısını oluştur (NuGet + npm dual publish) | Backend + Frontend | 2 gün |
| 1.2 | Three.js sahne yöneticisi (Scene, Camera, Renderer sarmalayıcı) | Frontend | 3 gün |
| 1.3 | GLTF/GLB loader (Draco + meshopt desteği) | Frontend | 3 gün |
| 1.4 | Raycaster parçalı seçim sistemi (hover, click, multi-select) | Frontend | 3 gün |
| 1.5 | Hareket sistemi temeli (HareketTuru enum, HareketParametreleri) | Frontend | 3 gün |
| 1.6 | Parça metadata okuma/yazma servisi | Backend | 2 gün |
| 1.7 | Tenant izolasyonu middleware (model yolu, DB sorgu) | Backend | 2 gün |
| 1.8 | Birim testleri (≥10 test) | Her ikisi | 2 gün |
| 1.9 | Performans benchmark (FPS, yükleme süresi) | Frontend | 1 gün |

### Kabul Kriterleri
- [ ] Shared core NuGet ve npm olarak yayınlanabilir
- [ ] GLB modeli 3 saniyeden kısa sürede yüklenir (10MB altında)
- [ ] Raycaster ile parça seçimi 16ms altında çalışır (60 FPS)
- [ ] Hareket sistemi en az 4 hareket turunu destekler
- [ ] Tenant izolasyonu tüm model yüklemelerinde doğrulanır
- [ ] ≥10 birim testi passed

---

## Aşama 2 — Admin Studio

**Süre:** 4 hafta
**Milestone:** M2 — Admin Studio Kullanıma Hazır

### Amaç
Admin panelinde 3D model yükleme, metadata düzenleme, sahne oluşturma ve test etme imkanı sunmak.

### Görevler

| # | Görev | Sorumlu | Tahmini |
|---|-------|---------|---------|
| 2.1 | Admin Studio sayfa yapısı (`/admin/studio`) | Frontend | 2 gün |
| 2.2 | Model yükleme bileşeni (drag-drop, GLB format, max 30MB) | Frontend | 3 gün |
| 2.3 | 3D viewport (sahne, kamera, ışık kontrolleri) | Frontend | 3 gün |
| 2.4 | Parça seçimi + metadata paneli (form editörü) | Frontend | 4 gün |
| 2.5 | Hareket parametreleri editörü (pivot noktası, eksen, limit) | Frontend | 3 gün |
| 2.6 | Renk/malzeme editörü (PBR parametreleri) | Frontend | 3 gün |
| 2.7 | Screenshot/video alma | Frontend | 2 gün |
| 2.8 | Admin API endpoint'leri (CRUD + metadata) | Backend | 4 gün |
| 2.9 | Model versiyonlama (aynı ürüne farklı varyant ekleme) | Backend | 2 gün |
| 2.10 | Entegrasyon testleri (≥15 test) | Her ikisi | 3 gün |

### Kabul Kriterleri
- [ ] Admin kullanıcı GLB dosyasını sürükleyip bırakabilir
- [ ] Her parçanın metadata bilgisi form ile düzenlenebilir
- [ ] Hareket parametreleri görsel olarak ayarlanabilir (pivot noktası sürükleme)
- [ ] Değişiklikler kaydedildiğinde DB ve dosya sistemi tutarlı olur
- [ ] Screenshot 1920x1080 çözünürlükte alınabilir
- [ ] ≥15 entegrasyon testi passed
- [ ] Admin Studio mobil responsive (en az tablet)

---

## Aşama 3 — Public Viewer

**Süre:** 3 hafta
**Milestone:** M2 ile birlikte

### Amaç
Ziyaretçilerin ürün sayfalarında 3D modeli interaktif olarak görüntülemesini sağlamak.

### Görevler

| # | Görev | Sorumlu | Tahmini |
|---|-------|---------|---------|
| 3.1 | Public Viewer bileşeni (`/urun/:slug/3d`) | Frontend | 2 gün |
| 3.2 | Optimizasyonlu model yükleme (LOD, progressive mesh) | Frontend | 3 gün |
| 3.3 | Touch/mouse etkileşim (orbit, zoom, pan) | Frontend | 2 gün |
| 3.4 | Parça bilgi tooltip'leri (hover → bilgi) | Frontend | 2 gün |
| 3.5 | Renk/malzeme değiştirici (seçili parçalar için) | Frontend | 3 gün |
| 3.6 | Paylaş butonu (screenshot + URL) | Frontend | 1 gün |
| 3.7 | CDN cache stratejisi (immutable headers, ETag) | Backend | 2 gün |
| 3.8 | Public API endpoint'leri (sadece okuma) | Backend | 2 gün |
| 3.9 | SEO: OpenGraph meta etiketleri (3D thumbnail) | Frontend | 1 gün |
| 3.10 | Performans testi (mobil 4G, orta segment telefon) | Her ikisi | 2 gün |

### Kabul Kriterleri
- [ ] Ziyaretçi modeli 3 saniyede yükler (4G bağlantıda)
- [ ] Touch etkileşimi mobilde akıcı (≥30 FPS orta segment)
- [ ] Parça hover bilgisi doğru metin gösterir
- [ ] Paylaş URL'si OpenGraph thumbnail içerir
- [ ] CDN cache hit oranı ≥%80
- [ ] Lighthouse performans skoru ≥80

---

## Aşama 4 — Embed API

**Süre:** 2 hafta
**Milestone:** M3 — Embed API Yayında

### Amaç
Üçüncü parti web sitelerinin 3D konfigüratörü `<iframe>` veya JavaScript SDK ile gömmesini sağlamak.

### Görevler

| # | Görev | Sorumlu | Tahmini |
|---|-------|---------|---------|
| 4.1 | Embed endpoint'i (`/embed/:urunId`) | Backend | 1 gün |
| 4.2 | Sandbox iframe (CSP kısıtlı, scroll yok) | Frontend | 2 gün |
| 4.3 | JavaScript SDK (`@vizitlink3d/embed`) | Frontend | 3 gün |
| 4.4 | PostMessage API (embed ↔ parent iletişim) | Frontend | 2 gün |
| 4.5 | Embed ayarları (boyut, tema, devre dışı bırakılan özellikler) | Frontend | 2 gün |
| 4.6 | Rate limiting (IP + tenant bazlı) | Backend | 1 gün |
| 4.7 | Embed istatistikleri (görüntülenme, etkileşim) | Backend | 1 gün |
| 4.8 | Dokümantasyon + örnek HTML | Her ikisi | 1 gün |

### Kabul Kriterleri
- [ ] `<iframe>` ile embed 5 satır kodla çalışır
- [ ] JavaScript SDK ≥3 farklı sitede test edilmiş
- [ ] CSP header'ı XSS ve clickjacking'e karşı korumalı
- [ ] Rate limit aşıldığında dostane hata mesajı döner
- [ ] Embed performansı bağımsız sayfa ile aynı FPS'i sağlar
- [ ] ≥5 test passed

---

## Aşama 5 — AI/MCP Entegrasyonu

**Süre:** 3 hafta
**Milestone:** M3 ile birlikte

### Amaç
AI asistanının 3D model üzerinde doğal dil ile işlem yapabilmesini sağlamak (MCP Protocol).

### Görevler

| # | Görev | Sorumlu | Tahmini |
|---|-------|---------|---------|
| 5.1 | MCP Server altyapısı (Model Context Protocol) | Backend | 3 gün |
| 5.2 | 3D model MCP tool'ları (parça bul, renk değiştir, konum ayarla) | Backend | 4 gün |
| 5.3 | AI asistan 3D bağlamı (mevcut konfigürasyonu anlama) | Backend | 3 gün |
| 5.4 | Admin paneline AI sohbet paneli ekleme | Frontend | 3 gün |
| 5.5 | Doğal dil → metadata dönüşümü (LLM orchestration) | Backend | 3 gün |
| 5.6 | Güvenlik: AI'ın yapabildiği/yapamayacağı işlemler listesi | Backend | 2 gün |
| 5.7 | Loglama ve audit trail (AI değişiklikleri) | Backend | 2 gün |
| 5.8 | Test senaryoları (≥10 test) | Her ikisi | 2 gün |

### Kabul Kriterleri
- [ ] "Kapıyı siyah yap" komutu kapının rengini değiştirir
- [ ] "Bu dolapta kaç çekmece var?" sorusuna doğru cevap verir
- [ ] AI'ın yapamayacağı işlemler (silme, fiyat değiştirme) engellenir
- [ ] Her AI değişikliği audit log'da kayıtlı
- [ ] MCP server 100.concurrent bağlantıyı kaldırır
- [ ] ≥10 test passed

---

## Aşama 6 — Ölçeklendirme ve Optimizasyon

**Süre:** 2 hafta
**Milestone:** Final — Üretim Hazır

### Amaç
Performans optimizasyonu, CDN ayarları, monitoring ve produccióna hazırlık.

### Görevler

| # | Görev | Sorumlu | Tahmini |
|---|-------|---------|---------|
| 6.1 | CDN yapılandırması (Cloudflare / Azure CDN) | DevOps | 2 gün |
| 6.2 | Model otomatik sıkıştırma pipeline'ı (glTF-Transform) | Backend | 2 gün |
| 6.3 | LOD otomatik oluşturma (high/medium/low) | Frontend | 2 gün |
| 6.4 | Monitoring (Sentry + Application Insights) | DevOps | 1 gün |
| 6.5 | Load test (1000 concurrent kullanıcı) | Her ikisi | 2 gün |
| 6.6 | Erişilebilirlik (WCAG 2.1 AA) | Frontend | 2 gün |
| 6.7 | Dokümantasyon güncelleme | Her ikisi | 1 gün |
| 6.8 | Production deploy checklist | DevOps | 1 gün |

### Kabul Kriterleri
- [ ] CDN global edge'den model yükleme ≤2 saniye
- [ ] 1000 concurrent kullanıcıda ≥30 FPS korunur
- [ ] Lighthouse erişilebilirlik skoru ≥90
- [ ] Sentry'de zero unhandled exception
- [ ] Deploy checklist'in tüm maddeleri tamamlandı

---

## Timeline Özeti

```
Hafta  1-3:  Aşama 1 — Core Motor           ████░░░░░░░░░░░░░░░░
Hafta  4-7:  Aşama 2 — Admin Studio         ░░░░████████░░░░░░░░
Hafta  5-7:  Aşama 3 — Public Viewer        ░░░░░░████░░░░░░░░░░
Hafta  8-9:  Aşama 4 — Embed API            ░░░░░░░░░░░░██░░░░░░
Hafta  8-10: Aşama 5 — AI/MCP               ░░░░░░░░░░░░████░░░░
Hafta 11-12: Aşama 6 — Ölçeklendirme        ░░░░░░░░░░░░░░░░████
                                        ▲M1         ▲M2    ▲M3  ▲Final
```

**Toplam Tahmini Süre:** 12 hafta (3 ay)
**Minimum MVP (M2):** 7 hafta

---

## Riskler ve Azaltma

| Risk | Olasılık | Etki | Azaltma |
|------|----------|------|---------|
| Three.js versiyon kırılması | Düşük | Yüksek | Shared core abstraction layer, pin version |
| Model dosya boyutu aşımı | Orta | Orta | Otomatik sıkıştırma, LOD, progressive mesh |
| Mobil performans düşüklüğü | Yüksek | Yüksek | Erken mobil test, low-poly yedek modeller |
| Tenant veri sızıntısı | Düşük | Kritik | Middleware katmanlı izolasyon, pentest |
| MCP throughput yetersizliği | Orta | Orta | Async processing, queue sistemi |

---

## Onay

- [ ] Ustam onayı
- [ ] Zaman çizelgesi revizyonu
- [ ] Kaynak tahsisi
