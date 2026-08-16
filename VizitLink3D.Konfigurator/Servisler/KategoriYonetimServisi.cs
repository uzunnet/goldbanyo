using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace VizitLink3D.Konfigurator.Servisler;

// ──────────────────────────────────────────────
// BFF DTO'ları — API kontratına uygun
// ──────────────────────────────────────────────

/// <summary>
/// Hiyerarşik kategori DTO'su. AltKategoriler alanı ağaç yapısını taşır.
/// API'deki KategoriDto record'una uygundur.
/// </summary>
public class KategoriAgacDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Aciklama { get; set; }
    public int? UstKategoriId { get; set; }
    public int Sira { get; set; }
    public bool AktifMi { get; set; } = true;
    public List<KategoriAgacDto>? AltKategoriler { get; set; }
}

/// <summary>
/// Kategori oluşturma istek DTO'su.
/// </summary>
public class KategoriEkleIstekDto
{
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public int? UstKategoriId { get; set; }
    public int Sira { get; set; }
}

/// <summary>
/// Kategori güncelleme istek DTO'su.
/// </summary>
public class KategoriGuncelleIstekDto
{
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public int? UstKategoriId { get; set; }
    public int Sira { get; set; }
    public bool AktifMi { get; set; } = true;
}

/// <summary>
/// Kategori yönetim servisi — BFF üzerinden Konfigurator API'ye erişir.
/// Türkçe wrapper: doğrudan HttpClient kullanılmaz.
/// </summary>
public class KategoriYonetimServisi
{
    private readonly HttpClient _http;
    private readonly IOptions<BffGuvenlikAyarlari> _bffAyarlari;
    private readonly ILogger<KategoriYonetimServisi> _logger;

    private static readonly JsonSerializerOptions _jsonSecenekleri = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public KategoriYonetimServisi(
        HttpClient http,
        IOptions<BffGuvenlikAyarlari> bffAyarlari,
        ILogger<KategoriYonetimServisi> logger)
    {
        _http = http;
        _bffAyarlari = bffAyarlari;
        _logger = logger;
    }

    private bool BffAnahtarTanimliMi =>
        !string.IsNullOrWhiteSpace(_bffAyarlari.Value.Anahtar);

    private void BffAnahtarEkle(HttpRequestMessage istek)
    {
        if (BffAnahtarTanimliMi)
            istek.Headers.Add("X-Konfigurator-Bff-Anahtari", _bffAyarlari.Value.Anahtar);
    }

    /// <summary>
    /// Ağaç yapısında tüm kategorileri getirir.
    /// </summary>
    public virtual async Task<Cevap<List<KategoriAgacDto>>> AgacGetirAsync(CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
        {
            _logger.LogError("BFF anahtarı tanımlı değil, kategori listesi alınamaz");
            return Cevap<List<KategoriAgacDto>>.Hata("Yapılandırma hatası.");
        }

        try
        {
            var istek = new HttpRequestMessage(HttpMethod.Get, "api/yonetim/kategoriler");
            BffAnahtarEkle(istek);

            var yanit = await _http.SendAsync(istek, iptal);
            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Kategori ağaç API hatası: {StatusCode}", (int)yanit.StatusCode);
                return Cevap<List<KategoriAgacDto>>.Hata("Kategori listesi alınamadı.");
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<List<KategoriAgacDto>>>(_jsonSecenekleri, iptal);
            return cevap ?? Cevap<List<KategoriAgacDto>>.Hata("Geçersiz API yanıtı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori ağaç çekilirken hata");
            return Cevap<List<KategoriAgacDto>>.Hata("Kategori listesi alınırken hata oluştu.");
        }
    }

    /// <summary>
    /// Düz liste olarak tüm kategorileri getirir (ağaç değil).
    /// </summary>
    public virtual async Task<Cevap<List<KategoriAgacDto>>> ListeGetirAsync(CancellationToken iptal = default)
    {
        var agacCevap = await AgacGetirAsync(iptal);
        if (!agacCevap.BasariliMi || agacCevap.Veri is null)
            return Cevap<List<KategoriAgacDto>>.Hata(agacCevap.Mesaj ?? "Liste alınamadı.");

        var duzListe = new List<KategoriAgacDto>();
        AgaciDuzlestir(agacCevap.Veri, duzListe);
        return Cevap<List<KategoriAgacDto>>.Basarili(duzListe);
    }

    /// <summary>
    /// Yeni kategori ekler.
    /// </summary>
    public virtual async Task<Cevap<KategoriAgacDto>> EkleAsync(KategoriEkleIstekDto dto, CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
            return Cevap<KategoriAgacDto>.Hata("Yapılandırma hatası.");

        try
        {
            var istek = new HttpRequestMessage(HttpMethod.Post, "api/yonetim/kategoriler")
            {
                Content = JsonContent.Create(dto, options: _jsonSecenekleri)
            };
            BffAnahtarEkle(istek);

            var yanit = await _http.SendAsync(istek, iptal);
            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Kategori ekleme API hatası: {StatusCode}", (int)yanit.StatusCode);
                return Cevap<KategoriAgacDto>.Hata("Kategori eklenemedi.");
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<KategoriAgacDto>>(_jsonSecenekleri, iptal);
            return cevap ?? Cevap<KategoriAgacDto>.Hata("Geçersiz API yanıtı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori eklenirken hata");
            return Cevap<KategoriAgacDto>.Hata("Kategori eklenirken hata oluştu.");
        }
    }

    /// <summary>
    /// Kategori günceller.
    /// </summary>
    public virtual async Task<Cevap<KategoriAgacDto>> GuncelleAsync(int id, KategoriGuncelleIstekDto dto, CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
            return Cevap<KategoriAgacDto>.Hata("Yapılandırma hatası.");

        try
        {
            var istek = new HttpRequestMessage(HttpMethod.Put, $"api/yonetim/kategoriler/{id}")
            {
                Content = JsonContent.Create(dto, options: _jsonSecenekleri)
            };
            BffAnahtarEkle(istek);

            var yanit = await _http.SendAsync(istek, iptal);
            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Kategori güncelleme API hatası: {StatusCode}", (int)yanit.StatusCode);
                return Cevap<KategoriAgacDto>.Hata("Kategori güncellenemedi.");
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<KategoriAgacDto>>(_jsonSecenekleri, iptal);
            return cevap ?? Cevap<KategoriAgacDto>.Hata("Geçersiz API yanıtı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori güncellenirken hata: id={Id}", id);
            return Cevap<KategoriAgacDto>.Hata("Kategori güncellenirken hata oluştu.");
        }
    }

    /// <summary>
    /// Kategoriyi soft-delete yapar.
    /// </summary>
    public virtual async Task<Cevap<bool>> SilAsync(int id, CancellationToken iptal = default)
    {
        if (!BffAnahtarTanimliMi)
            return Cevap<bool>.Hata("Yapılandırma hatası.");

        try
        {
            var istek = new HttpRequestMessage(HttpMethod.Delete, $"api/yonetim/kategoriler/{id}");
            BffAnahtarEkle(istek);

            var yanit = await _http.SendAsync(istek, iptal);
            if (!yanit.IsSuccessStatusCode)
            {
                _logger.LogWarning("Kategori silme API hatası: {StatusCode}", (int)yanit.StatusCode);
                return Cevap<bool>.Hata("Kategori silinemedi.");
            }

            var cevap = await yanit.Content.ReadFromJsonAsync<Cevap<bool>>(_jsonSecenekleri, iptal);
            return cevap ?? Cevap<bool>.Hata("Geçersiz API yanıtı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori silinirken hata: id={Id}", id);
            return Cevap<bool>.Hata("Kategori silinirken hata oluştu.");
        }
    }

    /// <summary>
    /// Ağaç yapısını düz listeye çevirir (select box için).
    /// </summary>
    private static void AgaciDuzlestir(List<KategoriAgacDto> agac, List<KategoriAgacDto> hedef, string onEk = "")
    {
        foreach (var kat in agac)
        {
            var kopya = new KategoriAgacDto
            {
                Id = kat.Id,
                Ad = onEk + kat.Ad,
                Slug = kat.Slug,
                Aciklama = kat.Aciklama,
                UstKategoriId = kat.UstKategoriId,
                Sira = kat.Sira,
                AktifMi = kat.AktifMi,
                AltKategoriler = null
            };
            hedef.Add(kopya);

            if (kat.AltKategoriler is { Count: > 0 })
                AgaciDuzlestir(kat.AltKategoriler, hedef, onEk + "  ");
        }
    }
}
