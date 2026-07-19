#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using System;
using Microsoft.Data.Sqlite;

var dbPath = @"i:\goldbanyo_web\VizitLink3D.Api\vizitlink3d.db";

using (var conn = new SqliteConnection($"Data Source={dbPath}"))
{
    conn.Open();

    var readCmd = conn.CreateCommand();
    readCmd.CommandText = "SELECT Id, Slug, Domain, YedekDomain FROM Firmalar";
    using (var reader = readCmd.ExecuteReader())
    {
        while (reader.Read())
        {
            Console.WriteLine($"ID: {reader["Id"]}, Slug: {reader["Slug"]}, Domain: '{reader["Domain"]}', YedekDomain: '{reader["YedekDomain"]}'");
        }
    }
}
