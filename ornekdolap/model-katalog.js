/**
 * Gold Banyo 3D Model Kataloğu — Demo
 * Her model objesi: { id, ad, kategori, dosya, aciklama, renk, varsayilan }
 * renk = kart önizleme rengi (modelin ana rengine göre)
 * model/ klasörüne GLB dosyaları bırakılarak kullanılır.
 */
const MODEL_KATALOGU = [
  // ─── ARTE SERİSİ (mevcut — ARTE.glb) ──────────────────────
  {
    id: "arte-120",
    ad: "ARTE 120",
    kategori: "Dolap",
    dosya: "./model/ARTE.glb",
    aciklama: "120cm Arte banyo dolabı",
    renk: "#808080",      // Gri gövde (Material__26)
    varsayilan: true
  },

  // ─── HERMES SERİSİ ─────────────────────────────────────────
  {
    id: "hermes-120",
    ad: "HERMES 120",
    kategori: "Dolap",
    dosya: "./model/HERMES_120.glb",
    aciklama: "120cm Hermes — Kahverengi lake gövde, altın aksesuar",
    renk: "#442F29",      // Koyu kahve lake
    varsayilan: false
  },
  {
    id: "hermes-150",
    ad: "HERMES 150",
    kategori: "Dolap",
    dosya: "./model/HERMES_150.glb",
    aciklama: "150cm Hermes — Geniş çift kapılı",
    renk: "#5A3A28",      // Ceviz kahve
    varsayilan: false
  },
  {
    id: "hermes-80",
    ad: "HERMES 80",
    kategori: "Dolap",
    dosya: "./model/HERMES_80.glb",
    aciklama: "80cm Hermes — Tek kapılı kompakt",
    renk: "#6B4C3B",      // Açık kahve
    varsayilan: false
  },

  // ─── GIORGIO SERİSİ ────────────────────────────────────────
  {
    id: "giorgio-120",
    ad: "GIORGIO 120",
    kategori: "Dolap",
    dosya: "./model/GIORGIO_120.glb",
    aciklama: "120cm Giorgio — Antrasit gri, modern çizgi",
    renk: "#383E42",      // Antrasit gri
    varsayilan: false
  },
  {
    id: "giorgio-150",
    ad: "GIORGIO 150",
    kategori: "Dolap",
    dosya: "./model/GIORGIO_150.glb",
    aciklama: "150cm Giorgio — Geniş çift kapılı",
    renk: "#4A5055",      // Koyu gri
    varsayilan: false
  },
  {
    id: "giorgio-80",
    ad: "GIORGIO 80",
    kategori: "Dolap",
    dosya: "./model/GIORGIO_80.glb",
    aciklama: "80cm Giorgio — Tek kapılı kompakt",
    renk: "#555B60",      // Orta gri
    varsayilan: false
  },

  // ─── BOTTEGA SERİSİ ────────────────────────────────────────
  {
    id: "bottega-100",
    ad: "BOTTEGA 100",
    kategori: "Dolap",
    dosya: "./model/BOTTEGA_100.glb",
    aciklama: "100cm Bottega — Doğal taş tonu",
    renk: "#928E85",      // Taş grisi
    varsayilan: false
  },
  {
    id: "bottega-120",
    ad: "BOTTEGA 120",
    kategori: "Dolap",
    dosya: "./model/BOTTEGA_120.glb",
    aciklama: "120cm Bottega — Krem-gri geçiş",
    renk: "#A09C93",      // Açık taş
    varsayilan: false
  },
  {
    id: "bottega-150",
    ad: "BOTTEGA 150",
    kategori: "Dolap",
    dosya: "./model/BOTTEGA_150.glb",
    aciklama: "150cm Bottega — Geniş çift kapılı",
    renk: "#B0ACA3",      // Açık krem-gri
    varsayilan: false
  },

  // ─── DİAGO SERİSİ ──────────────────────────────────────────
  {
    id: "diago-80",
    ad: "DİAGO 80",
    kategori: "Dolap",
    dosya: "./model/DIAGO_80.glb",
    aciklama: "80cm Diago — Mat siyah, minimal tasarım",
    renk: "#1A1A1C",      // Koyu siyah
    varsayilan: false
  },
  {
    id: "diago-100",
    ad: "DİAGO 100",
    kategori: "Dolap",
    dosya: "./model/DIAGO_100.glb",
    aciklama: "100cm Diago — Mat siyah, altın kulp",
    renk: "#252527",      // Siyah
    varsayilan: false
  },
  {
    id: "diago-120",
    ad: "DİAGO 120",
    kategori: "Dolap",
    dosya: "./model/DIAGO_120.glb",
    aciklama: "120cm Diago — Çift kapılı siyah",
    renk: "#2E2E30",      // Siyah-gri
    varsayilan: false
  },

  // ─── LUX SERİSİ ────────────────────────────────────────────
  {
    id: "lux-120",
    ad: "LUX 120",
    kategori: "Dolap",
    dosya: "./model/LUX_120.glb",
    aciklama: "120cm Lux — Beyaz lake, altın detay",
    renk: "#F2ECE1",      // Saf beyaz
    varsayilan: false
  },
  {
    id: "lux-150",
    ad: "LUX 150",
    kategori: "Dolap",
    dosya: "./model/LUX_150.glb",
    aciklama: "150cm Lux — Geniş beyaz lake",
    renk: "#E9E0CB",      // Krem beyaz
    varsayilan: false
  },

  // ─── CLASSIC SERİSİ ────────────────────────────────────────
  {
    id: "classic-80",
    ad: "CLASSIC 80",
    kategori: "Dolap",
    dosya: "./model/CLASSIC_80.glb",
    aciklama: "80cm Classic — Klasik beyaz, oymalı",
    renk: "#F5F0E5",      // Klasik beyaz
    varsayilan: false
  },
  {
    id: "classic-100",
    ad: "CLASSIC 100",
    kategori: "Dolap",
    dosya: "./model/CLASSIC_100.glb",
    aciklama: "100cm Classic — Klasik krem",
    renk: "#EDE5D8",      // Krem
    varsayilan: false
  },
  {
    id: "classic-120",
    ad: "CLASSIC 120",
    kategori: "Dolap",
    dosya: "./model/CLASSIC_120.glb",
    aciklama: "120cm Classic — Klasik krem, çift kapılı",
    renk: "#E5DDD0",      // Koyu krem
    varsayilan: false
  },
  {
    id: "classic-150",
    ad: "CLASSIC 150",
    kategori: "Dolap",
    dosya: "./model/CLASSIC_150.glb",
    aciklama: "150cm Classic — Geniş klasik",
    renk: "#D8D0C3",      // Taş krem
    varsayilan: false
  },

  // ─── MINIMAL SERİSİ ────────────────────────────────────────
  {
    id: "minimal-60",
    ad: "MINIMAL 60",
    kategori: "Dolap",
    dosya: "./model/MINIMAL_60.glb",
    aciklama: "60cm Minimal — Süper kompakt, beyaz",
    renk: "#FAFAFA",      // Saf beyaz
    varsayilan: false
  },
  {
    id: "minimal-80",
    ad: "MINIMAL 80",
    kategori: "Dolap",
    dosya: "./model/MINIMAL_80.glb",
    aciklama: "80cm Minimal — Temiz çizgi, beyaz",
    renk: "#F5F5F5",      // Beyaz
    varsayilan: false
  },
  {
    id: "minimal-100",
    ad: "MINIMAL 100",
    kategori: "Dolap",
    dosya: "./model/MINIMAL_100.glb",
    aciklama: "100cm Minimal — Geniş minimal",
    renk: "#EEEEEE",      // Açık gri-beyaz
    varsayilan: false
  },

  // ─── ROMA SERİSİ ───────────────────────────────────────────
  {
    id: "roma-100",
    ad: "ROMA 100",
    kategori: "Dolap",
    dosya: "./model/ROMA_100.glb",
    aciklama: "100cm Roma — Mermer dokulu, sıcak ton",
    renk: "#C9B99A",      // Sıcak taş
    varsayilan: false
  },
  {
    id: "roma-120",
    ad: "ROMA 120",
    kategori: "Dolap",
    dosya: "./model/ROMA_120.glb",
    aciklama: "120cm Roma — Mermer dokulu, geniş",
    renk: "#B8A889",      // Koyu sıcak taş
    varsayilan: false
  }
];
