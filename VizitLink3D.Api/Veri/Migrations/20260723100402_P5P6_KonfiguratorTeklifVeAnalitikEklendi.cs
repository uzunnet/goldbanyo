using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class P5P6_KonfiguratorTeklifVeAnalitikEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdminOnayliMi",
                table: "UrunUcBoyutModelleri",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnayTarihi",
                table: "UrunUcBoyutModelleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnaylayanKullaniciId",
                table: "UrunUcBoyutModelleri",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KonfiguratorOlayKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: true),
                    ModelId = table.Column<int>(type: "INTEGER", nullable: true),
                    OturumAnahtari = table.Column<string>(type: "TEXT", nullable: true),
                    OlayTipi = table.Column<string>(type: "TEXT", nullable: false),
                    OlayVerisiJson = table.Column<string>(type: "TEXT", nullable: true),
                    KullaniciIp = table.Column<string>(type: "TEXT", nullable: true),
                    TarayiciBilgisi = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KonfiguratorOlayKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KonfiguratorOlayKayitlari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KonfiguratorOlayKayitlari_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "KonfiguratorTeklifler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    MusteriKonfigurasyonuId = table.Column<int>(type: "INTEGER", nullable: true),
                    UrunId = table.Column<int>(type: "INTEGER", nullable: true),
                    OturumAnahtari = table.Column<string>(type: "TEXT", nullable: true),
                    MusteriAdSoyad = table.Column<string>(type: "TEXT", nullable: true),
                    Eposta = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    Not = table.Column<string>(type: "TEXT", nullable: true),
                    BomJson = table.Column<string>(type: "TEXT", nullable: true),
                    ToplamFiyat = table.Column<decimal>(type: "TEXT", nullable: true),
                    Durum = table.Column<string>(type: "TEXT", nullable: false),
                    DurumGuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AdminNotu = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturanKullaniciId = table.Column<int>(type: "INTEGER", nullable: true),
                    GuncelleyenKullaniciId = table.Column<int>(type: "INTEGER", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KonfiguratorTeklifler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KonfiguratorTeklifler_MusteriKonfigurasyonlari_MusteriKonfigurasyonuId",
                        column: x => x.MusteriKonfigurasyonuId,
                        principalTable: "MusteriKonfigurasyonlari",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KonfiguratorTeklifler_Urunler_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urunler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KonfiguratorOlayKayitlari_FirmaId_OlayTipi_OlusturulmaTarihi",
                table: "KonfiguratorOlayKayitlari",
                columns: new[] { "FirmaId", "OlayTipi", "OlusturulmaTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_KonfiguratorOlayKayitlari_OturumAnahtari",
                table: "KonfiguratorOlayKayitlari",
                column: "OturumAnahtari");

            migrationBuilder.CreateIndex(
                name: "IX_KonfiguratorOlayKayitlari_UrunId",
                table: "KonfiguratorOlayKayitlari",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_KonfiguratorTeklifler_Eposta",
                table: "KonfiguratorTeklifler",
                column: "Eposta");

            migrationBuilder.CreateIndex(
                name: "IX_KonfiguratorTeklifler_FirmaId_Durum",
                table: "KonfiguratorTeklifler",
                columns: new[] { "FirmaId", "Durum" });

            migrationBuilder.CreateIndex(
                name: "IX_KonfiguratorTeklifler_MusteriKonfigurasyonuId",
                table: "KonfiguratorTeklifler",
                column: "MusteriKonfigurasyonuId");

            migrationBuilder.CreateIndex(
                name: "IX_KonfiguratorTeklifler_UrunId",
                table: "KonfiguratorTeklifler",
                column: "UrunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KonfiguratorOlayKayitlari");

            migrationBuilder.DropTable(
                name: "KonfiguratorTeklifler");

            migrationBuilder.DropColumn(
                name: "AdminOnayliMi",
                table: "UrunUcBoyutModelleri");

            migrationBuilder.DropColumn(
                name: "OnayTarihi",
                table: "UrunUcBoyutModelleri");

            migrationBuilder.DropColumn(
                name: "OnaylayanKullaniciId",
                table: "UrunUcBoyutModelleri");
        }
    }
}
