using Desadoor.Api.Modeller;
using Desadoor.Api.VeriTabani;
using Desadoor.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Desadoor.Api.Kontrolcüler.Iletisim;

/// <summary>
/// IletisimKontrolcu — İletişim formu mesajlarını yöneten kontrolcüdür.
/// POST /api/desadoor/iletisim → Yeni mesaj veritabanına kaydeder (herkes erişebilir).
/// GET /api/desadoor/iletisim/mesajlar → Admin: tüm mesajları listeler (yetki gerekir).
/// PATCH /api/desadoor/iletisim/mesajlar/{id}/okundu → Admin: mesajı okundu işaretler.
/// DELETE /api/desadoor/iletisim/mesajlar/{id} → Admin: mesajı siler.
/// </summary>
[ApiController]
[Route("api/desadoor/iletisim")]
public class IletisimKontrolcu(DesadoorDbContext vt) : ControllerBase
{
    // ─── Yeni Mesaj Kaydet (Herkese açık) ───────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> MesajKaydet([FromBody] IletisimMesajiGiris giris)
    {
        if (string.IsNullOrWhiteSpace(giris.AdSoyad) || string.IsNullOrWhiteSpace(giris.Email) || string.IsNullOrWhiteSpace(giris.Mesaj))
            return BadRequest(new { BasariliMi = false, Mesaj = "Zorunlu alanlar eksik." });

        var yeniMesaj = new IletisimMesaji
        {
            AdSoyad = giris.AdSoyad.Trim(),
            Eposta = giris.Email.Trim(),
            Telefon = giris.Telefon?.Trim() ?? string.Empty,
            Konu = giris.Konu?.Trim() ?? string.Empty,
            Mesaj = giris.Mesaj.Trim(),
            Tarih = DateTime.UtcNow,
            OkunduMu = false
        };

        vt.IletisimMesajlari.Add(yeniMesaj);
        await vt.SaveChangesAsync();

        return Ok(new { BasariliMi = true, Mesaj = "Mesajınız alındı. En kısa sürede dönüş yapacağız." });
    }

    // ─── Tüm Mesajları Listele (Admin) ───────────────────────────────────────
    [HttpGet("mesajlar")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> MesajlariniGetir([FromQuery] bool? okundu = null, [FromQuery] int sayfa = 1, [FromQuery] int sayfaBoyutu = 20)
    {
        var sorgu = vt.IletisimMesajlari
            .Where(m => !m.CevaplandiMi)
            .AsQueryable();

        if (okundu.HasValue)
            sorgu = sorgu.Where(m => m.OkunduMu == okundu.Value);

        var toplam = await sorgu.CountAsync();
        var mesajlar = await sorgu
            .OrderByDescending(m => m.Tarih)
            .Skip((sayfa - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .Select(m => new
            {
                m.Id, m.AdSoyad, Eposta = m.Eposta, m.Telefon, m.Konu,
                m.Mesaj, Tarih = m.Tarih, m.OkunduMu
            })
            .ToListAsync();

        return Ok(new { BasariliMi = true, Veri = new { Toplam = toplam, Mesajlar = mesajlar } });
    }

    // ─── Mesajı Okundu İşaretle (Admin) ─────────────────────────────────────
    [HttpPatch("mesajlar/{id:int}/okundu")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> OkunduIsaretle(int id)
    {
        var mesaj = await vt.IletisimMesajlari.FindAsync(id);
        if (mesaj is null) return NotFound();
        mesaj.OkunduMu = true;
        await vt.SaveChangesAsync();
        return Ok(new { BasariliMi = true });
    }

    // ─── Mesajı Arşivle / Sil (Admin) ────────────────────────────────────────
    [HttpDelete("mesajlar/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> MesajiSil(int id)
    {
        var mesaj = await vt.IletisimMesajlari.FindAsync(id);
        if (mesaj is null) return NotFound();
        mesaj.CevaplandiMi = true; // Arşivleme yerine Cevaplandı işaretle
        await vt.SaveChangesAsync();
        return Ok(new { BasariliMi = true });
    }
}

/// <summary>
/// IletisimMesajiGiris — Kullanıcının iletişim formunda doldurduğu alanları temsil eden DTO.
/// </summary>
public record IletisimMesajiGiris(
    string AdSoyad,
    string Email,
    string? Telefon,
    string? Konu,
    string Mesaj
);

