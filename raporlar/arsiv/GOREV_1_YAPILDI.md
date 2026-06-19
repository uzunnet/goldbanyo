# DesaDoor — Tamamlanan Görevler

> **Anayasa:** KURALLAR.md (Vizitlink v11.0 adaptasyonu)
> **Düzelme Planı:** DUZELT.md

---

## 2026-05-14

### Paket 0 — Anayasa ve Görev Sistemi Kurulumu
- [x] KURALLAR.md mevcut, Vizitlink kuralları eklendi (§K1-K8)
- [x] DUZELT.md oluşturuldu (araştırma: Haiku 4.5)
- [x] GOREV_1_YAPILDI.md güncellendi
- [x] GOREV_2_YAPILACAK.md güncellendi (DUZELT.md paketleri aktarıldı)
- [x] .agent/ klasörü oluşturuldu (AI_ANAYASA_KILIDI.md + AI_KOD_YAZMA_KONTROL.md)
- [x] Yedekler/ klasörü oluşturuldu, DB yedeği alındı
- [x] .gitignore oluşturuldu
- [x] dotnet build hatasız geçti
- [x] i18n tr.json + en.json anahtar uyumu sağlandı
- [x] DilServisi tanımlandı (UI tarafında mevcut)
- [x] DESEPLAN.md oluşturuldu (kapsamlı durum analizi — 40 tablo, 56 razor, 9 glb)
- [x] Proje derinlemesine keşfedildi (tüm .cs, .razor, .glb, migration, servisler)
- [x] MIMARI_VIZYON.md oluşturuldu (cinematic front-end + admin vizyonu)
- [x] PLAN_MEDYA_VE_AI.md oluşturuldu (Medya Havuzu + AI Altyapı planı)

### Keşif Bulguları (14.05.2026)
- Mevcut 40 tablo (DUZELT.md'de tahmin edilen 7 tablonun çok üstünde)
- 56 .razor dosyası (13 ziyaretçi + 29 admin + 9 bileşen + 4 layout)
- 18 kontrolcü modüler klasörlerde
- UcBoyutServisi (246 satır) + uc-boyut-motoru.js (503 satır) tam işlevsel
- 9 .glb model dosyası mevcut (dağınık)
- Eksikler: 13 admin sayfası code-behind'sız, 3 mock kontrolcü, boş servis klasörleri

---

## 2026-04-24 (Önceki)

- [x] Hakkimizda.razor sayfasındaki 401 Unauthorized hatası çözüldü
- [x] baslat.ps1 ile sistem ayağa kaldırıldı (API:5015, UI:5013)
- [x] .NET 10 + Blazor WASM + MudBlazor 9.4 altyapısı kurulu
- [x] SQLite desadoor.db + 7 temel tablo oluşturuldu
- [x] Three.js 3D motor scripti entegre edildi
- [x] SignalR SohbetHub kuruldu
- [x] JWT + BCrypt auth altyapısı hazır
- [x] Admin paneli 14 sayfa iskeleti hazır
