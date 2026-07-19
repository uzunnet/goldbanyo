console.log("📌 uygulama.js başladı");

const elemanlar = {
  // YENİ KONTROLLER
  govdeRenk: document.getElementById("govdeRenk"),
  govdeMalzeme: document.getElementById("govdeMalzeme"),
  kapakRenk: document.getElementById("kapakRenk"),
  kapakMalzeme: document.getElementById("kapakMalzeme"),
  kulpKaplama: document.getElementById("kulpKaplama"),
  ledIsik: document.getElementById("ledIsik"),
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
    renk: "#e8e8e8",
    metalness: 0.95,
    roughness: 0.05,
    envMapIntensity: 1.7,
    clearcoat: 0.8,
    clearcoatRoughness: 0.1,
    aciklama: "Krom — Ayna gibi yansıma"
  },
  metal: {
    renk: "#b8b8b8",
    metalness: 0.88,
    roughness: 0.15,
    envMapIntensity: 1.2,
    clearcoat: 0.3,
    clearcoatRoughness: 0.2,
    aciklama: "Metal — Güçlü yansıma"
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
    roughness: 0.68,
    envMapIntensity: 0.2,
    clearcoat: 0,
    clearcoatRoughness: 0.7,
    aciklama: "MDF/Laminat — Ahşap"
  },
  cam: {
    renk: "#e0f2f1",
    metalness: 0,
    roughness: 0.01,
    transmission: 0.95,
    ior: 1.5,
    envMapIntensity: 0.9,
    thickness: 1,
    attenuationDistance: 10,
    attenuationColor: "#ffffff",
    clearcoat: 1,
    clearcoatRoughness: 0.01,
    aciklama: "Cam — Refraction + reflection"
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
    renk: "#f5f5f5",
    metalness: 0,
    roughness: 0.06,
    envMapIntensity: 0.8,
    clearcoat: 0.9,
    clearcoatRoughness: 0.03,
    aciklama: "Porselen/Seramik — Parlak, cilalı yüzey"
  },
  lakeboya: {
    renk: "#2c2c2c",
    metalness: 0.1,
    roughness: 0.08,
    envMapIntensity: 0.85,
    clearcoat: 0.75,
    clearcoatRoughness: 0.02,
    aciklama: "Lake Boya — Yüksek parlak, cam gibi"
  },
  lakeboyaMat: {
    renk: "#2c2c2c",
    metalness: 0.05,
    roughness: 0.45,
    envMapIntensity: 0.5,
    clearcoat: 0.15,
    clearcoatRoughness: 0.4,
    aciklama: "Lake Boya — Mat yüzey"
  },
  mermer: {
    renk: "#d4ccc8",
    metalness: 0,
    roughness: 0.25,
    envMapIntensity: 0.8,
    clearcoat: 0.4,
    clearcoatRoughness: 0.15,
    aciklama: "Mermer — Doğal taş, hafif parlak"
  },
  kompozit: {
    renk: "#e8e4de",
    metalness: 0,
    roughness: 0.1,
    envMapIntensity: 1.0,
    clearcoat: 0.6,
    clearcoatRoughness: 0.05,
    aciklama: "Kompozit Kuvars — Pürüzsüz, parlak"
  }
};

// Ürün galerisi varyasyon kombinasyonları — gerçek Gold Banyo kataloğu (Hermes/Giorgio/Bottega) renklerine göre
const urunTemalari = {
  gorsel1: { // Kahve Kapak (Hermes tarzı — sıcak kahve + krem)
    govdeRenk: "RAL 9001|#E9E0CB", govdeMalzeme: "mdf",
    kapakRenk: "RAL 8017|#442F29", kapakMalzeme: "lakeboya",
    kulpKaplama: "krom|#d4af37", muslukKaplama: "krom|#d4af37",
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
    kulpKaplama: "krom|#d4af37", muslukKaplama: "krom|#d4af37",
    lavaboRenk: "RAL 9010|#F2ECE1", ustTablaRenk: "RAL 7030|#928E85",
    tezgahMalzeme: "kompozit"
  },
  gorsel4: { // Antrasit Kapak (Giorgio tarzı — soğuk gri + altın armatür)
    govdeRenk: "RAL 7030|#928E85", govdeMalzeme: "mdf",
    kapakRenk: "RAL 7016|#383E42", kapakMalzeme: "lakeboyaMat",
    kulpKaplama: "krom|#d4af37", muslukKaplama: "krom|#d4af37",
    lavaboRenk: "RAL 9010|#F2ECE1", ustTablaRenk: "RAL 7030|#928E85",
    tezgahMalzeme: "mermer"
  },
  gorsel5: { // Ahşap Kapak (Bottega/Diago tarzı — doğal ceviz)
    govdeRenk: "RAL 9001|#E9E0CB", govdeMalzeme: "mdf",
    kapakRenk: "AHSAP|#5A3A28", kapakMalzeme: "mdf",
    kulpKaplama: "krom|#d4af37", muslukKaplama: "krom|#d4af37",
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
  const config = {
    color: new THREE.Color(renk || malzeme.renk),
    metalness: malzeme.metalness,
    roughness: malzeme.roughness,
    envMap: sahneDurumu.sahne?.environment || null, // r128'de scene.environment otomatik uygulanmıyor, elle bağlanması gerekiyor
    envMapIntensity: malzeme.envMapIntensity,
    clearcoat: malzeme.clearcoat,
    clearcoatRoughness: malzeme.clearcoatRoughness
  };

  // Cam için transmission özellikleri ekle (eski Three.js versiyonunda bazı property'ler yok)
  if (malzemeTuru === "cam") {
    if (config.transmission !== undefined) config.transmission = malzeme.transmission;
    if (config.ior !== undefined) config.ior = malzeme.ior;
    // thickness, attenuationDistance, attenuationColor eski Three.js'de yok
  }

  return new THREE.MeshPhysicalMaterial(config);
}

function camMateryalOlustur(renk) {
  return new THREE.MeshPhysicalMaterial({
    color: new THREE.Color(renk).lerp(new THREE.Color("#f1f3ef"), 0.5),
    envMap: sahneDurumu.aynaYansima?.hedef.texture || null,
    metalness: 1,
    roughness: 0.035,
    envMapIntensity: 1.6,
    clearcoat: 1,
    clearcoatRoughness: 0.025,
    reflectivity: 0.95
  });
}

function ledMateryalOlustur() {
  const materyal = new THREE.MeshStandardMaterial({
    color: "#ffe066",
    emissive: "#ffc400",
    emissiveIntensity: 4.5,
    metalness: 0,
    roughness: 0.3
  });
  materyal.toneMapped = false;
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
  tuval.width = 1024;
  tuval.height = 512;
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
  for (let x = 0; x <= tuval.width; x += 96) {
    cizim.beginPath();
    cizim.moveTo(x, 0);
    cizim.lineTo(x, tuval.height);
    cizim.stroke();
  }
  for (let y = 80; y <= 360; y += 72) {
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
  cizim.fillRect(0, 34, tuval.width, 34);

  cizim.fillStyle = "rgba(206, 219, 226, 0.78)";
  cizim.fillRect(92, 118, 170, 126);
  cizim.fillRect(762, 126, 130, 104);
  cizim.fillStyle = "rgba(35, 33, 31, 0.72)";
  cizim.fillRect(350, 285, 330, 76);
  cizim.fillStyle = "rgba(198, 149, 42, 0.36)";
  cizim.fillRect(390, 265, 250, 12);

  // Keskin kenarlı "pencere" panelleri — yansımanın belirgin/tanınabilir olması için
  const pencereler = [
    { x: 40, y: 60, w: 150, h: 220 },
    { x: 440, y: 70, w: 150, h: 230 },
    { x: 840, y: 65, w: 150, h: 220 }
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
      sahne.background = texture;
      sahne.environment = texture;
      URL.revokeObjectURL(blobUrl);
      console.log("✅ Gerçek HDR yüklendi (Poly Haven — Modern Bathroom, CC0)");
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
    const genislik = Math.max(olcu.x * 0.86, 0.18);
    const yukseklik = Math.max(olcu.y * 0.86, 0.18);
    const geometri = new THREE.PlaneGeometry(genislik, yukseklik);
    const materyal = new THREE.MeshPhysicalMaterial({
      color: 0xf8faf5,
      metalness: 1,
      roughness: 0.018,
      envMap: sahneDurumu.aynaYansima.hedef.texture,
      envMapIntensity: 3.4,
      clearcoat: 1,
      clearcoatRoughness: 0.01,
      side: THREE.DoubleSide
    });
    const kaplama = new THREE.Mesh(geometri, materyal);
    kaplama.name = "GERCEK_AYNA_YANSIMA_KAPLAMA";
    kaplama.position.set(merkez.x, merkez.y, kutu.max.z + 0.006);
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

  // ═══ GÖVDE ═══ (ALT GÖVDE, BOY DOLAP GÖVDE, ana gövde, kasa)
  if (ad.includes("alt govde") || ad.includes("boy dolap govde") ||
      ad.includes("govde") || ad.includes("kasa") || ad.includes("body")) return "govde";

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

  // KAPAK — 4 kapı (kapak1-4) aynı anda, birlikte değişir
  parcaMateryaliUygula(sahneDurumu.modelParcalari.kapak1, () => pbrMateryalOlustur(kapakMalzeme, kapakRenk));
  parcaMateryaliUygula(sahneDurumu.modelParcalari.kapak2, () => pbrMateryalOlustur(kapakMalzeme, kapakRenk));
  parcaMateryaliUygula(sahneDurumu.modelParcalari.kapak3, () => pbrMateryalOlustur(kapakMalzeme, kapakRenk));
  parcaMateryaliUygula(sahneDurumu.modelParcalari.kapak4, () => pbrMateryalOlustur(kapakMalzeme, kapakRenk));

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
  const tema = urunTemalari[temaAdi];
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
    sahneDurumu.kamera.position.set(3.8, 2.2, 5.7);
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
  const mesafe = enBuyuk * 2.05;

  sahneDurumu.kontroller.target.copy(merkez);
  sahneDurumu.kamera.position.set(merkez.x, merkez.y + enBuyuk * 0.08, merkez.z + mesafe);
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
  renderer.toneMappingExposure = 0.82;
  if ("outputColorSpace" in renderer && THREE.SRGBColorSpace) {
    renderer.outputColorSpace = THREE.SRGBColorSpace;
  } else if ("outputEncoding" in renderer && THREE.sRGBEncoding) {
    renderer.outputEncoding = THREE.sRGBEncoding;
  }
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = THREE.PCFSoftShadowMap;

  const sahne = new THREE.Scene();
  const odaDokusu = banyoYansimaDokusuOlustur();
  sahne.background = new THREE.Color(0x000000); // Sabit siyah arka plan — HDR geçici kaldırıldı
  sahne.environment = odaDokusu; // Yansımalar için prosedürel doku korunuyor
  sahne.fog = new THREE.Fog(0x171717, 16, 34);
  // gercekHdrYukle(sahne); // HDR geçici olarak devre dışı — tekrar istenirse yorum kaldırılır

  const aynaYansimaHedefi = new THREE.WebGLCubeRenderTarget(512, {
    generateMipmaps: true,
    minFilter: THREE.LinearMipmapLinearFilter
  });
  const aynaYansimaKamerasi = new THREE.CubeCamera(0.05, 40, aynaYansimaHedefi);
  sahne.add(aynaYansimaKamerasi);

  const kamera = new THREE.PerspectiveCamera(36, 1, 0.1, 100);
  kamera.position.set(3.8, 2.2, 5.7);

  const kontroller = new THREE.OrbitControls(kamera, elemanlar.kanvas);
  kontroller.enableDamping = true;
  kontroller.dampingFactor = 0.08;
  kontroller.target.set(0, 1.2, 0);
  kontroller.minDistance = 2.5;
  kontroller.maxDistance = 10;

  const ortamIsigi = new THREE.HemisphereLight(0xfff6df, 0x2b2622, 0.75);
  sahne.add(ortamIsigi);

  // Ana ışık — daha yandan/sıyırıcı açı: oluklu (fluted) yüzeylerde her girinti kendi ışık/gölgesini alsın
  const anaIsik = new THREE.DirectionalLight(0xfff1cd, 0.75);
  anaIsik.position.set(5.6, 4.6, 3.4);
  anaIsik.castShadow = true;
  anaIsik.shadow.mapSize.set(2048, 2048);
  anaIsik.shadow.bias = -0.0018;
  anaIsik.shadow.normalBias = 0.02;
  sahne.add(anaIsik);

  // Kenar/sıyırma ışığı — ters yönden, oluklu dokunun derinliğini vurgular
  const kenarIsik = new THREE.DirectionalLight(0xeaf1ff, 0.32);
  kenarIsik.position.set(-4.2, 2.6, 4.8);
  sahne.add(kenarIsik);

  const dolguIsik = new THREE.PointLight(0xc9def8, 0.3, 14);
  dolguIsik.position.set(-4.5, 3.1, -1.8);
  sahne.add(dolguIsik);

  const zemin = new THREE.Mesh(
    new THREE.CircleGeometry(6.5, 80),
    new THREE.ShadowMaterial({ color: 0x000000, opacity: 0.22 })
  );
  zemin.rotation.x = -Math.PI / 2;
  zemin.position.y = -0.002; // Modelin tam alt yüzeyinin hemen altı — z-fighting'i önlemek için ufak ofset
  zemin.receiveShadow = true;
  sahne.add(zemin);

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
  renkleriUygula();
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

    const ledLight = new THREE.PointLight(0xffeb3b, 2, 50);
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

  console.log("🔄 Model değişiyor: " + model.ad);
  yuklemeKatmaniniAyarla(true);
  modeliYukleAdresten(model.dosya, model.ad);
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

  // LED pulse animasyonu — hem ışık hem mesh parlaklığı (sadece LED açıkken)
  if (sahneDurumu.ledIsigi && sahneDurumu.ledAcik) {
    sahneDurumu.ledZamani += 0.02;
    const pulse = 0.8 + Math.sin(sahneDurumu.ledZamani) * 0.6;
    sahneDurumu.ledIsigi.intensity = 2.5 * pulse;

    sahneDurumu.modelParcalari.led.forEach((mesh) => {
      if (mesh.material) {
        mesh.material.emissiveIntensity = 3.5 + pulse * 2.5;
      }
    });
  }

  if (sahneDurumu.renderer && sahneDurumu.sahne && sahneDurumu.kamera) {
    aynaYansimasiniGuncelle();
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

console.log("✅ Event listeners bağlandı");

// Model seçiciyi başlat
modelSeciciyiHazirla();

window.addEventListener("resize", pencereBoyutunuUygula);

console.log("🚀 sahneyiHazirla() çağrılıyor...");
try {
  sahneyiHazirla();
  console.log("✅ sahneyiHazirla() başarılı");
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


