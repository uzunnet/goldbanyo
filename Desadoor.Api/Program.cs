using Desadoor.Api.VeriTabani;
using Desadoor.Api.AraYazilimlar;
using Desadoor.Api.Servisler;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Desadoor.Api.Hubs;

// Serilog yapilandirmasi (anayasa §15)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/desadoor-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var yapici = WebApplication.CreateBuilder(args);
yapici.Host.UseSerilog();

// Veritabani
var vtYolu = yapici.Configuration["VeriTabani:Yol"] ?? "desadoor.db";
yapici.Services.AddHttpContextAccessor();
yapici.Services.AddDbContext<DesadoorDbContext>((sp, sec) =>
{
    var httpErisimi = sp.GetService<IHttpContextAccessor>();
    sec.UseSqlite($"Data Source={vtYolu}")
       .AddInterceptors(new AuditInterceptor(httpErisimi));
});

// JWT Kimlik Dogrulama — anahtar ortam degiskeninden, yoksa config'den
var jwtAnahtar = Environment.GetEnvironmentVariable("DESADOOR_JWT_KEY")
    ?? yapici.Configuration["Jwt:Anahtar"]!;
yapici.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(sec =>
    {
        sec.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAnahtar)),
            ValidateIssuer = true,
            ValidIssuer = yapici.Configuration["Jwt:Yayinci"],
            ValidateAudience = true,
            ValidAudience = yapici.Configuration["Jwt:Izleyici"],
            ValidateLifetime = true
        };
    });

yapici.Services.AddAuthorization();
yapici.Services.AddSignalR();

// Rate Limiting (anayasa §23.5)
yapici.Services.RateLimitingEkle();

// CORS
yapici.Services.AddCors(sec => sec.AddDefaultPolicy(politika =>
    politika.WithOrigins("http://localhost:5013", "https://localhost:5013", "http://localhost:5015", "https://localhost:5015", "http://127.0.0.1:5013", "http://127.0.0.1:5015")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

yapici.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
yapici.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
yapici.Services.AddControllers();
yapici.Services.AddOpenApi();

// Onbellek ve Ceviri servisleri (anayasa §35)
yapici.Services.AddMemoryCache();
yapici.Services.AddSingleton<IOnbellekYonetici, OnbellekYonetici>();
yapici.Services.AddScoped<ICeviriServisi, CeviriServisi>();
yapici.Services.AddScoped<IOtomatikCeviriServisi, OtomatikCeviriServisi>();

// Medya Havuzu servisleri
yapici.Services.AddScoped<Desadoor.Api.Moduller.Medya.Servisler.IDepolamaAdaptoru, Desadoor.Api.Moduller.Medya.Servisler.YerelDepolama>();
yapici.Services.AddScoped<Desadoor.Api.Moduller.Medya.Servisler.IResimIslemcisi, Desadoor.Api.Moduller.Medya.Servisler.ResimIslemcisi>();
yapici.Services.AddScoped<Desadoor.Api.Moduller.Medya.Servisler.IYoutubeMetadataServisi, Desadoor.Api.Moduller.Medya.Servisler.YoutubeMetadataServisi>();
yapici.Services.AddScoped<Desadoor.Api.Moduller.Medya.Servisler.IMedyaServisi, Desadoor.Api.Moduller.Medya.Servisler.MedyaServisi>();

// API Key Sifreleme ve PII Filtre
yapici.Services.AddDataProtection();
yapici.Services.AddSingleton<IApiKeySifrelemeServisi, ApiKeySifrelemeServisi>();
yapici.Services.AddSingleton<IPIIFiltreServisi, PIIFiltreServisi>();

// AI Asistan servisleri
yapici.Services.AddHttpClient();
yapici.Services.AddScoped<Desadoor.Api.Moduller.AI.Servisler.AIGuvenlikServisi>();
yapici.Services.AddScoped<Desadoor.Api.Moduller.AI.Servisler.AISaglayiciFabrikasi>();
yapici.Services.AddScoped<Desadoor.Api.Moduller.AI.Servisler.IAIMaliyetTakipServisi, Desadoor.Api.Moduller.AI.Servisler.AIMaliyetTakipServisi>();

// Kapak → Urun goc servisi
yapici.Services.AddScoped<KapakGocServisi>();
yapici.Services.AddScoped<UrunMedyaBaglamaServisi>();
yapici.Services.AddScoped<PdfCozumlemeServisi>();
yapici.Services.AddScoped<PdfOnizlemeServisi>();
yapici.Services.AddScoped<PdfUygulamaAlanServisi>();
yapici.Services.AddScoped<MedyaGocServisi>();

// Multi-tenant servisleri
yapici.Services.AddScoped<KiraciServisi>();
yapici.Services.AddScoped<Desadoor.Api.Moduller.Tema.Servisler.StitchTemaServisi>();

// Lisans servisi
yapici.Services.AddScoped<Desadoor.Api.Servisler.Kimlik.LisansServisi>();

var uygulama = yapici.Build();

// Veritabanini otomatik olustur ve baslangic verilerini tohumla
using (var kapsam = uygulama.Services.CreateScope())
{
    var vt = kapsam.ServiceProvider.GetRequiredService<DesadoorDbContext>();
    var webEnv = kapsam.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    // Mevcut DB'de SayfaDuzenAyarlari tablosunu kontrol et ve varsa olustur
    var conn = vt.Database.GetDbConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='SayfaDuzenAyarlari'";
    var tabloVar = await cmd.ExecuteScalarAsync();
    if (tabloVar == null)
    {
        using var createCmd = conn.CreateCommand();
        createCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS SayfaDuzenAyarlari (
            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            SayfaKodu TEXT NOT NULL,
            SayfaAdi TEXT NOT NULL,
            SutunAdet INTEGER NOT NULL DEFAULT 4,
            SatirAdet INTEGER NOT NULL DEFAULT 3,
            SayfaBasinaAdet INTEGER NOT NULL DEFAULT 12,
            SayfalamaAktif INTEGER NOT NULL DEFAULT 1,
            AktifMi INTEGER NOT NULL DEFAULT 1,
            SilindiMi INTEGER NOT NULL DEFAULT 0,
            OlusturulmaTarihi TEXT NOT NULL,
            GuncellenmeTarihi TEXT NULL
        )";
        await createCmd.ExecuteNonQueryAsync();
    }
    await conn.CloseAsync();
    await TohumVerisi.TohumlaAsync(vt);
    await TohumVerisi.TemizleSlaytResimlerAsync(vt, webEnv.WebRootPath);

    // Kapak → Urun gocu (idempotent — tekrar calisinca cogaltmaz)
    var goc = kapsam.ServiceProvider.GetRequiredService<KapakGocServisi>();
    var gocSonuc = await goc.GogAsync();
    Log.Information("Kapak gocu: {Gocen} goc etti, {Atlandi} atlandi", gocSonuc.Veri?.Gocen, gocSonuc.Veri?.Atlandi);

    // NRD gorsel + 3D model baglama (idempotent — wwwroot/medya'daki standart dosyalari urunlere baglar)
    var medyaBaglama = kapsam.ServiceProvider.GetRequiredService<UrunMedyaBaglamaServisi>();
    var baglamaSonuc = await medyaBaglama.BaglaAsync();
    Log.Information("Urun medya baglama: {Urun} urun, {Gorsel} gorsel, {Model} 3D model",
        baglamaSonuc.Veri?.GuncellenenUrun, baglamaSonuc.Veri?.BaglananGorsel, baglamaSonuc.Veri?.BaglananModel);
}

if (uygulama.Environment.IsDevelopment())
    uygulama.MapOpenApi();

// ==================== MIDDLEWARE SIRASI ====================
// 1. Hata yonetimi (en dis katman)
uygulama.UseMiddleware<HataYonetimiMiddleware>();

// 2. Firma cozumleme (multi-tenant)
uygulama.UseMiddleware<FirmaCozumlemeMiddleware>();

// 3. Lisans dogrulama
uygulama.UseMiddleware<LisansDogrulamaMiddleware>();

// 3. Guvenlik header'lari
uygulama.UseMiddleware<GuvenlikHeaderlariMiddleware>();

// 4. Rate limiting
uygulama.UseRateLimiter();

// 4. CORS
uygulama.UseCors();

// 5. Statik dosyalar (3D modeller icin GLB/GLTF MIME)
var saglayici = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
saglayici.Mappings[".glb"] = "model/gltf-binary";
saglayici.Mappings[".gltf"] = "model/gltf+json";
saglayici.Mappings[".wasm"] = "application/wasm";
saglayici.Mappings[".dll"] = "application/octet-stream";
saglayici.Mappings[".pdb"] = "application/octet-stream";
saglayici.Mappings[".dat"] = "application/octet-stream";
saglayici.Mappings[".js"] = "text/javascript";
saglayici.Mappings[".mjs"] = "text/javascript";
saglayici.Mappings[".css"] = "text/css";
saglayici.Mappings[".map"] = "application/json";
saglayici.Mappings[".br"] = "application/octet-stream";

uygulama.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = saglayici,
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

uygulama.ResimOptimizasyonKullan();

// 6. Kimlik dogrulama
uygulama.UseAuthentication();

// 7. Yetkilendirme
uygulama.UseAuthorization();

// 8. API Controller'lari
uygulama.MapControllers();

// 9. SignalR Hub
uygulama.MapHub<SohbetHub>("/hubs/sohbet");
uygulama.MapHub<BildirimHub>("/hubs/bildirim");
uygulama.MapHub<AIHub>("/hubs/ai");
uygulama.MapHub<SahneAyarHub>("/hubs/sahne-ayar");
uygulama.MapHub<TemaHub>("/hubs/tema");

// 10. Blazor WASM fallback
uygulama.MapFallbackToFile("index.html");

uygulama.Run();

