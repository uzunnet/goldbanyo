using System;

namespace Desadoor.Ortak.Modeller.Urunler;

public class MusteriKonfigurasyonu
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public int? KullaniciId { get; set; }
    public string? OturumAnahtari { get; set; }
    public string? Not { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
