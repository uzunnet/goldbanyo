# AI KOD YAZMA KONTROL LİSTESİ — VIZITLINK3D

> **Kullanım:** Her kod yazma işleminden ÖNCE bu liste kontrol edilir.
> **Kaynak:** KURALLAR.md + .agent/AI_ANAYASA_KILIDI.md

---

## 📋 KOD YAZMADAN ÖNCE KONTROL (ZORUNLU)

### A. Anayasa Uyumu
- [ ] KURALLAR.md ilgili bölümleri okundu mu?
- [ ] AI_ANAYASA_KILIDI.md kırmızı çizgiler kontrol edildi mi?
- [ ] GOREV_2_YAPILACAK.md'deki aktif görev bu mu?

### B. İsimlendirme
- [ ] Sınıf adı Türkçe PascalCase mi? (`KapiServisi` ✅, `DoorService` ❌)
- [ ] Metot adı Fiil + Türkçe PascalCase mi? (`KapiEkleAsync` ✅, `AddDoorAsync` ❌)
- [ ] Değişken adı camelCase Türkçe mi? (`_kapiListesi` ✅, `_doorList` ❌)
- [ ] Veritabanı sütun adında İ,Ğ,Ş,Ç,Ö,Ü karakteri yok mu? (`SifreHash` ✅, `ŞifreHash` ❌)

### C. Mimari
- [ ] Partial Class kullanılıyor mu (`.razor` + `.razor.cs`)?
- [ ] `@code` bloğu yok mu?
- [ ] Harici kütüphane wrapper üzerinden mi çağrılıyor?
- [ ] Dosya 1500 satırı geçmiyor mu?
- [ ] Kod tekrarı (DRY ihlali) var mı? → varsa refactor et

### D. API
- [ ] Endpoint `Cevap<T>` dönüyor mu?
- [ ] Kontrolcüde `try-catch` yok mu?
- [ ] Route Türkçe mi? (`api/kapi-modelleri` ✅, `api/door-models` ❌)
- [ ] `[Authorize]` veya `[AllowAnonymous]` doğru mu?

### E. UI (Blazor/Razor)
- [ ] `.razor` içinde `<style>` etiketi yok mu?
- [ ] Hardcoded metin yok mu? (`@DilServisi.T(...)` kullanıldı mı)
- [ ] `@inject DilServisi DilServisi` her sayfada var mı?
- [ ] CSS değişkenleri `var(--degisken-adi)` ile mi kullanılıyor?
- [ ] MudBlazor bileşen adları İngilizce, C# olayları Türkçe mi?

### F. Güvenlik
- [ ] Hassas alanlar `[JsonIgnore]` ile korunuyor mu?
- [ ] JWT anahtarı/env değişkeni hardcoded değil mi?
- [ ] Log'a şifre/token yazılmıyor mu?

### G. Veritabanı
- [ ] EF Core Code-First Migration kullanılıyor mu?
- [ ] Elle SQL veya tablo değişikliği yok mu?
- [ ] Migration adı Türkçe açıklayıcı mı? (`KapiModeliTablosuEklendi`)

---

## 🛠️ KOD YAZDIKTAN SONRA KONTROL (ZORUNLU)

- [ ] `dotnet build` hatasız geçti mi?
- [ ] DB yedeği alındı mı?
- [ ] GOREV_1_YAPILDI.md güncellendi mi?
- [ ] Commit mesajı Türkçe ve açıklayıcı mı?

---

## ⚠️ UYARI FORMATI

Bir kural ihlali tespit edildiğinde şu formatta uyarı verilir:

```
⚠️ YAPAY ZEKA UYARISI: Ustam, [ihlal açıklaması].
   Kural: [KURALLAR.md ilgili bölüm]
   İşlem: [ne yapılacağı]
```

---

*Oluşturma: 2026-05-14 | VIZITLINK3D Projesi*
