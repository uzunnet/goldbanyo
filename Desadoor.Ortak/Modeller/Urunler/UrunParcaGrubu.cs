using System;

namespace Desadoor.Ortak.Modeller.Urunler;

public class UrunParcaGrubu
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
}