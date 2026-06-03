# Enhanced Map Editor with PCG Integration

## Overview

The Enhanced Map Editor replaces Celeste's default debug map editor with an enhanced version that includes procedural content generation (PCG) capabilities via the loenn-mcp integration.

## Features

### Original Map Editor Features
- All original debug map editor functionality preserved
- Room selection, movement, and resizing
- Teleport to rooms with various options
- Undo/redo support
- Grid visualization and room highlighting
- Key and strawberry visualization

### New PCG Features
- **Terrain Generation**: Generate complete maps using Perlin noise and Voronoi biomes
- **Pattern-Based Room Generation**: Create rooms from pattern libraries
- **Image-to-Map Conversion**: Convert images into playable Celeste maps
- **Interactive PCG Menu**: Easy-to-use interface for all PCG operations

## Controls

### Map Editor Controls
- **Right Click**: Teleport to the room
- **Confirm**: Teleport to the room
- **Hold Control**: Restart chapter before teleporting
- **Hold Shift**: Teleport to mouse position
- **Cancel**: Exit debug map
- **Q**: Show red berries
- **F1**: Show keys
- **F2**: Center on current respawn point
- **F5**: Exit debug map
- **P**: Toggle PCG Menu
- **Space + Ctrl**: Reset camera zoom and position
- **Room Colors (24 total)**:
  - **D1-D9**: Colors 0-8
  - **D0**: Color 9
  - **Shift + D1-D9**: Colors 10-18
  - **Shift + D0**: Color 19
  - **Ctrl + D1-D5**: Colors 20-24

### PCG Menu Controls
- **P**: Open/Close PCG Menu
- **Arrow Up/Down**: Navigate menu options
- **Enter/Space**: Select menu option
- **ESC**: Close menu
- **1/2**: Adjust difficulty (1-5)
- **3/4**: Adjust map width (2-10 rooms)
- **5/6**: Adjust map height (2-10 rooms)
- **R**: Randomize seed

## PCG Generation Options

### 1. Generate Terrain Map
Creates a complete playable map using:
- Perlin noise for terrain shaping
- Voronoi diagrams for biome regions
- Configurable difficulty (affects hazards and tile density)
- Customizable map dimensions
- Random or fixed seed for reproducibility

**Parameters:**
- Difficulty: 1-5 (affects hazard count and complexity)
- Width: 2-10 rooms horizontally
- Height: 2-10 rooms vertically
- Seed: Random or specific number for reproducible results

### 2. Generate Room from Pattern
Creates a single room using pattern library:
- Strategy options: balanced, exploration, challenge, speedrun
- Model profile: creative, deterministic, architect
- Uses pattern library from `PCG/patterns.json`
- Configurable room dimensions

**Parameters:**
- Strategy: Determines generation style
- Seed: Random or specific number
- Library path: Path to pattern library JSON

### 3. Generate from Image
Converts an image into a Celeste map:
- Each pixel (or pixel block) becomes a tile
- Color mapping determines tile types
- Configurable scale for large images
- Color tolerance for matching

**Parameters:**
- Image path: Source image file
- Scale: Pixels per tile (1 = 1 pixel = 1 tile)
- Tolerance: Color matching tolerance (0-255)
- Color map: Custom color-to-tile mapping (optional)

## Pattern Library

To use pattern-based generation, you need to build a pattern library first:

1. Place reference `.bin` map files in your `Maps/` directory
2. Use the PCG service to extract patterns (via code or future UI)
3. Patterns are saved to `PCG/patterns.json`
4. The library contains room layouts, entity placements, and tile patterns

## Integration with loenn-mcp

The PCG service is designed to integrate with the loenn-mcp MCP server for Celeste map editing. Currently, the service provides a structured interface with placeholder implementations. To fully enable loenn-mcp integration:

1. Ensure the loenn-mcp MCP server is running
2. Update the PCGService methods to call actual loenn-mcp tools
3. The service supports:
   - `mcp3_generate_terrain_map`
   - `mcp3_generate_room_from_pattern`
   - `mcp3_generate_map_from_image`
   - `mcp3_build_pattern_library`
   - `mcp3_preview_terrain_biomes`

## Enhanced Features

### 24 Room Colors
The vanilla Celeste map editor supports 7 room colors. This mod expands this to 24 colors via a MonoMod hook on the `LevelTemplate` static constructor. The color selection uses modifier keys:

- **D1-D9**: Colors 0-8 (original colors 0-6 + 3 new)
- **D0**: Color 9
- **Shift + D1-D9**: Colors 10-18 (reds/pinks)
- **Shift + D0**: Color 19 (magenta)
- **Ctrl + D1-D5**: Colors 20-24 (purples)

The hook modifies the vanilla `LevelTemplate.fgTilesColor` array in place, so both the enhanced and vanilla map editors benefit from the expanded color palette.

## File Structure

```
Source/MapEditor/
├── EnhancedMapEditor.cs    # Main enhanced map editor class
├── PCGService.cs           # PCG generation service
├── InGameMapEditor.cs      # Existing in-game editor (separate)
├── EDITOR_HELP.txt         # Original editor help
└── README.md               # This file
```

## Usage Example

### Using the Enhanced Map Editor

1. Enter debug mode in Celeste
2. Open the map editor (usual debug map key)
3. The enhanced editor will automatically load
4. Press **P** to open the PCG menu
5. Navigate options with arrow keys
6. Adjust parameters with number keys
7. Press Enter to generate content
8. Generated content will appear in the editor

### Programmatic PCG Generation

```csharp
// Generate a terrain map
bool success = await PCGService.GenerateTerrainMapAsync(
    outputPath: "Maps/TerrainGen/seed_123.bin",
    seed: 123,
    difficulty: 3,
    widthRooms: 4,
    heightRooms: 3
);

// Generate a room from pattern
success = await PCGService.GenerateRoomFromPatternAsync(
    mapPath: "Maps/MyMap.bin",
    roomName: "pcg_room_001",
    strategy: "balanced",
    seed: 456
);

// Generate from image
success = await PCGService.GenerateMapFromImageAsync(
    imagePath: "PCG/input_image.png",
    outputPath: "Maps/ImageGen/image_map.bin",
    scale: 1
);
```

## Technical Details

### Hook Implementation
The enhanced editor is hooked via MonoMod in `Source/Core/MonoModHooks.cs`:
- Intercepts `Celeste.Editor.MapEditor` constructor
- Replaces with `Celeste.Editor.EnhancedMapEditor`
- Uses reflection to avoid namespace conflicts
- Falls back to original if enhanced version fails

### Architecture
- **EnhancedMapEditor**: Main editor class with UI and interaction
- **PCGService**: Static service for PCG operations
- **MonoModHooks**: Hook registration and management

## Troubleshooting

### Enhanced Editor Not Loading
- Check that `MonoModHooks.Load()` is called in `KIRBY_CELESTEModule.Load()`
- Verify the hook is registered in the logs
- Ensure no namespace conflicts exist

### PCG Generation Failing
- Verify loenn-mcp server is running (if using MCP integration)
- Check that output directories exist
- Ensure pattern library exists for pattern-based generation
- Verify image file exists for image-to-map conversion

### Performance Issues
- Large terrain maps may take time to generate
- Reduce room dimensions for faster generation
- Lower difficulty for simpler maps
- Use fixed seeds for reproducible testing

## Future Enhancements

- [ ] Full loenn-mcp tool integration
- [ ] File picker for image-to-map conversion
- [ ] Visual biome preview in editor
- [ ] Pattern library management UI
- [ ] Save/load PCG presets
- [ ] Undo/redo for PCG operations
- [ ] Real-time parameter adjustment
- [ ] Multi-biome selection interface

## Credits

- Original Celeste Map Editor by Maddy Thorson & Noel Berry
- Enhanced version with PCG integration for MaggyHelper mod
- loenn-mcp integration for Celeste map editing
