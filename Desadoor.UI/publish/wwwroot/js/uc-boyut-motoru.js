/**
 * DESADOOR — ÜÇ BOYUT MOTORU (THREE.JS TÜRKÇE SARMALAYICI)
 * =============================================================
 * Bu dosya, Three.js kütüphanesini doğrudan kullanmak yerine
 * tüm 3D görüntüleme işlemlerini Türkçe fonksiyon adlarıyla
 * sarmalar. Blazor tarafı yalnızca bu arayüzü bilir, Three.js'i değil.
 *
 * KURALLAR.md §2 uyarınca: Harici kütüphane doğrudan iş mantığında kullanılamaz.
 * Tüm çağrılar bu wrapper üzerinden geçer.
 */

window.UcBoyutMotoru = (function () {
    // =============================================
    // ÖZEL DEĞİŞKENLER (Private State)
    // Her kanvas için ayrı sahne tutulur (çoklu viewer desteği)
    // =============================================
    const _sahneler = {};

    /**
     * Belirtilen kanvas kimliğine ait sahne nesnesini döndürür.
     * Eğer sahne yoksa null döner.
     */
    function _sahneGetir(kanvasId) {
        return _sahneler[kanvasId] || null;
    }

    /**
     * Sahne için animasyon döngüsünü başlatır.
     * requestAnimationFrame ile sürekli render yapılır.
     */
    function _animasyonDongusu(kanvasId) {
        const veri = _sahneGetir(kanvasId);
        if (!veri || veri.durduruldu) return;

        veri.animKaresi = requestAnimationFrame(() => _animasyonDongusu(kanvasId));

        // Otomatik döndürme aktifse modeli yavaşça döndür
        if (veri.otomatikDondurmeMi && veri.model) {
            veri.model.rotation.y += 0.005;
        }

        // Kontrol güncellemesi (mouse/touch drag)
        if (veri.kontroller) {
            veri.kontroller.update();
        }

        veri.renderer.render(veri.sahne, veri.kamera);
    }

    /**
     * Boyutlandırma olayını işler — pencere yeniden boyutlandırıldığında
     * kamera ve renderer güncellenir, görüntü bozulmaz.
     */
    function _yenidenBoyutlandir(kanvasId) {
        const veri = _sahneGetir(kanvasId);
        if (!veri) return;

        const genislik = veri.konteyner.clientWidth;
        const yukseklik = veri.konteyner.clientHeight;

        veri.kamera.aspect = genislik / yukseklik;
        veri.kamera.updateProjectionMatrix();
        veri.renderer.setSize(genislik, yukseklik);
    }

    // =============================================
    // GENEL (PUBLIC) API — Blazor bu fonksiyonları çağırır
    // =============================================
    return {

        /**
         * Belirtilen kanvas elementine 3D sahneyi başlatır.
         * Three.js Sahne, Kamera, Renderer ve OrbitControls oluşturulur.
         * Varsayılan model olarak parametrik bir kapak geometrisi çizilir.
         *
         * @param {string} kanvasId - HTML kanvas elementinin ID'si
         * @param {string|null} modelYolu - GLB/GLTF dosya yolu (opsiyonel)
         * @param {string} baslangicRenk - Başlangıç rengi (hex, örn: "#FFFFFF")
         */
        baslat: function (kanvasId, modelYolu, baslangicRenk) {
            // Zaten başlatılmışsa temizle
            if (_sahneler[kanvasId]) {
                this.temizle(kanvasId);
            }

            const konteyner = document.getElementById(kanvasId);
            if (!konteyner) {
                console.warn('[UcBoyutMotoru] Konteyner bulunamadı:', kanvasId);
                return;
            }

            // Three.js kontrolü
            if (typeof THREE === 'undefined') {
                console.error('[UcBoyutMotoru] Three.js yüklenmemiş!');
                return;
            }

            // --- SAHNE ---
            const sahne = new THREE.Scene();
            sahne.background = new THREE.Color(0xEAE3D8); // Banyo sıcak ton

            // Hafif sis efekti (derinlik hissi)
            sahne.fog = new THREE.Fog(0xF8F8F8, 10, 50);

            // --- KAMERA ---
            const genislik = konteyner.clientWidth || 600;
            const yukseklik = konteyner.clientHeight || 600;
            const kamera = new THREE.PerspectiveCamera(35, genislik / yukseklik, 0.1, 100);
            kamera.position.set(0, 0.5, 4);

            // --- IŞIKLANDIRMA ---
            // Ortam ışığı (yumuşak genel aydınlatma)
            const ortamIsik = new THREE.AmbientLight(0xffffff, 0.35);

            // Ana ışık
            const anaIsik = new THREE.DirectionalLight(0xffffff, 0.50);
            anaIsik.position.set(2, 4, 3);
            anaIsik.castShadow = true;
            sahne.add(anaIsik);

            // Dolgu ışığı (sağ taraftan yumuşak)
            const dolguIsik = new THREE.DirectionalLight(0xffffff, 0.20);
            dolguIsik.position.set(-3, 1, -2);
            sahne.add(dolguIsik);

            // Zemin yansıması ışığı
            const yansımaIsik = new THREE.HemisphereLight(0xffffff, 0xcccccc, 0.2);
            sahne.add(yansımaIsik);

            // --- RENDERER ---
            const renderer = new THREE.WebGLRenderer({
                antialias: true,
                alpha: false
            });
            renderer.setSize(genislik, yukseklik);
            renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
            renderer.shadowMap.enabled = true;
            renderer.shadowMap.type = THREE.PCFSoftShadowMap;
            renderer.outputEncoding = THREE.sRGBEncoding;
            renderer.toneMapping = THREE.LinearToneMapping; // ACESFilmic renkleri fazla kaydırabilir, Linear daha sadıktır
            renderer.toneMappingExposure = 0.95;

            // --- HDR CEVRE HARITASI (gercekci yansima — anayasa §5.3) ---
            if (typeof THREE.RGBELoader !== 'undefined' && typeof THREE.PMREMGenerator !== 'undefined') {
                const pmrem = new THREE.PMREMGenerator(renderer);
                pmrem.compileEquirectangularShader();
                new THREE.RGBELoader()
                    .setDataType(THREE.HalfFloatType)
                    .load('https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/studio_country_hall_1k.hdr',
                        function (doku) {
                            const envHarita = pmrem.fromEquirectangular(doku).texture;
                            sahne.environment = envHarita;
                            sahne.background = new THREE.Color(0xF8F8F8);
                            doku.dispose();
                            console.log('[UcBoyutMotoru] HDR cevre haritasi yuklendi.');
                        },
                        undefined,
                        function () {
                            console.warn('[UcBoyutMotoru] HDR yuklenemedi, varsayilan aydinlatma kullaniliyor.');
                        }
                    );
            }

            // Kanvas'ı konteyner'a ekle
            konteyner.innerHTML = '';
            konteyner.appendChild(renderer.domElement);

            // --- KONTROLLer (OrbitControls) ---
            let kontroller = null;
            if (typeof THREE.OrbitControls !== 'undefined') {
                kontroller = new THREE.OrbitControls(kamera, renderer.domElement);
                kontroller.enableDamping = true;
                kontroller.dampingFactor = 0.08;
                kontroller.enableZoom = true;
                kontroller.minDistance = 1.5;
                kontroller.maxDistance = 8;
                kontroller.maxPolarAngle = Math.PI * 0.75;
                kontroller.minPolarAngle = Math.PI * 0.1;
                kontroller.autoRotate = false;
                kontroller.enablePan = false;
            }

            // --- PARÇA SEÇME (Tıklama ile) ---
            const isaretci = new THREE.Vector2();
            const isinIzgarasi = new THREE.Raycaster();
            
            const parcaSecTiklayici = function (olay) {
                const mevcutModel = _sahneGetir(kanvasId)?.model;
                if (!mevcutModel) return;
                
                const rect = renderer.domElement.getBoundingClientRect();
                isaretci.x = ((olay.clientX - rect.left) / rect.width) * 2 - 1;
                isaretci.y = -((olay.clientY - rect.top) / rect.height) * 2 + 1;

                isinIzgarasi.setFromCamera(isaretci, kamera);
                const kesisme = isinIzgarasi.intersectObjects(mevcutModel.children, true);

                if (kesisme.length > 0) {
                    const tiklanan = kesisme[0].object;
                    const parcaIsmi = tiklanan.name || '(isimsiz)';
                    const veri = _sahneGetir(kanvasId);

                    // Seçim vurgusunu kaldır
                    if (veri && veri.seciliParca && veri.seciliParca.material) {
                        const mat = veri.seciliParca.material;
                        if (mat.emissive && veri.seciliParcaOrijinalEmissive) {
                            mat.emissive.copy(veri.seciliParcaOrijinalEmissive);
                            mat.emissiveIntensity = 0;
                        }
                    }

                    // Yeni seçimi vurgula
                    if (tiklanan.material) {
                        veri.seciliParca = tiklanan;
                        const mat = Array.isArray(tiklanan.material) ? tiklanan.material[0] : tiklanan.material;
                        if (!veri.seciliParcaOrijinalEmissive && mat.emissive) {
                            veri.seciliParcaOrijinalEmissive = mat.emissive.clone();
                        }
                        mat.emissive = new THREE.Color(0x00ff00);
                        mat.emissiveIntensity = 0.3;
                    }

                    if (veri && veri.parcaSecildiCallback) {
                        veri.parcaSecildiCallback.invokeMethodAsync('ParcaSecildi', parcaIsmi);
                    }
                }
            };
            renderer.domElement.addEventListener('click', parcaSecTiklayici);

            // --- VARSAYILAN MODEL (Parametrik Kapak Geometrisi) ---
            // Gerçek GLB dosyası olmayana kadar programatik kapak çizilir
            const secilenRenk = baslangicRenk || '#E8E4DF';
            const model = this._parametrikKapakOlustur(sahne, secilenRenk);

            // --- ZEMIN GÖLGESİ ---
            const zeminGeo = new THREE.PlaneGeometry(10, 10);
            const zeminMat = new THREE.ShadowMaterial({ opacity: 0.1 });
            const zemin = new THREE.Mesh(zeminGeo, zeminMat);
            zemin.rotation.x = -Math.PI / 2;
            zemin.position.y = -1.1;
            zemin.receiveShadow = true;
            sahne.add(zemin);

            // --- DURUM KAYDET ---
            _sahneler[kanvasId] = {
                sahne, kamera, renderer, kontroller, model,
                konteyner,
                otomatikDondurmeMi: false,
                durduruldu: false,
                animKaresi: null,
                secilenRenk,
                seciliParca: null,
                seciliParcaOrijinalEmissive: null,
                parcaSecildiCallback: null
            };

            // Pencere boyut olayı
            const boyutFonk = () => _yenidenBoyutlandir(kanvasId);
            window.addEventListener('resize', boyutFonk);
            _sahneler[kanvasId].boyutFonk = boyutFonk;

            // Animasyon döngüsünü başlat
            _animasyonDongusu(kanvasId);

            // Gerçek model dosyası varsa yükle
            if (modelYolu && modelYolu.length > 0) {
                this.modeli_yukle(kanvasId, modelYolu);
            }

            console.log('[UcBoyutMotoru] Sahne başlatıldı:', kanvasId);
        },

        /**
         * Parametrik kapak geometrisi oluşturur.
         * Gerçek 3D model dosyası olmadığında gösterilecek
         * gerçekçi kapak formu.
         */
        _parametrikKapakOlustur: function (sahne, renk) {
            const grup = new THREE.Group();

            // Ana kapak gövdesi
            const govdeGeo = new THREE.BoxGeometry(1.4, 2.0, 0.06);
            const govdeMat = new THREE.MeshStandardMaterial({
                color: new THREE.Color(renk).convertSRGBToLinear(), // sRGB -> Linear dönüşümü
                roughness: 0.3, // Parlaklığı biraz azaltarak rengin patlamasını önle
                metalness: 0.0,
            });
            const govde = new THREE.Mesh(govdeGeo, govdeMat);
            govde.castShadow = true;
            govde.userData.anaRenk = true; // Renk değişimi bu mesh'e uygulanır
            grup.add(govde);

            // Çerçeve profili — üst
            const cerceveMat = new THREE.MeshStandardMaterial({
                color: new THREE.Color(renk).convertSRGBToLinear(),
                roughness: 0.3,
                metalness: 0.0
            });
            const ustCerceve = new THREE.Mesh(
                new THREE.BoxGeometry(1.4, 0.08, 0.085),
                cerceveMat.clone()
            );
            ustCerceve.position.set(0, 0.96, 0.01);
            ustCerceve.castShadow = true;
            ustCerceve.userData.cerceve = true;
            grup.add(ustCerceve);

            // Alt çerçeve
            const altCerceve = new THREE.Mesh(
                new THREE.BoxGeometry(1.4, 0.08, 0.085),
                cerceveMat.clone()
            );
            altCerceve.position.set(0, -0.96, 0.01);
            altCerceve.castShadow = true;
            altCerceve.userData.cerceve = true;
            grup.add(altCerceve);

            // Sol çerçeve
            const solCerceve = new THREE.Mesh(
                new THREE.BoxGeometry(0.08, 2.0, 0.085),
                cerceveMat.clone()
            );
            solCerceve.position.set(-0.66, 0, 0.01);
            solCerceve.castShadow = true;
            solCerceve.userData.cerceve = true;
            grup.add(solCerceve);

            // Sağ çerçeve
            const sagCerceve = new THREE.Mesh(
                new THREE.BoxGeometry(0.08, 2.0, 0.085),
                cerceveMat.clone()
            );
            sagCerceve.position.set(0.66, 0, 0.01);
            sagCerceve.castShadow = true;
            sagCerceve.userData.cerceve = true;
            grup.add(sagCerceve);

            // Kapı kolu
            const kolGeo = new THREE.CylinderGeometry(0.015, 0.015, 0.3, 16);
            const kolMat = new THREE.MeshStandardMaterial({
                color: 0xC0C0C0,
                roughness: 0.05,
                metalness: 0.9
            });
            const kol = new THREE.Mesh(kolGeo, kolMat);
            kol.rotation.z = Math.PI / 2;
            kol.position.set(0.5, 0, 0.1);
            kol.castShadow = true;
            grup.add(kol);

            // Kol tutucu - üst
            const tutucuGeo = new THREE.CylinderGeometry(0.025, 0.025, 0.05, 16);
            const tutucuMat = kolMat.clone();
            const tutucuUst = new THREE.Mesh(tutucuGeo, tutucuMat);
            tutucuUst.position.set(0.35, 0, 0.1);
            tutucuUst.castShadow = true;
            grup.add(tutucuUst);

            const tutucuSag = new THREE.Mesh(tutucuGeo.clone(), tutucuMat.clone());
            tutucuSag.position.set(0.65, 0, 0.1);
            tutucuSag.castShadow = true;
            grup.add(tutucuSag);

            sahne.add(grup);
            return grup;
        },

        /**
         * Dışarıdan GLB/GLTF model dosyası yükler.
         * Yükleme tamamlandığında mevcut parametrik model kaldırılır.
         */
        modeli_yukle: function (kanvasId, modelYolu) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;

            if (typeof THREE.GLTFLoader === 'undefined') {
                console.warn('[UcBoyutMotoru] GLTFLoader bulunamadı, parametrik model kullanılıyor.');
                return;
            }

            const yukleme = new THREE.GLTFLoader();

            // DRACO sıkıştırılmış model desteği (anayasa §5.2 — 10MB → 800KB)
            if (typeof THREE.DRACOLoader !== 'undefined') {
                const dracoYukleyici = new THREE.DRACOLoader();
                dracoYukleyici.setDecoderPath('https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/libs/draco/');
                yukleme.setDRACOLoader(dracoYukleyici);
                console.log('[UcBoyutMotoru] DRACO dekompresyon aktif.');
            }

            yukleme.load(
                modelYolu,
                (gltf) => {
                    // Eski modeli kaldır
                    if (veri.model) {
                        veri.sahne.remove(veri.model);
                    }

                    const yeniModel = gltf.scene;
                    yeniModel.castShadow = true;

                    // Modeli ortala ve boyutlandır
                    const kutu = new THREE.Box3().setFromObject(yeniModel);
                    const merkez = kutu.getCenter(new THREE.Vector3());
                    const boyut = kutu.getSize(new THREE.Vector3());
                    const maxBoyut = Math.max(boyut.x, boyut.y, boyut.z);
                    const olcek = 2.0 / maxBoyut;

                    yeniModel.position.sub(merkez.multiplyScalar(olcek));
                    yeniModel.scale.setScalar(olcek);

                    veri.sahne.add(yeniModel);
                    veri.model = yeniModel;

                    // Başlangıç rengini uygula
                    if (veri.secilenRenk) {
                        this.renk_uygula(kanvasId, veri.secilenRenk);
                    }

                    console.log('[UcBoyutMotoru] Model yüklendi:', modelYolu);
                },
                (progress) => {
                    const yuzde = (progress.loaded / progress.total * 100).toFixed(0);
                    console.log('[UcBoyutMotoru] Yükleniyor:', yuzde + '%');
                },
                (hata) => {
                    console.error('[UcBoyutMotoru] Model yüklenemedi:', hata);
                }
            );
        },

        /**
         * 3D modelin rengini anında değiştirir.
         * Tüm mesh'lerin materyali güncellenir.
         *
         * @param {string} kanvasId - Hangi sahnenin rengi değişecek
         * @param {string} renkHex - Hex renk kodu (örn: "#FF5733")
         */
        renk_uygula: function (kanvasId, renkHex) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            veri.secilenRenk = renkHex;
            const yeniRenk = new THREE.Color(renkHex).convertSRGBToLinear(); // sRGB -> Linear dönüşümü zorunlu

            // Grup içindeki tüm mesh'leri gez
            veri.model.traverse((nesne) => {
                if (nesne.isMesh && nesne.material) {
                    // Çerçeve ve gövde rengi değişir, metal kol değişmez
                    if (nesne.userData.anaRenk || nesne.userData.cerceve) {
                        if (Array.isArray(nesne.material)) {
                            nesne.material.forEach(mat => {
                                mat.color.set(yeniRenk);
                                mat.needsUpdate = true;
                            });
                        } else {
                            nesne.material.color.set(yeniRenk);
                            nesne.material.needsUpdate = true;
                        }
                    } else if (!nesne.userData.metal) {
                        // GLB modelinde userData yoksa tüm mesh'lere uygula
                        if (Array.isArray(nesne.material)) {
                            nesne.material.forEach(mat => {
                                mat.color.set(yeniRenk);
                                mat.needsUpdate = true;
                            });
                        } else if (nesne.material.color) {
                            nesne.material.color.set(yeniRenk);
                            nesne.material.needsUpdate = true;
                        }
                    }
                }
            });

            console.log('[UcBoyutMotoru] Renk uygulandı:', renkHex);
        },

        /**
         * Otomatik döndürme animasyonunu açar veya kapatır.
         * Mouse ile döndürme sırasında otomatik döndürme durur,
         * 2 saniye sonra tekrar başlar.
         *
         * @param {string} kanvasId - Hedef sahne
         * @param {boolean} aktifMi - true = açık, false = kapalı
         */
        otomatik_dondur: function (kanvasId, aktifMi) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;

            veri.otomatikDondurmeMi = aktifMi;

            if (veri.kontroller) {
                veri.kontroller.autoRotate = aktifMi;
                veri.kontroller.autoRotateSpeed = 1.5;
            }
        },

        /**
         * Görüntüleyiciyi tam ekran moduna alır.
         * Tarayıcının Fullscreen API'si kullanılır.
         *
         * @param {string} kanvasId - Tam ekrana alınacak konteyner
         */
        tam_ekran: function (kanvasId) {
            const konteyner = document.getElementById(kanvasId);
            if (!konteyner) return;

            if (document.fullscreenElement) {
                document.exitFullscreen();
            } else {
                konteyner.requestFullscreen().catch(err => {
                    console.warn('[UcBoyutMotoru] Tam ekran başlatılamadı:', err);
                });
            }
        },

        /**
         * Kamerayı başlangıç pozisyonuna sıfırlar.
         * Kullanıcı çok uzaklaştı veya kaybettiyse kullanışlıdır.
         *
         * @param {string} kanvasId - Hedef sahne
         */
        kamera_sifirla: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;

            veri.kamera.position.set(0, 0.5, 4);
            veri.kamera.lookAt(0, 0, 0);

            if (veri.kontroller) {
                veri.kontroller.reset();
            }
        },

        /**
         * Belirtilen sahneyi tamamen temizler ve belleği serbest bırakır.
         * Bileşen kaldırıldığında (OnAfterRenderAsync/Dispose) çağrılmalıdır.
         * Bellek sızıntısını önler.
         *
         * @param {string} kanvasId - Temizlenecek sahne
         */
        temizle: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;

            veri.durduruldu = true;

            if (veri.animKaresi) {
                cancelAnimationFrame(veri.animKaresi);
            }

            if (veri.boyutFonk) {
                window.removeEventListener('resize', veri.boyutFonk);
            }

            if (veri.kontroller) {
                veri.kontroller.dispose();
            }

            // Tüm geometri ve materyalleri temizle
            veri.sahne.traverse((nesne) => {
                if (nesne.isMesh) {
                    nesne.geometry.dispose();
                    if (Array.isArray(nesne.material)) {
                        nesne.material.forEach(m => m.dispose());
                    } else if (nesne.material) {
                        nesne.material.dispose();
                    }
                }
            });

            veri.renderer.dispose();

            if (veri.konteyner) {
                veri.konteyner.innerHTML = '';
            }

            delete _sahneler[kanvasId];
            console.log('[UcBoyutMotoru] Sahne temizlendi:', kanvasId);
        },

        /**
         * 3D Model uzerinde tiklanabilir noktalar (Hotspot) sistemini etkinlestirir.
         * Raycaster ile tiklanan mesh tespit edilir ve .NET callback'i cagrilir.
         *
         * @param {string} kanvasId - Hedef sahne
         * @param {string} dotNetRef - .NET nesne referansi (DotNetObjectReference)
         * @param {string} metodAdi - Cagrilacak C# metodu
         */
        hotspot_etkinlestir: function (kanvasId, dotNetRef, metodAdi) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.konteyner) return;

            // Raycaster olustur
            const isaretci = new THREE.Vector2();
            const isinIzgarasi = new THREE.Raycaster();

            veri.konteyner.addEventListener('click', function (olay) {
                const rect = veri.konteyner.getBoundingClientRect();
                isaretci.x = ((olay.clientX - rect.left) / rect.width) * 2 - 1;
                isaretci.y = -((olay.clientY - rect.top) / rect.height) * 2 + 1;

                isinIzgarasi.setFromCamera(isaretci, veri.kamera);

                if (veri.model) {
                    const kesismeNoktalari = isinIzgarasi.intersectObjects(veri.model.children, true);
                    if (kesismeNoktalari.length > 0) {
                        const tiklanan = kesismeNoktalari[0].object;
                        const noktaAdi = tiklanan.userData.hotspot || tiklanan.name || 'Bilinmeyen';
                        const nokta = kesismeNoktalari[0].point;

                        // .NET callback'ini cagir
                        if (dotNetRef && metodAdi) {
                            dotNetRef.invokeMethodAsync(metodAdi, noktaAdi, {
                                x: nokta.x.toFixed(3),
                                y: nokta.y.toFixed(3),
                                z: nokta.z.toFixed(3)
                            });
                        }
                    }
                }
            });

            console.log('[UcBoyutMotoru] Hotspot sistemi etkinlestirildi:', kanvasId);
        },

        /**
         * Yüklenen 3D modelin tüm parçalarını (node isimlerini) analiz eder.
         * Her mesh/node için: isim, tip, vertex/triangle sayısı döner.
         * Sonuç JSON string olarak döndürülür.
         */
        model_analiz_et: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return '[]';

            const parcalar = [];
            veri.model.traverse(function (nesne) {
                if (nesne.isMesh) {
                    parcalar.push({
                        isim: nesne.name || '(isimsiz)',
                        tip: 'Mesh',
                        ucgenSayisi: nesne.geometry.index
                            ? Math.floor(nesne.geometry.index.count / 3)
                            : Math.floor((nesne.geometry.attributes.position?.count || 0) / 3),
                        gorunur: nesne.visible
                    });
                } else if (nesne.isObject3D && nesne !== veri.model && nesne.children.length > 0) {
                    parcalar.push({
                        isim: nesne.name || '(isimsiz)',
                        tip: 'Grup',
                        cocukSayisi: nesne.children.length,
                        gorunur: nesne.visible
                    });
                }
            });

            console.log('[UcBoyutMotoru] Model analizi:', parcalar);
            return JSON.stringify(parcalar);
        },

        /**
         * Belirtilen isimdeki mesh/grubun görünürlüğünü değiştirir.
         * @param {string} kanvasId - Hedef sahne
         * @param {string} parcaIsmi - Gizlenecek/gösterilecek parça adı
         * @param {boolean} gorunurMu - true = göster, false = gizle
         */
        parca_gorunurluk: function (kanvasId, parcaIsmi, gorunurMu) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            veri.model.traverse(function (nesne) {
                if (nesne.name === parcaIsmi) {
                    nesne.visible = gorunurMu;
                }
            });
            console.log('[UcBoyutMotoru] Parça görünürlük:', parcaIsmi, gorunurMu ? 'göster' : 'gizle');
        },

        /**
         * Belirtilen isimdeki mesh'in rengini değiştirir.
         */
        parca_renk: function (kanvasId, parcaIsmi, renkHex) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            const yeniRenk = new THREE.Color(renkHex).convertSRGBToLinear();
            veri.model.traverse(function (nesne) {
                if (nesne.name === parcaIsmi && nesne.isMesh && nesne.material) {
                    if (Array.isArray(nesne.material)) {
                        nesne.material.forEach(m => { if (m.color) m.color.set(yeniRenk); });
                    } else if (nesne.material.color) {
                        nesne.material.color.set(yeniRenk);
                    }
                }
            });
        },

        /**
         * Belirtilen isimdeki mesh'i derece cinsinden döndürür (Y ekseninde).
         * Kapak açma/kapama için kullanılır.
         * @param {number} derece — 0 = kapalı, 30-90 = açık
         */
        kapak_derece: function (kanvasId, parcaIsmi, derece) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            const radyan = (derece * Math.PI) / 180;
            veri.model.traverse(function (nesne) {
                if (nesne.name === parcaIsmi) {
                    nesne.rotation.y = radyan;
                }
            });
        },

        /**
         * Belirtilen isimdeki mesh'in malzemesini değiştirir.
         * @param {string} malzeme — "metal", "cam", "ayna", "ahsap", "plastik"
         */
        parca_malzeme: function (kanvasId, parcaIsmi, malzeme) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            // PBR malzeme ayarlari: { roughness, metalness, color, opacity }
            const malzemeAyarlari = {
                krom:     { roughness: 0.08, metalness: 1.0,  color: '#C8C8D0' },
                ayna:     { roughness: 0.0,  metalness: 1.0,  color: '#F8F8FF' },
                metal:    { roughness: 0.20, metalness: 0.90, color: '#B0B0B8' },
                cam:      { roughness: 0.05, metalness: 0.05, color: '#D4E8F0', opacity: 0.50, transparent: true },
                ahsap:    { roughness: 0.55, metalness: 0.0,  color: '#8B5A2B' },
                plastik:  { roughness: 0.40, metalness: 0.0,  color: '#F0F0F0' },
                porselen: { roughness: 0.15, metalness: 0.0,  color: '#FFFFF5' },
            };

            const ayar = malzemeAyarlari[malzeme] || malzemeAyarlari.plastik;

            veri.model.traverse(function (nesne) {
                if (nesne.name === parcaIsmi && nesne.isMesh && nesne.material) {
                    const uygula = function(mat) {
                        mat.roughness = ayar.roughness;
                        mat.metalness = ayar.metalness;
                        if (ayar.color && mat.color) {
                            mat.color.set(new THREE.Color(ayar.color).convertSRGBToLinear());
                        }
                        if (ayar.opacity !== undefined) {
                            mat.opacity = ayar.opacity;
                            mat.transparent = ayar.transparent;
                        }
                        mat.needsUpdate = true;
                    };

                    if (Array.isArray(nesne.material)) {
                        nesne.material.forEach(uygula);
                    } else {
                        uygula(nesne.material);
                    }
                }
            });
        },

        /**
         * Sahne ışık şiddetini ayarlar.
         * @param {number} seviye — 0.0 (karanlık) ile 2.0 (çok parlak) arası
         */
        isik_ayar: function (kanvasId, seviye) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.sahne) return;

            veri.sahne.traverse(function (nesne) {
                if (nesne.isLight) {
                    nesne.intensity = nesne.userData.orijinalSiddet
                        ? nesne.userData.orijinalSiddet * seviye
                        : nesne.intensity;
                }
            });
        },

        /**
         * Admin sahne panelinden kaydedilen ayarlari acik viewer'a canli uygular.
         */
        sahne_ayar_uygula: function (kanvasId, ayarTipi, ayarJson) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !ayarJson) return;

            let ayar = null;
            try {
                ayar = JSON.parse(ayarJson);
            } catch (e) {
                console.warn('[UcBoyutMotoru] Sahne ayari JSON okunamadi:', e);
                return;
            }

            if (ayarTipi === 'kamera') {
                const x = Number(ayar.baslangicAciX ?? ayar.BaslangicAciX ?? 0);
                const y = Number(ayar.baslangicAciY ?? ayar.BaslangicAciY ?? 0.5);
                const z = Number(ayar.baslangicAciZ ?? ayar.BaslangicAciZ ?? 4);
                veri.kamera.position.set(x, y, z);
                veri.kamera.lookAt(0, Number(ayar.hedefYukseklik ?? ayar.HedefYukseklik ?? 0), 0);

                if (veri.kontroller) {
                    veri.kontroller.minDistance = Number(ayar.zoomMin ?? ayar.ZoomMin ?? veri.kontroller.minDistance);
                    veri.kontroller.maxDistance = Number(ayar.zoomMax ?? ayar.ZoomMax ?? veri.kontroller.maxDistance);
                    veri.kontroller.autoRotate = Boolean(ayar.otomatikDonme ?? ayar.OtomatikDonme ?? veri.kontroller.autoRotate);
                    veri.kontroller.autoRotateSpeed = Number(ayar.donmeHizi ?? ayar.DonmeHizi ?? veri.kontroller.autoRotateSpeed);
                    veri.kontroller.update();
                }
            }

            if (ayarTipi === 'isik') {
                const siddet = Number(ayar.siddet ?? ayar.Siddet ?? 1);
                this.isik_kaydet(kanvasId);
                this.isik_ayar(kanvasId, siddet);
                veri.renderer.toneMappingExposure = Number(ayar.pozlama ?? ayar.Pozlama ?? veri.renderer.toneMappingExposure);
            }

            if (ayarTipi === 'cevre') {
                const arkaPlan = ayar.arkaPlanRengi ?? ayar.ArkaPlanRengi;
                if (arkaPlan) {
                    veri.sahne.background = new THREE.Color(arkaPlan);
                }

                const hdrYolu = ayar.hdrYolu ?? ayar.HdrYolu;
                if (hdrYolu && typeof THREE.RGBELoader !== 'undefined') {
                    new THREE.RGBELoader().load(hdrYolu, function (doku) {
                        const pmrem = new THREE.PMREMGenerator(veri.renderer);
                        veri.sahne.environment = pmrem.fromEquirectangular(doku).texture;
                        doku.dispose();
                    });
                }
            }
        },

        /**
         * Işık orijinal şiddetini kaydeder (ilk isik_ayar çağrısı için).
         */
        isik_kaydet: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.sahne) return;

            veri.sahne.traverse(function (nesne) {
                if (nesne.isLight && nesne.userData.orijinalSiddet === undefined) {
                    nesne.userData.orijinalSiddet = nesne.intensity;
                }
            });
        },

        /**
         * Parça seçildiğinde .NET tarafına bildirim için callback kaydeder.
         */
        parca_sec_callback_kaydet: function (kanvasId, dotNetRef) {
            const veri = _sahneGetir(kanvasId);
            if (veri) {
                veri.parcaSecildiCallback = dotNetRef;
            }
        },

        /**
         * Sahnede yuklu olan 3D modeli degistirir.
         * Eski model temizlenir, yeni GLB/GLTF dosyasi yuklenir.
         */
        model_degistir: function (kanvasId, modelYolu) {
            const veri = _sahneGetir(kanvasId);
            if (!veri) return;
            if (veri.model) {
                veri.sahne.remove(veri.model);
            }
            this.modeli_yukle(kanvasId, modelYolu);
        },

        /**
         * Kullanicinin gordugu sahnenin ekran goruntusunu PNG olarak alir.
         * Base64 formatinda geri doner.
         */
        ekran_goruntusu_al: function (kanvasId) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.renderer) return null;
            return veri.renderer.domElement.toDataURL('image/png');
        },

        /**
         * 3D modeli olceklendirir. Genislik ve yukseklik mm cinsinden.
         * Mevcut model bounding box'ina gore olcek hesaplanir.
         */
        olcu_uygula: function (kanvasId, genislikMm, yukseklikMm) {
            const veri = _sahneGetir(kanvasId);
            if (!veri || !veri.model) return;

            const kutu = new THREE.Box3().setFromObject(veri.model);
            const boyut = kutu.getSize(new THREE.Vector3());
            const maxBoyut = Math.max(boyut.x, boyut.y, boyut.z);
            const hedefMax = Math.max(genislikMm / 1000, yukseklikMm / 1000);
            if (maxBoyut > 0) {
                const olcek = hedefMax / maxBoyut;
                veri.model.scale.setScalar(olcek);
            }
        }
    };
})();

// === DRACO Loader Desteği ===
// DRACO sıkıştırılmış GLB/GLTF modelleri için
window.initDracoLoader = function(dracoPath) {
    if (!window.THREE) return;
    
    try {
        const dracoLoader = new THREE.DRACOLoader();
        dracoLoader.setDecoderPath(dracoPath || '/js/draco/');
        dracoLoader.setDecoderConfig({ type: 'js' });
        
        const gltfLoader = new THREE.GLTFLoader();
        gltfLoader.setDRACOLoader(dracoLoader);
        
        window._dracoGltfLoader = gltfLoader;
        console.log('DRACO loader başlatıldı');
    } catch (e) {
        console.warn('DRACO loader başlatılamadı:', e);
    }
};

// DRACO destekli model yükleme
window.loadDracoModel = function(url, canvasId) {
    return new Promise((resolve, reject) => {
        if (!window._dracoGltfLoader) {
            reject('DRACO loader başlatılmamış');
            return;
        }
        window._dracoGltfLoader.load(
            url,
            (gltf) => resolve({ scene: gltf.scene, animations: gltf.animations }),
            (progress) => {
                if (progress.total > 0) {
                    const pct = Math.round((progress.loaded / progress.total) * 100);
                    window._modelProgress = pct;
                }
            },
            (error) => reject(error)
        );
    });
};

// === HDR Çevre Haritası Desteği ===
window.setEnvironmentMap = function(hdrUrl) {
    if (!window._renderer || !window._scene) return;
    
    try {
        const pmremGenerator = new THREE.PMREMGenerator(window._renderer);
        pmremGenerator.compileEquirectangularShader();
        
        new THREE.RGBELoader()
            .setDataType(THREE.HalfFloatType)
            .load(hdrUrl, function(texture) {
                const envMap = pmremGenerator.fromEquirectangular(texture).texture;
                window._scene.environment = envMap;
                window._scene.background = envMap;
                window._scene.backgroundIntensity = 0.4;
                
                // Tüm mesh'lere yansıma uygula
                window._scene.traverse(function(child) {
                    if (child.isMesh && child.material.isMeshStandardMaterial) {
                        child.material.envMapIntensity = 0.6;
                        child.material.needsUpdate = true;
                    }
                });
                
                texture.dispose();
            });
    } catch (e) {
        console.warn('HDR yüklenemedi:', e);
    }
};

// === Hotspot (Tıklanabilir Nokta) Sistemi ===
window._hotspots = [];

window.addHotspot = function(position, meshName, label) {
    if (!window._scene || !window._camera) return;
    
    // Küçük küre işaretçi
    const geometry = new THREE.SphereGeometry(0.05, 16, 16);
    const material = new THREE.MeshBasicMaterial({ 
        color: 0xc8952a,  // altın rengi
        transparent: true,
        opacity: 0.8
    });
    const marker = new THREE.Mesh(geometry, material);
    marker.position.copy(position);
    marker.userData = { meshName, label, isHotspot: true };
    window._scene.add(marker);
    window._hotspots.push(marker);
    
    return marker;
};

window.clearHotspots = function() {
    window._hotspots.forEach(function(h) {
        h.geometry.dispose();
        h.material.dispose();
        window._scene.remove(h);
    });
    window._hotspots = [];
};

window.checkHotspotClick = function(mouseX, mouseY) {
    if (!window._raycaster || !window._camera) return null;
    
    window._raycaster.setFromCamera(
        new THREE.Vector2(mouseX, mouseY), 
        window._camera
    );
    
    const intersects = window._raycaster.intersectObjects(window._hotspots);
    if (intersects.length > 0) {
        return intersects[0].object.userData;
    }
    return null;
};
