using System;
using System.Text.Json.Serialization;

namespace Desadoor.Ortak.Modeller.Urunler;

public class UrunParcaEslemesi
{
    public int Id { get; set; }
    public int UrunUcBoyutModeliId { get; set; }
    public int UrunUcBoyutParcasiId { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    [JsonIgnore]
    public UrunUcBoyutModeli? UrunUcBoyutModeli { get; set; }

    [JsonIgnore]
    public UrunUcBoyutParcasi? UrunUcBoyutParcasi { get; set; }
}