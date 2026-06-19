using Desadoor.Api.Moduller.AI.Servisler;
using Desadoor.Api.VeriTabani;
using Microsoft.AspNetCore.SignalR;

namespace Desadoor.Api.Hubs;

public class AIHub : Hub
{
    private readonly AISaglayiciFabrikasi _fabrika;
    private readonly DesadoorDbContext _db;

    public AIHub(AISaglayiciFabrikasi fabrika, DesadoorDbContext db)
    {
        _fabrika = fabrika;
        _db = db;
    }

    public async IAsyncEnumerable<string> MetinUretStream(string prompt, string? sistemMesaji = null)
    {
        var saglayici = await _fabrika.SaglayiciGetirAsync(_db);
        if (saglayici == null)
        {
            yield return "HATA: Aktif AI sağlayıcı bulunamadı";
            yield break;
        }

        var istek = new AIIstek
        {
            KullaniciPrompt = prompt,
            SistemPrompt = sistemMesaji ?? "Sen profesyonel bir içerik üreticisisin."
        };

        await foreach (var parca in saglayici.MetinStreamAsync(istek, Context.ConnectionAborted))
        {
            yield return parca;
        }
    }
}
