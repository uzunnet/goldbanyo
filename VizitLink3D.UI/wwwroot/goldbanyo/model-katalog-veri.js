const MODEL_KATALOG_VERI = {
  "olusturulmaTarihi": "2026-07-19",
  "aciklama": "Gold Banyo 3D Model Katalogu - ornekdolap GLB modellerinin parca yapisi, malzeme ve renk secenekleri",
  "kaynakKlasor": "ornekdolap/model",
  "toplamModel": 9,

  "parcaTipleri": {
    "govde": { "aciklama": "Govde (Alt/Üst Dolap, Boy Dolap, Kasa)", "renklenebilirMi": true, "malzemeDegisebilirMi": true, "malzemeKisiti": ["mdf", "lakeboya", "lakeboyaMat"] },
    "kapak": { "aciklama": "Kapak (On kapak, cam kapak, dolap kapagi)", "renklenebilirMi": true, "malzemeDegisebilirMi": true, "malzemeKisiti": ["mdf", "lakeboya", "cam"] },
    "ayna": { "aciklama": "Ayna (Gumus kaplama cam)", "renklenebilirMi": false, "malzemeDegisebilirMi": false, "malzemeKisiti": ["ayna"] },
    "lavabo": { "aciklama": "Lavabo (Porselen/seramik)", "renklenebilirMi": true, "malzemeDegisebilirMi": false, "malzemeKisiti": ["porselen"] },
    "musluk": { "aciklama": "Musluk / Armatur", "renklenebilirMi": false, "malzemeDegisebilirMi": true, "malzemeKisiti": ["krom", "metal", "lakeboya"] },
    "kulp": { "aciklama": "Kulp / Tutamak", "renklenebilirMi": false, "malzemeDegisebilirMi": true, "malzemeKisiti": ["krom", "metal", "lakeboya"] },
    "led": { "aciklama": "LED Aydinlatma seridi", "renklenebilirMi": false, "malzemeDegisebilirMi": false, "ozel": "acik/kapali + emissive" },
    "ustTabla": { "aciklama": "Ust Tabla / Tezgah", "renklenebilirMi": true, "malzemeDegisebilirMi": true, "malzemeKisiti": ["mermer", "kompozit", "mdf"] },
    "icTabla": { "aciklama": "Ic Raf / Tabla", "renklenebilirMi": true, "malzemeDegisebilirMi": true, "malzemeKisiti": ["mdf", "lakeboyaMat"] },
    "metalAksam": { "aciklama": "Metal cerceve/yan metal aksam", "renklenebilirMi": false, "malzemeDegisebilirMi": true, "malzemeKisiti": ["krom", "metal"] },
    "ayak": { "aciklama": "Dolap ayagi / bacak", "renklenebilirMi": false, "malzemeDegisebilirMi": true, "malzemeKisiti": ["krom", "metal", "plastik"] },
    "yanDolap": { "aciklama": "Yan dolap (bagimsiz modul)", "renklenebilirMi": true, "malzemeDegisebilirMi": true, "malzemeKisiti": ["mdf", "lakeboya", "lakeboyaMat"] },
    "bilinmeyen": { "aciklama": "Siniflandirilamayan mesh (manuel eslesme gerekir)", "renklenebilirMi": false, "malzemeDegisebilirMi": false }
  },

  "malzemeTipleri": {
    "krom": { "aciklama": "Krom - Ayna gibi yansima", "metalness": 0.95, "roughness": 0.05, "varsayilanRenk": "#e8e8e8" },
    "metal": { "aciklama": "Metal - Guclu yansima", "metalness": 0.88, "roughness": 0.15, "varsayilanRenk": "#b8b8b8" },
    "plastik": { "aciklama": "Plastik - Mat yuzey", "metalness": 0, "roughness": 0.55, "varsayilanRenk": "#4a4a4a" },
    "mdf": { "aciklama": "MDF/Laminat - Ahsap dokulu", "metalness": 0, "roughness": 0.68, "varsayilanRenk": "#8b7355" },
    "cam": { "aciklama": "Cam - Refraction + reflection", "metalness": 0, "roughness": 0.01, "varsayilanRenk": "#e0f2f1" },
    "ayna": { "aciklama": "Gercek Ayna - Gumus kaplama cam, tam yansima", "metalness": 1.0, "roughness": 0.02, "varsayilanRenk": "#f2f4f3" },
    "porselen": { "aciklama": "Porselen/Seramik - Parlak, cilali yuzey", "metalness": 0, "roughness": 0.06, "varsayilanRenk": "#f5f5f5" },
    "lakeboya": { "aciklama": "Lake Boya - Yuksek parlak, cam gibi", "metalness": 0.1, "roughness": 0.08, "varsayilanRenk": "#2c2c2c" },
    "lakeboyaMat": { "aciklama": "Lake Boya - Mat yuzey", "metalness": 0.05, "roughness": 0.45, "varsayilanRenk": "#2c2c2c" },
    "mermer": { "aciklama": "Mermer - Dogal tas, hafif parlak", "metalness": 0, "roughness": 0.25, "varsayilanRenk": "#d4ccc8" },
    "kompozit": { "aciklama": "Kompozit Kuvars - Puruzsuz, parlak", "metalness": 0, "roughness": 0.1, "varsayilanRenk": "#e8e4de" }
  },

  "renkKatalogu": [
    { "kod": "RAL 9001", "ad": "Krem", "hex": "#E9E0CB", "grup": "Beyaz/Krem" },
    { "kod": "RAL 9010", "ad": "Saf Beyaz", "hex": "#F2ECE1", "grup": "Beyaz/Krem" },
    { "kod": "RAL 7030", "ad": "Tas Grisi", "hex": "#928E85", "grup": "Gri" },
    { "kod": "RAL 7016", "ad": "Antrasit Grisi", "hex": "#383E42", "grup": "Gri" },
    { "kod": "RAL 9005", "ad": "Derin Siyah", "hex": "#0E0E10", "grup": "Siyah" },
    { "kod": "RAL 8017", "ad": "Cikolata Kahverengisi", "hex": "#442F29", "grup": "Kahverengi" },
    { "kod": "RAL 7040", "ad": "Pencere Grisi", "hex": "#9DA1A2", "grup": "Gri" },
    { "kod": "AHSAP", "ad": "Amerikan Ceviz (Ahsap)", "hex": "#5A3A28", "grup": "Ahsap" }
  ],

  "urunTemalari": [
    { "ad": "Cikolata Kahve", "govde": "RAL 9001|#E9E0CB|mdf", "kapak": "RAL 8017|#442F29|lakeboya", "kulp": "krom|#d4af37", "musluk": "krom|#d4af37", "lavabo": "RAL 9010|#F2ECE1", "ustTabla": "RAL 9001|#E9E0CB", "tezgah": "mermer" },
    { "ad": "Derin Siyah", "govde": "RAL 7016|#383E42|lakeboyaMat", "kapak": "RAL 9005|#0E0E10|lakeboya", "kulp": "metal|#1a1a1a", "musluk": "metal|#1a1a1a", "lavabo": "RAL 9005|#0E0E10", "ustTabla": "RAL 9005|#0E0E10", "tezgah": "mermer" },
    { "ad": "Saf Beyaz", "govde": "RAL 9001|#E9E0CB|mdf", "kapak": "RAL 9010|#F2ECE1|mdf", "kulp": "krom|#d4af37", "musluk": "krom|#d4af37", "lavabo": "RAL 9010|#F2ECE1", "ustTabla": "RAL 7030|#928E85", "tezgah": "kompozit" },
    { "ad": "Antrasit Gri", "govde": "RAL 7030|#928E85|mdf", "kapak": "RAL 7016|#383E42|lakeboyaMat", "kulp": "krom|#d4af37", "musluk": "krom|#d4af37", "lavabo": "RAL 9010|#F2ECE1", "ustTabla": "RAL 7030|#928E85", "tezgah": "mermer" },
    { "ad": "Ahsap Ceviz", "govde": "RAL 9001|#E9E0CB|mdf", "kapak": "AHSAP|#5A3A28|mdf", "kulp": "krom|#d4af37", "musluk": "krom|#d4af37", "lavabo": "RAL 9010|#F2ECE1", "ustTabla": "RAL 9001|#E9E0CB", "tezgah": "mermer" }
  ],

  "modeller": [
    {
      "id": "adonis",
      "ad": "ADONIS",
      "dosya": "model/ADONIS.glb",
      "kategori": "Dolap Seti",
      "boyut": "Standart",
      "ozet": "Alt+Ust dolap, ayna, musluk, lavabo",
      "parcalar": [
        { "meshAdi": "alt dolap", "parcaTipi": "govde", "altTip": "altDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "üst dolap", "parcaTipi": "govde", "altTip": "ustDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "musluk", "parcaTipi": "musluk", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "ayna", "parcaTipi": "ayna", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "kapak", "parcaTipi": "kapak", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "lavabo", "parcaTipi": "lavabo", "renklenebilirMi": true, "malzemeDegisebilirMi": false }
      ]
    },
    {
      "id": "arte",
      "ad": "ARTE",
      "dosya": "model/ARTE.glb",
      "kategori": "Dolap Seti",
      "boyut": "120cm",
      "ozet": "Boy dolap + alt govde, ayna, LED, musluk, lavabo, kulp",
      "parcalar": [
        { "meshAdi": "LED", "parcaTipi": "led", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "Lavabo", "parcaTipi": "lavabo", "renklenebilirMi": true, "malzemeDegisebilirMi": false },
        { "meshAdi": "ALT GÖVDE", "parcaTipi": "govde", "altTip": "altGovde", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "BOY DOLAP GÖVDE", "parcaTipi": "govde", "altTip": "boyDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "AYNA", "parcaTipi": "ayna", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "BOY DOLAP KAPAK", "parcaTipi": "kapak", "altTip": "boyDolapKapak", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "MUSLUK", "parcaTipi": "musluk", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "KAPAK", "parcaTipi": "kapak", "altTip": "altKapak", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "kulp", "parcaTipi": "kulp", "renklenebilirMi": false, "malzemeDegisebilirMi": true }
      ]
    },
    {
      "id": "atreus",
      "ad": "ATREUS",
      "dosya": "model/ATREUS.glb",
      "kategori": "Dolap Seti",
      "boyut": "Standart",
      "ozet": "Govde+alt unite, ayakli, kapak, LED, lavabo",
      "parcalar": [
        { "meshAdi": "GÖVDE", "parcaTipi": "govde", "altTip": "anaGovde", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "ALT ÜNİTE", "parcaTipi": "govde", "altTip": "altUnite", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "AYAK", "parcaTipi": "ayak", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "KAPAK", "parcaTipi": "kapak", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "LAVABO", "parcaTipi": "lavabo", "renklenebilirMi": true, "malzemeDegisebilirMi": false },
        { "meshAdi": "LED", "parcaTipi": "led", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "Cylinder062", "parcaTipi": "bilinmeyen", "not": "Silindirik dekoratif parca - manuel eslesme gerekir" },
        { "meshAdi": "Circle176", "parcaTipi": "bilinmeyen", "not": "Dairesel dekoratif parca - manuel eslesme gerekir" }
      ]
    },
    {
      "id": "capelli-100",
      "ad": "CAPELLI 100",
      "dosya": "model/capelli_100 cm.glb",
      "kategori": "Dolap Seti",
      "boyut": "100cm",
      "ozet": "Alt govde+kasa, cam kapak, led, ayna, musluk, lavabo",
      "parcalar": [
        { "meshAdi": "led", "parcaTipi": "led", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "ayna", "parcaTipi": "ayna", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "musluk", "parcaTipi": "musluk", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "lavabo", "parcaTipi": "lavabo", "renklenebilirMi": true, "malzemeDegisebilirMi": false },
        { "meshAdi": "alt gövde", "parcaTipi": "govde", "altTip": "altGovde", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "kasa", "parcaTipi": "govde", "altTip": "kasa", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "kapak cam", "parcaTipi": "kapak", "altTip": "camKapak", "renklenebilirMi": true, "malzemeDegisebilirMi": true, "malzemeKisiti": ["cam", "lakeboya"] }
      ]
    },
    {
      "id": "capelli-150",
      "ad": "CAPELLI 150",
      "dosya": "model/CAPELLI 150.glb",
      "kategori": "Dolap Seti",
      "boyut": "150cm",
      "ozet": "Genis alt dolap+kasa, cam kapak, led, ayna, musluk, lavabo",
      "parcalar": [
        { "meshAdi": "alt dolap", "parcaTipi": "govde", "altTip": "altDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "led", "parcaTipi": "led", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "alt dolap kasa", "parcaTipi": "govde", "altTip": "kasa", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "alt kapak cam", "parcaTipi": "kapak", "altTip": "camKapak", "renklenebilirMi": true, "malzemeDegisebilirMi": true, "malzemeKisiti": ["cam", "lakeboya"] },
        { "meshAdi": "lavabo", "parcaTipi": "lavabo", "renklenebilirMi": true, "malzemeDegisebilirMi": false },
        { "meshAdi": "ayna", "parcaTipi": "ayna", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "musluk", "parcaTipi": "musluk", "renklenebilirMi": false, "malzemeDegisebilirMi": true }
      ]
    },
    {
      "id": "cavalli-90",
      "ad": "CAVALLI 90",
      "dosya": "model/CAVALLI 90.glb",
      "kategori": "Dolap Seti",
      "boyut": "90cm",
      "ozet": "Alt+Ust dolap, kapaklar, kulp, ayak, ayna, LED, musluk, lavabo",
      "parcalar": [
        { "meshAdi": "lavabo", "parcaTipi": "lavabo", "renklenebilirMi": true, "malzemeDegisebilirMi": false },
        { "meshAdi": "üst dolap", "parcaTipi": "govde", "altTip": "ustDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "ayna", "parcaTipi": "ayna", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "kapaklar", "parcaTipi": "kapak", "not": "Tek mesh icinde coklu kapak - ayni renk/malzeme uygulanir", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "kulp", "parcaTipi": "kulp", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "ayak", "parcaTipi": "ayak", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "musluk", "parcaTipi": "musluk", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "led", "parcaTipi": "led", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "alt dolap", "parcaTipi": "govde", "altTip": "altDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true }
      ]
    },
    {
      "id": "diago-100",
      "ad": "DIAGO 100",
      "dosya": "model/DIAGO 100 CM.glb",
      "kategori": "Dolap Seti",
      "boyut": "100cm",
      "ozet": "Govde+kapak, krom cerceve, ayna, LED, musluk, lavabo, tabla",
      "parcalar": [
        { "meshAdi": "LAVABO", "parcaTipi": "lavabo", "renklenebilirMi": true, "malzemeDegisebilirMi": false },
        { "meshAdi": "KROM CERCEVE", "parcaTipi": "metalAksam", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "AYNA", "parcaTipi": "ayna", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "LED", "parcaTipi": "led", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "MUSLUK", "parcaTipi": "musluk", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "TABLA", "parcaTipi": "ustTabla", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "GÖVDE", "parcaTipi": "govde", "altTip": "anaGovde", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "KAPAK", "parcaTipi": "kapak", "renklenebilirMi": true, "malzemeDegisebilirMi": true }
      ]
    },
    {
      "id": "dolce-100-plus",
      "ad": "DOLCE 100 PLUS",
      "dosya": "model/DOLCE 100 PLUS.glb",
      "kategori": "Dolap Seti",
      "boyut": "100cm+",
      "ozet": "EN KARMASIK MODEL: Ust+alt+yan dolaplar, coklu kapak, yan metal, ayna, LED, musluk, lavabo",
      "parcalar": [
        { "meshAdi": "lavabo", "parcaTipi": "lavabo", "renklenebilirMi": true, "malzemeDegisebilirMi": false },
        { "meshAdi": "dolce80.006", "parcaTipi": "bilinmeyen", "not": "Dekoratif profil/aksesuar - manuel eslesme gerekir" },
        { "meshAdi": "musluk", "parcaTipi": "musluk", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "yan dolap", "parcaTipi": "yanDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "üst dolap", "parcaTipi": "govde", "altTip": "ustDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "ayna", "parcaTipi": "ayna", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "led", "parcaTipi": "led", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "yan metal", "parcaTipi": "metalAksam", "renklenebilirMi": false, "malzemeDegisebilirMi": true },
        { "meshAdi": "yan dolap kapak", "parcaTipi": "kapak", "altTip": "yanKapak1", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "yan dolap kapak1", "parcaTipi": "kapak", "altTip": "yanKapak2", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "alt dolap kapak", "parcaTipi": "kapak", "altTip": "altKapak", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "alt dolap", "parcaTipi": "govde", "altTip": "altDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true }
      ]
    },
    {
      "id": "elsa-60",
      "ad": "ELSA 60",
      "dosya": "model/ELSA 60.glb",
      "kategori": "Dolap Seti",
      "boyut": "60cm",
      "ozet": "Kompakt: Alt+Ust dolap, ayna, lavabo, musluk, ic tabla",
      "parcalar": [
        { "meshAdi": "üst dolap", "parcaTipi": "govde", "altTip": "ustDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "ayna", "parcaTipi": "ayna", "renklenebilirMi": false, "malzemeDegisebilirMi": false },
        { "meshAdi": "alt dolap", "parcaTipi": "govde", "altTip": "altDolap", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "lavobo", "parcaTipi": "lavabo", "not": "Yazim hatasi: 'lavobo' -> lavabo", "renklenebilirMi": true, "malzemeDegisebilirMi": false },
        { "meshAdi": "ic", "parcaTipi": "icTabla", "renklenebilirMi": true, "malzemeDegisebilirMi": true },
        { "meshAdi": "musluk", "parcaTipi": "musluk", "renklenebilirMi": false, "malzemeDegisebilirMi": true }
      ]
    }
  ],

  "notlar": {
    "modelKatalogEksikGLB": "model-katalog.js icinde 30 model tanimli ama sadece 9 GLB mevcut. Eksik HERMES, GIORGIO, BOTTEGA, DIAGO 80/120, LUX, CLASSIC, MINIMAL, ROMA GLB'leri bekleniyor.",
    "parcaSiniflandirma": "Mesh isimleri CAD yazilimindan geldigi icin bazi isimler anlamsiz (Cylinder062, Circle176, dolce80.006). Bunlar admin panelinde manuel eslesme gerektirir.",
    "turkceKarakter": "Bazi mesh isimlerinde Turkce karakter sorunu var (GOVDE yerine GÖVDE okunamamis). GLB binary'de UTF-8 sorunu olabilir.",
    "kapakGrubu": "CAVALLI 90'da 'kapaklar' tek mesh icinde coklu kapak. Ayirma yapilamaz, tek renk/malzeme uygulanir.",
    "adminEntegrasyon": "Bu JSON, admin panelindeki UrunUcBoyutModeli + UrunUcBoyutParcasi entity'lerine aktarilacak. Parca tipleri mevcut enum ile uyumlu."
  }
}
;