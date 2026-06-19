using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

class Program {
    static void Main() {
        string dbPath = @"i:\desedoorweb\Desadoor.Api\desadoor.db";
        string backupDir = @"i:\desedoorweb\Yedekler\db";
        
        Console.WriteLine("=== REFERANS EKLEME BAŞLADI ===");
        
        if (!File.Exists(dbPath)) {
            Console.WriteLine($"HATA: Veritabanı dosyası bulunamadı: {dbPath}");
            return;
        }

        try {
            if (!Directory.Exists(backupDir)) {
                Directory.CreateDirectory(backupDir);
            }
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(backupDir, $"desadoor_{timestamp}_guncelleme_oncesi.db");
            File.Copy(dbPath, backupPath, true);
            Console.WriteLine($"YEDEK ALINDI: {backupPath}");
        }
        catch (Exception ex) {
            Console.WriteLine($"YEDEKLEME HATASI: {ex.Message}");
            return;
        }

        var referanslar = new List<(string Ad, string Aciklama)>
        {
            ("SERTEPE İNŞAAT", "45 DAİRE"),
            ("ALPİŞ İNŞAAT", "120 DAİRE"),
            ("YG GÖKTAŞ İNŞ.", "96 DAİRE"),
            ("KUMOVA İNŞAAT", "196 DAİRE"),
            ("CELAL İNŞAAT", "40 DAİRE"),
            ("ULU ÇINAR", "16 VİLLA KOMPLE"),
            ("SADRİOĞULLARI İNŞ.", "200 DAİRE"),
            ("BEZEK MİMARLIK", "150 DAİRE"),
            ("FAHRETTİN DENGİZ İNŞ.", "60 DAİRE"),
            ("OLCAY ANIK İNŞAAT", "50 DAİRE"),
            ("SÜLEYMAN GARİP İNŞAAT", "30 DAİRE"),
            ("DİRLİK İNŞAAT", "35 DAİRE"),
            ("CEM İNŞAAT", "40 DAİRE"),
            ("KUDU İNŞAAT", "70 DAİRE"),
            ("SADİ ALAGÖZ İNŞAAT", "50 DAİRE"),
            ("KLAS İNŞAAT", "30 DAİRE"),
            ("YASİN TEKİN İNŞAAT", "40 DAİRE"),
            ("ŞURA İNŞAAT", "70 DAİRE"),
            ("AKAR İNŞAAT", "60 DAİRE"),
            ("EDT TEKSTİL", "30 DAİRE"),
            ("ZENGİN İNŞAAT", "60 DAİRE")
        };

        try {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            
            // Get current max SiraNo
            int maxSiraNo = 0;
            using (var cmd = new SqliteCommand("SELECT MAX(SiraNo) FROM Referanslar WHERE SilindiMi = 0", connection)) {
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null) {
                    maxSiraNo = Convert.ToInt32(result);
                }
            }

            int eklenen = 0;
            string nowStr = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var r in referanslar) {
                // Check if already exists
                using (var checkCmd = new SqliteCommand("SELECT COUNT(1) FROM Referanslar WHERE Ad = @Ad AND SilindiMi = 0", connection)) {
                    checkCmd.Parameters.AddWithValue("@Ad", r.Ad);
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count > 0) {
                        Console.WriteLine($"ATLANDI: {r.Ad} (Zaten var)");
                        continue;
                    }
                }

                maxSiraNo++;
                string sql = @"
                    INSERT INTO Referanslar (Ad, Tip, Aciklama, SiraNo, AktifMi, OlusturulmaTarihi, SilindiMi)
                    VALUES (@Ad, 'Müşteri', @Aciklama, @SiraNo, 1, @OlusturulmaTarihi, 0)";
                
                using (var cmd = new SqliteCommand(sql, connection)) {
                    cmd.Parameters.AddWithValue("@Ad", r.Ad);
                    cmd.Parameters.AddWithValue("@Aciklama", r.Aciklama);
                    cmd.Parameters.AddWithValue("@SiraNo", maxSiraNo);
                    cmd.Parameters.AddWithValue("@OlusturulmaTarihi", nowStr);
                    
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0) {
                        Console.WriteLine($"EKLENDI: {r.Ad} ({r.Aciklama})");
                        eklenen++;
                    }
                }
            }

            Console.WriteLine($"İŞLEM TAMAMLANDI. Toplam {eklenen} yeni referans eklendi.");
        }
        catch (Exception ex) {
            Console.WriteLine($"HATA OLUŞTU: {ex.Message}");
        }
    }
}
