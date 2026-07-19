#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using System;
using Microsoft.Data.Sqlite;

var dbPath = @"i:\goldbanyo_web\VizitLink3D.Api\vizitlink3d.db";

using (var conn = new SqliteConnection($"Data Source={dbPath}"))
{
    conn.Open();

    var readCmd = conn.CreateCommand();
    readCmd.CommandText = "SELECT Id, FirmaId, BirincilDomain, LisansAnahtari FROM Lisanslar";
    using (var reader = readCmd.ExecuteReader())
    {
        while (reader.Read())
        {
            Console.WriteLine($"ID: {reader["Id"]}, Firma: {reader["FirmaId"]}, Domain: {reader["BirincilDomain"]}, LisansAnahtari: '{reader["LisansAnahtari"]}'");
        }
    }

    var updateCmd = conn.CreateCommand();
    updateCmd.CommandText = "UPDATE Lisanslar SET LisansAnahtari = '';";
    int affected = updateCmd.ExecuteNonQuery();

    Console.WriteLine($"\nGuncellenen lisans sayisi: {affected}");
}
