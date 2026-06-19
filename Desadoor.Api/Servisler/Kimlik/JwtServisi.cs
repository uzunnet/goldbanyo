namespace Desadoor.Api.Servisler.Kimlik;

public class JwtServisi(IConfiguration yapilandirma)
{
    public string Anahtar => yapilandirma["Jwt:Anahtar"]!;
    public string Yayinci => yapilandirma["Jwt:Yayinci"] ?? "DesaDoorAPI";
    public string Izleyici => yapilandirma["Jwt:Izleyici"] ?? "DesaDoorUI";
    public int GecerlilikSuresiDakika => int.Parse(yapilandirma["Jwt:GecerlilikSuresiDakika"] ?? "60");
}
