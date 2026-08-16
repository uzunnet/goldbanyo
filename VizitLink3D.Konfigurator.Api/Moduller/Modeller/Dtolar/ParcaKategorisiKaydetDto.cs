namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

/// <summary>
/// Parça kategorisi oluşturma/güncelleme istek gövdesi.
/// </summary>
public class ParcaKategorisiKaydetDto
{
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public bool AktifMi { get; set; } = true;
    public int SiraNo { get; set; }
}
