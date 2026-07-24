using System.Net;
using System.Net.Http.Json;
using System.Text;
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
/// P06-C: Studio parca paneli ve BFF katmani entegrasyon testleri.
/// En az 8 test — senkronizasyon, metadata, guvenlik, arayuz dogrulama.
/// GoldBanyo referansi, localStorage, dogrudan tarayici API'si YOK.
/// </summary>
public class StudioParcaTestleri : IDisposable
{
    private readonly WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program> _apiFabrika;
    private readonly WebApplicationFactory<Program> _bffFabrika;
    private readonly HttpClient _bffIstemci;
    private readonly HttpClient _genelIstemci;
    private readonly string _testDbYolu;
    private readonly string _bffAnahtar = "p06c-test-gizli-anahtar";

    public StudioParcaTestleri()
    {
        _testDbYolu = Path.Combine(Path.GetTempPath(), $"studio_parca_test_{Guid.NewGuid():N}.db");

        // ── API fabrikasi (5116 benzeri) ──
        _apiFabrika = new WebApplicationFactory<VizitLink3D.Konfigurator.Api.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:KonfiguratorVeriTabani", $"Data Source={_testDbYolu}");
                builder.UseSetting("BffGuvenlik:Anahtar", _bffAnahtar);
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

        // BFF anahtarli istemci
        _bffIstemci = _apiFabrika.CreateClient();
        _bffIstemci.DefaultRequestHeaders.Add("X-Konfigurator-Bff-Anahtari", _bffAnahtar);

        // Genel (anahtarsiz) istemci
        _genelIstemci = _apiFabrika.CreateClient();

        // ── BFF fabrikasi (5114 Blazor) — HTML ve redirect testleri icin ──
        _bffFabrika = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ApiAyarlari:BaseUrl", "http://localhost:5116/");
                builder.UseSetting("BffGuvenlik:Anahtar", _bffAnahtar);
                builder.UseSetting("UygulamaAyarlari:Port", "5114");
            });
    }

    public void Dispose()
    {
        _bffIstemci.Dispose();
        _genelIstemci.Dispose();
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

    private async Task<UcBoyutModel> ModelEkleAsync(string ad, string slug, bool aktifMi = true)
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
    // TEST 1: Senkronize edilmeden önce parça listesi boş döner
    // (No sync before explicit action — otomatik DB mutasyonu YAPILMAZ)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task SenkronizeEdilmedenOnce_ParcaListesiBosDoner()
    {
        var model = await ModelEkleAsync("Bos Model", "bos-model");

        // Model var ama HIC senkronizasyon yapilmadi
        var yanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{model.Id}/parcalar");
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.Equal(0, cevap.GetProperty("veri").GetArrayLength());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 2: Senkronizasyon mesh adlarini guvenle iletir
    // (Sync forwarding safe list — BFF API'ye dogru mesh adlarini gonderir)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senkronize_MeshAdlariGuvenleIletilir()
    {
        var model = await ModelEkleAsync("Iletim Test", "iletim-test");

        var komut = new { meshAdlari = new[] { "Govde_Mesh", "Kapak_Mesh" } };
        var yanit = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());
        Assert.Equal(2, cevap.GetProperty("veri").GetProperty("eklenen").GetInt32());

        // GET ile dogrula: 2 parca, mesh adlari eslesmeli
        var listeYanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{model.Id}/parcalar");
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        Assert.Equal(2, listeCevap.GetProperty("veri").GetArrayLength());

        var meshAdlari = listeCevap.GetProperty("veri").EnumerateArray()
            .Select(p => p.GetProperty("meshAdi").GetString())
            .OrderBy(x => x)
            .ToList();

        Assert.Contains("Govde_Mesh", meshAdlari);
        Assert.Contains("Kapak_Mesh", meshAdlari);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 3: Metadata guncelleme BFF uzerinden API'ye iletilir
    // (Metadata save forwarding)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task MetadataGuncelle_BffUzerindenApiyeIletilir()
    {
        var model = await ModelEkleAsync("Meta Iletim", "meta-iletim");

        // Once senkronize et
        var komut = new { meshAdlari = new[] { "Cekmece_Sol" } };
        var senkroYanit = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut);
        senkroYanit.EnsureSuccessStatusCode();

        // Parca ID'sini al
        var listeYanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{model.Id}/parcalar");
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        var parcaId = listeCevap.GetProperty("veri")[0].GetProperty("id").GetInt32();

        // Metadata guncelle
        var guncelleDto = new
        {
            gorunenAd = "Sol Çekmece",
            parcaTuru = "Cekmece",
            renkDegistirilebilirMi = true,
            gorunurMu = true,
            varsayilanRenk = "#4a7c59"
        };

        var putYanit = await _bffIstemci.PutAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/{parcaId}", guncelleDto);
        putYanit.EnsureSuccessStatusCode();

        var putCevap = await CevapOlarakOkuAsync(putYanit);
        Assert.True(putCevap.GetProperty("basariliMi").GetBoolean());
        Assert.Equal("Sol Çekmece", putCevap.GetProperty("veri").GetProperty("gorunenAd").GetString());
        Assert.Equal("Cekmece", putCevap.GetProperty("veri").GetProperty("parcaTuru").GetString());
        Assert.True(putCevap.GetProperty("veri").GetProperty("renkDegistirilebilirMi").GetBoolean());
        Assert.Equal("#4a7c59", putCevap.GetProperty("veri").GetProperty("varsayilanRenk").GetString());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 4: BFF anahtari HTML'de gorunmez
    // (BFF secret not HTML — guvenlik)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task BffAnahtari_HtmlCiktisindaGorunmez()
    {
        var istemci = _bffFabrika.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // /saglik endpoint'ini cek — HTML'de anahtar OLMAMALI
        var yanit = await istemci.GetAsync("/saglik");
        yanit.EnsureSuccessStatusCode();

        var html = await yanit.Content.ReadAsStringAsync();

        // BFF anahtari hicbir sekilde HTML ciktisinda yer almamali
        Assert.DoesNotContain(_bffAnahtar, html);
        // Base64, encode edilmis hali de olmamali
        var base64Anahtar = Convert.ToBase64String(Encoding.UTF8.GetBytes(_bffAnahtar));
        Assert.DoesNotContain(base64Anahtar, html);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 5: Senkronizasyon sonrasi tum parcalar Diger turunde
    // (No type guessing rendering — tur tahmini YAPILMAZ)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senkronize_TumParcalarDigerTurunde_TurTahminiYapilmaz()
    {
        var model = await ModelEkleAsync("Tur Test", "tur-test-p06c");

        // Govde, Cekmece, LED gibi anlamli isimlere sahip mesh'ler
        var komut = new { meshAdlari = new[] { "Govde_Ana", "Cekmece_Alt", "LED_Serit", "Kapak_Ust" } };
        var yanit = await _bffIstemci.PostAsJsonAsync(
            $"/api/yonetim/modeller/{model.Id}/parcalar/senkronize", komut);
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());

        // Admin listesinde TUM parcalar "Diger" olmali
        var listeYanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{model.Id}/parcalar");
        var listeCevap = await CevapOlarakOkuAsync(listeYanit);
        Assert.True(listeCevap.GetProperty("basariliMi").GetBoolean());

        foreach (var parca in listeCevap.GetProperty("veri").EnumerateArray())
        {
            Assert.Equal("Diger", parca.GetProperty("parcaTuru").GetString());
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 6: MeshSecAsync arayuzde mevcut ve dogru imzaya sahip
    // (Wrapper selection integration)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Arayuz_MeshSecAsync_DogruImzayaSahip()
    {
        var metot = typeof(IUcBoyutGoruntuleyiciServisi).GetMethod("MeshSecAsync");

        Assert.NotNull(metot);
        Assert.Equal(typeof(Task<bool>), metot!.ReturnType);

        var parametreler = metot.GetParameters();
        Assert.Single(parametreler);
        Assert.Equal(typeof(string), parametreler[0].ParameterType);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 7: MeshSecimiTemizleAsync arayuzde mevcut
    // (Wrapper selection integration — temizleme)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Arayuz_MeshSecimiTemizleAsync_DogruImzayaSahip()
    {
        var metot = typeof(IUcBoyutGoruntuleyiciServisi).GetMethod("MeshSecimiTemizleAsync");

        Assert.NotNull(metot);
        Assert.Equal(typeof(Task), metot!.ReturnType);
        Assert.Empty(metot.GetParameters());
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 8: UcBoyutGoruntuleyiciServisi MeshSecAsync'i implemente eder
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public void Servis_MeshSecAsync_ImplementeEder()
    {
        var metot = typeof(UcBoyutGoruntuleyiciServisi).GetMethod("MeshSecAsync");

        Assert.NotNull(metot);
        Assert.Equal(typeof(Task<bool>), metot!.ReturnType);
    }

    // ═══════════════════════════════════════════════════════════════
    // TEST 9: Yetkisiz /admin/studio login'e redirect
    // (Unauth redirect)
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

    // ═══════════════════════════════════════════════════════════════
    // TEST 10: Parca paneli bos durum — guvenli empty state
    // (Safe empty state — hata firlatmaz, guvenli JSON doner)
    // ═══════════════════════════════════════════════════════════════
    [Fact]
    public async Task ParcaListesi_BosModel_GuvenliBosDiziDoner()
    {
        var model = await ModelEkleAsync("Guvenli Bos", "guvenli-bos");

        // Hic parca yok — bos dizi donmeli, hata degil
        var yanit = await _bffIstemci.GetAsync($"/api/yonetim/modeller/{model.Id}/parcalar");
        yanit.EnsureSuccessStatusCode();

        var cevap = await CevapOlarakOkuAsync(yanit);
        Assert.True(cevap.GetProperty("basariliMi").GetBoolean());

        var veri = cevap.GetProperty("veri");
        Assert.Equal(JsonValueKind.Array, veri.ValueKind);
        Assert.Equal(0, veri.GetArrayLength());
    }
}
