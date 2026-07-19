#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using System;
using Microsoft.Data.Sqlite;

var dbPath = @"i:\goldbanyo_web\VizitLink3D.Api\vizitlink3d.db";
Console.WriteLine($"Veritabanina baglaniliyor: {dbPath}");

using (var conn = new SqliteConnection($"Data Source={dbPath}"))
{
    conn.Open();

    var readCmd = conn.CreateCommand();
    readCmd.CommandText = "SELECT Id, FirmaId, BirincilDomain, BitisTarihi, LisansTipi FROM Lisanslar";
    using (var reader = readCmd.ExecuteReader())
    {
        while (reader.Read())
        {
            Console.WriteLine($"ID: {reader["Id"]}, Firma: {reader["FirmaId"]}, Domain: {reader["BirincilDomain"]}, Bitis: {reader["BitisTarihi"]}, Tip: {reader["LisansTipi"]}");
        }
    }

    var updateCmd = conn.CreateCommand();
    updateCmd.CommandText = "UPDATE Lisanslar SET AktifMi = 1, SuresizMi = 1, BirincilDomain = 'localhost', YedekDomain = '127.0.0.1', BitisTarihi = '9999-12-31 00:00:00', LisansTipi = 'Suresiz';";
    int affected = updateCmd.ExecuteNonQuery();

    Console.WriteLine($"\nGuncellenen lisans sayisi: {affected}");

    var readCmd2 = conn.CreateCommand();
    readCmd2.CommandText = "SELECT Id, FirmaId, BirincilDomain, BitisTarihi, LisansTipi FROM Lisanslar";
    using (var reader = readCmd2.ExecuteReader())
    {
        while (reader.Read())
        {
            Console.WriteLine($"ID: {reader["Id"]}, Firma: {reader["FirmaId"]}, Domain: {reader["BirincilDomain"]}, Bitis: {reader["BitisTarihi"]}, Tip: {reader["LisansTipi"]}");
        }
    }
}
