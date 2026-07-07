using Microsoft.AspNetCore.Components;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class Iletisim : ComponentBase, IDisposable
{
    [Inject]
    private ApiIstemcisi ApiIstemcisi { get; set; }

    [Inject]
    private DilServisi DilServisi { get; set; } = default!;

    private string? _adres;
    private string? _email;
    private List<string>? _telefonlar;

    protected override async Task OnInitializedAsync()
    {
        DilServisi.DilDegisti += DilDegistiginde;
        await IcerigiYukleAsync();
    }

    private async void DilDegistiginde()
    {
        await IcerigiYukleAsync();
        StateHasChanged();
    }

    private async Task IcerigiYukleAsync()
    {
        try
        {
            var icerigi = await ApiIstemcisi.GetAsync<Dictionary<string, string>>($"api/sayfa-icerigi/iletisim?dil={DilServisi.AktifDil}");

            if (icerigi != null)
            {
                if (icerigi.TryGetValue("Adres", out var adres))
                    _adres = adres;

                if (icerigi.TryGetValue("Eposta", out var email))
                    _email = email;

                _telefonlar = new List<string>();
                if (icerigi.TryGetValue("Telefon1", out var tel1))
                    _telefonlar.Add(tel1);
                if (icerigi.TryGetValue("Telefon2", out var tel2))
                    _telefonlar.Add(tel2);
                if (icerigi.TryGetValue("Telefon3", out var tel3))
                    _telefonlar.Add(tel3);
            }
        }
        catch
        {
            _adres = "Adres bilgisi yüklenemedi";
            _email = "Email bilgisi yüklenemedi";
            _telefonlar = new List<string> { "Telefon bilgisi yüklenemedi" };
        }
    }

    public void Dispose()
    {
        DilServisi.DilDegisti -= DilDegistiginde;
    }
}
