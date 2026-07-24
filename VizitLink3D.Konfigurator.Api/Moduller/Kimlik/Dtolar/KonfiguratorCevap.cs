namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;

public class KonfiguratorCevap<T>
{
    public bool BasariliMi { get; set; }
    public string Mesaj { get; set; } = string.Empty;
    public List<string> Hatalar { get; set; } = [];
    public T? Veri { get; set; }

    public static KonfiguratorCevap<T> Basarili(T veri, string mesaj = "Islem basarili.") =>
        new() { BasariliMi = true, Mesaj = mesaj, Veri = veri };

    public static KonfiguratorCevap<T> Hata(string mesaj, List<string>? hatalar = null) =>
        new() { BasariliMi = false, Mesaj = mesaj, Hatalar = hatalar ?? [] };
}
