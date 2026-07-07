using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Iletisim;

/// <summary>
/// IletisimKontrolcu � �leti�im formu mesajlar�n� y�neten kontrolc�d�r.
/// POST /api/iletisim � Yeni mesaj veritaban�na kaydeder (herkes eri�ebilir).
/// GET /api/iletisim/mesajlar � Admin: t�m mesajlar� listeler (yetki gerekir).
/// PATCH /api/iletisim/mesajlar/{id}/okundu � Admin: mesaj� okundu i�aretler.
/// DELETE /api/iletisim/mesajlar/{id} � Admin: mesaj� siler.
/// </summary>
[ApiController]
[Route("api/iletisim")]
public class IletisimKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    // ��� Yeni Mesaj Kaydet (Herkese a��k) �����������������������������������
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

        return Ok(new { BasariliMi = true, Mesaj = "Mesaj�n�z al�nd�. En k�sa s�rede d�n�� yapaca��z." });
    }

    // ��� T�m Mesajlar� Listele (Admin) ���������������������������������������
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

    // ��� Mesaj� Okundu ��aretle (Admin) �������������������������������������
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

    // ��� Mesaj� Ar�ivle / Sil (Admin) ����������������������������������������
    [HttpDelete("mesajlar/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> MesajiSil(int id)
    {
        var mesaj = await vt.IletisimMesajlari.FindAsync(id);
        if (mesaj is null) return NotFound();
        mesaj.CevaplandiMi = true; // Ar�ivleme yerine Cevapland� i�aretle
        await vt.SaveChangesAsync();
        return Ok(new { BasariliMi = true });
    }
}

/// <summary>
/// IletisimMesajiGiris � Kullan�c�n�n ileti�im formunda doldurdu�u alanlar� temsil eden DTO.
/// </summary>
public record IletisimMesajiGiris(
    string AdSoyad,
    string Email,
    string? Telefon,
    string? Konu,
    string Mesaj
);




