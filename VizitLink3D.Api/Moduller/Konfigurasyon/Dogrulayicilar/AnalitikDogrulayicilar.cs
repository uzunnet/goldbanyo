using FluentValidation;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;

/// <summary>
/// Analytics olay kayıt komutu validasyonu.
/// </summary>
public class OlayKaydetDogrulayici : AbstractValidator<Komutlar.AnalitikKomutlari.OlayKaydetKomutu>
{
    public OlayKaydetDogrulayici()
    {
        RuleFor(x => x.OturumAnahtari)
            .NotEmpty().WithMessage("Oturum anahtarı zorunludur.")
            .MaximumLength(100);

        RuleFor(x => x.OlayTipi)
            .NotEmpty().WithMessage("Olay tipi zorunludur.")
            .MaximumLength(100);
    }
}
