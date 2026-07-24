namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

/// <summary>
/// Yönetim paneli parça listesi DTO'su.
/// Audit alanları dahil, hassas veri yok.
/// </summary>
public record UcBoyutModelParcasiYonetimDto(
    int Id,
    int ModelId,
    string MeshAdi,
    string GorunenAd,
    string ParcaTuru,
    bool RenkDegistirilebilirMi,
    bool GorunurMu,
    string? VarsayilanRenk,
    string? VarsayilanMalzeme,
    DateTime OlusturulmaTarihi,
    DateTime? GuncellenmeTarihi
);
