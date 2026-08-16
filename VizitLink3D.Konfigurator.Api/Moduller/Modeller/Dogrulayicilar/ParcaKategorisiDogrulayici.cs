using FluentValidation;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dogrulayicilar;

public class ParcaKategorisiDogrulayici : AbstractValidator<ParcaKategorisiKaydetDto>
{
    public ParcaKategorisiDogrulayici()
    {
        RuleFor(x => x.Ad)
            .NotEmpty().WithMessage("Kategori adı zorunludur.")
            .MaximumLength(200).WithMessage("Kategori adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Aciklama)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.SiraNo)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra numarası 0 veya daha büyük olmalıdır.");
    }
}
