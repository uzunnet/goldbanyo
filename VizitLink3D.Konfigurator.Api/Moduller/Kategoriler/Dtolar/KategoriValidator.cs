using FluentValidation;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kategoriler.Dtolar;

public class KategoriOlusturValidator : AbstractValidator<KategoriOlusturDto>
{
    public KategoriOlusturValidator()
    {
        RuleFor(x => x.Ad)
            .NotEmpty().WithMessage("Kategori adı zorunludur.")
            .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olabilir.");
    }
}

public class KategoriGuncelleValidator : AbstractValidator<KategoriGuncelleDto>
{
    public KategoriGuncelleValidator()
    {
        RuleFor(x => x.Ad)
            .NotEmpty().WithMessage("Kategori adı zorunludur.")
            .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olabilir.");
    }
}
