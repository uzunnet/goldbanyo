using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace VizitLink3D.Konfigurator.Servisler;

// ──────────────────────────────────────────────
// DTO'lar — 5116 API kontratina uygun
// ──────────────────────────────────────────────

/// <summary>
/// Admin model listesindeki bir ogenin guvenli ozeti.
/// Yonetim paneli icin GuncellenmeTarihi alanini da icerir.
/// </summary>
public class ModelYonetimListeOgesiDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Aciklama { get; set; }
    public long BoyutBayt { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; }
    public DateTime? GuncellenmeTarihi { get; set; }
}

/// <summary>
/// Public model listesindeki bir ogenin guvenli ozeti.
/// DosyaYolu ve Sha256Hash gibi hassas alanlari icermez.
/// </summary>
public class ModelListeOgesiDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Aciklama { get; set; }
    public string DosyaAdi { get; set; } = "";
    public long BoyutBayt { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
    public bool AktifMi { get; set; } = true;
}

/// <summary>
/// Model yukleme sonucu — API yanit DTO'su.
/// </summary>
public class ModelYukleSonucuDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Aciklama { get; set; }
    public string DosyaAdi { get; set; } = "";
    public string IcerikTuru { get; set; } = "";
    public long BoyutBayt { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
}

/// <summary>
/// API'den gelen UcBoyutModelDto record'unu karsilayan DTO.
/// Konfigurator.Api public endpoint kontratina uygundur.
/// </summary>
public class UcBoyutModelDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Aciklama { get; set; }
    public string DosyaAdi { get; set; } = "";
    public string IcerikTuru { get; set; } = "";
    public long BoyutBayt { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
}

// ──────────────────────────────────────────────
// P06-C: Parca yonetimi DTO'lari
// ──────────────────────────────────────────────

/// <summary>
/// Yonetim paneli parca listesi DTO'su.
/// API'deki UcBoyutModelParcasiYonetimDto record kontratina uygundur.
/// </summary>
public class ParcaYonetimDto
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string MeshAdi { get; set; } = "";
    public string GorunenAd { get; set; } = "";
    public string ParcaTuru { get; set; } = "";
    public bool RenkDegistirilebilirMi { get; set; }
    public bool GorunurMu { get; set; }
    public string? VarsayilanRenk { get; set; }
    public string? VarsayilanMalzeme { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
    public DateTime? GuncellenmeTarihi { get; set; }
}

/// <summary>
/// Senkronizasyon sonuc DTO'su.
/// API'deki SenkronizeSonucDto record kontratina uygundur.
/// </summary>
public class ParcaSenkronizeSonucDto
{
    public int Eklenen { get; set; }
    public int GeriYuklenen { get; set; }
    public int YumusakSilinen { get; set; }
}

/// <summary>
/// Parca metadata guncelleme istek DTO'su.
/// API'deki ParcaMetadataGuncelleDto kontratina uygundur.
/// Tum alanlar opsiyonel — sadece gonderilen alanlar guncellenir.
/// </summary>
public class ParcaMetadataGuncelleIstekDto
{
    public string? GorunenAd { get; set; }
    public string? ParcaTuru { get; set; }
    public bool? RenkDegistirilebilirMi { get; set; }
    public bool? GorunurMu { get; set; }
    public string? VarsayilanRenk { get; set; }
    public string? VarsayilanMalzeme { get; set; }
}

/// <summary>
/// Modeller yonetim servisi — BFF uzerinden Konfigurator API'ye erisir.
/// Tum istekler sunucu tarafinda yapilir; browser'a credential sizmaz.
/// </summary>
public class ModellerYonetimServisi
{
    private readonly HttpClient _http;
    private readonly IOptions<BffGuvenlikAyarlari> _bffAyarlari;
    private readonly ILogger<ModellerYonetimServisi> _logger;

    private static readonly JsonSerializerOptions _jsonSecenekleri = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// BFF gizli anahtarinin tanimli olup olmadigini dondurur.
    /// Bos ise yukleme yapilamaz; UI yapilandirma hatasi gostermelidir.
    /// </summary>
    public virtual bool BffAnahtarTanimliMi =>
        !string.IsNullOrWhiteSpace(_bffAyarlari.Value.Anahtar);

    public ModellerYonetimServisi(
        HttpClient http,
        IOptions<BffGuvenlikAyarlari> bffAyarlari,
        ILogger<ModellerYonetimServisi> logger)
    {
        _http = http;
        _bffAyarlari = bffAyarlari;
        _logger = logger;
    }

    /// <summary>
    /// Public model listesini Konfigurator API'den ceker.
    /// Basarisiz olursa null doner; UI generic hata gosterir.
    /// </summary>
    public virtual async Task<List<ModelListeOgesiDto>?> ListeleAsync(CancellationToken iptal = default)
    {
        try
        {
            var yanit = await _http.GetAsync("api/modeller", iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Model listesi API hatasi: {StatusCode}", (int)yanit.StatusCode);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<List<ModelListeOgesiDto>>>(_jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi)
            {
                _logger.LogWarning("Model listesi API yaniti basarisiz: {Mesaj}", cevap?.Mesaj);
                return null;
            }

            return cevap.Veri ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model listesi cekilirken hata olustu");
            return null;
        }
    }

    /// <summary>
    /// GLB dosyasi yukler. BFF gizli anahtari header'a eklenir.
    /// Anahtar tanimli degilse islem yapilmaz, hata doner.
    /// </summary>
    public virtual async Task<ModelYukleSonucuDto?> YukleAsync(
        string ad,
        string? aciklama,
        Stream dosyaAkisi,
        string dosyaAdi,
        string icerikTuru,
        CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
        {
            _logger.LogError("BFF guvenlik anahtari tanimli degil, yukleme yapilamaz");
            return null;
        }

        try
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(ad), "ad");

            if (!string.IsNullOrWhiteSpace(aciklama))
                form.Add(new StringContent(aciklama), "aciklama");

            var dosyaIcerigi = new StreamContent(dosyaAkisi);
            dosyaIcerigi.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(icerikTuru);
            form.Add(dosyaIcerigi, "dosya", dosyaAdi);

            var istek = new HttpRequestMessage(HttpMethod.Post, "api/yonetim/modeller")
            {
                Content = form
            };
            istek.Headers.Add("X-Konfigurator-Bff-Anahtari", _bffAyarlari.Value.Anahtar);

            var yanit = await _http.SendAsync(istek, iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Model yukleme API hatasi: {StatusCode}", (int)yanit.StatusCode);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<ModelYukleSonucuDto>>(_jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi)
            {
                _logger.LogWarning("Model yukleme API yaniti basarisiz: {Mesaj}", cevap?.Mesaj);
                return null;
            }

            return cevap.Veri;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model yuklenirken hata olustu");
            return null;
        }
    }

    /// <summary>
    /// Public model detayini Konfigurator API'den slug ile ceker.
    /// Basarisiz olursa null doner; UI generic hata gosterir.
    /// </summary>
    public virtual async Task<UcBoyutModelDto?> GetirAsync(string slug, CancellationToken iptal = default)
    {
        try
        {
            var yanit = await _http.GetAsync($"api/modeller/{slug}", iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Model detay API hatasi: {StatusCode} slug={Slug}", (int)yanit.StatusCode, slug);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<UcBoyutModelDto>>(_jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi)
            {
                _logger.LogWarning("Model detay API yaniti basarisiz: {Mesaj}", cevap?.Mesaj);
                return null;
            }

            return cevap.Veri;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model detay cekilirken hata: slug={Slug}", slug);
            return null;
        }
    }

    /// <summary>
    /// Admin model listesini BFF korumali API'den ceker.
    /// X-Konfigurator-Bff-Anahtari header'i otomatik eklenir.
    /// Basarisiz olursa null doner; UI generic hata gosterir.
    /// </summary>
    public virtual async Task<List<ModelYonetimListeOgesiDto>?> YonetimListeleAsync(CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
        {
            _logger.LogError("BFF guvenlik anahtari tanimli degil, admin listesi alinamaz");
            return null;
        }

        try
        {
            var istek = new HttpRequestMessage(HttpMethod.Get, "api/yonetim/modeller");
            istek.Headers.Add("X-Konfigurator-Bff-Anahtari", _bffAyarlari.Value.Anahtar);

            var yanit = await _http.SendAsync(istek, iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Admin model listesi API hatasi: {StatusCode}", (int)yanit.StatusCode);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<List<ModelYonetimListeOgesiDto>>>(_jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi)
            {
                _logger.LogWarning("Admin model listesi API yaniti basarisiz: {Mesaj}", cevap?.Mesaj);
                return null;
            }

            return cevap.Veri ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin model listesi cekilirken hata olustu");
            return null;
        }
    }

    /// <summary>
    /// Model yayin durumunu (AktifMi) BFF korumali API ile gunceller.
    /// X-Konfigurator-Bff-Anahtari header'i otomatik eklenir.
    /// Basarisiz olursa null doner; UI generic hata gosterir.
    /// </summary>
    public virtual async Task<ModelYonetimListeOgesiDto?> YayinDurumuGuncelleAsync(
        int id, bool aktifMi, CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
        {
            _logger.LogError("BFF guvenlik anahtari tanimli degil, yayin durumu guncellenemez");
            return null;
        }

        try
        {
            var govde = new { aktifMi };
            var istek = new HttpRequestMessage(HttpMethod.Put, $"api/yonetim/modeller/{id}/yayin-durumu")
            {
                Content = JsonContent.Create(govde, options: _jsonSecenekleri)
            };
            istek.Headers.Add("X-Konfigurator-Bff-Anahtari", _bffAyarlari.Value.Anahtar);

            var yanit = await _http.SendAsync(istek, iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Yayin durumu API hatasi: {StatusCode} id={Id}", (int)yanit.StatusCode, id);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<ModelYonetimListeOgesiDto>>(_jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi)
            {
                _logger.LogWarning("Yayin durumu API yaniti basarisiz: {Mesaj}", cevap?.Mesaj);
                return null;
            }

            return cevap.Veri;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yayin durumu guncellenirken hata: id={Id}", id);
            return null;
        }
    }

    /// <summary>
    /// Public model listesini guvenli DTO formatinda dondurur.
    /// ModelUrl BFF proxy uzerinden olusturulur; browser API'ye dogrudan erisemez.
    /// </summary>
    public virtual async Task<List<PublicModelListeOgesiDto>?> PublicModelListesiGetirAsync(CancellationToken iptal = default)
    {
        try
        {
            var yanit = await _http.GetAsync("api/modeller", iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Public model listesi API hatasi: {StatusCode}", (int)yanit.StatusCode);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<List<ModelListeOgesiDto>>>(_jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi || cevap.Veri is null)
            {
                _logger.LogWarning("Public model listesi API yaniti basarisiz");
                return null;
            }

            return cevap.Veri.Select(x => new PublicModelListeOgesiDto
            {
                Id = x.Id,
                Ad = x.Ad,
                Slug = x.Slug,
                Aciklama = x.Aciklama,
                DosyaAdi = x.DosyaAdi,
                BoyutBayt = x.BoyutBayt,
                KapsulResimUrl = null,
                ModelUrl = $"/api/public/modeller/{x.Slug}/dosya",
                OlusturulmaTarihi = x.OlusturulmaTarihi
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public model listesi cekilirken hata");
            return null;
        }
    }

    /// <summary>
    /// Public model detayini guvenli DTO formatinda dondurur.
    /// ModelUrl BFF proxy uzerinden olusturulur; browser API'ye dogrudan erisemez.
    /// </summary>
    public virtual async Task<PublicModelDetayDto?> PublicModelDetayGetirAsync(string slug, CancellationToken iptal = default)
    {
        try
        {
            var yanit = await _http.GetAsync($"api/modeller/{slug}", iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Public model detay API hatasi: {StatusCode} slug={Slug}", (int)yanit.StatusCode, slug);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<UcBoyutModelDto>>(_jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi || cevap.Veri is null)
            {
                _logger.LogWarning("Public model detay API yaniti basarisiz");
                return null;
            }

            var veri = cevap.Veri;
            return new PublicModelDetayDto
            {
                Id = veri.Id,
                Ad = veri.Ad,
                Slug = veri.Slug,
                Aciklama = veri.Aciklama,
                DosyaAdi = veri.DosyaAdi,
                IcerikTuru = veri.IcerikTuru,
                BoyutBayt = veri.BoyutBayt,
                ModelUrl = $"/api/public/modeller/{veri.Slug}/dosya",
                OlusturulmaTarihi = veri.OlusturulmaTarihi
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public model detay cekilirken hata: slug={Slug}", slug);
            return null;
        }
    }

    /// <summary>
    /// GLB model dosyasini API medya havuzundan indirir.
    /// Once slug ile model detayini ceker, DosyaAdi'ni ogrenir,
    /// sonra /medya/3d-modeller/{dosyaAdi} adresinden binary'yi alir.
    /// </summary>
    public virtual async Task<(Stream? Akis, string? IcerikTuru, string? DosyaAdi)> ModelDosyasiIndirAsync(
        string slug, CancellationToken iptal = default)
    {
        try
        {
            // Once model detayini al, DosyaAdi'ni ogren
            var detay = await GetirAsync(slug, iptal);
            if (detay is null)
                return (null, null, null);

            var dosyaAdi = detay.DosyaAdi;
            if (string.IsNullOrWhiteSpace(dosyaAdi))
                return (null, null, null);

            // API medya havuzundan GLB dosyasini indir
            var medyaYanit = await _http.GetAsync(
                $"medya/3d-modeller/{dosyaAdi}",
                HttpCompletionOption.ResponseHeadersRead, iptal);

            if (!medyaYanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("GLB dosya indirme hatasi: {StatusCode}", (int)medyaYanit.StatusCode);
                return (null, null, null);
            }

            var akis = await medyaYanit.Content.ReadAsStreamAsync(iptal);
            var icerikTuru = detay.IcerikTuru;
            return (akis, icerikTuru, dosyaAdi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GLB dosya indirilirken hata: slug={Slug}", slug);
            return (null, null, null);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // P06-C: Parca yonetimi BFF metodlari
    // Tum istekler X-Konfigurator-Bff-Anahtari header ile korunur.
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Admin parca listesini BFF korumali API'den ceker.
    /// Silinmis kayitlari da icerir (admin paneli icin).
    /// </summary>
    public virtual async Task<List<ParcaYonetimDto>?> ParcalariGetirAsync(
        int modelId, CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
        {
            _logger.LogError("BFF guvenlik anahtari tanimli degil, parca listesi alinamaz");
            return null;
        }

        try
        {
            var istek = new HttpRequestMessage(HttpMethod.Get,
                $"api/yonetim/modeller/{modelId}/parcalar");
            istek.Headers.Add("X-Konfigurator-Bff-Anahtari", _bffAyarlari.Value.Anahtar);

            var yanit = await _http.SendAsync(istek, iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Parca listesi API hatasi: {StatusCode} modelId={ModelId}",
                    (int)yanit.StatusCode, modelId);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<List<ParcaYonetimDto>>>(
                _jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi)
            {
                _logger.LogWarning("Parca listesi API yaniti basarisiz: {Mesaj}", cevap?.Mesaj);
                return null;
            }

            return cevap.Veri ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parca listesi cekilirken hata: modelId={ModelId}", modelId);
            return null;
        }
    }

    /// <summary>
    /// Istemcide kesfedilen mesh adlarini BFF uzerinden API'ye senkronize eder.
    /// Yeni mesh'ler eklenir, listede olmayanlar soft-delete yapilir.
    /// ASLA fiziksel silme yapilmaz.
    /// </summary>
    public virtual async Task<ParcaSenkronizeSonucDto?> ParcalariSenkronizeEtAsync(
        int modelId, string[] meshAdlari, CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
        {
            _logger.LogError("BFF guvenlik anahtari tanimli degil, parca senkronizasyonu yapilamaz");
            return null;
        }

        try
        {
            var govde = new { meshAdlari };
            var istek = new HttpRequestMessage(HttpMethod.Post,
                $"api/yonetim/modeller/{modelId}/parcalar/senkronize")
            {
                Content = JsonContent.Create(govde, options: _jsonSecenekleri)
            };
            istek.Headers.Add("X-Konfigurator-Bff-Anahtari", _bffAyarlari.Value.Anahtar);

            var yanit = await _http.SendAsync(istek, iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Parca senkronizasyon API hatasi: {StatusCode} modelId={ModelId}",
                    (int)yanit.StatusCode, modelId);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<ParcaSenkronizeSonucDto>>(
                _jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi)
            {
                _logger.LogWarning("Parca senkronizasyon API yaniti basarisiz: {Mesaj}", cevap?.Mesaj);
                return null;
            }

            return cevap.Veri;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parca senkronizasyonu sirasinda hata: modelId={ModelId}", modelId);
            return null;
        }
    }

    /// <summary>
    /// Tek bir parcanin metadata'sini BFF uzerinden gunceller.
    /// Sadece gonderilen alanlar guncellenir; diger alanlara dokunulmaz.
    /// </summary>
    public virtual async Task<ParcaYonetimDto?> ParcaMetadataGuncelleAsync(
        int modelId, int parcaId, ParcaMetadataGuncelleIstekDto dto,
        CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
        {
            _logger.LogError("BFF guvenlik anahtari tanimli degil, parca metadata guncellenemez");
            return null;
        }

        try
        {
            var istek = new HttpRequestMessage(HttpMethod.Put,
                $"api/yonetim/modeller/{modelId}/parcalar/{parcaId}")
            {
                Content = JsonContent.Create(dto, options: _jsonSecenekleri)
            };
            istek.Headers.Add("X-Konfigurator-Bff-Anahtari", _bffAyarlari.Value.Anahtar);

            var yanit = await _http.SendAsync(istek, iptal);

            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Parca metadata API hatasi: {StatusCode} modelId={ModelId} parcaId={ParcaId}",
                    (int)yanit.StatusCode, modelId, parcaId);
                return null;
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<ParcaYonetimDto>>(
                _jsonSecenekleri, iptal);

            if (cevap is null || !cevap.BasariliMi)
            {
                _logger.LogWarning("Parca metadata API yaniti basarisiz: {Mesaj}", cevap?.Mesaj);
                return null;
            }

            return cevap.Veri;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parca metadata guncellenirken hata: modelId={ModelId} parcaId={ParcaId}",
                modelId, parcaId);
            return null;
        }
    }
}
