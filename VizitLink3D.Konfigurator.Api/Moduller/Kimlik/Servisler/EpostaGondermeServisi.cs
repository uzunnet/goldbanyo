using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Servisler;

/// <summary>
/// MailKit v4.17 uzerinden e-posta gonderimi.
/// Her EpostaGonderAsync cagrisinda lokal SmtpClient olusturulur;
/// shared instance field YOKTUR. IAsyncDisposable implemente EDILMEZ.
/// DI lifetime: Scoped (durumsuz — instance field yok).
/// TLS: SecureSocketOptions.StartTls ile zorunlu sifreli baglanti.
/// </summary>
public class EpostaGondermeServisi : IEpostaGondermeServisi
{
    private readonly IConfiguration _yapilandirma;
    private readonly ILogger<EpostaGondermeServisi> _logger;

    public EpostaGondermeServisi(IConfiguration yapilandirma, ILogger<EpostaGondermeServisi> logger)
    {
        _yapilandirma = yapilandirma;
        _logger = logger;
    }

    public async Task<bool> EpostaGonderAsync(string aliciEposta, string konu, string govdeHtml)
    {
        var sunucu = _yapilandirma["Eposta:Sunucu"];
        var portStr = _yapilandirma["Eposta:Port"];
        var kullaniciAdi = _yapilandirma["Eposta:KullaniciAdi"];
        var appSifresi = _yapilandirma["Eposta:AppSifresi"];
        var gonderenAdres = _yapilandirma["Eposta:GonderenAdres"];

        // Yapilandirma eksikse sessizce false don
        if (string.IsNullOrWhiteSpace(sunucu) ||
            string.IsNullOrWhiteSpace(portStr) ||
            string.IsNullOrWhiteSpace(kullaniciAdi) ||
            string.IsNullOrWhiteSpace(appSifresi) ||
            string.IsNullOrWhiteSpace(gonderenAdres))
        {
            _logger.LogDebug("E-posta yapilandirmasi eksik, gonderim atlandi.");
            return false;
        }

        if (!int.TryParse(portStr, out var port))
        {
            _logger.LogDebug("E-posta port gecersiz, gonderim atlandi.");
            return false;
        }

        try
        {
            var eposta = new MimeMessage();
            eposta.From.Add(new MailboxAddress("Konfigurator", gonderenAdres));
            eposta.To.Add(new MailboxAddress("", aliciEposta));
            eposta.Subject = konu;

            var govde = new BodyBuilder { HtmlBody = govdeHtml };
            eposta.Body = govde.ToMessageBody();

            // ── Lokal SmtpClient: her cagrida yeni olusturulur ──
            using var istemci = new SmtpClient();

            // MailKit v4.17: SecureSocketOptions.StartTls = firsatci degil ZORUNLU TLS
            await istemci.ConnectAsync(sunucu, port, SecureSocketOptions.StartTls);
            await istemci.AuthenticateAsync(kullaniciAdi, appSifresi);
            await istemci.SendAsync(eposta);

            // Explicit QUIT + disconnect
            await istemci.DisconnectAsync(true);

            _logger.LogDebug("E-posta basariyla gonderildi.");
            return true;
        }
        catch (Exception ex)
        {
            // Ic detay sizdirmaz; sadece Debug log
            _logger.LogDebug("E-posta gonderim hatasi: {HataTipi}", ex.GetType().Name);
            return false;
        }
    }
}
