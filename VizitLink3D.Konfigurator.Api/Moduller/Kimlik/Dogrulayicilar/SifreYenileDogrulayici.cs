using FluentValidation;
using VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dtolar;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Dogrulayicilar;

public class SifreYenileDogrulayici : AbstractValidator<SifreYenileDto>
{
    public SifreYenileDogrulayici()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token bos olamaz.")
            .MaximumLength(512).WithMessage("Token cok uzun.");

        RuleFor(x => x.YeniSifre)
            .NotEmpty().WithMessage("Sifre bos olamaz.")
            .MinimumLength(8).WithMessage("Sifre en az 8 karakter olmalidir.")
            .MaximumLength(128).WithMessage("Sifre en fazla 128 karakter olabilir.")
            .Matches(@"[A-Z]").WithMessage("Sifre en az bir buyuk harf icermelidir.")
            .Matches(@"[a-z]").WithMessage("Sifre en az bir kucuk harf icermelidir.")
            .Matches(@"[0-9]").WithMessage("Sifre en az bir rakam icermelidir.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Sifre en az bir ozel karakter icermelidir.");

        // Defense-in-depth: API seviyesinde şifre-tekrar eşleşme kontrolü.
        // BFF zaten kontrol eder, ancak API'ye doğrudan erişim senaryosuna karşı
        // ek güvenlik katmanı.
        RuleFor(x => x.YeniSifreTekrar)
            .NotEmpty().WithMessage("Sifre tekrar alani bos olamaz.");

        RuleFor(x => x)
            .Must(x => x.YeniSifre == x.YeniSifreTekrar)
            .WithMessage("Sifreler eslesmiyor.")
            .When(x => !string.IsNullOrEmpty(x.YeniSifre) && !string.IsNullOrEmpty(x.YeniSifreTekrar));
    }
}
