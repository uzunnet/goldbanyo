using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Konfigurator.Api.VeriTabani.Migrations
{
    /// <inheritdoc />
    public partial class UcBoyutModelAdlandirma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tablo adı: GlbModeller → UcBoyutModeller
            // EF Core SQLite provider rename desteği ile index'ler de otomatik yeniden adlandırılır.
            migrationBuilder.RenameTable(
                name: "GlbModeller",
                newName: "UcBoyutModeller");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UcBoyutModeller",
                newName: "GlbModeller");
        }
    }
}
