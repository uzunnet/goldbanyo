extern alias KonfApi;

using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KonfApi::VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;
using KonfApi::VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Modeller;
using KonfApi::VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Testler.Moduller.Konfigurator;

public class KonfiguratorApiTestleri
{
    // ───── TEST 1: Saglik kontrolu 200 donmeli ─────
    [Fact]
    public async Task Saglik_Doner()
    {
        using var fabrika = new KonfiguratorWebAppFactory();
        var istemci = fabrika.CreateClient();

        var cevap = await istemci.GetFromJsonAsync<KonfiguratorCevap<string>>("/saglik");

        Assert.NotNull(cevap);
        Assert.True(cevap!.BasariliMi);
        Assert.Equal("Calisiyor", cevap.Veri);
    }

    // ───── TEST 2: Gecerli kullanici ile basarili giris ─────
    [Fact]
    public async Task Giris_GecerliKimlik_BasariliDoner()
    {
        using var fabrika = new KonfiguratorWebAppFactory();
        var istemci = fabrika.CreateClient();

        using var kapsam = fabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

        var kullanici = new KonfiguratorKullanicisi
        {
            KullaniciAdi = "testkullanici",
            Eposta = "test@test.local",
            SifreHash = BCrypt.Net.BCrypt.HashPassword("GucluSifre123!"),
            Rol = "Yonetici"
        };
        db.Kullanicilar.Add(kullanici);
        await db.SaveChangesAsync();

        var girisDto = new GirisDto { KullaniciAdi = "testkullanici", Sifre = "GucluSifre123!" };
        var cevap = await istemci.PostAsJsonAsync("/api/kimlik/giris", girisDto);

        Assert.Equal(HttpStatusCode.OK, cevap.StatusCode);
        var sonuc = await cevap.Content.ReadFromJsonAsync<KonfiguratorCevap<GirisCevapDto>>();
        Assert.NotNull(sonuc);
        Assert.True(sonuc!.BasariliMi);
        Assert.NotNull(sonuc.Veri);
        Assert.Equal("testkullanici", sonuc.Veri!.KullaniciAdi);
        Assert.Equal("Yonetici", sonuc.Veri.Rol);
    }

    // ───── TEST 3: Hatali sifre ile hata donmeli ─────
    [Fact]
    public async Task Giris_HataliSifre_BasarisizDoner()
    {
        using var fabrika = new KonfiguratorWebAppFactory();
        var istemci = fabrika.CreateClient();

        using var kapsam = fabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

        var kullanici = new KonfiguratorKullanicisi
        {
            KullaniciAdi = "kullanici2",
            Eposta = "k2@test.local",
            SifreHash = BCrypt.Net.BCrypt.HashPassword("DogruSifre"),
            Rol = "Yonetici"
        };
        db.Kullanicilar.Add(kullanici);
        await db.SaveChangesAsync();

        var girisDto = new GirisDto { KullaniciAdi = "kullanici2", Sifre = "YanlisSifre" };
        var cevap = await istemci.PostAsJsonAsync("/api/kimlik/giris", girisDto);

        Assert.Equal(HttpStatusCode.OK, cevap.StatusCode);
        var sonuc = await cevap.Content.ReadFromJsonAsync<KonfiguratorCevap<GirisCevapDto>>();
        Assert.NotNull(sonuc);
        Assert.False(sonuc!.BasariliMi);
        Assert.Equal("Kullanici adi veya sifre hatali.", sonuc.Mesaj);
    }

    // ───── TEST 4: Rate limit asimi 429 donmeli ─────
    [Fact]
    public async Task Giris_RateLimitAsimi_429Doner()
    {
        using var fabrika = new KonfiguratorWebAppFactory();
        var istemci = fabrika.CreateClient();

        var girisDto = new GirisDto { KullaniciAdi = "herhangi", Sifre = "herhangi" };

        HttpResponseMessage? sonIstek = null;
        for (int i = 0; i < 6; i++)
        {
            sonIstek = await istemci.PostAsJsonAsync("/api/kimlik/giris", girisDto);
        }

        Assert.NotNull(sonIstek);
        Assert.Equal(429, (int)sonIstek!.StatusCode);
    }

    // ───── TEST 5a: Bos IlkYonetici konfigurasyonu — kullanici olusmaz ─────
    [Fact]
    public async Task IlkYoneticiBootstrap_BosKonfig_KullaniciOlusmaz()
    {
        // hic bir IlkYonetici ayari verilmeden — Program.cs bootstrap user OLUSTURMAMALI
        using var fabrika = new KonfiguratorWebAppFactory();
        var istemci = fabrika.CreateClient();

        using var kapsam = fabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

        var varMi = await db.Kullanicilar.AnyAsync();
        Assert.False(varMi, "Bos IlkYonetici konfigurasyonunda kullanici olusmamali.");
    }

    // ───── TEST 5b: Dolu IlkYonetici konfigurasyonu — BCrypt hash ile yonetici olusur ─────
    [Fact]
    public async Task IlkYoneticiBootstrap_KonfigDolu_YoneticiOlusturur()
    {
        using var fabrika = new KonfiguratorWebAppFactory(
            yoneticiKullaniciAdi: "ilkyonetici",
            yoneticiSifre: "IlkSifre123!");
        var istemci = fabrika.CreateClient();

        using var kapsam = fabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

        var olusan = await db.Kullanicilar.FirstOrDefaultAsync(k => k.KullaniciAdi == "ilkyonetici");
        Assert.NotNull(olusan);
        Assert.Equal("Yonetici", olusan!.Rol);
        Assert.True(BCrypt.Net.BCrypt.Verify("IlkSifre123!", olusan.SifreHash));
    }

    // ───── TEST 6: Token/sifre sizintisi kontrolu ─────
    [Fact]
    public async Task Giris_CevaptaHashVeyaTokenYok()
    {
        using var fabrika = new KonfiguratorWebAppFactory();
        var istemci = fabrika.CreateClient();

        using var kapsam = fabrika.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();

        var kullanici = new KonfiguratorKullanicisi
        {
            KullaniciAdi = "guvenlitest",
            Eposta = "g@test.local",
            SifreHash = BCrypt.Net.BCrypt.HashPassword("Guvenli123!"),
            Rol = "Yonetici"
        };
        db.Kullanicilar.Add(kullanici);
        await db.SaveChangesAsync();

        var girisDto = new GirisDto { KullaniciAdi = "guvenlitest", Sifre = "Guvenli123!" };
        var cevap = await istemci.PostAsJsonAsync("/api/kimlik/giris", girisDto);
        var hamJson = await cevap.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, cevap.StatusCode);
        Assert.DoesNotContain("SifreHash", hamJson);
        Assert.DoesNotContain("Token", hamJson);
        Assert.DoesNotContain("sifre", hamJson.ToLowerInvariant());
    }

    // ───── TEST 7: Bos kullanici adi FluentValidation tarafindan reddedilir ─────
    [Fact]
    public async Task Giris_BosKullaniciAdi_DogrulamaHatasiDoner()
    {
        using var fabrika = new KonfiguratorWebAppFactory();
        var istemci = fabrika.CreateClient();

        var girisDto = new GirisDto { KullaniciAdi = "", Sifre = "sifre123" };
        var cevap = await istemci.PostAsJsonAsync("/api/kimlik/giris", girisDto);

        Assert.Equal(HttpStatusCode.OK, cevap.StatusCode);
        var sonuc = await cevap.Content.ReadFromJsonAsync<KonfiguratorCevap<GirisCevapDto>>();
        Assert.NotNull(sonuc);
        Assert.False(sonuc!.BasariliMi);
        Assert.Contains("Dogrulama hatasi", sonuc.Mesaj);
    }
}
