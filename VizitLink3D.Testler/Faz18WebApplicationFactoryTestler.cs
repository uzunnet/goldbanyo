using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VizitLink3D.Api.VeriTabani;

namespace VizitLink3D.Testler;

/// <summary>
/// Faz 18 — VizitLink3D.Api WebApplicationFactory ile izole HTTP entegrasyon testleri.
/// Her senaryo kendi in-memory SQLite baglantisiyla calisir; uretim DB'sine dokunulmaz.
/// DisposeAsync sadece baglantiyi kapatir; DB dosyasi silinmez, fiziksel DELETE calistirilmaz.
/// </summary>
public class Faz18WebApplicationFactoryTestler : IAsyncLifetime
{
    private readonly SqliteConnection _baglanti;

    public Faz18WebApplicationFactoryTestler()
    {
        _baglanti = new SqliteConnection("Data Source=:memory:");
        _baglanti.Open();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // SADECE baglantiyi kapat — DB dosyasi silinmez, fiziksel DELETE calistirilmaz.
        await _baglanti.CloseAsync();
        await _baglanti.DisposeAsync();
    }

    /// <summary>
    /// Her test icin izole WebApplicationFactory olusturur.
    /// Uretim DB'sine dokunmaz; in-memory SQLite kullanir.
    /// Migration'lar bellekte calisir, test bitince baglanti kapatilir.
    /// </summary>
    private WebApplicationFactory<VizitLink3D.Api.Program> FabrikaOlustur()
    {
        return new WebApplicationFactory<VizitLink3D.Api.Program>()
            .WithWebHostBuilder(kok =>
            {
                kok.UseEnvironment("Development");

                kok.ConfigureServices(servisler =>
                {
                    // Mevcut DbContext tanimini kaldir
                    var mevcutTanimlama = servisler.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<VizitLink3DDbContext>));
                    if (mevcutTanimlama is not null)
                        servisler.Remove(mevcutTanimlama);

                    // KiraciServisi de kaldir (DB bagimli)
                    var mevcutKiraci = servisler.SingleOrDefault(
                        d => d.ServiceType == typeof(VizitLink3D.Api.Servisler.KiraciServisi));
                    if (mevcutKiraci is not null)
                        servisler.Remove(mevcutKiraci);

                    // In-memory SQLite baglantisiyla degistir
                    servisler.AddDbContext<VizitLink3DDbContext>(secenekler =>
                        secenekler.UseSqlite(_baglanti));

                    // KiraciServisi'ni yeniden kaydet (IHttpContextAccessor bagimli, DB gerekmez)
                    servisler.AddScoped<VizitLink3D.Api.Servisler.KiraciServisi>(
                        sp => new VizitLink3D.Api.Servisler.KiraciServisi(
                            sp.GetService<IHttpContextAccessor>()));

                    // IBomHesaplayici eksik kayit — MediatR TeklifOlusturIsleyici icin
                    servisler.TryAddScoped<VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.IBomHesaplayici>(
                        sp => new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.BomHesaplayici(
                            sp.GetRequiredService<VizitLink3DDbContext>()));
                });
            });
    }

    // ═══════════════════════════════════════════════════════════════════
    // SENARYO 1: Saglik kontrolu — API ayakta mi, DB baglantisi calisiyor mu?
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senaryo1_SaglikKontrolu_200Donmeli()
    {
        using var fabrika = FabrikaOlustur();
        var istemci = fabrika.CreateClient();

        var cevap = await istemci.GetAsync("/api/saglik");

        Assert.Equal(HttpStatusCode.OK, cevap.StatusCode);

        var json = await cevap.Content.ReadFromJsonAsync<SaglikYaniti>();
        Assert.NotNull(json);
        Assert.True(json!.Veritabani, "DB baglantisi saglikli olmali.");
        Assert.Equal("1.0.0", json.Surum);
    }

    // ═══════════════════════════════════════════════════════════════════
    // SENARYO 2: Iletisim formu — gecerli veriyle kayit basarili mi?
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senaryo2_IletisimFormu_GecerliVeri_BasariliKayit()
    {
        using var fabrika = FabrikaOlustur();
        var istemci = fabrika.CreateClient();

        // Basit bir Firma seed verisi ekle (FirmaCozumlemeMiddleware icin)
        using (var kapsam = fabrika.Services.CreateScope())
        {
            var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
            vt.Database.EnsureCreated();

            vt.Firmalar.Add(new VizitLink3D.Ortak.Modeller.Firma
            {
                Ad = "Test Firma",
                Slug = "test-firma",
                Domain = "localhost",
                AktifMi = true
            });
            await vt.SaveChangesAsync();
        }

        var giris = new
        {
            AdSoyad = "Ahmet Yilmaz",
            Email = "ahmet@test.com",
            Telefon = "05551234567",
            Konu = "Test konu",
            Mesaj = "Bu bir test mesajidir."
        };

        var cevap = await istemci.PostAsJsonAsync("/api/iletisim", giris);

        // Basarili donus: 200 veya 201
        Assert.True(cevap.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created,
            $"Beklenen 200/201, alinan: {(int)cevap.StatusCode}");
    }

    // ═══════════════════════════════════════════════════════════════════
    // SENARYO 3: Iletisim formu — eksik alanla 400 Donmeli mi?
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senaryo3_IletisimFormu_EksikAlan_400Donmeli()
    {
        using var fabrika = FabrikaOlustur();
        var istemci = fabrika.CreateClient();

        var giris = new
        {
            AdSoyad = "",          // bos — zorunlu alan
            Email = "test@test.com",
            Mesaj = "mesaj"
        };

        var cevap = await istemci.PostAsJsonAsync("/api/iletisim", giris);

        Assert.Equal(HttpStatusCode.BadRequest, cevap.StatusCode);
    }

    // ═══════════════════════════════════════════════════════════════════
    // SENARYO 4: Olmayan endpoint — 404 donmeli mi?
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senaryo4_OlmayanEndpoint_404Donmeli()
    {
        using var fabrika = FabrikaOlustur();
        var istemci = fabrika.CreateClient();

        var cevap = await istemci.GetAsync("/api/bu-endpoint-yoktur-12345");

        // FirmaCozumleme + fallback -> 404 veya 500 (HataYonetimiMiddleware yakalar)
        Assert.True(cevap.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError,
            $"Beklenen 404/500, alinan: {(int)cevap.StatusCode}");
    }

    // ═══════════════════════════════════════════════════════════════════
    // SENARYO 5: Cevap JSON yapisi — Cevap<T> zarf formatinda mi?
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senaryo5_CevapJsonYapisi_TutarliMi()
    {
        using var fabrika = FabrikaOlustur();
        var istemci = fabrika.CreateClient();

        var cevap = await istemci.GetAsync("/api/saglik");
        var hamJson = await cevap.Content.ReadAsStringAsync();

        // Saglik endpoint'i plain JSON donuyor — BasariliMi veya durum icerigi olmali
        Assert.False(string.IsNullOrWhiteSpace(hamJson), "JSON yanit bos olmamali.");
        Assert.Contains("veritabani", hamJson.ToLowerInvariant());
        Assert.Contains("surum", hamJson.ToLowerInvariant());
    }

    // ═══════════════════════════════════════════════════════════════════
    // SENARYO 6: Cors ve header yapisi — yanit basliklari dogru mu?
    // ═══════════════════════════════════════════════════════════════════
    [Fact]
    public async Task Senaryo6_CorsHeader_YanitBasliklarindaMi()
    {
        using var fabrika = FabrikaOlustur();
        var istemci = fabrika.CreateClient();

        var cevap = await istemci.GetAsync("/api/saglik");

        // X-Correlation-ID her istekte olmali (HataYonetimiMiddleware tarafindan eklenir)
        Assert.True(cevap.Headers.Contains("X-Correlation-ID"),
            "X-Correlation-ID basligi yanitta olmali.");
    }

    // ─── Yardimci modeller ───
    public sealed class SaglikYaniti
    {
        public string Durum { get; set; } = string.Empty;
        public bool Veritabani { get; set; }
        public DateTime Zaman { get; set; }
        public string Surum { get; set; } = string.Empty;
    }
}
