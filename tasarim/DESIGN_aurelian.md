---
name: "Aurelian Onyx"
version: 1.0
firma: "Gold Banyo A.Ş."
aciklama: "Stitch AI'dan alınan lüks karanlık tema — glassmorphism, glowing gold, Playfair Display"
tokens:
  color:
    primary:
      value: "#050505"
      type: color
      description: "Derin Onyx Siyah — ana zemin"
    primary-light:
      value: "#1e2020"
      type: color
    primary-dark:
      value: "#000000"
      type: color
    secondary:
      value: "#1a1a1a"
      type: color
    accent:
      value: "#d4af37"
      type: color
      description: "Glowing Gold — ana vurgu (Stitch tertiary override)"
    accent-light:
      value: "#e9c349"
      type: color
      description: "Açık altın hover/parlak"
    accent-dark:
      value: "#a07020"
      type: color
    gold:
      value: "#d4af37"
      type: color
    bg-base:
      value: "#121414"
      type: color
    bg-alt:
      value: "#1a1a1a"
      type: color
    bg-dark:
      value: "#050505"
      type: color
    bg-section:
      value: "#161818"
      type: color
    surface:
      value: "#1e2020"
      type: color
    surface-hover:
      value: "#252828"
      type: color
    border:
      value: "rgba(255,255,255,0.1)"
      type: color
      description: "Frosted glass border"
    text:
      value: "#e2e2e2"
      type: color
    text-secondary:
      value: "rgba(255, 255, 255, 0.70)"
      type: color
    text-muted:
      value: "rgba(255, 255, 255, 0.40)"
      type: color
    text-light:
      value: "rgba(255, 255, 255, 0.20)"
      type: color
    text-inverse:
      value: "#121414"
      type: color
    success:
      value: "#4a7c59"
      type: color
    warning:
      value: "#c9a449"
      type: color
    error:
      value: "#ffb4ab"
      type: color
    info:
      value: "#4a6c8c"
      type: color
  typography:
    heading:
      value: "Playfair Display"
      type: fontFamily
      description: "Editoryal lüks serif, başlıklar için"
    body:
      value: "Space Grotesk"
      type: fontFamily
      description: "Geometrik sans, gövde için"
    accent:
      value: "Cormorant Garamond"
      type: fontFamily
    mono:
      value: "JetBrains Mono"
      type: fontFamily
  spacing:
    xs:
      value: "0.25rem"
      type: dimension
    sm:
      value: "0.5rem"
      type: dimension
    md:
      value: "1rem"
      type: dimension
    lg:
      value: "1.5rem"
      type: dimension
    xl:
      value: "2.5rem"
      type: dimension
    2xl:
      value: "4rem"
      type: dimension
    3xl:
      value: "6rem"
      type: dimension
  shadow:
    sm:
      value: "0 2px 8px rgba(0,0,0,0.5)"
      type: shadow
    md:
      value: "0 4px 20px rgba(0,0,0,0.6)"
      type: shadow
    lg:
      value: "0 10px 40px rgba(0,0,0,0.7)"
      type: shadow
    xl:
      value: "0 20px 60px rgba(0,0,0,0.8)"
      type: shadow
    lux:
      value: "0 0 15px rgba(212,175,55,0.25)"
      type: shadow
      description: "Altın glow"
  animation:
    fast:
      value: "0.15s ease"
      type: transition
    normal:
      value: "0.3s cubic-bezier(0.4,0,0.2,1)"
      type: transition
    slow:
      value: "0.6s cubic-bezier(0.4,0,0.2,1)"
      type: transition
  glassmorphism:
    blur:
      value: "blur(20px) saturate(180%)"
      type: string
    border-opacity:
      value: "0.10"
      type: string
    bg-opacity:
      value: "0.06"
      type: string
---

# AURELIAN ONYX — Stitch AI Lüks Teması

## Kimlik
Stitch (stitch.withgoogle.com) AI tasarım üreticisinden alınmıştır. Glassmorphism + Minimalism estetiğini birleştirir. Yüksek kontrastlı metalik vurgular, ince parlayan kenarlıklar ve yarı saydam katmanların derinliği ile ürünleri sanat eseri olarak sunar.

## Kullanım
Bu dosya `CokluTemaServisi.cs` tarafından okunur ve `:root[data-site-tema="aurelian-onyx"]` override'ı otomatik üretilir. Admin panelden tema seçildiğinde SignalR üzerinden tüm açık tarayıcılara anında yansır.

## Tema Özellikleri
- **Renk:** Derin Onyx Siyah (#050505) + Glowing Gold (#d4af37)
- **Tipografi:** Playfair Display (başlık) + Space Grotesk (gövde)
- **Geometri:** 4px corner radius, 8px spacing scale
- **İmza Efekt:** Glassmorphism (backdrop-filter blur 20px, rgba 255,255,255,0.1 border)
- **Vurgu:** Subtle Glowing Border (1px solid + 0 0 15px outer glow)

## Erişim
- Admin: `/admin/tema-yonetimi` → "Aurelian Onyx" kartı
- API: `POST /api/tema/aktif` → `{"temaAd": "aurelian-onyx"}`
- Frontend: `data-site-tema="aurelian-onyx"` attribute
