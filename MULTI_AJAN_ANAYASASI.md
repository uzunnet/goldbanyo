# EVRENSEL ÇOKLU AJAN (MULTI-AGENT) VE KESİN KREDİ KORUMA ANAYASASI

**Tarih:** Temmuz 2026  
**Durum:** ✅ KESIN KURAL - Tüm Projeler İçin Bağlayıcı  
**Yetki:** İNSAN DENETÇİ (Kullanıcı)  
**Dil:** TÜRKÇE (İngilizcesi Yasaklı)

---

⚠️ **UYARI - TÜM MODELLER OKUYUNUZ!**

🔴 **BU PROJE VE TÜM ALT PROJELERİMİZ TÜRKÇE DÜZENLENMİŞTİR**

- SADECE TÜRKÇE SOHBET YAPILIR
- İNGİLİZCE YAZMA YASAKLANMIŞTIR
- TÜM GÖRÜŞMELERDE TÜRKÇE ZORUNLUDUR
- MODELLER VE AJANLAR TÜRKÇE KONUŞUR

---

## 🎯 KESİN KREDİ KORUMA - TEMELİ KURAL

**KREDİLİ MODELLERIN (Grok 4.5, GPT-5.5, Claude Sonnet/Opus vs.) OTONOM KULLANIMI KESİNLİKLE YASAKLANMIŞTIR.**

- ✅ Kredisiz modeller OTOMATİK tetiklenebilir
- ❌ Kredili modeller YALNIZCA kullanıcı manuel seçerse devreye girebilir
- ❌ Arka plan ajanları krediyi tetikleyemez
- ❌ Bütçe yükseltme kesinlikle engellenmiştir

---

## 📋 UNLIMITED MODEL HAVUZU (Otonom Kullanım İzinli)

| Model | Katman | Maliyeti | Durum |
|-------|--------|----------|-------|
| **Llama4** | Doktor/Kriz | 0 (Unlimited) | ✅ Otonom |
| **Gemini 3 Flash** | Doktor/Medya | 0 (Unlimited) | ✅ Otonom |
| **Grok Code Fast** | Kodlama/QA | 0 (Unlimited) | ✅ Otonom |
| **GLM 4.7** | Kodlama fallback | 0 (Unlimited) | ✅ Otonom |
| **GPT-5 Mini** | QA/Arka plan | 0 (Unlimited) | ✅ Otonom |
| **RouteLLM** | Arka plan tarama | 0 (Unlimited) | ✅ Otonom |
| **Kimi K2** | Alternatif | 0 (Unlimited) | ✅ Otonom |

---

## ❌ YASAKLANAN MODELLER (Sadece Manuel Seçimle)

| Model | Neden | Tetikleme |
|-------|-------|-----------|
| Claude Haiku 4.5 (Reasoning) | Ücretli | ❌ Otonom YASAKLI |
| Claude Sonnet | Ücretli | ❌ Otonom YASAKLI |
| Claude Opus | Ücretli | ❌ Otonom YASAKLI |
| Grok 4.5 | Ücretli | ❌ Otonom YASAKLI |
| GPT-5.5 | Ücretli | ❌ Otonom YASAKLI |
| Deepseek V4 Flash | Ücretli | ❌ Otonom YASAKLI |

---

## 7'Lİ HİYERARŞİK İŞ AKIŞI

### 1️⃣ DOKTOR / MİMAR (Master Planner - Unlimited Havuz)

**Görev:** Her yeni görev emrinde projeyi analiz ederek iş haritası çıkar.

```
✅ KULLANABİLECEK MODELLER (OTOMATİK)
   - Llama4 (Birinci seçim)
   - Gemini 3 Flash (Fallback)

❌ KULLANAMADIĞI MODELLER
   - Grok 4.5, GPT-5.5, Claude Sonnet vb.
```

**Yapacağı İşler:**
- Projeyi ve girdileri analiz et
- O görev zincirine ait `AJAN_HIZLI_BAGLAM.md` oto çıkar
- Model havuzunu ve kaynak gereksinimlerini belirle
- Aşağıdaki katmanları tetikle

---

### 2️⃣ UYGULAYICI AJAN (İşçi - Unlimited Havuz)

**Görev:** Kodlama/mühendislik işlerini en sıkı standartlarla yap.

```
✅ KULLANABİLECEK MODELLER (OTOMATİK)
   - Grok Code Fast (Birinci seçim)
   - GLM 4.7 (Fallback)

❌ KULLANAMADIĞI MODELLER
   - Herhangi bir ücretli model
```

**Kuralları:**
- Dilin en sıkı standartlarına göre saf üretim yap
- Sabit/sahte veri, uydurma placeholder bırakma
- Projenin ana dil kurallarına %100 sadık kal
- İsimlendirme (naming) kurallarını hiç bozma
- Türkçe proje = Türkçe değişkenler/fonksiyonlar/yorumlar

---

### 3️⃣ MEDYA & DOSYA ANALİSTİ (Vision/OCR - Unlimited Havuz)

**Görev:** Girdi olarak Görsel (PNG, JPEG, TIF) veya Belge (PDF, Excel, CAD) alındığında otomatik devreye gir.

```
✅ KULLANABİLECEK MODELLER (OTOMATİK)
   - Gemini 3 Flash (Birinci seçim)
   - Llama4 (Fallback)

❌ KULLANAMADIĞI MODELLER
   - Ücretli görsel modeller
```

**Yapacağı İşler:**
- Nesne temizleme, ekleme/çıkarma işlemleri
- Bölgesel renk/doku manipülasyonu
- OCR veri çıkarma
- Veri tabanına/dosya sistemine entegrasyon betikleri hazırla

---

### 4️⃣ OTONOM TEST VE DOĞRULAMA (QA Katmanı - Unlimited Havuz)

**Görev:** Her çıktıyı teslimat öncesi bağımsız test et.

```
✅ KULLANABİLECEK MODELLER (OTOMATİK)
   - GPT-5 Mini (Tekil seçim)

❌ KULLANAMADIĞI MODELLER
   - Başka herhangi bir model
```

**Yazılım İşleri İçin:**
- Sözdizimi (syntax) hatalarını tarat
- Derleme (build) simülasyonunu çalıştır
- Try/catch ve Belge/Dil servisleri uyumluluğunu kontrol et
- Tip (type) ve kısıtlama (constraint) testleri yürüt

**Görsel İşleri İçin:**
- Üretilen imajı orijinal komutla (prompt) karşılaştır
- Geometrik ve renk doğruluğunu doğrula

**Veri Tabanı İşleri İçin:**
- İlişki (constraint) ID ve şema testlerini yürüt
- Veri bütünlüğünü doğrula

---

### 5️⃣ KENDİ KENDİNİ DENETLEYEN TETİKLEYİCİ (Kriz Yönetimi - Unlimited Havuz)

**Görev:** 4. adımda hata/uyumsuzluk tespit edersen sistemi durdur ve kök nedeni çöz.

```
✅ KULLANABİLECEK MODELLER (OTOMATİK)
   - Llama4 (En derin düşünme modu)
   
FALLBACK
   - Grok Code Fast

❌ KULLANAMADIĞI MODELLER
   - Herhangi bir ücretli model
   - Bütçe yükseltme (KESIN YASAKLI)
```

**Tetikleme Şartları:**
- ❌ Derleme (build) hatası
- ❌ Mantıksal kördüğüm
- ❌ Görsel uyumsuzluk
- ❌ Kural ihlali

**Yapacağı İşler:**
1. Sistemi DURDUR
2. Llama4'ü en derin düşünme modunda devreye al
3. Kök nedeni çöz
4. Tamir et
5. Doğrula
6. Işı çözdükten sonra standart hatta geri düşür

---

### 6️⃣ ARKA PLAN VE VERİ TARAMA (Maliyet Koruması - Unlimited Havuz)

**Görev:** Klasör tarama, MCP sunucusu okuma, CoWork senkronizasyonu.

```
✅ KULLANABİLECEK MODELLER (OTOMATİK)
   - RouteLLM (Birinci seçim)
   - GPT-5 Mini (Fallback)
   - Grok Code Fast (Fallback)

❌ KULLANAMADIĞI MODELLER
   - Claude Haiku 4.5 (KESIN YASAKLI)
   - Herhangi bir ücretli akıl yürütme modeli
```

**Kuralları:**
- Arka plan işlerinde 'Claude Haiku 4.5' ASLA kullanma
- Büyük veri taramalarında sistemi gereksiz döngüye sokma
- Senkronizasyon işlemleri sadece unlimited modellerle yap

---

### 7️⃣ TEMİZ TESLİMAT (İnsan Denetimi)

**Görev:** İş bittiğinde doğrulama, raporlama ve insan onayı.

```
YAPILACak
   1. Derleme/doğrulama özetini çıkar
   2. MODEL_TESLIM.md'ye BEKLİYOR yazı ile kaydet
   3. Tam rapor hazırla

YAPILMADIĞI İŞ
   ❌ KABUL/APPROVED/OKEİ yazma
   ❌ Kendi kendine onaylama
   ❌ İnsan denetçiye sormadan ileri gitme
```

**Rapor İçeriği:**
- Hangi modeller kullanıldı
- Ne kadar otonom işlem yaptı
- Hata/uyarı sayısı
- Finale hazır mı?
- İNSAN DENETÇİ KARARı (BEKLİYOR)

---

## 🔐 BÜTÇE KORUMA MAKİZMALARI

### ✅ YAPILACAK

```json
{
  "budget_protection": {
    "enforce_unlimited_only": true,
    "auto_model_selection": "unlimited_only",
    "paid_model_trigger": "manual_only_by_user"
  }
}
```

### ❌ YASAKLANAN DAVRANIŞLAR

- Otomatik bütçe yükseltme
- Ücretli modelle oto tetikleme
- İnsan izni olmadan kredi harcama
- "Bütçe kaydı çıkabilir" diye model değiştirme

---

## 🚨 SİSTEM DURMA VE KRIZ YÖNETIMI

Herhangi bir noktada hata tespit edildi mi?

**SİSTEM DERHAL DURUR:**
1. Kodu/görseli ilgili katmana geri gönder
2. Llama4'ü en derin modda çalıştır
3. Kök nedeni bul
4. Tamir et
5. Doğrula
6. İnsan onayı bekle

**Bütçe yükseltmeye ASLA başvurma.**

---

## 📋 TÜM PROJELERİ GÜNCELLEME

### Adım 1: Config Dosyası
Her projedeki `.abacusai/config.json`:

```json
{
  "constitution": "EVRENSEL_ÇOKLU_AJAN_KREDİ_KORUMA_v1.0",
  "language_enforcement": "ONLY_TURKISH",
  "budget_protection": {
    "enforce_unlimited_only": true,
    "auto_trigger_allowed": "unlimited_models_only",
    "manual_models": ["grok-4.5", "gpt-5.5", "claude-sonnet", "claude-opus"]
  }
}
```

### Adım 2: MULTI_AJAN_ANAYASASI.md
Her projenin kökünde bu dosyayı kopyala.

### Adım 3: MODEL_TESLIM.md
Teslim öncesi tüm işleri buraya kaydet (BEKLİYOR).

---

## 📝 NOTLAR VE KURALLAR

1. **Türkçe zorunlu** - Tüm sohbetler Türkçe
2. **Unlimited havuz sabit** - Llama4, Gemini Flash, Grok Code Fast, GLM 4.7, GPT-5 Mini, RouteLLM, Kimi K2
3. **Kredili modeller yasak** - Grok 4.5, GPT-5.5, Claude Sonnet/Opus vs.
4. **Otonom ≠ Onaylı** - Teslim öncesi insan denetim şart
5. **Kriz mimarı aktif** - Hata bulur bulunmaz tetiklen
6. **Bütçe hiç yükselmesin** - Unlimited havuzla sonuna kadar git

---

## ✅ ONAY VE GEÇERLILIK

- **Verilen Emir:** Kullanıcı (İNSAN DENETÇİ)
- **Yürürlük:** Temmuz 2026 - Sonsuza kadar
- **Tüm Projeler:** Kapsam alanı global
- **Değişiklik Hakkı:** Yalnız kullanıcı
- **Sistem Sorumluluğu:** Yazılı anayasaya mutlak sadakat

---

**Bu anayasa İNAV SABITTUR. Değişiklik ve istisna YASAKLANMIŞTIR.**

**KESIN EMIR: SADECE UNLIMITED MODELLERLE İŞ YAPA İŞ BITIRE KADAR DURA.**
