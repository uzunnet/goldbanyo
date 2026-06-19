using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Desadoor.UI.Pages;

public partial class Fabrikamiz : ComponentBase
{
    private readonly (string Deger, string Etiket)[] _istatistikler =
    [
        ("5.000 m²", "Kapalı Üretim Alanı"),
        ("50.000+", "Aylık Kapak Üretimi"),
        ("500+", "Özgün Model"),
        ("30+", "Yıllık Tecrübe"),
        ("120+", "Uzman Çalışan"),
        ("20+", "İhracat Ülkesi"),
    ];

    private readonly (string Gorsel, string Baslik, string Aciklama)[] _uretimAlanlari =
    [
        ("fabrika/fabrikadis_ic_1.jpg", "CNC Kesim Atölyesi",  "5 eksenli CNC tezgâhlarımızla 0,1 mm hassasiyetle kesim ve frezeleme işlemleri gerçekleştirilmektedir."),
        ("fabrika/fabrikadis_ic_2.jpg", "Membran Pres Hattı",  "Tam otomatik membran pres hatlarımızda günde 3.000+ kapak yüzey kaplaması yapılmaktadır."),
        ("fabrika/fabrikadis_ic_3.jpg", "Lake Boyama Kabini",  "ISO sertifikalı lake boyama kabinlerimizde RAL, NCS ve Pantone renk uyumlu uygulama yapılmaktadır."),
        ("fabrika/fabrikadis_ic_4.jpg", "Kalite Kontrol Birimi","Her ürün sevkiyat öncesinde boyut, yüzey ve renk açısından tam kontrolden geçirilmektedir."),
    ];

    private readonly (string Ikon, string Baslik, string Aciklama)[] _teknolojiler =
    [
        (Icons.Material.Filled.Build,          "CNC Teknolojisi",       "5 eksenli hassas CNC merkezlerimiz, karmaşık geometrileri ve özel tasarımları mükemmel toleranslarla üretir."),
        (Icons.Material.Filled.Settings,       "Otomatik Pres Hattı",  "Tam otomatik membran ve vakum pres hatları, yüksek hacimli siparişleri hızlı ve tutarlı şekilde karşılar."),
        (Icons.Material.Filled.Palette,        "Renk Uyumu Sistemi",   "Spektrofotometre cihazlarımızla RAL, NCS ve Pantone renk standartlarında %99,5 uyum sağlanır."),
        (Icons.Material.Filled.VerifiedUser,   "Kalite Yönetimi",      "ISO 9001:2015 sertifikalı kalite yönetim sistemimiz, hammaddeden son kontrole kadar tüm süreçleri kapsar."),
        (Icons.Material.Filled.NaturePeople,   "Sürdürülebilir Üretim","FSC sertifikalı hammaddeler ve düşük VOC içerikli laklar kullanarak çevre dostu üretim yapıyoruz."),
        (Icons.Material.Filled.Speed,          "Hızlı Üretim",         "Optimize edilmiş üretim akışımız, standart siparişlerde 7–10 iş günü teslimat garantisi sunar."),
    ];

    private string Gorsel(string yol)
        => $"{api.ApiBaseUrl}/medya/{yol}";
}
