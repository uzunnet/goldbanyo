using Desadoor.Api.Moduller.AI.Servisler;
using Desadoor.Api.Servisler;
using Desadoor.Api.VeriTabani;
using Desadoor.Ortak.Modeller;
using Desadoor.Ortak.Modeller.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Desadoor.Api.Moduller.AI.Kontrolcu;

[ApiController]
[Route("api/ai")]
public class AIKontrolcu : ControllerBase
{
    private readonly AISaglayiciFabrikasi _fabrika;
    private readonly IAIMaliyetTakipServisi _maliyetTakip;
    private readonly IPIIFiltreServisi _piiServisi;
    private readonly DesadoorDbContext _db;

    public AIKontrolcu(AISaglayiciFabrikasi fabrika, IAIMaliyetTakipServisi maliyetTakip, IPIIFiltreServisi piiServisi, DesadoorDbContext db)
    {
        _fabrika = fabrika;
        _maliyetTakip = maliyetTakip;
        _piiServisi = piiServisi;
        _db = db;
    }

    [HttpPost("yaz")]
    public async Task<Cevap<string?>> AIYaz([FromBody] AIYazIstegi istek)
    {
        var saglayici = await _fabrika.SaglayiciGetirAsync(_db, istek.SaglayiciTip);
        if (saglayici == null)
            return Cevap<string?>.Hata("Aktif AI sağlayıcısı bulunamadı.");

        var saglayiciEntity = await _db.AISaglayicilari.FirstOrDefaultAsync(s => s.AktifMi);
        if (saglayiciEntity != null)
        {
            var limitVar = await _maliyetTakip.LimitKontrolAsync(saglayiciEntity.Id, null);
            if (!limitVar) return Cevap<string?>.Hata("Aylık AI kullanım limiti doldu.");
        }

        var guvenliPrompt = _piiServisi.Filtrele(istek.Prompt);

        var yanit = await saglayici.MetinUretAsync(new AIIstek
        {
            KullaniciPrompt = guvenliPrompt,
            SistemPrompt = istek.SistemPrompt ?? "Sen DesaDoor kapı ve mobilya sektöründe uzman bir asistansın. Türkçe yanıt ver.",
            Model = saglayiciEntity?.Model ?? "llama3.2-3b"
        }, HttpContext.RequestAborted);

        if (saglayiciEntity != null)
        {
            await _maliyetTakip.KaydetAsync(
                saglayiciEntity.Id, null, istek.Amac ?? "MetinYaz", guvenliPrompt,
                yanit.IstekTokenSayisi, yanit.CevapTokenSayisi, yanit.MaliyetUsd,
                yanit.BasariliMi, yanit.HataMesaji);
        }

        return yanit.BasariliMi
            ? Cevap<string?>.Basarili(yanit.Metin)
            : Cevap<string?>.Hata(yanit.HataMesaji ?? "AI yanıtı alınamadı.");
    }

    [HttpGet("saglayicilar")]
    public async Task<Cevap<List<AISaglayicisi>>> SaglayicilariGetir()
    {
        var liste = await _db.AISaglayicilari.AsNoTracking().OrderBy(s => s.SiraNo).ToListAsync();
        foreach (var s in liste) s.ApiKeyEncrypted = "********";
        return Cevap<List<AISaglayicisi>>.Basarili(liste);
    }

    [HttpPost("saglayici")]
    public async Task<Cevap<AISaglayicisi>> SaglayiciEkle([FromBody] AISaglayicisi saglayici)
    {
        if (saglayici.Id > 0)
        {
            saglayici.GuncellenmeTarihi = DateTime.UtcNow;
            _db.AISaglayicilari.Update(saglayici);
        }
        else
        {
            saglayici.OlusturulmaTarihi = DateTime.UtcNow;
            _db.AISaglayicilari.Add(saglayici);
        }
        await _db.SaveChangesAsync();
        return Cevap<AISaglayicisi>.Basarili(saglayici, saglayici.Id > 0 ? "Sağlayıcı güncellendi." : "Sağlayıcı eklendi.");
    }

    [HttpPost("saglayici/{id:int}/test")]
    public async Task<Cevap<bool>> SaglayiciTest(int id)
    {
        var saglayiciEntity = await _db.AISaglayicilari.FindAsync(id);
        if (saglayiciEntity == null)
            return Cevap<bool>.Hata("Sağlayıcı bulunamadı.");

        var saglayici = _fabrika.SaglayiciOlustur(saglayiciEntity, new HttpClient());
        var sonuc = await saglayici.SaglikTestiAsync();
        return Cevap<bool>.Basarili(sonuc, sonuc ? "Bağlantı başarılı" : "Bağlantı başarısız");
    }

    [HttpGet("maliyet")]
    public async Task<Cevap<object>> AylikMaliyet()
    {
        var buAy = await _db.AICagrisiKayitlari
            .Where(k => k.OlusturulmaTarihi.Month == DateTime.UtcNow.Month)
            .GroupBy(k => 1)
            .Select(g => new { ToplamCagri = g.Count(), ToplamMaliyet = g.Sum(k => k.ToplamMaliyetUsd) })
            .FirstOrDefaultAsync();

        return Cevap<object>.Basarili(buAy ?? new { ToplamCagri = 0, ToplamMaliyet = 0m }!);
    }

    [HttpGet("cagrilar")]
    public async Task<Cevap<List<AICagrisiKaydi>>> CagrilariGetir([FromQuery] int sayfa = 1)
    {
        var liste = await _db.AICagrisiKayitlari
            .AsNoTracking()
            .Include(k => k.Saglayici)
            .OrderByDescending(k => k.OlusturulmaTarihi)
            .Skip((sayfa - 1) * 20)
            .Take(20)
            .ToListAsync();
        return Cevap<List<AICagrisiKaydi>>.Basarili(liste);
    }

    /// <summary>
    /// AI ile çeviri. Eksik çevirileri tamamlamak için kullanılır.
    /// Hedef dilde karşılığı olmayan anahtarları AI otomatik çevirir.
    /// </summary>
    [HttpPost("cevir")]
    public async Task<Cevap<string?>> AICevir([FromBody] AICeviriIstegi istek)
    {
        if (string.IsNullOrWhiteSpace(istek.Metin))
            return Cevap<string?>.Hata("Çevrilecek metin boş.");

        var saglayici = await _fabrika.SaglayiciGetirAsync(_db, istek.SaglayiciTip);
        if (saglayici == null)
            return Cevap<string?>.Hata("Aktif AI sağlayıcısı bulunamadı.");

        var saglayiciEntity = await _db.AISaglayicilari.FirstOrDefaultAsync(s => s.AktifMi);

        var dilHaritasi = new Dictionary<string, string>
        {
            ["tr"] = "Turkce", ["en"] = "Ingilizce", ["de"] = "Almanca",
            ["fr"] = "Fransizca", ["ru"] = "Rusca", ["ar"] = "Arapca",
            ["es"] = "Ispanyolca", ["zh"] = "Cince"
        };

        var kaynakAd = istek.KaynakDil != null && dilHaritasi.ContainsKey(istek.KaynakDil) ? dilHaritasi[istek.KaynakDil] : "Turkce";
        var hedefAd = dilHaritasi.ContainsKey(istek.HedefDil) ? dilHaritasi[istek.HedefDil] : istek.HedefDil;

        var sistemPrompt = $"Sen profesyonel bir cevirmensin. Verilen metni {kaynakAd}'den {hedefAd}'ye cevir. SADECE ceviriyi ver, aciklama yapma. Kisa ve net ol.";

        var yanit = await saglayici.MetinUretAsync(new AIIstek
        {
            KullaniciPrompt = istek.Metin,
            SistemPrompt = sistemPrompt,
            Model = saglayiciEntity?.Model ?? "llama3.2:3b",
            Sicaklik = 0.2f,
            MaksimumToken = 500
        }, HttpContext.RequestAborted);

        return yanit.BasariliMi
            ? Cevap<string?>.Basarili(yanit.Metin.Trim())
            : Cevap<string?>.Hata(yanit.HataMesaji ?? "AI ceviri yapamadi.");
    }

    public class AICeviriIstegi
    {
        public string Metin { get; set; } = "";
        public string HedefDil { get; set; } = "en";
        public string? KaynakDil { get; set; }
        public AISaglayiciTipi? SaglayiciTip { get; set; }
    }

    public class AIYazIstegi
    {
        public string Prompt { get; set; } = "";
        public string? SistemPrompt { get; set; }
        public string? Amac { get; set; }
        public AISaglayiciTipi? SaglayiciTip { get; set; }
    }
}
