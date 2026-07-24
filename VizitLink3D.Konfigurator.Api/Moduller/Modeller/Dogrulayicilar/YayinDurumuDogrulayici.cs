using FluentValidation;
using VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dogrulayicilar;

/// <summary>
/// Yayın durumu güncelleme DTO'su için FluentValidation kuralları.
/// AktifMi değer tip (bool) olduğu için nullable kontrolüne gerek yoktur;
/// model binding başarısız olursa ASP.NET Core kendi InvalidModelState üretir.
/// </summary>
public class YayinDurumuDogrulayici : AbstractValidator<YayinDurumuDto>
{
    public YayinDurumuDogrulayici()
    {
        // bool değer tip — her zaman geçerli bir değere sahiptir.
        // Gelecekte ek alanlar eklenirse burada doğrulanır.
    }
}
