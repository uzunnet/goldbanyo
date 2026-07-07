using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class CeviriYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;

    private List<Ceviri> _liste = [];
    private List<Ceviri> _filtreliListe = [];
    private List<Dil> _tumDiller = [];
    private List<string> _diller = [];
    private List<string> _bolumler = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _kaydediliyor;
    private bool _aiCeviriliyor;
    private int _aiCeviriSayac;
    private int _aiCeviriToplam;
    private Ceviri _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private string? _filtreDil;
    private string? _filtreBolum;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<Ceviri>>("api/dil/admin/tum-ceviriler") ?? [];
        _diller = _liste.Select(x => x.Dil).Distinct().OrderBy(x => x).ToList();
        _bolumler = _liste.Select(x => x.Bolum).Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x).Cast<string>().ToList();
        _tumDiller = await Api.GetAsync<List<Dil>>("api/dil/admin/diller") ?? [];
        FiltreUygula(null);
        _yukleniyor = false;
    }

    async Task DilAktifDegistir(Dil d, bool yeniDurum)
    {
        d.AktifMi = yeniDurum;
        await Api.PutAsync<object>($"api/dil/admin/dil/{d.Id}", new { d.AktifMi, d.SiraNo });
        Snackbar.Add($"{d.Ad} {(d.AktifMi ? "aktif" : "pasif")} edildi.", Severity.Success);
    }

    void AramaYap(KeyboardEventArgs e) => FiltreUygula(null);

    void FiltreDilDegisti(string? yeniDeger)
    {
        _filtreDil = yeniDeger;
        FiltreUygula(null);
    }

    void FiltreBolumDegisti(string? yeniDeger)
    {
        _filtreBolum = yeniDeger;
        FiltreUygula(null);
    }

    void FiltreUygula(string? _)
    {
        var a = _arama?.ToLower() ?? "";
        var sorgu = _liste.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(a))
            sorgu = sorgu.Where(x =>
                (x.Anahtar?.ToLower().Contains(a) ?? false) ||
                (x.Deger?.ToLower().Contains(a) ?? false) ||
                (x.Bolum?.ToLower().Contains(a) ?? false));

        if (!string.IsNullOrWhiteSpace(_filtreDil))
            sorgu = sorgu.Where(x => x.Dil == _filtreDil);

        if (!string.IsNullOrWhiteSpace(_filtreBolum))
            sorgu = sorgu.Where(x => x.Bolum == _filtreBolum);

        _filtreliListe = sorgu.ToList();
    }

    void Duzenle(Ceviri c)
    {
        _form = new Ceviri
        {
            Id = c.Id,
            Anahtar = c.Anahtar,
            Dil = c.Dil,
            Deger = c.Deger,
            Bolum = c.Bolum
        };
        _duzenlenenId = c.Id;
        _formAcik = true;
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        try
        {
            await Api.PutAsync<Ceviri>("api/dil/admin/ceviri", _form);
            _formAcik = false;
            Snackbar.Add("Çeviri güncellendi.", Severity.Success);
            await Yukle();
        }
        catch (Exception ex) { Snackbar.Add($"Hata: {ex.Message}", Severity.Error); }
        finally { _kaydediliyor = false; }
    }

    void FormIptal() { _formAcik = false; }

    /// <summary>
    /// Secili dil ve bolumdeki eksik cevirileri AI ile tamamlar.
    /// TR anahtari varsa EN karsiligini AI uretip kaydeder.
    /// </summary>
    async Task AIileTopluCevir()
    {
        if (_aiCeviriliyor) return;
        _aiCeviriliyor = true;
        _aiCeviriSayac = 0;

        // TR'de olup EN'de eksik olanlari bul
        var trAnahtarlar = _liste.Where(c => c.Dil == "tr").Select(c => c.Anahtar).ToHashSet();
        var enAnahtarlar = _liste.Where(c => c.Dil == "en").Select(c => c.Anahtar).ToHashSet();
        var eksikler = _liste.Where(c => c.Dil == "tr" && !string.IsNullOrWhiteSpace(c.Deger) && !enAnahtarlar.Contains(c.Anahtar)).Take(20).ToList();
        _aiCeviriToplam = eksikler.Count;

        if (_aiCeviriToplam == 0)
        {
            Snackbar.Add("Eksik ceviri bulunamadi.", Severity.Info);
            _aiCeviriliyor = false;
            return;
        }

        foreach (var c in eksikler)
        {
            _aiCeviriSayac++;
            StateHasChanged();

            var ceviri = await Dil.AICeviriAlAsync(c.Anahtar, c.Deger, "en");
            if (ceviri != null)
            {
                // DB'ye kalici kaydet
                await Api.PostAsync<object>("api/dil/ceviri-ekle", new { Anahtar = c.Anahtar, Dil = "en", Deger = ceviri });
            }
            await Task.Delay(200); // LLM'yi bogmamak icin
        }

        await Yukle();
        _aiCeviriliyor = false;
        Snackbar.Add($"{_aiCeviriToplam} ceviri AI ile tamamlandi.", Severity.Success);
    }
}
