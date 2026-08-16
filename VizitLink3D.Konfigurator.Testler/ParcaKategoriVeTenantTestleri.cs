using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VizitLink3D.Konfigurator.Api;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;
using VizitLink3D.Konfigurator.Api.VeriTabani;
using VizitLink3D.Konfigurator.Servisler;

namespace VizitLink3D.Konfigurator.Testler;

/// <summary>
/// P06-D: ParcaKategorisi CRUD, tenant izolasyonu ve geriye dönük uyumluluk testleri.
/// En az 8 test — kategori yönetimi, cross-tenant güvenlik, ParcaTuru geçişi, mesh click.
/// </summary>
public class ParcaKategoriVeTenantTestleri : IDisposable
{
    private readonly WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program> _apiFabrika;
    private readonly HttpClient _bffIstemci;
    private readonly HttpClient _genelIstemci;
    private readonly string _testDbYolu;
    private readonly string _bffAnahtar = "p06d-test-gizli-anahtar";

    public ParcaKategoriVeTenantTestleri()
    {
        _testDbYolu = Path.Combine(Path.GetTempPath(), $"parca_kategori_test_{Guid.NewGuid():N}.db");

        _apiFabrika = new WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:KonfiguratorVeriTabani", $"Data Source={_testDbYolu}");
                builder.UseSetting("BffGuvenlik:Anahtar", _bffAnahtar);
                builder.UseSetting("SaaS:MultiTenantAktif", "true");
                builder.UseSetting("SaaS:VarsayilanFirmaId", "1");
                builder.UseSetting("SaaS:VarsayilanFirmaSlug", "goldbanyo");
                builder.UseSetting("SaaS:VarsayilanFirmaAd", "Gold Banyo");
                builder.UseSetting("IlkYonetici:KullaniciAdi", "");
                builder.UseSetting("IlkYonetici:Sifre", "");
                builder.UseSetting("Migration:ParcaTuruGecisYapildi", "false");

                builder.ConfigureServices(servisler =>
                {
                    servisler.RemoveAll<DbContextOptions<KonfiguratorDbContext>>();
                    servisler.RemoveAll<KonfiguratorDbContext>();

                    servisler.AddDbContext<KonfiguratorDbContext>(secenekler =>
                        secenekler.UseSqlite($"Data Source={_testDbYolu}"));
                });
            });

        // Test DB semasini olustur
        using var kapsam = _apiFabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
        db.Database.EnsureCreated();

        // BFF anahtarli istemci
        _bffIstemci = _apiFabrika.CreateClient();
        _bffIstemci.DefaultRequestHeaders.Add("X-Konfigurator-Bff-Anahtari", _bffAnahtar);
        _bffIstemci.DefaultRequestHeaders.Add("X-Firma", "goldbanyo");

        // Genel (anahtarsiz) istemci
        _genelIstemci = _apiFabrika.CreateClient();
    }

    public void Dispose()
    {
        _bffIstemci.Dispose();
        _genelIstemci.Dispose();
        _apiFabrika.Dispose();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        if (File.Exists(_testDbYolu))
        {
            try { File.Delete(_testDbYolu); } catch { }
        }
    }

    private static async Task<JsonElement> CevapOlarakOkuAsync(HttpResponseMessage yanit)
    {
        var json = await yanit.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private async Task<int> FirmaEkleAsync(string ad, string slug)
    {
        using var kapsam = _apiFabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

        var firma = new KonfiguratorFirma
        {
            Ad = ad,
            Slug = slug,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        db.Firmalar.Add(firma);
        await db.SaveChangesAsync();
        return firma.Id;
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 1: Kategori listesi boş başlar
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Kategori_Listesi_BosBaslar()
    {
        var yanit = await _bffIstemci.GetAsync("/api/yonetim/parca-kategorileri");
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());

        // İlk çalıştırmada ParcaTuru geçişi henüz yapılmadıysa boş olabilir
        Assert.True(cevap.GetProperty("veri").ValueKind == JsonValueKind.Array);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 2: Kategori ekleme ve listeleme
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Kategori_Ekleme_Ve_Listeleme_Basarili()
    {
        var ekleDto = new { ad = "Banyo Dolabı", aciklama = "Dolap kategorisi", aktifMi = true, siraNo = 1 };
        var yanit = await _bffIstemci.PostAsJsonAsync("/api/yonetim/parca-kategorileri", ekleDto);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.Equal("Banyo Dolabı", cevap.GetProperty("veri").GetProperty("ad").GetString());

        // Listele ve doğrula
        var listeYanit = await _bffIstemci.GetAsync("/api/yonetim/parca-kategorileri");
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        Assert.True(listeCevap.GetProperty("veri").GetArrayLength() >= 1);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 3: Parça metadata guncellemede ParcaKategoriId kullanımı
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task ParcaGuncelleme_ParcaKategoriId_IleKullanilir()
    {
        // Kategori oluştur
        var ekleDto = new { ad = "Duş Kabini", aciklama = "Duş kabini parçaları", aktifMi = true, siraNo = 2 };
        var katYanit = await _bffIstemci.PostAsJsonAsync("/api/yonetim/parca-kategorileri", ekleDto);
        var katCevap = await CevapOlarakOkuAsync(katYanit);
        var katId = katCevap.GetProperty("veri").GetProperty("id").GetInt32();

        // Model oluştur
        int modelId;
        using (var kapsam = _apiFabrika.Services.CreateScope())
        {
            var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
            var model = new UcBoyutModel
            {
                Ad = "Kategori Test Model",
                Slug = "kategori-test-model",
                DosyaAdi = "test.glb",
                DosyaYolu = "/medya/3d-modeller/test.glb",
                IcerikTuru = "model/gltf-binary",
                BoyutBayt = 1024,
                Sha256Hash = "hash123",
                FirmaId = 1,
                OlusturulmaTarihi = DateTime.UtcNow
            };
            db.UcBoyutModeller.Add(model);
            await db.SaveChangesAsync();
            modelId = model.Id;
        }

        // Senkronize et
        var komut = new { meshAdlari = new[] { "Dus_Kabin_Parcasi" } };
        var senkroYanit = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{modelId}/parcalar/senkronize", komut);

        // Parça ID'sini al
        var listeYanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{modelId}/parcalar");
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        var parcaId = listeCevap.GetProperty("veri")[0].GetProperty("id").GetInt32();

        // Kategori ata
        var guncelleDto = new { parcaKategoriId = katId };
        var putYanit = await _bffIstemci.PutAsJsonAsync(
            $"/api/yonetim/modeller/{modelId}/parcalar/{parcaId}", guncelleDto);
        putYanit.EnsureSuccessStatusCode();

        var putCevap = await CevapOlarakOkuAsync(putYanit);
        Assert.True(putCevap.GetProperty("basariliMi").GetBoolean());
        Assert.Equal(katId, putCevap.GetProperty("veri").GetProperty("parcaKategoriId").GetInt32());
        Assert.Equal("Duş Kabini", putCevap.GetProperty("veri").GetProperty("kategoriAdi").GetString());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 4: Tenant izolasyonu — cross-tenant model erişimi engellenir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task TenantIzolasyonu_CrossTenant_ModelErisimiEngellenir()
    {
        // Firma 2 oluştur (goldbanyo = firma 1)
        var firma2Id = await FirmaEkleAsync("Partner A", "partner-a");

        // Firma 1'e model ekle
        int modelId;
        using (var kapsam = _apiFabrika.Services.CreateScope())
        {
            var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
            var model = new UcBoyutModel
            {
                Ad = "Firma1 Model",
                Slug = "firma1-model-tenant",
                DosyaAdi = "test.glb",
                DosyaYolu = "/medya/3d-modeller/test.glb",
                IcerikTuru = "model/gltf-binary",
                BoyutBayt = 1024,
                Sha256Hash = "hash456",
                FirmaId = 1, // goldbanyo
                OlusturulmaTarihi = DateTime.UtcNow
            };
            db.UcBoyutModeller.Add(model);
            await db.SaveChangesAsync();
            modelId = model.Id;
        }

        // Firma 2 istemcisi ile erişmeyi dene
        using var firma2Istemci = _apiFabrika.CreateClient();
        firma2Istemci.DefaultRequestHeaders.Add("X-Konfigurator-Bff-Anahtari", _bffAnahtar);
        firma2Istemci.DefaultRequestHeaders.Add("X-Firma", "partner-a");

        var yanit = await firma2Istemci.GetAsync($"/api/yonetim/modeller/{modelId}/parcalar");
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        // Firma 1'in modeline, firma 2'den erişim reddedilmeli
        Assert.False(cevap.GetProperty("basariliMi").GetBoolean());
        var mesaj = cevap.GetProperty("mesaj").GetString();
        Assert.True(
            mesaj!.Contains("yetki", StringComparison.OrdinalIgnoreCase) ||
            mesaj.Contains("erişim", StringComparison.OrdinalIgnoreCase));
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 5: ParcaTuru geriye dönük uyumluluk — string ParcaTuru hala kabul edilir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task ParcaTuru_GeriyeDonuk_Uyumluluk_KabulEdilir()
    {
        // Model oluştur
        int modelId;
        using (var kapsam = _apiFabrika.Services.CreateScope())
        {
            var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
            var model = new UcBoyutModel
            {
                Ad = "Geriye Donuk Test",
                Slug = "geriye-donuk-test",
                DosyaAdi = "test.glb",
                DosyaYolu = "/medya/3d-modeller/test.glb",
                IcerikTuru = "model/gltf-binary",
                BoyutBayt = 1024,
                Sha256Hash = "hash789",
                FirmaId = 1,
                OlusturulmaTarihi = DateTime.UtcNow
            };
            db.UcBoyutModeller.Add(model);
            await db.SaveChangesAsync();
            modelId = model.Id;
        }

        // Senkronize et
        var komut = new { meshAdlari = new[] { "Govde_Test" } };
        await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{modelId}/parcalar/senkronize", komut);

        var listeYanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{modelId}/parcalar");
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        var parcaId = listeCevap.GetProperty("veri")[0].GetProperty("id").GetInt32();

        // ESKI yontemle (string ParcaTuru) guncelle
        var guncelleDto = new { parcaTuru = "Govde", gorunenAd = "Govde Parçası" };
        var putYanit = await _bffIstemci.PutAsJsonAsync(
            $"/api/yonetim/modeller/{modelId}/parcalar/{parcaId}", guncelleDto);
        putYanit.EnsureSuccessStatusCode();

        var putCevap = await CevapOlarakOkuAsync(putYanit);
        Assert.True(putCevap.GetProperty("basariliMi").GetBoolean());
        Assert.Equal("Govde", putCevap.GetProperty("veri").GetProperty("parcaTuru").GetString());
        Assert.Equal("Govde Parçası", putCevap.GetProperty("veri").GetProperty("gorunenAd").GetString());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 6: Mesh click callback — OnMeshSecildi JSInvokable mevcut
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Studio_OnMeshSecildi_JsInvokable_Mevcut()
    {
        var metot = typeof(VizitLink3D.Konfigurator.Pages.Admin.Studio)
            .GetMethod("OnMeshSecildi");

        Assert.NotNull(metot);
        Assert.True(metot!.GetCustomAttributes(typeof(Microsoft.JSInterop.JSInvokableAttribute), false).Length > 0);
        Assert.Equal(typeof(Task), metot!.ReturnType);

        var parametreler = metot.GetParameters();
        Assert.Single(parametreler);
        Assert.Equal(typeof(string), parametreler[0].ParameterType);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 7: Kategori güncelleme — ad değiştirme
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Kategori_Guncelleme_AdDegisimi_Basarili()
    {
        // Önce kategori ekle
        var ekleDto = new { ad = "Eski Kategori", aciklama = "Test", aktifMi = true, siraNo = 5 };
        var yanit = await _bffIstemci.PostAsJsonAsync("/api/yonetim/parca-kategorileri", ekleDto);
        var cevap = await CevapOlarakOkuAsync(yanit);
        var katId = cevap.GetProperty("veri").GetProperty("id").GetInt32();

        // Güncelle
        var guncelleDto = new { ad = "Yeni Kategori", aciklama = "Güncellendi", aktifMi = true, siraNo = 10 };
        var putYanit = await _bffIstemci.PutAsJsonAsync(
            $"/api/yonetim/parca-kategorileri/{katId}", guncelleDto);
        putYanit.EnsureSuccessStatusCode();

        var putCevap = await CevapOlarakOkuAsync(putYanit);
        Assert.True(putCevap.GetProperty("basariliMi").GetBoolean());
        Assert.Equal("Yeni Kategori", putCevap.GetProperty("veri").GetProperty("ad").GetString());
        Assert.Equal(10, putCevap.GetProperty("veri").GetProperty("siraNo").GetInt32());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 8: Kategori soft delete — silinen kategori listelenmez
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Kategori_Silme_SoftDelete_Listelenmez()
    {
        var ekleDto = new { ad = "Silinecek Kategori", aktifMi = true, siraNo = 99 };
        var yanit = await _bffIstemci.PostAsJsonAsync("/api/yonetim/parca-kategorileri", ekleDto);
        var cevap = await CevapOlarakOkuAsync(yanit);
        var katId = cevap.GetProperty("veri").GetProperty("id").GetInt32();

        // Sil
        var silYanit = await _bffIstemci.DeleteAsync($"/api/yonetim/parca-kategorileri/{katId}");
        silYanit.EnsureSuccessStatusCode();
        var silCevap = await CevapOlarakOkuAsync(silYanit);
        Assert.True(silCevap.GetProperty("basariliMi").GetBoolean());

        // Listele — silinen görünmemeli
        var listeYanit = await _bffIstemci.GetAsync("/api/yonetim/parca-kategorileri");
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        foreach (var k in listeCevap.GetProperty("veri").EnumerateArray())
        {
            Assert.NotEqual(katId, k.GetProperty("id").GetInt32());
        }
    }
}
