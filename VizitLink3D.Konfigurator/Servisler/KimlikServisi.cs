using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace VizitLink3D.Konfigurator.Servisler;

/// <summary>
/// BFF Cookie Authentication tabanli kimlik dogrulama ve oturum yonetimi.
/// Tum durum sunucu cookie'si ile yonetilir, tarayici tarafinda durum tutulmaz.
/// </summary>
public class KimlikServisi
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiIstemcisi _api;

    public KimlikServisi(IHttpContextAccessor httpContextAccessor, ApiIstemcisi api)
    {
        _httpContextAccessor = httpContextAccessor;
        _api = api;
    }

    /// <summary>
    /// Kullanici adi ve sifre ile harici API uzerinden giris yapar,
    /// basarili olursa cookie tabanli oturum acar.
    /// </summary>
    public async Task<GirisSonuc> GirisYapAsync(string kullaniciAdi, string sifre)
    {
        try
        {
            var yanit = await _api.PostHamAsync<GirisYanitVerisi>("api/kimlik/giris",
                new { KullaniciAdi = kullaniciAdi, Sifre = sifre });

            if (yanit?.BasariliMi != true || yanit.Veri is null)
            {
                return new GirisSonuc
                {
                    Basarili = false,
                    Hata = "Giris basarisiz. Kullanici adi veya sifre hatali."
                };
            }

            var veri = yanit.Veri;

            var kullaniciIdStr = veri.KullaniciId?.ToString(CultureInfo.InvariantCulture) ?? "";

            var talepler = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, kullaniciIdStr),
                new(ClaimTypes.Name, veri.KullaniciAdi ?? ""),
                new("KullaniciId", kullaniciIdStr),
                new("KullaniciAdi", veri.KullaniciAdi ?? ""),
                new(ClaimTypes.Role, veri.Rol ?? ""),
                new("Rol", veri.Rol ?? "")
            };

            var kimlik = new ClaimsIdentity(talepler, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(kimlik);

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    });
            }

            return new GirisSonuc { Basarili = true };
        }
        catch (Exception)
        {
            return new GirisSonuc
            {
                Basarili = false,
                Hata = "Sunucu hatasi. Lutfen tekrar deneyin."
            };
        }
    }

    /// <summary>
    /// Cookie tabanli oturumu sonlandirir.
    /// </summary>
    public async Task CikisYapAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
        catch
        {
            // Oturum kapatma hatasi sessizce gec
        }
    }

    /// <summary>
    /// Kullanicinin giris yapip yapmadigini dondurur.
    /// </summary>
    public async Task<bool> GirisliMiAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Oturum acmis kullanicinin adini claim'lerden okur.
    /// </summary>
    public async Task<string?> KullaniciAdiGetirAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return null;

        return user.FindFirst("KullaniciAdi")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Oturum acmis kullanicinin rolunu claim'lerden okur.
    /// </summary>
    public async Task<string?> RolGetirAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return null;

        return user.FindFirst(ClaimTypes.Role)?.Value
            ?? user.FindFirst("Rol")?.Value;
    }

    /// <summary>
    /// Cookie tabanli auth'da token kullanilmadigi icin null doner.
    /// </summary>
    public async Task<string?> TokenGetirAsync()
    {
        return null;
    }

    /// <summary>
    /// Harici API'nin saglik durumunu kontrol eder.
    /// </summary>
    public async Task<bool> ApiDurumKontrolAsync()
    {
        return await _api.SaglikKontrolAsync();
    }
}

/// <summary>
/// Giris islemi sonucu.
/// </summary>
public class GirisSonuc
{
    public bool Basarili { get; set; }
    public string? Hata { get; set; }
}

/// <summary>
/// Harici API'den donen giris yanit veri modeli.
/// Token icermez — BFF cookie auth kullanilir.
/// </summary>
public class GirisYanitVerisi
{
    public int? KullaniciId { get; set; }
    public string? KullaniciAdi { get; set; }
    public string? Rol { get; set; }
    public string? AdSoyad { get; set; }
    public string? Eposta { get; set; }
}
