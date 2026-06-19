---
# ╔══════════════════════════════════════════════════════════════════╗
# ║  BU DOSYA HER PROJEDE ELLE DOLDURULUR.                          ║
# ║  Diğer kural dosyaları bu değerlere REFERANS verir.              ║
# ╚══════════════════════════════════════════════════════════════════╝

# === PROJE KİMLİĞİ ===
proje_adi: "DesaDoor"
firma_adi: "DesaDoor A.Ş."
firma_unvan: "DesaDoor Mobilya Kapak ve Kapı Sistemleri"
sektor: "kapı/mobilya"
slogan: "Her mekana özel kapılar"
kurulus_yili: 1992

# === DOMAIN / URL ===
url_birincil: "desadoor.com.tr"
url_yedek: "www.desadoor.com.tr"
admin_url: "/admin"
api_base_url: "/api"

# === PORTLAR ===
port_api: 5015
port_ui: 5013
port_signalr: 5015

# === İLETİŞİM ===
iletisim:
  eposta: "info@desadoor.com.tr"
  telefon_1: "+90 224 482 24 00"
  telefon_2: "+90 533 597 32 14"
  whatsapp: ""
  adres: "Çalı Mah. Ömer Biltekin Bulv. No:3/1A Nilüfer / BURSA"
  sehir: "Bursa"
  ilce: "Nilüfer"
  posta_kodu: ""
  enlem: 40.225
  boylam: 28.854
  calisma_saatleri: "Pzt-Cmt 09:00-18:00"

# === SOSYAL MEDYA ===
sosyal:
  instagram: "https://instagram.com/desadoor.com.tr"
  facebook: "https://facebook.com/desadoor"
  twitter: ""
  linkedin: ""
  youtube: ""
  pinterest: ""
  tiktok: ""

# === TEMA / RENK PALETİ ===
tema:
  varyant: "Industrial Luxury"
  ana_renk: "#1A1A27"
  ana_renk_2: "#0a0a0a"
  ikincil_renk: "#C8952A"
  ikincil_renk_2: "#a07020"
  vurgu_renk: "#d4a574"
  vurgu_parlak: "#e8c896"
  arkaplan: "#ffffff"
  arkaplan_yumusak: "#f8f6f2"
  arkaplan_koyu: "#1a1a1a"
  metin: "#2c2c2c"
  metin_acik: "#6c6c6c"
  metin_soluk: "#9a9a9a"
  basari: "#4a7c59"
  uyari: "#c9a449"
  hata: "#9b3d3d"
  bilgi: "#4a6c8c"

# === TİPOGRAFİ ===
font:
  baslik: "Noto Serif"
  metin: "Manrope"
  vurgu: "Cormorant Garamond"
  mono: "JetBrains Mono"

# === STITCH (Google) ENTEGRASYONU ===
stitch:
  aktif: false
  design_md_yolu: "tasarim/DESIGN.md"
  hot_reload: false
  fallback_palet: "tema"

# === DİL / YERELLEŞTİRME ===
diller:
  varsayilan: "tr"
  destekli: ["tr", "en"]
  ceviri_kaynak: "db"

# === MODÜL AKTİVASYONU ===
moduller:
  Blog: true
  Galeri: true
  Iletisim: true
  Sohbet: true
  Medya_Havuzu: true
  AI_Asistan: true
  3D_Goruntu: true
  E_Ticaret: false
  Coklu_Dil: true
  PWA_Offline: false
  Audit_Log: true
  Yedekleme: false

# === GÜVENLİK ===
guvenlik:
  jwt_gecerlilik_dakika: 10080
  refresh_token_gun: 7
  bcrypt_work_factor: 12
  rate_limit_genel_per_5dk: 1000
  rate_limit_giris_per_dk: 5
  iki_adim_dogrulama: false
  passkey: false

# === DEPOLAMA ===
depolama:
  saglayici: "yerel"
  yerel_yol: "wwwroot/medya"
  max_resim_mb: 20
  max_video_mb: 500
  max_pdf_mb: 50
  max_glb_mb: 30

# === AI ENTEGRASYON (admin içi) ===
ai:
  varsayilan_saglayici: "openai"
  varsayilan_model: "gpt-4o-mini"
  fallback_saglayici: "anthropic"
  aylik_limit_usd: 100
  kullanici_gunluk_limit_cagri: 50
  streaming: true
  pii_filtre: true

# === MULTI-TENANT (SaaS) ===
multi_tenant:
  aktif: true
  tenant_tespit: "domain"

# === YEDEK / TEST ===
yedek:
  otomatik_gunluk: false
  saat: "02:00"
  saklama_gun: 30
  konum: "Yedekler/db/"

test:
  min_test_per_ozellik: 5
  postgres_testcontainer: false

# === DEPLOY ===
deploy:
  ortam: "development"
  https_zorunlu: false
  hsts_aktif: false
  csp_aktif: true
---

# DesaDoor Proje Bilgisi

Bu dosya `AGENTS.md` standardına göre doldurulmuştur. Tüm kural dosyaları bu değerlere referans verir.

## Bağlantılar

- **Domain:** desadoor.com.tr
- **API:** http://localhost:5015
- **UI:** http://localhost:5013
- **Admin:** http://localhost:5013/admin/giris
- **Admin Kullanıcı:** admin / desadoor2024
