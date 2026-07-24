namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

/// <summary>
/// Parça metadata güncelleme (PUT) istek gövdesi.
/// Tüm alanlar opsiyonel — sadece gönderilen alanlar güncellenir.
/// </summary>
public class ParcaMetadataGuncelleDto
{
    public string? GorunenAd { get; set; }
    public string? ParcaTuru { get; set; }
    public bool? RenkDegistirilebilirMi { get; set; }
    public bool? GorunurMu { get; set; }
    public string? VarsayilanRenk { get; set; }
    public string? VarsayilanMalzeme { get; set; }
}
