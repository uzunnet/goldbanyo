using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace GorselAraci
{
    class Program
    {
        static void Main(string[] args)
        {
            int width = 800;
            int height = 800;
            string logoPath = @"I:\vizitlink3d_resimler\goldbanyo\logo.png";
            string outputPath = @"I:\vizitlink3d_resimler\vizitlink3d_default.png";
            string outputPath2 = @"I:\goldbanyo_web\VizitLink3D.UI\wwwroot\medya\vizitlink3d_default.png";

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;

                    // Fill background
                    g.Clear(ColorTranslator.FromHtml("#1a1a1a"));

                    if (File.Exists(logoPath))
                    {
                        using (Image logo = Image.FromFile(logoPath))
                        {
                            // max width 400
                            int targetW = 400;
                            int targetH = (int)((float)logo.Height * ((float)targetW / (float)logo.Width));
                            int x = (width - targetW) / 2;
                            int y = (height - targetH) / 2 - 50;

                            g.DrawImage(logo, new Rectangle(x, y, targetW, targetH));
                        }
                    }

                    using (Font font = new Font("Arial", 24, FontStyle.Bold))
                    {
                        string text = "GÖRSEL BULUNAMADI\nIMAGE NOT AVAILABLE";
                        SizeF textSize = g.MeasureString(text, font);
                        using (SolidBrush brush = new SolidBrush(ColorTranslator.FromHtml("#b39a4d")))
                        {
                            StringFormat sf = new StringFormat();
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;

                            RectangleF rect = new RectangleF(0, height / 2 + 100, width, 100);
                            g.DrawString(text, font, brush, rect, sf);
                        }
                    }
                }

                bmp.Save(outputPath, ImageFormat.Png);
                
                // Also copy to UI wwwroot
                if (Directory.Exists(Path.GetDirectoryName(outputPath2)))
                {
                    bmp.Save(outputPath2, ImageFormat.Png);
                }

                Console.WriteLine("Image generated successfully!");
            }
        }
    }
}
