using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VizitLink3D.Konfigurator.Api;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Testler;

/// <summary>
/// P05-A: 3D model yayin yonetimi entegrasyon testleri.
/// BFF guvenlik, admin listesi, AktifMi toggle, soft-delete davranisi.
/// En az 8 test.
/// </summary>
public class ModellerYonetimTestleri : IDisposable
{
    private readonly WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program> _fabrika;
    private readonly HttpClient _bffIstemci;
    private readonly HttpClient _genelIstemci;
    private readonly string _testDbYolu;

    public ModellerYonetimTestleri()
    {
        _testDbYolu = Path.Combine(Path.GetTempPath(), $"konfigurator_test_{Guid.NewGuid():N}.db");

        _fabrika = new WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:KonfiguratorVeriTabani", $"Data Source={_testDbYolu}");
                builder.UseSetting("BffGuvenlik:Anahtar", "test-gizli-anahtar");
                builder.UseSetting("IlkYonetici:KullaniciAdi", "");
                builder.UseSetting("IlkYonetici:Sifre", "");

                builder.ConfigureServices(servisler =>
                {
                    // Varolan DbContext kaydini kaldir, yerine test SQLite ekle
                    servisler.RemoveAll<DbContextOptions<KonfiguratorDbContext>>();
                    servisler.RemoveAll<KonfiguratorDbContext>();

                    servisler.AddDbContext<KonfiguratorDbContext>(secenekler =>
                        secenekler.UseSqlite($"Data Source={_testDbYolu}"));
                });
            });

        // Test DB semasini olustur
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

        // SQLite baglantilarinin kapanmasi icin kisa bekleme
        GC.Collect();
        GC.WaitForPendingFinalizers();

        if (File.Exists(_testDbYolu))
        {
            try { File.Delete(_testDbYolu); } catch { }
        }
    }

    // ─── Yardimci metodlar ───

    private KonfiguratorDbContext DbContextOlustur()
    {
        var kapsam = _fabrika.Services.CreateScope();
        return kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
    }

    private async Task<UcBoyutModel> ModelEkleAsync(string ad, string slug, bool aktifMi = true, bool silindiMi = false)
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
            SilindiMi = silindiMi,
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
    // TEST 1: BFF anahtari YOK → 401 Unauthorized
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YonetimListele_BffAnahtariYok_401Doner()
    {
        // Header'siz HttpClient
        using var hamIstemci = _fabrika.CreateClient();

        var yanit = await hamIstemci.GetAsync("/api/yonetim/modeller");

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 2: BFF anahtari YANLIS → 401 Unauthorized
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YonetimListele_BffAnahtariYanlis_401Doner()
    {
        using var yanlisIstemci = _fabrika.CreateClient();
        yanlisIstemci.DefaultRequestHeaders.Add("X-Konfigurator-Bff-Anahtari", "yanlis-anahtar");

        var yanit = await yanlisIstemci.GetAsync("/api/yonetim/modeller");

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 3: Admin listesi — pasif modelleri de icerir, hassas alan yok
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YonetimListele_AktifVePasifModelleriDondurur_HassasAlanYok()
    {
        // Seed: 1 aktif, 1 pasif, 1 silinmis
        await ModelEkleAsync("Aktif Model", "aktif-model", aktifMi: true);
        await ModelEkleAsync("Pasif Model", "pasif-model", aktifMi: false);
        await ModelEkleAsync("Silinmis Model", "silinmis-model", aktifMi: true, silindiMi: true);

        var yanit = await _bffIstemci.GetAsync("/api/yonetim/modeller");
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);

        // BasariliMi kontrolu
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());

        var veri = cevap.GetProperty("veri");
        Assert.Equal(2, veri.GetArrayLength()); // silinmis haric 2 model

        // Ilk modelin alanlarini kontrol et
        var ilkModel = veri[0];
        Assert.True(ilkModel.TryGetProperty("id", out _));
        Assert.True(ilkModel.TryGetProperty("ad", out _));
        Assert.True(ilkModel.TryGetProperty("slug", out _));
        Assert.True(ilkModel.TryGetProperty("boyutBayt", out _));
        Assert.True(ilkModel.TryGetProperty("aktifMi", out _));
        Assert.True(ilkModel.TryGetProperty("olusturulmaTarihi", out _));

        // Hassas alanlar BULUNMAMALI
        Assert.False(ilkModel.TryGetProperty("dosyaYolu", out _));
        Assert.False(ilkModel.TryGetProperty("sha256Hash", out _));
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 4: AktifMi=true yap → public listede gorunur
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YayinDurumu_AktifYap_PublicListedeGorunur()
    {
        var model = await ModelEkleAsync("Test Model", "test-model", aktifMi: false);

        // BFF ile aktif yap
        var govde = new StringContent("""{"aktifMi":true}""", Encoding.UTF8, "application/json");
        var yanit = await _bffIstemci.PutAsync($"/api/yonetim/modeller/{model.Id}/yayin-durumu", govde);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.True(cevap.GetProperty("veri").GetProperty("aktifMi").GetBoolean());

        // Public endpoint'te gorunmeli
        var genelYanit = await _genelIstemci.GetAsync("/api/modeller");
        genelYanit.EnsureSuccessStatusCode();
        var genelCevap = await CevapOlarakOkuAsync(genelYanit);

        var genelVeri = genelCevap.GetProperty("veri");
        Assert.True(genelVeri.GetArrayLength() >= 1);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 5: AktifMi=false yap → public listede gorunmez
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YayinDurumu_PasifYap_PublicListedeGorunmez()
    {
        var model = await ModelEkleAsync("Gizli Model", "gizli-model", aktifMi: true);

        // Public'te gorundugunu teyit et
        var onceYanit = await _genelIstemci.GetAsync("/api/modeller");
        onceYanit.EnsureSuccessStatusCode();
        var onceCevap = await CevapOlarakOkuAsync(onceYanit);
        Assert.True(onceCevap.GetProperty("veri").GetArrayLength() >= 1);

        // BFF ile pasif yap
        var govde = new StringContent("""{"aktifMi":false}""", Encoding.UTF8, "application/json");
        var yanit = await _bffIstemci.PutAsync($"/api/yonetim/modeller/{model.Id}/yayin-durumu", govde);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.False(cevap.GetProperty("veri").GetProperty("aktifMi").GetBoolean());

        // Public endpoint'te gorunmemeli
        var genelYanit = await _genelIstemci.GetAsync("/api/modeller");
        genelYanit.EnsureSuccessStatusCode();
        var genelCevap = await CevapOlarakOkuAsync(genelYanit);

        var genelVeri = genelCevap.GetProperty("veri");
        // Pasif yapilan model listede olmamali
        foreach (var m in genelVeri.EnumerateArray())
        {
            if (m.TryGetProperty("id", out var idElem))
                Assert.NotEqual(model.Id, idElem.GetInt32());
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 6: Gecersiz ID (0 veya negatif) → hata doner
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YayinDurumu_GecersizId_HataDoner()
    {
        var govde = new StringContent("""{"aktifMi":true}""", Encoding.UTF8, "application/json");
        var yanit = await _bffIstemci.PutAsync("/api/yonetim/modeller/0/yayin-durumu", govde);

        // 200 OK donmeli (KonfiguratorCevap her zaman 200 doner)
        yanit.EnsureSuccessStatusCode();
        var cevap = await CevapOlarakOkuAsync(yanit);

        Assert.False(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.Contains("Geçersiz model kimliği", cevap.GetProperty("mesaj").GetString());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 7: Bulunamayan ID → "Model bulunamadı." hatasi
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YayinDurumu_BulunamayanId_HataDoner()
    {
        var govde = new StringContent("""{"aktifMi":true}""", Encoding.UTF8, "application/json");
        var yanit = await _bffIstemci.PutAsync("/api/yonetim/modeller/99999/yayin-durumu", govde);

        yanit.EnsureSuccessStatusCode();
        var cevap = await CevapOlarakOkuAsync(yanit);

        Assert.False(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.Contains("Model bulunamadı", cevap.GetProperty("mesaj").GetString());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 8: Soft-delete edilmis model yayin durumu degistirilemez
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YayinDurumu_SoftDeleteEdilmisModel_Bulunamaz()
    {
        var model = await ModelEkleAsync("Silinecek", "silinecek", aktifMi: true, silindiMi: true);

        var govde = new StringContent("""{"aktifMi":false}""", Encoding.UTF8, "application/json");
        var yanit = await _bffIstemci.PutAsync($"/api/yonetim/modeller/{model.Id}/yayin-durumu", govde);

        yanit.EnsureSuccessStatusCode();
        var cevap = await CevapOlarakOkuAsync(yanit);

        Assert.False(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.Contains("Model bulunamadı", cevap.GetProperty("mesaj").GetString());
    }
}
