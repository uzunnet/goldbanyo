using System.Reflection;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Testler;

/// <summary>
/// P06-B: UcBoyutGoruntuleyici mesh traversal + MeshleriGetirAsync testleri.
///
/// Kapsam: IUcBoyutGoruntuleyiciServisi arayuzu, servis implementasyonu,
/// JS tarafi mesh isimlendirme ve highlight guvenligi.
///
/// GoldBanyo referansi, localStorage, dogrudan tarayici API'si KULLANILMAZ.
/// </summary>
public class GoruntuleyiciMeshTestleri
{
    // ═══════════════════════════════════════════════════════════════
    // YARDIMCI: JS dosya yolunu cozumler
    // ═══════════════════════════════════════════════════════════════
    private static string JsDosyaYoluCozumle()
    {
        // Test assembly'sinin calistigi dizinden basla (genelde test projesi bin/Debug/net10.0)
        var tabanKlasor = AppContext.BaseDirectory;

        // Cozum kokunu bulana kadar yukari cik
        // Cozum koku: VizitLink3D.Konfigurator.Testler ve VizitLink3D.Konfigurator
        // klasorlerinin ikisini de iceren dizin
        var aday = tabanKlasor;
        for (int i = 0; i < 6; i++)
        {
            var konfiguratorJsYolu = Path.Combine(aday, "VizitLink3D.Konfigurator",
                "wwwroot", "js", "ucboyut", "ucboyut-goruntuleyici.js");
            if (File.Exists(konfiguratorJsYolu))
                return konfiguratorJsYolu;

            aday = Path.GetFullPath(Path.Combine(aday, ".."));
        }

        // Bulunamadi — test basarisiz olacak, anlamli mesaj ver
        return Path.Combine(tabanKlasor, "wwwroot", "js", "ucboyut", "ucboyut-goruntuleyici.js");
    }

    private static string JsDosyasiOku()
    {
        var yol = JsDosyaYoluCozumle();
        Assert.True(File.Exists(yol), $"JS dosyasi bulunamadi: {yol}");
        return File.ReadAllText(yol);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 1: Arayuzde MeshleriGetirAsync metodu mevcut
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Arayuz_MeshleriGetirAsync_MetoduMevcut()
    {
        var metot = typeof(IUcBoyutGoruntuleyiciServisi).GetMethod("MeshleriGetirAsync");

        Assert.NotNull(metot);
        Assert.Equal(typeof(Task<string[]>), metot!.ReturnType);
        Assert.Empty(metot.GetParameters());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 2: Servis MeshleriGetirAsync'i implemente eder
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Servis_MeshleriGetirAsync_ImplementeEder()
    {
        var metot = typeof(UcBoyutGoruntuleyiciServisi).GetMethod("MeshleriGetirAsync");

        Assert.NotNull(metot);
        Assert.Equal(typeof(Task<string[]>), metot!.ReturnType);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 3: Arayuz IJSRuntime'i aciga cikarmaz
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Arayuz_IJSRuntimeAcigaCikarmaz()
    {
        var arayuz = typeof(IUcBoyutGoruntuleyiciServisi);

        foreach (var metot in arayuz.GetMethods())
        {
            // Parametrelerde IJSRuntime olmamali
            foreach (var param in metot.GetParameters())
            {
                Assert.NotEqual(typeof(Microsoft.JSInterop.IJSRuntime), param.ParameterType);
                Assert.NotEqual(typeof(Microsoft.JSInterop.IJSObjectReference), param.ParameterType);
                Assert.NotEqual(typeof(Microsoft.JSInterop.DotNetObjectReference), param.ParameterType);
            }

            // Donus tipinde IJSRuntime olmamali
            Assert.NotEqual(typeof(Microsoft.JSInterop.IJSRuntime), metot.ReturnType);
            Assert.NotEqual(typeof(Microsoft.JSInterop.IJSObjectReference), metot.ReturnType);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 4: Servis sinifinda GoldBanyo referansi yok
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Servis_GoldBanyoReferansiYok()
    {
        var tip = typeof(UcBoyutGoruntuleyiciServisi);

        // Namespace GoldBanyo icermemeli
        Assert.DoesNotContain("GoldBanyo", tip.Namespace);
        Assert.DoesNotContain("GoldBanyo", tip.FullName);

        // Tum metod isimleri GoldBanyo icermemeli
        foreach (var metot in tip.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            Assert.DoesNotContain("GoldBanyo", metot.Name);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 5: Arayuzde GoldBanyo referansi yok
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Arayuz_GoldBanyoReferansiYok()
    {
        var arayuz = typeof(IUcBoyutGoruntuleyiciServisi);

        Assert.DoesNotContain("GoldBanyo", arayuz.Namespace);
        Assert.DoesNotContain("GoldBanyo", arayuz.FullName);

        foreach (var metot in arayuz.GetMethods())
        {
            Assert.DoesNotContain("GoldBanyo", metot.Name);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 6: JS dosyasinda meshIsimleriGetir fonksiyonu mevcut
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void JsDosyasi_MeshIsimleriGetir_FonksiyonuMevcut()
    {
        var js = JsDosyasiOku();

        // Sinif icinde metot tanimi
        Assert.Contains("meshIsimleriGetir()", js);
        // Bridge fonksiyonu
        Assert.Contains("function meshIsimleriGetir()", js);
        // window export
        Assert.Contains("window.meshIsimleriGetir", js);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 7: JS dosyasinda meshSec ve meshSecimiTemizle mevcut
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void JsDosyasi_MeshSecVeTemizle_FonksiyonlariMevcut()
    {
        var js = JsDosyasiOku();

        // Sinif metotlari
        Assert.Contains("meshSec(meshAdi)", js);
        Assert.Contains("meshSecimiTemizle()", js);

        // Bridge fonksiyonlari
        Assert.Contains("function meshSec(", js);
        Assert.Contains("function meshSecimiTemizle()", js);

        // window export
        Assert.Contains("window.meshSec", js);
        Assert.Contains("window.meshSecimiTemizle", js);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 8: JS mesh isimlendirme deterministik (sayac bazli, rastgele yok)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void JsDosyasi_MeshIsimlendirme_Deterministik()
    {
        var js = JsDosyasiOku();

        // Deterministik olmayan API'ler KULLANILMAMALI
        Assert.DoesNotContain("Math.random()", js.Split("meshIsimleriGetir")[1]);
        Assert.DoesNotContain("Date.now()", js.Split("meshIsimleriGetir")[1]);
        Assert.DoesNotContain("crypto.randomUUID", js);

        // Sayac tabanli isimlendirme kullanilmali
        Assert.Contains("sayac", js);
        Assert.Contains("mesh_", js);

        // Isimsiz mesh'ler icin traversal sirasi kullaniliyor
        Assert.Contains("dugum.isMesh", js);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 9: JS dosyasinda localStorage referansi yok
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void JsDosyasi_LocalStorageKullanmaz()
    {
        var js = JsDosyasiOku();

        Assert.DoesNotContain("localStorage", js);
        Assert.DoesNotContain("sessionStorage", js);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 10: JS dosyasinda dogrudan tarayici API'si (fetch, XMLHttpRequest) yok
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void JsDosyasi_DogrudanTarayiciApiYok()
    {
        var js = JsDosyasiOku();

        // GLTFLoader zaten model yuklemede kullaniliyor — bu test
        // yeni eklenen mesh fonksiyonlarinda dogrudan fetch/XMLHttpRequest
        // kullanilmadigini kontrol eder.
        var meshKismi = js.Split("meshIsimleriGetir")[1].Split("yenidenBoyutlandir")[0];

        Assert.DoesNotContain("fetch(", meshKismi);
        Assert.DoesNotContain("XMLHttpRequest", meshKismi);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 11: JS highlight materyal geri yukleme — orijinal durum saklanir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void JsDosyasi_MeshSec_MateryalGeriYukleme()
    {
        var js = JsDosyasiOku();

        // Orijinal emissive durumu saklanmali
        Assert.Contains("orijinalEmissiveHex", js);
        Assert.Contains("orijinalEmissiveIntensity", js);

        // Geri yukleme yapilmali
        Assert.Contains("emissive.setHex", js);
        Assert.Contains("emissiveIntensity", js);

        // GC yardim: mesh referanslari null'lanmali
        Assert.Contains("kayit.mesh = null", js);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 12: JS highlight base color DEGISTIRMEZ
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void JsDosyasi_MeshSec_BaseColorDegistirmez()
    {
        var js = JsDosyasiOku();

        // Sinif metodu "meshSec(meshAdi) {" kalibini bul (kopru fonksiyonu degil)
        var sinifMeshSecBaslangic = js.IndexOf("meshSec(meshAdi) {", StringComparison.Ordinal);
        Assert.True(sinifMeshSecBaslangic > 0, "Sinif meshSec metodu baslangici bulunamadi.");

        // Sonraki sinif metodu "meshSecimiTemizle() {" kalibini bul
        var sonrakiMetot = js.IndexOf("meshSecimiTemizle() {", sinifMeshSecBaslangic, StringComparison.Ordinal);
        Assert.True(sonrakiMetot > sinifMeshSecBaslangic);

        var secKismi = js.Substring(sinifMeshSecBaslangic, sonrakiMetot - sinifMeshSecBaslangic);

        // Diffuse/color/specular/metallic/roughness DEGISTIRILMEMELI
        Assert.DoesNotContain(".color", secKismi);
        Assert.DoesNotContain("setRGB", secKismi);
        Assert.DoesNotContain("setStyle", secKismi);

        // SADECE emissive degistirilmeli
        Assert.Contains("emissive", secKismi);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 13: JS dosyasinda dosya yolu/model log'lamasi yok
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void JsDosyasi_DosyaYoluVeyaModelLoglamasiYok()
    {
        var js = JsDosyasiOku();

        // meshIsimleriGetir ici — dosya yolu log'lanmamali
        var meshKismi = js.Split("meshIsimleriGetir()")[1].Split("meshSec(meshAdi)")[0];

        Assert.DoesNotContain("console.log", meshKismi);
        Assert.DoesNotContain("gltf", meshKismi);
        Assert.DoesNotContain("glb", meshKismi.ToLowerInvariant());
        Assert.DoesNotContain(".glb", meshKismi.ToLowerInvariant());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 14: IUcBoyutGoruntuleyiciServisi IAsyncDisposable'dan turer
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Arayuz_IAsyncDisposable_Turetir()
    {
        var arayuz = typeof(IUcBoyutGoruntuleyiciServisi);

        Assert.True(typeof(IAsyncDisposable).IsAssignableFrom(arayuz),
            "IUcBoyutGoruntuleyiciServisi IAsyncDisposable'dan turemeli.");
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 15: Servis constructor sadece IJSRuntime alir (DI uyumlu)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Servis_Constructor_SadeceIJSRuntimeAlir()
    {
        var yapicilar = typeof(UcBoyutGoruntuleyiciServisi).GetConstructors();

        Assert.Single(yapicilar);

        var parametreler = yapicilar[0].GetParameters();
        Assert.Single(parametreler);
        Assert.Equal(typeof(Microsoft.JSInterop.IJSRuntime), parametreler[0].ParameterType);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 16: JS _seciliMeshler yokEt() icinde temizlenir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void JsDosyasi_YokEt_SeciliMeshleriTemizler()
    {
        var js = JsDosyasiOku();

        // yokEt() metodu icinde _seciliMeshler temizligi
        var yokEtKismi = js.Split("yokEt()")[1].Split("yenidenBoyutlandir")[0];

        Assert.Contains("_seciliMeshler", yokEtKismi);
    }
}
