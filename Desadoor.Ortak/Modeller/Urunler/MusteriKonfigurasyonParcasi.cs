using System;

namespace Desadoor.Ortak.Modeller.Urunler;

public class MusteriKonfigurasyonParcasi
{
    public int Id { get; set; }
    public int MusteriKonfigurasyonuId { get; set; }
    public int UrunUcBoyutParcasiId { get; set; }
    public int? SeciliRenkId { get; set; }
    public int? SeciliMalzemeId { get; set; }
    public int? SeciliKaplamaId { get; set; }
    public double? Deger { get; set; }
    public bool GorunurMu { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
