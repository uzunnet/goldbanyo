namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Servisler;

/// <summary>
/// Task.Delay soyutlamasi — test edilebilir zamanlama jitter'i icin.
/// Uretimde SystemZamanlayici, testte spy/mock kullanilir.
/// </summary>
public interface IZamanlayici
{
    /// <summary>
    /// Belirtilen sure kadar asenkron gecikme uygular.
    /// </summary>
    Task GecikmeAsync(TimeSpan sure, CancellationToken iptal = default);
}
