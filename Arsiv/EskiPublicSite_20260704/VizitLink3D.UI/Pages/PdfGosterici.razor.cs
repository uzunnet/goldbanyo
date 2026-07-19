using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace VizitLink3D.UI.Pages;

public partial class PdfGosterici : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private NavigationManager Navigasyon { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "dosya")]
    public string? Dosya { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "baslik")]
    public string? Baslik { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "donus")]
    public string? Donus { get; set; }

    private string _pdfUrl = string.Empty;
    private string _baslik = string.Empty;
    private string? _sonUygulananDil;
    private bool _gorselMi;

    protected override void OnParametersSet()
    {
        _pdfUrl = TamUrl(Dosya);
        _baslik = string.IsNullOrWhiteSpace(Baslik)
            ? Dil.T("pdfGosterici.baslik", "PDF Belgesi")
            : Baslik;
        _gorselMi = GorselMi(Dosya);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_gorselMi || string.IsNullOrWhiteSpace(_pdfUrl) || (!firstRender && _sonUygulananDil == Dil.AktifDil))
        {
            return;
        }

        _sonUygulananDil = Dil.AktifDil;
        await Js.InvokeVoidAsync("vizitlink3dPdfDiliUygula", Dil.AktifDil);
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return string.Empty;
        }

        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return yol;
        }

        return $"{Api.ApiBaseUrl}/api/belge-dosya?dosya={Uri.EscapeDataString(yol)}";
    }

    private void GeriDon()
    {
        if (!string.IsNullOrWhiteSpace(Donus))
        {
            Navigasyon.NavigateTo(Donus);
            return;
        }

        Navigasyon.NavigateTo("/sertifikalar");
    }

    private static bool GorselMi(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return false;
        }

        var uzanti = Path.GetExtension(yol);
        return uzanti.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || uzanti.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || uzanti.Equals(".png", StringComparison.OrdinalIgnoreCase);
    }
}

