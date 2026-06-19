using System.Text.Json;
using Desadoor.Ortak.Modeller;

namespace Desadoor.Api.Servisler;

public interface IOtomatikCeviriServisi
{
    Task<Cevap<string>> CevirAsync(string metin, string kaynakDil, string hedefDil);
}

public class OtomatikCeviriServisi : IOtomatikCeviriServisi
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OtomatikCeviriServisi> _logger;

    public OtomatikCeviriServisi(IHttpClientFactory httpClientFactory, ILogger<OtomatikCeviriServisi> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Cevap<string>> CevirAsync(string metin, string kaynakDil, string hedefDil)
    {
        if (string.IsNullOrWhiteSpace(metin))
            return Cevap<string>.Basarili(string.Empty);

        if (kaynakDil.Equals(hedefDil, StringComparison.OrdinalIgnoreCase))
            return Cevap<string>.Basarili(metin);

        try
        {
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={kaynakDil}&tl={hedefDil}&dt=t&q={Uri.EscapeDataString(metin)}";
            
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Çeviri servisi hata döndürdü. Hedef: {HedefDil}, Durum: {DurumKodu}", hedefDil, response.StatusCode);
                return Cevap<string>.Hata($"Çeviri API hatası: {response.StatusCode}");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            // Google Translate GTX endpoint returns an array like: [[["translated text", "original text", null, null, 1]], null, "tr", ...]
            
            using var doc = JsonDocument.Parse(responseText);
            var rootArray = doc.RootElement.EnumerateArray().FirstOrDefault();
            
            if (rootArray.ValueKind == JsonValueKind.Array)
            {
                var translatedText = string.Empty;
                foreach (var segment in rootArray.EnumerateArray())
                {
                    if (segment.ValueKind == JsonValueKind.Array && segment.GetArrayLength() > 0)
                    {
                        translatedText += segment[0].GetString();
                    }
                }
                
                return Cevap<string>.Basarili(translatedText.Trim());
            }

            return Cevap<string>.Hata("Çeviri yanıtı yorumlanamadı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yapay zeka çevirisi sırasında beklenmeyen bir hata oluştu.");
            return Cevap<string>.Hata("Sistem hatası nedeniyle çeviri yapılamadı.");
        }
    }
}
