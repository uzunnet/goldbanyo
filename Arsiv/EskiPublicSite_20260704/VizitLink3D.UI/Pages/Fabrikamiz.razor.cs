using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages;

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
        ("goldbanyo/uretim.jpg", "Mobilya Gövde Hazırlığı", "Banyo mobilyası gövdeleri, proje ölçülerine göre hassas kesim ve montaj akışıyla hazırlanır."),
        ("goldbanyo/hakkimizda/fabrika.jpg", "Koleksiyon Detayları", "Gold Banyo koleksiyonlarında çekmece, ayna ve depolama detayları kullanım konforu için birlikte tasarlanır."),
        ("goldbanyo/showroom.jpg", "Ürün Sunum Alanı", "Showroom akışında malzeme, renk ve ölçü seçenekleri gerçek kullanım senaryolarıyla değerlendirilir."),
        ("goldbanyo/hakkimizda/fabrika_ic.jpg", "Kalite Kontrol Birimi", "Her ürün sevkiyat öncesinde yüzey, bağlantı ve paketleme kontrolünden geçirilir."),
    ];

    private readonly (string DosyaYolu, string AltMetin, int Kolon, string Yukseklik)[] _uretimGalerisi =
    [
        ("goldbanyo/uretim.jpg", "Gold Banyo ürün detayı", 6, "280px"),
        ("goldbanyo/hakkimizda/fabrika.jpg", "Koleksiyon depolama detayı", 3, "200px"),
        ("goldbanyo/showroom.jpg", "Gold Banyo showroom", 3, "200px"),
        ("goldbanyo/hakkimizda/fabrika_ic.jpg", "Banyo mobilyası detay alanı", 3, "200px"),
        ("goldbanyo/uretim.jpg", "Lavabo ve gövde detayı", 3, "200px"),
        ("goldbanyo/showroom.jpg", "Ürün sunum detayı", 6, "280px"),
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
