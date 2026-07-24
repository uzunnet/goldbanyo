using FluentValidation;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dogrulayicilar;

public class ParcaSenkronizeDogrulayici : AbstractValidator<ParcaSenkronizeKomutu>
{
    public ParcaSenkronizeDogrulayici()
    {
        RuleFor(x => x.MeshAdlari)
            .NotNull().WithMessage("Mesh adları listesi zorunludur.")
            .Must(l => l.Count <= 200).WithMessage("En fazla 200 mesh adı gönderilebilir.");

        RuleForEach(x => x.MeshAdlari)
            .NotEmpty().WithMessage("Mesh adı boş olamaz.")
            .MaximumLength(300).WithMessage("Mesh adı en fazla 300 karakter olabilir.")
            .Must(m => !m.Contains('\0') && !m.Contains("..")).WithMessage("Geçersiz mesh adı.");
    }
}
