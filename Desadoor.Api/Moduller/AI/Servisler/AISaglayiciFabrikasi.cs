using Desadoor.Api.Servisler;
using Desadoor.Api.VeriTabani;
using Desadoor.Ortak.Modeller.AI;
using Microsoft.EntityFrameworkCore;

namespace Desadoor.Api.Moduller.AI.Servisler;

public class AISaglayiciFabrikasi
{
    private readonly IServiceProvider _sp;
    private readonly IApiKeySifrelemeServisi _sifrelemeServisi;

    public AISaglayiciFabrikasi(IServiceProvider sp, IApiKeySifrelemeServisi sifrelemeServisi)
    {
        _sp = sp;
        _sifrelemeServisi = sifrelemeServisi;
    }

    public IAISaglayici SaglayiciOlustur(AISaglayicisi saglayici, HttpClient http)
    {
        var apiKey = _sifrelemeServisi.Coz(saglayici.ApiKeyEncrypted) ?? "";

        return saglayici.Tip switch
        {
            AISaglayiciTipi.OpenAI => new OpenAISaglayici(apiKey, http),
            AISaglayiciTipi.Anthropic => new AnthropicSaglayici(apiKey, http),
            AISaglayiciTipi.Gemini => new GeminiSaglayici(apiKey, http),
            AISaglayiciTipi.LlamaLocal => new LlamaLocalSaglayici(apiKey, http),
            AISaglayiciTipi.GoogleTranslate => new GoogleTranslateSaglayici(apiKey, http),
            _ => new OpenAISaglayici(apiKey, http)
        };
    }

    public async Task<IAISaglayici?> SaglayiciGetirAsync(DesadoorDbContext db, AISaglayiciTipi? tip = null, HttpClient? http = null)
    {
        var entity = tip.HasValue
            ? await db.AISaglayicilari.FirstOrDefaultAsync(s => s.AktifMi && s.Tip == tip.Value)
            : await db.AISaglayicilari.FirstOrDefaultAsync(s => s.AktifMi);

        if (entity == null) return null;

        var client = http ?? new HttpClient();
        return SaglayiciOlustur(entity, client);
    }
}
