namespace VizitLink3D.Konfigurator.Api.Moduller.Kategoriler.Modeller;

public class Kategori
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int? UstKategoriId { get; set; }
    public Kategori? UstKategori { get; set; }
    public ICollection<Kategori>? AltKategoriler { get; set; }
    public int Sira { get; set; }
    public bool AktifMi { get; set; } = true;
    public bool SilindiMi { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
