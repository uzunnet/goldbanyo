using Microsoft.AspNetCore.SignalR;
using Desadoor.Api.Hubs;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Desadoor.Api.Moduller.Tema.Servisler;

public class StitchTemaServisi(
    IWebHostEnvironment ortam,
    IHubContext<TemaHub> temaHub,
    ILogger<StitchTemaServisi> log)
{
    private const string CSS_HEDEF = "wwwroot/css/sistem/temeller/degiskenler.css";
    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public async Task TokensCssUretAsync(string firmaId = "varsayilan")
    {
        try
        {
            var tasarimYolu = Path.Combine(ortam.ContentRootPath, "..", "tasarim", $"DESIGN{firmaSonek(firmaId)}.md");
            if (!File.Exists(tasarimYolu))
            {
                tasarimYolu = Path.Combine(ortam.ContentRootPath, "..", "tasarim", "DESIGN.md");
                if (!File.Exists(tasarimYolu))
                {
                    log.LogWarning("DESIGN.md bulunamadi: {Yol}", tasarimYolu);
                    return;
                }
            }

            var icerik = await File.ReadAllTextAsync(tasarimYolu);
            var css = DesignMdToCss(icerik);

            var hedefYol = Path.Combine(ortam.ContentRootPath, CSS_HEDEF);
            var dizin = Path.GetDirectoryName(hedefYol);
            if (!Directory.Exists(dizin)) Directory.CreateDirectory(dizin!);

            await File.WriteAllTextAsync(hedefYol, css, Encoding.UTF8);
            log.LogInformation("degiskenler.css uretildi. {Firma} {Boyut} byte", firmaId, css.Length);

            await temaHub.Clients.Group(firmaId).SendAsync("TemaGuncellendi", firmaId);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Tema CSS uretme hatasi");
        }
    }

    public string DesignMdToCss(string designMd)
    {
        var frontMatter = FrontMatterCikar(designMd);
        if (frontMatter is null) return VarsayilanCss();

        var css = new StringBuilder();
        css.AppendLine("/*");
        css.AppendLine(" * ═══════════════════════════════════════════════════");
        css.AppendLine(" * DESADOOR — DEGISKENLER (degiskenler.css)");
        css.AppendLine(" * StitchTemaServisi tarafindan otomatik uretilmistir.");
        css.AppendLine(" * Elle duzenleme YAPMAYIN — DESIGN.md uzerinden yonetin.");
        css.AppendLine(" * ═══════════════════════════════════════════════════");
        css.AppendLine(" */");
        css.AppendLine();
        css.AppendLine(":root {");

        using var doc = JsonDocument.Parse(frontMatter);
        var root = doc.RootElement;

        if (root.TryGetProperty("tokens", out var tokens))
        {
            CssRenkUret(css, tokens);
            CssTipografiUret(css, tokens);
            CssBoslukUret(css, tokens);
            CssGolgeUret(css, tokens);
            CssGecisUret(css, tokens);
            CssAdminUret(css, tokens);
        }

        css.AppendLine("}");
        return css.ToString();
    }

    private static void CssRenkUret(StringBuilder css, JsonElement tokens)
    {
        if (!tokens.TryGetProperty("color", out var c)) return;
        css.AppendLine();
        css.AppendLine("    /* ═══ ANA RENK PALETI ═══ */");
        RenkEkle(css, "--desa-primary", c, "primary");
        RenkEkle(css, "--desa-primary-light", c, "primary-light");
        RenkEkle(css, "--desa-primary-dark", c, "primary-dark");
        RenkEkle(css, "--desa-secondary", c, "secondary");
        RenkEkle(css, "--desa-accent", c, "accent");
        RenkEkle(css, "--desa-accent-light", c, "accent-light");
        RenkEkle(css, "--desa-accent-dark", c, "accent-dark");
        RenkEkle(css, "--desa-gold", c, "gold");
        css.AppendLine();
        css.AppendLine("    /* ═══ ARKAPLAN VE YUZEY ═══ */");
        RenkEkle(css, "--desa-bg-base", c, "bg-base");
        RenkEkle(css, "--desa-bg-alt", c, "bg-alt");
        RenkEkle(css, "--desa-bg-dark", c, "bg-dark");
        RenkEkle(css, "--desa-bg-section", c, "bg-section");
        RenkEkle(css, "--desa-surface", c, "surface");
        RenkEkle(css, "--desa-border", c, "border");
        css.AppendLine();
        css.AppendLine("    /* ═══ METIN ═══ */");
        RenkEkle(css, "--desa-text", c, "text");
        RenkEkle(css, "--desa-text-secondary", c, "text-secondary");
        RenkEkle(css, "--desa-text-muted", c, "text-muted");
        RenkEkle(css, "--desa-text-light", c, "text-light");
        RenkEkle(css, "--desa-text-inverse", c, "text-inverse");
        css.AppendLine();
        css.AppendLine("    /* ═══ DURUM RENKLERİ ═══ */");
        RenkEkle(css, "--desa-success", c, "success");
        RenkEkle(css, "--desa-warning", c, "warning");
        RenkEkle(css, "--desa-error", c, "error");
        RenkEkle(css, "--desa-info", c, "info");
    }

    private static void CssTipografiUret(StringBuilder css, JsonElement tokens)
    {
        if (!tokens.TryGetProperty("typography", out var t)) return;
        css.AppendLine();
        css.AppendLine("    /* ═══ TIPOGRAFI ═══ */");
        FontEkle(css, "--desa-font-serif", t, "heading");
        FontEkle(css, "--desa-font-sans", t, "body");
        FontEkle(css, "--desa-font-accent", t, "accent");
        FontEkle(css, "--desa-font-mono", t, "mono");
    }

    private static void CssBoslukUret(StringBuilder css, JsonElement tokens)
    {
        if (!tokens.TryGetProperty("spacing", out var s)) return;
        css.AppendLine();
        css.AppendLine("    /* ═══ BOSLUK ═══ */");
        UzunlukEkle(css, "--desa-space-xs", s, "xs");
        UzunlukEkle(css, "--desa-space-sm", s, "sm");
        UzunlukEkle(css, "--desa-space-md", s, "md");
        UzunlukEkle(css, "--desa-space-lg", s, "lg");
        UzunlukEkle(css, "--desa-space-xl", s, "xl");
        UzunlukEkle(css, "--desa-space-2xl", s, "2xl");
        UzunlukEkle(css, "--desa-space-3xl", s, "3xl");
    }

    private static void CssGolgeUret(StringBuilder css, JsonElement tokens)
    {
        if (!tokens.TryGetProperty("shadow", out var s)) return;
        css.AppendLine();
        css.AppendLine("    /* ═══ GOLGE ═══ */");
        GolgeEkle(css, "--desa-shadow-sm", s, "sm");
        GolgeEkle(css, "--desa-shadow-md", s, "md");
        GolgeEkle(css, "--desa-shadow-lg", s, "lg");
        GolgeEkle(css, "--desa-shadow-xl", s, "xl");
        GolgeEkle(css, "--desa-shadow-lux", s, "lux");
    }

    private static void CssGecisUret(StringBuilder css, JsonElement tokens)
    {
        if (!tokens.TryGetProperty("animation", out var a)) return;
        css.AppendLine();
        css.AppendLine("    /* ═══ GECIS VE ANIMASYON ═══ */");
        GecisEkle(css, "--desa-transition-fast", a, "fast");
        GecisEkle(css, "--desa-transition", a, "normal");
        GecisEkle(css, "--desa-transition-slow", a, "slow");
        css.AppendLine();
        css.AppendLine("    /* ═══ KENAR YARICAPI ═══ */");
        css.AppendLine("    --desa-radius-none: 0px;");
        css.AppendLine("    --desa-radius-sm: 2px;");
        css.AppendLine("    --desa-radius-md: 4px;");
        css.AppendLine("    --desa-radius-lg: 0px;");
        css.AppendLine();
        css.AppendLine("    /* ═══ RESPONSIVE ═══ */");
        css.AppendLine("    --desa-screen-mobile: 480px;");
        css.AppendLine("    --desa-screen-tablet: 768px;");
        css.AppendLine("    --desa-screen-desktop: 1280px;");
    }

    private static void CssAdminUret(StringBuilder css, JsonElement tokens)
    {
        if (!tokens.TryGetProperty("admin", out var a)) return;
        css.AppendLine();
        css.AppendLine("    /* ═══ ADMIN PANEL ═══ */");
        RenkEkle(css, "--admin-bg", a, "bg");
        RenkEkle(css, "--admin-surface", a, "surface");
        RenkEkle(css, "--admin-border", a, "border");
        RenkEkle(css, "--admin-accent", a, "accent");
        RenkEkle(css, "--admin-text", a, "text");
        RenkEkle(css, "--admin-text-muted", a, "text-muted");
        css.AppendLine("    --admin-shadow: 0 4px 6px -1px rgba(0,0,0,0.05), 0 2px 4px -1px rgba(0,0,0,0.03);");
        css.AppendLine("    --admin-radius: 0px;");
        css.AppendLine("    --admin-transition: all 0.3s cubic-bezier(0.4,0,0.2,1);");
    }

    private static void RenkEkle(StringBuilder css, string degisken, JsonElement parent, string key)
    {
        if (parent.TryGetProperty(key, out var t) && t.TryGetProperty("value", out var v))
            css.AppendLine($"    {degisken}: {v.GetString()};");
    }

    private static void FontEkle(StringBuilder css, string degisken, JsonElement parent, string key)
    {
        if (parent.TryGetProperty(key, out var t) && t.TryGetProperty("value", out var v))
            css.AppendLine($"    {degisken}: '{v.GetString()}', serif;");
    }

    private static void UzunlukEkle(StringBuilder css, string degisken, JsonElement parent, string key)
    {
        if (parent.TryGetProperty(key, out var t) && t.TryGetProperty("value", out var v))
            css.AppendLine($"    {degisken}: {v.GetString()};");
    }

    private static void GolgeEkle(StringBuilder css, string degisken, JsonElement parent, string key)
    {
        if (parent.TryGetProperty(key, out var t) && t.TryGetProperty("value", out var v))
            css.AppendLine($"    {degisken}: {v.GetString()};");
    }

    private static void GecisEkle(StringBuilder css, string degisken, JsonElement parent, string key)
    {
        if (parent.TryGetProperty(key, out var t) && t.TryGetProperty("value", out var v))
            css.AppendLine($"    {degisken}: {v.GetString()};");
    }

    private static string? FrontMatterCikar(string icerik)
    {
        var match = Regex.Match(icerik, @"^---\s*\n(.*?)\n---", RegexOptions.Singleline);
        if (!match.Success) return null;

        var yaml = match.Groups[1].Value;
        return YamlToJson(yaml);
    }

    private static string YamlToJson(string yaml)
    {
        var json = new StringBuilder();
        json.AppendLine("{");

        var lines = yaml.Split('\n');
        var stack = new Stack<string>();
        foreach (var line in lines)
        {
            var trimmed = line.TrimEnd();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.TrimStart().StartsWith("#")) continue;

            var currentIndent = (line.Length - line.TrimStart().Length) / 2;
            while (stack.Count > currentIndent) { json.AppendLine(new string(' ', stack.Count * 2) + "}"); stack.Pop(); }

            if (trimmed.EndsWith(":"))
            {
                var key = trimmed.TrimEnd(':').Trim();
                json.AppendLine(new string(' ', currentIndent * 2) + $"\"{key}\": {{");
                stack.Push(key);
            }
            else
            {
                var colonIdx = trimmed.IndexOf(':');
                if (colonIdx > 0)
                {
                    var key = trimmed[..colonIdx].Trim();
                    var value = trimmed[(colonIdx + 1)..].Trim().Trim('"');
                    json.AppendLine(new string(' ', currentIndent * 2) + $"\"{key}\": \"{value}\",");
                }
            }
        }

        while (stack.Count > 0) { json.AppendLine(new string(' ', stack.Count * 2) + "}"); stack.Pop(); }
        json.AppendLine("}");

        return json.ToString().Replace(",\n}", "\n}").Replace(",\n  }", "\n  }");
    }

    private static string VarsayilanCss()
    {
        return @":root {
    --desa-primary: #0a0a0a;
    --desa-accent: #c19b76;
    --desa-gold: #C5A059;
    --desa-bg-base: #ffffff;
    --desa-bg-dark: #0a0a0a;
    --desa-text: #1A1A1A;
    --desa-text-inverse: #FFFFFF;
    --desa-font-serif: 'Noto Serif', serif;
    --desa-font-sans: 'Manrope', sans-serif;
    --admin-accent: #C5A059;
}";
    }

    private static string firmaSonek(string firmaId) => firmaId == "varsayilan" ? "" : $"_{firmaId}";
}
