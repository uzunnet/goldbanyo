namespace VizitLink3D.Konfigurator.Servisler;

/// <summary>
/// Uygulama genel konfigurasyon sinifi — Options pattern ile DI'a kaydedilir.
/// appsettings.json -> IOptions&lt;UygulamaAyarlari&gt; uzerinden okunur.
/// </summary>
public class UygulamaAyarlari
{
    public const string BolumAdi = "UygulamaAyarlari";

    public int Port { get; set; } = 5114;
    public string Proje { get; set; } = "VizitLink3D.Konfigurator (Bagimsiz)";
    public string Versiyon { get; set; } = "P04";
    public string Aciklama { get; set; } = "VizitLink3D Studio — bagimsiz 3D SaaS uygulamasi";
}

/// <summary>
/// API baglantisi konfigurasyonu.
/// </summary>
public class ApiAyarlari
{
    public const string BolumAdi = "ApiAyarlari";

    public string BaseUrl { get; set; } = "";
}
