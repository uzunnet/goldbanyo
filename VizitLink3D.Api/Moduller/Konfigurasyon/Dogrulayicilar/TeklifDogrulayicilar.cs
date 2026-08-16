using FluentValidation;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;

/// <summary>
/// Teklif isteği oluşturma DTO validasyonu.
/// </summary>
public class TeklifIstegiOlusturDogrulayici : AbstractValidator<TeklifIstegiOlusturDto>
{
    public TeklifIstegiOlusturDogrulayici()
    {
        RuleFor(x => x.MusteriKonfigurasyonuId)
            .GreaterThan(0).WithMessage("Müşteri konfigürasyonu ID zorunludur.");

        RuleFor(x => x.UrunId)
            .GreaterThan(0).WithMessage("Ürün ID zorunludur.");

        RuleFor(x => x.MusteriAdSoyad)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(200).WithMessage("Ad soyad en fazla 200 karakter olabilir.");

        RuleFor(x => x.Eposta)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(200).WithMessage("E-posta en fazla 200 karakter olabilir.");

        RuleFor(x => x.Telefon)
            .MaximumLength(20).WithMessage("Telefon en fazla 20 karakter olabilir.");

        RuleFor(x => x.Not)
            .MaximumLength(2000).WithMessage("Not en fazla 2000 karakter olabilir.");
    }
}
