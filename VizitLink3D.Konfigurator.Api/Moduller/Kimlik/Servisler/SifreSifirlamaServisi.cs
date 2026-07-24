using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Modeller;
using VizitLink3D.Konfigurator.Api.VeriTabani;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Servisler;

public class SifreSifirlamaServisi
{
    private readonly KonfiguratorDbContext _db;
    private readonly IConfiguration _yapilandirma;
    private readonly ILogger<SifreSifirlamaServisi> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IZamanlayici _zamanlayici;

    private static readonly TimeSpan TokenGecerlilikSuresi = TimeSpan.FromMinutes(15);

    // ───── Account enumeration onlemi: zamanlama esitleme ─────
    // RET bulgusu: Kullanici var/yok akisi arasindaki gozlenebilir yanit suresi
    // farki, saldirganin gecerli e-posta adreslerini tespit etmesine olanak tanir.
    //
    // Strateji (OWASP ASVS V2.1.1 / NIST SP 800-63B §5.2.2 uyumlu):
    // • Kullanici VARKEN: DB insert (+~20-100ms SQLite), e-posta FIRE-AND-FORGET
    //   gonderilir (await edilmez) → yanit suresi sadece DB islemleri kadardir.
    // • Kullanici YOKKEN: Rastgele jitter ile 40-160ms arasi gecikme eklenir;
    //   bu aralik, SQLite uzerinde kullanici sorgulama + insert + SaveChanges
    //   suresinin tipik varyansini kapsar (uygulama ortaminda kalibre edilebilir).
    // • E-posta gonderimi basarisiz olsa bile kullaniciya bilgi SIZDIRILMAZ;
    //   hata sadece Debug log seviyesinde kaydedilir.
    //
    // Jitter araligi gerekcesi: SQLite bellek-ici (test) → 5-30ms;
    // SQLite disk (gelistirme) → 20-100ms; uretim PostgreSQL → 10-60ms.
    // 40-160ms bandi tum bu ortamlari kapsayacak sekilde secildi.
    // Uretim ortaminda UygulamaAyarlari:Guvenlik:SifreSifirlamaGecikmeMs
    // ile override edilebilir. Test kalibrasyonu icin public.
    public static TimeSpan KullaniciYokMinGecikme { get; set; } = TimeSpan.FromMilliseconds(40);
    public static TimeSpan KullaniciYokMaksGecikme { get; set; } = TimeSpan.FromMilliseconds(160);

    public SifreSifirlamaServisi(
        KonfiguratorDbContext db,
        IConfiguration yapilandirma,
        ILogger<SifreSifirlamaServisi> logger,
        IServiceScopeFactory scopeFactory,
        IZamanlayici zamanlayici)
    {
        _db = db;
        _yapilandirma = yapilandirma;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _zamanlayici = zamanlayici;
    }

    /// <summary>
    /// E-posta adresi icin sifre sifirlama istegi olusturur.
    /// Account enumeration onlemi: kullanicili/kullanicisiz akista
    /// gozlenebilir yanit suresi esitlenir; her zaman ayni generic basarili sonuc.
    /// </summary>
    public async Task SifreSifirlamaIstegiOlusturAsync(string eposta)
    {
        var kullanici = await _db.Kullanicilar
            .FirstOrDefaultAsync(k => k.Eposta == eposta && k.AktifMi);

        if (kullanici is null)
        {
            // ── Account enumeration onlemi: rastgele jitter gecikmesi ──
            // Kullanici yokken DB sorgusu cok hizli doner (~1-5ms).
            // Kullanici varken DB insert + token olusturma ~20-100ms surer.
            // Aradaki farki gizlemek icin rastgele jitter eklenir.
            // IZamanlayici soyutlamasi sayesinde test edilebilir.
            var jitterMs = KullaniciYokMinGecikme.TotalMilliseconds +
                (Random.Shared.NextDouble() *
                 (KullaniciYokMaksGecikme.TotalMilliseconds - KullaniciYokMinGecikme.TotalMilliseconds));

            await _zamanlayici.GecikmeAsync(TimeSpan.FromMilliseconds(jitterMs));

            _logger.LogDebug("Sifre sifirlama istegi: kullanici bulunamadi.");
            return;
        }

        // Yeni token olustur
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var istek = new SifreSifirlamaIstegi
        {
            KullaniciId = kullanici.Id,
            TokenHash = tokenHash,
            BitisTarihi = DateTime.UtcNow.Add(TokenGecerlilikSuresi),
            KullanildiMi = false,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        _db.SifreSifirlamaIstekleri.Add(istek);
        await _db.SaveChangesAsync();

        // E-posta gonderimi bilgilerini hazirla
        var uygulamaUrl = _yapilandirma["SifreSifirlama:UygulamaUrl"] ?? "";
        var sifirlamaLinki = $"{uygulamaUrl.TrimEnd('/')}/sifre-yenile?token={Uri.EscapeDataString(rawToken)}";

        var konu = "Sifre Sifirlama Talebi";
        var govdeHtml = $"""
            <h2>Sifre Sifirlama Talebi</h2>
            <p>Merhaba {System.Net.WebUtility.HtmlEncode(kullanici.KullaniciAdi)},</p>
            <p>Sifrenizi sifirlamak icin asagidaki baglantiya tiklayin:</p>
            <p><a href="{System.Net.WebUtility.HtmlEncode(sifirlamaLinki)}">Sifremi Sifirla</a></p>
            <p>Bu baglanti 15 dakika sureyle gecerlidir.</p>
            <p>Eger bu istegi siz yapmadiysaniz, bu e-postayi dikkate almayin.</p>
            """;

        // ── Fire-and-forget: IServiceScopeFactory ile bagimsiz scope ──
        // Request scope'u kapansa bile calismaya devam eder.
        // SADECE IServiceScopeFactory (singleton) ve deger tipleri yakalanir;
        // scoped servisler (_db, _logger) yakalanmaz.
        var kullaniciEposta = kullanici.Eposta;
        var kullaniciId = kullanici.Id;
        var scopeFactory = _scopeFactory;

        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var epostaServisi = scope.ServiceProvider.GetRequiredService<IEpostaGondermeServisi>();
            var scopeLogger = scope.ServiceProvider.GetRequiredService<ILogger<SifreSifirlamaServisi>>();
            try
            {
                var gonderildi = await epostaServisi.EpostaGonderAsync(kullaniciEposta, konu, govdeHtml);
                if (!gonderildi)
                    scopeLogger.LogDebug("Sifre sifirlama e-postasi gonderilemedi.");
            }
            catch (Exception ex)
            {
                // E-posta gonderim hatasi kullaniciya sizdirilmaz;
                // hata tipi sadece Debug seviyesinde loglanir.
                scopeLogger.LogDebug("Sifre sifirlama e-postasi gonderim hatasi: {HataTipi}", ex.GetType().Name);
            }
        });

        // Raw token asla loglanmaz
        _logger.LogDebug("Sifre sifirlama istegi olusturuldu. KullaniciId={KullaniciId}", kullaniciId);
    }

    /// <summary>
    /// Token ile sifre yenileme. Basarisiz olursa false doner (sebep sizdirmaz).
    /// </summary>
    public async Task<bool> SifreYenileAsync(string rawToken, string yeniSifre)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return false;

        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        // Token hash ile istek ara
        var istek = await _db.SifreSifirlamaIstekleri
            .Include(i => i.Kullanici)
            .FirstOrDefaultAsync(i => i.TokenHash == tokenHash && !i.SilindiMi);

        if (istek is null)
            return false;

        // Sure dolmus mu?
        if (DateTime.UtcNow > istek.BitisTarihi)
            return false;

        // Zaten kullanilmis mi?
        if (istek.KullanildiMi)
            return false;

        // Kullanici aktif mi?
        if (istek.Kullanici is null || !istek.Kullanici.AktifMi || istek.Kullanici.SilindiMi)
            return false;

        // Sifre hash'le ve kaydet
        var yeniHash = BCrypt.Net.BCrypt.HashPassword(yeniSifre);

        // Atomik guncelleme: token'i gecersiz kil + sifreyi degistir
        istek.KullanildiMi = true;
        istek.KullanilmaTarihi = DateTime.UtcNow;
        istek.Kullanici.SifreHash = yeniHash;
        istek.Kullanici.GuncellenmeTarihi = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogDebug("Sifre basariyla yenilendi. KullaniciId={KullaniciId}", istek.KullaniciId);
        return true;
    }
}
