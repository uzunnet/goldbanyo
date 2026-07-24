namespace VizitLink3D.Konfigurator.Servisler;

/// <summary>
/// BFF guvenlik konfigurasyonu — API'ye giden isteklerde kullanilacak gizli anahtar.
/// Browser'a asla sizmaz; sadece sunucu tarafinda okunur.
/// </summary>
public class BffGuvenlikAyarlari
{
    public const string BolumAdi = "BffGuvenlik";

    /// <summary>
    /// API BFF korumasi icin kullanilan gizli anahtar.
    /// Bos birakilirsa yukleme islemleri yapilandirma hatasi gosterir.
    /// </summary>
    public string Anahtar { get; set; } = "";
}
