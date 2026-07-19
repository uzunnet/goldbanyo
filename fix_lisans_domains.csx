#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using System;
using Microsoft.Data.Sqlite;

var dbPath = @"i:\goldbanyo_web\VizitLink3D.Api\vizitlink3d.db";

using (var conn = new SqliteConnection($"Data Source={dbPath}"))
{
    conn.Open();

    var updateCmd = conn.CreateCommand();
    updateCmd.CommandText = "UPDATE Lisanslar SET AktifMi = 1, SuresizMi = 1, BirincilDomain = 'goldbanyom.com.tr', YedekDomain = 'localhost', LisansAnahtari = '', BitisTarihi = '9999-12-31 00:00:00', LisansTipi = 'Suresiz';";
    int affected = updateCmd.ExecuteNonQuery();

    Console.WriteLine($"\nGuncellenen lisans sayisi: {affected}");
}
