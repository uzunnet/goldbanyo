namespace VizitLink3D.Konfigurator.Api.Moduller.Kategoriler.Dtolar;

public record KategoriDto(
    int Id,
    string Ad,
    string Slug,
    string? Aciklama,
    int? UstKategoriId,
    int Sira,
    bool AktifMi,
    List<KategoriDto>? AltKategoriler
);

public record KategoriOlusturDto(string Ad, string? Aciklama, int? UstKategoriId, int Sira);

public record KategoriGuncelleDto(string Ad, string? Aciklama, int? UstKategoriId, int Sira, bool AktifMi);
