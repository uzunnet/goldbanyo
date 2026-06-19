using System;

namespace Desadoor.Ortak.Modeller.Urunler;

public class UrunMedya
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public string MedyaUrl { get; set; } = string.Empty;
    public string MedyaTuru { get; set; } = "Resim";
    public string? Aciklama { get; set; }
    public int SiraNo { get; set; }
    public bool AnaGosterim { get; set; }
}