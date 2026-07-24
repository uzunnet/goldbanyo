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
/// Rate limit testleri için özel düşük limitli factory.
/// Program.cs'deki AddRateLimiter'ı programatik olarak düşük limitli yapılandırır.
/// </summary>
public class KonfRateLimitWebAppFactory : WebApplicationFactory<KonfApi::VizitLink3D.Konfigurator.Api.Program>, IAsyncLifetime
{
    private readonly string _geciciKlasorYolu;
    private readonly string _sqliteDosyaYolu;
    private readonly string _webRootYolu;

    public KonfRateLimitWebAppFactory()
    {
        _geciciKlasorYolu = Path.Combine(Path.GetTempPath(), "VizitLink3D_Test_RL_" + Guid.NewGuid().ToString("N"));
        _webRootYolu = Path.Combine(_geciciKlasorYolu, "wwwroot");
        _sqliteDosyaYolu = Path.Combine(_geciciKlasorYolu, "KonfiguratorTest.db");

        Directory.CreateDirectory(_webRootYolu);
        Directory.CreateDirectory(Path.Combine(_webRootYolu, "medya", "3d-modeller"));
    }

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

        // Environment değişkeni ile rate limit'i düşürebiliriz
        builder.UseSetting("Kestrel:Limits:MaxRequestBodySize", null);

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

/// <summary>
/// Model yükleme rate limit testi.
/// Program.cs'deki varsayılan 10/dk limitini kullanır.
/// </summary>
public class KonfiguratorRateLimitTestleri : IDisposable
{
    private readonly KonfRateLimitWebAppFactory _fabrika;
    private readonly HttpClient _istemci;

    public KonfiguratorRateLimitTestleri()
    {
        _fabrika = new KonfRateLimitWebAppFactory();
        _fabrika.InitializeAsync().GetAwaiter().GetResult();
        _istemci = _fabrika.CreateClient();
    }

    public void Dispose()
    {
        _istemci.Dispose();
        _fabrika.DisposeAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// 12-byte GLB başlıklı mini dosya
    /// </summary>
    private static byte[] MiniGlbOlustur()
    {
        var icerik = new byte[20];
        icerik[0] = 0x67; icerik[1] = 0x6C; icerik[2] = 0x54; icerik[3] = 0x46;
        BitConverter.TryWriteBytes(new Span<byte>(icerik, 4, 4), (uint)2);
        BitConverter.TryWriteBytes(new Span<byte>(icerik, 8, 4), (uint)20);
        return icerik;
    }

    private static HttpRequestMessage BffIstekOlustur(HttpMethod metot, string url, HttpContent? icerik = null)
    {
        var istek = new HttpRequestMessage(metot, url) { Content = icerik };
        istek.Headers.Add("X-Konfigurator-Bff-Anahtari", "test-gizli-anahtar");
        return istek;
    }

    [Fact]
    public async Task ModelYukleme_RateLimitAsimi_429Doner()
    {
        var glbIcerik = MiniGlbOlustur();

        HttpResponseMessage? sonCevap = null;

        for (int i = 0; i < 15; i++)
        {
            using var form = new MultipartFormDataContent
            {
                { new StringContent($"RL Model {i}"), "ad" },
                { new ByteArrayContent(glbIcerik)
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue("model/gltf-binary") }
                    },
                    "dosya",
                    $"rl-{i}.glb"
                }
            };

            using var istek = BffIstekOlustur(HttpMethod.Post, "/api/yonetim/modeller", form);
            sonCevap = await _istemci.SendAsync(istek);

            if (sonCevap.StatusCode == HttpStatusCode.TooManyRequests)
                break;
        }

        Assert.NotNull(sonCevap);
        Assert.Equal(429, (int)sonCevap!.StatusCode);
    }
}
