extern alias KonfApi;

using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using KonfApi::VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Testler;

/// <summary>
/// Paylaşımlı test factory — varsayılan Program.cs rate limit (modelyukleme: 10/dk) ile çalışır.
/// Ana test sınıfı 10'dan az POST isteği gönderir.
/// </summary>
public class KonfWebAppFactory : WebApplicationFactory<KonfApi::VizitLink3D.Konfigurator.Api.Program>, IAsyncLifetime
{
    private readonly string _geciciKlasorYolu;
    private readonly string _sqliteDosyaYolu;
    private readonly string _webRootYolu;

    public KonfWebAppFactory()
    {
        _geciciKlasorYolu = Path.Combine(Path.GetTempPath(), "VizitLink3D_Test_" + Guid.NewGuid().ToString("N"));
        _webRootYolu = Path.Combine(_geciciKlasorYolu, "wwwroot");
        _sqliteDosyaYolu = Path.Combine(_geciciKlasorYolu, "KonfiguratorTest.db");

        Directory.CreateDirectory(_webRootYolu);
        Directory.CreateDirectory(Path.Combine(_webRootYolu, "medya", "3d-modeller"));
    }

    public string GeciciKlasorYolu => _geciciKlasorYolu;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BffGuvenlik:Anahtar"] = "test-gizli-anahtar",
                ["GlbYukleme:MaxDosyaBoyutuMb"] = "1",
                ["ConnectionStrings:KonfiguratorVeriTabani"] = $"Data Source={_sqliteDosyaYolu}"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<KonfiguratorDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<KonfiguratorDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_sqliteDosyaYolu}");
            });
        });

        builder.UseContentRoot(_geciciKlasorYolu);
        builder.UseWebRoot(_webRootYolu);
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        try
        {
            if (Directory.Exists(_geciciKlasorYolu))
                Directory.Delete(_geciciKlasorYolu, true);
        }
        catch { }
    }
}

public class KonfiguratorModellerTestleri : IClassFixture<KonfWebAppFactory>, IDisposable
{
    private readonly KonfWebAppFactory _fabrika;
    private readonly HttpClient _istemci;

    public KonfiguratorModellerTestleri(KonfWebAppFactory fabrika)
    {
        _fabrika = fabrika;
        _istemci = fabrika.CreateClient();
    }

    public void Dispose()
    {
        _istemci.Dispose();
    }

    private static byte[] GecerliGlbDosyasiOlustur(long ekVeriBoyutu = 100)
    {
        var toplamBoyut = 12 + ekVeriBoyutu;
        var icerik = new byte[toplamBoyut];

        icerik[0] = 0x67; icerik[1] = 0x6C; icerik[2] = 0x54; icerik[3] = 0x46;
        BitConverter.TryWriteBytes(new Span<byte>(icerik, 4, 4), (uint)2);
        BitConverter.TryWriteBytes(new Span<byte>(icerik, 8, 4), (uint)toplamBoyut);

        for (var i = 12; i < icerik.Length; i++)
            icerik[i] = (byte)(i % 256);

        return icerik;
    }

    private static byte[] GecersizDosyaOlustur(long boyut = 100)
    {
        var icerik = new byte[boyut];
        for (var i = 0; i < icerik.Length; i++)
            icerik[i] = (byte)(i % 256);
        return icerik;
    }

    private static byte[] HataliSurumGlbDosyasiOlustur(long ekVeriBoyutu = 100)
    {
        var toplamBoyut = 12 + ekVeriBoyutu;
        var icerik = new byte[toplamBoyut];
        icerik[0] = 0x67; icerik[1] = 0x6C; icerik[2] = 0x54; icerik[3] = 0x46;
        BitConverter.TryWriteBytes(new Span<byte>(icerik, 4, 4), (uint)99);
        BitConverter.TryWriteBytes(new Span<byte>(icerik, 8, 4), (uint)toplamBoyut);
        for (var i = 12; i < icerik.Length; i++) icerik[i] = (byte)(i % 256);
        return icerik;
    }

    private static byte[] HataliUzunlukGlbDosyasiOlustur(long ekVeriBoyutu = 100)
    {
        var toplamBoyut = 12 + ekVeriBoyutu;
        var icerik = new byte[toplamBoyut];
        icerik[0] = 0x67; icerik[1] = 0x6C; icerik[2] = 0x54; icerik[3] = 0x46;
        BitConverter.TryWriteBytes(new Span<byte>(icerik, 4, 4), (uint)2);
        BitConverter.TryWriteBytes(new Span<byte>(icerik, 8, 4), (uint)(toplamBoyut + 999));
        for (var i = 12; i < icerik.Length; i++) icerik[i] = (byte)(i % 256);
        return icerik;
    }

    private static HttpRequestMessage BffIstekOlustur(HttpMethod metot, string url, HttpContent? icerik = null)
    {
        var istek = new HttpRequestMessage(metot, url) { Content = icerik };
        istek.Headers.Add("X-Konfigurator-Bff-Anahtari", "test-gizli-anahtar");
        return istek;
    }

    // TEST 1: Boş liste başarılı döner (GET)
    [Fact]
    public async Task BosListe_BasariliDoner()
    {
        var cevap = await _istemci.GetAsync("/api/modeller");
        cevap.EnsureSuccessStatusCode();
        var icerik = await cevap.Content.ReadAsStringAsync();
        Assert.Contains("\"basariliMi\":true", icerik, StringComparison.OrdinalIgnoreCase);
    }

    // TEST 2: Geçerli GLB yükleme başarılı olur (POST #1)
    [Fact]
    public async Task GecerliGlbYukle_BasariliOlur()
    {
        var glbIcerik = GecerliGlbDosyasiOlustur();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Test Model"), "ad" },
            { new StringContent("Test açıklaması"), "aciklama" },
            { new ByteArrayContent(glbIcerik) { Headers = { ContentType = new MediaTypeHeaderValue("model/gltf-binary") } }, "dosya", "test-model.glb" }
        };
        var istek = BffIstekOlustur(HttpMethod.Post, "/api/yonetim/modeller", form);
        var cevap = await _istemci.SendAsync(istek);
        cevap.EnsureSuccessStatusCode();
        var icerik = await cevap.Content.ReadAsStringAsync();
        Assert.Contains("\"basariliMi\":true", icerik, StringComparison.OrdinalIgnoreCase);
    }

    // TEST 3: Geçersiz uzantı reddedilir (POST #2)
    [Fact]
    public async Task GecersizGlbDosyasi_UzantiVeSihirliBaytReddedilir()
    {
        var gecersizIcerik = GecersizDosyaOlustur();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Geçersiz Model"), "ad" },
            { new ByteArrayContent(gecersizIcerik) { Headers = { ContentType = new MediaTypeHeaderValue("application/octet-stream") } }, "dosya", "test-model.txt" }
        };
        var istek = BffIstekOlustur(HttpMethod.Post, "/api/yonetim/modeller", form);
        var cevap = await _istemci.SendAsync(istek);
        var icerik = await cevap.Content.ReadAsStringAsync();
        Assert.Contains("\"basariliMi\":false", icerik, StringComparison.OrdinalIgnoreCase);
    }

    // TEST 4: Eksik BFF anahtarı → 401 (POST #3)
    [Fact]
    public async Task EksikBffAnahtari_401Doner()
    {
        var glbIcerik = GecerliGlbDosyasiOlustur();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Test Model"), "ad" },
            { new ByteArrayContent(glbIcerik) { Headers = { ContentType = new MediaTypeHeaderValue("model/gltf-binary") } }, "dosya", "test-model.glb" }
        };
        var istek = new HttpRequestMessage(HttpMethod.Post, "/api/yonetim/modeller") { Content = form };
        var cevap = await _istemci.SendAsync(istek);
        Assert.Equal(HttpStatusCode.Unauthorized, cevap.StatusCode);
    }

    // TEST 5: Yanlış BFF anahtarı → 401 (POST #4)
    [Fact]
    public async Task YanlisBffAnahtari_401Doner()
    {
        var glbIcerik = GecerliGlbDosyasiOlustur();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Test Model"), "ad" },
            { new ByteArrayContent(glbIcerik) { Headers = { ContentType = new MediaTypeHeaderValue("model/gltf-binary") } }, "dosya", "test-model.glb" }
        };
        var istek = new HttpRequestMessage(HttpMethod.Post, "/api/yonetim/modeller") { Content = form };
        istek.Headers.Add("X-Konfigurator-Bff-Anahtari", "yanlis-anahtar");
        var cevap = await _istemci.SendAsync(istek);
        Assert.Equal(HttpStatusCode.Unauthorized, cevap.StatusCode);
    }

    // TEST 6: Public DTO'da iç detaylar sızmaz (POST #5)
    [Fact]
    public async Task PublicDto_GuvenliDetaylarIcermez()
    {
        var glbIcerik = GecerliGlbDosyasiOlustur();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Güvenli Model"), "ad" },
            { new StringContent("Dto kontrol açıklaması"), "aciklama" },
            { new ByteArrayContent(glbIcerik) { Headers = { ContentType = new MediaTypeHeaderValue("model/gltf-binary") } }, "dosya", "guvenli-model.glb" }
        };
        var yuklemeCevabi = await _istemci.SendAsync(BffIstekOlustur(HttpMethod.Post, "/api/yonetim/modeller", form));
        yuklemeCevabi.EnsureSuccessStatusCode();
        var yuklemeIcerigi = await yuklemeCevabi.Content.ReadAsStringAsync();

        var slugBaslangic = yuklemeIcerigi.IndexOf("\"slug\":\"", StringComparison.OrdinalIgnoreCase);
        Assert.True(slugBaslangic >= 0, "Slug bulunamadı");
        slugBaslangic += 8;
        var slugBitis = yuklemeIcerigi.IndexOf("\"", slugBaslangic, StringComparison.OrdinalIgnoreCase);
        var slug = yuklemeIcerigi.Substring(slugBaslangic, slugBitis - slugBaslangic);

        var getirmeCevabi = await _istemci.GetAsync($"/api/modeller/{slug}");
        getirmeCevabi.EnsureSuccessStatusCode();
        var getirmeIcerigi = await getirmeCevabi.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"dosyaYolu\"", getirmeIcerigi, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"sha256Hash\"", getirmeIcerigi, StringComparison.OrdinalIgnoreCase);
    }

    // TEST 7: Soft-deleted model public listede görünmez (POST #6)
    [Fact]
    public async Task SilinmisModel_PublicListedeGozukmez()
    {
        var glbIcerik = GecerliGlbDosyasiOlustur();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Model Silinecek"), "ad" },
            { new StringContent("Silinecek model"), "aciklama" },
            { new ByteArrayContent(glbIcerik) { Headers = { ContentType = new MediaTypeHeaderValue("model/gltf-binary") } }, "dosya", "silinecek-model.glb" }
        };
        var yuklemeCevabi = await _istemci.SendAsync(BffIstekOlustur(HttpMethod.Post, "/api/yonetim/modeller", form));
        yuklemeCevabi.EnsureSuccessStatusCode();
        var yuklemeIcerigi = await yuklemeCevabi.Content.ReadAsStringAsync();

        var idBaslangic = yuklemeIcerigi.IndexOf("\"id\":", StringComparison.OrdinalIgnoreCase);
        Assert.True(idBaslangic >= 0);
        idBaslangic += 5;
        var idBitis = yuklemeIcerigi.IndexOf(",", idBaslangic, StringComparison.OrdinalIgnoreCase);
        if (idBitis < 0) idBitis = yuklemeIcerigi.IndexOf("}", idBaslangic, StringComparison.OrdinalIgnoreCase);
        var id = int.Parse(yuklemeIcerigi.Substring(idBaslangic, idBitis - idBaslangic).Trim());

        using (var scope = _fabrika.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
            var model = await db.UcBoyutModeller.FindAsync(id);
            Assert.NotNull(model);
            model!.SilindiMi = true;
            model.SilinmeTarihi = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var listeCevabi = await _istemci.GetAsync("/api/modeller");
        listeCevabi.EnsureSuccessStatusCode();
        var listeIcerigi = await listeCevabi.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Model Silinecek", listeIcerigi, StringComparison.OrdinalIgnoreCase);
    }

    // TEST 8 (P03-A): Malformed GLB header — hatalı version (POST #7)
    [Fact]
    public async Task HataliSurumGlbBasligi_Reddedilir()
    {
        var glbIcerik = HataliSurumGlbDosyasiOlustur();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Hatalı Sürüm Model"), "ad" },
            { new ByteArrayContent(glbIcerik) { Headers = { ContentType = new MediaTypeHeaderValue("model/gltf-binary") } }, "dosya", "hatali-surum.glb" }
        };
        var cevap = await _istemci.SendAsync(BffIstekOlustur(HttpMethod.Post, "/api/yonetim/modeller", form));
        var icerik = await cevap.Content.ReadAsStringAsync();
        Assert.Contains("\"basariliMi\":false", icerik, StringComparison.OrdinalIgnoreCase);
    }

    // TEST 9 (P03-A): Malformed GLB header — hatalı total length (POST #8)
    [Fact]
    public async Task HataliUzunlukGlbBasligi_Reddedilir()
    {
        var glbIcerik = HataliUzunlukGlbDosyasiOlustur();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Hatalı Uzunluk Model"), "ad" },
            { new ByteArrayContent(glbIcerik) { Headers = { ContentType = new MediaTypeHeaderValue("model/gltf-binary") } }, "dosya", "hatali-uzunluk.glb" }
        };
        var cevap = await _istemci.SendAsync(BffIstekOlustur(HttpMethod.Post, "/api/yonetim/modeller", form));
        var icerik = await cevap.Content.ReadAsStringAsync();
        Assert.Contains("\"basariliMi\":false", icerik, StringComparison.OrdinalIgnoreCase);
    }

    // TEST 10 (P03-A): Path traversal dosya adı reddedilir — tek theory testi (POST #9)
    [Theory]
    [InlineData("../etc/passwd.glb")]
    [InlineData("..\\..\\windows\\system.glb")]
    public async Task PathTraversalDosyaAdi_Reddedilir(string tehlikeliAd)
    {
        var glbIcerik = GecerliGlbDosyasiOlustur();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Path Traversal"), "ad" },
            { new ByteArrayContent(glbIcerik) { Headers = { ContentType = new MediaTypeHeaderValue("model/gltf-binary") } }, "dosya", tehlikeliAd }
        };
        var cevap = await _istemci.SendAsync(BffIstekOlustur(HttpMethod.Post, "/api/yonetim/modeller", form));
        var icerik = await cevap.Content.ReadAsStringAsync();
        Assert.Contains("\"basariliMi\":false", icerik, StringComparison.OrdinalIgnoreCase);
    }
}
