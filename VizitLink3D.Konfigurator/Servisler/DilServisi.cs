namespace VizitLink3D.Konfigurator.Servisler;

/// <summary>
/// Minimal bagimsiz dil servisi. API/DB/JSON dosya bagimliligi yoktur.
/// Sadece Turkce varsayilan metinleri yedek olarak kullanir.
/// Hardcoded metin yasagina uygun: T(anahtar, yedekMetin) kalibi.
/// </summary>
public class DilServisi
{
    private readonly Dictionary<string, string> _sozluk = [];

    /// <summary>
    /// Ceviri anahtarini sozlukte arar, bulamazsa yedek metni dondurur.
    /// </summary>
    public string T(string anahtar, string yedekMetin = "")
    {
        return _sozluk.TryGetValue(anahtar, out var deger) && !string.IsNullOrEmpty(deger)
            ? deger
            : yedekMetin;
    }

    /// <summary>
    /// Calisma aninda ceviri eklemek icin.
    /// </summary>
    public void Ekle(string anahtar, string deger)
    {
        _sozluk[anahtar] = deger;
    }

    /// <summary>
    /// Toplu ceviri yuklemek icin.
    /// </summary>
    public void TopluEkle(Dictionary<string, string> ceviriler)
    {
        foreach (var (anahtar, deger) in ceviriler)
            _sozluk[anahtar] = deger;
    }
}
