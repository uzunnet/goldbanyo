using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Konfigurator.Api.VeriTabani.Migrations
{
    /// <inheritdoc />
    public partial class SifreSifirlamaIstegiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SifreSifirlamaIstekleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KullaniciId = table.Column<int>(type: "INTEGER", nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KullanildiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    KullanilmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SifreSifirlamaIstekleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SifreSifirlamaIstekleri_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SifreSifirlamaIstekleri_KullaniciId",
                table: "SifreSifirlamaIstekleri",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_SifreSifirlamaIstekleri_TokenHash",
                table: "SifreSifirlamaIstekleri",
                column: "TokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SifreSifirlamaIstekleri");
        }
    }
}
