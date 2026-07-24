extern alias KonfApi;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KonfApi::VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Testler.Moduller.Konfigurator;

public class KonfiguratorWebAppFactory : WebApplicationFactory<KonfApi::VizitLink3D.Konfigurator.Api.Program>
{
    private readonly SqliteConnection _baglanti;
    private readonly string _yoneticiKullaniciAdi;
    private readonly string _yoneticiSifre;

    public KonfiguratorWebAppFactory(string? yoneticiKullaniciAdi = null, string? yoneticiSifre = null)
    {
        _baglanti = new SqliteConnection("Data Source=:memory:");
        _baglanti.Open();
        _yoneticiKullaniciAdi = yoneticiKullaniciAdi ?? string.Empty;
        _yoneticiSifre = yoneticiSifre ?? string.Empty;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(servisler =>
        {
            var tanimlayici = servisler.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<KonfiguratorDbContext>));
            if (tanimlayici is not null)
                servisler.Remove(tanimlayici);

            servisler.AddDbContext<KonfiguratorDbContext>(secenekler =>
                secenekler.UseSqlite(_baglanti));
        });

        builder.UseSetting("IlkYonetici:KullaniciAdi", _yoneticiKullaniciAdi);
        builder.UseSetting("IlkYonetici:Sifre", _yoneticiSifre);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _baglanti.Dispose();
    }
}
