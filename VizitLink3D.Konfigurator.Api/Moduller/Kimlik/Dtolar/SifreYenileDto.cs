namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;

public class SifreYenileDto
{
    public string Token { get; set; } = string.Empty;
    public string YeniSifre { get; set; } = string.Empty;
    /// <summary>
    /// Defense-in-depth: Şifre tekrar alanı.
    /// BFF seviyesinde eşleşme kontrolü yapılır; API validator da doğrular.
    /// </summary>
    public string YeniSifreTekrar { get; set; } = string.Empty;
}
