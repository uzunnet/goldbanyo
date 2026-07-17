---
name: Gold Banyo Gold
colors:
  primary: '#1A1A27'
  secondary: '#C8952A'
  accent: '#E8C896'
  accent-light: '#F3DEC1'
  accent-dark: '#9E6C17'
  bg-base: '#F8F6F2'
  bg-alt: '#FFFDFC'
  surface: '#FFFFFF'
  surface-hover: '#F3EEE4'
  border: '#DCC9A1'
  text: '#241F17'
  text-secondary: '#5B5143'
  text-muted: '#867A68'
  text-inverse: '#FFF9EF'
  success: '#4A7C59'
  warning: '#C9A449'
  error: '#9B3D3D'
  info: '#4A6C8C'
typography:
  heading: Cormorant Garamond
  body: Manrope
  accent: Noto Serif
  mono: JetBrains Mono
spacing:
  xs: 0.375rem
  sm: 0.75rem
  md: 1rem
  lg: 1.5rem
  xl: 2.5rem
radius:
  sm: 0.35rem
  md: 0.75rem
  lg: 1.25rem
shadow:
  soft: 0 16px 40px rgba(26, 26, 39, 0.08)
  medium: 0 20px 60px rgba(37, 29, 16, 0.14)
  glow: 0 0 24px rgba(200, 149, 42, 0.25)
motion:
  speed: medium
  reveal: soft-rise
  hover-lift: 8
---

## Brand & Style

This design system is for **Gold Banyo**, a premium bathroom furniture brand that must feel crafted, luminous, and architectural. The experience is not a generic dark luxury store. It combines warm ivory surfaces, brushed gold highlights, smoked graphite anchors, and boutique showroom pacing.

The site should feel like walking through a custom bathroom gallery in Bursa: elegant, tactile, and highly curated. Gold is not a background flood; it is a controlled premium accent used on trims, details, dividers, focused calls-to-action, and key product moments.

## Theme Philosophy

There are **two separate theme domains**:

1. **Admin theme**
   Must remain independent, utility-first, readable, and stable. No luxury motion experimentation here.
2. **Frontend site theme**
   Must be showroom-like, animated, layered, and template-driven. Each firm can own a different site identity without affecting admin.

Gold Banyo frontend templates must support multiple variants, but the canonical base is `gold`. Derivatives may include lighter and darker sub-variants, yet they must remain recognizably Gold Banyo.

## Visual Language

Use a composition based on:

- warm light backgrounds,
- dark graphite anchors,
- brushed gold details,
- wide spacing,
- strong editorial headings,
- soft glass panels,
- layered category blocks,
- premium reveal animations.

Avoid a fully black page as the default. The homepage must alternate between bright ivory sections and darker contrast scenes so the gold accent feels alive.

## Homepage Direction

The homepage should include:

- a cinematic hero with layered headline, subtext, CTA, and refined motion,
- collection bento sections,
- premium bathroom cabinet categories,
- craftsmanship / production credibility,
- featured models such as Diago 100 and Diago 360,
- catalog CTA,
- proposal/contact CTA,
- smooth transitions between sections.

No door-focused wording, no industrial door branding, no Desadoor copy. Desadoor is only a rhythm and interaction reference.

## Motion & Effects

Motion must be visible and intentional:

- staggered reveal on section entry,
- image parallax or slow drift,
- card hover lift with glow edge,
- CTA shimmer or subtle gold sweep,
- timeline-like section transitions,
- sticky or layered hero behavior when appropriate.

Animations should feel premium and calm, not playful.

## Component Notes

Buttons should have a tailored luxury feel with subtle metallic borders or filled gold states. Product cards should emphasize imagery first, then model name and short descriptor. Detail pages should have animated galleries, spec transitions, and visually distinct CTA zones.

## Non-Negotiables

- The slug and core identity must stay `gold`.
- Frontend template logic must not alter admin appearance.
- Every generated output must map into `manifest.json`, `tokens.css`, `bilesenler.css`, and `animasyonlar.css`.
- If another DESIGN file exists, Gold Banyo should still prefer this source unless an explicit override path is supplied.
