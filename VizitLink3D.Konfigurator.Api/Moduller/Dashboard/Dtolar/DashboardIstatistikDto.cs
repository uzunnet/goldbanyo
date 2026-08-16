namespace VizitLink3D.Konfigurator.Api.Moduller.Dashboard.Dtolar;

/// <summary>
/// Dashboard istatistik yanit DTO'su.
/// Toplam model, aktif model, parca sayisi ve son eklenen modelleri icerir.
/// </summary>
public class DashboardIstatistikDto
{
    public int ToplamModelSayisi { get; set; }
    public int AktifModelSayisi { get; set; }
    public int ToplamParcaSayisi { get; set; }
    public List<SonModelDto> SonEklenenModeller { get; set; } = [];
}

/// <summary>
/// Dashboard'da gosterilecek son eklenen model ozeti (en fazla 5 kayit).
/// </summary>
public class SonModelDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public long BoyutBayt { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
    public bool AktifMi { get; set; }
}
