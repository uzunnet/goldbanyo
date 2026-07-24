namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

public record UcBoyutModelOzetDto(
    int Id,
    string Ad,
    string Slug,
    string? Aciklama,
    string DosyaAdi,
    long BoyutBayt,
    DateTime OlusturulmaTarihi
);
