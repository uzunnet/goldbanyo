using System;
using System.Collections.Generic;
using System.Linq;
using Desadoor.Ortak.Modeller.Urunler;
using Desadoor.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Desadoor.UI.Pages;

public partial class KatalogIncelemeDialog : ComponentBase
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public List<PdfSayfaGorseli> Sayfalar { get; set; } = new();
    [Parameter] public string KatalogBaslik { get; set; } = "";
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    private string MedyaUrl(long medyaId)
    {
        return $"{Api.ApiBaseUrl}/api/medya-havuzu/dosya/{medyaId}";
    }

    private void Kapat() => MudDialog.Close();
}
