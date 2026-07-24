namespace VizitLink3D.Konfigurator.Servisler;

/// <summary>
/// Public 3D viewer icin guvenli model listesi ogesi DTO'su.
/// DosyaYolu, Sha256Hash gibi hassas alanlari icermez.
/// ModelUrl BFF proxy uzerinden guvenli GLB erisimi saglar.
/// </summary>
public class PublicModelListeOgesiDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Aciklama { get; set; }
    public string DosyaAdi { get; set; } = "";
    public long BoyutBayt { get; set; }
    public string? KapsulResimUrl { get; set; }
    public string ModelUrl { get; set; } = "";
    public DateTime OlusturulmaTarihi { get; set; }
}

/// <summary>
/// Public 3D viewer icin guvenli model detay DTO'su.
/// DosyaYolu, Sha256Hash gibi hassas alanlari icermez.
/// ModelUrl BFF proxy uzerinden guvenli GLB erisimi saglar.
/// </summary>
public class PublicModelDetayDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Aciklama { get; set; }
    public string DosyaAdi { get; set; } = "";
    public string IcerikTuru { get; set; } = "model/gltf-binary";
    public long BoyutBayt { get; set; }
    public string ModelUrl { get; set; } = "";
    public DateTime OlusturulmaTarihi { get; set; }
}
