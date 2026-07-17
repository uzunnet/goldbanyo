---
name: Aurelian Onyx
colors:
  surface: '#121414'
  surface-dim: '#121414'
  surface-bright: '#37393a'
  surface-container-lowest: '#0c0f0f'
  surface-container-low: '#1a1c1c'
  surface-container: '#1e2020'
  surface-container-high: '#282a2b'
  surface-container-highest: '#333535'
  on-surface: '#e2e2e2'
  on-surface-variant: '#c4c7c7'
  inverse-surface: '#e2e2e2'
  inverse-on-surface: '#2f3131'
  outline: '#8e9192'
  outline-variant: '#444748'
  surface-tint: '#c9c6c5'
  primary: '#c9c6c5'
  on-primary: '#313030'
  primary-container: '#050505'
  on-primary-container: '#797777'
  inverse-primary: '#5f5e5e'
  secondary: '#c8c6c5'
  on-secondary: '#313030'
  secondary-container: '#4a4949'
  on-secondary-container: '#bab8b7'
  tertiary: '#e9c349'
  on-tertiary: '#3c2f00'
  tertiary-container: '#080500'
  on-tertiary-container: '#917400'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#e5e2e1'
  primary-fixed-dim: '#c9c6c5'
  on-primary-fixed: '#1c1b1b'
  on-primary-fixed-variant: '#474646'
  secondary-fixed: '#e5e2e1'
  secondary-fixed-dim: '#c8c6c5'
  on-secondary-fixed: '#1c1b1b'
  on-secondary-fixed-variant: '#474646'
  tertiary-fixed: '#ffe088'
  tertiary-fixed-dim: '#e9c349'
  on-tertiary-fixed: '#241a00'
  on-tertiary-fixed-variant: '#574500'
  background: '#121414'
  on-background: '#e2e2e2'
  surface-variant: '#333535'
typography:
  display-lg:
    fontFamily: Playfair Display
    fontSize: 64px
    fontWeight: '700'
    lineHeight: 72px
    letterSpacing: -0.02em
  display-lg-mobile:
    fontFamily: Playfair Display
    fontSize: 40px
    fontWeight: '700'
    lineHeight: 48px
    letterSpacing: -0.01em
  headline-md:
    fontFamily: Playfair Display
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
  headline-sm:
    fontFamily: Playfair Display
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  body-lg:
    fontFamily: Space Grotesk
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Space Grotesk
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-caps:
    fontFamily: Space Grotesk
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.1em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 8px
  container-max: 1440px
  gutter: 24px
  margin-desktop: 80px
  margin-mobile: 20px
  stack-sm: 16px
  stack-md: 32px
  stack-lg: 64px
---

## Brand & Style

The design system is engineered for a high-end luxury e-commerce experience, specifically targeting a sophisticated clientele seeking premium bathroom fixtures. The brand personality is one of exclusive opulence, architectural precision, and serene darkness. 

The aesthetic merges **Glassmorphism** with **Minimalism**, creating an environment where products are treated as art pieces. The interface should evoke the feeling of walking through a dimly lit, high-end showroom at night—focused, calm, and expensive. Visual interest is driven by high-contrast metallic accents, subtle glowing borders, and the depth created by translucent layers. Every interaction should feel intentional and weighted, reflecting the quality of the physical hardware being sold.

## Colors

The palette is rooted in a deep, nocturnal foundation. **Deep Onyx Black** serves as the primary canvas, providing an infinite sense of depth. **Matte Charcoal** is used for secondary surfaces and container backgrounds to define structure without breaking the dark immersion.

**Glowing Gold** is the sole accent color, reserved for high-priority calls to action, price points, and premium branding elements. It should be used sparingly to maintain its impact. **Frosted Glass** (white at varying low opacities) is utilized for borders, dividers, and overlays to create the glassmorphic effect. Text should primarily use high-brightness neutrals to ensure legibility against the dark void.

## Typography

The typographic strategy relies on a sharp contrast between traditional luxury and futuristic precision. 

**Playfair Display** provides the editorial authority required for a luxury brand. Headlines should feature tight letter-spacing and generous line-height to feel like a high-fashion magazine. **Space Grotesk** is used for all functional and body text. Its geometric, slightly technical character balances the serif's classicism, suggesting modern engineering and "smart" bathroom technology. 

Use `label-caps` for technical specifications and overlines to create a sense of organized, architectural data.

## Layout & Spacing

The layout follows a **Fixed Grid** model on desktop to maintain a curated, gallery-like feel. Content is centered within a 1440px container with wide 80px outer margins to allow the product photography to breathe. 

A 12-column system is used, but elements should often span larger blocks (e.g., 6 or 12 columns) to avoid a cluttered look. Spacing follows a strict 8px rhythmic scale. Use `stack-lg` for section breathing room to reinforce the minimalist aesthetic. On mobile, the grid collapses to a single column with increased vertical padding to maintain the premium, unhurried browsing experience.

## Elevation & Depth

Depth is not achieved through traditional drop shadows, but through **Tonal Layering and Glassmorphism**. 

1.  **Base:** Deep Onyx Black (#050505).
2.  **Mantle:** Matte Charcoal (#121212) surfaces for secondary sections.
3.  **Floating:** Frosted Glass containers using a backdrop-blur (12px to 20px) and a subtle 1px border of `rgba(255, 255, 255, 0.1)`.

For active or featured elements, apply a **Subtle Glowing Border**. This is a 1px solid stroke using the `border_glow` token, accompanied by a very soft outer glow (0px 0px 15px) of the same color to simulate the reflection of light on metallic gold hardware.

## Shapes

The design system utilizes **Soft** roundedness. While the overall vibe is architectural and sharp, a slight 4px (`0.25rem`) corner radius prevents the UI from feeling aggressive or "dated-web." 

Larger containers (Cards, Modals) may use `rounded-lg` (8px) to emphasize the "glass plate" feel. Interactive elements like buttons should remain strictly rectangular or use the minimal `0.25rem` radius to maintain a sophisticated, bespoke furniture appearance.

## Components

### Buttons
Primary buttons use a solid **Glowing Gold** background with black text. Secondary buttons are "Ghost" style: a 1px white-glass border with white text. All buttons should have a hover state that increases the border's glow intensity.

### Cards & Product Displays
Product cards are frameless, using only image and typography, or contained within a glassmorphic panel with a `backdrop-filter: blur(20px)`. The product image should appear to float over the background.

### Input Fields
Fields are represented by a single 1px bottom border (underline style) in white-glass. On focus, the border transitions to Gold with a subtle glow.

### Navigation
The header should be a sticky, frosted glass bar. Links use `label-caps` typography. The active link is indicated by a small gold dot beneath the text rather than an underline.

### Lists & Specs
Technical specifications for bathroom fixtures should be presented in a clean, two-column list with `body-md` text and 1px Matte Charcoal dividers, emphasizing the "spec-sheet" precision of high-end manufacturing.
