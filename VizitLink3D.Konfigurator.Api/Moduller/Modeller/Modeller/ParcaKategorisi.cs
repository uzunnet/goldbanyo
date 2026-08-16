namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;

/// <summary>
/// Firma bazlı yönetilebilir parça kategorisi.
/// ParcaTuru enum'unun yerini alır — her firma kendi kategorilerini ekleyebilir.
/// Banyo dolabı, duş kabini, koltuk vb. için özelleştirilebilir.
/// </summary>
public class ParcaKategorisi
{
    public int Id { get; set; }

    /// <summary>
    /// Multi-tenant izolasyon: Hangi firmaya ait.
    /// null ise sistem genelinde (super admin) kategoridir.
    /// </summary>
    public int? FirmaId { get; set; }

    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public bool AktifMi { get; set; } = true;
    public int SiraNo { get; set; }

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    // Soft delete
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
