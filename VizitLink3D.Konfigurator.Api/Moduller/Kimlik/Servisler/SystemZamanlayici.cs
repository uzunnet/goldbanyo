namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Servisler;

/// <summary>
/// IZamanlayici'nin uretim implementasyonu — Task.Delay sarmalar.
/// </summary>
public class SystemZamanlayici : IZamanlayici
{
    public Task GecikmeAsync(TimeSpan sure, CancellationToken iptal = default)
        => Task.Delay(sure, iptal);
}
