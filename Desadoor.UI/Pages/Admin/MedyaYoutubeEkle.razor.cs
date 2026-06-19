using Desadoor.Ortak.Modeller.Medya;
using Desadoor.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Desadoor.UI.Pages.Admin;

public partial class MedyaYoutubeEkle : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    private string _url = "";
    private string? _baslik;
    private string? _videoId;
    private string? _sonucMesaj;
    private bool _yukleniyor;
    private bool _basarili;

    private async Task Ekle()
    {
        if (string.IsNullOrWhiteSpace(_url))
        {
            _sonucMesaj = dil.T("medya.urlGerekli", "YouTube URL gerekli");
            _basarili = false;
            return;
        }

        _yukleniyor = true;
        var yanit = await Api.PostAsync<Ortak.Modeller.Medya.Medya>("api/medya/youtube", new
        {
            url = _url,
            baslik = _baslik
        });

        if (yanit?.Veri != null)
        {
            _sonucMesaj = dil.T("medya.youtubeEklendi", "YouTube videosu eklendi");
            _basarili = true;
            _videoId = YoutubeIdCikar(_url);
            snackbar.Add(_sonucMesaj, Severity.Success);
        }
        else
        {
            _sonucMesaj = dil.T("medya.youtubeHata", "Video eklenemedi");
            _basarili = false;
        }

        _yukleniyor = false;
    }

    private static string? YoutubeIdCikar(string url)
    {
        if (url.Contains("youtube.com/watch"))
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }
        if (url.Contains("youtu.be/"))
            return url[(url.LastIndexOf('/') + 1)..].Split('?')[0];
        if (url.Contains("youtube.com/embed/"))
            return url[(url.LastIndexOf('/') + 1)..].Split('?')[0];
        return null;
    }
}
