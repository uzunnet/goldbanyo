using MediatR;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;

/// <summary>
/// SuperAdmin model onay handler'ı.
/// Yalnız SuperAdmin rolüne sahip kullanıcılar model onaylayabilir.
/// Onaylanan model public konfigüratörde AdminOnayliMi=true filtresinden geçer.
/// </summary>
public class ModelOnaylaIsleyici(
    VizitLink3DDbContext vt,
    KiraciServisi kiraciServisi,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<ModelOnaylaKomutu, Cevap<bool>>
{
    public async Task<Cevap<bool>> Handle(
        ModelOnaylaKomutu istek,
        CancellationToken iptal)
    {
        var firmaId = kiraciServisi.MevcutFirmaId;
        if (firmaId is null or 0)
            return Cevap<bool>.Hata("Firma tanımlanamadı.");

        // SuperAdmin yetki kontrolü
        var kullanici = httpContextAccessor.HttpContext?.User;
        var rolClaim = kullanici?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (string.IsNullOrWhiteSpace(rolClaim) || rolClaim != "SuperAdmin")
            return Cevap<bool>.Hata("Bu işlem için SuperAdmin yetkisi gereklidir.");

        var kullaniciIdStr = kullanici?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? kullaniciId = int.TryParse(kullaniciIdStr, out var kid) ? kid : null;

        // Modeli bul
        var model = await vt.UrunUcBoyutModelleri
            .FirstOrDefaultAsync(m =>
                m.Id == istek.ModelId &&
                !m.SilindiMi,
                iptal);

        if (model is null)
            return Cevap<bool>.Hata("3D model bulunamadı.");

        // Modelin ait olduğu ürünün firmaya ait olduğunu doğrula (tenant izolasyonu)
        var urun = await vt.Urunler
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == model.UrunId &&
                u.FirmaId == firmaId,
                iptal);

        if (urun is null && firmaId > 0 && model.UrunId > 0)
            return Cevap<bool>.Hata("Model bu firmaya ait değil.");

        // Onayla
        model.AdminOnayliMi = true;
        model.OnayTarihi = DateTime.UtcNow;
        model.OnaylayanKullaniciId = kullaniciId;
        model.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync(iptal);

        return Cevap<bool>.Basarili(true, "Model onaylandı ve yayınlandı.");
    }
}
