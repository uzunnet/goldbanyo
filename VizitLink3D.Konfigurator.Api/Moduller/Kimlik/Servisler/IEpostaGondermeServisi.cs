namespace VizitLink3D.Konfigurator.Api.Moduller.Kimlik.Servisler;

public interface IEpostaGondermeServisi
{
    /// <summary>
    /// E-posta gönderir. Yapılandırma eksikse veya gönderim başarısız olursa
    /// false döner; iç detay sızdırmaz.
    /// </summary>
    Task<bool> EpostaGonderAsync(string aliciEposta, string konu, string govdeHtml);
}
