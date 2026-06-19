using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Desadoor.Api.VeriTabani;
using Desadoor.Api.Modeller;
using Desadoor.Ortak.Modeller;

namespace Desadoor.Api.Kontrolcüler.Iletisim;

/// <summary>
/// Canlı sohbet mesajlarını ve oturumlarını yöneten API kontrolcüsü.
/// </summary>
[ApiController]
[Route("api/sohbet")]
public class SohbetKontrolcu : ControllerBase
{
    private readonly DesadoorDbContext _vt;

    public SohbetKontrolcu(DesadoorDbContext vt)
    {
        _vt = vt;
    }

    /// <summary>
    /// Tüm aktif sohbet oturumlarını gruplayarak getirir.
    /// </summary>
    [HttpGet("oturumlar")]
    public async Task<ActionResult<Cevap<List<object>>>> OturumlariGetir()
    {
        // Mesajları OturumId'ye göre gruplayıp her oturumun son mesajını alıyoruz
        var oturumlar = await _vt.CanliSohbetMesajlari
            .OrderByDescending(m => m.Tarih)
            .GroupBy(m => m.OturumId)
            .Select(g => new
            {
                OturumId = g.Key,
                Ad = g.First().GonderenAd,
                SonMesaj = g.First().MesajMetni,
                Tarih = g.First().Tarih,
                OkunmayanSayisi = g.Count(m => !m.OkunduMu && !m.YoneticiMi)
            })
            .ToListAsync();

        return Ok(new Cevap<List<object>>
        {
            Veri = oturumlar.Cast<object>().ToList(),
            BasariliMi = true,
            Mesaj = "Oturumlar başarıyla getirildi"
        });
    }

    /// <summary>
    /// Belirli bir oturuma ait tüm mesaj geçmişini getirir.
    /// </summary>
    [HttpGet("gecmis/{oturumId}")]
    public async Task<ActionResult<Cevap<List<CanliSohbetMesaji>>>> GecmisiGetir(string oturumId)
    {
        var mesajlar = await _vt.CanliSohbetMesajlari
            .Where(m => m.OturumId == oturumId)
            .OrderBy(m => m.Tarih)
            .ToListAsync();

        // Admin geçmişi okuduğu için okunmamış mesajları işaretliyoruz
        var okunmamislar = mesajlar.Where(m => !m.OkunduMu && !m.YoneticiMi).ToList();
        if (okunmamislar.Any())
        {
            okunmamislar.ForEach(m => m.OkunduMu = true);
            await _vt.SaveChangesAsync();
        }

        return Ok(new Cevap<List<CanliSohbetMesaji>>
        {
            Veri = mesajlar,
            BasariliMi = true,
            Mesaj = "Sohbet geçmişi başarıyla getirildi"
        });
    }
}

