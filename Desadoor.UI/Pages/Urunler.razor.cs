using Desadoor.Ortak.Modeller.Urunler;
using Desadoor.UI.Models;
using Microsoft.AspNetCore.Components;

namespace Desadoor.UI.Pages;

public partial class Urunler : ComponentBase, IDisposable
{
    private List<Urun> _urunler = [];
    private List<UrunAilesi> _urunAileleri = [];
    private List<UrunKategori> _kategoriler = [];
    private bool _yukleniyor = true;
    private string _arama = "";
    private int? _seciliUrunAilesiId;
    private int? _seciliKategoriId;
    private int _sayfa = 1;
    private int _sayfaBoyutu = 12;
    private int _sutunAdet = 4;
    private int _toplamSayfa => Math.Max(1, (int)Math.Ceiling(_urunler.Count / (double)_sayfaBoyutu));
    private string? _hataMesaji;

    private int MudItemMd => 12 / Math.Min(_sutunAdet, 4);
    private int MudItemLg => 12 / Math.Min(_sutunAdet, 6);

    protected override async Task OnInitializedAsync()
    {
        dil.DilDegisti += OnDilDegisti;
        await Task.WhenAll(
            DuzenAyariYukleAsync(),
            FiltreleriYukleAsync(),
            UrunleriYukleAsync()
        );
    }

    private async Task DuzenAyariYukleAsync()
    {
        var ayar = await api.GetAsync<SayfaDuzenAyariDto>("api/desadoor/sayfa-duzen-ayarlari/urunler");
        if (ayar != null)
        {
            _sutunAdet = ayar.SutunAdet > 0 ? ayar.SutunAdet : 4;
            _sayfaBoyutu = ayar.SayfaBasinaAdet > 0 ? ayar.SayfaBasinaAdet : 12;
        }
    }

    private async Task FiltreleriYukleAsync()
    {
        try
        {
            var aileSonuc = await api.GetAsync<List<UrunAilesi>>("api/urun-ailesi");
            if (aileSonuc != null)
                _urunAileleri = aileSonuc;
        }
        catch { /* ürün aileleri yüklenemezse filtre boş kalır */ }

        try
        {
            var kategoriSonuc = await api.GetAsync<List<UrunKategori>>("api/urun-kategorileri");
            if (kategoriSonuc != null)
                _kategoriler = kategoriSonuc;
        }
        catch { /* kategoriler yüklenemezse filtre boş kalır */ }
    }

    private async Task UrunleriYukleAsync()
    {
        _yukleniyor = true;
        _hataMesaji = null;
        StateHasChanged();

        try
        {
            var sorguParcalari = new List<string> { $"dil={dil.AktifDil}" };
            if (_seciliKategoriId.HasValue)
                sorguParcalari.Add($"kategoriId={_seciliKategoriId}");
            if (_seciliUrunAilesiId.HasValue)
                sorguParcalari.Add($"urunAilesiId={_seciliUrunAilesiId}");
            if (!string.IsNullOrWhiteSpace(_arama))
                sorguParcalari.Add($"arama={Uri.EscapeDataString(_arama)}");

            var sorgu = "?" + string.Join("&", sorguParcalari);
            var sonuc = await api.GetAsync<List<Urun>>($"api/urunler{sorgu}");
            _urunler = sonuc ?? [];
        }
        catch (Exception ex)
        {
            _hataMesaji = $"Hata: {ex.Message}";
            Console.Error.WriteLine($"[Urunler] Urun yukleme hatasi: {ex}");
            _urunler = [];
        }
        finally
        {
            _sayfa = 1;
            _yukleniyor = false;
        }
    }

    private IEnumerable<Urun> SayfaliUrunler()
    {
        return _urunler
            .Skip((_sayfa - 1) * _sayfaBoyutu)
            .Take(_sayfaBoyutu);
    }

    private async Task UrunAilesiDegisti(int? deger)
    {
        _seciliUrunAilesiId = deger;
        await UrunleriYukleAsync();
    }

    private async Task KategoriDegisti(int? deger)
    {
        _seciliKategoriId = deger;
        await UrunleriYukleAsync();
    }

    private async Task AramaDegisti(string deger)
    {
        _arama = deger;
        await UrunleriYukleAsync();
    }

    private async Task SayfaDegisti(int sayfa)
    {
        _sayfa = sayfa;
        await Task.CompletedTask;
    }

    private void UrunDetayinaGit(string slug)
    {
        nav.NavigateTo($"/urun/{slug}");
    }

    private void PopupAc(Urun urun)
    {
        nav.NavigateTo($"/urun/{urun.Slug}");
    }

    private async void OnDilDegisti()
    {
        var filtreGorevi = FiltreleriYukleAsync();
        var urunGorevi = UrunleriYukleAsync();
        await Task.WhenAll(filtreGorevi, urunGorevi);
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
    }
}
