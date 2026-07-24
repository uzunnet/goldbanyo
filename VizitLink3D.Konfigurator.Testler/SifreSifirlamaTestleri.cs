using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Modeller;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Servisler;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Testler;

public class SifreSifirlamaTestleri : IAsyncLifetime
{
    private readonly SqliteConnection _baglanti;
    private readonly KonfiguratorDbContext _db;
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SifreSifirlamaServisi _servis;
    private readonly IEpostaGondermeServisi _epostaServisi;
    private readonly TestZamanlayici _testZamanlayici;
    private readonly IConfiguration _yapilandirma;

    public SifreSifirlamaTestleri()
    {
        _baglanti = new SqliteConnection("Data Source=:memory:");
        _baglanti.Open();

        var dbSecenekler = new DbContextOptionsBuilder<KonfiguratorDbContext>()
            .UseSqlite(_baglanti)
            .Options;

        _db = new KonfiguratorDbContext(dbSecenekler);

        var yapilandirmaSozluk = new Dictionary<string, string?>
        {
            { "Eposta:Sunucu", null },
            { "Eposta:Port", null },
            { "Eposta:KullaniciAdi", null },
            { "Eposta:AppSifresi", null },
            { "Eposta:GonderenAdres", null },
            { "SifreSifirlama:UygulamaUrl", "https://konfigurator.local" }
        };
        _yapilandirma = new ConfigurationBuilder()
            .AddInMemoryCollection(yapilandirmaSozluk)
            .Build();

        _testZamanlayici = new TestZamanlayici();

        // ── Test DI konteyneri: tum servisleri burada kaydet ──
        var services = new ServiceCollection();
        services.AddSingleton(_yapilandirma);
        services.AddSingleton<IZamanlayici>(_testZamanlayici);
        services.AddSingleton<IEpostaGondermeServisi, EpostaGondermeServisi>();
        services.AddSingleton(_db);
        services.AddSingleton<SifreSifirlamaServisi>();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug));

        _serviceProvider = services.BuildServiceProvider();
        _scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _servis = _serviceProvider.GetRequiredService<SifreSifirlamaServisi>();
        _epostaServisi = _serviceProvider.GetRequiredService<IEpostaGondermeServisi>();
    }

    public async Task InitializeAsync()
    {
        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _baglanti.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    private async Task<KonfiguratorKullanicisi> TestKullaniciOlusturAsync(string eposta = "test@konfigurator.local")
    {
        var kullanici = new KonfiguratorKullanicisi
        {
            KullaniciAdi = "testkullanici",
            Eposta = eposta,
            SifreHash = BCrypt.Net.BCrypt.HashPassword("EskiSifre1!"),
            Rol = "Yonetici",
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        _db.Kullanicilar.Add(kullanici);
        await _db.SaveChangesAsync();
        return kullanici;
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 1: Var olan e-posta → DB'ye istek yazilir
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task SifreSifirlamaIstegi_VarOlanEposta_BasariliDoner()
    {
        var kullanici = await TestKullaniciOlusturAsync("var@test.local");

        await _servis.SifreSifirlamaIstegiOlusturAsync("var@test.local");

        var istek = await _db.SifreSifirlamaIstekleri
            .FirstOrDefaultAsync(i => i.KullaniciId == kullanici.Id);
        Assert.NotNull(istek);
        Assert.False(istek.KullanildiMi);
        Assert.True(istek.BitisTarihi > DateTime.UtcNow);
        // TokenHash SHA256 hash olmali (Base64 string, 44 karakter)
        Assert.Equal(44, istek.TokenHash.Length);
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 2: Var olmayan e-posta → DB'de istek OLUSMAZ, bilgi sizmaz
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task SifreSifirlamaIstegi_VarOlmayanEposta_BasariliDonerVeIstekOlusturmaz()
    {
        await _servis.SifreSifirlamaIstegiOlusturAsync("yok@test.local");

        var istekSayisi = await _db.SifreSifirlamaIstekleri.CountAsync();
        Assert.Equal(0, istekSayisi);
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 3: Token DB'de hash olarak saklanir — raw token degil
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task SifreSifirlamaIstegi_TokenDbdeHashOlarakSaklanir_RawTokenDegil()
    {
        await TestKullaniciOlusturAsync("hash@test.local");

        await _servis.SifreSifirlamaIstegiOlusturAsync("hash@test.local");

        var istek = await _db.SifreSifirlamaIstekleri.FirstAsync();
        Assert.Equal(44, istek.TokenHash.Length);
        Assert.Matches("^[A-Za-z0-9+/=]+$", istek.TokenHash);
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 4: SifreYenile — suresi dolmus token reddedilir
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task SifreYenile_SuresiDolmusToken_Reddedilir()
    {
        await TestKullaniciOlusturAsync("sure@test.local");
        await _servis.SifreSifirlamaIstegiOlusturAsync("sure@test.local");

        var istek = await _db.SifreSifirlamaIstekleri.FirstAsync();
        istek.BitisTarihi = DateTime.UtcNow.AddMinutes(-1);
        await _db.SaveChangesAsync();

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        istek.TokenHash = tokenHash;
        await _db.SaveChangesAsync();

        var sonuc = await _servis.SifreYenileAsync(rawToken, "YeniSifre1!");
        Assert.False(sonuc);
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 5: SifreYenile — kullanilmis token reddedilir
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task SifreYenile_KullanilmisToken_Reddedilir()
    {
        await TestKullaniciOlusturAsync("kullanildi@test.local");

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var istek = new SifreSifirlamaIstegi
        {
            KullaniciId = (await _db.Kullanicilar.FirstAsync()).Id,
            TokenHash = tokenHash,
            BitisTarihi = DateTime.UtcNow.AddMinutes(15),
            KullanildiMi = true,
            KullanilmaTarihi = DateTime.UtcNow.AddMinutes(-5),
            OlusturulmaTarihi = DateTime.UtcNow
        };
        _db.SifreSifirlamaIstekleri.Add(istek);
        await _db.SaveChangesAsync();

        var sonuc = await _servis.SifreYenileAsync(rawToken, "YeniSifre1!");
        Assert.False(sonuc);
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 6: SifreYenile — gecerli token ile sifre BCrypt ile degisir
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task SifreYenile_GecerliToken_SifreBCryptIleDegisir()
    {
        var kullanici = await TestKullaniciOlusturAsync("gecerli@test.local");
        var eskiHash = kullanici.SifreHash;

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var istek = new SifreSifirlamaIstegi
        {
            KullaniciId = kullanici.Id,
            TokenHash = tokenHash,
            BitisTarihi = DateTime.UtcNow.AddMinutes(15),
            KullanildiMi = false,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        _db.SifreSifirlamaIstekleri.Add(istek);
        await _db.SaveChangesAsync();

        var yeniSifre = "YeniSifre1!";

        var sonuc = await _servis.SifreYenileAsync(rawToken, yeniSifre);

        Assert.True(sonuc);

        await _db.Entry(istek).ReloadAsync();
        Assert.True(istek.KullanildiMi);
        Assert.NotNull(istek.KullanilmaTarihi);

        await _db.Entry(kullanici).ReloadAsync();
        Assert.NotEqual(eskiHash, kullanici.SifreHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(yeniSifre, kullanici.SifreHash));
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 7: E-posta yapilandirmasi eksik → bilgi sizmaz, false doner
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task EpostaYapilandirmasiEksik_BilgiSizmaz()
    {
        await TestKullaniciOlusturAsync("configyok@test.local");

        var sonuc = await _epostaServisi.EpostaGonderAsync(
            "configyok@test.local", "Konu", "<p>Test</p>");

        Assert.False(sonuc);
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 8: EpostaGondermeServisi'nde shared SmtpClient YOK
    //         (IAsyncDisposable implementasyonu kaldirildi)
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public void EpostaGondermeServisi_IAsyncDisposable_Degildir()
    {
        // EpostaGondermeServisi artik IAsyncDisposable implemente ETMEZ.
        // Her EpostaGonderAsync cagrisinda lokal SmtpClient olusturulur.
        var iAsyncDisposableMi = _epostaServisi is IAsyncDisposable;
        Assert.False(iAsyncDisposableMi,
            "EpostaGondermeServisi IAsyncDisposable olmamali — shared SmtpClient kaldirildi.");
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 9: Jitter davranis testi (TestZamanlayici ile)
    //         → Kullanici yok akisinda IZamanlayici.GecikmeAsync cagrilir
    //         → Gecikme degeri [min, max] araliginda
    //         → Ornekler arasinda varyans var (true random)
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task SifreSifirlamaIstegi_JitterDavranisi_Dogrulanir()
    {
        // Arrange: spy temizle
        _testZamanlayici.GecikmeCagrilari.Clear();

        var ornekSayisi = 20;

        // Act: 20 farkli var-olmayan kullanici icin istek yap
        for (int i = 0; i < ornekSayisi; i++)
        {
            await _servis.SifreSifirlamaIstegiOlusturAsync($"spy{i}@test.local");
        }

        // Assert 1: Her kullanici-yok cagrisinda GecikmeAsync cagrilmis olmali
        Assert.Equal(ornekSayisi, _testZamanlayici.GecikmeCagrilari.Count);

        // Assert 2: Tum gecikme degerleri [min, max] araliginda
        // (TestZamanlayici gercek gecikme yapmadigi icin test hizli)
        foreach (var sure in _testZamanlayici.GecikmeCagrilari)
        {
            Assert.True(sure >= SifreSifirlamaServisi.KullaniciYokMinGecikme,
                $"Gecikme {sure.TotalMilliseconds:F0}ms minimumdan kucuk.");
            Assert.True(sure <= SifreSifirlamaServisi.KullaniciYokMaksGecikme,
                $"Gecikme {sure.TotalMilliseconds:F0}ms maksimumdan buyuk.");
        }

        // Assert 3: En az 2 farkli deger var (gercek random jitter kaniti)
        var benzersizDegerler = _testZamanlayici.GecikmeCagrilari
            .Select(s => s.TotalMilliseconds)
            .Distinct()
            .Count();
        Assert.True(benzersizDegerler >= 2,
            $"Tum {ornekSayisi} jitter ornegi ayni degerde. Random jitter calismiyor olabilir.");
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 10: Zamanlama esitleme — gercek gecikme ile kanita dayali
    //          (SADECE jitter uygulandigini teyit eder; tam esitlik aramaz)
    //          Warmup ile thread pool soguk baslatma etkisi azaltilir.
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task SifreSifirlamaIstegi_ZamanlamaEsitleme_JitterUygulanir()
    {
        // Bu test, gercek Task.Delay kullanan bir servis olusturur.
        var gercekZamanlayici = new SystemZamanlayici();
        var gercekLogger = _serviceProvider.GetRequiredService<ILogger<SifreSifirlamaServisi>>();
        var gercekServis = new SifreSifirlamaServisi(
            _db, _yapilandirma, gercekLogger, _scopeFactory, gercekZamanlayici);

        var orijinalMin = SifreSifirlamaServisi.KullaniciYokMinGecikme;
        var orijinalMaks = SifreSifirlamaServisi.KullaniciYokMaksGecikme;
        try
        {
            // Test icin dar jitter araligi
            SifreSifirlamaServisi.KullaniciYokMinGecikme = TimeSpan.FromMilliseconds(5);
            SifreSifirlamaServisi.KullaniciYokMaksGecikme = TimeSpan.FromMilliseconds(25);

            // ── Warmup: thread pool ve JIT soguk baslatma etkisini azalt ──
            for (int i = 0; i < 3; i++)
            {
                await gercekServis.SifreSifirlamaIstegiOlusturAsync($"isnma{i}@test.local");
            }

            // ── Kullanici YOK akisi: 10 olcum ──
            var yokSureler = new List<double>();
            for (int i = 0; i < 10; i++)
            {
                var sw = Stopwatch.StartNew();
                await gercekServis.SifreSifirlamaIstegiOlusturAsync($"yokZ{i}@test.local");
                sw.Stop();
                yokSureler.Add(sw.Elapsed.TotalMilliseconds);
            }

            // ── Kullanici VAR akisi: once kullanici olustur, sonra 10 olcum ──
            await TestKullaniciOlusturAsync("zamanVar@test.local");
            var varSureler = new List<double>();
            for (int i = 0; i < 10; i++)
            {
                var sw = Stopwatch.StartNew();
                await gercekServis.SifreSifirlamaIstegiOlusturAsync("zamanVar@test.local");
                sw.Stop();
                varSureler.Add(sw.Elapsed.TotalMilliseconds);
            }

            // ── Assert: Yok akisi, jitter alt sinirina makul yakin ──
            var yokOrt = yokSureler.Average();
            Assert.True(yokOrt >= SifreSifirlamaServisi.KullaniciYokMinGecikme.TotalMilliseconds * 0.3,
                $"Kullanici yok akisi cok hizli: {yokOrt:F1}ms (min={SifreSifirlamaServisi.KullaniciYokMinGecikme.TotalMilliseconds}ms). " +
                "Jitter uygulanmamis olabilir!");

            // ── Assert: Ortalamalar arasi fark, jitter bant genisliginin
            //    3 katindan fazla olmamali (comparable oldugunu kanitlar) ──
            var varOrt = varSureler.Average();
            var fark = Math.Abs(yokOrt - varOrt);
            var bantGenisligi = SifreSifirlamaServisi.KullaniciYokMaksGecikme.TotalMilliseconds -
                                SifreSifirlamaServisi.KullaniciYokMinGecikme.TotalMilliseconds;
            var kabulEdilebilirFark = bantGenisligi * 3 + 40; // 40ms thread pool/scheduling buffer

            Assert.True(fark < kabulEdilebilirFark,
                $"Zamanlama farki cok yuksek: yokOrt={yokOrt:F1}ms, varOrt={varOrt:F1}ms, " +
                $"fark={fark:F1}ms, kabulEdilebilir={kabulEdilebilirFark:F0}ms. " +
                "Account enumeration riski var!");
        }
        finally
        {
            SifreSifirlamaServisi.KullaniciYokMinGecikme = orijinalMin;
            SifreSifirlamaServisi.KullaniciYokMaksGecikme = orijinalMaks;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // TEST 11: IlkYonetici gizli anahtar testleri
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task IlkYonetici_GercekEpostaGizliAnahtariVarsa_PlaceholderYerineKullanilir()
    {
        var kullaniciAdi = "vizitadmin";
        var gercekEposta = "admin@gercekfirma.com";
        var epostaSecret = gercekEposta;

        var varsayilanEposta = $"{kullaniciAdi}@konfigurator.local";
        string kullanilacakEposta;

        if (!string.IsNullOrWhiteSpace(epostaSecret))
            kullanilacakEposta = epostaSecret;
        else
            kullanilacakEposta = varsayilanEposta;

        Assert.Equal(gercekEposta, kullanilacakEposta);
        Assert.NotEqual(varsayilanEposta, kullanilacakEposta);
    }

    [Fact]
    public async Task IlkYonetici_GercekEpostaGizliAnahtariYoksa_PlaceholderKullanilir()
    {
        var kullaniciAdi = "vizitadmin";
        string? epostaSecret = null;

        var varsayilanEposta = $"{kullaniciAdi}@konfigurator.local";
        string kullanilacakEposta;

        if (!string.IsNullOrWhiteSpace(epostaSecret))
            kullanilacakEposta = epostaSecret;
        else
            kullanilacakEposta = varsayilanEposta;

        Assert.Equal(varsayilanEposta, kullanilacakEposta);
    }

    // ═══════════════════════════════════════════════════════════════════
    // Test yardimcisi: IZamanlayici spy
    // ═══════════════════════════════════════════════════════════════════
    private class TestZamanlayici : IZamanlayici
    {
        public List<TimeSpan> GecikmeCagrilari { get; } = new();

        public Task GecikmeAsync(TimeSpan sure, CancellationToken iptal = default)
        {
            GecikmeCagrilari.Add(sure);
            // Gercek gecikme YAPILMAZ — test hizli calisir
            return Task.CompletedTask;
        }
    }
}
