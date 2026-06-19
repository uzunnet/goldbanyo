using System.Text.Json;
using Desadoor.UI.Models;
using Desadoor.Ortak.Modeller;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Desadoor.UI.Servisler;

public class ApiIstemcisi(HttpClient http, IJSRuntime js)
{
    public Uri? BaseAddress => http.BaseAddress;
    public string ApiBaseUrl => http.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5015";

    private async Task AuthorizationAyarlaAsync()
    {
        try
        {
            var token = await js.InvokeAsync<string?>("localStorage.getItem", "desadoor_token");
            if (!string.IsNullOrEmpty(token))
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch { /* localStorage erişilemezse token olmadan devam edilir */ }
    }

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            await AuthorizationAyarlaAsync();
            var response = await http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[ApiIstemcisi GET] Url: {url}, Status: {response.StatusCode}, Body: {errBody?.Substring(0, Math.Min(200, errBody?.Length ?? 0))}");
                return default;
            }
            var json = await response.Content.ReadAsStringAsync();
            var yanit = JsonSerializer.Deserialize<Cevap<T>>(json, _jsonOpts);
            return yanit?.BasariliMi == true ? yanit.Veri : default;
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi GET HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return default;
        }
    }

    public async Task<Cevap<T>?> PostAsync<T>(string url, object govde)
    {
        try
        {
            await AuthorizationAyarlaAsync();
            var yanit = await http.PostAsJsonAsync(url, govde);
            return await yanit.Content.ReadFromJsonAsync<Cevap<T>>(_jsonOpts);
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi POST HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }

    public async Task<Cevap<T>?> PutAsync<T>(string url, object govde)
    {
        try
        {
            await AuthorizationAyarlaAsync();
            var yanit = await http.PutAsJsonAsync(url, govde);
            return await yanit.Content.ReadFromJsonAsync<Cevap<T>>(_jsonOpts);
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi PUT HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }

    public async Task<Cevap<object>?> PatchAsync(string url, object? govde = null)
    {
        try
        {
            await AuthorizationAyarlaAsync();
            var istek = new HttpRequestMessage(HttpMethod.Patch, url);
            if (govde != null)
                istek.Content = JsonContent.Create(govde);
            var yanit = await http.SendAsync(istek);
            return await yanit.Content.ReadFromJsonAsync<Cevap<object>>();
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi PATCH HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }

    public async Task<Cevap<object>?> DeleteAsync(string url)
    {
        try
        {
            await AuthorizationAyarlaAsync();
            var yanit = await http.DeleteAsync(url);
            return await yanit.Content.ReadFromJsonAsync<Cevap<object>>();
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi DELETE HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }

    public async Task<Cevap<T>?> PostMultipartAsync<T>(string url, MultipartFormDataContent icerik)
    {
        try
        {
            await AuthorizationAyarlaAsync();
            var yanit = await http.PostAsync(url, icerik);
            return await yanit.Content.ReadFromJsonAsync<Cevap<T>>(_jsonOpts);
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi POSTMULTIPART HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }
}



