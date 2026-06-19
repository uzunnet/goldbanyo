using Desadoor.Ortak.Modeller.Urunler;
using Desadoor.Ortak.Modeller.Renkler;
using Desadoor.Ortak.Modeller.Malzemeler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using Desadoor.UI.Servisler;

namespace Desadoor.UI.Pages;

public partial class UrunDetay : ComponentBase, IDisposable
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private UcBoyutServisi UcBoyut { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager navManager { get; set; } = default!;

    [Parameter] public string Slug { get; set; } = string.Empty;

    // ─── Ürün verileri ────────────────────────────────────────────────────────
    private Urun? _urun;
    private string? _modelYolu;
    private string? _hdriOrtam;
    private List<TeknikOzellikSatiri> _teknikOzellikler = [];
    private List<RalRengi> _renkler = [];
    private List<Malzeme> _malzemeler = [];
    private List<UrunUcBoyutParcasi> _onayliParcalar = [];
    private List<Malzeme> _parcaMalzemeleri = [];
    private List<UrunMedya> _medyalar = [];
    private List<Urun> _benzerUrunler = [];
    private List<Urun> _enCokGezilenler = [];
    private List<Urun> _oneCikanlar = [];

    // ─── UI Durumu ────────────────────────────────────────────────────────────
    private bool _yukleniyor = true;
    private bool _otomatikDonuyor = false;
    private bool _sahneBaslatildi = false;
    private bool _teklifPanelAcik = false;
    private string _aktifTab = "gorsel";
    private string _anaGorselUrl = string.Empty;
    private int _seciliMedyaIdx = 0;

    // ─── Parça Seçimi ────────────────────────────────────────────────────────
    private int? _secilenParcaId;
    private UrunUcBoyutParcasi? _secilenParca;

    // ─── Renk / Malzeme Durumu ──────────────────────────────────────────────
    private string _aktifRenk = "#F1F0EB";
    private string _aktifRenkAdi = "RAL 9016";
    private string _aktifMalzeme = string.Empty;
    private string _ralArama = string.Empty;
    private Dictionary<string, List<RalRengi>> _renkGruplari = new();
    private HashSet<string> _acikGruplar = new();

    // ─── Teklif Formu ────────────────────────────────────────────────────────
    private string _teklifAdSoyad = string.Empty;
    private string _teklifTelefon = string.Empty;
    private string _teklifEposta = string.Empty;
    private string _teklifNot = string.Empty;

    // ─── Breadcrumb ──────────────────────────────────────────────────────────
    private List<BreadcrumbItem> _breadcrumb = [];
    private string? _yuklenenSlug;
    private HubConnection? _sahneBaglantisi;

    protected override void OnInitialized()
    {
        dil.DilDegisti += OnDilDegisti;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.Equals(_yuklenenSlug, Slug, StringComparison.OrdinalIgnoreCase))
        {
            _yuklenenSlug = Slug;
            await UrunDetayiniYukleAsync();
        }
    }

    private async Task DetayDurumunuSifirlaAsync()
    {
        if (_sahneBaslatildi)
        {
            await UcBoyut.Temizle("urun-detay-3d");
        }

        _urun = null;
        _modelYolu = null;
        _hdriOrtam = null;
        _teknikOzellikler = [];
        _renkler = [];
        _malzemeler = [];
        _onayliParcalar = [];
        _parcaMalzemeleri = [];
        _medyalar = [];
        _benzerUrunler = [];
        _enCokGezilenler = [];
        _oneCikanlar = [];
        _sahneBaslatildi = false;
        _otomatikDonuyor = false;
        _aktifTab = "gorsel";
        _anaGorselUrl = string.Empty;
        _seciliMedyaIdx = 0;
        _secilenParcaId = null;
        _secilenParca = null;
        _aktifRenk = "#F1F0EB";
        _aktifRenkAdi = "RAL 9016";
        _aktifMalzeme = string.Empty;
        _renkGruplari = [];
        _acikGruplar = [];
        _breadcrumb = [];
    }

    private async Task UrunDetayiniYukleAsync()
    {
        _yukleniyor = true;
        await DetayDurumunuSifirlaAsync();
        try
        {
            _urun = await Api.GetAsync<Urun>($"api/urunler/slug/{Slug}?dil={dil.AktifDil}");
            if (_urun != null)
            {
                _breadcrumb =
                [
                    new(dil.T("menu.anasayfa", "Ana Sayfa"), "/"),
                    new(dil.T("menu.urunler", "Ürünler"), "/urunler"),
                    new(_urun.Ad, null, disabled: true)
                ];

                await Task.WhenAll(
                    ModelYoluYukleAsync(),
                    RenkleriYukleAsync(),
                    MalzemeleriYukleAsync(),
                    OnayliParcalariYukleAsync(),
                    MedyalariYukleAsync(),
                    BenzerUrunleriYukleAsync(),
                    EnCokGezilenleriYukleAsync(),
                    OneCikanlariYukleAsync()
                );
                TeknikOzellikleriDoldur();
                RenkleriGrupla();
                _aktifTab = string.IsNullOrWhiteSpace(_modelYolu) ? "gorsel" : "3d";

                // Ana görsel URL belirle
                var anaMedya = _medyalar.FirstOrDefault(m => m.AnaGosterim) ?? _medyalar.FirstOrDefault();
                if (anaMedya is not null)
                    _anaGorselUrl = MedyaUrlOlustur(anaMedya.MedyaUrl);
                else if (_urun.AnaGorselMedyaId is long mid && mid > 0)
                    _anaGorselUrl = $"{Api.ApiBaseUrl}/api/medya/dosya/{mid}";
                else
                    _anaGorselUrl = string.Empty;
            }
        }
        finally { _yukleniyor = false; }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_yukleniyor && _urun != null && _aktifTab == "3d" && !_sahneBaslatildi && _modelYolu != null)
        {
            _sahneBaslatildi = true;
            try
            {
                await UcBoyut.Baslat("urun-detay-3d", _modelYolu, _aktifRenk, _hdriOrtam);
                if (!string.IsNullOrWhiteSpace(_hdriOrtam))
                {
                    await UcBoyut.CevreAyarla("urun-detay-3d", _hdriOrtam);
                }
                await Task.Delay(1200);
                await UcBoyut.ParcaSecCallbackKaydet("urun-detay-3d", DotNetObjectReference.Create(this));
                await CanliSahneBaglantisiniBaslat();
                StateHasChanged();
            }
            catch { Snackbar.Add(dil.T("urun.3dHata", "3D sahne başlatılamadı"), Severity.Warning); }
        }
    }

    private async Task AktifTabDegistir(string tab)
    {
        _aktifTab = tab;
        StateHasChanged();
        if (tab == "3d" && !_sahneBaslatildi)
        {
            await Task.Delay(100); // DOM render için bekle
            await OnAfterRenderAsync(false);
        }
    }

    [JSInvokable]
    public Task ParcaSecildi(string parcaIsmi)
    {
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task CanliSahneBaglantisiniBaslat()
    {
        if (_urun == null || _sahneBaglantisi is not null)
            return;

        try
        {
            _sahneBaglantisi = new HubConnectionBuilder()
                .WithUrl($"{Api.ApiBaseUrl}/hubs/sahne-ayar")
                .WithAutomaticReconnect()
                .Build();

            _sahneBaglantisi.On<int, string, string>("SahneAyarGuncellendi", async (modelId, ayarTipi, ayarJson) =>
            {
                await UcBoyut.SahneAyarUygula("urun-detay-3d", ayarTipi, ayarJson);
            });

            await _sahneBaglantisi.StartAsync();
        }
        catch { /* SignalR baglantisi kurulamazsa 3D yine de calisir */ }
    }

    // ─── Parça Seçimi ────────────────────────────────────────────────────────

    private async Task ParcaKartSec(UrunUcBoyutParcasi parca)
    {
        _secilenParcaId = parca.Id;
        _secilenParca = parca;
        _parcaMalzemeleri = [];

        // Parça ID'sine göre malzemeleri filtrele
        var sonuc = await Api.GetAsync<List<Malzeme>>($"api/urunler/malzemeler-parca-tipi?parcaId={parca.Id}");
        if (sonuc != null)
            _parcaMalzemeleri = sonuc;
        else
            _parcaMalzemeleri = _malzemeler; // fallback

        StateHasChanged();
    }

    // ─── Renk / Malzeme Değiştirme ──────────────────────────────────────────

    private async Task RenkDegistir(string hexKod, string ad)
    {
        _aktifRenk = hexKod;
        _aktifRenkAdi = ad;
        StateHasChanged();
        if (_aktifTab == "3d" && _sahneBaslatildi)
        {
            try
            {
                if (_secilenParca != null)
                    await UcBoyut.ParcaRenk("urun-detay-3d", _secilenParca.MeshAdi, hexKod);
                else
                    await UcBoyut.RenkUygula("urun-detay-3d", hexKod);
            }
            catch { /* renk değişimi 3D'ye uygulanamadı */ }
        }
    }

    private async Task MalzemeDegistir(string malzemeAdi)
    {
        _aktifMalzeme = malzemeAdi;
        StateHasChanged();
        if (_aktifTab == "3d" && _sahneBaslatildi && _secilenParca != null)
        {
            try
            {
                var tip = _parcaMalzemeleri.FirstOrDefault(m => m.Ad == malzemeAdi)?.Tip ?? "plastik";
                var tipMap = tip.ToLowerInvariant() switch
                {
                    string t when t.Contains("ayna") => "ayna",
                    string t when t.Contains("krom") && t.Contains("mat") => "krom_mat",
                    string t when t.Contains("krom") => "krom_parlak",
                    string t when t.Contains("metal") || t.Contains("alum") => "metal",
                    string t when t.Contains("ahsap") && t.Contains("kaplama") => "ahsap_kaplama",
                    string t when t.Contains("ahsap") || t.Contains("ahşap") => "ahsap",
                    string t when t.Contains("mdf") && t.Contains("parlak") => "mdf_parlak",
                    string t when t.Contains("mdf") => "mdf_mat",
                    string t when t.Contains("cam") => "cam",
                    string t when t.Contains("porselen") => "porselen",
                    _ => "plastik"
                };
                await UcBoyut.ParcaMalzeme("urun-detay-3d", _secilenParca.MeshAdi, tipMap);
            }
            catch { /* malzeme değişimi uygulanamadı */ }
        }
    }

    // ─── Renk Grupları ───────────────────────────────────────────────────────

    private void RenkleriGrupla()
    {
        _renkGruplari = _renkler
            .GroupBy(r => r.Grup ?? "Diğer")
            .ToDictionary(g => g.Key, g => g.ToList());

        // İlk grubu açık tut
        if (_renkGruplari.Count > 0)
            _acikGruplar.Add(_renkGruplari.Keys.First());
    }

    private void GrupToggle(string grupAdi)
    {
        if (_acikGruplar.Contains(grupAdi))
            _acikGruplar.Remove(grupAdi);
        else
            _acikGruplar.Add(grupAdi);
    }

    // ─── Görsel Yönetimi ────────────────────────────────────────────────────

    private void MedyaSec(int idx)
    {
        _seciliMedyaIdx = idx;
        if (idx < _medyalar.Count)
            _anaGorselUrl = MedyaUrlOlustur(_medyalar[idx].MedyaUrl);
    }

    // ─── 3D Kontroller ──────────────────────────────────────────────────────

    private async Task KameraSifirla()
    {
        try { await UcBoyut.KameraSifirla("urun-detay-3d"); } catch { /* kamera sıfırlanamazsa kullanıcı manuel döndürür */ }
    }

    private async Task OtomatikDondurToggle()
    {
        _otomatikDonuyor = !_otomatikDonuyor;
        try { await UcBoyut.OtomatikDondur("urun-detay-3d", _otomatikDonuyor); } catch { /* döndürme animasyonu başlatılamadı */ }
    }

    private async Task TamEkran()
    {
        try { await UcBoyut.TamEkran("urun-detay-3d"); } catch { /* tam ekran desteklenmiyor */ }
    }

    // ─── Teklif Paneli ──────────────────────────────────────────────────────

    private void TeklifPanelAc() => _teklifPanelAcik = true;
    private void TeklifPanelKapat() => _teklifPanelAcik = false;

    private async Task TeklifGonder()
    {
        if (string.IsNullOrWhiteSpace(_teklifAdSoyad) || string.IsNullOrWhiteSpace(_teklifTelefon))
        {
            Snackbar.Add(dil.T("teklif.eksikBilgi", "Ad soyad ve telefon zorunludur."), Severity.Warning);
            return;
        }
        // TODO: API teklif gönderimi
        Snackbar.Add(dil.T("teklif.gonderildi", "Teklif talebiniz alındı. En kısa sürede iletişime geçeceğiz."), Severity.Success);
        TeklifPanelKapat();
    }

    // ─── Yardımcı Metodlar ───────────────────────────────────────────────────

    private string ParcaTipiIkon(string? tip) => tip?.ToLower() switch
    {
        "cam" => "🪟",
        "ayna" => "🪞",
        "metal" or "aluminyum" or "alüminyum" => "⚙️",
        "ahsap" or "ahşap" or "mdf" => "🪵",
        "porselen" => "🏺",
        "govde" or "gövde" => "📦",
        "musluk" => "🚿",
        "kulp" => "🔧",
        _ => "🔷"
    };

    private string MalzemeRenkOnizleme(string tip) => tip?.ToLower() switch
    {
        "cam" => "rgba(100,200,255,0.4)",
        "ayna" => "linear-gradient(135deg, #C8C8D0 0%, #F8F8FF 50%, #C8C8D0 100%)",
        "metal" or "aluminyum" or "alüminyum" => "#B8B8C0",
        "krom" => "linear-gradient(135deg, #888 0%, #FFF 50%, #888 100%)",
        "ahsap" or "ahşap" => "#8B6A3B",
        "mdf" => "#C4A882",
        "porselen" => "#FFFFF5",
        "membran" => "#F0EDE8",
        "lake" => "#FAFAFA",
        "laminant" => "#D4C9B8",
        _ => "#E0E0E0"
    };

    private static string RenkHexCss(string? hexKod)
    {
        if (string.IsNullOrWhiteSpace(hexKod))
            return "#F1F0EA";

        var temizKod = hexKod.Trim();
        return temizKod.StartsWith('#') ? temizKod : $"#{temizKod}";
    }

    private static string RenkYaziSinifi(string? hexKod)
    {
        var temizKod = RenkHexCss(hexKod).TrimStart('#');
        if (temizKod.Length != 6)
            return "renk-yazi-koyu";

        var r = Convert.ToInt32(temizKod[..2], 16);
        var g = Convert.ToInt32(temizKod.Substring(2, 2), 16);
        var b = Convert.ToInt32(temizKod.Substring(4, 2), 16);
        var parlaklik = (r * 299 + g * 587 + b * 114) / 1000;

        return parlaklik < 135 ? "renk-yazi-acik" : "renk-yazi-koyu";
    }

    private string MedyaUrlOlustur(string? medyaUrl)
    {
        if (string.IsNullOrWhiteSpace(medyaUrl))
            return string.Empty;

        if (medyaUrl.StartsWith("http"))
            return medyaUrl;

        if (!medyaUrl.StartsWith('/'))
            medyaUrl = "/" + medyaUrl;

        return Api.ApiBaseUrl.TrimEnd('/') + medyaUrl;
    }

    private void TeknikOzellikleriDoldur()
    {
        if (_urun == null) return;
        _teknikOzellikler = [];
        if (!string.IsNullOrEmpty(_urun.Kod)) _teknikOzellikler.Add(new("Ürün Kodu", _urun.Kod));
        if (_urun.UrunAilesi != null) _teknikOzellikler.Add(new("Ürün Ailesi", _urun.UrunAilesi.Ad));
        _teknikOzellikler.Add(new("3D Görüntüleme", _modelYolu != null ? "Var" : "Yok"));
        if (_onayliParcalar.Count > 0) _teknikOzellikler.Add(new("Parça Sayısı", _onayliParcalar.Count.ToString()));
        if (_renkler.Count > 0) _teknikOzellikler.Add(new("Renk Seçeneği", $"{_renkler.Count} renk"));
        _teknikOzellikler.Add(new("Durum", _urun.AktifMi ? "Aktif" : "Pasif"));
    }

    private record TeknikOzellikSatiri(string Anahtar, string Deger);

    // ─── Veri Yükleme Metodları ─────────────────────────────────────────────

    private async Task ModelYoluYukleAsync()
    {
        try
        {
            var modeller = await Api.GetAsync<List<UrunUcBoyutModeli>>($"api/urunler/{_urun!.Id}/uc-boyut-modelleri");
            if (modeller != null && modeller.Count > 0)
            {
                var model = modeller.FirstOrDefault(m => m.Id == _urun.VarsayilanUcBoyutModeliId)
                            ?? modeller.FirstOrDefault(m => m.VarsayilanMi)
                            ?? modeller[0];
                var yol = string.IsNullOrWhiteSpace(model.ModelYolu) ? model.ModelDosyaYolu : model.ModelYolu;
                _modelYolu = ModelYoluOlustur(yol);
                _hdriOrtam = model.CevreAyarJson;
            }
        }
        catch { /* model yolu alınamazsa 3D gösterilmez */ }
    }

    private string? ModelYoluOlustur(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
            return null;

        if (!yol.StartsWith('/'))
            return yol;

        if (yol.StartsWith("/models/", StringComparison.OrdinalIgnoreCase))
            return new Uri(new Uri(navManager.BaseUri), yol.TrimStart('/')).ToString();

        return Api.ApiBaseUrl + yol;
    }

    private async Task RenkleriYukleAsync()
    {
        try
        {
            var s = await Api.GetAsync<List<RalRengi>>($"api/urunler/{_urun!.Id}/renkler");
            if (s != null) _renkler = s;
        }
        catch { /* renk listesi yüklenemezse palet boş kalır */ }
    }

    private async Task MalzemeleriYukleAsync()
    {
        try
        {
            var s = await Api.GetAsync<List<Malzeme>>("api/malzemeler");
            if (s != null) _malzemeler = s;
        }
        catch { /* malzeme listesi yüklenemezse seçenekler boş kalır */ }
    }

    private async Task OnayliParcalariYukleAsync()
    {
        try
        {
            var s = await Api.GetAsync<List<UrunUcBoyutParcasi>>($"api/urunler/{_urun!.Id}/parcalar/onayli");
            if (s != null) _onayliParcalar = s;
        }
        catch { /* onaylı parçalar yüklenemezse panel boş kalır */ }
    }

    private async Task MedyalariYukleAsync()
    {
        try
        {
            var s = await Api.GetAsync<List<UrunMedya>>($"api/urunler/{_urun!.Id}/medyalar");
            if (s != null)
            {
                foreach (var m in s)
                    m.MedyaUrl = MedyaUrlOlustur(m.MedyaUrl);
                _medyalar = s;
            }
        }
        catch { /* medya galerisi yüklenemezse boş kalır */ }
    }

    private async Task BenzerUrunleriYukleAsync()
    {
        try
        {
            var s = await Api.GetAsync<List<Urun>>($"api/urunler/{_urun!.Id}/benzer?dil={dil.AktifDil}&adet=8");
            if (s != null) _benzerUrunler = s;
        }
        catch { /* benzer ürünler yüklenemezse bölüm gösterilmez */ }
    }

    private async Task EnCokGezilenleriYukleAsync()
    {
        try
        {
            var s = await Api.GetAsync<List<Urun>>($"api/urunler/en-cok-gezilen?dil={dil.AktifDil}&adet=8");
            if (s != null) _enCokGezilenler = s;
        }
        catch { /* hata yutulur */ }
    }

    private async Task OneCikanlariYukleAsync()
    {
        try
        {
            // Fallback: If one-cikanlar endpoint doesn't exist, this will fail gracefully.
            var s = await Api.GetAsync<List<Urun>>($"api/urunler/one-cikanlar?dil={dil.AktifDil}&adet=8");
            if (s != null) _oneCikanlar = s;
        }
        catch { /* hata yutulur */ }
    }

    // ─── Yaşam Döngüsü ───────────────────────────────────────────────────────

    private async void OnDilDegisti()
    {
        await UrunDetayiniYukleAsync();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        dil.DilDegisti -= OnDilDegisti;
        _ = _sahneBaglantisi?.DisposeAsync().AsTask();
    }
}
