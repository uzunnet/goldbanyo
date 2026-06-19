using System.ComponentModel.DataAnnotations;

namespace Desadoor.Ortak.Modeller.Istekler;

public class OtomatikCeviriIstegi
{
    [Required]
    public string Metin { get; set; } = string.Empty;

    [Required]
    public string KaynakDil { get; set; } = "tr";

    [Required]
    public string HedefDil { get; set; } = string.Empty;
}
