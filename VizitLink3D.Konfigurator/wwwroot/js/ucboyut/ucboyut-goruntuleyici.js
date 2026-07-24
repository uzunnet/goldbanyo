/**
 * UcBoyutGoruntuleyici — Three.js ES module wrapper
 *
 * P04: Bagimsiz public 3D viewer icin tarayici tarafi modul.
 * Three.js 0.170.0 pinned, CDN (jsdelivr) uzerinden ES module import.
 *
 * CDN kullanim sebebi: Three.js npm paketi Blazor SSR ile uyumlu degildir;
 * tarayici tarafi ES module import haritasi ile yuklenir.
 * CSP: sadece cdn.jsdelivr.net script-src izni verilmistir.
 *
 * @module UcBoyutGoruntuleyici
 */

class UcBoyutGoruntuleyici {

    /** @type {WebGLRenderer} */
    _renderer = null;
    /** @type {Scene} */
    _scene = null;
    /** @type {PerspectiveCamera} */
    _camera = null;
    /** @type {OrbitControls} */
    _controls = null;
    /** @type {Object3D} */
    _mevcutModel = null;
    /** @type {number} */
    _animasyonId = null;
    /** @type {Object} */
    _secenekler = {};
    /** @type {HTMLElement} */
    _kapsayici = null;
    /** @type {boolean} */
    _temizlemeGerekiyor = false;
    /** @type {Array<{hedef: EventTarget, tur: string, isleyici: Function}>} */
    _eventListenerlar = [];
    /** @type {Array<{mesh: Mesh, orijinalEmissiveHex: number, orijinalEmissiveIntensity: number}>} */
    _seciliMeshler = [];

    /**
     * @param {HTMLElement} kapsayiciElement - 3D sahnenin eklenecegi DOM elemani
     * @param {Object} [secenekler={}] - Opsiyonel yapilandirma
     * @param {Function} [secenekler.onYukleniyor] - Yukleme basladiginda cagrilir
     * @param {Function} [secenekler.onYuklendi] - Yukleme basariyla bittiginde cagrilir, (gltf) parametresi alir
     * @param {Function} [secenekler.onHata] - Hata olustugunda cagrilir, (hata) parametresi alir
     */
    constructor(kapsayiciElement, secenekler = {}) {
        this._kapsayici = kapsayiciElement;
        this._secenekler = {
            onYukleniyor: null,
            onYuklendi: null,
            onHata: null,
            ...secenekler
        };
    }

    /**
     * Three.js modullerini CDN'den import eder, sahneyi ve isiklari kurar,
     * OrbitControls ve animasyon dongusunu baslatir.
     * @returns {Promise<void>}
     */
    async init() {
        const T = 'https://cdn.jsdelivr.net/npm/three@0.170.0';

        // Three.js cekirdek modulleri (pinned 0.170.0)
        const THREE_CORE = await import(`${T}/build/three.module.js`);
        const { Scene, PerspectiveCamera, WebGLRenderer,
                AmbientLight, DirectionalLight, Color,
                Box3, Vector3, SRGBColorSpace } = THREE_CORE;

        // Eklenti modulleri
        const { GLTFLoader } = await import(`${T}/examples/jsm/loaders/GLTFLoader.js`);
        const { OrbitControls } = await import(`${T}/examples/jsm/controls/OrbitControls.js`);

        // Modul referanslarini sakla (ileride kullanmak icin)
        this._moduller = { Scene, PerspectiveCamera, WebGLRenderer,
            AmbientLight, DirectionalLight, Color,
            Box3, Vector3, GLTFLoader, OrbitControls, SRGBColorSpace };

        // ── Sahne ──
        this._scene = new Scene();
        this._scene.background = new Color(0xd4d4d4);

        // ── Kamera ──
        const enBoy = this._kapsayici.clientWidth / Math.max(this._kapsayici.clientHeight, 1);
        this._camera = new PerspectiveCamera(45, enBoy, 0.1, 1000);
        this._camera.position.set(3, 2, 3);

        // ── Renderer ──
        this._renderer = new WebGLRenderer({
            antialias: true,
            alpha: false,
            powerPreference: 'high-performance'
        });
        this._renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        this._renderer.setSize(this._kapsayici.clientWidth, this._kapsayici.clientHeight);
        this._renderer.physicallyCorrectLights = true;
        this._renderer.outputColorSpace = SRGBColorSpace || 'srgb';

        // ── Isiklar ──
        const ortamIsik = new AmbientLight(0xffffff, 0.6);
        this._scene.add(ortamIsik);

        const yonluIsik = new DirectionalLight(0xffffff, 1.2);
        yonluIsik.position.set(5, 10, 7.5);
        this._scene.add(yonluIsik);

        // ── OrbitControls ──
        this._controls = new OrbitControls(this._camera, this._renderer.domElement);
        this._controls.enableDamping = true;
        this._controls.dampingFactor = 0.08;
        this._controls.minDistance = 0.5;
        this._controls.maxDistance = 10;
        this._controls.target.set(0, 0, 0);

        // ── Pencere resize ──
        const resizeIsleyici = () => this.yenidenBoyutlandir();
        window.addEventListener('resize', resizeIsleyici);
        this._eventListenerlar.push({ hedef: window, tur: 'resize', isleyici: resizeIsleyici });

        // ── Canvas'i DOM'a ekle ──
        this._kapsayici.appendChild(this._renderer.domElement);

        // ── Animasyon dongusu ──
        this._animasyonDongusuBaslat();
    }

    /**
     * Animasyon dongusunu baslatir.
     * Dirty flag (_temizlemeGerekiyor) kontrolu yapar.
     */
    _animasyonDongusuBaslat() {
        const dongu = () => {
            this._animasyonId = requestAnimationFrame(dongu);

            // Temizleme gerekiyorsa once onu yap
            if (this._temizlemeGerekiyor) {
                this._oncekiModeliTemizle();
                this._temizlemeGerekiyor = false;
            }

            if (this._controls) {
                this._controls.update();
            }

            if (this._renderer && this._scene && this._camera) {
                this._renderer.render(this._scene, this._camera);
            }
        };
        this._animasyonId = requestAnimationFrame(dongu);
    }

    /**
     * URL'den GLB/GLTF model yukler.
     * Onceki model temizlenir, yeni model sahneye eklenir, ortalanir.
     *
     * @param {string} url - Model dosyasinin BFF proxy URL'i
     * @returns {Promise<void>}
     */
    async modelYukle(url) {
        if (!url) {
            if (this._secenekler.onHata) {
                this._secenekler.onHata(new Error('Model URL bos.'));
            }
            return;
        }

        // Dirty flag ile temizleme talep et
        this._temizlemeGerekiyor = true;

        if (this._secenekler.onYukleniyor) {
            this._secenekler.onYukleniyor();
        }

        try {
            const { GLTFLoader } = this._moduller;
            const loader = new GLTFLoader();

            const gltf = await new Promise((coz, red) => {
                loader.load(
                    url,
                    (sonuc) => coz(sonuc),
                    undefined,
                    (hata) => red(hata)
                );
            });

            // Dirty flag'i sifirla ve hemen temizle
            this._temizlemeGerekiyor = false;
            this._oncekiModeliTemizle();

            // Yeni modeli sahneye ekle
            this._mevcutModel = gltf.scene;
            this._scene.add(this._mevcutModel);

            // Modeli ortala ve sigdir
            this._ortalaVeSigdir();

            if (this._secenekler.onYuklendi) {
                this._secenekler.onYuklendi(gltf);
            }
        } catch (hata) {
            console.error('[UcBoyutGoruntuleyici] Model yukleme hatasi:', hata);
            if (this._secenekler.onHata) {
                this._secenekler.onHata(hata);
            }
        }
    }

    /**
     * Sahnede bir onceki modeli temizler.
     * Tum geometry, material ve texture'lari dispose eder.
     */
    _oncekiModeliTemizle() {
        if (!this._mevcutModel) return;

        this._mevcutModel.traverse((cocuk) => {
            if (cocuk.geometry) {
                cocuk.geometry.dispose();
            }

            if (cocuk.material) {
                if (Array.isArray(cocuk.material)) {
                    cocuk.material.forEach((m) => this._materyalTemizle(m));
                } else {
                    this._materyalTemizle(cocuk.material);
                }
            }
        });

        this._scene.remove(this._mevcutModel);
        this._mevcutModel = null;
    }

    /**
     * Tek bir materyalin tum texture'larini dispose eder.
     * @param {Material} materyal
     */
    _materyalTemizle(materyal) {
        if (!materyal) return;

        const textureAlanlari = [
            'map', 'normalMap', 'roughnessMap', 'metalnessMap',
            'alphaMap', 'emissiveMap', 'aoMap', 'envMap',
            'lightMap', 'bumpMap', 'displacementMap'
        ];

        for (const alan of textureAlanlari) {
            if (materyal[alan]) {
                materyal[alan].dispose();
            }
        }

        materyal.dispose();
    }

    /**
     * Modeli sahnenin merkezine ortalar ve kamera mesafesini boyuta gore ayarlar.
     */
    _ortalaVeSigdir() {
        if (!this._mevcutModel || !this._camera || !this._controls) return;

        const { Box3, Vector3 } = this._moduller;

        const kutu = new Box3().setFromObject(this._mevcutModel);

        // Bos kutu kontrolu
        if (kutu.isEmpty()) return;

        const boyut = new Vector3();
        const merkez = new Vector3();
        kutu.getSize(boyut);
        kutu.getCenter(merkez);

        // Modeli kendi merkezine oturt (origin'e kaydir)
        this._mevcutModel.position.set(-merkez.x, -merkez.y, -merkez.z);

        // En buyuk boyuta gore kamera mesafesi hesapla
        const enBuyukBoyut = Math.max(boyut.x, boyut.y, boyut.z, 0.1);
        const mesafe = enBuyukBoyut * 2.2;

        this._camera.position.set(mesafe * 0.8, mesafe * 0.6, mesafe * 0.8);
        this._camera.lookAt(0, 0, 0);

        // OrbitControls hedefini sifirla
        this._controls.target.set(0, 0, 0);
        this._controls.update();
    }

    /**
     * Yuklenmis modeldeki tum mesh isimlerini dondurur.
     *
     * Isimsiz mesh'ler icin deterministik teknik tanimlayici (mesh_0, mesh_1, ...)
     * uretir — traversal sirasi baz alinir, anlamsal tahmin YAPILMAZ.
     *
     * Ayni isme sahip birden fazla mesh varsa sadece ilk karsilasilan dondurulur.
     *
     * @returns {string[]} Essiz mesh isimleri dizisi
     */
    meshIsimleriGetir() {
        if (!this._mevcutModel) return [];

        /** @type {string[]} */
        const isimler = [];
        /** @type {Set<string>} */
        const gorulen = new Set();
        let sayac = 0;

        this._mevcutModel.traverse((dugum) => {
            if (!dugum.isMesh) return;

            let isim = dugum.name;
            if (!isim || isim.trim() === '') {
                isim = `mesh_${sayac}`;
            }
            sayac++;

            if (!gorulen.has(isim)) {
                gorulen.add(isim);
                isimler.push(isim);
            }
        });

        return isimler;
    }

    /**
     * Belirtilen isimdeki mesh'i secer ve highlight uygular.
     *
     * ISIMLI mesh'ler secilebilir; isimsiz (mesh_N) olanlar secilmez.
     * Orijinal materyal emissive durumu saklanir; meshSecimiTemizle() ile
     * geri yuklenebilir.
     *
     * Renk davranisi (diffuse, specular, base color) DEGISTIRILMEZ —
     * sadece gecici emissive highlight eklenir.
     *
     * @param {string} meshAdi - Secilecek mesh'in adi
     * @returns {boolean} - En az bir mesh bulunup secildiyse true
     */
    meshSec(meshAdi) {
        if (!this._mevcutModel || !meshAdi) return false;

        // Onceki secimi temizle
        this.meshSecimiTemizle();
        this._seciliMeshler = [];

        this._mevcutModel.traverse((dugum) => {
            if (!dugum.isMesh) return;

            const isim = dugum.name;
            // Sadece isimli mesh'ler secilebilir
            if (!isim || isim.trim() === '' || isim !== meshAdi) return;

            const materyal = dugum.material;
            if (!materyal || !materyal.emissive) return;

            // Orijinal durumu sakla
            this._seciliMeshler.push({
                mesh: dugum,
                orijinalEmissiveHex: materyal.emissive.getHex(),
                orijinalEmissiveIntensity: materyal.emissiveIntensity ?? 0
            });

            // Gecici highlight: base color DEGISMEZ, sadece emissive
            materyal.emissive.setHex(0x444444);
            materyal.emissiveIntensity = 0.4;
        });

        return this._seciliMeshler.length > 0;
    }

    /**
     * Tum mesh secimlerini temizler ve orijinal materyal durumunu geri yukler.
     *
     * Highlight sirasinda degistirilen emissive ve emissiveIntensity
     * degerleri eski haline dondurulur. Saklanan referanslar null'lanarak
     * GC'ye yardim edilir.
     */
    meshSecimiTemizle() {
        if (!this._seciliMeshler || this._seciliMeshler.length === 0) return;

        for (const kayit of this._seciliMeshler) {
            const mat = kayit.mesh?.material;
            if (!mat?.emissive) {
                kayit.mesh = null;
                continue;
            }

            mat.emissive.setHex(kayit.orijinalEmissiveHex);
            mat.emissiveIntensity = kayit.orijinalEmissiveIntensity;

            kayit.mesh = null; // GC yardim
        }
        this._seciliMeshler = [];
    }

    /**
     * Renderer ve kamera boyutlarini kapsayici elemente gore gunceller.
     * Pencere resize olayinda cagrilir.
     */
    yenidenBoyutlandir() {
        if (!this._renderer || !this._camera || !this._kapsayici) return;

        const genislik = this._kapsayici.clientWidth;
        const yukseklik = this._kapsayici.clientHeight;

        if (genislik <= 0 || yukseklik <= 0) return;

        this._camera.aspect = genislik / yukseklik;
        this._camera.updateProjectionMatrix();
        this._renderer.setSize(genislik, yukseklik);
    }

    /**
     * Tum kaynaklari serbest birakir: animasyon, renderer, model, event listener'lar.
     * Cagrildiktan sonra viewer tekrar kullanilamaz.
     */
    yokEt() {
        // Animasyon dongusunu durdur
        if (this._animasyonId !== null) {
            cancelAnimationFrame(this._animasyonId);
            this._animasyonId = null;
        }

        // Mevcut modeli temizle
        this._oncekiModeliTemizle();

        // OrbitControls
        if (this._controls) {
            this._controls.dispose();
            this._controls = null;
        }

        // Renderer
        if (this._renderer) {
            this._renderer.dispose();
            this._renderer.forceContextLoss?.();

            // Canvas'i DOM'dan kaldir
            const canvas = this._renderer.domElement;
            if (canvas && canvas.parentNode) {
                canvas.parentNode.removeChild(canvas);
            }

            this._renderer = null;
        }

        // Event listener'lari temizle
        for (const { hedef, tur, isleyici } of this._eventListenerlar) {
            hedef.removeEventListener(tur, isleyici);
        }
        this._eventListenerlar = [];

        // Secili mesh kayitlarini temizle
        if (this._seciliMeshler) {
            for (const kayit of this._seciliMeshler) {
                kayit.mesh = null;
            }
            this._seciliMeshler = [];
        }

        // Referanslari sifirla
        this._scene = null;
        this._camera = null;
        this._mevcutModel = null;
        this._moduller = null;
    }
}

// ═══════════════════════════════════════════════════════════════
// Blazor JS Interop Bridge (window kapsaminda)
// ═══════════════════════════════════════════════════════════════

/** @type {UcBoyutGoruntuleyici|null} */
let _aktifGoruntuleyici = null;
/** @type {DotNet.DotNetObject|null} */
let _dotNetRef = null;

/**
 * Blazor tarafindan cagrilan baslatma fonksiyonu.
 * @param {DotNet.DotNetObject} dotNetRef - .NET callback referansi
 * @param {string} elementId - Canvas host element ID'si
 */
async function baslatGoruntuleyici(dotNetRef, elementId) {
    if (_aktifGoruntuleyici) {
        _aktifGoruntuleyici.yokEt();
        _aktifGoruntuleyici = null;
    }

    const kapsayici = document.getElementById(elementId);
    if (!kapsayici) {
        console.error('[UcBoyutGoruntuleyici] Kapsayici element bulunamadi:', elementId);
        return;
    }

    _dotNetRef = dotNetRef;

    _aktifGoruntuleyici = new UcBoyutGoruntuleyici(kapsayici, {
        onYukleniyor: () => {
            if (_dotNetRef) {
                _dotNetRef.invokeMethodAsync('OnModelYukleniyor');
            }
        },
        onYuklendi: () => {
            if (_dotNetRef) {
                _dotNetRef.invokeMethodAsync('OnModelYuklendi');
            }
        },
        onHata: (hata) => {
            if (_dotNetRef) {
                const mesaj = hata?.message || hata?.toString() || 'Bilinmeyen hata';
                _dotNetRef.invokeMethodAsync('OnModelHata', mesaj);
            }
        }
    });

    await _aktifGoruntuleyici.init();
}

/**
 * Blazor tarafindan cagrilan model yukleme fonksiyonu.
 * @param {string} url - BFF proxy GLB URL
 */
async function modelYukle(url) {
    if (!_aktifGoruntuleyici) {
        console.error('[UcBoyutGoruntuleyici] Goruntuleyici baslatilmadi.');
        return;
    }

    await _aktifGoruntuleyici.modelYukle(url);
}

/**
 * Blazor tarafindan cagrilan temizleme fonksiyonu.
 */
function yokEtGoruntuleyici() {
    if (_aktifGoruntuleyici) {
        _aktifGoruntuleyici.yokEt();
        _aktifGoruntuleyici = null;
    }
    _dotNetRef = null;
}

/**
 * Blazor tarafindan cagrilan mesh isimleri getirme fonksiyonu.
 * @returns {string[]} Essiz mesh isimleri dizisi
 */
function meshIsimleriGetir() {
    if (!_aktifGoruntuleyici) {
        console.error('[UcBoyutGoruntuleyici] Goruntuleyici baslatilmadi.');
        return [];
    }
    return _aktifGoruntuleyici.meshIsimleriGetir();
}

/**
 * Blazor tarafindan cagrilan mesh secme fonksiyonu.
 * @param {string} meshAdi - Secilecek mesh'in adi
 * @returns {boolean} - Secim basariliysa true
 */
function meshSec(meshAdi) {
    if (!_aktifGoruntuleyici) {
        console.error('[UcBoyutGoruntuleyici] Goruntuleyici baslatilmadi.');
        return false;
    }
    return _aktifGoruntuleyici.meshSec(meshAdi);
}

/**
 * Blazor tarafindan cagrilan mesh secimi temizleme fonksiyonu.
 */
function meshSecimiTemizle() {
    if (_aktifGoruntuleyici) {
        _aktifGoruntuleyici.meshSecimiTemizle();
    }
}

// window kapsamina ata
window.baslatGoruntuleyici = baslatGoruntuleyici;
window.modelYukle = modelYukle;
window.yokEtGoruntuleyici = yokEtGoruntuleyici;
window.meshIsimleriGetir = meshIsimleriGetir;
window.meshSec = meshSec;
window.meshSecimiTemizle = meshSecimiTemizle;
window.UcBoyutGoruntuleyici = UcBoyutGoruntuleyici;

export { UcBoyutGoruntuleyici, baslatGoruntuleyici, modelYukle, yokEtGoruntuleyici, meshIsimleriGetir, meshSec, meshSecimiTemizle };
