using Microsoft.AspNetCore.DataProtection;

namespace Desadoor.Api.Servisler;

public interface IApiKeySifrelemeServisi
{
    string Sifrele(string duzMetin);
    string? Coz(string? sifreliMetin);
}

public class ApiKeySifrelemeServisi : IApiKeySifrelemeServisi
{
    private readonly IDataProtector _koruyucu;

    public ApiKeySifrelemeServisi(IDataProtectionProvider saglayici)
    {
        _koruyucu = saglayici.CreateProtector("DesaDoor.ApiKey");
    }

    public string Sifrele(string duzMetin)
        => _koruyucu.Protect(duzMetin);

    public string? Coz(string? sifreliMetin)
    {
        if (string.IsNullOrEmpty(sifreliMetin)) return null;
        try
        {
            return _koruyucu.Unprotect(sifreliMetin);
        }
        catch
        {
            return sifreliMetin; // Sifreli degilse duz metni dondur (seed verisi vb.)
        }
    }
}
