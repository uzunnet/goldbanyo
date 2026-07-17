using System;
using System.IO;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        string dbPath = @"I:\goldbanyo_web\VizitLink3D.Api\vizitlink3d.db";

        Console.WriteLine("=== FIRMA TEMA DOGRULAMA ===");

        if (!File.Exists(dbPath))
        {
            Console.WriteLine($"HATA: Veritabanı dosyası bulunamadı: {dbPath}");
            return;
        }

        try
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            using (var oku = new SqliteCommand("""
SELECT Id, Slug, Ad, AdminTema, SiteTema
FROM Firmalar
WHERE Slug IN ('goldbanyo', 'goldbanyo-demo', 'VIZITLINK3D')
ORDER BY Id;
""", connection))
            using (var okuyucu = oku.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    Console.WriteLine($"{okuyucu.GetInt32(0)} | {okuyucu.GetString(1)} | {okuyucu.GetString(2)} | admin={okuyucu["AdminTema"]} | site={okuyucu["SiteTema"]}");
                }
            }

            using (var guncelle = new SqliteCommand("""
UPDATE Firmalar
SET SiteTema = 'gold',
    AdminTema = COALESCE(NULLIF(AdminTema, ''), 'endustri-karanlik')
WHERE Slug = 'goldbanyo';
""", connection))
            {
                int rows = guncelle.ExecuteNonQuery();
                Console.WriteLine($"Guncellenen satir: {rows}");
            }

            using (var tekrarOku = new SqliteCommand("""
SELECT Id, Slug, Ad, AdminTema, SiteTema
FROM Firmalar
WHERE Slug = 'goldbanyo';
""", connection))
            using (var okuyucu = tekrarOku.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    Console.WriteLine($"SONUC => {okuyucu.GetInt32(0)} | {okuyucu.GetString(1)} | {okuyucu.GetString(2)} | admin={okuyucu["AdminTema"]} | site={okuyucu["SiteTema"]}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HATA OLUŞTU: {ex.Message}");
        }
    }
}
