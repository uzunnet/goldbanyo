using System.Net.Http.Json;
using System.Text.Json;

namespace VizitLink3D.Konfigurator.Servisler;

/// <summary>
/// Harici API'ye HttpClient tabanli basit istemci.
/// </summary>
public class ApiIstemcisi
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _jsonSecenekleri = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiIstemcisi(HttpClient http)
    {
        _http = http;
    }

    public async Task<Cevap<T>?> PostHamAsync<T>(string url, object govde)
    {
        try
        {
            var yanit = await _http.PostAsJsonAsync(url, govde);
            return await yanit.Content.ReadFromJsonAsync<Cevap<T>>(_jsonSecenekleri);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ApiIstemcisi POST HATA] {url}: {ex.Message}");
            return null;
        }
    }

    public async Task<T?> PostAsync<T>(string url, object govde)
    {
        try
        {
            var yanit = await _http.PostAsJsonAsync(url, govde);
            yanit.EnsureSuccessStatusCode();
            var sonuc = await yanit.Content.ReadFromJsonAsync<Cevap<T>>(_jsonSecenekleri);
            return sonuc is not null && sonuc.BasariliMi ? sonuc.Veri : default;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ApiIstemcisi POST HATA] {url}: {ex.Message}");
            return default;
        }
    }

    public async Task<bool> SaglikKontrolAsync()
    {
        try
        {
            var yanit = await _http.GetAsync("/saglik");
            return yanit.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// API yanit modeli (Cevap<T> esdegeri).
/// </summary>
public class Cevap<T>
{
    public bool BasariliMi { get; set; }
    public string? Mesaj { get; set; }
    public T? Veri { get; set; }
    public List<string>? Hatalar { get; set; }

    public static Cevap<T> Basarili(T veri, string mesaj = "Islem basarili.") =>
        new() { BasariliMi = true, Mesaj = mesaj, Veri = veri };

    public static Cevap<T> Hata(string mesaj, List<string>? hatalar = null) =>
        new() { BasariliMi = false, Mesaj = mesaj, Hatalar = hatalar ?? [] };
}
