using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Konfigurator.Api.VeriTabani.Migrations
{
    /// <inheritdoc />
    public partial class ParcaKategorisiVeTenantIzolasyonu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "UcBoyutModelParcalari",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParcaKategoriId",
                table: "UcBoyutModelParcalari",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "UcBoyutModeller",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Firmalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    YedekDomain = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firmalar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParcaKategorileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcaKategorileri", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UcBoyutModelParcalari_FirmaId",
                table: "UcBoyutModelParcalari",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_UcBoyutModelParcalari_ParcaKategoriId",
                table: "UcBoyutModelParcalari",
                column: "ParcaKategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_UcBoyutModeller_FirmaId",
                table: "UcBoyutModeller",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Firmalar_Domain",
                table: "Firmalar",
                column: "Domain");

            migrationBuilder.CreateIndex(
                name: "IX_Firmalar_Slug",
                table: "Firmalar",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcaKategorileri_FirmaId",
                table: "ParcaKategorileri",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcaKategorileri_FirmaId_Ad",
                table: "ParcaKategorileri",
                columns: new[] { "FirmaId", "Ad" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UcBoyutModelParcalari_ParcaKategorileri_ParcaKategoriId",
                table: "UcBoyutModelParcalari",
                column: "ParcaKategoriId",
                principalTable: "ParcaKategorileri",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UcBoyutModelParcalari_ParcaKategorileri_ParcaKategoriId",
                table: "UcBoyutModelParcalari");

            migrationBuilder.DropTable(
                name: "Firmalar");

            migrationBuilder.DropTable(
                name: "ParcaKategorileri");

            migrationBuilder.DropIndex(
                name: "IX_UcBoyutModelParcalari_FirmaId",
                table: "UcBoyutModelParcalari");

            migrationBuilder.DropIndex(
                name: "IX_UcBoyutModelParcalari_ParcaKategoriId",
                table: "UcBoyutModelParcalari");

            migrationBuilder.DropIndex(
                name: "IX_UcBoyutModeller_FirmaId",
                table: "UcBoyutModeller");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "UcBoyutModelParcalari");

            migrationBuilder.DropColumn(
                name: "ParcaKategoriId",
                table: "UcBoyutModelParcalari");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "UcBoyutModeller");
        }
    }
}
