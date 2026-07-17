using Microsoft.Data.Sqlite;

// Test temizligi: yukleme testi sirasinda olusan gecici medya kayitlarini
// ve fiziksel dosyalarini kaldirir.
string dbYolu = @"I:\goldbanyo_web\VizitLink3D.Api\vizitlink3d.db";
long[] silinecekIdler = [35, 36];

using var conn = new SqliteConnection($"Data Source={dbYolu}");
conn.Open();

foreach (var id in silinecekIdler)
{
    string? dosyaYolu = null;
    using (var sec = conn.CreateCommand())
    {
        sec.CommandText = "SELECT DosyaYolu FROM Medyalar WHERE Id = @id";
        sec.Parameters.AddWithValue("@id", id);
        dosyaYolu = sec.ExecuteScalar() as string;
    }

    using (var sil = conn.CreateCommand())
    {
        sil.CommandText = "DELETE FROM Medyalar WHERE Id = @id";
        sil.Parameters.AddWithValue("@id", id);
        int n = sil.ExecuteNonQuery();
        Console.WriteLine($"Id={id}: DB kaydi silindi ({n}), DosyaYolu={dosyaYolu}");
    }

    if (!string.IsNullOrEmpty(dosyaYolu))
    {
        var tamYol = Path.Combine(@"I:\goldbanyo_web\VizitLink3D.Api\wwwroot", dosyaYolu.TrimStart('/', '\\'));
        if (File.Exists(tamYol))
        {
            File.Delete(tamYol);
            Console.WriteLine($"  Fiziksel dosya silindi: {tamYol}");
        }
        else
        {
            Console.WriteLine($"  Fiziksel dosya bulunamadi: {tamYol}");
        }
    }
}

Console.WriteLine("Temizlik tamamlandi.");
