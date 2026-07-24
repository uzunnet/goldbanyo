namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

public record UcBoyutModelDto(
    int Id,
    string Ad,
    string Slug,
    string? Aciklama,
    string DosyaAdi,
    string IcerikTuru,
    long BoyutBayt,
    DateTime OlusturulmaTarihi,
    List<UcBoyutModelParcasiDto> Parcalar
);
