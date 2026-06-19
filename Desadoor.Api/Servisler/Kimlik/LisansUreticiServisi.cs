using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Desadoor.Api.Servisler.Kimlik;

public class LisansUreticiServisi(IConfiguration yapilandirma)
{
    private readonly string _gizliAnahtar = yapilandirma["LisansAyarlari:GizliAnahtar"]
        ?? "DESADOOR_HMAC_2026_SECRET_KEY_min_32char";

    public string LisansAnahtariUret(string domain, DateTime baslangic, DateTime bitis, string lisansTipi)
    {
        var veri = $"{domain}|{baslangic:yyyy-MM-dd}|{bitis:yyyy-MM-dd}|{lisansTipi}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_gizliAnahtar));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(veri));
        return Convert.ToBase64String(hash);
    }

    public bool LisansDogrula(string domain, DateTime baslangic, DateTime bitis, string lisansTipi, string lisansAnahtari)
    {
        var beklenen = LisansAnahtariUret(domain, baslangic, bitis, lisansTipi);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(beklenen),
            Encoding.UTF8.GetBytes(lisansAnahtari));
    }

    public bool GecerliMi(string birincilDomain, string? yedekDomain, DateTime bitisTarihi)
    {
        if (DateTime.UtcNow > bitisTarihi)
            return false;

        return true;
    }
}
