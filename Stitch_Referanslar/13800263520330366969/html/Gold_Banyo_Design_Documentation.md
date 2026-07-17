# Gold Banyo Design System & Implementation Guide

## 1. Architectural Vision
The Gold Banyo digital ecosystem is built on the principle of "Adaptive Luxury." The interface must feel like a high-end physical showroom—minimalist, architectural, and responsive to the "rhythm of light."

## 2. Dual-Theme Strategy (Day & Night)
The system utilizes two distinct visual modes that can be toggled or triggered by environmental variables.

### Day Mode (Aura Luxury)
- **Palette**: Warm whites (`#FFF9EE`), soft beiges, and brushed gold accents.
- **Atmosphere**: Bright, airy, and editorial. Mimics a showroom bathed in natural morning light.
- **Usage**: General browsing, standard product lines.

### Night Mode (Onyx Architectural)
- **Palette**: Deep onyx blacks (`#121414`), charcoal greys, and high-contrast gold highlights.
- **Atmosphere**: Dramatic, prestigious, and intimate. Mimics a luxury gallery at night with focused architectural lighting.
- **Usage**: Exclusive collections, 3D configuration stages, high-end "Onyx" series.

## 3. Interaction & Motion Models
Animation is not decorative; it is functional and "industrial."

### Entry Animations (Choreography)
- **Staggered Reveal**: Elements enter the stage from bottom to top with a 20px vertical slide and a 600ms ease-out.
- **Hierarchical Loading**: Navigation first, then the Hero imagery, then functional UI elements.

### Product Micro-interactions
- **The "Glint" Effect**: A subtle diagonal light sweep across product cards on hover to emphasize the "Gold" finish.
- **Spatial Expansion**: Product cards use a "popup" transition (scale 1.02) to reveal secondary details without leaving the page.

### 3D Configuration Stage (Three.js)
- **Orbit Model**: Full 360° rotation with damped friction for a "heavy, high-quality" feel.
- **Real-time Material Swap**: Instantaneous texture updates for cabinet finishes (Nero Marquina, Walnut, etc.) while preserving lighting and shadows.

## 4. Page Architecture
- **Collections**: Dynamic grid with smart recommendations based on "Most Viewed" and "Customer Favorites."
- **Details**: Multi-layered layout where technical specs (2D) exist alongside the interactive configurator (3D).
- **Navigation**: Persistent but translucent, using backdrop-blur to maintain context with the background imagery or shaders.

## 5. Development Specs
- **Grid**: 12-column architectural grid with 32px gutters.
- **Typography**: Playfair Display (Headlines) for elegance; Overpass (Body) for technical clarity.
- **Components**: Capsule buttons with 40px radius; glassmorphic containers for UI overlays.