using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Konfigurator.Api.VeriTabani.Migrations
{
    /// <inheritdoc />
    public partial class KategoriTablosuVeModelKategoriId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KategoriId",
                table: "UcBoyutModeller",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UstKategoriId = table.Column<int>(type: "INTEGER", nullable: true),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kategoriler_Kategoriler_UstKategoriId",
                        column: x => x.UstKategoriId,
                        principalTable: "Kategoriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UcBoyutModeller_KategoriId",
                table: "UcBoyutModeller",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_Slug",
                table: "Kategoriler",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_UstKategoriId",
                table: "Kategoriler",
                column: "UstKategoriId");

            migrationBuilder.AddForeignKey(
                name: "FK_UcBoyutModeller_Kategoriler_KategoriId",
                table: "UcBoyutModeller",
                column: "KategoriId",
                principalTable: "Kategoriler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UcBoyutModeller_Kategoriler_KategoriId",
                table: "UcBoyutModeller");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropIndex(
                name: "IX_UcBoyutModeller_KategoriId",
                table: "UcBoyutModeller");

            migrationBuilder.DropColumn(
                name: "KategoriId",
                table: "UcBoyutModeller");
        }
    }
}
