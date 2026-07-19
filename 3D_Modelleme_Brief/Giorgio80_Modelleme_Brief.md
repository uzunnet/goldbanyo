# Giorgio 80 — 3D Modelleme Brief (Konfigüre Edilebilir)

**Ürün:** Giorgio 80 | **Kod:** GIORGIO-80 | **Koleksiyon:** Exclusive | **Katalog s.14**
**Hedef:** FUGA modelindeki gibi parça parça, kodla renk/malzeme değiştirilebilir 3D model.

---

## 1. Ölçüler (kaynak: teknik ölçü sayfası + katalog verisi)

| Parça | Yükseklik (H) | Genişlik (W) | Derinlik (D) |
|---|---|---|---|
| Dolap (lavabo dolabı) | 85 cm | 80 cm | 50 cm |
| Ayna | 90 cm | 70 cm | 5 cm |
| Boy Dolap | 140 cm | 38 cm | 33 cm |

Renk seçenekleri (katalog): Gri, Siyah, Ahşap
Özellikler: Soft kapak, MDF gövde, dokunmatik ledli ayna, stone lavabo

---

## 2. ÖNEMLİ — Doku/renk BAKE ETME

Sistem, materyalleri modelin üzerine **çalışma anında kod ile** uyguluyor (Three.js `MeshPhysicalMaterial`, düz PBR renk — hiçbir texture/renk haritası kullanılmıyor). Yani:

- Modele **renk, doku, texture bake ETMEYİN** — geometri düz/nötr gri gelsin yeterli.
- UV unwrap **gerekli değil** (materyaller solid renk, texture map yok).
- Sanatçının tek işi: **temiz geometri + doğru mesh isimlendirmesi.** Renk/malzeme ataması tamamen koddan geliyor.

Bu, işi klasik "photoreal render" modellemesine göre ciddi ölçüde hafifletiyor — sadece doğru oranlarda, doğru parçalara bölünmüş, temiz topolojili mesh yeterli.

---

## 3. Mesh İsimlendirme Kuralı (ZORUNLU — kod bu isimlere göre parçaları tanıyor)

Her parça **ayrı bir mesh/obje** olmalı ve adı aşağıdaki listeden birini içermeli (büyük/küçük harf ve Türkçe karakter farketmez, boşluk/alt çizgi ikisi de kabul edilir):

| Mesh Adı (örnek) | Karşılık geldiği parça | Not |
|---|---|---|
| `govde` / `gövde` | Dolap gövdesi (iskelet) | Kapaklar hariç tüm gövde |
| `ayna` / `mirror` | Ayna camı | Düz panel, sistem otomatik ayna yansıması ekliyor |
| `kapak_1`, `kapak_2`, `kapak_3`, `kapak_4` | Dolap kapakları (1-4 adet) | Her kapak **ayrı mesh**, aynı anda aynı renk/malzemeyi alır |
| `kulp` | Kapak kulpları | Tüm kulplar aynı malzeme/renk (varsayılan krom) |
| `led` | LED ışık şeridi (ayna çevresi) | Emissive materyal koddan geliyor, açık/kapalı state var |
| `musluk` / `faucet` | Musluk | Varsayılan krom |
| `lavabo` / `sink` / `basin` | Lavabo teknesi | "Stone Lavabo" — porselen/taş görünüm koddan atanıyor |
| `ust_tabla` / `üst tabla` | Üst tabla (tezgah üstü) | Gövde malzemesiyle uyumlu renk |
| `ic_ust_tabla` / `iç üst tabla` | İç üst raf | — |
| `ic_alt_tabla` / `iç alt tabla` | İç alt tabla / tezgah | Kendi doğal taş rengini korur (mermer vb.) |
| `montaj` | Montaj aparatları (varsa) | Sabit, renklendirilmez — dekoratif/fonksiyonel parça |

**Boy Dolap** (140x38x33) ayrı bir ürün parçası — kendi `govde` + `kapak_N` + `kulp` mesh'lerini aynı kurala göre içermeli (ayrı bir GLB dosyası olarak teslim edilebilir).

---

## 4. Kapak Menteşe/Pivot Notu

Kapak açılma pivotu **kod tarafından otomatik hesaplanıyor** (her kapak mesh'inin kendi bounding-box kenarından, sol/sağ yönüne göre). Sanatçının elle pivot ayarlamasına gerek yok — sadece kapak geometrisinin menteşe tarafı kenarının **gerçek/net kenar** olmasına dikkat edin (fazladan boşluk/taşma bırakmayın), yoksa açılma noktası kaymış görünür.

---

## 5. Teslimat Formatı

- **Format:** GLB (glTF binary) — tercih edilen, mevcut viewer (Three.js `GLTFLoader`) ile doğrudan uyumlu.
- **Ölçek:** Gerçek dünya ölçeği, 1 birim = 1 metre (yukarıdaki cm ölçülerini ÷100 olarak modelleyin).
- **Poligon sayısı:** Web'de gerçek zamanlı döndürülebilir konfigüratör için optimize (gereksiz alt bölme yok, düşük-orta poly).
- **Materyal:** Boş/nötr gri materyal yeterli, texture/renk atamasına gerek yok (bkz. madde 2).
- **Dosya adı önerisi:** `giorgio-80.glb` (ana set), `giorgio-80-boy-dolap.glb` (ayrı boy dolap, isteğe bağlı ayrı model ise).

---

## 6. Referans

Mevcut çalışan örnek: `ornekdolap/` (FUGA modeli) — aynı mesh isimlendirme ve materyal mantığını kullanıyor, karşılaştırma için incelenebilir.
