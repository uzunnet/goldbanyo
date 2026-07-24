using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Konfigurator.Api.VeriTabani.Migrations
{
    /// <inheritdoc />
    public partial class GlbModelEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlbModeller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    DosyaAdi = table.Column<string>(type: "TEXT", nullable: false),
                    DosyaYolu = table.Column<string>(type: "TEXT", nullable: false),
                    IcerikTuru = table.Column<string>(type: "TEXT", nullable: false),
                    BoyutBayt = table.Column<long>(type: "INTEGER", nullable: false),
                    Sha256Hash = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlbModeller", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlbModeller_Slug",
                table: "GlbModeller",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlbModeller");
        }
    }
}
