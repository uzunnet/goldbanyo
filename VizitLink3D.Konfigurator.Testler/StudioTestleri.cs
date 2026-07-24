using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Testler;

/// <summary>
/// Studio sayfasi ve yonetim API entegrasyon testleri.
/// P05-A: 3D Studio admin listesi, yayin durumu, BFF guvenligi.
/// En az 5 test.
/// </summary>
public class StudioTestleri : IDisposable
{
    private readonly WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program> _apiFabrika;
    private readonly WebApplicationFactory<Program> _bffFabrika;
    private readonly HttpClient _bffIstemci;
    private readonly string _testDbYolu;

    public StudioTestleri()
    {
        _testDbYolu = Path.Combine(Path.GetTempPath(), $"studio_test_{Guid.NewGuid():N}.db");

        // ── API fabrikasi (5116 benzeri) ──
        _apiFabrika = new WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:KonfiguratorVeriTabani", $"Data Source={_testDbYolu}");
                builder.UseSetting("BffGuvenlik:Anahtar", "studio-test-gizli-anahtar");
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

        // Test DB semasini olustur
        using var kapsam = _apiFabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
        db.Database.EnsureCreated();

        _bffIstemci = _apiFabrika.CreateClient();
        _bffIstemci.DefaultRequestHeaders.Add("X-Konfigurator-Bff-Anahtari", "studio-test-gizli-anahtar");

        // ── BFF fabrikasi (5114 Blazor) ──
        _bffFabrika = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ApiAyarlari:BaseUrl", "http://localhost:5116/");
                builder.UseSetting("BffGuvenlik:Anahtar", "bff-test-anahtar");
                builder.UseSetting("UygulamaAyarlari:Port", "5114");
            });
    }

    public void Dispose()
    {
        _bffIstemci.Dispose();
        _apiFabrika.Dispose();
        _bffFabrika.Dispose();

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
        var kapsam = _apiFabrika.Services.CreateScope();
        return kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
    }

    private async Task<UcBoyutModel> ModelEkleAsync(string ad, string slug, bool aktifMi = true, bool silindiMi = false)
    {
        using var kapsam = _apiFabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

        var model = new UcBoyutModel
        {
            Ad = ad,
            Slug = slug,
            Aciklama = $"{ad} açıklaması",
            DosyaAdi = $"{slug}.glb",
            DosyaYolu = $"/medya/3d-modeller/{slug}.glb",
            IcerikTuru = "model/gltf-binary",
            BoyutBayt = 2048,
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
    // TEST 1: BFF anahtari olmayan istek → 401 Unauthorized
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task AdminListe_BffAnahtariYok_401Doner()
    {
        using var hamIstemci = _apiFabrika.CreateClient();

        var yanit = await hamIstemci.GetAsync("/api/yonetim/modeller");

        Assert.Equal(HttpStatusCode.Unauthorized, yanit.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 2: Admin listesi GuncellenmeTarihi alanini icerir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task AdminListe_GuncellenmeTarihiAlaniniIcerir()
    {
        await ModelEkleAsync("Studio Model", "studio-model", aktifMi: true);

        var yanit = await _bffIstemci.GetAsync("/api/yonetim/modeller");
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());

        var veri = cevap.GetProperty("veri");
        Assert.True(veri.GetArrayLength() >= 1);

        var ilkModel = veri[0];
        // Studio DTO'su icin gerekli alanlar
        Assert.True(ilkModel.TryGetProperty("id", out _));
        Assert.True(ilkModel.TryGetProperty("ad", out _));
        Assert.True(ilkModel.TryGetProperty("guncellenmeTarihi", out var guncelleme));
        // null olabilir ama property mevcut olmali
        Assert.Equal(JsonValueKind.Null, guncelleme.ValueKind);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 3: Admin listesi pasif modelleri de dondurur
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task AdminListe_PasifModelleriDondurur()
    {
        await ModelEkleAsync("Aktif Studio", "aktif-studio", aktifMi: true);
        await ModelEkleAsync("Pasif Studio", "pasif-studio", aktifMi: false);

        var yanit = await _bffIstemci.GetAsync("/api/yonetim/modeller");
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());

        var veri = cevap.GetProperty("veri");
        Assert.Equal(2, veri.GetArrayLength());

        // Pasif model listede gorunmeli
        var pasifBulundu = false;
        foreach (var m in veri.EnumerateArray())
        {
            if (!m.GetProperty("aktifMi").GetBoolean())
                pasifBulundu = true;
        }
        Assert.True(pasifBulundu, "Pasif model admin listesinde bulunamadi");
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 4: Yayin durumu aktif → pasif, listedeki durum guncellenir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YayinDurumu_AktiftenPasife_Guncellenir()
    {
        var model = await ModelEkleAsync("Toggle Model", "toggle-model", aktifMi: true);

        // Pasif yap
        var govde = new StringContent("""{"aktifMi":false}""", Encoding.UTF8, "application/json");
        var yanit = await _bffIstemci.PutAsync($"/api/yonetim/modeller/{model.Id}/yayin-durumu", govde);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.False(cevap.GetProperty("veri").GetProperty("aktifMi").GetBoolean());

        // Admin listesinden teyit
        var listeYanit = await _bffIstemci.GetAsync("/api/yonetim/modeller");
        listeYanit.EnsureSuccessStatusCode();
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        var veri = listeCevap.GetProperty("veri");
        foreach (var m in veri.EnumerateArray())
        {
            if (m.GetProperty("id").GetInt32() == model.Id)
            {
                Assert.False(m.GetProperty("aktifMi").GetBoolean());
                Assert.NotEqual(JsonValueKind.Null, m.GetProperty("guncellenmeTarihi").ValueKind);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 5: Yayin durumu pasif → aktif, guncellenme tarihi set edilir
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task YayinDurumu_PasiftenAktife_GuncellenmeTarihiSetEdilir()
    {
        var model = await ModelEkleAsync("Aktiflesen Model", "aktiflesen-model", aktifMi: false);

        // Aktif yap
        var govde = new StringContent("""{"aktifMi":true}""", Encoding.UTF8, "application/json");
        var yanit = await _bffIstemci.PutAsync($"/api/yonetim/modeller/{model.Id}/yayin-durumu", govde);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.True(cevap.GetProperty("veri").GetProperty("aktifMi").GetBoolean());

        // GuncellenmeTarihi dolu olmali
        var guncelleme = cevap.GetProperty("veri").GetProperty("guncellenmeTarihi");
        Assert.NotEqual(JsonValueKind.Null, guncelleme.ValueKind);

        // Public API'de gorunmeli
        var genelIstemci = _apiFabrika.CreateClient();
        var genelYanit = await genelIstemci.GetAsync("/api/modeller");
        genelYanit.EnsureSuccessStatusCode();
        var genelCevap = await CevapOlarakOkuAsync(genelYanit);
        var genelVeri = genelCevap.GetProperty("veri");
        Assert.True(genelVeri.GetArrayLength() >= 1);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 6: Bulunamayan ID → hata mesaji
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
    // TEST 7: Studio sayfasi yetkisiz → login redirect
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task StudioSayfasi_Yetkisiz_LoginRedirect()
    {
        var istemci = _bffFabrika.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var yanit = await istemci.GetAsync("/admin/studio");

        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        var konum = yanit.Headers.Location?.ToString() ?? "";
        Assert.Contains("/admin", konum);
    }
}
