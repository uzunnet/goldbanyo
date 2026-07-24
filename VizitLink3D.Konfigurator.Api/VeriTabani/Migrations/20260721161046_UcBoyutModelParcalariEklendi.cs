using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Konfigurator.Api.VeriTabani.Migrations
{
    /// <inheritdoc />
    public partial class UcBoyutModelParcalariEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UcBoyutModelParcalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    MeshAdi = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    GorunenAd = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ParcaTuru = table.Column<int>(type: "INTEGER", nullable: false),
                    RenkDegistirilebilirMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    GorunurMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    VarsayilanRenk = table.Column<string>(type: "TEXT", maxLength: 9, nullable: true),
                    VarsayilanMalzeme = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UcBoyutModelParcalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UcBoyutModelParcalari_UcBoyutModeller_ModelId",
                        column: x => x.ModelId,
                        principalTable: "UcBoyutModeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UcBoyutModelParcalari_ModelId_MeshAdi",
                table: "UcBoyutModelParcalari",
                columns: new[] { "ModelId", "MeshAdi" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UcBoyutModelParcalari");
        }
    }
}
