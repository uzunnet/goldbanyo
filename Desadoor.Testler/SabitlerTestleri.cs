using Desadoor.Ortak.Modeller;
using Desadoor.Ortak.Modeller.Medya;

namespace Desadoor.Testler;

/// <summary>
/// Sinirlar ve sabitler testleri.
/// </summary>
public class SabitlerTestleri
{
    [Fact] public void Sinirlar_MaksResim_20MB() { Assert.Equal(20*1024*1024, Desadoor.Api.Sabitler.Sinirlar.MAKSIMUM_RESIM_BYTE); }
    [Fact] public void Sinirlar_MaksVideo_500MB() { Assert.Equal(500L*1024*1024, Desadoor.Api.Sabitler.Sinirlar.MAKSIMUM_VIDEO_BYTE); }
    [Fact] public void Sinirlar_MaksGlb_30MB() { Assert.Equal(30*1024*1024, Desadoor.Api.Sabitler.Sinirlar.MAKSIMUM_GLB_BYTE); }
    [Fact] public void Sinirlar_SayfaBoyutu_20() { Assert.Equal(20, Desadoor.Api.Sabitler.Sinirlar.VARSAYILAN_SAYFA_BOYUTU); }
    [Fact] public void Sinirlar_MaksMesaj_5000() { Assert.Equal(5000, Desadoor.Api.Sabitler.Sinirlar.MAKSIMUM_MESAJ_UZUNLUK); }
    [Fact] public void MedyaKullanim_OlusturulmaTarihi_Utc() { var k=new MedyaKullanim{MedyaId=1,EntiteAdi="T",EntiteId=1}; Assert.True((DateTime.UtcNow-k.OlusturulmaTarihi).TotalSeconds<5); }
    [Fact] public void MedyaKlasoru_OlusturulmaTarihi_Utc() { var k=new MedyaKlasoru{Ad="T"}; Assert.True((DateTime.UtcNow-k.OlusturulmaTarihi).TotalSeconds<5); }
    [Fact] public void Cevap_VarsayilanHata_BosListe() { var c=Cevap<int>.Hata("H"); Assert.Empty(c.Hatalar); }
    [Fact] public void KapakModeli_SilinmeTarihi_Null() { var m=new Desadoor.Api.Modeller.KapakModeli{ModelAdi="T",Slug="t"}; Assert.Null(m.SilinmeTarihi); Assert.False(m.SilindiMi); }
    [Fact] public void KapakModeli_Guncellenme_Null() { var m=new Desadoor.Api.Modeller.KapakModeli{ModelAdi="T",Slug="t"}; Assert.Null(m.GuncellenmeTarihi); }
}
