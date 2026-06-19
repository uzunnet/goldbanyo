using Microsoft.JSInterop;

namespace Desadoor.UI.Servisler;

/// <summary>
/// GSAP ve ScrollTrigger harici animasyon kutuphanelerini sarmalayan Turkce servistir.
/// Anayasa §11 wrapper kurali geregi dogrudan JSRuntime erisimini gizler.
/// </summary>
public class AnimasyonMotoruServisi : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;

    public AnimasyonMotoruServisi(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ScrollAnimasyonlariniBaslatAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("desadoorAnimasyon.baslatScrollAnimasyonlari");
            await _jsRuntime.InvokeVoidAsync("desadoorAnimasyon.smoothScroll");
        }
        catch { /* animasyon başlatılamazsa sayfa işlevselliği etkilenmez */ }
    }

    public async Task SayfaGirisAsync()
    {
        try { await _jsRuntime.InvokeVoidAsync("desadoorAnimasyon.sayfaGirisAnimasyonu"); }
        catch { /* animasyon başlatılamazsa sayfa işlevselliği etkilenmez */ }
    }

    public async Task HeroAnimasyonunuOynatAsync()
    {
        try { await _jsRuntime.InvokeVoidAsync("desadoorAnimasyon.heroAnimasyonunuOynat"); }
        catch { /* animasyon başlatılamazsa sayfa işlevselliği etkilenmez */ }
    }

    public async Task AnimasyonlariYenileAsync()
    {
        try { await _jsRuntime.InvokeVoidAsync("desadoorAnimasyon.yenile"); }
        catch { /* animasyon başlatılamazsa sayfa işlevselliği etkilenmez */ }
    }

    public async ValueTask DisposeAsync() => await Task.CompletedTask;
}
