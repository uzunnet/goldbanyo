using FluentValidation;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dogrulayicilar;

public class GirisDtoDogrulayici : AbstractValidator<GirisDto>
{
    public GirisDtoDogrulayici()
    {
        RuleFor(x => x.KullaniciAdi)
            .NotEmpty().WithMessage("Kullanici adi bos olamaz.")
            .MaximumLength(50).WithMessage("Kullanici adi en fazla 50 karakter olabilir.");

        RuleFor(x => x.Sifre)
            .NotEmpty().WithMessage("Sifre bos olamaz.")
            .MinimumLength(6).WithMessage("Sifre en az 6 karakter olmalidir.")
            .MaximumLength(100).WithMessage("Sifre en fazla 100 karakter olabilir.");
    }
}
