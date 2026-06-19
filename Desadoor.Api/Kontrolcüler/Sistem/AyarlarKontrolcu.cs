using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Desadoor.Api.VeriTabani;
using Desadoor.Ortak.Modeller;

namespace Desadoor.Api.Kontrolcüler.Sistem;

[ApiController]
[Route("api/desadoor/ayarlar")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AyarlarKontrolcu(DesadoorDbContext db) : ControllerBase
{
    private const string GOOGLE_ANALYTICS_ANAHTARI = "api.GoogleAnalyticsId";
    private const string FACEBOOK_PIXEL_ANAHTARI = "api.FacebookPixelId";
    private const string GOOGLE_TAG_MANAGER_ANAHTARI = "api.GoogleTagManagerId";
    private const string GEMINI_API_ANAHTARI = "ai.GeminiApiKey";
    private const string OPENAI_API_ANAHTARI = "ai.OpenAiApiKey";

    [HttpGet("api-entegrasyon")]
    public async Task<IActionResult> GetApiEntegrasyon()
    {
        var ayarlar = await db.SistemAyarlari
            .Where(a => a.Anahtar.StartsWith("api.") || a.Anahtar.StartsWith("ai."))
            .ToListAsync();

        return Ok(new Cevap<List<SistemAyari>> { BasariliMi = true, Veri = ayarlar });
    }

    [HttpGet("{grup}")]
    public async Task<IActionResult> GrupGetir(string grup)
    {
        var ayarlar = await db.SistemAyarlari
            .Where(a => a.Anahtar.StartsWith(grup + "."))
            .ToListAsync();

        return Ok(new Cevap<List<SistemAyari>> { BasariliMi = true, Veri = ayarlar });
    }

    [HttpPost]
    public async Task<IActionResult> Kaydet([FromBody] List<SistemAyari> ayarlar)
    {
        foreach (var ayar in ayarlar)
        {
            var mevcut = await db.SistemAyarlari.FirstOrDefaultAsync(a => a.Anahtar == ayar.Anahtar);
            if (mevcut != null)
            {
                mevcut.Deger = ayar.Deger;
                mevcut.GuncellenmeTarihi = DateTime.UtcNow;
            }
            else
            {
                ayar.OlusturulmaTarihi = DateTime.UtcNow;
                db.SistemAyarlari.Add(ayar);
            }
        }

        await db.SaveChangesAsync();
        return Ok(new Cevap<object> { BasariliMi = true, Mesaj = "Ayarlar kaydedildi" });
    }

    [HttpPost("analytics")]
    public async Task<IActionResult> AnalyticsKaydet([FromBody] AnalyticsAyarIstegi istek)
    {
        await AyarKaydetAsync(GOOGLE_ANALYTICS_ANAHTARI, istek.GoogleAnalyticsId);
        await AyarKaydetAsync(FACEBOOK_PIXEL_ANAHTARI, istek.FacebookPixelId);
        await AyarKaydetAsync(GOOGLE_TAG_MANAGER_ANAHTARI, istek.GoogleTagManagerId);

        await db.SaveChangesAsync();
        return Ok(new Cevap<object> { BasariliMi = true, Mesaj = "Analytics ayarlari kaydedildi." });
    }

    [HttpPost("ai")]
    public async Task<IActionResult> AiKaydet([FromBody] AiAyarIstegi istek)
    {
        await AyarKaydetAsync(GEMINI_API_ANAHTARI, istek.GeminiApiKey);
        await AyarKaydetAsync(OPENAI_API_ANAHTARI, istek.OpenAiApiKey);

        await db.SaveChangesAsync();
        return Ok(new Cevap<object> { BasariliMi = true, Mesaj = "Yapay zeka ayarlari kaydedildi." });
    }

    [HttpGet("ai/test")]
    public async Task<IActionResult> AiTest()
    {
        var anahtarVarMi = await db.SistemAyarlari.AnyAsync(a =>
            (a.Anahtar == GEMINI_API_ANAHTARI || a.Anahtar == OPENAI_API_ANAHTARI)
            && !string.IsNullOrWhiteSpace(a.Deger));

        return Ok(new Cevap<bool>
        {
            BasariliMi = true,
            Veri = anahtarVarMi,
            Mesaj = anahtarVarMi ? "AI anahtari kayitli." : "AI anahtari bulunamadi."
        });
    }

    private async Task AyarKaydetAsync(string anahtar, string? deger)
    {
        var mevcut = await db.SistemAyarlari.FirstOrDefaultAsync(a => a.Anahtar == anahtar);
        if (mevcut is null)
        {
            db.SistemAyarlari.Add(new SistemAyari
            {
                Anahtar = anahtar,
                Deger = deger ?? string.Empty,
                Tip = "string",
                OlusturulmaTarihi = DateTime.UtcNow
            });
            return;
        }

        mevcut.Deger = deger ?? string.Empty;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;
    }

    public sealed record AnalyticsAyarIstegi(string? GoogleAnalyticsId, string? FacebookPixelId, string? GoogleTagManagerId);

    public sealed record AiAyarIstegi(string? GeminiApiKey, string? OpenAiApiKey);
}
