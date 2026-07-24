console.log("📌 uygulama.js başladı");

const elemanlar = {
  // YENİ KONTROLLER
  govdeRenk: document.getElementById("govdeRenk"),
  govdeMalzeme: document.getElementById("govdeMalzeme"),
  kapakRenk: document.getElementById("kapakRenk"),
  kapakMalzeme: document.getElementById("kapakMalzeme"),
  kulpKaplama: document.getElementById("kulpKaplama"),
  ledIsik: document.getElementById("ledIsik"),
  ledSeviye: document.getElementById("ledSeviye"),
  ledRenk: document.getElementById("ledRenk"),
  muslukKaplama: document.getElementById("muslukKaplama"),
  lavaboRenk: document.getElementById("lavaboRenk"),
  ustTablaRenk: document.getElementById("ustTablaRenk"),
  tezgahMalzeme: document.getElementById("tezgahMalzeme"),

  // CANVAS VE TOOLS
  kanvas: document.getElementById("ucboyutKanvas"),
  yuklemeKatmani: document.getElementById("yuklemeKatmani"),
  glbDosyaSec: document.getElementById("glbDosyaSec"),
  sahneButonlari: document.querySelectorAll(".ikon-buton"),
  modelRenkKaplamaAktif: document.getElementById("modelRenkKaplamaAktif"),
};

// modelYolu kaldırıldı — artık MODEL_KATALOGU kullanılıyor

// PBR Material sistem — fiziksel yansımalar
const pbrMalzemeler = {
  krom: {
    renk: "#d8d5ce",
    metalness: 0.82,
    roughness: 0.22,
    envMapIntensity: 0.55,
    clearcoat: 0.12,
    clearcoatRoughness: 0.22,
    aciklama: "Krom — Ayna gibi yansıma"
  },
  metal: {
    renk: "#aaa49a",
    metalness: 0.65,
    roughness: 0.36,
    envMapIntensity: 0.42,
    clearcoat: 0.08,
    clearcoatRoughness: 0.35,
    aciklama: "Metal — Güçlü yansıma"
  },
  kromMat: {
    renk: "#b7b2a8",
    metalness: 0.72,
    roughness: 0.48,
    envMapIntensity: 0.30,
    clearcoat: 0.04,
    clearcoatRoughness: 0.55,
    aciklama: "Krom Mat — Fırçalı/saten krom"
  },
  metalMat: {
    renk: "#8f8a80",
    metalness: 0.55,
    roughness: 0.58,
    envMapIntensity: 0.22,
    clearcoat: 0.03,
    clearcoatRoughness: 0.60,
    aciklama: "Metal Mat — Saten metal yüzey"
  },
  endustriyelBoya: {
    renk: "#30363a",
    metalness: 0.03,
    roughness: 0.68,
    envMapIntensity: 0.10,
    clearcoat: 0.05,
    clearcoatRoughness: 0.55,
    aciklama: "Endüstriyel Boya — Toz boya/mat kaplama"
  },
  pirinc: {
    renk: "#b08d45",
    metalness: 0.78,
    roughness: 0.36,
    envMapIntensity: 0.34,
    clearcoat: 0.08,
    clearcoatRoughness: 0.35,
    aciklama: "Pirinç/Gold — Saten sıcak metal"
  },
  plastik: {
    renk: "#4a4a4a",
    metalness: 0,
    roughness: 0.55,
    envMapIntensity: 0.3,
    clearcoat: 0,
    clearcoatRoughness: 0.8,
    aciklama: "Plastik — Mat yüzey"
  },
  mdf: {
    renk: "#8b7355",
    metalness: 0,
    roughness: 0.82,
    envMapIntensity: 0.08,
    clearcoat: 0,
    clearcoatRoughness: 0.85,
    aciklama: "MDF/Laminat — Ahşap"
  },
  cam: {
    renk: "#d0e4e0",
    metalness: 0.02,
    roughness: 0.08,
    envMapIntensity: 0.25,
    clearcoat: 0.18,
    clearcoatRoughness: 0.25,
    aciklama: "Cam — Hafif seffaf, parlak kaplama"
  },
  ayna: {
    renk: "#f2f4f3",
    metalness: 1.0,
    roughness: 0.02,
    envMapIntensity: 3.8,
    clearcoat: 0,
    clearcoatRoughness: 0,
    aciklama: "Gerçek Ayna — Gümüş kaplama cam, tam yansıma (HDR)"
  },
  porselen: {
    renk: "#f1eee7",
    metalness: 0,
    roughness: 0.30,
    envMapIntensity: 0.22,
    clearcoat: 0.18,
    clearcoatRoughness: 0.25,
    aciklama: "Porselen/Seramik — Parlak, cilalı yüzey"
  },
  lakeboya: {
    renk: "#2c2c2c",
    metalness: 0.02,
    roughness: 0.46,
    envMapIntensity: 0.18,
    clearcoat: 0.12,
    clearcoatRoughness: 0.32,
    aciklama: "Lake Boya — Saten parlak, kontrollu yansima"
  },
  lakeboyaMat: {
    renk: "#2c2c2c",
    metalness: 0,
    roughness: 0.70,
    envMapIntensity: 0.10,
    clearcoat: 0.03,
    clearcoatRoughness: 0.55,
    aciklama: "Lake Boya — Mat yüzey"
  },
  mermer: {
    renk: "#d4ccc8",
    metalness: 0,
    roughness: 0.44,
    envMapIntensity: 0.22,
    clearcoat: 0.10,
    clearcoatRoughness: 0.35,
    aciklama: "Mermer — Doğal taş, hafif parlak"
  },
  kompozit: {
    renk: "#e0ddd5",
    metalness: 0,
    roughness: 0.38,
    envMapIntensity: 0.20,
    clearcoat: 0.10,
    clearcoatRoughness: 0.30,
    aciklama: "Kompozit Kuvars — Pürüzsüz, parlak"
  }
};

const dokuKatalogu = [
  { id: "yok", ad: "Doku Yok", dosya: null, kategori: "genel" },
  { id: "ahsap", ad: "Ahşap", dosya: "./doku/ahsap.png", kategori: "govde" },
  { id: "ceviz", ad: "Amerikan Ceviz", dosya: "./doku/UD_CEViZ_PACO_MODELE.jpg", kategori: "govde" },
  { id: "velure", ad: "Velur", dosya: "./doku/5804_velure.jpg", kategori: "kapak" },
  { id: "velur-tiffany", ad: "Velur Tiffany", dosya: "./doku/GENTA_4236_VELUR_TIFFANY.jpg", kategori: "kapak" },
  { id: "touch", ad: "Touch", dosya: "./doku/5743_touch.jpg", kategori: "kapak" },
  { id: "luna", ad: "Luna", dosya: "./doku/LUNA.jpg", kategori: "kapak" },
  { id: "florida", ad: "Florida", dosya: "./doku/florida.jpg", kategori: "tezgah" },
  { id: "mat-z53", ad: "Mat Z53", dosya: "./doku/MAT_Z53.jpg", kategori: "genel" }
];

// Ürün galerisi varyasyon kombinasyonları — gerçek Gold Banyo kataloğu (Hermes/Giorgio/Bottega) renklerine göre
const urunTemalari = {
  gorsel1: { // Kahve Kapak (Hermes tarzı — sıcak kahve + krem)
    govdeRenk: "RAL 9001|#E9E0CB", govdeMalzeme: "mdf",
    kapakRenk: "RAL 8017|#442F29", kapakMalzeme: "lakeboya",
    kulpKaplama: "krom|#d4af37", muslukKaplama: "krom|#d8d5ce",
    lavaboRenk: "RAL 9010|#F2ECE1", ustTablaRenk: "RAL 9001|#E9E0CB",
    tezgahMalzeme: "mermer"
  },
  gorsel2: { // Siyah Kapak (Hermes siyah varyant)
    govdeRenk: "RAL 7016|#383E42", govdeMalzeme: "lakeboyaMat",
    kapakRenk: "RAL 9005|#0E0E10", kapakMalzeme: "lakeboya",
    kulpKaplama: "metal|#1a1a1a", muslukKaplama: "metal|#1a1a1a",
    lavaboRenk: "RAL 9005|#0E0E10", ustTablaRenk: "RAL 9005|#0E0E10",
    tezgahMalzeme: "mermer"
  },
  gorsel3: { // Beyaz Kapak — tonlar ayrışsın diye gövde/kapak/lavabo/tabla farklı beyaz-krem tonları kullanır
    govdeRenk: "RAL 9001|#E9E0CB", govdeMalzeme: "mdf",
    kapakRenk: "RAL 9010|#F2ECE1", kapakMalzeme: "mdf",
    kulpKaplama: "krom|#d4af37", muslukKaplama: "krom|#d8d5ce",
    lavaboRenk: "RAL 9010|#F2ECE1", ustTablaRenk: "RAL 7030|#928E85",
    tezgahMalzeme: "kompozit"
  },
  gorsel4: { // Antrasit Kapak (Giorgio tarzı — soğuk gri + altın armatür)
    govdeRenk: "RAL 7030|#928E85", govdeMalzeme: "mdf",
    kapakRenk: "RAL 7016|#383E42", kapakMalzeme: "lakeboyaMat",
    kulpKaplama: "krom|#d4af37", muslukKaplama: "krom|#d8d5ce",
    lavaboRenk: "RAL 9010|#F2ECE1", ustTablaRenk: "RAL 7030|#928E85",
    tezgahMalzeme: "mermer"
  },
  gorsel5: { // Ahşap Kapak (Bottega/Diago tarzı — doğal ceviz)
    govdeRenk: "RAL 9001|#E9E0CB", govdeMalzeme: "mdf",
    kapakRenk: "AHSAP|#5A3A28", kapakMalzeme: "mdf",
    kulpKaplama: "krom|#d4af37", muslukKaplama: "krom|#d8d5ce",
    lavaboRenk: "RAL 9010|#F2ECE1", ustTablaRenk: "RAL 9001|#E9E0CB",
    tezgahMalzeme: "mermer"
  }
};

const sahneDurumu = {
  renderer: null,
  sahne: null,
  kamera: null,
  kontroller: null,
  modelKoku: null,
  modelParcalari: {
    govde: [],
    ayna: [],
    kapak1: [],
    kapak2: [],
    kapak3: [],
    kapak4: [],
    kulp: [],
    led: [],
    musluk: [],
    lavabo: [],
    metalAksam: [],
    ustTabla: [],
    icUstTabla: [],
    icAltTabla: [],
    kapaklar: [],
    montajAparati: []
  },
  kapakPivotlari: {
    sol: null,
    sag: null
  },
  aynaYansima: null,
  aynaKaplamalari: [],
  etkinNesneAdresi: null,
  ledIsigi: null,
  ledZamani: 0,
  sahneAyarlari: null,
  isiklar: {},
  malzemeleri: {
    govde: "mdf",
    kapak: "plastik",
    metal: "krom"
  },
  dokular: {
    govdeDoku: "yok",
    kapakDoku: "yok",
    tezgahDoku: "yok",
    govdeDokuOffsetX: 0,
    govdeDokuOffsetY: 0,
    govdeDokuRepeat: 1,
    govdeDokuRotation: 0,
    kapakDokuOffsetX: 0,
    kapakDokuOffsetY: 0,
    kapakDokuRepeat: 1,
    kapakDokuRotation: 0,
    tezgahDokuOffsetX: 0,
    tezgahDokuOffsetY: 0,
    tezgahDokuRepeat: 1,
    tezgahDokuRotation: 0
  },
  zeminRenk: "#050505",
  zeminOpaklik: 0.16,
  golgeOpaklik: 0.16,
  golgeBoyut: 2048,
  golgeBias: -0.0008,
  arkaPlanRenk: "#050505",
  hdrYogunluk: 1,
  hdrDonme: 0,
  hdrBlurluk: 0,
  hdrPmremDoku: null,
  dokuYukleyici: new THREE.TextureLoader(),
  dokuOnbellek: {},
  renkKaplamaAktif: false,
  // Parça seçim sistemi
  raycaster: null,
  pointerBaslangic: new THREE.Vector2(),
  secilenMesh: null,
  secilenParcaKategorisi: null,
  secimVurgusu: null,
  secimListenerKuruldu: false
};

window.__ornekDolap3D = sahneDurumu;

function ralDegeriniAl(deger) {
  return deger.split("|")[1] || deger;
}

function secimEtiketiniAl(deger) {
  return deger.split("|")[0] || deger;
}

function yaziyiBasliklastir(metin) {
  return metin
    .split("-")
    .map((parca) => parca.charAt(0).toUpperCase() + parca.slice(1))
    .join(" ");
}

function durumYaz(metin) {
  // Eski sistemde kullanılmıyor
}

function yuklemeKatmaniniAyarla(goster) {
  elemanlar.yuklemeKatmani.classList.toggle("gizli", !goster);
}

function modelParcalariniSifirla() {
  // Seçim vurgusunu temizle
  if (sahneDurumu.secimVurgusu) {
    if (sahneDurumu.sahne) sahneDurumu.sahne.remove(sahneDurumu.secimVurgusu);
    if (sahneDurumu.secimVurgusu.geometry) sahneDurumu.secimVurgusu.geometry.dispose();
    if (sahneDurumu.secimVurgusu.material) sahneDurumu.secimVurgusu.material.dispose();
    sahneDurumu.secimVurgusu = null;
  }
  sahneDurumu.secilenMesh = null;
  sahneDurumu.secilenParcaKategorisi = null;
  // Bilgi metnini sıfırla
  var bilgiEl = document.getElementById('secilenParcaBilgisi');
  if (bilgiEl) { bilgiEl.textContent = 'Model üzerinde bir parça seçin.'; bilgiEl.classList.remove('vurgulu'); }

  gercekAynaKaplamalariniTemizle();
  sahneDurumu.modelParcalari = {
    govde: [],
    ayna: [],
    kapak1: [],
    kapak2: [],
    kapak3: [],
    kapak4: [],
    kulp: [],
    led: [],
    musluk: [],
    lavabo: [],
    metalAksam: [],
    ustTabla: [],
    icUstTabla: [],
    icAltTabla: [],
    kapaklar: [],
    montajAparati: []
  };
  sahneDurumu.kapakPivotlari.sol = null;
  sahneDurumu.kapakPivotlari.sag = null;
}

function konfigOzetiniGuncelle() {
  // Yeni sistemde kullanılmıyor
}

function konfigOzetiniGuncelleEski() {
  const ust = secimEtiketiniAl(elemanlar.ustRenk.value);
  const alt = secimEtiketiniAl(elemanlar.altRenk.value);
  const ayna = secimEtiketiniAl(elemanlar.aynaRenk.value);
  const lavabo = secimEtiketiniAl(elemanlar.lavaboRenk.value);
  const cerceve = yaziyiBasliklastir(elemanlar.cerceveKaplama.value);
  const musluk = yaziyiBasliklastir(elemanlar.muslukKaplama.value);

  elemanlar.seciliUstRenk.textContent = ust;
  elemanlar.seciliAltRenk.textContent = alt;
  elemanlar.seciliAynaRenk.textContent = ayna;
  elemanlar.seciliLavaboRenk.textContent = lavabo;
  elemanlar.seciliCerceveKaplama.textContent = cerceve;
  elemanlar.seciliMuslukKaplama.textContent = musluk;
  if (elemanlar.solKapak.disabled || elemanlar.sagKapak.disabled) {
    elemanlar.seciliKapakDurumu.textContent = "Model tek kapak mesh";
  } else {
    elemanlar.seciliKapakDurumu.textContent = `${elemanlar.solKapak.value} / ${elemanlar.sagKapak.value}`;
  }
  elemanlar.altGovdeOzeti.textContent = `${ust} / ${alt}`;
  elemanlar.altMetalOzeti.textContent = `${cerceve} / ${musluk}`;

  rengiGorsellestir();
}

function rengiGorsellestir() {
  const aynaGorsel = document.getElementById("aynaGorsel");

  // Ayna görseli rengini güncelle
  if (aynaGorsel) {
    const aynaRengi = ralDegeriniAl(elemanlar.aynaRenk.value);
    const aynaGradyant = `linear-gradient(135deg, ${aynaRengi}cc 0%, ${aynaRengi} 50%, ${aynaRengi}99 100%)`;
    aynaGorsel.style.background = aynaGradyant;
  }

  // Renk swatches'ı güncelle
  if (elemanlar.govdeRenkSwatch) {
    const govdeRengi = ralDegeriniAl(elemanlar.ustRenk.value);
    elemanlar.govdeRenkSwatch.style.backgroundColor = govdeRengi;
  }
  if (elemanlar.aynaRenkSwatch) {
    const aynaRengi = ralDegeriniAl(elemanlar.aynaRenk.value);
    elemanlar.aynaRenkSwatch.style.backgroundColor = aynaRengi;
  }
  if (elemanlar.lavaboRenkSwatch) {
    const lavaboRengi = ralDegeriniAl(elemanlar.lavaboRenk.value);
    elemanlar.lavaboRenkSwatch.style.backgroundColor = lavaboRengi;
  }
  if (elemanlar.cerceveRenkSwatch) {
    const cerceveRengi = kaplamalar[elemanlar.cerceveKaplama.value]?.renk || "#333";
    elemanlar.cerceveRenkSwatch.style.backgroundColor = cerceveRengi;
  }
  if (elemanlar.muslukRenkSwatch) {
    const muslukRengi = kaplamalar[elemanlar.muslukKaplama.value]?.renk || "#333";
    elemanlar.muslukRenkSwatch.style.backgroundColor = muslukRengi;
  }
}

function parcaDurumunuGuncelle() {
  // Yeni sistemde kullanılmıyor
}

function fizikselMateryalOlustur(renk, metalness = 0.18, roughness = 0.58) {
  return new THREE.MeshPhysicalMaterial({
    color: new THREE.Color(renk),
    metalness,
    roughness,
    envMapIntensity: 0.72,
    clearcoat: 0.12,
    clearcoatRoughness: 0.42
  });
}

function pbrMateryalOlustur(malzemeTuru, renk = null, dokuId = null, dokuAyar) {
  const malzeme = pbrMalzemeler[malzemeTuru] || pbrMalzemeler.plastik;
  const malzemeAyarlari = sahneDurumu.sahneAyarlari?.ayarlar?.materials || {};
  const envScale = malzemeAyarlari.globalEnvMapScale ?? 1;
  const clearcoatScale = malzemeAyarlari.globalClearcoatScale ?? 1;
  const roughnessOffset = malzemeAyarlari.globalRoughnessOffset ?? 0;
  // HDR baz intensity: env scale uygulanmis ama HDR yogunluk henuz eklenmemis
  var baseEnvIntensity = malzeme.envMapIntensity * envScale;
  const config = {
    color: new THREE.Color(renk || malzeme.renk),
    metalness: malzeme.metalness,
    roughness: Math.min(1, Math.max(0, malzeme.roughness + roughnessOffset)),
    envMap: sahneDurumu.sahne?.environment || null,
    envMapIntensity: baseEnvIntensity * (sahneDurumu.hdrYogunluk ?? 1),
    clearcoat: Math.min(1, malzeme.clearcoat * clearcoatScale),
    clearcoatRoughness: malzeme.clearcoatRoughness
  };

  if (dokuId && dokuId !== "yok") {
    var doku = sahneDurumu.dokuOnbellek[dokuId];
    if (doku) {
      // Texture kopyasi olustur ki farkli parcalar birbirinin ayarlarini bozmasin
      var dokuKopya = doku.clone();
      var ayar = dokuAyar || {};
      var offsetX = ayar.offsetX || 0;
      var offsetY = ayar.offsetY || 0;
      var repeat = ayar.repeat || 1;
      var rotation = ayar.rotation || 0;
      dokuKopya.offset.set(offsetX, offsetY);
      dokuKopya.repeat.set(repeat, repeat);
      if (rotation !== 0) {
        dokuKopya.center.set(0.5, 0.5);
        dokuKopya.rotation = rotation * Math.PI / 180;
      }
      dokuKopya.needsUpdate = true;
      config.map = dokuKopya;
    }
  }

  // Cam seffaf kaplama
  if (malzemeTuru === 'cam') {
    config.transparent = true;
    config.opacity = malzemeAyarlari.camOpacity ?? 0.82;
    config.depthWrite = false;
  }


  var materyal = new THREE.MeshPhysicalMaterial(config);
  materyal.userData.hdrBazEnvMapIntensity = baseEnvIntensity;
  return materyal;
}

function camMateryalOlustur(renk) {
  // GERÇEK AYNA: gümüş kaplama, opak, canlı CubeCamera yansımalı
  var baseEnvIntensity = sahneDurumu.sahneAyarlari?.ayarlar?.materials?.mirror?.envMapIntensity ?? 1.25;
  const materyal = new THREE.MeshPhysicalMaterial({
    color: new THREE.Color(sahneDurumu.sahneAyarlari?.ayarlar?.materials?.mirror?.color || "#d8dbd8"),
    envMap: sahneDurumu.sahne?.environment || null,
    metalness: 1.0,
    roughness: sahneDurumu.sahneAyarlari?.ayarlar?.materials?.mirror?.roughness ?? 0.045,
    envMapIntensity: baseEnvIntensity * (sahneDurumu.hdrYogunluk ?? 1),
    clearcoat: 0,
    clearcoatRoughness: 0,
    reflectivity: 1,
    side: THREE.FrontSide
  });
  materyal.toneMapped = true;
  materyal.userData.hdrBazEnvMapIntensity = baseEnvIntensity;
  return materyal;
}

function ledAyariniAl() {
  const seviye = elemanlar.ledSeviye?.value || "dusuk";
  const renkSecimi = elemanlar.ledRenk?.value || "sicakSari";
  const seviyeler = sahneDurumu.sahneAyarlari?.ayarlar?.led?.levels || {
    dusuk: { lightIntensity: 0.14, emissiveIntensity: 0.38 },
    orta: { lightIntensity: 0.24, emissiveIntensity: 0.58 },
    yuksek: { lightIntensity: 0.36, emissiveIntensity: 0.82 },
    maksimum: { lightIntensity: 0.52, emissiveIntensity: 1.08 }
  };
  const renkler = sahneDurumu.sahneAyarlari?.ayarlar?.led?.colors || {
    sicakSari: "#ffd36a",
    gunisigi: "#fff1c1",
    beyaz: "#ffffff",
    sogukBeyaz: "#dff3ff"
  };
  const secili = seviyeler[seviye] || seviyeler.dusuk;
  return {
    isik: secili.isik ?? secili.lightIntensity ?? 0.14,
    emissive: secili.emissive ?? secili.emissiveIntensity ?? 0.38,
    renk: renkler[renkSecimi] || renkler.sicakSari
  };
}

function ledMateryalOlustur() {
  const ayar = ledAyariniAl();
  const materyal = new THREE.MeshStandardMaterial({
    color: ayar.renk,
    emissive: ayar.renk,
    emissiveIntensity: ayar.emissive,
    metalness: 0,
    roughness: 0.45
  });
  materyal.toneMapped = true;
  return materyal;
}

function ledSonukMateryalOlustur() {
  return new THREE.MeshStandardMaterial({
    color: "#3a3a3a",
    emissive: "#000000",
    emissiveIntensity: 0,
    metalness: 0.1,
    roughness: 0.6
  });
}

function banyoYansimaDokusuOlustur() {
  const tuval = document.createElement("canvas");
  tuval.width = 2048;
  tuval.height = 1024;
  const cizim = tuval.getContext("2d");

  const arkaPlan = cizim.createLinearGradient(0, 0, 0, tuval.height);
  arkaPlan.addColorStop(0, "#d8d4ca");
  arkaPlan.addColorStop(0.42, "#8d877b");
  arkaPlan.addColorStop(0.66, "#3b3935");
  arkaPlan.addColorStop(1, "#151515");
  cizim.fillStyle = arkaPlan;
  cizim.fillRect(0, 0, tuval.width, tuval.height);

  cizim.globalAlpha = 0.2;
  cizim.strokeStyle = "#ffffff";
  cizim.lineWidth = 1;
  for (let x = 0; x <= tuval.width; x += 192) {
    cizim.beginPath();
    cizim.moveTo(x, 0);
    cizim.lineTo(x, tuval.height);
    cizim.stroke();
  }
  for (let y = 160; y <= 720; y += 144) {
    cizim.beginPath();
    cizim.moveTo(0, y);
    cizim.lineTo(tuval.width, y);
    cizim.stroke();
  }
  cizim.globalAlpha = 1;

  const isik = cizim.createLinearGradient(0, 0, tuval.width, 0);
  isik.addColorStop(0, "rgba(255,255,255,0)");
  isik.addColorStop(0.34, "rgba(255,244,210,0.72)");
  isik.addColorStop(0.5, "rgba(255,255,255,0.92)");
  isik.addColorStop(0.66, "rgba(255,244,210,0.72)");
  isik.addColorStop(1, "rgba(255,255,255,0)");
  cizim.fillStyle = isik;
  cizim.fillRect(0, 68, tuval.width, 68);

  cizim.fillStyle = "rgba(206, 219, 226, 0.78)";
  cizim.fillRect(184, 236, 340, 252);
  cizim.fillRect(1524, 252, 260, 208);
  cizim.fillStyle = "rgba(35, 33, 31, 0.72)";
  cizim.fillRect(700, 570, 660, 152);
  cizim.fillStyle = "rgba(198, 149, 42, 0.36)";
  cizim.fillRect(780, 530, 500, 24);

  // Keskin kenarlı "pencere" panelleri — yansımanın belirgin/tanınabilir olması için
  const pencereler = [
    { x: 80, y: 120, w: 300, h: 440 },
    { x: 880, y: 140, w: 300, h: 460 },
    { x: 1680, y: 130, w: 300, h: 440 }
  ];
  pencereler.forEach((p) => {
    cizim.fillStyle = "#f5f2e8";
    cizim.fillRect(p.x, p.y, p.w, p.h);
    cizim.strokeStyle = "#2b2822";
    cizim.lineWidth = 6;
    cizim.strokeRect(p.x, p.y, p.w, p.h);
    cizim.strokeStyle = "#2b2822";
    cizim.lineWidth = 3;
    cizim.beginPath();
    cizim.moveTo(p.x + p.w / 2, p.y);
    cizim.lineTo(p.x + p.w / 2, p.y + p.h);
    cizim.moveTo(p.x, p.y + p.h / 2);
    cizim.lineTo(p.x + p.w, p.y + p.h / 2);
    cizim.stroke();
  });

  const doku = new THREE.CanvasTexture(tuval);
  doku.mapping = THREE.EquirectangularReflectionMapping;
  doku.magFilter = THREE.LinearFilter;
  doku.minFilter = THREE.LinearMipmapLinearFilter;
  doku.generateMipmaps = true;
  doku.needsUpdate = true;
  if ("colorSpace" in doku && THREE.SRGBColorSpace) {
    doku.colorSpace = THREE.SRGBColorSpace;
  }
  return doku;
}

function dokuYukle(dokuId) {
  if (!dokuId || dokuId === "yok") return Promise.resolve(null);
  if (sahneDurumu.dokuOnbellek[dokuId]) return Promise.resolve(sahneDurumu.dokuOnbellek[dokuId]);
  var dokuKayit = dokuKatalogu.find(function(d) { return d.id === dokuId; });
  if (!dokuKayit || !dokuKayit.dosya) return Promise.resolve(null);
  return new Promise(function(resolve) {
    sahneDurumu.dokuYukleyici.load(dokuKayit.dosya, function(texture) {
      texture.wrapS = THREE.RepeatWrapping;
      texture.wrapT = THREE.RepeatWrapping;
      texture.repeat.set(1, 1);
      if (texture.colorSpace !== undefined) texture.colorSpace = THREE.SRGBColorSpace;
      sahneDurumu.dokuOnbellek[dokuId] = texture;
      console.log("✅ Doku yüklendi:", dokuKayit.ad, "boyut:", texture.image.width + "x" + texture.image.height);
      resolve(texture);
    }, undefined, function(hata) {
      console.warn("⚠️ Doku yüklenemedi:", dokuKayit.dosya);
      resolve(null);
    });
  });
}

function dokuSec(hedef, dokuId) {
  sahneDurumu.dokular[hedef] = dokuId;
  // Doku ayar panelini goster/gizle
  var ayarPanelId = hedef === "govdeDoku" ? "govdeDokuAyar" : hedef === "kapakDoku" ? "kapakDokuAyar" : hedef === "tezgahDoku" ? "tezgahDokuAyar" : null;
  if (ayarPanelId) {
    var ayarPanel = document.getElementById(ayarPanelId);
    if (ayarPanel) ayarPanel.style.display = (dokuId && dokuId !== "yok") ? "" : "none";
  }

  if (dokuId && dokuId !== "yok") {
    dokuYukle(dokuId).then(function() { renkleriUygula(); });
  } else {
    renkleriUygula();
  }
}

// Slider hareketlerinde TUM malzemeyi yeniden olusturmadan sadece texture parametrelerini guncelle
function dokuAyarlariniGuncelle(parcaTipi) {
  var parcalar = sahneDurumu.modelParcalari[parcaTipi] || [];
  var hedefDokuId;
  var hedefAyarlari;

  if (parcaTipi === "govde") {
    hedefDokuId = sahneDurumu.dokular.govdeDoku;
    hedefAyarlari = {
      offsetX: sahneDurumu.dokular.govdeDokuOffsetX,
      offsetY: sahneDurumu.dokular.govdeDokuOffsetY,
      repeat: sahneDurumu.dokular.govdeDokuRepeat,
      rotation: sahneDurumu.dokular.govdeDokuRotation
    };
  } else if (parcaTipi === "kapak1" || parcaTipi === "kapak2" || parcaTipi === "kapak3" || parcaTipi === "kapak4" || parcaTipi === "kapaklar") {
    hedefDokuId = sahneDurumu.dokular.kapakDoku;
    hedefAyarlari = {
      offsetX: sahneDurumu.dokular.kapakDokuOffsetX,
      offsetY: sahneDurumu.dokular.kapakDokuOffsetY,
      repeat: sahneDurumu.dokular.kapakDokuRepeat,
      rotation: sahneDurumu.dokular.kapakDokuRotation
    };
  } else if (parcaTipi === "icAltTabla") {
    hedefDokuId = sahneDurumu.dokular.tezgahDoku;
    hedefAyarlari = {
      offsetX: sahneDurumu.dokular.tezgahDokuOffsetX,
      offsetY: sahneDurumu.dokular.tezgahDokuOffsetY,
      repeat: sahneDurumu.dokular.tezgahDokuRepeat,
      rotation: sahneDurumu.dokular.tezgahDokuRotation
    };
  }

  if (!hedefDokuId || hedefDokuId === "yok") return;

  parcalar.forEach(function(parca) {
    parca.traverse(function(nesne) {
      if (!nesne.isMesh || !nesne.material) return;
      var mat = nesne.material;
      if (mat.map && mat.map.isTexture) {
        var orijinalDoku = sahneDurumu.dokuOnbellek[hedefDokuId];
        if (!orijinalDoku) return;
        var yeniDoku = orijinalDoku.clone();
        yeniDoku.offset.set(hedefAyarlari.offsetX || 0, hedefAyarlari.offsetY || 0);
        yeniDoku.repeat.set(hedefAyarlari.repeat || 1, hedefAyarlari.repeat || 1);
        if (hedefAyarlari.rotation && hedefAyarlari.rotation !== 0) {
          yeniDoku.center.set(0.5, 0.5);
          yeniDoku.rotation = hedefAyarlari.rotation * Math.PI / 180;
        }
        yeniDoku.needsUpdate = true;
        mat.map = yeniDoku;
        mat.needsUpdate = true;
      }
    });
  });
}

function gercekHdrYukle(sahne) {
  if (typeof BANYO_HDR_BASE64 === "undefined" || !BANYO_HDR_BASE64 || typeof THREE.RGBELoader === "undefined") {
    console.warn("⚠️ Gerçek HDR verisi/loader bulunamadı, prosedürel doku kullanılıyor");
    return;
  }

  const arrayBuffer = base64ArrayBufferineCevir(BANYO_HDR_BASE64);
  const blob = new Blob([arrayBuffer], { type: "application/octet-stream" });
  const blobUrl = URL.createObjectURL(blob);

  const yukleyici = new THREE.RGBELoader();
  yukleyici.load(
    blobUrl,
    (texture) => {
      texture.mapping = THREE.EquirectangularReflectionMapping;
      texture.magFilter = THREE.LinearFilter;
      texture.minFilter = THREE.LinearFilter;

      if (sahneDurumu.renderer && THREE.PMREMGenerator) {
        const pmremGenerator = new THREE.PMREMGenerator(sahneDurumu.renderer);
        pmremGenerator.compileEquirectangularShader();
        const pmrem = pmremGenerator.fromEquirectangular(texture);
        sahne.environment = pmrem.texture;
        sahneDurumu._hdrDoku = texture; // Cache'le (dispose ETME)
        pmremGenerator.dispose();
      } else {
        sahne.environment = texture;
      }

      sahne.background = new THREE.Color(0x050505);
      URL.revokeObjectURL(blobUrl);
      console.log("✅ HDR PMREM filtreli environment olarak yüklendi (piksel/patlama azaltıldı)");
    },
    undefined,
    (hata) => {
      console.error("❌ Gerçek HDR yüklenemedi, prosedürel doku ile devam ediliyor:", hata);
      URL.revokeObjectURL(blobUrl);
    }
  );
}

function materyalGuvenliKopya(mesh) {
  if (Array.isArray(mesh.material)) {
    mesh.material = mesh.material.map((materyal) => materyal.clone());
  } else if (mesh.material) {
    mesh.material = mesh.material.clone();
  }
}

function herMeshIcin(nesneler, islem) {
  nesneler.forEach((nesne) => {
    nesne.traverse((alt) => {
      if (alt.isMesh) {
        islem(alt);
      }
    });
  });
}

function parcaMateryaliUygula(nesneler, materyalUretici) {
  herMeshIcin(nesneler, (mesh) => {
    mesh.material = materyalUretici(mesh);
    mesh.castShadow = true;
    mesh.receiveShadow = true;
  });
}

function parcayiTumListelerdenCikar(mesh) {
  Object.values(sahneDurumu.modelParcalari).forEach((liste) => {
    const indeks = liste.indexOf(mesh);
    if (indeks >= 0) {
      liste.splice(indeks, 1);
    }
  });
}

function aynaMeshleriniGoster(goster) {
  herMeshIcin(sahneDurumu.modelParcalari.ayna, (mesh) => {
    mesh.visible = goster;
  });
  sahneDurumu.aynaKaplamalari.forEach((kaplama) => {
    kaplama.visible = goster;
  });
}

function aynaYansimasiniGuncelle() {
  if (!sahneDurumu.aynaYansima || !sahneDurumu.renderer || !sahneDurumu.sahne || !sahneDurumu.modelParcalari.ayna.length) {
    return;
  }

  const kutu = new THREE.Box3();
  sahneDurumu.modelParcalari.ayna.forEach((parca) => kutu.expandByObject(parca));
  const merkez = kutu.getCenter(new THREE.Vector3());
  if (![merkez.x, merkez.y, merkez.z].every(Number.isFinite)) {
    return;
  }

  sahneDurumu.aynaYansima.kamera.position.copy(merkez);
  aynaMeshleriniGoster(false);

  // Görünen arka plan sabit renk olsa da ayna zengin bir ortamı yansıtsın
  const gorunenArkaPlan = sahneDurumu.sahne.background;
  sahneDurumu.sahne.background = sahneDurumu.sahne.environment;
  sahneDurumu.aynaYansima.kamera.update(sahneDurumu.renderer, sahneDurumu.sahne);
  sahneDurumu.sahne.background = gorunenArkaPlan;

  aynaMeshleriniGoster(true);
}

function gercekAynaKaplamalariniTemizle() {
  sahneDurumu.aynaKaplamalari.forEach((kaplama) => {
    kaplama.parent?.remove(kaplama);
    kaplama.geometry?.dispose();
    kaplama.material?.dispose();
  });
  sahneDurumu.aynaKaplamalari = [];
}

function gercekAynaKaplamalariniKur() {
  // Kapalı: modelin kendi ayna mesh'i kullanılacak, ekstra düz plane kare izi yapıyor.
  return;
  if (!sahneDurumu.aynaYansima || !sahneDurumu.modelParcalari.ayna.length) {
    return;
  }

  gercekAynaKaplamalariniTemizle();
  sahneDurumu.modelParcalari.ayna.forEach((mesh) => {
    if (!mesh.geometry?.boundingBox) {
      mesh.geometry.computeBoundingBox();
    }

    const kutu = mesh.geometry.boundingBox;
    if (!kutu) {
      return;
    }

    const olcu = kutu.getSize(new THREE.Vector3());
    const merkez = kutu.getCenter(new THREE.Vector3());
    const genislik = Math.max(olcu.x * 0.96, 0.18);
    const yukseklik = Math.max(olcu.y * 0.96, 0.18);
    const geometri = new THREE.PlaneGeometry(genislik, yukseklik);
    const materyal = new THREE.MeshPhysicalMaterial({
      color: 0xf2f4f2,
      metalness: 1,
      roughness: 0.016,
      envMap: sahneDurumu.sahne?.environment || sahneDurumu.aynaYansima.hedef.texture,
      envMapIntensity: 2.35,
      clearcoat: 0,
      clearcoatRoughness: 0,
      side: THREE.FrontSide
    });
    const kaplama = new THREE.Mesh(geometri, materyal);
    kaplama.name = "GERCEK_AYNA_YANSIMA_KAPLAMA";
    kaplama.position.set(merkez.x, merkez.y, kutu.max.z + 0.003);
    mesh.add(kaplama);
    sahneDurumu.aynaKaplamalari.push(kaplama);
  });
}

function isimNormallestir(metin) {
  return (metin || "")
    .toLowerCase()
    .replaceAll("ı", "i")
    .replaceAll("ş", "s")
    .replaceAll("ğ", "g")
    .replaceAll("ü", "u")
    .replaceAll("ö", "o")
    .replaceAll("ç", "c");
}

function nesneAdYolunuAl(nesne) {
  const adlar = [];
  let aktif = nesne;
  while (aktif) {
    if (aktif.name) {
      adlar.push(aktif.name);
    }
    aktif = aktif.parent;
  }
  return adlar.join(" ");
}

function kutuMerkeziAl(nesne) {
  const kutu = new THREE.Box3().setFromObject(nesne);
  return kutu.getCenter(new THREE.Vector3());
}

function kutuOlcusuAl(nesne) {
  const kutu = new THREE.Box3().setFromObject(nesne);
  return kutu.getSize(new THREE.Vector3());
}

function modelParcasiniSiniflandir(mesh, modelKutu) {
  const meshAdi = nesneAdYolunuAl(mesh);
  const ad = isimNormallestir(`${meshAdi} ${mesh.material?.name || ""}`);

  // ═══ METAL / KASA / ÇERÇEVE ═══ (camlı modellerde sabit kasa/çerçeve)
  if (ad.includes("kasa") || ad.includes("cerceve") || ad.includes("frame") || ad.includes("metal")) return "metalAksam";

  // ═══ GÖVDE ═══ (ALT GÖVDE, BOY DOLAP GÖVDE, ana gövde)
  if (ad.includes("alt govde") || ad.includes("boy dolap govde") ||
      ad.includes("govde") || ad.includes("body")) return "govde";

  // ═══ AYNA ═══ (ayna, mirror, cam yüzey)
  if (ad.includes("ayna") || ad.includes("atna") || ad.includes("mirror") ||
      ad.includes("glass") || ad.includes("gercek_ayna")) return "ayna";

  // ═══ KAPAK ═══ (BOY DOLAP KAPAK, KAPAK, kapak1-4, door)
  // Önce numaralı kapakları kontrol et (kapak1, kapak2, ...)
  if (ad.includes("kapak 1") || ad.includes("kapak1") || ad.includes("kapak_1")) return "kapak1";
  if (ad.includes("kapak 2") || ad.includes("kapak2") || ad.includes("kapak_2")) return "kapak2";
  if (ad.includes("kapak 3") || ad.includes("kapak3") || ad.includes("kapak_3")) return "kapak3";
  if (ad.includes("kapak 4") || ad.includes("kapak4") || ad.includes("kapak_4")) return "kapak4";
  // Genel kapak eşleşmesi (BOY DOLAP KAPAK, KAPAK, door, panel)
  if (ad.includes("boy dolap kapak") || ad.includes("kapak") ||
      ad.includes("door") || ad.includes("panel")) return "kapak1";

  // ═══ KULP ═══ (kulp, handle, tutamak)
  if (ad.includes("kulp") || ad.includes("handle") || ad.includes("tutamak")) return "kulp";

  // ═══ LED ═══ (led, isik, aydinlatma, light)
  if (ad.includes("led") || ad.includes("isik") || ad.includes("aydinlatma") || ad.includes("light")) return "led";

  // ═══ MUSLUK ═══ (musluk, faucet, tap, armatur)
  if (ad.includes("musluk") || ad.includes("faucet") || ad.includes("tap") ||
      ad.includes("armatur") || ad.includes("vana")) return "musluk";

  // ═══ LAVABO ═══ (lavabo, sink, basin, umumiye)
  if (ad.includes("lavabo") || ad.includes("lavobo") || ad.includes("sink") ||
      ad.includes("basin") || ad.includes("umumiye")) return "lavabo";

  // ═══ ÜST TABLA ═══ (üst tabla, tezgah üstü)
  if (ad.includes("ust tabla") || ad.includes("ust_tabla") || ad.includes("ust tabla")) return "ustTabla";
  if (ad.includes("ic ust tabla") || ad.includes("ic_ust_tabla") ||
      ad.includes("ic ust tabla") || ad.includes("ic_ust_tabla")) return "icUstTabla";

  // ═══ İÇ TABLAR ═══
  if (ad.includes("ic alt tabla") || ad.includes("ic_alt_tabla") ||
      ad.includes("ic alt tabla") || ad.includes("ic_alt_tabla")) return "icAltTabla";

  // ═══ DİĞER ═══
  if (ad.includes("kapaklar")) return "kapaklar";
  if (ad.includes("montaj")) return "montajAparati";
  if (ad.includes("tabana") || ad.includes("base")) return "icAltTabla";

  // ═══ MALZEME ADI EŞLEŞTIRMESİ ═══ (mesh adı tanınamıyorsa malzemeye bak)
  // M_01___DefaultA → muhtemelen kapak (açık gri, parlak yüzey)
  if (ad.includes("m_01")) return "kapak1";
  // M_02___Default → muhtemelen lavabo (beyaz, porselen)
  if (ad.includes("m_02")) return "lavabo";
  // Material__26 → muhtemelen gövde (gri, mat)
  if (ad.includes("material__26") || ad.includes("material_26")) return "govde";
  // M_03___Default → kulp bileşeni (koyu)
  if (ad.includes("m_03")) return "kulp";
  // M_04___Default → kulp bileşeni (altın/bronz)
  if (ad.includes("m_04")) return "kulp";

  // Fallback: pozisyona göre belirleme
  return "govde";
}

function pivotOlustur(hedefler, yon) {
  if (!hedefler.length || !sahneDurumu.modelKoku) {
    return null;
  }

  const kutu = new THREE.Box3();
  hedefler.forEach((hedef) => kutu.expandByObject(hedef));

  const merkez = kutu.getCenter(new THREE.Vector3());
  const menteseX = yon === "sol" ? kutu.min.x : kutu.max.x;
  const yerelKonum = sahneDurumu.modelKoku.worldToLocal(new THREE.Vector3(menteseX, merkez.y, merkez.z));

  const pivot = new THREE.Group();
  pivot.position.copy(yerelKonum);
  sahneDurumu.modelKoku.add(pivot);

  hedefler.forEach((hedef) => {
    pivot.attach(hedef);
  });

  return pivot;
}

function kapakPivotlariniHazirla() {
  // Yeni sistemde kapak1-4 var, eski solKapak/sagKapak yok
  if (sahneDurumu.modelParcalari.solKapak && sahneDurumu.modelParcalari.solKapak.length > 0) {
    sahneDurumu.kapakPivotlari.sol = pivotOlustur(sahneDurumu.modelParcalari.solKapak, "sol");
  }
  if (sahneDurumu.modelParcalari.sagKapak && sahneDurumu.modelParcalari.sagKapak.length > 0) {
    sahneDurumu.kapakPivotlari.sag = pivotOlustur(sahneDurumu.modelParcalari.sagKapak, "sag");
  }
}

function kapakKontrolleriniGuncelle() {
  // Yeni sistemde kullanılmıyor
}

function renkleriUygula() {
  if (!sahneDurumu.modelKoku) {
    console.warn("⚠️ Model henüz yüklenmedi");
    return;
  }

  console.log("🎨 renkleriUygula() başladı");

  // YENİ KONTROLLLERDEN RENK VE MALZEME SEÇİMLERİNİ AL
  const aynaRenk = "#A5A8A6"; // Ayna sabit — kullanıcı değiştiremez
  const govdeRenk = ralDegeriniAl(elemanlar.govdeRenk?.value || "RAL 9001|#E9E0CB");
  const govdeMalzeme = elemanlar.govdeMalzeme?.value || "mdf";

  const kapakRenk = ralDegeriniAl(elemanlar.kapakRenk?.value || "RAL 8017|#442F29");
  const kapakMalzeme = elemanlar.kapakMalzeme?.value || "mdf";

  const [kulpMalzeme, kulpRenk] = (elemanlar.kulpKaplama?.value || "krom|#e8e8e8").split("|");
  const lavaboRenk = ralDegeriniAl(elemanlar.lavaboRenk?.value || "RAL 9010|#F2ECE1");
  const ustTablaRenk = ralDegeriniAl(elemanlar.ustTablaRenk?.value || "RAL 7030|#928E85");
  const [muslukMalzeme, muslukRenk] = (elemanlar.muslukKaplama?.value || "krom|#e8e8e8").split("|");
  const tezgahMalzeme = elemanlar.tezgahMalzeme?.value || "mermer";

  // AYNA — Gerçek gümüş kaplama, canlı HDR yansıma (CubeCamera)
  parcaMateryaliUygula(sahneDurumu.modelParcalari.ayna, () => camMateryalOlustur(aynaRenk));

  // GÖVDE
  parcaMateryaliUygula(sahneDurumu.modelParcalari.govde, () => pbrMateryalOlustur(govdeMalzeme, govdeRenk, sahneDurumu.dokular.govdeDoku, { offsetX: sahneDurumu.dokular.govdeDokuOffsetX, offsetY: sahneDurumu.dokular.govdeDokuOffsetY, repeat: sahneDurumu.dokular.govdeDokuRepeat, rotation: sahneDurumu.dokular.govdeDokuRotation }));

  // METAL/KASA/ÇERÇEVE — camlı modellerde sabit saten metal, gövde/cam rengine karışmaz
  parcaMateryaliUygula(sahneDurumu.modelParcalari.metalAksam || [], () => pbrMateryalOlustur("metal", "#b7b2a8"));

  // KAPAK — Mesh adında "cam" geçiyorsa cam malzeme, değilse seçili kapak malzemesi
  function kapakMateryaliUret(mesh) {
    var kapakDoku = sahneDurumu.dokular.kapakDoku;
    var kapakDokuAyar = { offsetX: sahneDurumu.dokular.kapakDokuOffsetX, offsetY: sahneDurumu.dokular.kapakDokuOffsetY, repeat: sahneDurumu.dokular.kapakDokuRepeat, rotation: sahneDurumu.dokular.kapakDokuRotation };
    // DEBUG: UV ve doku durumunu kontrol et
    if (mesh.geometry) {
      var hasUV = !!mesh.geometry.attributes.uv;
      console.log("🔍 Kapak mesh:", mesh.name, "UV var mi:", hasUV, "Doku:", kapakDoku);
    }
    var meshAdi = (mesh.name || "").toLowerCase();
    var yolAdi = nesneAdYolunuAl(mesh).toLowerCase();
    if (meshAdi.includes("cam") || yolAdi.includes("cam") || meshAdi.includes("glass") || yolAdi.includes("glass") || meshAdi.includes("seffaf") || yolAdi.includes("seffaf")) {
      return pbrMateryalOlustur("cam", kapakRenk);
    }
    if (kapakDoku && kapakDoku !== "yok") {
      return pbrMateryalOlustur(kapakMalzeme, kapakRenk, kapakDoku, kapakDokuAyar);
    }
    return pbrMateryalOlustur(kapakMalzeme, kapakRenk);
  }
  
  herMeshIcin(sahneDurumu.modelParcalari.kapak1, (mesh) => { mesh.material = kapakMateryaliUret(mesh); mesh.castShadow = true; mesh.receiveShadow = true; });
  herMeshIcin(sahneDurumu.modelParcalari.kapak2, (mesh) => { mesh.material = kapakMateryaliUret(mesh); mesh.castShadow = true; mesh.receiveShadow = true; });
  herMeshIcin(sahneDurumu.modelParcalari.kapak3, (mesh) => { mesh.material = kapakMateryaliUret(mesh); mesh.castShadow = true; mesh.receiveShadow = true; });
  herMeshIcin(sahneDurumu.modelParcalari.kapak4, (mesh) => { mesh.material = kapakMateryaliUret(mesh); mesh.castShadow = true; mesh.receiveShadow = true; });

  // KAPAKLAR (tek mesh çoklu kapak - CAVALLI 90 gibi)
  if (sahneDurumu.modelParcalari.kapaklar && sahneDurumu.modelParcalari.kapaklar.length > 0) {
    herMeshIcin(sahneDurumu.modelParcalari.kapaklar, (mesh) => {
      mesh.material = kapakMateryaliUret(mesh);
      mesh.castShadow = true;
      mesh.receiveShadow = true;
    });
  }

  // KULP
  parcaMateryaliUygula(sahneDurumu.modelParcalari.kulp, () => pbrMateryalOlustur(kulpMalzeme, kulpRenk));

  // LED — Emissive (açık/kapalı)
  const ledAcik = elemanlar.ledIsik?.checked ?? true;
  sahneDurumu.ledAcik = ledAcik;
  if (ledAcik) {
    parcaMateryaliUygula(sahneDurumu.modelParcalari.led, () => ledMateryalOlustur());
    if (sahneDurumu.ledIsigi) {
      sahneDurumu.ledIsigi.visible = true;
    }
  } else {
    parcaMateryaliUygula(sahneDurumu.modelParcalari.led, () => ledSonukMateryalOlustur());
    if (sahneDurumu.ledIsigi) {
      sahneDurumu.ledIsigi.visible = false;
    }
  }

  // MUSLUK
  parcaMateryaliUygula(sahneDurumu.modelParcalari.musluk, () => pbrMateryalOlustur(muslukMalzeme, muslukRenk));

  // LAVABO — Porselen
  parcaMateryaliUygula(sahneDurumu.modelParcalari.lavabo, () => pbrMateryalOlustur("porselen", lavaboRenk));

  // ÜST TABLA
  parcaMateryaliUygula(sahneDurumu.modelParcalari.ustTabla, () => pbrMateryalOlustur(govdeMalzeme, ustTablaRenk));
  parcaMateryaliUygula(sahneDurumu.modelParcalari.icUstTabla, () => pbrMateryalOlustur(govdeMalzeme, ustTablaRenk));

  // TEZGAH — kendi doğal taş rengini kullanır, gövde rengine zorla bağlı değil
  parcaMateryaliUygula(sahneDurumu.modelParcalari.icAltTabla, () => pbrMateryalOlustur(tezgahMalzeme, null, sahneDurumu.dokular.tezgahDoku, { offsetX: sahneDurumu.dokular.tezgahDokuOffsetX, offsetY: sahneDurumu.dokular.tezgahDokuOffsetY, repeat: sahneDurumu.dokular.tezgahDokuRepeat, rotation: sahneDurumu.dokular.tezgahDokuRotation }));

  // Renk değişiminden sonra seçim vurgusunu koru
  if (sahneDurumu.secilenMesh) {
    secimVurgusunuGuncelle(sahneDurumu.secilenMesh);
  }
}

function kapakAyarla() {
  // Yeni sistemde kullanılmıyor
}

function temayiUygula(temaAdi) {
  // Önce katalogdaki ürün temalarına bak
  var tema = null;
  if (typeof MODEL_KATALOG_VERI !== "undefined" && MODEL_KATALOG_VERI.urunTemalari) {
    // gorsel1, gorsel2... → indekse çevir: gorsel1 → 0, gorsel2 → 1
    var temaIndeks = parseInt(temaAdi.replace("gorsel", "")) - 1;
    if (temaIndeks >= 0 && temaIndeks < MODEL_KATALOG_VERI.urunTemalari.length) {
      var katalogTema = MODEL_KATALOG_VERI.urunTemalari[temaIndeks];
      tema = {
        govdeRenk: katalogTema.govde.split("|").slice(0, 2).join("|"),
        govdeMalzeme: katalogTema.govde.split("|")[2] || "mdf",
        kapakRenk: katalogTema.kapak.split("|").slice(0, 2).join("|"),
        kapakMalzeme: katalogTema.kapak.split("|")[2] || "mdf",
        kulpKaplama: katalogTema.kulp,
        muslukKaplama: katalogTema.musluk && katalogTema.musluk.startsWith("krom|") ? "krom|#d8d5ce" : katalogTema.musluk,
        lavaboRenk: katalogTema.lavabo,
        ustTablaRenk: katalogTema.ustTabla,
        tezgahMalzeme: katalogTema.tezgah
      };
    }
  }
  
  // Fallback: eski urunTemalari
  if (!tema) {
    tema = urunTemalari[temaAdi];
  }
  if (!tema) return;

  Object.entries(tema).forEach(([alanId, deger]) => {
    const input = document.getElementById(alanId);
    if (!input) return;
    input.value = deger;

    // İlgili renk/malzeme grubundaki aktif class'ı senkronize et
    const grup = document.querySelector(`[data-hedef="${alanId}"]`);
    if (grup) {
      grup.querySelectorAll("button").forEach((buton) => {
        buton.classList.toggle("aktif", buton.dataset.deger === deger);
      });
    }
  });

  renkleriUygula();
}

function gorunumuAyarla(gorunum) {
  if (!sahneDurumu.kamera || !sahneDurumu.kontroller) {
    return;
  }

  sahneDurumu.kontroller.target.set(0, 0.9, 0);

  if (gorunum === "on") {
    sahneDurumu.kamera.position.set(0.15, 1.3, 6.2);
  } else if (gorunum === "ust") {
    sahneDurumu.kamera.position.set(0.4, 4.8, 4.1);
  } else {
    sahneDurumu.kamera.position.set(3.2, 1.85, 5.9);
  }

  sahneDurumu.kontroller.update();
}

function kamerayiModeleSigdir(model) {
  if (!sahneDurumu.kamera || !sahneDurumu.kontroller) {
    return;
  }

  const kutu = new THREE.Box3().setFromObject(model);
  const merkez = kutu.getCenter(new THREE.Vector3());
  const boyut = kutu.getSize(new THREE.Vector3());
  const enBuyuk = Math.max(boyut.x, boyut.y, boyut.z) || 1;
  if (![merkez.x, merkez.y, merkez.z, boyut.x, boyut.y, boyut.z, enBuyuk].every(Number.isFinite)) {
    sahneDurumu.kontroller.target.set(0, 0, 0);
    sahneDurumu.kamera.position.set(0, 0.4, 6);
    sahneDurumu.kamera.near = 0.01;
    sahneDurumu.kamera.far = 100;
    sahneDurumu.kamera.updateProjectionMatrix();
    sahneDurumu.kontroller.update();
    // Kamera varsayilan kadrajda
    return;
  }
  const mesafe = enBuyuk * 2.28;

  sahneDurumu.kontroller.target.copy(merkez);
  sahneDurumu.kamera.position.set(merkez.x + enBuyuk * 0.28, merkez.y + enBuyuk * 0.10, merkez.z + mesafe);
  sahneDurumu.kamera.near = Math.max(enBuyuk / 100, 0.01);
  sahneDurumu.kamera.far = Math.max(enBuyuk * 12, 100);
  sahneDurumu.kamera.updateProjectionMatrix();
  sahneDurumu.kontroller.update();
}

function sahneyiHazirla() {
  const renderer = new THREE.WebGLRenderer({
    canvas: elemanlar.kanvas,
    antialias: true,
    alpha: true
  });
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  renderer.toneMapping = THREE.ACESFilmicToneMapping;
  renderer.toneMappingExposure = 0.56;
  if ("outputColorSpace" in renderer && THREE.SRGBColorSpace) {
    renderer.outputColorSpace = THREE.SRGBColorSpace;
  } else if ("outputEncoding" in renderer && THREE.sRGBEncoding) {
    renderer.outputEncoding = THREE.sRGBEncoding;
  }
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = THREE.PCFSoftShadowMap;

  const sahne = new THREE.Scene();
  const odaDokusu = banyoYansimaDokusuOlustur();
  sahne.background = new THREE.Color(0x050505); // Profesyonel ürün render için koyu stüdyo arka plan
  sahne.environment = odaDokusu; // HDR yüklenene kadar prosedürel environment
  sahne.fog = new THREE.Fog(0x171717, 16, 34);
  gercekHdrYukle(sahne); // Gerçek HDR environment: materyal yansımalarını doğal yapar

  const aynaYansimaHedefi = new THREE.WebGLCubeRenderTarget(1024, {
    generateMipmaps: true,
    minFilter: THREE.LinearMipmapLinearFilter
  });
  const aynaYansimaKamerasi = new THREE.CubeCamera(0.05, 40, aynaYansimaHedefi);
  sahne.add(aynaYansimaKamerasi);

  const kamera = new THREE.PerspectiveCamera(32, 1, 0.1, 120);
  kamera.position.set(3.2, 1.85, 5.9);

  const kontroller = new THREE.OrbitControls(kamera, elemanlar.kanvas);
  kontroller.enableDamping = true;
  kontroller.dampingFactor = 0.08;
  kontroller.target.set(0, 1.2, 0);
  kontroller.minDistance = 1.5;
  kontroller.maxDistance = 20;

  const ortamIsigi = new THREE.HemisphereLight(0xf0eadf, 0x151515, 0.24);
  sahne.add(ortamIsigi);
  sahneDurumu.isiklar.ortam = ortamIsigi;

  // Ana ışık — daha yandan/sıyırıcı açı: oluklu (fluted) yüzeylerde her girinti kendi ışık/gölgesini alsın
  const anaIsik = new THREE.DirectionalLight(0xfff4e3, 0.30);
  anaIsik.position.set(4.2, 5.4, 4.8);
  anaIsik.castShadow = true;
  anaIsik.shadow.mapSize.set(2048, 2048);
  anaIsik.shadow.bias = -0.0008;
  anaIsik.shadow.normalBias = 0.035;
  sahne.add(anaIsik);
  sahneDurumu.isiklar.ana = anaIsik;

  // Kenar/sıyırma ışığı — ters yönden, oluklu dokunun derinliğini vurgular
  const kenarIsik = new THREE.DirectionalLight(0xdfeaff, 0.10);
  kenarIsik.position.set(-4.2, 2.6, 4.8);
  sahne.add(kenarIsik);
  sahneDurumu.isiklar.kenar = kenarIsik;

  const dolguIsik = new THREE.PointLight(0xc9def8, 0.06, 10);
  dolguIsik.position.set(-4.5, 3.1, -1.8);
  sahne.add(dolguIsik);
  sahneDurumu.isiklar.dolgu = dolguIsik;

  const zemin = new THREE.Mesh(
    new THREE.CircleGeometry(6.5, 80),
    new THREE.ShadowMaterial({ color: 0x000000, opacity: 0.16 })
  );
  zemin.rotation.x = -Math.PI / 2;
  zemin.position.y = -0.002; // Modelin tam alt yüzeyinin hemen altı — z-fighting'i önlemek için ufak ofset
  zemin.receiveShadow = true;
  sahne.add(zemin);
  sahneDurumu.isiklar.zemin = zemin;

  sahneDurumu.renderer = renderer;
  sahneDurumu.sahne = sahne;
  sahneDurumu.kamera = kamera;
  sahneDurumu.kontroller = kontroller;
  sahneDurumu.aynaYansima = {
    hedef: aynaYansimaHedefi,
    kamera: aynaYansimaKamerasi
  };

  pencereBoyutunuUygula();
  canlandir();
}

function pencereBoyutunuUygula() {
  if (!sahneDurumu.renderer || !sahneDurumu.kamera) {
    return;
  }

  const kapsayici = elemanlar.kanvas.parentElement;
  const genislik = kapsayici.clientWidth;
  const yukseklik = kapsayici.clientHeight;
  sahneDurumu.renderer.setSize(genislik, yukseklik, false);
  sahneDurumu.kamera.aspect = genislik / Math.max(yukseklik, 1);
  sahneDurumu.kamera.updateProjectionMatrix();
}

function modeliOrtalaVeOlcekle(model) {
  const kutu = new THREE.Box3().setFromObject(model);
  const boyut = kutu.getSize(new THREE.Vector3());
  const enBuyukOlcu = Math.max(boyut.x, boyut.y, boyut.z) || 1;
  const olcek = 3.2 / enBuyukOlcu;

  model.scale.setScalar(olcek);

  const yeniKutu = new THREE.Box3().setFromObject(model);
  const yeniMerkez = yeniKutu.getCenter(new THREE.Vector3());
  const yeniMin = yeniKutu.min.clone();

  model.position.x -= yeniMerkez.x;
  model.position.y -= yeniMin.y; // Modelin alt yüzü tam y=0'da olsun — zemin/gölge oraya hizalanır
  model.position.z -= yeniMerkez.z;
}

// ═══ PARÇA SEÇİM SİSTEMİ ═══════════════════════════════════

// Parça kategorisini (iç isim) UI grup adına ve bilgi metnine çevir
function parcaKategorisiniCoz(kategori) {
  var harita = {
    govde: { grup: 'govde', metin: 'Gövde seçildi.' },
    kapak1: { grup: 'kapak', metin: 'Kapak seçildi.' },
    kapak2: { grup: 'kapak', metin: 'Kapak seçildi.' },
    kapak3: { grup: 'kapak', metin: 'Kapak seçildi.' },
    kapak4: { grup: 'kapak', metin: 'Kapak seçildi.' },
    kapaklar: { grup: 'kapak', metin: 'Kapak seçildi.' },
    kulp: { grup: 'kulp', metin: 'Kulp seçildi.' },
    musluk: { grup: 'musluk', metin: 'Musluk seçildi.' },
    lavabo: { grup: 'lavabo', metin: 'Lavabo seçildi.' },
    ustTabla: { grup: 'ustTabla', metin: 'Üst Tabla seçildi.' },
    icUstTabla: { grup: 'ustTabla', metin: 'Üst Tabla seçildi.' },
    icAltTabla: { grup: 'tezgah', metin: 'Tezgah seçildi.' },
    led: { grup: 'led', metin: 'LED Işık seçildi.' }
  };
  return harita[kategori] || null;
}

// Sadece hedef parça grubunu aç, diğer tüm data-model-ayari gruplarını kapat
function parcaAyarGrubunuAc(hedefGrup) {
  var tumGruplar = document.querySelectorAll('[data-model-ayari]');
  tumGruplar.forEach(function(grup) {
    var parcaGrubu = grup.getAttribute('data-parca-grubu');
    if (!parcaGrubu) return; // Ürün Galerisi gibi parca-grubu olmayanları atla
    if (parcaGrubu === hedefGrup) {
      // Hedef grubu aç
      grup.classList.remove('kapali');
      var baslik = grup.querySelector('.bolum-baslik');
      if (baslik) baslik.setAttribute('aria-expanded', 'true');
    } else {
      // Diğerlerini kapat
      grup.classList.add('kapali');
      var baslik = grup.querySelector('.bolum-baslik');
      if (baslik) baslik.setAttribute('aria-expanded', 'false');
    }
  });
}

// Seçim vurgusu temizle
function secimVurgusunuTemizle() {
  if (sahneDurumu.secimVurgusu) {
    if (sahneDurumu.sahne) sahneDurumu.sahne.remove(sahneDurumu.secimVurgusu);
    if (sahneDurumu.secimVurgusu.geometry) sahneDurumu.secimVurgusu.geometry.dispose();
    if (sahneDurumu.secimVurgusu.material) sahneDurumu.secimVurgusu.material.dispose();
    sahneDurumu.secimVurgusu = null;
  }
}

// Seçim vurgusunu güncelle
function secimVurgusunuGuncelle(mesh) {
  secimVurgusunuTemizle();
  if (!mesh || !sahneDurumu.sahne) return;
  try {
    var vurgu = new THREE.BoxHelper(mesh, 0xC8952A);
    sahneDurumu.sahne.add(vurgu);
    sahneDurumu.secimVurgusu = vurgu;
  } catch (e) {
    console.warn('BoxHelper oluşturulamadı:', e);
  }
}

// Canvas üzerinde parça seçim listener'larını kur (bir kez)
function modelParcasiSeciminiKur() {
  if (sahneDurumu.secimListenerKuruldu) return;
  if (!elemanlar.kanvas) return;
  sahneDurumu.secimListenerKuruldu = true;
  if (!sahneDurumu.raycaster) sahneDurumu.raycaster = new THREE.Raycaster();

  var suruklemeEsigi = 6; // piksel

  elemanlar.kanvas.addEventListener('pointerdown', function(olay) {
    if (olay.pointerType !== 'mouse' || olay.button !== 0) return;
    sahneDurumu.pointerBaslangic.set(olay.clientX, olay.clientY);
  });

  elemanlar.kanvas.addEventListener('pointerup', function(olay) {
    if (olay.pointerType !== 'mouse' || olay.button !== 0) return;
    var dx = olay.clientX - sahneDurumu.pointerBaslangic.x;
    var dy = olay.clientY - sahneDurumu.pointerBaslangic.y;
    var mesafe = Math.sqrt(dx * dx + dy * dy);
    if (mesafe > suruklemeEsigi) return; // drag, tıklama değil

    if (!sahneDurumu.modelKoku || !sahneDurumu.kamera) return;

    var rect = elemanlar.kanvas.getBoundingClientRect();
    var ndcX = ((olay.clientX - rect.left) / rect.width) * 2 - 1;
    var ndcY = -((olay.clientY - rect.top) / rect.height) * 2 + 1;

    sahneDurumu.raycaster.setFromCamera(new THREE.Vector2(ndcX, ndcY), sahneDurumu.kamera);

    // Sadece modelKoku altındaki mesh'leri recursive intersect et
    var tumMeshler = [];
    sahneDurumu.modelKoku.traverse(function(nesne) {
      if (nesne.isMesh) tumMeshler.push(nesne);
    });

    var kesisme = sahneDurumu.raycaster.intersectObjects(tumMeshler, false);
    if (kesisme.length === 0) {
      // Boş alana tıklandı, seçimi temizle
      secimVurgusunuTemizle();
      sahneDurumu.secilenMesh = null;
      sahneDurumu.secilenParcaKategorisi = null;
      var bilgiEl = document.getElementById('secilenParcaBilgisi');
      if (bilgiEl) { bilgiEl.textContent = 'Model üzerinde bir parça seçin.'; bilgiEl.classList.remove('vurgulu'); }
      return;
    }

    var mesh = kesisme[0].object;
    var modelKutu = new THREE.Box3().setFromObject(sahneDurumu.modelKoku);
    var kategori = modelParcasiniSiniflandir(mesh, modelKutu);
    var cozum = parcaKategorisiniCoz(kategori);

    var bilgiEl = document.getElementById('secilenParcaBilgisi');

    if (!cozum) {
      // Bilinmeyen/ayarlanamaz parça (ayna, metalAksam, montajAparati vb.)
      secimVurgusunuTemizle();
      sahneDurumu.secilenMesh = null;
      sahneDurumu.secilenParcaKategorisi = null;
      if (bilgiEl) { bilgiEl.textContent = 'Bu parçanın değiştirilebilir bir ayarı yok.'; bilgiEl.classList.remove('vurgulu'); }
      return;
    }

    // Geçerli parça seçildi
    sahneDurumu.secilenMesh = mesh;
    sahneDurumu.secilenParcaKategorisi = kategori;
    secimVurgusunuGuncelle(mesh);
    parcaAyarGrubunuAc(cozum.grup);
    if (bilgiEl) { bilgiEl.textContent = cozum.metin; bilgiEl.classList.add('vurgulu'); }
  });
}

function kutuUVOlustur(geometri) {
  if (!geometri || !geometri.attributes.position) return;
  // Eger UV zaten varsa dokunma — modelin kendi UV'sini koru
  if (geometri.attributes.uv) {
    console.log("ℹ️ UV zaten mevcut, atlaniyor");
    return;
  }
  var pozisyon = geometri.attributes.position;
  var sayi = pozisyon.count;
  var uvDizisi = new Float32Array(sayi * 2);
  var kutu = new THREE.Box3().setFromBufferAttribute(pozisyon);
  var olcu = kutu.getSize(new THREE.Vector3());
  var merkez = kutu.getCenter(new THREE.Vector3());
  var enBuyuk = Math.max(olcu.x, olcu.y, olcu.z) || 1;

  for (var i = 0; i < sayi; i++) {
    var x = pozisyon.getX(i);
    var y = pozisyon.getY(i);
    var z = pozisyon.getZ(i);
    // Box-projection: her yuzu duzleme esitle
    var nx = (x - merkez.x) / enBuyuk + 0.5;
    var ny = (y - merkez.y) / enBuyuk + 0.5;
    var nz = (z - merkez.z) / enBuyuk + 0.5;
    // En buyuk absolute degeri hangi yuze projekte edilecegini belirler
    var ax = Math.abs(x - merkez.x);
    var ay = Math.abs(y - merkez.y);
    var az = Math.abs(z - merkez.z);
    if (ax >= ay && ax >= az) {
      uvDizisi[i * 2] = ny;
      uvDizisi[i * 2 + 1] = nz;
    } else if (ay >= ax && ay >= az) {
      uvDizisi[i * 2] = nx;
      uvDizisi[i * 2 + 1] = nz;
    } else {
      uvDizisi[i * 2] = nx;
      uvDizisi[i * 2 + 1] = ny;
    }
  }
  geometri.setAttribute("uv", new THREE.BufferAttribute(uvDizisi, 2));
  console.log("✅ Box-projection UV olusturuldu:", sayi + " vertex");
}

function parcalariAyikla(model) {
  modelParcalariniSifirla();
  const modelKutu = new THREE.Box3().setFromObject(model);

  model.traverse((nesne) => {
    if (!nesne.isMesh) {
      return;
    }

    materyalGuvenliKopya(nesne);
    nesne.castShadow = true;
    nesne.receiveShadow = true;

    const kategori = modelParcasiniSiniflandir(nesne, modelKutu);
    sahneDurumu.modelParcalari[kategori].push(nesne);
  });

  model.traverse((nesne) => {
    if (!nesne.isMesh) {
      return;
    }

    const adYolu = isimNormallestir(nesneAdYolunuAl(nesne));
    if (adYolu.includes("ayna") || adYolu.includes("mirror")) {
      parcayiTumListelerdenCikar(nesne);
      sahneDurumu.modelParcalari.ayna.push(nesne);
    }
  });

  // UV'si olmayan mesh'lere box-projection UV olustur
  model.traverse(function(nesne) {
    if (!nesne.isMesh) return;
    var geo = nesne.geometry;
    if (geo && !geo.attributes.uv) {
      console.log("🔧 UV yok, box-projection UV olusturuluyor:", nesne.name, "vertex:", geo.attributes.position?.count);
      kutuUVOlustur(geo);
    } else if (geo && geo.attributes.uv) {
      console.log("ℹ️ UV mevcut korunuyor:", nesne.name, "UV count:", geo.attributes.uv.count);
    }
  });
}

function gltfIslendiktenSonra(gltf, kaynakEtiketi) {
  console.log("✅ Model başarıyla yüklendi!", gltf);
  if (sahneDurumu.modelKoku) {
    sahneDurumu.sahne.remove(sahneDurumu.modelKoku);
  }

  const model = gltf.scene;
  model.rotation.y = Math.PI / 10;
  modeliOrtalaVeOlcekle(model);
  sahneDurumu.sahne.add(model);
  sahneDurumu.modelKoku = model;

  parcalariAyikla(model);
  kapakPivotlariniHazirla();

  // Model katalog verisinden parça bilgilerini al ve UI'ı güncelle
  if (typeof MODEL_KATALOG_VERI !== "undefined" && aktifModelId) {
    var katalogModel = MODEL_KATALOG_VERI.modeller.find(function(m) { return m.id === aktifModelId; });
    if (katalogModel && typeof kontrolPaneliGuncelle === "function") {
      kontrolPaneliGuncelle(katalogModel);
    }
  }

  renkleriUygula();
  // gercekAynaKaplamalariniKur(); // kapalı: dikdörtgen plane izi/kare oluşturuyordu
  kamerayiModeleSigdir(model);

  // Model değişimi sonrası toggle durumunu koru
  modelRenkKaplamaArayuzunuGuncelle();

  const meshler = [];
  const malzemeler = new Set();
  model.traverse((nesne) => {
    if (nesne.isMesh) {
      meshler.push(nesne);
      if (Array.isArray(nesne.material)) {
        nesne.material.forEach(m => malzemeler.add(m.name));
      } else if (nesne.material) {
        malzemeler.add(nesne.material.name);
      }
      console.log(`Mesh: "${nesne.name}", Material:`, nesne.material?.name || "(unnamed)");
    }
  });
  console.log("🔍 GLB Malzemeleri:", Array.from(malzemeler));

  const kapakSayisi = sahneDurumu.modelParcalari.kapak1.length + sahneDurumu.modelParcalari.kapak2.length +
                      sahneDurumu.modelParcalari.kapak3.length + sahneDurumu.modelParcalari.kapak4.length;
  console.log(`✅ ${kaynakEtiketi} yüklendi. Mesh: ${meshler.length}, Kapaklar: ${kapakSayisi}`);

  // LED ışığını oluştur
  if (sahneDurumu.modelParcalari.led.length > 0) {
    const ledMeshler = sahneDurumu.modelParcalari.led;
    const ledKutu = new THREE.Box3();
    ledMeshler.forEach(m => ledKutu.expandByObject(m));
    const ledMerkez = ledKutu.getCenter(new THREE.Vector3());

    if (sahneDurumu.ledIsigi) {
      sahneDurumu.sahne.remove(sahneDurumu.ledIsigi);
    }

    const ledLight = new THREE.PointLight(0xffd36a, 0.14, 8);
    ledLight.position.copy(ledMerkez);
    sahneDurumu.sahne.add(ledLight);
    sahneDurumu.ledIsigi = ledLight;
  }

  // Bekleyen ürün konfigürasyonu varsa uygula
  if (bekleyenUrunKonfigurasyonu) {
    var pending = bekleyenUrunKonfigurasyonu;
    bekleyenUrunKonfigurasyonu = null;
    urunKonfigurasyonunuUygula(pending);
  }

  // Model değişiminde parça seçim listener'ını bir kez kur
  modelParcasiSeciminiKur();

  yuklemeKatmaniniAyarla(false);
}

function modeliYukleAdresten(adres, kaynakEtiketi) {
  console.log("🔴 Model yükleme başladı:", adres);
  yuklemeKatmaniniAyarla(true);

  const loader = new THREE.GLTFLoader();

  loader.load(
    adres,
    (gltf) => gltfIslendiktenSonra(gltf, kaynakEtiketi),
    (ilerleme) => {
      console.log(`📊 Yükleme ilerleme: ${(ilerleme.loaded / ilerleme.total * 100).toFixed(0)}%`);
    },
    (hata) => {
      console.error("❌ Model yükleme hatası:", hata);
      yuklemeKatmaniniAyarla(false);
    }
  );
}

function base64ArrayBufferineCevir(base64) {
  const ikili = atob(base64);
  const uzunluk = ikili.length;
  const bayt = new Uint8Array(uzunluk);
  for (let i = 0; i < uzunluk; i++) {
    bayt[i] = ikili.charCodeAt(i);
  }
  return bayt.buffer;
}

function modeliYukleArrayBufferden(arrayBuffer, kaynakEtiketi) {
  console.log("🔴 Gömülü model verisi yükleniyor:", kaynakEtiketi);
  yuklemeKatmaniniAyarla(true);

  const loader = new THREE.GLTFLoader();
  loader.parse(
    arrayBuffer,
    "",
    (gltf) => gltfIslendiktenSonra(gltf, kaynakEtiketi),
    (hata) => {
      console.error("❌ Model parse hatası:", hata);
      yuklemeKatmaniniAyarla(false);
    }
  );
}

// ═══ DOKU SEÇİCİ EVENT LİSTENERS ═══════════════════════════
document.querySelectorAll('.doku-secici').forEach(function(grup) {
  var hedef = grup.dataset.hedef;
  grup.querySelectorAll('button').forEach(function(buton) {
    buton.addEventListener('click', function() {
      grup.querySelectorAll('button').forEach(function(b) { b.classList.remove('aktif'); });
      buton.classList.add('aktif');
      dokuSec(hedef, buton.dataset.deger);
    });
  });
});

// Doku ayar slider'lari
var dokuAyarlari = [
  { id: "govdeDokuOffsetX", alan: "govdeDokuOffsetX" },
  { id: "govdeDokuOffsetY", alan: "govdeDokuOffsetY" },
  { id: "govdeDokuRepeat", alan: "govdeDokuRepeat" },
  { id: "govdeDokuRotation", alan: "govdeDokuRotation" },
  { id: "kapakDokuOffsetX", alan: "kapakDokuOffsetX" },
  { id: "kapakDokuOffsetY", alan: "kapakDokuOffsetY" },
  { id: "kapakDokuRepeat", alan: "kapakDokuRepeat" },
  { id: "kapakDokuRotation", alan: "kapakDokuRotation" },
  { id: "tezgahDokuOffsetX", alan: "tezgahDokuOffsetX" },
  { id: "tezgahDokuOffsetY", alan: "tezgahDokuOffsetY" },
  { id: "tezgahDokuRepeat", alan: "tezgahDokuRepeat" },
  { id: "tezgahDokuRotation", alan: "tezgahDokuRotation" }
];
dokuAyarlari.forEach(function(kayit) {
  var el = document.getElementById(kayit.id);
  var degerEl = document.getElementById(kayit.id + "_deger");
  
  // Slider → sayisal giris senkronizasyonu
  if (el) {
    el.addEventListener("input", function() {
      sahneDurumu.dokular[kayit.alan] = parseFloat(el.value);
      if (degerEl) degerEl.value = el.value;
      if (kayit.alan.startsWith("govde")) {
        dokuAyarlariniGuncelle("govde");
      } else if (kayit.alan.startsWith("kapak")) {
        ["kapak1","kapak2","kapak3","kapak4","kapaklar"].forEach(function(p) { dokuAyarlariniGuncelle(p); });
      } else if (kayit.alan.startsWith("tezgah")) {
        dokuAyarlariniGuncelle("icAltTabla");
      }
    });
  }
  
  // Sayisal giris → slider senkronizasyonu
  if (degerEl) {
    degerEl.addEventListener("input", function() {
      var deger = parseFloat(degerEl.value);
      if (isNaN(deger)) return;
      sahneDurumu.dokular[kayit.alan] = deger;
      if (el) el.value = deger;
      if (kayit.alan.startsWith("govde")) {
        dokuAyarlariniGuncelle("govde");
      } else if (kayit.alan.startsWith("kapak")) {
        ["kapak1","kapak2","kapak3","kapak4","kapaklar"].forEach(function(p) { dokuAyarlariniGuncelle(p); });
      } else if (kayit.alan.startsWith("tezgah")) {
        dokuAyarlariniGuncelle("icAltTabla");
      }
    });
    // Sayisal giristen cikinca (blur) degeri sinirla
    degerEl.addEventListener("blur", function() {
      var deger = parseFloat(degerEl.value);
      var min = parseFloat(el?.min || degerEl.min);
      var max = parseFloat(el?.max || degerEl.max);
      if (isNaN(deger)) deger = parseFloat(el?.value || 0);
      deger = Math.max(min, Math.min(max, deger));
      degerEl.value = deger;
      if (el) el.value = deger;
      sahneDurumu.dokular[kayit.alan] = deger;
      if (kayit.alan.startsWith("govde")) {
        dokuAyarlariniGuncelle("govde");
      } else if (kayit.alan.startsWith("kapak")) {
        ["kapak1","kapak2","kapak3","kapak4","kapaklar"].forEach(function(p) { dokuAyarlariniGuncelle(p); });
      } else if (kayit.alan.startsWith("tezgah")) {
        dokuAyarlariniGuncelle("icAltTabla");
      }
    });
  }
});

// ═══ MODEL SEÇİCİ SİSTEMİ ═══════════════════════════════════
let aktifModelId = null;
let bekleyenUrunKonfigurasyonu = null;

function modelSeciciyiHazirla() {
  const kutu = document.getElementById("modelKartKutusu");
  if (!kutu || typeof MODEL_KATALOGU === "undefined") {
    console.warn("⚠️ Model kataloğu bulunamadı");
    return;
  }

  MODEL_KATALOGU.forEach((model) => {
    const kart = document.createElement("button");
    kart.className = "model-kart" + (model.varsayilan ? " aktif" : "");
    kart.dataset.modelId = model.id;
    kart.innerHTML =
      '<span class="model-kart-renk" style="background:' + model.renk + '"></span>' +
      '<span class="model-kart-ad">' + model.ad + '</span>' +
      '<span class="model-kart-aciklama">' + model.aciklama + '</span>';
    kart.addEventListener("click", function () { modelDegistir(model.id); });
    kutu.appendChild(kart);
  });

  console.log("✅ Model seçici hazır — " + MODEL_KATALOGU.length + " model");
}

function modelDegistir(modelId) {
  var model = MODEL_KATALOGU.find(function (m) { return m.id === modelId; });
  if (!model) {
    console.warn("⚠️ Model bulunamadı:", modelId);
    return;
  }

  aktifModelId = modelId;

  // Kart aktifliğini güncelle
  document.querySelectorAll(".model-kart").forEach(function (k) { k.classList.remove("aktif"); });
  var aktifKart = document.querySelector('.model-kart[data-model-id="' + modelId + '"]');
  if (aktifKart) aktifKart.classList.add("aktif");

  // ═══ YENİ: Model katalog verisinden parça bilgilerini al ═══
  if (typeof MODEL_KATALOG_VERI !== "undefined" && MODEL_KATALOG_VERI.modeller) {
    var katalogModel = MODEL_KATALOG_VERI.modeller.find(function(m) { return m.id === modelId; });
    if (katalogModel) {
      // UI kontrollerini bu modelin parçalarına göre güncelle
      kontrolPaneliGuncelle(katalogModel);
    }
  }

  console.log("🔄 Model değişiyor: " + model.ad);
  yuklemeKatmaniniAyarla(true);
  modeliYukleAdresten(model.dosya, model.ad);
}

// ═══════════════════════════════════════════════════════════════
// KONTROL PANELİ DİNAMİK GÜNCELLEME FONKSİYONLARI
// MODEL_KATALOG_VERI entegrasyonu — model değişince renk/malzeme seçenekleri güncellenir
// ═══════════════════════════════════════════════════════════════

// Seçilen modelin katalogdaki parça tanımlarına göre UI kontrollerini dinamik güncelle
function kontrolPaneliGuncelle(katalogModel) {
  if (typeof MODEL_KATALOG_VERI === "undefined") return;
  
  var renkKatalogu = MODEL_KATALOG_VERI.renkKatalogu || [];
  var parcaTipleri = MODEL_KATALOG_VERI.parcaTipleri || {};
  var malzemeTipleri = MODEL_KATALOG_VERI.malzemeTipleri || {};
  var urunTemalari = MODEL_KATALOG_VERI.urunTemalari || [];
  
  // Modelin parçalarını tara
  var modelParcaTipleri = {};
  katalogModel.parcalar.forEach(function(parca) {
    // Aynı parça tipinden birden fazla varsa ilkini sakla (malzeme kısıtı için yeterli)
    if (!modelParcaTipleri[parca.parcaTipi]) {
      modelParcaTipleri[parca.parcaTipi] = parca;
    }
  });
  
  // Parça grubu güncelleme: (parcaTipi, renkHedefId, malzemeHedefId, ozelKontrol)
  // ozelKontrol: null = normal, "led" = LED özel, "tezgah" = tezgah özel
  var parcaGrubuYapilandirma = {
    "govde":    { renk: "govdeRenk",    malzeme: "govdeMalzeme" },
    "kapak":    { renk: "kapakRenk",    malzeme: "kapakMalzeme" },
    "kulp":     { renk: null,           malzeme: "kulpKaplama", kaplama: true },
    "musluk":   { renk: null,           malzeme: "muslukKaplama", kaplama: true },
    "lavabo":   { renk: "lavaboRenk",   malzeme: null },
    "ustTabla": { renk: "ustTablaRenk", malzeme: null },
    "ayna":     { renk: null,           malzeme: null, ozel: "ayna" },
    "led":      { renk: null,           malzeme: null, ozel: "led" },
    "metalAksam": { renk: null,         malzeme: null },
    "ayak":     { renk: null,           malzeme: null },
    "yanDolap": { renk: null,           malzeme: null },
    "icTabla":  { renk: null,           malzeme: null },
    "bilinmeyen": { renk: null,         malzeme: null, gizli: true }
  };
  
  Object.keys(parcaGrubuYapilandirma).forEach(function(parcaTipi) {
    var config = parcaGrubuYapilandirma[parcaTipi];
    var modelParca = modelParcaTipleri[parcaTipi];
    var parcaTipiVeri = parcaTipleri[parcaTipi];
    
    // Renk swatch grubu
    if (config.renk) {
      var renkGrubu = document.querySelector('[data-hedef="' + config.renk + '"]');
      var renkBolumu = renkGrubu ? renkGrubu.closest('.kontrol-grubu') : null;
      
      if (modelParca && modelParca.renklenebilirMi !== false) {
        // Parça var ve renklenebilir → göster
        if (renkBolumu) renkBolumu.style.display = "";
        renkSwatchGuncelle(renkGrubu, renkKatalogu, config.renk);
      } else {
        // Parça yok veya renklenemez → gizle
        if (renkBolumu) renkBolumu.style.display = "none";
      }
    }
    
    // Malzeme buton grubu
    if (config.malzeme) {
      var malzemeGrubu = document.querySelector('[data-hefed="' + config.malzeme + '"]');
      if (!malzemeGrubu) malzemeGrubu = document.querySelector('[data-hedef="' + config.malzeme + '"]');
      var malzemeBolumu = malzemeGrubu ? malzemeGrubu.closest('.kontrol-grubu') : null;
      
      if (modelParca && modelParca.malzemeDegisebilirMi !== false) {
        // Parça var ve malzeme değişebilir → göster
        if (malzemeBolumu) malzemeBolumu.style.display = "";
        
        // ÖNCE parçanın kendi malzemeKisiti'na bak, YOKSA parça tipininkine bak
        var kisit = modelParca.malzemeKisiti || (parcaTipiVeri ? parcaTipiVeri.malzemeKisiti : null);
        malzemeButonGuncelle(malzemeGrubu, kisit, config.malzeme);
      } else {
        // Parça yok → gizle
        if (malzemeBolumu) malzemeBolumu.style.display = "none";
        
        // EĞER renk grubu da yoksa ve bu bölüm sadece malzemeden oluşuyorsa
        if (!config.renk && malzemeBolumu) {
          malzemeBolumu.style.display = "none";
        }
      }
    }
    
    // Özel durum: LED checkbox grubu
    if (config.ozel === "led") {
      var ledCheckbox = document.getElementById("ledIsik");
      var ledBolumu = ledCheckbox ? ledCheckbox.closest('.kontrol-grubu') : null;
      if (ledBolumu) {
        ledBolumu.style.display = modelParca ? "" : "none";
      }
    }
    
    // Özel durum: Ayna (sadece etiket göster, kontrol yok)
    if (config.ozel === "ayna") {
      // Ayna için özel kontrol yok, sadece varlığını not et
    }
  });
  
  // === TEZGAH MALZEME (özel durum - üstTabla varsa tezgah grubunu da göster) ===
  var ustTablaVar = modelParcaTipleri["ustTabla"];
  var tezgahGrubu = document.querySelector('[data-hedef="tezgahMalzeme"]');
  var tezgahBolumu = tezgahGrubu ? tezgahGrubu.closest('.kontrol-grubu') : null;
  if (tezgahBolumu) {
    tezgahBolumu.style.display = ustTablaVar ? "" : "none";
  }
  if (ustTablaVar && tezgahGrubu) {
    // Tezgah malzeme: mermer, kompozit
    malzemeButonGuncelle(tezgahGrubu, ["mermer", "kompozit"], "tezgahMalzeme");
  }
  
  // === İLK TEMAYI UYGULA ===
  if (urunTemalari.length > 0) {
    var varsayilanTema = urunTemalari[0];
    temaVerisindenUygula(varsayilanTema);
  }

  // Modelde olan ama UI kontrolü olmayan parçaları log'la
  var uiOlmayanParcalar = ["metalAksam", "yanDolap", "ayak", "icTabla", "bilinmeyen"];
  uiOlmayanParcalar.forEach(function(pt) {
    if (modelParcaTipleri[pt]) {
      console.log("ℹ️ Modelde '" + pt + "' parçası var ama UI kontrolü henüz yok — mesh: " + modelParcaTipleri[pt].meshAdi);
    }
  });

  console.log("📋 Kontrol paneli güncellendi: " + katalogModel.ad + 
    " - Parçalar: " + Object.keys(modelParcaTipleri).join(", "));
}

// Renk swatch butonlarını katalogdaki renklere göre güncelle
function renkSwatchGuncelle(grupElement, renkKatalogu, hedefId, zorlaIlkSec) {
  if (!grupElement) return;
  grupElement.innerHTML = "";

  var hedefInput = document.getElementById(hedefId);
  var aktifDeger = hedefInput ? hedefInput.value : "";
  var mevcutVarMi = renkKatalogu.some(function(renk) { return aktifDeger === (renk.kod + "|" + renk.hex); });
  var secilecekDeger = aktifDeger;
  if (zorlaIlkSec || !aktifDeger || !mevcutVarMi) {
    secilecekDeger = renkKatalogu.length > 0 ? (renkKatalogu[0].kod + "|" + renkKatalogu[0].hex) : "";
  }

  renkKatalogu.forEach(function(renk) {
    var buton = document.createElement("button");
    var deger = renk.kod + "|" + renk.hex;
    buton.className = "renk-ornek";
    buton.style.background = renk.hex;
    buton.dataset.deger = deger;
    buton.title = renk.kod + " " + renk.ad;
    if (deger === secilecekDeger) buton.classList.add("aktif");

    buton.addEventListener("click", function() {
      grupElement.querySelectorAll("button").forEach(function(b) { b.classList.remove("aktif"); });
      buton.classList.add("aktif");
      if (hedefInput) hedefInput.value = buton.dataset.deger;
      malzemeyeGoreRenkPaletiniGuncelle(hedefId);
      renkleriUygula();
    });

    grupElement.appendChild(buton);
  });

  if (hedefInput && secilecekDeger) {
    hedefInput.value = secilecekDeger;
  }
}

function malzemeKodunuAl(deger) {
  return (deger || "").split("|")[0];
}

function malzemeyeGoreRenkKataloguAl(malzemeKodu, varsayilanRenkKatalogu) {
  var ral = varsayilanRenkKatalogu || (typeof MODEL_KATALOG_VERI !== "undefined" ? (MODEL_KATALOG_VERI.renkKatalogu || []) : []);
  var kataloglar = {
    mdf: [
      { kod: "MDF-KREM", ad: "Mat Krem MDF", hex: "#E7DDC6" },
      { kod: "MDF-BEYAZ", ad: "Mat Beyaz MDF", hex: "#EEE9DD" },
      { kod: "MDF-TAS", ad: "Mat Taş Gri MDF", hex: "#8F8B82" },
      { kod: "MDF-CEVIZ", ad: "Ceviz MDF", hex: "#6A432E" },
      { kod: "MDF-MESE", ad: "Meşe MDF", hex: "#B28A5F" }
    ],
    lakeboya: ral,
    lakeboyaMat: ral,
    cam: [
      { kod: "CAM-SEFFAF", ad: "Şeffaf Cam", hex: "#D8EEE9" },
      { kod: "CAM-BUZLU", ad: "Buzlu Cam", hex: "#C9D6D3" },
      { kod: "CAM-FUME", ad: "Füme Cam", hex: "#687273" },
      { kod: "CAM-BRONZ", ad: "Bronz Cam", hex: "#8A6A4A" }
    ],
    mermer: [
      { kod: "MERMER-BEYAZ", ad: "Beyaz Mermer", hex: "#E5E1DA" },
      { kod: "MERMER-GRI", ad: "Gri Mermer", hex: "#AAA59C" },
      { kod: "MERMER-SIYAH", ad: "Siyah Mermer", hex: "#1D1D1D" }
    ],
    kompozit: [
      { kod: "KOMPOZIT-KIRIK-BEYAZ", ad: "Kırık Beyaz Kompozit", hex: "#E8E4DE" },
      { kod: "KOMPOZIT-GRI", ad: "Gri Kompozit", hex: "#A9AAA5" }
    ]
  };
  return kataloglar[malzemeKodu] || ral;
}

function malzemeyeGoreRenkPaletiniGuncelle(malzemeHedefId) {
  var esleme = {
    govdeMalzeme: "govdeRenk",
    kapakMalzeme: "kapakRenk",
    tezgahMalzeme: "ustTablaRenk"
  };
  var renkHedefId = esleme[malzemeHedefId];
  if (!renkHedefId) return;

  var malzemeInput = document.getElementById(malzemeHedefId);
  var renkGrubu = document.querySelector('[data-hedef="' + renkHedefId + '"]');
  if (!malzemeInput || !renkGrubu) return;

  var malzemeKodu = malzemeKodunuAl(malzemeInput.value);
  var renkKatalogu = malzemeyeGoreRenkKataloguAl(malzemeKodu, MODEL_KATALOG_VERI.renkKatalogu || []);
  renkSwatchGuncelle(renkGrubu, renkKatalogu, renkHedefId, true);
}
// Malzeme butonlarını katalogdaki malzeme kısıtlarına göre güncelle
function malzemeButonGuncelle(grupElement, malzemeKisiti, hedefId) {
  if (!grupElement) return;
  
  grupElement.innerHTML = "";
  
  var hedefInput = document.getElementById(hedefId);

  // Musluk/Kulp gerçek kaplama seçenekleri: malzeme + renk birlikte gelir
  if (hedefId === "muslukKaplama" || hedefId === "kulpKaplama") {
    var kaplamaSecenekleri = hedefId === "muslukKaplama" ? [
      { etiket: "Krom Parlak", deger: "krom|#d8d5ce" },
      { etiket: "Krom Mat", deger: "kromMat|#b7b2a8" },
      { etiket: "Siyah Metal Mat", deger: "metal|#202020" },
      { etiket: "Saten Metal", deger: "metalMat|#8f8a80" },
      { etiket: "Pirinç / Gold", deger: "pirinc|#b08d45" }
    ] : [
      { etiket: "Krom Parlak", deger: "krom|#d8d5ce" },
      { etiket: "Krom Mat", deger: "kromMat|#b7b2a8" },
      { etiket: "Siyah Metal", deger: "metal|#202020" },
      { etiket: "Saten Metal", deger: "metalMat|#8f8a80" },
      { etiket: "Plastik Beyaz", deger: "plastik|#f2f0ea" },
      { etiket: "Plastik Siyah", deger: "plastik|#1c1c1c" },
      { etiket: "Endüstriyel Boya Antrasit", deger: "endustriyelBoya|#30363a" },
      { etiket: "Endüstriyel Boya Krem", deger: "endustriyelBoya|#e8e3d7" }
    ];

    kaplamaSecenekleri.forEach(function(secenek, indeks) {
      var buton = document.createElement("button");
      buton.className = "malzeme-ornek";
      if (indeks === 0) buton.classList.add("aktif");
      buton.dataset.deger = secenek.deger;
      buton.textContent = secenek.etiket;
      buton.addEventListener("click", function() {
        grupElement.querySelectorAll("button").forEach(function(b) { b.classList.remove("aktif"); });
        buton.classList.add("aktif");
        if (hedefInput) hedefInput.value = buton.dataset.deger;
        renkleriUygula();
      });
      grupElement.appendChild(buton);
    });

    if (hedefInput && kaplamaSecenekleri.length > 0) {
      hedefInput.value = kaplamaSecenekleri[0].deger;
    }
    return;
  }
  
  // Tüm malzeme tiplerini MODEL_KATALOG_VERI'den al
  var tumMalzemeler = MODEL_KATALOG_VERI.malzemeTipleri || {};
  
  // Gösterilecek malzeme listesini belirle
  var gosterilecekler = malzemeKisiti ? malzemeKisiti : Object.keys(tumMalzemeler);
  if (hedefId === "muslukKaplama") {
    gosterilecekler = gosterilecekler.filter(function(m) { return m === "krom" || m === "metal"; });
  }
  
var ilkDeger = "";
  
  gosterilecekler.forEach(function(malzemeKodu, indeks) {
    var malzeme = tumMalzemeler[malzemeKodu];
    if (!malzeme) return;
    
    var buton = document.createElement("button");
    buton.className = "malzeme-ornek";
    if (indeks === 0) {
      buton.classList.add("aktif");
      ilkDeger = malzemeKodu;
    }
    buton.dataset.deger = malzemeKodu;
    var etiket = malzemeKodu;
    // CamelCase'i boşluklu yap: "lakeboyaMat" -> "Lakeboya Mat"
    etiket = etiket.charAt(0).toUpperCase() + etiket.slice(1);
    etiket = etiket.replace(/([a-z])([A-Z])/g, '$1 $2');
    // Özel Türkçe etiketler
    var etiketHaritasi = {
      "krom": "Krom (Parlak)",
      "metal": "Metal (Mat)",
      "plastik": "Plastik",
      "mdf": "MDF (Mat)",
      "cam": "Cam (Şeffaf)",
      "ayna": "Ayna",
      "porselen": "Porselen",
      "lakeboya": "Lake Boya (Parlak)",
      "lakeboyaMat": "Lake Boya (Mat)",
      "mermer": "Mermer",
      "kompozit": "Kompozit Kuvars"
    };
    buton.textContent = etiketHaritasi[malzemeKodu] || etiket;
    
    // Kaplama tipleri için özel etiketleme
    if (hedefId === "kulpKaplama" || hedefId === "muslukKaplama") {
      // Kaplama tipleri için renk bilgisini de içeren değer ata
      var varsayilanRenk = malzeme.varsayilanRenk || "#e8e8e8";
      buton.dataset.deger = malzemeKodu + "|" + varsayilanRenk;
    }
    
    buton.addEventListener("click", function() {
      grupElement.querySelectorAll("button").forEach(function(b) { b.classList.remove("aktif"); });
      buton.classList.add("aktif");
      if (hedefInput) hedefInput.value = buton.dataset.deger;
      malzemeyeGoreRenkPaletiniGuncelle(hedefId);
      renkleriUygula();
    });
    
    grupElement.appendChild(buton);
  });
  
  if (hedefInput && ilkDeger) {
    // Kaplama tipleri için ilk değeri renk koduyla birlikte ata
    if (hedefId === "kulpKaplama" || hedefId === "muslukKaplama") {
      var ilkMalzeme = tumMalzemeler[ilkDeger];
      var ilkRenk = ilkMalzeme ? ilkMalzeme.varsayilanRenk : "#e8e8e8";
      hedefInput.value = ilkDeger + "|" + ilkRenk;
    } else {
      hedefInput.value = ilkDeger;
    }
    malzemeyeGoreRenkPaletiniGuncelle(hedefId);
  }
}

// Katalogdaki ürün temasını UI kontrollerine uygula
function temaVerisindenUygula(tema) {
  if (!tema) return;
  
  // Renk ve malzeme eşleştirmesi: tema.gövde = "RAL 9001|#E9E0CB|mdf" formatında
  var parcaEsleme = {
    "govde": ["govdeRenk", "govdeMalzeme"],
    "kapak": ["kapakRenk", "kapakMalzeme"],
    "kulp": ["kulpKaplama"],
    "musluk": ["muslukKaplama"],
    "lavabo": ["lavaboRenk"],
    "ustTabla": ["ustTablaRenk"],
    "tezgah": ["tezgahMalzeme"]
  };
  
  Object.keys(parcaEsleme).forEach(function(parcaAdi) {
    var temaDegeri = tema[parcaAdi];
    if (!temaDegeri) return;
    
    var hedefler = parcaEsleme[parcaAdi];
    hedefler.forEach(function(hedefId) {
      var input = document.getElementById(hedefId);
      if (!input) return;
      
      // Tema değerini parçala: "RAL 9001|#E9E0CB|mdf" → parçalar
      var parcalar = temaDegeri.split("|");
      
      if (hedefId.endsWith("Renk")) {
        // Renk: ilk iki parça (RAL kodu + hex)
        if (parcalar.length >= 2) {
          input.value = parcalar[0] + "|" + parcalar[1];
        }
      } else if (hedefId.endsWith("Malzeme")) {
        // Malzeme: üçüncü parça
        if (parcalar.length >= 3) {
          input.value = parcalar[2];
        } else if (parcalar.length >= 1) {
          input.value = parcalar[0];
        }
      } else if (hedefId.endsWith("Kaplama")) {
        // Kaplama: malzeme kodu, renk katalogdan alınır
        input.value = temaDegeri; // "krom|#d4af37" formatında
      }
      
      // İlgili gruptaki aktif class'ı senkronize et
      var grup = document.querySelector('[data-hedef="' + hedefId + '"]');
      if (grup) {
        grup.querySelectorAll("button").forEach(function(buton) {
          buton.classList.toggle("aktif", buton.dataset.deger === input.value);
        });
      }
    });
  });

  malzemeyeGoreRenkPaletiniGuncelle("govdeMalzeme");
  malzemeyeGoreRenkPaletiniGuncelle("kapakMalzeme");
  malzemeyeGoreRenkPaletiniGuncelle("tezgahMalzeme");
}


// ═══════════════════════════════════════════════════════════════
// KATLANABİLİR BÖLÜM BAŞLIKLARI
// ═══════════════════════════════════════════════════════════════

function bolumleriKatlanabilirYap() {
  var panel = document.querySelector(".urun-panel");
  if (!panel) return;

  // Sadece h3 içeren .kontrol-grubu ve .urun-galeri gruplarını katlanabilir yap
  var h3Gruplari = panel.querySelectorAll(".kontrol-grubu h3, .urun-galeri h3");
  h3Gruplari.forEach(function(h3) {
    var grup = h3.closest(".kontrol-grubu") || h3.closest(".urun-galeri");
    if (!grup || grup.querySelector(".bolum-baslik")) return; // zaten yapıldıysa atla

    var baslikMetni = h3.textContent;
    var buton = document.createElement("button");
    buton.className = "bolum-baslik";
    buton.type = "button";
    buton.setAttribute("aria-expanded", "true");
    buton.textContent = baslikMetni;

    // h3'ü buton ile değiştir
    h3.replaceWith(buton);

    buton.addEventListener("click", function() {
      var kapali = grup.classList.toggle("kapali");
      buton.setAttribute("aria-expanded", kapali ? "false" : "true");
    });
  });

  // ═══ BAŞLANGIÇ DURUMU ═══
  // Sahne/Render Ayarları → kapalı
  var sahnePaneli = panel.querySelector(".sahne-ayar-paneli");
  if (sahnePaneli) {
    sahnePaneli.classList.add("kapali");
    var sahneButon = sahnePaneli.querySelector(".bolum-baslik");
    if (sahneButon) sahneButon.setAttribute("aria-expanded", "false");
  }

  // Tüm data-model-ayari grupları → kapalı
  var modelGruplari = panel.querySelectorAll("[data-model-ayari]");
  modelGruplari.forEach(function(g) {
    g.classList.add("kapali");
    var b = g.querySelector(".bolum-baslik");
    if (b) b.setAttribute("aria-expanded", "false");
  });

  // Model Seç → açık (varsayılan)
  // modelRenkKaplamaAktif grubu açık (toggle bölümü, h3 yok zaten)

  console.log("✅ Katlanabilir bölüm başlıkları hazır");
}

// ═══════════════════════════════════════════════════════════════
// MODEL RENK/KAPLAMA TOGGLE DAVRANIŞI
// ═══════════════════════════════════════════════════════════════

function modelRenkKaplamaArayuzunuGuncelle() {
  var aktif = !!sahneDurumu.renkKaplamaAktif;
  var modelGruplari = document.querySelectorAll("[data-model-ayari]");

  modelGruplari.forEach(function(grup) {
    if (aktif) {
      grup.classList.remove("model-ayarlari-pasif");
    } else {
      grup.classList.add("model-ayarlari-pasif");
    }

    // Etkileşimli kontrolleri bul ve pasif/aktif yap
    var kontroller = grup.querySelectorAll("button, select, input");
    kontroller.forEach(function(kontrol) {
      // Hidden input'ları disable etme
      if (kontrol.type === "hidden") return;
      // Bölüm başlıkları (accordion) her zaman aktif kalsın — toggle kapalıyken
      // kullanıcı bölümü açıp ayarları görebilsin, yalnız içteki kontroller disabled olsun
      if (kontrol.classList && kontrol.classList.contains("bolum-baslik")) return;

      if (aktif) {
        kontrol.disabled = false;
      } else {
        kontrol.disabled = true;
      }
    });
  });
}

function modeliYukle() {
  // Katalogtan ilk modeli yükle
  if (typeof MODEL_KATALOGU !== "undefined" && MODEL_KATALOGU.length > 0) {
    var ilkModel = MODEL_KATALOGU.find(function (m) { return m.varsayilan; }) || MODEL_KATALOGU[0];
    console.log("📦 Katalogtan ilk model yükleniyor: " + ilkModel.ad);
    aktifModelId = ilkModel.id;
    modeliYukleAdresten(ilkModel.dosya, ilkModel.ad);
    return;
  }

  // Fallback: base64
  if (typeof FUGA_MODEL_BASE64 !== "undefined" && FUGA_MODEL_BASE64) {
    console.log("📦 Fallback: base64 model kullanılıyor");
    var arrayBuffer = base64ArrayBufferineCevir(FUGA_MODEL_BASE64);
    modeliYukleArrayBufferden(arrayBuffer, "FUGA 2 DOLAP.glb");
    return;
  }

  console.warn("⚠️ Hiçbir model kaynağı bulunamadı");
  yuklemeKatmaniniAyarla(false);
}

function secilenDosyayiYukle() {
  const dosya = elemanlar.glbDosyaSec.files?.[0];
  if (!dosya) {
    return;
  }

  if (sahneDurumu.etkinNesneAdresi) {
    URL.revokeObjectURL(sahneDurumu.etkinNesneAdresi);
  }

  const nesneAdresi = URL.createObjectURL(dosya);
  sahneDurumu.etkinNesneAdresi = nesneAdresi;
  modeliYukleAdresten(nesneAdresi, dosya.name);
}

function canlandir() {
  requestAnimationFrame(canlandir);

  if (sahneDurumu.kontroller) {
    sahneDurumu.kontroller.update();
  }

  // LED sabit yanar — yanıp sönme kapalı
  if (sahneDurumu.ledIsigi && sahneDurumu.ledAcik) {
    const ledAyar = ledAyariniAl();
    sahneDurumu.ledIsigi.intensity = ledAyar.isik;
    sahneDurumu.ledIsigi.color.set(ledAyar.renk);
    sahneDurumu.modelParcalari.led.forEach((mesh) => {
      if (mesh.material) {
        mesh.material.emissive.set(ledAyar.renk);
        mesh.material.color.set(ledAyar.renk);
        mesh.material.emissiveIntensity = ledAyar.emissive;
    }
  });

}

  if (sahneDurumu.renderer && sahneDurumu.sahne && sahneDurumu.kamera) {
    // aynaYansimasiniGuncelle(); // koyu arka planı aynaya basmasın diye kapalı
    sahneDurumu.renderer.render(sahneDurumu.sahne, sahneDurumu.kamera);
  }
}

// YENİ KONTROLLER - RENK VE MALZEME SEÇİMLERİ
// File input — GLB dosya seçimi
if (elemanlar.glbDosyaSec) {
  elemanlar.glbDosyaSec.addEventListener("change", secilenDosyayiYukle);
}

// Sahne görünüm butonları
elemanlar.sahneButonlari.forEach((buton) => {
  buton.addEventListener("click", () => {
    gorunumuAyarla(buton.dataset.gorunum);
  });
});

// Form controls — change event listeners (checkbox + eski select'ler icin)
Object.entries(elemanlar).forEach(([key, element]) => {
  // NodeList'i ve null'u skip et
  if (!element || !element.addEventListener || key === 'sahneButonlari') return;

  if (element.tagName === 'SELECT' || (element.type && element.type === 'checkbox')) {
    element.addEventListener('change', renkleriUygula);
  }
});

// Renk ve malzeme swatch butonları — her grup bağımsız çalışır
document.querySelectorAll('.renk-secici, .malzeme-secici').forEach((grup) => {
  const hedefId = grup.dataset.hedef;
  const hedefInput = document.getElementById(hedefId);
  if (!hedefInput) return;

  grup.querySelectorAll('button').forEach((buton) => {
    buton.addEventListener('click', () => {
      grup.querySelectorAll('button').forEach((b) => b.classList.remove('aktif'));
      buton.classList.add('aktif');
      hedefInput.value = buton.dataset.deger;
      renkleriUygula();
    });
  });
});

// Ürün galerisi — her kart bir kombin varyasyonu uygular
document.querySelectorAll('.galeri-kart').forEach((kart) => {
  kart.addEventListener('click', () => {
    document.querySelectorAll('.galeri-kart').forEach((k) => k.classList.remove('aktif'));
    kart.classList.add('aktif');
    temayiUygula(kart.dataset.tema);
  });
});


// ═══ SAHNE AYAR SİSTEMİ (JSON / DB UYUMLU) ═════════════════════
function derinKopya(nesne) {
  return JSON.parse(JSON.stringify(nesne));
}

function urunKonfigurasyonunuOku() {
  var hdrEl = document.getElementById("hdrOrtam");
  return {
    surum: 1,
    modelId: aktifModelId || null,
    renkler: {
      govdeRenk: elemanlar.govdeRenk?.value || "",
      kapakRenk: elemanlar.kapakRenk?.value || "",
      lavaboRenk: elemanlar.lavaboRenk?.value || "",
      ustTablaRenk: elemanlar.ustTablaRenk?.value || ""
    },
    malzemeler: {
      govdeMalzeme: elemanlar.govdeMalzeme?.value || "",
      kapakMalzeme: elemanlar.kapakMalzeme?.value || "",
      tezgahMalzeme: elemanlar.tezgahMalzeme?.value || ""
    },
    kaplamalar: {
      kulpKaplama: elemanlar.kulpKaplama?.value || "",
      muslukKaplama: elemanlar.muslukKaplama?.value || ""
    },
    led: {
      acik: elemanlar.ledIsik?.checked ?? true,
      seviye: elemanlar.ledSeviye?.value || "dusuk",
      renk: elemanlar.ledRenk?.value || "sicakSari"
    },
    dokular: derinKopya(sahneDurumu.dokular),
    hdrOrtam: (hdrEl && hdrEl.value) ? hdrEl.value : "varsayilan",
    renkKaplamaAktif: !!sahneDurumu.renkKaplamaAktif
  };
}

function sayiInputDegeri(id, varsayilan) {
  const el = document.getElementById(id);
  const deger = Number.parseFloat(el?.value);
  return Number.isFinite(deger) ? deger : varsayilan;
}

function sahneAyarFormundanOku() {
  const mevcut = derinKopya(sahneDurumu.sahneAyarlari || window.__sahneAyarFabrika || {});
  mevcut.ayarlar = mevcut.ayarlar || {};
  mevcut.ayarlar.render = mevcut.ayarlar.render || {};
  mevcut.ayarlar.camera = mevcut.ayarlar.camera || {};
  mevcut.ayarlar.lighting = mevcut.ayarlar.lighting || {};
  mevcut.ayarlar.materials = mevcut.ayarlar.materials || {};

  mevcut.ayarlar.render.exposure = sayiInputDegeri("ayarExposure", 0.56);
  mevcut.ayarlar.camera.fov = sayiInputDegeri("ayarFov", 32);
  mevcut.ayarlar.camera.fitDistanceMultiplier = sayiInputDegeri("ayarKameraMesafe", 2.28);
  mevcut.ayarlar.lighting.hemisphere = mevcut.ayarlar.lighting.hemisphere || {};
  mevcut.ayarlar.lighting.key = mevcut.ayarlar.lighting.key || {};
  mevcut.ayarlar.lighting.fill = mevcut.ayarlar.lighting.fill || {};
  mevcut.ayarlar.lighting.hemisphere.intensity = sayiInputDegeri("ayarOrtamIsik", 0.24);
  mevcut.ayarlar.lighting.key.intensity = sayiInputDegeri("ayarAnaIsik", 0.30);
  mevcut.ayarlar.lighting.fill.intensity = sayiInputDegeri("ayarDolguIsik", 0.06);
  mevcut.ayarlar.materials.mirror = mevcut.ayarlar.materials.mirror || {};
  mevcut.ayarlar.materials.mirror.envMapIntensity = sayiInputDegeri("ayarAynaYansima", 1.25);
  mevcut.ayarlar.materials.mirror.roughness = sayiInputDegeri("ayarAynaRoughness", 0.045);
  mevcut.ayarlar.materials.camOpacity = sayiInputDegeri("ayarCamOpacity", 0.82);
  mevcut.ayarlar.materials.globalEnvMapScale = sayiInputDegeri("ayarEnvScale", 1);
  mevcut.ayarlar.materials.globalClearcoatScale = sayiInputDegeri("ayarClearcoatScale", 1);
  mevcut.ayarlar.hdr = mevcut.ayarlar.hdr || {};
  mevcut.ayarlar.hdr.intensity = sayiInputDegeri("ayarHdrYogunluk", 1);
  mevcut.ayarlar.hdr.rotation = sayiInputDegeri("ayarHdrDondurme", 0);
  mevcut.ayarlar.hdr.blur = sayiInputDegeri("ayarHdrBlurluk", 0);
  mevcut.ayarlar.arkaPlan = mevcut.ayarlar.arkaPlan || {};
  mevcut.ayarlar.arkaPlan.renk = document.getElementById("ayarArkaPlanRenk")?.value || "#050505";
  mevcut.ayarlar.zemin = mevcut.ayarlar.zemin || {};
  mevcut.ayarlar.zemin.renk = document.getElementById("ayarZeminRenk")?.value || "#050505";
  mevcut.ayarlar.zemin.opaklik = sayiInputDegeri("ayarZeminOpaklik", 0.16);
  mevcut.ayarlar.golge = mevcut.ayarlar.golge || {};
  mevcut.ayarlar.golge.opaklik = sayiInputDegeri("ayarGolgeOpaklik", 0.16);
  mevcut.ayarlar.golge.boyut = sayiInputDegeri("ayarGolgeBoyut", 2048);
  mevcut.ayarlar.golge.bias = sayiInputDegeri("ayarGolgeBias", -0.0008);
  mevcut.urunKonfigurasyonu = urunKonfigurasyonunuOku();
  mevcut.guncellenmeTarihi = new Date().toISOString();
  return mevcut;
}

function sahneAyarFormunuDoldur(ayar) {
  const a = ayar?.ayarlar || {};
  const set = (id, deger) => { const el = document.getElementById(id); if (el && deger !== undefined) el.value = deger; };
  set("ayarExposure", a.render?.exposure);
  set("ayarFov", a.camera?.fov);
  set("ayarKameraMesafe", a.camera?.fitDistanceMultiplier);
  set("ayarOrtamIsik", a.lighting?.hemisphere?.intensity);
  set("ayarAnaIsik", a.lighting?.key?.intensity);
  set("ayarDolguIsik", a.lighting?.fill?.intensity);
  set("ayarAynaYansima", a.materials?.mirror?.envMapIntensity);
  set("ayarAynaRoughness", a.materials?.mirror?.roughness);
  set("ayarCamOpacity", a.materials?.camOpacity);
  set("ayarEnvScale", a.materials?.globalEnvMapScale);
  set("ayarClearcoatScale", a.materials?.globalClearcoatScale);
  set("ayarHdrYogunluk", a.hdr?.intensity);
  set("ayarHdrDondurme", a.hdr?.rotation);
  set("ayarHdrBlurluk", a.hdr?.blur);
  set("ayarArkaPlanRenk", a.arkaPlan?.renk);
  set("ayarZeminRenk", a.zemin?.renk);
  set("ayarZeminOpaklik", a.zemin?.opaklik);
  set("ayarGolgeOpaklik", a.golge?.opaklik);
  set("ayarGolgeBoyut", a.golge?.boyut);
  set("ayarGolgeBias", a.golge?.bias);
  // Sayisal giris (_deger) alanlarini da esitle
  const setDeger = (id, deger) => { const el = document.getElementById(id + "_deger"); if (el && deger !== undefined) el.value = deger; };
  setDeger("ayarExposure", a.render?.exposure);
  setDeger("ayarFov", a.camera?.fov);
  setDeger("ayarKameraMesafe", a.camera?.fitDistanceMultiplier);
  setDeger("ayarOrtamIsik", a.lighting?.hemisphere?.intensity);
  setDeger("ayarAnaIsik", a.lighting?.key?.intensity);
  setDeger("ayarDolguIsik", a.lighting?.fill?.intensity);
  setDeger("ayarAynaYansima", a.materials?.mirror?.envMapIntensity);
  setDeger("ayarAynaRoughness", a.materials?.mirror?.roughness);
  setDeger("ayarCamOpacity", a.materials?.camOpacity);
  setDeger("ayarEnvScale", a.materials?.globalEnvMapScale);
  setDeger("ayarClearcoatScale", a.materials?.globalClearcoatScale);
  setDeger("ayarHdrYogunluk", a.hdr?.intensity);
  setDeger("ayarHdrDondurme", a.hdr?.rotation);
  setDeger("ayarHdrBlurluk", a.hdr?.blur);
  setDeger("ayarZeminOpaklik", a.zemin?.opaklik);
  setDeger("ayarGolgeOpaklik", a.golge?.opaklik);
  setDeger("ayarGolgeBoyut", a.golge?.boyut);
  setDeger("ayarGolgeBias", a.golge?.bias);
}

async function urunKonfigurasyonunuUygula(konfigurasyon) {
  if (!konfigurasyon || konfigurasyon.surum !== 1) return;

  var guvenliSet = function(id, deger) {
    var el = document.getElementById(id);
    if (el && deger !== undefined && deger !== null) el.value = deger;
  };

  // --- Renkler ---
  var rnk = konfigurasyon.renkler || {};
  guvenliSet("govdeRenk", rnk.govdeRenk);
  guvenliSet("kapakRenk", rnk.kapakRenk);
  guvenliSet("lavaboRenk", rnk.lavaboRenk);
  guvenliSet("ustTablaRenk", rnk.ustTablaRenk);

  // --- Malzemeler ---
  var mlz = konfigurasyon.malzemeler || {};
  guvenliSet("govdeMalzeme", mlz.govdeMalzeme);
  guvenliSet("kapakMalzeme", mlz.kapakMalzeme);
  guvenliSet("tezgahMalzeme", mlz.tezgahMalzeme);

  // --- Kaplamalar ---
  var kpl = konfigurasyon.kaplamalar || {};
  guvenliSet("kulpKaplama", kpl.kulpKaplama);
  guvenliSet("muslukKaplama", kpl.muslukKaplama);

  // --- LED ---
  var led = konfigurasyon.led || {};
  if (elemanlar.ledIsik) elemanlar.ledIsik.checked = led.acik !== false;
  guvenliSet("ledSeviye", led.seviye);
  guvenliSet("ledRenk", led.renk);

  // --- Dokular ---
  var kayitliDokular = konfigurasyon.dokular || {};
  Object.keys(kayitliDokular).forEach(function(anahtar) {
    sahneDurumu.dokular[anahtar] = kayitliDokular[anahtar];
  });

  ['govdeDoku', 'kapakDoku', 'tezgahDoku'].forEach(function(hedef) {
    var dokuId = sahneDurumu.dokular[hedef] || 'yok';
    var dokuGrubu = document.querySelector('.doku-secici[data-hedef="' + hedef + '"]');
    if (dokuGrubu) {
      dokuGrubu.querySelectorAll('button').forEach(function(b) {
        b.classList.toggle('aktif', b.dataset.deger === dokuId);
      });
    }
    var panelEsleme = { govdeDoku: 'govdeDokuAyar', kapakDoku: 'kapakDokuAyar', tezgahDoku: 'tezgahDokuAyar' };
    var ayarPanel = document.getElementById(panelEsleme[hedef]);
    if (ayarPanel) ayarPanel.style.display = (dokuId && dokuId !== 'yok') ? '' : 'none';
  });

  // Doku slider ve _deger alanlarini state ile esitle
  var onEkler = ['govde', 'kapak', 'tezgah'];
  onEkler.forEach(function(onEk) {
    ['OffsetX', 'OffsetY', 'Repeat', 'Rotation'].forEach(function(sufiks) {
      var alanAdi = onEk + 'Doku' + sufiks;
      var deger = sahneDurumu.dokular[alanAdi];
      if (deger === undefined) return;
      var sliderEl = document.getElementById(onEk + 'Doku' + sufiks);
      var degerEl = document.getElementById(onEk + 'Doku' + sufiks + '_deger');
      if (sliderEl) sliderEl.value = deger;
      if (degerEl) degerEl.value = deger;
    });
  });

  // --- Renk/malzeme/kaplama buton gruplarinda aktif class guncelle ---
  var butonHedefleri = [
    'govdeRenk', 'kapakRenk', 'lavaboRenk', 'ustTablaRenk',
    'govdeMalzeme', 'kapakMalzeme', 'tezgahMalzeme',
    'kulpKaplama', 'muslukKaplama'
  ];
  butonHedefleri.forEach(function(hedefId) {
    var inputEl = document.getElementById(hedefId);
    if (!inputEl) return;
    var aktifDeger = inputEl.value;
    var grup = document.querySelector('[data-hedef="' + hedefId + '"]');
    if (!grup) return;
    grup.querySelectorAll('button').forEach(function(b) {
      b.classList.toggle('aktif', b.dataset.deger === aktifDeger);
    });
  });

  if (typeof malzemeyeGoreRenkPaletiniGuncelle === 'function') {
    malzemeyeGoreRenkPaletiniGuncelle('govdeMalzeme');
    malzemeyeGoreRenkPaletiniGuncelle('kapakMalzeme');
    malzemeyeGoreRenkPaletiniGuncelle('tezgahMalzeme');
  }

  // --- HDR ortam ---
  var hdrDegeri = konfigurasyon.hdrOrtam || 'varsayilan';
  guvenliSet('hdrOrtam', hdrDegeri);
  var hdrSeciciEl = document.getElementById('hdrSecici');
  if (hdrSeciciEl) {
    hdrSeciciEl.querySelectorAll('button').forEach(function(b) {
      b.classList.toggle('aktif', b.dataset.hdr === hdrDegeri);
    });
  }

  // --- Doku preload + renkleriUygula ---
  var dokuYuklemeIsleri = [];
  ['govdeDoku', 'kapakDoku', 'tezgahDoku'].forEach(function(hedef) {
    var dokuId = sahneDurumu.dokular[hedef];
    if (dokuId && dokuId !== 'yok') {
      dokuYuklemeIsleri.push(dokuYukle(dokuId));
    }
  });

  if (dokuYuklemeIsleri.length > 0) {
    await Promise.all(dokuYuklemeIsleri);
  }

  // --- Model Renk/Kaplama Toggle ---
  if (konfigurasyon.renkKaplamaAktif !== undefined) {
    sahneDurumu.renkKaplamaAktif = !!konfigurasyon.renkKaplamaAktif;
    if (elemanlar.modelRenkKaplamaAktif) {
      elemanlar.modelRenkKaplamaAktif.checked = sahneDurumu.renkKaplamaAktif;
    }
    modelRenkKaplamaArayuzunuGuncelle();
  }

  renkleriUygula();
}

function hdrOrtamDegistir(hdrId) {
  // Tum secenekler icin ayni gercek HDR dosyasini kullan
  // Farkli "hissiyat" icin color temperature ve intensity ayarlari
  var ortamAyarlari = {
    "varsayilan": { sicaklik: 0, intensity: 1, tint: "#ffffff" },
    "banyo": { sicaklik: 0.15, intensity: 1.1, tint: "#ffe8d0" },
    "studiyo": { sicaklik: 0, intensity: 1.3, tint: "#f0f0f0" },
    "dis": { sicaklik: -0.2, intensity: 0.9, tint: "#d0e8ff" }
  };
  
  var ayar = ortamAyarlari[hdrId] || ortamAyarlari["varsayilan"];
  sahneDurumu.hdrAyar = ayar;
  
  // Gercek HDR dosyasini yukle
  var hdrYolu = "./hdr/modern_bathroom_1k.hdr";
  if (!sahneDurumu._hdrDoku) {
    // Ilk kez yukleniyor
    var yukleyici = new THREE.RGBELoader();
    yukleyici.load(hdrYolu, function(doku) {
      sahneDurumu._hdrDoku = doku;
      pmremIsleVeUygula(doku, ayar);
      console.log("✅ HDR ilk kez yüklendi ve uygulandı:", hdrId);
    }, undefined, function(hata) {
      console.warn("⚠️ HDR yüklenemedi, prosedürel devam:", hata);
      prosedurelOrtamUygula(ayar);
    });
  } else {
    // Zaten yuklu, direkt uygula
    pmremIsleVeUygula(sahneDurumu._hdrDoku, ayar);
    console.log("✅ HDR ortam değiştirildi:", hdrId);
  }
}

function pmremIsleVeUygula(hdrDoku, ayar) {
  if (!sahneDurumu.sahne || !sahneDurumu.renderer) return;
  
  // Orijinal HDR'i kopyala ve renk ayari uygula
  var islenmisDoku = hdrDoku.clone();
  if (ayar.tint && ayar.tint !== "#ffffff") {
    islenmisDoku.color = new THREE.Color(ayar.tint);
  }
  islenmisDoku.needsUpdate = true;
  
  var pmremDoku;
  if (THREE.PMREMGenerator) {
    var pmremGenerator = new THREE.PMREMGenerator(sahneDurumu.renderer);
    pmremGenerator.compileEquirectangularShader();
    var pmrem = pmremGenerator.fromEquirectangular(islenmisDoku);
    pmremDoku = pmrem.texture;
    islenmisDoku.dispose();
    pmremGenerator.dispose();
  } else {
    pmremDoku = islenmisDoku;
  }
  
  // Onceki farkli PMREM texture varsa dispose et
  if (sahneDurumu.hdrPmremDoku && sahneDurumu.hdrPmremDoku !== pmremDoku) {
    sahneDurumu.hdrPmremDoku.dispose();
  }
  sahneDurumu.hdrPmremDoku = pmremDoku;
  sahneDurumu.sahne.environment = pmremDoku;
  
  // Intensity ayarini kaydet
  sahneDurumu.hdrYogunluk = ayar.intensity;
  // Materyallerin envMapIntensity'lerini guncelle (renkleriUygula degil)
  hdrAyarlariniUygula();
}

function prosedurelOrtamUygula(ayar) {
  // Fallback: sadece basit ortam isigi
  if (sahneDurumu.isiklar?.ortam) {
    sahneDurumu.isiklar.ortam.intensity = 0.24 * ayar.intensity;
  }
}

// uygulaHdrDoku — artik kullanilmiyor, pmremIsleVeUygula ile degistirildi
function uygulaHdrDoku(doku) {
  // Deprecated: pmremIsleVeUygula kullanin
}

function hdrDosyaYukle(url, ad) {
  var yukleyici = new THREE.RGBELoader();
  yukleyici.load(url, function(doku) {
    sahneDurumu._hdrDoku = doku; // ozel HDR'i da cache'le
    var ayar = sahneDurumu.hdrAyar || { sicaklik: 0, intensity: 1, tint: "#ffffff" };
    pmremIsleVeUygula(doku, ayar);
    URL.revokeObjectURL(url);
    console.log("✅ Özel HDR yüklendi:", ad);
  }, undefined, function(hata) {
    console.error("❌ Özel HDR yüklenemedi:", hata);
    URL.revokeObjectURL(url);
  });
}

function hdrAyarlariniUygula() {
  // Model kokundeki tum MeshPhysicalMaterial/MeshStandardMaterial traverse et
  // userData.hdrBazEnvMapIntensity yoksa mevcut envMapIntensity ile ilk kez sakla
  // envMapIntensity = base * (sahneDurumu.hdrYogunluk ?? 1)
  // Renderer exposure degismez
  if (!sahneDurumu.modelKoku) return;
  var yogunluk = sahneDurumu.hdrYogunluk ?? 1;
  sahneDurumu.modelKoku.traverse(function(nesne) {
    if (nesne.isMesh) {
      var materyaller = Array.isArray(nesne.material) ? nesne.material : [nesne.material];
      materyaller.forEach(function(m) {
        if (m && (m.isMeshPhysicalMaterial || m.isMeshStandardMaterial)) {
          if (m.userData.hdrBazEnvMapIntensity === undefined) {
            m.userData.hdrBazEnvMapIntensity = m.envMapIntensity;
          }
          m.envMapIntensity = m.userData.hdrBazEnvMapIntensity * yogunluk;
        }
      });
    }
  });
}

function golgeAyarlariniUygula() {
  if (sahneDurumu.isiklar?.ana?.shadow) {
    var boyut = sahneDurumu.golgeBoyut ?? 2048;
    sahneDurumu.isiklar.ana.shadow.mapSize.set(boyut, boyut);
    sahneDurumu.isiklar.ana.shadow.bias = sahneDurumu.golgeBias ?? -0.0008;
    // Map'i yeniden olustur
    if (sahneDurumu.isiklar.ana.shadow.map) {
      sahneDurumu.isiklar.ana.shadow.map.dispose();
      sahneDurumu.isiklar.ana.shadow.map = null;
    }
  }
  // Zemin golge opakligini da guncelle
  if (sahneDurumu.isiklar?.zemin?.material) {
    sahneDurumu.isiklar.zemin.material.opacity = sahneDurumu.golgeOpaklik ?? 0.16;
  }
}

function sahneAyarlariniUygula(ayar) {
  if (!ayar) return;
  sahneDurumu.sahneAyarlari = ayar;
  const a = ayar.ayarlar || {};

  if (sahneDurumu.renderer && a.render?.exposure !== undefined) {
    sahneDurumu.renderer.toneMappingExposure = a.render.exposure;
  }
  if (sahneDurumu.kamera && a.camera?.fov !== undefined) {
    sahneDurumu.kamera.fov = a.camera.fov;
    sahneDurumu.kamera.updateProjectionMatrix();
  }
  if (sahneDurumu.isiklar?.ortam && a.lighting?.hemisphere) {
    sahneDurumu.isiklar.ortam.intensity = a.lighting.hemisphere.intensity ?? sahneDurumu.isiklar.ortam.intensity;
    if (a.lighting.hemisphere.skyColor) sahneDurumu.isiklar.ortam.color.set(a.lighting.hemisphere.skyColor);
    if (a.lighting.hemisphere.groundColor) sahneDurumu.isiklar.ortam.groundColor.set(a.lighting.hemisphere.groundColor);
  }
  if (sahneDurumu.isiklar?.ana && a.lighting?.key) {
    sahneDurumu.isiklar.ana.intensity = a.lighting.key.intensity ?? sahneDurumu.isiklar.ana.intensity;
    if (a.lighting.key.color) sahneDurumu.isiklar.ana.color.set(a.lighting.key.color);
  }
  if (sahneDurumu.isiklar?.dolgu && a.lighting?.fill) {
    sahneDurumu.isiklar.dolgu.intensity = a.lighting.fill.intensity ?? sahneDurumu.isiklar.dolgu.intensity;
    if (a.lighting.fill.color) sahneDurumu.isiklar.dolgu.color.set(a.lighting.fill.color);
  }
  if (sahneDurumu.isiklar?.zemin?.material && a.lighting?.shadowPlane?.opacity !== undefined) {
    sahneDurumu.isiklar.zemin.material.opacity = a.lighting.shadowPlane.opacity;
  }

  // HDR ayarlari
  if (sahneDurumu.sahne && a.hdr) {
    if (a.hdr.intensity !== undefined) {
      sahneDurumu.hdrYogunluk = a.hdr.intensity;
    }
    if (a.hdr.rotation !== undefined) {
      sahneDurumu.hdrDonme = a.hdr.rotation;
    }
    if (a.hdr.blur !== undefined) {
      sahneDurumu.hdrBlurluk = a.hdr.blur;
    }
    hdrAyarlariniUygula();
    if (a.hdr.rotation !== undefined) {
      var rad = THREE.MathUtils.degToRad(a.hdr.rotation);
      if (sahneDurumu.sahne.environmentRotation !== undefined) sahneDurumu.sahne.environmentRotation = new THREE.Euler(0, rad, 0);
      if (sahneDurumu.sahne.backgroundRotation !== undefined) sahneDurumu.sahne.backgroundRotation = new THREE.Euler(0, rad, 0);
    }
    if (a.hdr.blur !== undefined && sahneDurumu.sahne.backgroundBlurriness !== undefined) {
      sahneDurumu.sahne.backgroundBlurriness = a.hdr.blur;
    }
  }
  
  // Zemin ayarlari
  if (a.zemin) {
    if (a.zemin.renk) {
      sahneDurumu.zeminRenk = a.zemin.renk;
    }
    if (a.zemin.opaklik !== undefined) {
      sahneDurumu.zeminOpaklik = a.zemin.opaklik;
    }
  }
  // Zemini guncelle
  if (sahneDurumu.isiklar?.zemin) {
    if (sahneDurumu.isiklar.zemin.material) {
      sahneDurumu.isiklar.zemin.material.opacity = sahneDurumu.zeminOpaklik;
    }
    if (sahneDurumu.zeminRenk) {
      sahneDurumu.isiklar.zemin.material.color.set(sahneDurumu.zeminRenk);
    }
  }
  
  // Arka plan rengi
  if (a.arkaPlan?.renk && sahneDurumu.sahne) {
    sahneDurumu.arkaPlanRenk = a.arkaPlan.renk;
    sahneDurumu.sahne.background = new THREE.Color(a.arkaPlan.renk);
    var apEl = document.getElementById("ayarArkaPlanRenk");
    var apDegerEl = document.getElementById("arkaPlanRenkDeger");
    if (apEl) apEl.value = a.arkaPlan.renk;
    if (apDegerEl) apDegerEl.textContent = a.arkaPlan.renk;
  }

  // Golge ayarlari
  if (a.golge) {
    if (a.golge.opaklik !== undefined) {
      sahneDurumu.golgeOpaklik = a.golge.opaklik;
      // Golge opakligini zemin golge materyaline uygula
      if (sahneDurumu.isiklar?.zemin?.material) {
        sahneDurumu.isiklar.zemin.material.opacity = a.golge.opaklik;
      }
    }
    if (a.golge.boyut !== undefined) {
      sahneDurumu.golgeBoyut = a.golge.boyut;
      if (sahneDurumu.isiklar?.ana?.shadow?.mapSize) {
        sahneDurumu.isiklar.ana.shadow.mapSize.set(a.golge.boyut, a.golge.boyut);
        if (sahneDurumu.isiklar.ana.shadow.map) {
          sahneDurumu.isiklar.ana.shadow.map.dispose();
          sahneDurumu.isiklar.ana.shadow.map = null;
        }
      }
    }
    if (a.golge.bias !== undefined) {
      sahneDurumu.golgeBias = a.golge.bias;
      if (sahneDurumu.isiklar?.ana?.shadow) {
        sahneDurumu.isiklar.ana.shadow.bias = a.golge.bias;
      }
    }
  }

  renkleriUygula();
  if (sahneDurumu.modelKoku) kamerayiModeleSigdir(sahneDurumu.modelKoku);
}

async function sahneAyarlariniBaslat() {
  try {
    const yanit = await fetch("./sahne-ayar-varsayilan.json", { cache: "no-store" });
    window.__sahneAyarFabrika = await yanit.json();
  } catch (hata) {
    console.warn("Sahne fabrika JSON yüklenemedi", hata);
    window.__sahneAyarFabrika = { schemaVersion: 1, ayarlar: {} };
  }

  const kayitli = localStorage.getItem("goldbanyo_sahne_ayarlari");
  const ayar = kayitli ? JSON.parse(kayitli) : derinKopya(window.__sahneAyarFabrika);
  sahneAyarFormunuDoldur(ayar);
  sahneAyarlariniUygula(ayar);

  document.querySelectorAll(".ayar-range").forEach((input) => {
    input.addEventListener("input", () => {
      const yeniAyar = sahneAyarFormundanOku();
      localStorage.setItem("goldbanyo_sahne_ayarlari", JSON.stringify(yeniAyar));
      sahneAyarlariniUygula(yeniAyar);
    });
  });

  document.getElementById("ayarResetButon")?.addEventListener("click", () => {
    const fabrika = derinKopya(window.__sahneAyarFabrika);
    localStorage.setItem("goldbanyo_sahne_ayarlari", JSON.stringify(fabrika));
    sahneAyarFormunuDoldur(fabrika);
    sahneAyarlariniUygula(fabrika);
  });

  document.getElementById("ayarKaydetButon")?.addEventListener("click", () => {
    const ayarJson = sahneAyarFormundanOku();
    localStorage.setItem("goldbanyo_sahne_ayarlari", JSON.stringify(ayarJson));
    const blob = new Blob([JSON.stringify(ayarJson, null, 2)], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "goldbanyo-sahne-ayar.json";
    a.click();
    URL.revokeObjectURL(url);
  });

  document.getElementById("sahneAyarDosya")?.addEventListener("change", async (event) => {
    const dosya = event.target.files?.[0];
    if (!dosya) return;
    const text = await dosya.text();
    const ayarJson = JSON.parse(text);
    localStorage.setItem("goldbanyo_sahne_ayarlari", JSON.stringify(ayarJson));
    sahneAyarFormunuDoldur(ayarJson);
    sahneAyarlariniUygula(ayarJson);
    // Ürün konfigürasyonunu uygula (model değişimi gerekebilir)
    var urunKonfig = ayarJson.urunKonfigurasyonu;
    if (urunKonfig && urunKonfig.surum === 1) {
      if (urunKonfig.modelId && urunKonfig.modelId !== aktifModelId) {
        bekleyenUrunKonfigurasyonu = urunKonfig;
        modelDegistir(urunKonfig.modelId);
      } else {
        urunKonfigurasyonunuUygula(urunKonfig);
      }
    }
  });
}
console.log("✅ Event listeners bağlandı");

// ═══ MODEL RENK/KAPLAMA TOGGLE EVENT ═══
if (elemanlar.modelRenkKaplamaAktif) {
  elemanlar.modelRenkKaplamaAktif.addEventListener("change", function() {
    sahneDurumu.renkKaplamaAktif = !!elemanlar.modelRenkKaplamaAktif.checked;
    modelRenkKaplamaArayuzunuGuncelle();
  });
}

// ═══ KATLANABİLİR BÖLÜMLERİ BAŞLAT ═══
bolumleriKatlanabilirYap();

// Model seçiciyi başlat
modelSeciciyiHazirla();

window.addEventListener("resize", pencereBoyutunuUygula);

console.log("🚀 sahneyiHazirla() çağrılıyor...");
try {
  sahneyiHazirla();
  console.log("✅ sahneyiHazirla() başarılı");
  sahneAyarlariniBaslat();
} catch (err) {
  console.error("❌ sahneyiHazirla() error:", err);
}

console.log("🎬 modeliYukle() çağrılıyor...");
try {
  modeliYukle();
  console.log("✅ modeliYukle() başarılı");
} catch (err) {
  console.error("❌ modeliYukle() error:", err);
}

// ═══ HDR SEÇİCİ ═══
var hdrSecici = document.getElementById("hdrSecici");
if (hdrSecici) {
  hdrSecici.querySelectorAll("button").forEach(function(buton) {
    buton.addEventListener("click", function() {
      hdrSecici.querySelectorAll("button").forEach(function(b) { b.classList.remove("aktif"); });
      buton.classList.add("aktif");
      var hdrId = buton.dataset.hdr;
      document.getElementById("hdrOrtam").value = hdrId;
      hdrOrtamDegistir(hdrId);
    });
  });
}

// Özel HDR yükleme
var hdrDosyaSec = document.getElementById("hdrDosyaSec");
if (hdrDosyaSec) {
  hdrDosyaSec.addEventListener("change", function(event) {
    var dosya = event.target.files?.[0];
    if (!dosya) return;
    var nesneAdresi = URL.createObjectURL(dosya);
    hdrDosyaYukle(nesneAdresi, dosya.name);
  });
}

// HDR ayar slider'lari
var hdrAyarlari = [
  { id: "ayarHdrYogunluk", alan: "hdrYogunluk" },
  { id: "ayarHdrDondurme", alan: "hdrDonme" },
  { id: "ayarHdrBlurluk", alan: "hdrBlurluk" }
];
hdrAyarlari.forEach(function(kayit) {
  var el = document.getElementById(kayit.id);
  if (el) {
    el.addEventListener("input", function() {
      sahneDurumu[kayit.alan] = parseFloat(el.value);
      hdrAyarlariniUygula();
    });
  }
});

// Zemin renk picker
var zeminRenkEl = document.getElementById("ayarZeminRenk");
var zeminDegerEl = document.getElementById("zeminRenkDeger");
if (zeminRenkEl) {
  zeminRenkEl.addEventListener("input", function() {
    sahneDurumu.zeminRenk = zeminRenkEl.value;
    if (zeminDegerEl) zeminDegerEl.textContent = zeminRenkEl.value;
    if (sahneDurumu.isiklar?.zemin?.material) {
      sahneDurumu.isiklar.zemin.material.color.set(zeminRenkEl.value);
    }
  });
}

// Zemin opaklik
var zeminOpaklikEl = document.getElementById("ayarZeminOpaklik");
if (zeminOpaklikEl) {
  zeminOpaklikEl.addEventListener("input", function() {
    sahneDurumu.zeminOpaklik = parseFloat(zeminOpaklikEl.value);
    if (sahneDurumu.isiklar?.zemin?.material) {
      sahneDurumu.isiklar.zemin.material.opacity = sahneDurumu.zeminOpaklik;
    }
  });
}

// ═══ TÜM SAHNE AYARLARI — SLIDER ↔ SAYISAL GİRİŞ SENKRONİZASYONU ═══
var tumSahneAyarlari = [
  { slider: "ayarExposure", sayi: "ayarExposure_deger" },
  { slider: "ayarFov", sayi: "ayarFov_deger" },
  { slider: "ayarKameraMesafe", sayi: "ayarKameraMesafe_deger" },
  { slider: "ayarOrtamIsik", sayi: "ayarOrtamIsik_deger" },
  { slider: "ayarAnaIsik", sayi: "ayarAnaIsik_deger" },
  { slider: "ayarDolguIsik", sayi: "ayarDolguIsik_deger" },
  { slider: "ayarAynaYansima", sayi: "ayarAynaYansima_deger" },
  { slider: "ayarAynaRoughness", sayi: "ayarAynaRoughness_deger" },
  { slider: "ayarCamOpacity", sayi: "ayarCamOpacity_deger" },
  { slider: "ayarEnvScale", sayi: "ayarEnvScale_deger" },
  { slider: "ayarHdrYogunluk", sayi: "ayarHdrYogunluk_deger" },
  { slider: "ayarHdrDondurme", sayi: "ayarHdrDondurme_deger" },
  { slider: "ayarHdrBlurluk", sayi: "ayarHdrBlurluk_deger" },
  { slider: "ayarZeminOpaklik", sayi: "ayarZeminOpaklik_deger" },
  { slider: "ayarGolgeOpaklik", sayi: "ayarGolgeOpaklik_deger" },
  { slider: "ayarGolgeBoyut", sayi: "ayarGolgeBoyut_deger" },
  { slider: "ayarGolgeBias", sayi: "ayarGolgeBias_deger" },
  { slider: "ayarClearcoatScale", sayi: "ayarClearcoatScale_deger" }
];
tumSahneAyarlari.forEach(function(kayit) {
  var sliderEl = document.getElementById(kayit.slider);
  var sayiEl = document.getElementById(kayit.sayi);
  if (sliderEl && sayiEl) {
    // Sayisal giris → slider (dispatchEvent yok — sadece deger guncelle)
    sayiEl.addEventListener("input", function() {
      var v = parseFloat(sayiEl.value);
      if (!isNaN(v)) {
        sliderEl.value = v;
        // dispatchEvent calistirilmaz — slider'in kendi input event'i zaten bagli
      }
    });
    // Slider → sayisal giris
    sliderEl.addEventListener("input", function() {
      sayiEl.value = sliderEl.value;
    });
  }
});

// ═══ ISIK SLIDER'LARI LIVE EVENT ═══
var isikAyarlari = [
  { id: "ayarOrtamIsik", fonksiyon: function(v) { if (sahneDurumu.isiklar?.ortam) sahneDurumu.isiklar.ortam.intensity = v; } },
  { id: "ayarAnaIsik", fonksiyon: function(v) { if (sahneDurumu.isiklar?.ana) sahneDurumu.isiklar.ana.intensity = v; } },
  { id: "ayarDolguIsik", fonksiyon: function(v) { if (sahneDurumu.isiklar?.dolgu) sahneDurumu.isiklar.dolgu.intensity = v; } }
];
isikAyarlari.forEach(function(kayit) {
  var el = document.getElementById(kayit.id);
  if (el) {
    el.addEventListener("input", function() {
      kayit.fonksiyon(parseFloat(el.value));
    });
  }
});

// ═══ KAMERA LIVE EVENT ═══
var kameraFovEl = document.getElementById("ayarFov");
var kameraMesafeEl = document.getElementById("ayarKameraMesafe");
if (kameraFovEl) {
  kameraFovEl.addEventListener("input", function() {
    var v = parseFloat(kameraFovEl.value);
    if (sahneDurumu.kamera) {
      sahneDurumu.kamera.fov = v;
      sahneDurumu.kamera.updateProjectionMatrix();
    }
  });
}
if (kameraMesafeEl) {
  kameraMesafeEl.addEventListener("input", function() {
    var v = parseFloat(kameraMesafeEl.value);
    if (sahneDurumu.kontroller && sahneDurumu.kamera) {
      var hedef = sahneDurumu.kontroller.target;
      var yon = new THREE.Vector3().subVectors(sahneDurumu.kamera.position, hedef).normalize();
      var mesafe = v * 2.5; // Slider 1.6-3.2 arasi, gercek mesafe 4-8
      sahneDurumu.kamera.position.copy(hedef).addScaledVector(yon, mesafe);
    }
  });
}

// ═══ EXPOSURE LIVE EVENT ═══
var exposureEl = document.getElementById("ayarExposure");
if (exposureEl) {
  exposureEl.addEventListener("input", function() {
    var v = parseFloat(exposureEl.value);
    if (sahneDurumu.renderer) {
      sahneDurumu.renderer.toneMappingExposure = v;
    }
  });
}

// ═══ HDR SLIDER'LARI LIVE EVENT ═══
// Bu ID'ler HTML'deki input elemanlarina karsilik gelir (ASCII, Turksuz)
var hdrIntensityEl = document.getElementById("ayarHdrYogunluk");
var hdrDonmeEl = document.getElementById("ayarHdrDondurme");
var hdrBlurlukEl = document.getElementById("ayarHdrBlurluk");
var hdrIntensityDegerEl = document.getElementById("ayarHdrYogunluk_deger");
var hdrDonmeDegerEl = document.getElementById("ayarHdrDondurme_deger");
var hdrBlurlukDegerEl = document.getElementById("ayarHdrBlurluk_deger");

if (hdrIntensityEl) {
  hdrIntensityEl.addEventListener("input", function() {
    var v = parseFloat(hdrIntensityEl.value);
    sahneDurumu.hdrYogunluk = v;
    if (hdrIntensityDegerEl) hdrIntensityDegerEl.textContent = v.toFixed(1);
    hdrAyarlariniUygula();
  });
}
if (hdrDonmeEl) {
  hdrDonmeEl.addEventListener("input", function() {
    var v = parseFloat(hdrDonmeEl.value);
    sahneDurumu.hdrDonme = v;
    if (hdrDonmeDegerEl) hdrDonmeDegerEl.textContent = v.toFixed(0) + "°";
    // Feature detection ile environment/background rotation, yeni PMREM yok
    if (sahneDurumu.sahne) {
      var rad = THREE.MathUtils.degToRad(v);
      if (sahneDurumu.sahne.environmentRotation !== undefined) sahneDurumu.sahne.environmentRotation = new THREE.Euler(0, rad, 0);
      if (sahneDurumu.sahne.backgroundRotation !== undefined) sahneDurumu.sahne.backgroundRotation = new THREE.Euler(0, rad, 0);
    }
  });
}
if (hdrBlurlukEl) {
  hdrBlurlukEl.addEventListener("input", function() {
    var v = parseFloat(hdrBlurlukEl.value);
    sahneDurumu.hdrBlurluk = v;
    if (hdrBlurlukDegerEl) hdrBlurlukDegerEl.textContent = v.toFixed(1);
    // backgroundBlurriness varsa uygula, renderer/PMREM/material reset yok
    if (sahneDurumu.sahne && sahneDurumu.sahne.backgroundBlurriness !== undefined) {
      sahneDurumu.sahne.backgroundBlurriness = v;
    }
  });
}

// ═══ ARKA PLAN RENGİ ═══
var arkaPlanRenkEl = document.getElementById("ayarArkaPlanRenk");
var arkaPlanDegerEl = document.getElementById("arkaPlanRenkDeger");
if (arkaPlanRenkEl) {
  arkaPlanRenkEl.addEventListener("input", function() {
    sahneDurumu.arkaPlanRenk = arkaPlanRenkEl.value;
    if (arkaPlanDegerEl) arkaPlanDegerEl.textContent = arkaPlanRenkEl.value;
    if (sahneDurumu.sahne) {
      sahneDurumu.sahne.background = new THREE.Color(arkaPlanRenkEl.value);
    }
  });
}

// Hızlı renk butonları
var arkaPlanHizliGrup = document.querySelector('[data-hedef="arkaPlanHizli"]');
if (arkaPlanHizliGrup) {
  arkaPlanHizliGrup.querySelectorAll("button").forEach(function(buton) {
    buton.addEventListener("click", function() {
      arkaPlanHizliGrup.querySelectorAll("button").forEach(function(b) { b.classList.remove("aktif"); });
      buton.classList.add("aktif");
      var renk = buton.dataset.deger;
      sahneDurumu.arkaPlanRenk = renk;
      if (arkaPlanRenkEl) arkaPlanRenkEl.value = renk;
      if (arkaPlanDegerEl) arkaPlanDegerEl.textContent = renk;
      if (sahneDurumu.sahne) {
        sahneDurumu.sahne.background = new THREE.Color(renk);
      }
    });
  });
}

// Golge ayarlari
var golgeAyarlari = [
  { id: "ayarGolgeOpaklik", alan: "golgeOpaklik" },
  { id: "ayarGolgeBoyut", alan: "golgeBoyut" },
  { id: "ayarGolgeBias", alan: "golgeBias" }
];
golgeAyarlari.forEach(function(kayit) {
  var el = document.getElementById(kayit.id);
  if (el) {
    el.addEventListener("input", function() {
      sahneDurumu[kayit.alan] = parseFloat(el.value);
      golgeAyarlariniUygula();
    });
  }
});

