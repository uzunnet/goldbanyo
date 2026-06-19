using Desadoor.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Desadoor.UI.Servisler;

namespace Desadoor.UI.Pages.Admin;

public partial class YorumYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<MusteriYorumu> _liste = [];
    private List<MusteriYorumu> _filtreliListe = [];
    private bool _yukleniyor = true;
    private string _arama = string.Empty;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<MusteriYorumu>>("api/admin/icerik/musteri-yorumlari") ?? [];
        AramaUygula();
        _yukleniyor = false;
    }

    void AramaYap(KeyboardEventArgs e) => AramaUygula();
    void AramaMetniDegisti(string deger) { _arama = deger; AramaUygula(); }

    void AramaUygula()
    {
        var a = _arama?.ToLower() ?? "";
        _filtreliListe = string.IsNullOrWhiteSpace(a)
            ? _liste
            : _liste.Where(x =>
                (x.MusteriAdi?.ToLower().Contains(a) ?? false) ||
                (x.MusteriSehir?.ToLower().Contains(a) ?? false) ||
                (x.Yorum?.ToLower().Contains(a) ?? false)).ToList();
    }

    async Task OnayDegistir(MusteriYorumu y)
    {
        var cevap = await Api.PutAsync<MusteriYorumu>($"api/admin/icerik/musteri-yorumlari/{y.Id}/onay?onay={y.Onaylandi.ToString().ToLower()}", null!);
        if (cevap?.BasariliMi == true)
            Snackbar.Add(cevap.Mesaj, Severity.Info);
        else
            Snackbar.Add(cevap?.Mesaj ?? "Yorum onayi guncellenemedi.", Severity.Error);
    }

    async Task SilOnay(MusteriYorumu y)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{y.MusteriAdi}' yorumu silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(y);
    }

    async Task Sil(MusteriYorumu y)
    {
        var cevap = await Api.DeleteAsync($"api/admin/icerik/musteri-yorumlari/{y.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Yorum silinemedi.", Severity.Error);
        }
    }
}
