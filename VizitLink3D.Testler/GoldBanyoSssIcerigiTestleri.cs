using VizitLink3D.Ortak.Yardimcilar;

namespace VizitLink3D.Testler;

public sealed class GoldBanyoSssIcerigiTestleri
{
    [Fact]
    public void OnUcAdetGoldBanyoSssKaydiVardir()
    {
        Assert.Equal(13, GoldBanyoSssIcerigi.Kayitlar.Count);
    }

    [Fact]
    public void TumSorularDoluOlmalidir()
    {
        Assert.All(GoldBanyoSssIcerigi.Kayitlar, kayit => Assert.False(string.IsNullOrWhiteSpace(kayit.Soru)));
    }

    [Fact]
    public void TumCevaplarDoluOlmalidir()
    {
        Assert.All(GoldBanyoSssIcerigi.Kayitlar, kayit => Assert.False(string.IsNullOrWhiteSpace(kayit.Cevap)));
    }

    [Fact]
    public void EskiFirmaAdiIcermez()
    {
        Assert.DoesNotContain(GoldBanyoSssIcerigi.Kayitlar, kayit =>
            kayit.Soru.Contains("VizitLink3D", StringComparison.OrdinalIgnoreCase)
            || kayit.Cevap.Contains("VizitLink3D", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void HerKaydinKategorisiVardir()
    {
        Assert.All(GoldBanyoSssIcerigi.Kayitlar, kayit => Assert.False(string.IsNullOrWhiteSpace(kayit.KategoriAdi)));
    }
}
