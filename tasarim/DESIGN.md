---
name: "DesaDoor Industrial Luxury"
version: 1.0
firma: "DesaDoor A.Ş."
tokens:
  color:
    primary:
      value: "#0a0a0a"
      type: color
      description: "Ana endüstriyel siyah"
    primary-light:
      value: "#1a1a1a"
      type: color
    secondary:
      value: "#1C1C1C"
      type: color
      description: "İkincil koyu ton"
    accent:
      value: "#c19b76"
      type: color
      description: "Bronz vurgu"
    accent-light:
      value: "#d4a574"
      type: color
      description: "Açık bronz hover"
    accent-dark:
      value: "#a0784c"
      type: color
    gold:
      value: "#C5A059"
      type: color
      description: "DesaDoor altın (admin)"
    bg-base:
      value: "#ffffff"
      type: color
    bg-alt:
      value: "#F8F8F8"
      type: color
    bg-dark:
      value: "#0a0a0a"
      type: color
    bg-section:
      value: "#F5F5F0"
      type: color
    surface:
      value: "#ffffff"
      type: color
    border:
      value: "#E8E8E8"
      type: color
    text:
      value: "#1A1A1A"
      type: color
    text-secondary:
      value: "#4A4A4A"
      type: color
    text-muted:
      value: "#888888"
      type: color
    text-light:
      value: "#AAAAAA"
      type: color
    text-inverse:
      value: "#FFFFFF"
      type: color
    success:
      value: "#4a7c59"
      type: color
    warning:
      value: "#c9a449"
      type: color
    error:
      value: "#9b3d3d"
      type: color
    info:
      value: "#4a6c8c"
      type: color
  typography:
    heading:
      value: "Noto Serif"
      type: fontFamily
    body:
      value: "Manrope"
      type: fontFamily
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
      value: "0 2px 8px rgba(0,0,0,0.04)"
      type: shadow
    md:
      value: "0 4px 20px rgba(0,0,0,0.06)"
      type: shadow
    lg:
      value: "0 10px 40px rgba(0,0,0,0.10)"
      type: shadow
    xl:
      value: "0 20px 60px rgba(0,0,0,0.15)"
      type: shadow
    lux:
      value: "0 20px 60px rgba(193,155,118,0.12)"
      type: shadow
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
  admin:
    bg:
      value: "#F8FAFC"
      type: color
    surface:
      value: "#FFFFFF"
      type: color
    border:
      value: "#E2E8F0"
      type: color
    accent:
      value: "#C5A059"
      type: color
    text:
      value: "#000000"
      type: color
    text-muted:
      value: "#666666"
      type: color
---

# DesaDoor Industrial Luxury — Design System

## Renk Paleti

Endüstriyel lüks teması: koyu siyah zemin, bronz/altın vurgular, temiz beyaz yüzeyler.

## Tipografi

- **Başlıklar:** Noto Serif (serif, lüks his)
- **Gövde:** Manrope (geometric sans, modern)
- **Vurgu:** Cormorant Garamond (elegant serif)
- **Kod:** JetBrains Mono

## Kullanım

Bu dosya `StitchTemaServisi` tarafından okunur ve `degiskenler.css` otomatik üretilir.
Admin panelden tema değişikliği yapıldığında SignalR ile tüm açık tarayıcılara anında yansıtılır.

## Multi-Tenant Desteği

Her firma için ayrı DESIGN.md dosyası: `tasarim/DESIGN_{firmaId}.md`
