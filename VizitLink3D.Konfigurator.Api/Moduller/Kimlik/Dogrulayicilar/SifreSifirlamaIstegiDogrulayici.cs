using FluentValidation;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dogrulayicilar;

public class SifreSifirlamaIstegiDogrulayici : AbstractValidator<SifreSifirlamaIstegiDto>
{
    public SifreSifirlamaIstegiDogrulayici()
    {
        RuleFor(x => x.Eposta)
            .NotEmpty().WithMessage("E-posta adresi bos olamaz.")
            .EmailAddress().WithMessage("Gecerli bir e-posta adresi giriniz.")
            .MaximumLength(256).WithMessage("E-posta adresi en fazla 256 karakter olabilir.");
    }
}
