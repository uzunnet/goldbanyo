using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VizitLink3D.Konfigurator.Api;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Testler;

/// <summary>
/// P06-A: UcBoyutModelParcasi entegrasyon testleri.
/// En az 8 test.
/// </summary>
public class ModelParcalariTestleri : IDisposable
{
    private readonly WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program> _fabrika;
    private readonly HttpClient _bffIstemci;
    private readonly HttpClient _genelIstemci;
    private readonly string _testDbYolu;

    public ModelParcalariTestleri()
    {
        _testDbYolu = Path.Combine(Path.GetTempPath(), $"konfigurator_parca_test_{Guid.NewGuid():N}.db");

        _fabrika = new WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:KonfiguratorVeriTabani", $"Data Source={_testDbYolu}");
                builder.UseSetting("BffGuvenlik:Anahtar", "test-gizli-anahtar");
                builder.UseSetting("IlkYonetici:KullaniciAdi", "");
                builder.UseSetting("IlkYonetici:Sifre", "");

                builder.ConfigureServices(servisler =>
                {
                    servisler.RemoveAll<DbContextOptions<KonfiguratorDbContext>>();
                    servisler.RemoveAll<KonfiguratorDbContext>();

                    servisler.AddDbContext<KonfiguratorDbContext>(secenekler =>
                        secenekler.UseSqlite($"Data Source={_testDbYolu}"));
                });
            });

        using var kapsam = _fabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
        db.Database.EnsureCreated();

        _bffIstemci = _fabrika.CreateClient();
        _bffIstemci.DefaultRequestHeaders.Add("X-Konfigurator-Bff-Anahtari", "test-gizli-anahtar");

        _genelIstemci = _fabrika.CreateClient();
    }

    public void Dispose()
    {
        _bffIstemci.Dispose();
        _genelIstemci.Dispose();
        _fabrika.Dispose();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        if (File.Exists(_testDbYolu))
        {
            try { File.Delete(_testDbYolu); } catch { }
        }
    }

    // ─── Yardımcı metodlar ───

    private async Task<UcBoyutModel> ModelEkleAsync(string ad, string slug, bool aktifMi = true)
    {
        using var kapsam = _fabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

        var model = new UcBoyutModel
        {
            Ad = ad,
            Slug = slug,
            Aciklama = $"{ad} açıklaması",
            DosyaAdi = $"{slug}.glb",
            DosyaYolu = $"/medya/3d-modeller/{slug}.glb",
            IcerikTuru = "model/gltf-binary",
            BoyutBayt = 1024,
            Sha256Hash = "abc123def456",
            AktifMi = aktifMi,
            SilindiMi = false,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        db.UcBoyutModeller.Add(model);
        await db.SaveChangesAsync();
        return model;
    }

    private static async Task<JsonElement> CevapOlarakOkuAsync(HttpResponseMessage yanit)
    {
        var json = await yanit.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 1: Senkronize — yeni mesh'leri güvenle ekler
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senkronize_YeniMeshlar_GuvenleEklenir()
    {
        var model = await ModelEkleAsync("Test Modeli", "test-modeli");

        var komut = new { meshAdlari = new[] { "Govde_Mesh", "Kapak_Mesh", "Cekmece_Mesh" } };
        var yanit = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.Equal(3, cevap.GetProperty("veri").GetProperty("eklenen").GetInt32());
        Assert.Equal(0, cevap.GetProperty("veri").GetProperty("yumusakSilinen").GetInt32());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 2: Senkronize — tür tahmini YAPILMAZ (hepsi Diger)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senkronize_TurTahminiYapilmaz_HepsiDiger()
    {
        var model = await ModelEkleAsync("Tur Test", "tur-test");

        var komut = new { meshAdlari = new[] { "Govde", "Cekmece_Alt", "LED_Serit" } };
        var yanit = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());

        // GET admin listesinde tüm parçalar Diger olmalı
        var listeYanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{model.Id}/parcalar");
        listeYanit.EnsureSuccessStatusCode();
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        Assert.True(listeCevap.GetProperty("basariliMi").GetBoolean());

        foreach (var parca in listeCevap.GetProperty("veri").EnumerateArray())
        {
            Assert.Equal("Diger", parca.GetProperty("parcaTuru").GetString());
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 3: Senkronize — listeden çıkan mesh soft-delete olur
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senkronize_ListedenCikanMesh_SoftDeleteOlur()
    {
        var model = await ModelEkleAsync("Sil Testi", "sil-testi");

        // İlk senkronizasyon: 3 mesh
        var komut1 = new { meshAdlari = new[] { "A_Mesh", "B_Mesh", "C_Mesh" } };
        var yanit1 = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut1);
        yanit1.EnsureSuccessStatusCode();

        // İkinci senkronizasyon: sadece 2 mesh (B_Mesh çıkarıldı)
        var komut2 = new { meshAdlari = new[] { "A_Mesh", "C_Mesh" } };
        var yanit2 = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut2);
        yanit2.EnsureSuccessStatusCode();

        var cevap2 = await CevapOlarakOkuAsync(yanit2);
        Assert.True(cevap2.GetProperty("basariliMi").GetBoolean());
        Assert.Equal(1, cevap2.GetProperty("veri").GetProperty("yumusakSilinen").GetInt32());
        Assert.Equal(0, cevap2.GetProperty("veri").GetProperty("eklenen").GetInt32());

        // Admin listesinde sadece 2 aktif parça görünmeli
        var listeYanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{model.Id}/parcalar");
        listeYanit.EnsureSuccessStatusCode();
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        Assert.Equal(2, listeCevap.GetProperty("veri").GetArrayLength());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 4: Unique kısıtı — aynı ModelId+MeshAdi tekrar eklenemez
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senkronize_AyniMeshTekrarEklenmez_GeriYuklenir()
    {
        var model = await ModelEkleAsync("Unique Test", "unique-test");

        // İlk senkronizasyon
        var komut1 = new { meshAdlari = new[] { "Tekil_Mesh" } };
        var yanit1 = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut1);
        yanit1.EnsureSuccessStatusCode();

        // Sil — boş listeyle senkronize et
        var komut2 = new { meshAdlari = Array.Empty<string>() };
        var yanit2 = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut2);
        yanit2.EnsureSuccessStatusCode();

        // Tekrar ekle — soft-delete'ten geri yüklenmeli
        var komut3 = new { meshAdlari = new[] { "Tekil_Mesh" } };
        var yanit3 = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut3);
        yanit3.EnsureSuccessStatusCode();

        var cevap3 = await CevapOlarakOkuAsync(yanit3);
        Assert.True(cevap3.GetProperty("basariliMi").GetBoolean());
        Assert.Equal(0, cevap3.GetProperty("veri").GetProperty("eklenen").GetInt32());
        Assert.Equal(1, cevap3.GetProperty("veri").GetProperty("geriYuklenen").GetInt32());

        // Aynı mesh adıyla ikinci kez çağır — yeni ekleme olmamalı
        var yanit4 = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut3);
        yanit4.EnsureSuccessStatusCode();

        var cevap4 = await CevapOlarakOkuAsync(yanit4);
        Assert.True(cevap4.GetProperty("basariliMi").GetBoolean());
        Assert.Equal(0, cevap4.GetProperty("veri").GetProperty("eklenen").GetInt32());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 5: Metadata güncelleme — parça türü ve görünürlük değişir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task MetadataGuncelle_ParcaTuruVeGorunurlukDegisir()
    {
        var model = await ModelEkleAsync("Meta Test", "meta-test");

        // Önce bir mesh senkronize et
        var komut = new { meshAdlari = new[] { "Govde_Parcasi" } };
        var senkroYanit = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut);
        senkroYanit.EnsureSuccessStatusCode();

        // Parça ID'sini admin listesinden al
        var listeYanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{model.Id}/parcalar");
        listeYanit.EnsureSuccessStatusCode();
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        var parcaId = listeCevap.GetProperty("veri")[0].GetProperty("id").GetInt32();

        // Metadata güncelle: türü Govde yap, görünür yap, renk özelleştir
        var guncelleDto = new
        {
            parcaTuru = "Govde",
            gorunenAd = "Ana Gövde",
            renkDegistirilebilirMi = true,
            gorunurMu = true,
            varsayilanRenk = "#C8952A"
        };

        var putYanit = await _bffIstemci.PutAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/{parcaId}", guncelleDto);
        putYanit.EnsureSuccessStatusCode();

        var putCevap = await CevapOlarakOkuAsync(putYanit);
        Assert.True(putCevap.GetProperty("basariliMi").GetBoolean());
        Assert.Equal("Govde", putCevap.GetProperty("veri").GetProperty("parcaTuru").GetString());
        Assert.Equal("Ana Gövde", putCevap.GetProperty("veri").GetProperty("gorunenAd").GetString());
        Assert.True(putCevap.GetProperty("veri").GetProperty("renkDegistirilebilirMi").GetBoolean());
        Assert.Equal("#C8952A", putCevap.GetProperty("veri").GetProperty("varsayilanRenk").GetString());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 6: BFF güvenlik — anahtarsız istek reddedilir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YonetimParcalar_BffAnahtariYok_401Doner()
    {
        using var hamIstemci = _fabrika.CreateClient();

        var komut = new { meshAdlari = new[] { "Test_Mesh" } };
        var yanit = await hamIstemci.PostAsJsonAsync("/api/yonetim/modeller/1/parcalar/senkronize", komut);

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 7: Public detay — sadece görünür parçalar döner
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task PublicDetay_SadeceGorunurParcalarDoner()
    {
        var model = await ModelEkleAsync("Gorunurluk Test", "gorunurluk-test", aktifMi: true);

        // İki mesh senkronize et
        var komut = new { meshAdlari = new[] { "Gorunen_Parca", "Gizli_Parca" } };
        var senkroYanit = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut);
        senkroYanit.EnsureSuccessStatusCode();

        // Admin listesinden parçaları al
        var listeYanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{model.Id}/parcalar");
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        var parcalar = listeCevap.GetProperty("veri").EnumerateArray().ToList();

        // "Gizli_Parca" isimli parçayı bul ve görünmez yap
        var gizliParca = parcalar.First(p => p.GetProperty("meshAdi").GetString() == "Gizli_Parca");
        var gizliId = gizliParca.GetProperty("id").GetInt32();
        var guncelleDto = new { gorunurMu = false };
        var putYanit = await _bffIstemci.PutAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/{gizliId}", guncelleDto);
        putYanit.EnsureSuccessStatusCode();

        // Public detay: sadece 1 parça görünmeli
        var genelYanit = await _genelIstemci.GetAsync($"/api/modeller/{model.Slug}");
        genelYanit.EnsureSuccessStatusCode();
        var genelCevap = await CevapOlarakOkuAsync(genelYanit);

        Assert.True(genelCevap.GetProperty("basariliMi").GetBoolean());
        var genelParcalar = genelCevap.GetProperty("veri").GetProperty("parcalar");
        Assert.Equal(1, genelParcalar.GetArrayLength());
        Assert.Equal("Gorunen_Parca", genelParcalar[0].GetProperty("meshAdi").GetString());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 8: Pasif model detayında veri dönmez
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task PublicDetay_PasifModel_VeriDonmez()
    {
        var model = await ModelEkleAsync("Pasif Model", "pasif-model-detay", aktifMi: false);

        // Mesh senkronize et
        var komut = new { meshAdlari = new[] { "Pasif_Parca" } };
        await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut);

        // Public detay: model pasif olduğu için bulunamamalı
        var genelYanit = await _genelIstemci.GetAsync($"/api/modeller/{model.Slug}");
        genelYanit.EnsureSuccessStatusCode();
        var genelCevap = await CevapOlarakOkuAsync(genelYanit);

        Assert.False(genelCevap.GetProperty("basariliMi").GetBoolean());
        Assert.Contains("Model bulunamadı", genelCevap.GetProperty("mesaj").GetString());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 9: Senkronize — geçersiz modelId hata döner
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senkronize_GecersizModelId_HataDoner()
    {
        var komut = new { meshAdlari = new[] { "Test_Mesh" } };
        var yanit = await _bffIstemci.PostAsJsonAsync(
            "/api/yonetim/modeller/0/parcalar/senkronize", komut);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.False(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.Contains("Geçersiz model kimliği", cevap.GetProperty("mesaj").GetString());
    }
}
