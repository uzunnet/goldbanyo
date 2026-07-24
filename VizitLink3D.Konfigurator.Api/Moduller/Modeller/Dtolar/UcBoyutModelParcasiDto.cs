namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

/// <summary>
/// Halka açık model detayında dönen parça bilgisi.
/// Audit alanları ve internal ID'ler dışlanmıştır.
/// </summary>
public record UcBoyutModelParcasiDto(
    int Id,
    string MeshAdi,
    string GorunenAd,
    string ParcaTuru,
    bool RenkDegistirilebilirMi,
    bool GorunurMu,
    string? VarsayilanRenk,
    string? VarsayilanMalzeme
);
