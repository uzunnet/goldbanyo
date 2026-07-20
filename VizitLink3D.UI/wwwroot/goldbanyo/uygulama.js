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
  sahneButonlari: document.querySelectorAll(".ikon-buton")
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
  }
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

function pbrMateryalOlustur(malzemeTuru, renk = null) {
  const malzeme = pbrMalzemeler[malzemeTuru] || pbrMalzemeler.plastik;
  const malzemeAyarlari = sahneDurumu.sahneAyarlari?.ayarlar?.materials || {};
  const envScale = malzemeAyarlari.globalEnvMapScale ?? 1;
  const clearcoatScale = malzemeAyarlari.globalClearcoatScale ?? 1;
  const roughnessOffset = malzemeAyarlari.globalRoughnessOffset ?? 0;
  const config = {
    color: new THREE.Color(renk || malzeme.renk),
    metalness: malzeme.metalness,
    roughness: Math.min(1, Math.max(0, malzeme.roughness + roughnessOffset)),
    envMap: sahneDurumu.sahne?.environment || null, // r128'de scene.environment otomatik uygulanmıyor, elle bağlanması gerekiyor
    envMapIntensity: malzeme.envMapIntensity * envScale,
    clearcoat: Math.min(1, malzeme.clearcoat * clearcoatScale),
    clearcoatRoughness: malzeme.clearcoatRoughness
  };

  // Cam seffaf kaplama
  if (malzemeTuru === 'cam') {
    config.transparent = true;
    config.opacity = malzemeAyarlari.camOpacity ?? 0.82;
    config.depthWrite = false;
  }


  return new THREE.MeshPhysicalMaterial(config);
}

function camMateryalOlustur(renk) {
  // GERÇEK AYNA: gümüş kaplama, opak, canlı CubeCamera yansımalı
  const materyal = new THREE.MeshPhysicalMaterial({
    color: new THREE.Color(sahneDurumu.sahneAyarlari?.ayarlar?.materials?.mirror?.color || "#d8dbd8"),
    envMap: sahneDurumu.sahne?.environment || null,
    metalness: 1.0,
    roughness: sahneDurumu.sahneAyarlari?.ayarlar?.materials?.mirror?.roughness ?? 0.045,
    envMapIntensity: sahneDurumu.sahneAyarlari?.ayarlar?.materials?.mirror?.envMapIntensity ?? 1.25,
    clearcoat: 0,
    clearcoatRoughness: 0,
    reflectivity: 1,
    side: THREE.FrontSide
  });
  materyal.toneMapped = true;
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
        texture.dispose();
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
  parcaMateryaliUygula(sahneDurumu.modelParcalari.govde, () => pbrMateryalOlustur(govdeMalzeme, govdeRenk));

  // METAL/KASA/ÇERÇEVE — camlı modellerde sabit saten metal, gövde/cam rengine karışmaz
  parcaMateryaliUygula(sahneDurumu.modelParcalari.metalAksam || [], () => pbrMateryalOlustur("metal", "#b7b2a8"));

  // KAPAK — Mesh adında "cam" geçiyorsa cam malzeme, değilse seçili kapak malzemesi
  function kapakMateryaliUret(mesh) {
    var meshAdi = (mesh.name || "").toLowerCase();
    var yolAdi = nesneAdYolunuAl(mesh).toLowerCase();
    if (meshAdi.includes("cam") || yolAdi.includes("cam") || meshAdi.includes("glass") || yolAdi.includes("glass") || meshAdi.includes("seffaf") || yolAdi.includes("seffaf")) {
      return pbrMateryalOlustur("cam", kapakRenk);
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
  parcaMateryaliUygula(sahneDurumu.modelParcalari.icAltTabla, () => pbrMateryalOlustur(tezgahMalzeme));
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
  kontroller.minDistance = 2.5;
  kontroller.maxDistance = 10;

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

// ═══ MODEL SEÇİCİ SİSTEMİ ═══════════════════════════════════
let aktifModelId = null;

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
  });
}
console.log("✅ Event listeners bağlandı");

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


