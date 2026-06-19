using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Desadoor.Api.VeriTabani;
using Desadoor.Ortak.Modeller;

namespace Desadoor.Api.Kontrolcüler.Sistem;

[ApiController]
[Route("api/desadoor/tema")]
public class TemaKontrolcu(DesadoorDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var temaAyarlari = await db.SistemAyarlari
            .Where(a => a.Anahtar.StartsWith("tema.") || a.Anahtar.StartsWith("gorunum."))
            .ToListAsync();

        var sozluk = temaAyarlari.ToDictionary(a => a.Anahtar, a => a.Deger);

        return Ok(new Cevap<object>
        {
            BasariliMi = true,
            Veri = new
            {
                BirincilRenk = sozluk.GetValueOrDefault("tema.birincilRenk", "#1A1A27"),
                IkincilRenk = sozluk.GetValueOrDefault("tema.ikincilRenk", "#C8952A"),
                VurguRengi = sozluk.GetValueOrDefault("tema.vurguRengi", "#8B4543"),
                ArkaPlanRengi = sozluk.GetValueOrDefault("tema.arkaPlanRengi", "#F5F2ED"),
                KoyuTemaMi = sozluk.GetValueOrDefault("gorunum.koyuTema", "false") == "true",
                YuvarlakKoseler = sozluk.GetValueOrDefault("gorunum.yuvarlakKoseler", "false") == "true",
                Glassmorphism = sozluk.GetValueOrDefault("gorunum.glassmorphism", "true") == "true"
            }
        });
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Kaydet([FromBody] Dictionary<string, string> ayarlar)
    {
        foreach (var (anahtar, deger) in ayarlar)
        {
            var mevcut = await db.SistemAyarlari.FirstOrDefaultAsync(a => a.Anahtar == anahtar);
            if (mevcut != null)
            {
                mevcut.Deger = deger;
                mevcut.GuncellenmeTarihi = DateTime.UtcNow;
            }
            else
            {
                db.SistemAyarlari.Add(new SistemAyari
                {
                    Anahtar = anahtar,
                    Deger = deger,
                    Tip = "string",
                    OlusturulmaTarihi = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
        return Ok(new Cevap<object> { BasariliMi = true, Mesaj = "Tema ayarları kaydedildi" });
    }
}
