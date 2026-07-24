using FluentValidation;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dogrulayicilar;

public record UcBoyutModelYukleKomutu(string Ad, string? Aciklama, IFormFile Dosya);

public class UcBoyutModelYukleDogrulayici : AbstractValidator<UcBoyutModelYukleKomutu>
{
    public UcBoyutModelYukleDogrulayici()
    {
        RuleFor(x => x.Ad)
            .NotEmpty().WithMessage("Model adı zorunludur.")
            .MaximumLength(200).WithMessage("Model adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Aciklama)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.Dosya)
            .NotNull().WithMessage("Dosya zorunludur.")
            .Must(DosyaGecerliExtension).WithMessage("Sadece .glb uzantılı dosyalar kabul edilir.")
            .Must(DosyaBoyutuGecerli).WithMessage("Dosya boyutu izin verilen sınırı aşıyor.");

        // Güvenlik: Path traversal saldırısına karşı dosya adı temizleme
        RuleFor(x => x.Dosya)
            .Must(d => d is null || !DosyaAdindaYolGecisiVar(d.FileName))
            .WithMessage("Geçersiz dosya adı.");
    }

    /// <summary>
    /// Dosya boyutu sınırını appsettings.json GlbYukleme:MaxDosyaBoyutuMb değerinden okur.
    /// Validator DI üzerinden IConfiguration almadığı için sabit değer kullanılır;
    /// asıl boyut kontrolü controller'da yapılır. Bu kural ek güvenlik katmanıdır.
    /// </summary>
    private static bool DosyaBoyutuGecerli(IFormFile? dosya)
    {
        if (dosya is null) return true; // null kontrolü üst kuralda yapılır

        // 100 MB üst sınır — controller'daki config tabanlı sınırdan bağımsız ek koruma
        const long maksimumBayt = 100L * 1024L * 1024L;
        return dosya.Length <= maksimumBayt;
    }

    private static bool DosyaGecerliExtension(IFormFile? dosya)
    {
        if (dosya is null) return false;
        var uzanti = Path.GetExtension(dosya.FileName);
        return string.Equals(uzanti, ".glb", StringComparison.OrdinalIgnoreCase);
    }

    private static bool DosyaAdindaYolGecisiVar(string dosyaAdi)
    {
        if (string.IsNullOrWhiteSpace(dosyaAdi)) return false;
        return dosyaAdi.Contains("..") ||
               dosyaAdi.Contains('/') ||
               dosyaAdi.Contains('\\') ||
               dosyaAdi.Contains(':');
    }
}
