namespace VizitLink3D.Konfigurator.Api.Moduller.Modeller.Dtolar;

/// <summary>
/// Senkronizasyon sonucu — kaç parça eklendi, geri yüklendi, soft-delete yapıldı.
/// </summary>
public record SenkronizeSonucDto(int Eklenen, int GeriYuklenen, int YumusakSilinen);
