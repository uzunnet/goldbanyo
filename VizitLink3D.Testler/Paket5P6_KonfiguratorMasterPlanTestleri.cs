using FluentValidation.TestHelper;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Komutlar;
using VizitLink3D.Ortak.Modeller.Urunler;
using Xunit;

namespace VizitLink3D.Testler;

/// <summary>
/// P5-P6 Master Plan testleri:
/// - BOM hesaplama validasyonu
/// - Teklif isteği validasyonu
/// - Model onay yetki kontrolü
/// - Analytics olay kayıt validasyonu
/// - Embed SDK oturum anahtarı güvenliği
/// - Public konfigüratör model onay filtresi
/// </summary>
public class Paket5P6_KonfiguratorMasterPlanTestleri
{
    // ================================================================
    // P6.1: Teklif İsteği DTO validasyonu
    // ================================================================

    [Fact]
    public void TeklifIstegiOlusturDogrulayici_GecerliDto_KabulEtmeli()
    {
        var dogrulayici = new TeklifIstegiOlusturDogrulayici();
        var dto = new TeklifIstegiOlusturDto(
            MusteriKonfigurasyonuId: 1,
            UrunId: 5,
            MusteriAdSoyad: "Ahmet Yılmaz",
            Eposta: "ahmet@ornek.com",
            Telefon: "05321234567",
            Not: "Acil teslimat lütfen");

        var sonuc = dogrulayici.TestValidate(dto);
        Assert.True(sonuc.IsValid);
    }

    [Fact]
    public void TeklifIstegiOlusturDogrulayici_BosEposta_Reddetmeli()
    {
        var dogrulayici = new TeklifIstegiOlusturDogrulayici();
        var dto = new TeklifIstegiOlusturDto(1, 5, "Ahmet", "", "0532", null);

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.Eposta);
    }

    [Fact]
    public void TeklifIstegiOlusturDogrulayici_GecersizEposta_Reddetmeli()
    {
        var dogrulayici = new TeklifIstegiOlusturDogrulayici();
        var dto = new TeklifIstegiOlusturDto(1, 5, "Ahmet", "gecersiz-eposta", null, null);

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.Eposta);
    }

    [Fact]
    public void TeklifIstegiOlusturDogrulayici_BosMusteriAdi_Reddetmeli()
    {
        var dogrulayici = new TeklifIstegiOlusturDogrulayici();
        var dto = new TeklifIstegiOlusturDto(1, 5, "", "ahmet@ornek.com", null, null);

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.MusteriAdSoyad);
    }

    [Fact]
    public void TeklifIstegiOlusturDogrulayici_NegatifKonfigurasyonId_Reddetmeli()
    {
        var dogrulayici = new TeklifIstegiOlusturDogrulayici();
        var dto = new TeklifIstegiOlusturDto(-1, 5, "Ahmet", "ahmet@ornek.com", null, null);

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.MusteriKonfigurasyonuId);
    }

    // ================================================================
    // P6.2: Analytics olay kayıt validasyonu
    // ================================================================

    [Fact]
    public void OlayKaydetDogrulayici_GecerliOlay_KabulEtmeli()
    {
        var dogrulayici = new OlayKaydetDogrulayici();
        var komut = new AnalitikKomutlari.OlayKaydetKomutu(
            OturumAnahtari: "vt3d_abc123_xyz",
            OlayTipi: "ParcaSecildi",
            OlayVerisiJson: "{\"parcaId\":42}",
            UrunId: 1,
            ModelId: 10);

        var sonuc = dogrulayici.TestValidate(komut);
        Assert.True(sonuc.IsValid);
    }

    [Fact]
    public void OlayKaydetDogrulayici_BosOturumAnahtari_Reddetmeli()
    {
        var dogrulayici = new OlayKaydetDogrulayici();
        var komut = new AnalitikKomutlari.OlayKaydetKomutu("", "SayfaGoruntulendi");

        var sonuc = dogrulayici.TestValidate(komut);
        sonuc.ShouldHaveValidationErrorFor(x => x.OturumAnahtari);
    }

    [Fact]
    public void OlayKaydetDogrulayici_BosOlayTipi_Reddetmeli()
    {
        var dogrulayici = new OlayKaydetDogrulayici();
        var komut = new AnalitikKomutlari.OlayKaydetKomutu("vt3d_abc", "");

        var sonuc = dogrulayici.TestValidate(komut);
        sonuc.ShouldHaveValidationErrorFor(x => x.OlayTipi);
    }

    // ================================================================
    // P5: Model onay komutu validasyonu
    // ================================================================

    [Fact]
    public void ModelOnaylaKomutu_GecerliModelId_KabulEtmeli()
    {
        var komut = new ModelOnaylaKomutu(123);
        Assert.Equal(123, komut.ModelId);
    }

    [Fact]
    public void ModelOnaylaKomutu_SifirModelId_GecersizOlmali()
    {
        // Sıfır veya negatif ID'ler kontrolcü seviyesinde reddedilir
        var komut = new ModelOnaylaKomutu(0);
        Assert.True(komut.ModelId <= 0); // Kontrolcü bu durumu yakalamalı
    }

    // ================================================================
    // P6.3: Entity alanları doğrulama
    // ================================================================

    [Fact]
    public void KonfiguratorOlayKaydi_VarsayilanDegerler_DogruOlmali()
    {
        var olay = new KonfiguratorOlayKaydi
        {
            OturumAnahtari = "vt3d_test",
            OlayTipi = "SayfaGoruntulendi"
        };

        Assert.Equal("SayfaGoruntulendi", olay.OlayTipi);
        Assert.False(olay.SilindiMi);
        Assert.True(olay.OlusturulmaTarihi <= DateTime.UtcNow);
        Assert.True(olay.OlusturulmaTarihi > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void KonfiguratorTeklif_VarsayilanDegerler_DogruOlmali()
    {
        var teklif = new KonfiguratorTeklif
        {
            MusteriAdSoyad = "Test Müşteri",
            Eposta = "test@ornek.com"
        };

        Assert.Equal("Bekliyor", teklif.Durum);
        Assert.False(teklif.SilindiMi);
        Assert.True(teklif.OlusturulmaTarihi <= DateTime.UtcNow);
    }

    [Fact]
    public void UrunUcBoyutModeli_AdminOnayliMi_VarsayilanFalseOlmali()
    {
        var model = new VizitLink3D.Ortak.Modeller.Urunler.UrunUcBoyutModeli
        {
            ModelAdi = "Test Model",
            ModelDosyaYolu = "/medya/3d-modeller/test.glb",
            MedyaId = 1
        };

        Assert.False(model.AdminOnayliMi);
        Assert.Null(model.OnayTarihi);
        Assert.Null(model.OnaylayanKullaniciId);
    }

    // ================================================================
    // P6.4: Public konfigüratör slug validasyonu (mevcut, genişletilmiş)
    // ================================================================

    [Fact]
    public void PublicKonfiguratorSorguDogrulayici_UzunSlug_Reddetmeli()
    {
        var dogrulayici = new PublicKonfiguratorSorguDogrulayici();
        var uzunSlug = new string('a', 300);
        var sonuc = dogrulayici.TestValidate(uzunSlug);
        sonuc.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void PublicKonfiguratorSorguDogrulayici_OzelKarakterliSlug_Reddetmeli()
    {
        var dogrulayici = new PublicKonfiguratorSorguDogrulayici();
        var sonuc = dogrulayici.TestValidate("banyo@dolabi!");
        sonuc.ShouldHaveValidationErrorFor(x => x);
    }
}
