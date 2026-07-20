/**
 * Gold Banyo 3D Model Kataloğu — Demo
 * Her model objesi: { id, ad, kategori, dosya, aciklama, renk, onizleme, varsayilan }
 * renk = kart önizleme rengi (modelin ana rengine göre)
 * onizleme = thumbnail resim URL'si (simdilik bos)
 * model/ klasörüne GLB dosyaları bırakılarak kullanılır.
 *
 * Mevcut GLB: 9 adet (ADONIS, ARTE, ATREUS, CAPELLI 100/150, CAVALLI 90,
 *                         DIAGO 100, DOLCE 100 PLUS, ELSA 60)
 * Beklenen GLB: HERMES, GIORGIO, BOTTEGA, LUX, CLASSIC, MINIMAL, ROMA (21 adet)
 */
const MODEL_KATALOGU = [
  // ═══════════ ADONIS ═══════════
  {
    id: "adonis",
    ad: "ADONIS",
    kategori: "Banyo Dolabı",
    dosya: "./model/ADONIS.glb",
    aciklama: "Alt+Üst dolap, ayna, musluk, lavabo — 6 parça",
    renk: "#8B8682",
    onizleme: "",
    varsayilan: false
  },

  // ═══════════ ARTE ═══════════
  {
    id: "arte",
    ad: "ARTE",
    kategori: "Banyo Dolabı",
    dosya: "./model/ARTE.glb",
    aciklama: "Boy dolap + alt gövde, ayna, LED, kulp — 9 parça",
    renk: "#808080",
    onizleme: "",
    varsayilan: true
  },

  // ═══════════ ATREUS ═══════════
  {
    id: "atreus",
    ad: "ATREUS",
    kategori: "Banyo Dolabı",
    dosya: "./model/ATREUS.glb",
    aciklama: "Ayaklı tasarım, alt ünite, LED — 8 parça",
    renk: "#6B5B4F",
    onizleme: "",
    varsayilan: false
  },

  // ═══════════ CAPELLI 100 ═══════════
  {
    id: "capelli-100",
    ad: "CAPELLI 100",
    kategori: "Banyo Dolabı",
    dosya: "./model/capelli_100 cm.glb",
    aciklama: "100cm — Cam kapak, alt gövde+kasa, LED — 7 parça",
    renk: "#9E8B7E",
    onizleme: "",
    varsayilan: false
  },

  // ═══════════ CAPELLI 150 ═══════════
  {
    id: "capelli-150",
    ad: "CAPELLI 150",
    kategori: "Banyo Dolabı",
    dosya: "./model/CAPELLI 150.glb",
    aciklama: "150cm — Geniş cam kapak, çift çekmeceli — 7 parça",
    renk: "#B8A99A",
    onizleme: "",
    varsayilan: false
  },

  // ═══════════ CAVALLI 90 ═══════════
  {
    id: "cavalli-90",
    ad: "CAVALLI 90",
    kategori: "Banyo Dolabı",
    dosya: "./model/CAVALLI 90.glb",
    aciklama: "90cm — Kulp+ayaklı, çoklu kapak, LED — 9 parça",
    renk: "#D4C5B2",
    onizleme: "",
    varsayilan: false
  },

  // ═══════════ DIAGO 100 ═══════════
  {
    id: "diago-100",
    ad: "DIAGO 100",
    kategori: "Banyo Dolabı",
    dosya: "./model/DIAGO 100 CM.glb",
    aciklama: "100cm — Krom çerçeve, mat siyah, tabla — 8 parça",
    renk: "#1A1A1C",
    onizleme: "",
    varsayilan: false
  },

  // ═══════════ DOLCE 100 PLUS ═══════════
  {
    id: "dolce-100-plus",
    ad: "DOLCE 100 PLUS",
    kategori: "Banyo Dolabı",
    dosya: "./model/DOLCE 100 PLUS.glb",
    aciklama: "100cm+ — En kapsamlı: yan dolap, 4 kapak, metal — 12 parça",
    renk: "#C8BFB0",
    onizleme: "",
    varsayilan: false
  },

  // ═══════════ ELSA 60 ═══════════
  {
    id: "elsa-60",
    ad: "ELSA 60",
    kategori: "Banyo Dolabı",
    dosya: "./model/ELSA 60.glb",
    aciklama: "60cm — Kompakt alt+üst dolap, iç tabla — 6 parça",
    renk: "#F5F0EB",
    onizleme: "",
    varsayilan: false
  }
];
