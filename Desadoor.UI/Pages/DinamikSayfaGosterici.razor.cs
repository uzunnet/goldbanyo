using Microsoft.AspNetCore.Components;
using Desadoor.UI.Models;

namespace Desadoor.UI.Pages
{
    /// <summary>
    /// Dinamik sayfa göstericisi — slug parametresine göre API'den sayfa içeriğini çeker.
    /// Hakkımızda, İletişim, Lake Kapak, Membran Kapak gibi tüm içerik sayfaları bu bileşenden render edilir.
    /// api servisi _Imports.razor üzerinden global olarak inject edilmiştir.
    /// </summary>
    public partial class DinamikSayfaGosterici : ComponentBase, IDisposable
    {
        [Parameter] public required string Slug { get; set; }

        private bool _yukleniyor = true;
        private bool _sayfaBulunamadi = false;
        private string _sayfaBasligi = "";
        private string _sayfaIcerigi = "";
        
        // Medya
        private string _videoUrl = "";
        private string _youtubeUrl = "";
        private string _pdfKatalogUrl = "";
        private bool _videoMute = true;
        private string _videoHiz = "1.0";

        protected override async Task OnParametersSetAsync()
        {
            _yukleniyor = true;
            _sayfaBulunamadi = false;
            StateHasChanged();

            try
            {
                var yanit = await api.GetAsync<Dictionary<string, string>>($"api/desadoor/sayfa-icerigi/{Slug.ToLower()}?dil={dil.AktifDil}");

                if (yanit != null && yanit.Count > 0)
                {
                    var dict = yanit;
                    _sayfaBasligi = dict.GetValueOrDefault("SayfaBasligi", Slug.ToUpper());
                    _sayfaIcerigi = dict.GetValueOrDefault("SayfaIcerigi", "");
                    
                    _videoUrl = dict.GetValueOrDefault("VideoUrl", "");
                    _youtubeUrl = dict.GetValueOrDefault("YoutubeUrl", "");
                    _pdfKatalogUrl = dict.GetValueOrDefault("PdfKatalogUrl", "");
                    _videoMute = dict.GetValueOrDefault("VideoMute", "true") == "true";
                    _videoHiz = dict.GetValueOrDefault("VideoHiz", "1.0");
                }
                else
                {
                    _sayfaBulunamadi = true;
                }
            }
            catch
            {
                _sayfaBulunamadi = true;
            }
            finally
            {
                _yukleniyor = false;
            }
        }

        protected override void OnInitialized()
        {
            dil.DilDegisti += OnDilDegisti;
        }

        private async void OnDilDegisti()
        {
            await OnParametersSetAsync();
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            dil.DilDegisti -= OnDilDegisti;
        }
    }
}
