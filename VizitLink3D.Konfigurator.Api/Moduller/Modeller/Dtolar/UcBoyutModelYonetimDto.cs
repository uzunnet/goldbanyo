namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

/// <summary>
/// Yönetim paneli model listesi DTO'su.
/// Hassas alanlar (DosyaYolu, Sha256Hash) dışlanmıştır.
/// </summary>
public record UcBoyutModelYonetimDto(
    int Id,
    string Ad,
    string Slug,
    string? Aciklama,
    long BoyutBayt,
    bool AktifMi,
    DateTime OlusturulmaTarihi,
    DateTime? GuncellenmeTarihi
);
