# Desolo Zantas Mountain Features - Development Checklist

## 1. Ruins (Placeholder Building)
- [ ] **Geometry**: Create large rectangular building structure
- [ ] **Color**: Change from current to purple/magenta theme
- [ ] **Scale**: Adjust size to fit mountain landscape proportions
- [ ] **Materials**: Apply purple/magenta MTL materials
- [ ] **Details**: Add weathered/ancient ruin textures
- [ ] **Integration**: Position on mountain terrain

## 2. Snowdin Slope City
- [ ] **Terrain**: Create sloped terrain base
- [ ] **Vegetation**: Place multiple Spruce trees (reuse/instance existing models)
- [ ] **Crystals**: Add crystallized gems/diamond formations
  - [ ] Model or import diamond/crystal geometry
  - [ ] Apply emissive cyan/blue materials
  - [ ] Distribute across slope naturally
- [ ] **Architecture**: Create miniature houses (multiple variations)
  - [ ] Design small house models
  - [ ] Apply winter/snow-themed colors
  - [ ] Place throughout slope city
- [ ] **Lighting**: Add ambient glow from crystals
- [ ] **Integration**: Connect to main mountain path

## 3. WaterEdgeFall (Tether Elevator)
- [ ] **Elevator**: Create tether/cable elevator structure
- [ ] **Floating Islands**: Design mini floating island lake platforms
  - [ ] Create island geometry
  - [ ] Add water surface shader
  - [ ] Scale appropriately (small/mini)
- [ ] **Tower**: Modify existing tower
  - [ ] **Color**: Recolor to match Halcandra warm theme (orange/gold)
  - [ ] **Texture**: Update materials for consistency
- [ ] **Waterfall**: Create flowing water effect
  - [ ] **Color**: Adjust to match new color scheme
  - [ ] **Material**: Apply water shader with glow
- [ ] **Base**: Recolor floating island base
  - [ ] Match warm volcanic aesthetic
  - [ ] Add emissive glow if needed
- [ ] **Integration**: Connect elevator to main mountain

## 4. HotCliff (Magma Factory)
- [ ] **Concept**: Factory-like structure similar to Egg Engine + Dangerous Dinner (KRtDL 6/7)
- [ ] **Geometry**: Create industrial/factory building
  - [ ] Main factory structure
  - [ ] Pipes and vents
  - [ ] Machinery details
- [ ] **Magma**: Add flowing lava/magma elements
  - [ ] Lava pools
  - [ ] Flowing magma channels
  - [ ] Emissive orange/yellow glow
- [ ] **Colors**: Apply warm volcanic/factory palette
  - [ ] Dark metal grays
  - [ ] Bright orange/red lava
  - [ ] Yellow glow accents
- [ ] **Materials**: Create factory metal + lava shaders
- [ ] **Integration**: Position as dangerous/hazardous area on mountain

## 5. Cyber Nexus (Gateway Portal)
- [ ] **Portal**: Create gateway portal structure
  - [ ] Portal frame geometry
  - [ ] Glowing portal surface
  - [ ] Sci-fi aesthetic
- [ ] **Colors**: Apply cyan/neon blue theme
  - [ ] Bright blue/cyan glow
  - [ ] Dark tech frame
- [ ] **Effects**: Add portal visual effects
  - [ ] Emissive glow
  - [ ] Animated swirl (optional)
- [ ] **Constraint**: Portal is the endpoint - nothing beyond it
- [ ] **Integration**: Position as final destination on mountain

## 6. Citadel (Recolor & Lighting)
- [ ] **Existing Model**: Modify current citadel
- [ ] **Color**: Recolor to match Halcandra warm theme
  - [ ] Update base material colors
  - [ ] Apply to all citadel surfaces
- [ ] **Lighting**: Lighten by ambient light
  - [ ] Adjust emissive values
  - [ ] Add light-affected textures
  - [ ] Ensure consistency with scene lighting
- [ ] **Rainbow Bridge**: Recolor and lighten
  - [ ] Match new color scheme
  - [ ] Apply lighting adjustments
  - [ ] Maintain visual distinction
- [ ] **Integration**: Ensure visual cohesion with rest of mountain

---

## Material Requirements Summary

### Color Palettes Needed:
- **Purple/Magenta** (Ruins)
- **Cyan/Blue** (Snowdin crystals, Cyber Nexus)
- **Warm Volcanic** (WaterEdgeFall tower, HotCliff, Citadel)
- **Factory Metal + Lava** (HotCliff)
- **Neon Cyan** (Cyber Nexus portal)

### Shader Types Needed:
- Standard diffuse (buildings, terrain)
- Emissive glow (crystals, lava, portal)
- Water shader (waterfall, lakes)
- Metallic industrial (factory)

---

## Asset Creation Checklist

### Models to Create:
- [ ] Rectangular ruin building
- [ ] Spruce tree variants (if not reusing)
- [ ] Crystal/diamond formations
- [ ] Miniature houses (2-3 variations)
- [ ] Tether elevator structure
- [ ] Mini floating islands
- [ ] Factory/magma structure
- [ ] Portal gateway frame

### Materials to Create:
- [ ] Purple/magenta stone
- [ ] Cyan crystal
- [ ] Warm volcanic rock
- [ ] Factory metal
- [ ] Lava/magma emissive
- [ ] Water shader
- [ ] Neon portal

### Textures to Generate/Bake:
- [ ] Ruin textures
- [ ] Factory textures
- [ ] Crystal textures
- [ ] Water surface maps

---

## Implementation Order (Recommended)

1. **Ruins** - Simplest, good starting point
2. **Citadel Recolor** - Modify existing, quick win
3. **Snowdin Slope City** - Medium complexity
4. **WaterEdgeFall** - Modify existing tower + new elements
5. **HotCliff** - Complex factory structure
6. **Cyber Nexus** - Final endpoint, can be stylized

---

## Notes

- All features should maintain the **Halcandra-inspired warm volcanic aesthetic** with **Celeste winter elements**
- Use existing material definitions from `Zantas_Halcandra_Winter.mtl`
- Consider LOD (Level of Detail) for performance
- Test scale/proportions in context of full mountain
