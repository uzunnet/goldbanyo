using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Compression;

namespace Desadoor.Api.Kontrolculer;

[ApiController]
[Route("api/db-yukle")]
public class DbYukleKontrolcu(IWebHostEnvironment env) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Yukle(IFormFile dosya)
    {
        if (dosya is null || dosya.Length == 0)
            return BadRequest("Dosya yok.");

        var hedefYolu = Path.Combine(env.ContentRootPath, "desadoor.db");
        var yedekYolu = hedefYolu + ".backup_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        if (System.IO.File.Exists(hedefYolu))
            System.IO.File.Copy(hedefYolu, yedekYolu, true);

        if (dosya.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            var geciciYol = Path.Combine(Path.GetTempPath(), "db_upload.zip");
            await using (var s = new FileStream(geciciYol, FileMode.Create))
                await dosya.CopyToAsync(s);

            using var archive = ZipFile.OpenRead(geciciYol);
            var dbEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".db"));
            if (dbEntry is null)
                return BadRequest("ZIP icinde .db dosyasi bulunamadi.");

            dbEntry.ExtractToFile(hedefYolu, overwrite: true);
            System.IO.File.Delete(geciciYol);
        }
        else
        {
            await using var stream = new FileStream(hedefYolu, FileMode.Create);
            await dosya.CopyToAsync(stream);
        }

        var boyut = new FileInfo(hedefYolu).Length;
        return Ok($"DB yuklendi: {boyut} byte. Yedek: {Path.GetFileName(yedekYolu)}");
    }
}
