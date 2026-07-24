using FluentValidation;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Modeller;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dogrulayicilar;

public class ParcaMetadataGuncelleDogrulayici : AbstractValidator<ParcaMetadataGuncelleDto>
{
    public ParcaMetadataGuncelleDogrulayici()
    {
        When(x => x.GorunenAd is not null, () =>
        {
            RuleFor(x => x.GorunenAd)
                .MaximumLength(300).WithMessage("Görünen ad en fazla 300 karakter olabilir.");
        });

        When(x => x.ParcaTuru is not null, () =>
        {
            RuleFor(x => x.ParcaTuru)
                .Must(pt => Enum.TryParse<ParcaTuru>(pt, true, out _))
                .WithMessage($"Geçersiz parça türü. Geçerli değerler: {string.Join(", ", Enum.GetNames<ParcaTuru>())}");
        });

        When(x => x.VarsayilanRenk is not null, () =>
        {
            RuleFor(x => x.VarsayilanRenk)
                .MaximumLength(9).WithMessage("Renk kodu en fazla 9 karakter olabilir.")
                .Matches(@"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$")
                .WithMessage("Geçersiz hex renk kodu. Format: #RGB, #RRGGBB veya #RRGGBBAA");
        });

        When(x => x.VarsayilanMalzeme is not null, () =>
        {
            RuleFor(x => x.VarsayilanMalzeme)
                .MaximumLength(100).WithMessage("Malzeme adı en fazla 100 karakter olabilir.");
        });
    }
}
