namespace Desadoor.Api.AraYazilimlar;

/// <summary>
/// Guvenlik header'lari middleware'i (anayasa §3.2, §17).
/// HSTS, X-Frame-Options, X-Content-Type-Options, CSP ve
/// Referrer-Policy header'larini otomatik ekler.
/// Sadece production ortaminda aktif olur.
/// </summary>
public class GuvenlikHeaderlariMiddleware(RequestDelegate sonraki)
{
    public async Task InvokeAsync(HttpContext baglam)
    {
        // Production guvenlik header'lari (anayasa §3.2)
        if (!baglam.Response.Headers.ContainsKey("X-Content-Type-Options"))
            baglam.Response.Headers["X-Content-Type-Options"] = "nosniff";

        if (!baglam.Response.Headers.ContainsKey("X-Frame-Options"))
            baglam.Response.Headers["X-Frame-Options"] = "DENY";

        if (!baglam.Response.Headers.ContainsKey("Referrer-Policy"))
            baglam.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        if (!baglam.Response.Headers.ContainsKey("X-XSS-Protection"))
            baglam.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Permissions-Policy: gereksiz API'leri kisitla
        if (!baglam.Response.Headers.ContainsKey("Permissions-Policy"))
            baglam.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // Content-Security-Policy: Blazor WASM + MudBlazor + Google Fonts uyumlu
        if (!baglam.Response.Headers.ContainsKey("Content-Security-Policy"))
            baglam.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' blob: https://cdnjs.cloudflare.com; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "img-src 'self' data: blob: https://images.unsplash.com https://*.unsplash.com https://desadoor.com.tr https://*.desadoor.com.tr; " +
                "connect-src 'self' blob: data: http: https: ws: wss: http://localhost:* ws://localhost:* wss://localhost:*; " +
                "frame-src 'self' https://www.youtube.com https://www.youtube-nocookie.com https://youtube.com https://youtube-nocookie.com https://www.google.com; " +
                "frame-ancestors 'self'; " +
                "worker-src 'self' blob:; " +
                "media-src 'self' blob:;";

        await sonraki(baglam);
    }
}
