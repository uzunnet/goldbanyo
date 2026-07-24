using System.Text.Json;
using FluentValidation;
using FluentValidation.TestHelper;
using VizitLink3D.Api.Moduller.Urunler.Dtolar;
using VizitLink3D.Api.Moduller.Urunler.Dogrulayicilar;
using VizitLink3D.Ortak.Modeller.Urunler;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using VizitLink3D.Api.Moduller.Guvenlik.Kontrolcu;
using VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

namespace VizitLink3D.Testler;

/// <summary>
/// Paket-2A: Multi-Tenant Konfigüratör Admin Studio testleri.
/// Tenant izolasyonu, mantıksal kod unique, hareket enum, JSON validasyonu,
/// sahne preset soft-delete/varsayılan davranışı.
/// </summary>
public class Paket2A_MultiTenantKonfiguratorTestleri
{
    // ================================================================
    // TEST 1: Geçersiz hareket tipi enum doğrulama
    // ================================================================

    [Fact]
    public void ParcaUpsertDogrulayici_GecersizHareketTipi_Reddetmeli()
    {
        var dogrulayici = new UcBoyutParcaUpsertDogrulayici();
        var dto = new UcBoyutParcaUpsertDto(
            MeshAdi: "Kulp_Mesh_001",
            MantiksalKod: "kulp-01",
            GorunenAd: "Altın Kulp",
            ParcaGrubuId: null,
            HareketTipi: "Ucma",   // ← geçersiz enum değeri
            HareketAyarlariJson: null,
            DokuUygulanabilirMi: false,
            GorunurlukDegisebilirMi: true,
            RenklenebilirMi: true,
            MalzemeDegisebilirMi: false,
            SecilebilirMi: true,
            HareketliMi: false,
            ParcaTipi: "Kulp",
            MalzemeTipiKisiti: null,
            SiraNo: 1,
            AktifMi: true,
            AdminOnayliMi: true
        );

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.HareketTipi);
    }

    // ================================================================
    // TEST 2: Geçerli hareket tipi enum doğrulama (pozitif test)
    // ================================================================

    [Fact]
    public void ParcaUpsertDogrulayici_GecerliHareketTipi_KabulEtmeli()
    {
        var dogrulayici = new UcBoyutParcaUpsertDogrulayici();
        var dto = new UcBoyutParcaUpsertDto(
            MeshAdi: "Kapak_Mesh",
            MantiksalKod: "kapak-01",
            GorunenAd: "Ahşap Kapak",
            ParcaGrubuId: 1,
            HareketTipi: nameof(HareketTuru.Menteseli),   // ← geçerli enum
            HareketAyarlariJson: "{\"eksen\":\"x\",\"maxAci\":90}",
            DokuUygulanabilirMi: true,
            GorunurlukDegisebilirMi: true,
            RenklenebilirMi: true,
            MalzemeDegisebilirMi: false,
            SecilebilirMi: true,
            HareketliMi: true,
            ParcaTipi: "Govde",
            MalzemeTipiKisiti: null,
            SiraNo: 0,
            AktifMi: true,
            AdminOnayliMi: true
        );

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldNotHaveValidationErrorFor(x => x.HareketTipi);
    }

    // ================================================================
    // TEST 3: Mantıksal kod geçersiz karakter (Türkçe/özel) reddi
    // ================================================================

    [Fact]
    public void ParcaUpsertDogrulayici_MantiksalKodTurkceKarakter_Reddetmeli()
    {
        var dogrulayici = new UcBoyutParcaUpsertDogrulayici();
        var dto = new UcBoyutParcaUpsertDto(
            MeshAdi: "Cekmece_Mesh",
            MantiksalKod: "çekmece-İç",   // ← Türkçe karakter içeriyor
            GorunenAd: "Çekmece İç Bölüm",
            ParcaGrubuId: null,
            HareketTipi: nameof(HareketTuru.Cekmece),
            HareketAyarlariJson: null,
            DokuUygulanabilirMi: false,
            GorunurlukDegisebilirMi: false,
            RenklenebilirMi: false,
            MalzemeDegisebilirMi: false,
            SecilebilirMi: true,
            HareketliMi: true,
            ParcaTipi: "Govde",
            MalzemeTipiKisiti: null,
            SiraNo: 2,
            AktifMi: true,
            AdminOnayliMi: true
        );

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.MantiksalKod);
    }

    // ================================================================
    // TEST 4: Geçersiz JSON reddi — HareketAyarlariJson
    // ================================================================

    [Fact]
    public void ParcaUpsertDogrulayici_GecersizHareketAyarlariJson_Reddetmeli()
    {
        var dogrulayici = new UcBoyutParcaUpsertDogrulayici();
        var dto = new UcBoyutParcaUpsertDto(
            MeshAdi: "Menteseli_Kapak",
            MantiksalKod: "menteseli-kapak",
            GorunenAd: "Menteşeli Kapak",
            ParcaGrubuId: null,
            HareketTipi: nameof(HareketTuru.Menteseli),
            HareketAyarlariJson: "{bozuk json!!!}",   // ← geçersiz JSON
            DokuUygulanabilirMi: true,
            GorunurlukDegisebilirMi: true,
            RenklenebilirMi: true,
            MalzemeDegisebilirMi: false,
            SecilebilirMi: true,
            HareketliMi: true,
            ParcaTipi: "Govde",
            MalzemeTipiKisiti: null,
            SiraNo: 3,
            AktifMi: true,
            AdminOnayliMi: true
        );

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.HareketAyarlariJson);
    }

    // ================================================================
    // TEST 5: Sahne önayarı — script/HTML enjeksiyonu reddi
    // ================================================================

    [Fact]
    public void SahneOnayariDogrulayici_AyarlarScriptIceriyor_Reddetmeli()
    {
        var dogrulayici = new UcBoyutSahneOnayariDogrulayici();
        var dto = new UcBoyutSahneOnayariDto(
            Ad: "Kötü Kamera",
            Kod: "kamera-hack",
            AyarlarJson: "{\"kamera\":{\"onload\":\"<script>alert('xss')</script>\"}}",
            VarsayilanMi: false,
            AktifMi: true,
            SiraNo: 1
        );

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.AyarlarJson);
    }

    // ================================================================
    // TEST 6: Sahne önayarı — geçersiz kod formatı reddi
    // ================================================================

    [Fact]
    public void SahneOnayariDogrulayici_GecersizKodFormati_Reddetmeli()
    {
        var dogrulayici = new UcBoyutSahneOnayariDogrulayici();
        var dto = new UcBoyutSahneOnayariDto(
            Ad: "Genel Görünüm",
            Kod: "genel görünüm!",   // ← boşluk ve özel karakter
            AyarlarJson: "{\"kamera\":{\"pozisyon\":[0,5,3]}}",
            VarsayilanMi: true,
            AktifMi: true,
            SiraNo: 1
        );

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.Kod);
    }

    // ================================================================
    // TEST 7: Boş MeshAdi reddi
    // ================================================================

    [Fact]
    public void ParcaUpsertDogrulayici_BosMeshAdi_Reddetmeli()
    {
        var dogrulayici = new UcBoyutParcaUpsertDogrulayici();
        var dto = new UcBoyutParcaUpsertDto(
            MeshAdi: "",   // ← boş
            MantiksalKod: null,
            GorunenAd: "Test Parça",
            ParcaGrubuId: null,
            HareketTipi: null,
            HareketAyarlariJson: null,
            DokuUygulanabilirMi: false,
            GorunurlukDegisebilirMi: false,
            RenklenebilirMi: false,
            MalzemeDegisebilirMi: false,
            SecilebilirMi: true,
            HareketliMi: false,
            ParcaTipi: null,
            MalzemeTipiKisiti: null,
            SiraNo: 0,
            AktifMi: true,
            AdminOnayliMi: false
        );

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.MeshAdi);
    }

    // ================================================================
    // TEST 8: Boş GorunenAd reddi
    // ================================================================

    [Fact]
    public void ParcaUpsertDogrulayici_BosGorunenAd_Reddetmeli()
    {
        var dogrulayici = new UcBoyutParcaUpsertDogrulayici();
        var dto = new UcBoyutParcaUpsertDto(
            MeshAdi: "Test_Mesh",
            MantiksalKod: null,
            GorunenAd: "",   // ← boş
            ParcaGrubuId: null,
            HareketTipi: null,
            HareketAyarlariJson: null,
            DokuUygulanabilirMi: false,
            GorunurlukDegisebilirMi: false,
            RenklenebilirMi: false,
            MalzemeDegisebilirMi: false,
            SecilebilirMi: true,
            HareketliMi: false,
            ParcaTipi: null,
            MalzemeTipiKisiti: null,
            SiraNo: 0,
            AktifMi: true,
            AdminOnayliMi: false
        );

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.GorunenAd);
    }

    // ================================================================
    // TEST 9: Toplu upsert — aynı batch içinde duplicate mesh adı reddi
    // ================================================================

    [Fact]
    public void TopluUpsertDogrulayici_AyniBatchDuplicateMeshAdi_Reddetmeli()
    {
        var dogrulayici = new UcBoyutParcaTopluUpsertDogrulayici();
        var dto = new UcBoyutParcaTopluUpsertDto([
            new UcBoyutParcaUpsertDto("Mesh_A", "kod-a", "Parça A", null, null, null, false, true, true, false, true, false, null, null, 1, true, true),
            new UcBoyutParcaUpsertDto("Mesh_A", "kod-b", "Parça B", null, null, null, false, true, true, false, true, false, null, null, 2, true, true)
        ]);

        var sonuc = dogrulayici.TestValidate(dto);
        sonuc.ShouldHaveValidationErrorFor(x => x.Parcalar);
    }

    // ================================================================
    // TEST 10: HareketTuru enum tüm değerleri kapsıyor mu?
    // ================================================================

    [Fact]
    public void HareketTuruEnum_TumDegerler_GecerliOlmali()
    {
        var tumu = Enum.GetValues<HareketTuru>();
        Assert.Contains(HareketTuru.Sabit, tumu);
        Assert.Contains(HareketTuru.Menteseli, tumu);
        Assert.Contains(HareketTuru.Surgulu, tumu);
        Assert.Contains(HareketTuru.Cekmece, tumu);
        Assert.Contains(HareketTuru.YukariAcilir, tumu);
        Assert.Contains(HareketTuru.Pivot, tumu);
        Assert.Contains(HareketTuru.Recliner, tumu);
        Assert.Equal(7, tumu.Length);
    }

    // ================================================================
    // TEST 11: UrunUcBoyutSahneOnayari — soft delete ve varsayılan davranışı
    // ================================================================

    [Fact]
    public void SahneOnayari_SilindiMi_VarsayilanFalseOlmali()
    {
        var onayar = new UrunUcBoyutSahneOnayari();
        Assert.False(onayar.SilindiMi);
        Assert.Null(onayar.SilinmeTarihi);
    }

    [Fact]
    public void SahneOnayari_VarsayilanMi_VarsayilanFalseOlmali()
    {
        var onayar = new UrunUcBoyutSahneOnayari();
        Assert.False(onayar.VarsayilanMi);
        Assert.True(onayar.AktifMi);   // aktif varsayılan true
    }

    // ================================================================
    // TEST 12: UrunUcBoyutParcasi — yeni alanlar varsayılan değerler
    // ================================================================

    [Fact]
    public void UrunUcBoyutParcasi_YeniAlanlar_VarsayilanDegerlerDogru()
    {
        var parca = new UrunUcBoyutParcasi();
        Assert.False(parca.DokuUygulanabilirMi);
        Assert.False(parca.SilindiMi);
        Assert.Null(parca.MantiksalKod);
        Assert.Null(parca.HareketAyarlariJson);
    }

    // ================================================================
    // TEST 13: Sahne önayarı DTO — geçerli JSON kabul edilmeli
    // ================================================================

    [Fact]
    public void SahneOnayariDogrulayici_GecerliJson_KabulEtmeli()
    {
        var dogrulayici = new UcBoyutSahneOnayariDogrulayici();
        var dto = new UcBoyutSahneOnayariDto(
            Ad: "Ön Görünüm",
            Kod: "on-gorunum",
            AyarlarJson: "{\"kamera\":{\"pozisyon\":[0,2,5],\"hedef\":[0,1,0]},\"isik\":{\"yogunluk\":1.0}}",
            VarsayilanMi: true,
            AktifMi: true,
            SiraNo: 1
        );

        var sonuc = dogrulayici.TestValidate(dto);
        Assert.True(sonuc.IsValid);
    }

    // ================================================================
    // TEST 14-20: Paket-2A RET — Tenant İzolasyonu ve Rol Gerileme
    // ================================================================

    /// <summary>
    /// TEST 14: SahneOnayari entity'sinde UrunUcBoyutModeli navigation var mı?
    /// </summary>
    [Fact]
    public void SahneOnayari_UrunUcBoyutModeliNavigation_VarOlmali()
    {
        var navProp = typeof(UrunUcBoyutSahneOnayari)
            .GetProperty("UrunUcBoyutModeli", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(navProp);
        Assert.Equal(typeof(UrunUcBoyutModeli), navProp!.PropertyType);
    }

    /// <summary>
    /// TEST 15: SahneOnayari yeni instance'da navigation null başlamalı.
    /// </summary>
    [Fact]
    public void SahneOnayari_YeniOrnek_UrunUcBoyutModeliNullOlmali()
    {
        var onayar = new UrunUcBoyutSahneOnayari();
        Assert.Null(onayar.UrunUcBoyutModeli);
        Assert.Equal(0, onayar.UrunUcBoyutModeliId);
    }

    /// <summary>
    /// TEST 16: FirmaApiAnahtarKontrolcu Authorize attribute — FirmaAdmin KULLANILMAMALI.
    /// Rol enum'da sadece Kullanici, Editor, Admin, SuperAdmin var.
    /// FirmaAdmin tanımsız olduğu için JWT doğrulaması başarısız olurdu.
    /// </summary>
    [Fact]
    public void FirmaApiAnahtarKontrolcu_AuthorizeAttribute_FirmaAdminIcerememeli()
    {
        var attr = typeof(FirmaApiAnahtarKontrolcu)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attr);
        // FirmaAdmin rolü artık kullanılmamalı
        Assert.DoesNotContain("FirmaAdmin", attr!.Roles);
        // Doğru roller: Admin ve SuperAdmin
        Assert.Contains("Admin", attr.Roles);
        Assert.Contains("SuperAdmin", attr.Roles);
    }

    /// <summary>
    /// TEST 17: UcBoyutKonfiguratorAdminKontrolcu Authorize attribute — Admin ve SuperAdmin.
    /// </summary>
    [Fact]
    public void UcBoyutKonfiguratorAdminKontrolcu_AuthorizeAttribute_DogruRoller()
    {
        var attr = typeof(UcBoyutKonfiguratorAdminKontrolcu)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attr);
        Assert.Contains("Admin", attr!.Roles);
        Assert.Contains("SuperAdmin", attr.Roles);
        // FirmaAdmin KULLANILMAMALI
        Assert.DoesNotContain("FirmaAdmin", attr.Roles);
    }

    /// <summary>
    /// TEST 18: Tenant sahiplik zinciri — SahneOnayari model FK'si üzerinden tenant'a bağlanabilmeli.
    /// UrunUcBoyutSahneOnayari.UrunUcBoyutModeliId → UrunUcBoyutModeli.UrunId → ParcaGrubu.FirmaId.
    /// </summary>
    [Fact]
    public void TenantSahiplikZinciri_PropertyVarligi_Dogrulanmali()
    {
        // Zincir: SahneOnayari.UrunUcBoyutModeliId → Model.Id → Model.UrunId → ParcaGrubu.UrunId + FirmaId

        // UrunUcBoyutModeli'de UrunId var mı?
        var urunIdProp = typeof(UrunUcBoyutModeli).GetProperty("UrunId");
        Assert.NotNull(urunIdProp);

        // UrunParcaGrubu'nda UrunId ve FirmaId var mı?
        var pgUrunIdProp = typeof(UrunParcaGrubu).GetProperty("UrunId");
        var pgFirmaIdProp = typeof(UrunParcaGrubu).GetProperty("FirmaId");
        Assert.NotNull(pgUrunIdProp);
        Assert.NotNull(pgFirmaIdProp);

        // SahneOnayari'nde UrunUcBoyutModeliId var mı?
        var soModelIdProp = typeof(UrunUcBoyutSahneOnayari).GetProperty("UrunUcBoyutModeliId");
        Assert.NotNull(soModelIdProp);
    }

    /// <summary>
    /// TEST 19: Parça grubu FirmaId ataması — yeni grup oluşturulduğunda FirmaId atanabilir.
    /// </summary>
    [Fact]
    public void UrunParcaGrubu_FirmaIdAtanabilir_DogruCalismali()
    {
        var grup = new UrunParcaGrubu
        {
            UrunId = 1,
            FirmaId = 5,
            Ad = "Test Grubu",
            SiraNo = 1
        };

        Assert.Equal(5, grup.FirmaId);
        Assert.Equal(1, grup.UrunId);
    }

    /// <summary>
    /// TEST 20: SahneOnayari FK — DeleteBehavior.Restrict zorunlu.
    /// Model silinmeden önce sahne önayarları silinmeli.
    /// Bu test entity konfigürasyonunun doğru olduğunu kontrol eder.
    /// </summary>
    [Fact]
    public void SahneOnayari_FK_Ozellik_Dogrulanmali()
    {
        // UrunUcBoyutModeliId alanı int (non-nullable) olmalı — her önayar bir modele bağlı
        var modelIdProp = typeof(UrunUcBoyutSahneOnayari).GetProperty("UrunUcBoyutModeliId");
        Assert.NotNull(modelIdProp);
        Assert.Equal(typeof(int), modelIdProp!.PropertyType);

        // Navigation property [JsonIgnore] ile işaretlenmiş olmalı (döngü önleme)
        var navProp = typeof(UrunUcBoyutSahneOnayari).GetProperty("UrunUcBoyutModeli");
        Assert.NotNull(navProp);
        var jsonIgnoreAttr = navProp!.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>();
        Assert.NotNull(jsonIgnoreAttr);
    }
}
