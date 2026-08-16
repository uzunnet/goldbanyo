namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

/// <summary>
/// Parça kategorisi listeleme ve detay DTO'su.
/// </summary>
public record ParcaKategorisiDto(
    int Id,
    string Ad,
    string? Aciklama,
    bool AktifMi,
    int SiraNo,
    DateTime OlusturulmaTarihi,
    DateTime? GuncellenmeTarihi
);
