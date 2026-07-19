# AI ANAYASA KİLİDİ — VIZITLINK3D

> **Kaynak:** KURALLAR.md (Vizitlink v11.0 adaptasyonu)
> **Geçerlilik:** DEĞİŞTİRİLEMEZ. Her AI asistanı uymak zorundadır.

---

## 🔴 KIRMIZI ÇİZGİLER (KESİN YASAKLAR)

1. **Python (*.py) veya dış terminal botları KESİNLİKLE YASAKTIR.** Bu proje %100 C# ve .NET 10 ile çalışır.
2. **KURALLAR.md okunmadan TEK SATIR KOD YAZILAMAZ.**
3. **MudBlazor dışında UI kütüphanesi KULLANILAMAZ.** (Radzen istisnası: sadece ağır DataGrid/grafik için)
4. **Hardcoded Türkçe/İngilizce metin Razor'da YASAKTIR.** `DilServisi.T("anahtar", "varsayilan")` kullanılır.
5. **Try-catch kontrolcüde YASAKTIR.** HataYonetimiMiddleware tüm hataları yakalar.
6. **Veritabanı sütun adında Türkçe karakter (İ, Ğ, Ş, Ç, Ö, Ü) YASAKTIR.** Ş→S, İ→I, Ğ→G dönüşümü zorunlu.
7. **Sadece EF Core Code-First Migration ile DB değişikliği yapılır.** Elle SQL YASAK.
8. **Kod tekrarı (DRY ihlali) SIFIR TOLERANS.** Aynı mantık iki yerde yazılamaz.
9. **`.razor` içinde `<style>` etiketi YASAKTIR.** CSS sadece `wwwroot/css/sistem/` altında.
10. **Harici kütüphane doğrudan çağrılamaz.** Türkçe Wrapper servisi zorunludur.

---

## 🟡 ZORUNLULUKLAR (HER ZAMAN)

- [ ] Tüm değişken/sınıf/dosya adları Türkçe (framework keyword'leri hariç)
- [ ] `Cevap<T>` dönüş standardı tüm endpoint'lerde
- [ ] FluentValidation her DTO için doğrulayıcı sınıfı
- [ ] Her dosya maksimum 1500 satır (500-800 ideal)
- [ ] `@code` bloğu yerine Partial Class (.razor.cs)
- [ ] Her değişiklik öncesi DB yedeği al
- [ ] `dotnet build` hatasız olmadan commit YAPILAMAZ
- [ ] Commit mesajları Türkçe ve açıklayıcı
- [ ] `[JsonIgnore]` korumalı: SifreHash, PinHash, DesenHash, WebAuthnPublicKey, SifreSifirlamaToken

---

## 🟢 İZİN VERİLENLER

- C# keywords: `public`, `private`, `class`, `async`, `await`, `override`, `static`
- Blazor/Razor keywords: `@page`, `@inject`, `@code` (sadece partial class'ta), `OnInitializedAsync`
- HTML/CSS/SQL standart etiketleri: `div`, `SELECT`, `FROM`
- MudBlazor bileşen etiket adları İngilizce kalır: `<MudButton>`, `<MudDataGrid>`
- Framework zorunlu dosyaları: `Program.cs`, `appsettings.json`, `launchSettings.json`

---

## 🚨 İHLAL TESPİT PROTOKOLÜ

AI asistanı şu durumlarda **hemen durur ve Ustam'ı uyarır:**
1. Python veya dış bot kodu yazılması istendiğinde
2. Anayasa okunmadan kod yazma talebi geldiğinde
3. MudBlazor dışında UI kütüphanesi ekleneceğinde
4. Doğrudan SQL veya elle tablo değişikliği yapılacağında
5. KURALLAR.md'yi kısaltma/silme/değiştirme teşebbüsünde

---

*Bu dosya silinemez, değiştirilemez. Sadece Ustam'ın onayı ile güncellenir.*
*Oluşturma: 2026-05-14 | VIZITLINK3D Projesi*
