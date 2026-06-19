using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Desadoor.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class LisansDomainGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Lisanslar
                SET BirincilDomain = 'desadoor.uzunreklam.com',
                    YedekDomain = 'www.desadoor.uzunreklam.com',
                    GuncellenmeTarihi = CURRENT_TIMESTAMP
                WHERE FirmaId IN (SELECT Id FROM Firmalar WHERE Slug = 'desadoor')
                  AND BirincilDomain = 'desadoor.com.tr'
                  AND AktifMi = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Lisanslar
                SET BirincilDomain = 'desadoor.com.tr',
                    YedekDomain = 'www.desadoor.com.tr',
                    GuncellenmeTarihi = CURRENT_TIMESTAMP
                WHERE FirmaId IN (SELECT Id FROM Firmalar WHERE Slug = 'desadoor')
                  AND BirincilDomain = 'desadoor.uzunreklam.com'
                  AND AktifMi = 1;
                """);
        }
    }
}
