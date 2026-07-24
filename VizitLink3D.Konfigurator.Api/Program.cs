using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using VizitLink3D.Konfigurator.Api.AraYazilimlar;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Modeller;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Servisler;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Servisler;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Servis kayitlari
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddScoped<BffGuvenlikFilter>();
        builder.Services.AddScoped<GlbDosyaServisi>();
        builder.Services.AddScoped<SifreSifirlamaServisi>();
        builder.Services.AddScoped<IEpostaGondermeServisi, EpostaGondermeServisi>();
        builder.Services.AddSingleton<IZamanlayici, SystemZamanlayici>();

        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(secenekler =>
            {
                secenekler.InvalidModelStateResponseFactory = context =>
                {
                    var hatalar = context.ModelState
                        .Where(kv => kv.Value?.Errors.Count > 0)
                        .SelectMany(kv => kv.Value!.Errors.Select(e => e.ErrorMessage))
                        .ToList();
                    var cevap = KonfiguratorCevap<object>.Hata("Dogrulama hatasi.", hatalar);
                    return new OkObjectResult(cevap);
                };
            });

        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<KonfiguratorDbContext>(secenekler =>
            secenekler.UseSqlite(builder.Configuration.GetConnectionString("KonfiguratorVeriTabani")));

        // Rate Limiting politikaları
        builder.Services.AddRateLimiter(secenekler =>
        {
            // Giriş endpoint için
            secenekler.AddFixedWindowLimiter("giris", yapilandirma =>
            {
                yapilandirma.PermitLimit = 5;
                yapilandirma.Window = TimeSpan.FromMinutes(1);
                yapilandirma.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                yapilandirma.QueueLimit = 0;
            });

            // Şifre sıfırlama isteği endpoint için
            secenekler.AddFixedWindowLimiter("sifre-sifirlama-istegi", yapilandirma =>
            {
                yapilandirma.PermitLimit = 3;
                yapilandirma.Window = TimeSpan.FromMinutes(15);
                yapilandirma.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                yapilandirma.QueueLimit = 0;
            });

            // Şifre yenileme endpoint için
            secenekler.AddFixedWindowLimiter("sifre-yenile", yapilandirma =>
            {
                yapilandirma.PermitLimit = 5;
                yapilandirma.Window = TimeSpan.FromMinutes(15);
                yapilandirma.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                yapilandirma.QueueLimit = 0;
            });

            // Model yükleme endpoint için (P03-A rate limit)
            secenekler.AddFixedWindowLimiter("modelyukleme", yapilandirma =>
            {
                yapilandirma.PermitLimit = 10;
                yapilandirma.Window = TimeSpan.FromMinutes(1);
                yapilandirma.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                yapilandirma.QueueLimit = 0;
            });

            // Yönetim endpointleri için (P05-A model yayın durumu)
            secenekler.AddFixedWindowLimiter("yonetim", yapilandirma =>
            {
                yapilandirma.PermitLimit = 30;
                yapilandirma.Window = TimeSpan.FromMinutes(1);
                yapilandirma.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                yapilandirma.QueueLimit = 0;
            });

            // Yönetim parça endpointleri için (P06-A)
            secenekler.AddFixedWindowLimiter("yonetim-parcalar", yapilandirma =>
            {
                yapilandirma.PermitLimit = 30;
                yapilandirma.Window = TimeSpan.FromMinutes(1);
                yapilandirma.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                yapilandirma.QueueLimit = 0;
            });

            secenekler.RejectionStatusCode = 429;
        });

        // P02-A: CORS yok — yalniz BFF (5114) server-to-server erisimi

        var app = builder.Build();

        // Migration tabanli sema olusturma + ilk yonetici bootstrap
        using (var kapsam = app.Services.CreateScope())
        {
            var db = kapsam.ServiceProvider.GetRequiredService<KonfiguratorDbContext>();
            await db.Database.MigrateAsync();

            var yapilandirma = kapsam.ServiceProvider.GetRequiredService<IConfiguration>();
            var kullaniciAdi = yapilandirma["IlkYonetici:KullaniciAdi"];
            var sifre = yapilandirma["IlkYonetici:Sifre"];

            if (!string.IsNullOrWhiteSpace(kullaniciAdi) && !string.IsNullOrWhiteSpace(sifre))
            {
                if (!await db.Kullanicilar.AnyAsync(k => k.KullaniciAdi == kullaniciAdi))
                {
                    // IlkYonetici:Eposta secret varsa ve mevcut placeholder @konfigurator.local ise
                    // gerçek e-posta ile güncelle (bootstrap)
                    var eposta = yapilandirma["IlkYonetici:Eposta"];
                    var varsayilanEposta = $"{kullaniciAdi}@konfigurator.local";

                    // Eğer IlkYonetici:Eposta gizli anahtarı doluysa, onu kullan
                    if (!string.IsNullOrWhiteSpace(eposta))
                    {
                        varsayilanEposta = eposta;
                    }

                    var yonetici = new KonfiguratorKullanicisi
                    {
                        KullaniciAdi = kullaniciAdi,
                        Eposta = varsayilanEposta,
                        SifreHash = BCrypt.Net.BCrypt.HashPassword(sifre),
                        Rol = "Yonetici",
                        AktifMi = true,
                        OlusturulmaTarihi = DateTime.UtcNow
                    };
                    db.Kullanicilar.Add(yonetici);
                    await db.SaveChangesAsync();
                }
            }
            else
            {
                var logger = kapsam.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("IlkYonetici yapilandirmasi eksik, atlaniyor.");
            }
        }

        // Medya statik dosyalari — sadece .glb izinli
        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path.StartsWithSegments("/medya") &&
                !ctx.Request.Path.Value!.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            await next();
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(builder.Environment.WebRootPath, "medya")),
            RequestPath = "/medya"
        });

        app.UseRateLimiter();
        app.MapControllers();

        await app.RunAsync();
    }
}
