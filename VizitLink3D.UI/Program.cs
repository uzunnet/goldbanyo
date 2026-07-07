using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using VizitLink3D.UI;
using VizitLink3D.UI.Servisler;
using Gotho.BlazorPdf;
using MudBlazor.Extensions;
using MudExtensions.Services;
using MudBlazor.Services;

var yapici = WebAssemblyHostBuilder.CreateDefault(args);
yapici.RootComponents.Add<App>("#app");
yapici.RootComponents.Add<HeadOutlet>("head::after");

var temelAdres = yapici.HostEnvironment.BaseAddress;
var yapilandirmaAdresi = yapici.Configuration["ApiTemelUrl"];
string apiUrl;

if (temelAdres.Contains("localhost") || temelAdres.Contains("127.0.0.1"))
{
    apiUrl = !string.IsNullOrEmpty(yapilandirmaAdresi) ? yapilandirmaAdresi : "http://localhost:5115";
}
else
{
    if (string.IsNullOrEmpty(yapilandirmaAdresi) || yapilandirmaAdresi.Contains("localhost") || yapilandirmaAdresi.Contains("127.0.0.1"))
    {
        apiUrl = temelAdres.TrimEnd('/') + "/";
    }
    else
    {
        apiUrl = yapilandirmaAdresi;
    }
}

yapici.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(yapici.HostEnvironment.BaseAddress) });
yapici.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

yapici.Services.AddScoped<ApiIstemcisi>(sp =>
{
    var istemci = new HttpClient { BaseAddress = new Uri(apiUrl) };
    return new ApiIstemcisi(istemci, sp.GetRequiredService<IJSRuntime>());
});

yapici.Services.AddMudServices();
yapici.Services.AddMudExtensions();
yapici.Services.AddBlazorPdfViewer();
yapici.Services.AddValidation();

yapici.Services.AddScoped<DilServisi>();
yapici.Services.AddScoped<KimlikServisi>();
yapici.Services.AddScoped<FirmaBilgisiServisi>();
yapici.Services.AddScoped<UcBoyutServisi>();
yapici.Services.AddScoped<AnimasyonMotoruServisi>();
yapici.Services.AddScoped<BildirimServisi>();
yapici.Services.AddScoped<AdminCeviriServisi>();

await yapici.Build().RunAsync();
