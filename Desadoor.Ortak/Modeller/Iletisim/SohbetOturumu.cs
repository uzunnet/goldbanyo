using System;
using System.Collections.Generic;

namespace Desadoor.Ortak.Modeller;

public class SohbetOturumu
{
    public int Id { get; set; }
    public string ZiyaretciId { get; set; } = string.Empty;
    public string? ZiyaretciIsmi { get; set; }
    public DateTime BaslangicZamani { get; set; } = DateTime.UtcNow;
    public DateTime SonIslemZamani { get; set; } = DateTime.UtcNow;
    public bool AktifMi { get; set; } = true;
    public bool YoneticiOkunmadiMi { get; set; } = false;
    public List<SohbetMesaji> Mesajlar { get; set; } = new List<SohbetMesaji>();
}
