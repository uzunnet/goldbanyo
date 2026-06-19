using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Bilesenler.Medya;

public partial class MedyaDuzenleyici : ComponentBase
{
    [Parameter] public string MedyaUrl { get; set; } = string.Empty;

    private void Dondur()
    {
        // Yer tutucu — ileride Cropper.js entegrasyonu ile değiştirilecek
    }

    private void Kirp()
    {
        // Yer tutucu — ileride Cropper.js entegrasyonu ile değiştirilecek
    }
}
