# PCG AI Map Editor Integration

Connects the GameLab Studio PCG AI to the DZ Celeste mod's Lönn map editor via the `gamelab-mcp` MCP server.

## Architecture

```
Windsurf/Cascade (AI assistant)
    ↕ (MCP protocol via SSE)
gamelab-mcp server (http://api.gamelabstudio.co:8765/sse)
    ↓ (generates map JSON)
Loenn/pcg/output/generated_map.json
    ↓ (imported by pcg_ai_import.lua)
Lönn map editor → Celeste mod maps
```

## Components

| File | Purpose |
|------|---------|
| `.windsurf/mcp_config.json` | MCP server config for Windsurf/Cascade |
| `Loenn/pcg/entity_catalog.json` | Full catalog of DZ mod + vanilla entities for the AI |
| `Loenn/pcg/pcg_config.json` | Generation presets and output schema |
| `Loenn/scripts/pcg_ai_request.lua` | Lönn script: exports current map context as a generation request |
| `Loenn/scripts/pcg_ai_import.lua` | Lönn script: imports AI-generated map JSON into the editor |
| `Loenn/pcg/output/` | Directory where generated maps are placed |

## Workflow

### 1. Build a Generation Request

In Lönn, open the map you want to extend. Run the **PCG AI: Build Generation Request** script from the Scripts menu. Configure:
- **preset**: `platforming_room`, `kirby_combat_room`, `puzzle_room`, `boss_arena`, `narrative_room`, `tower_climb`, or `speedrun_room`
- **roomCount**: how many rooms to generate
- **theme**: `summit`, `ruins`, `tower`, `digital`, `void`, `inferno`, `resort`, `snow`
- **difficulty**: `easy`, `normal`, `hard`, `expert`
- **includeExistingContext**: sends current map rooms as context to the AI
- **customPrompt**: free-text instructions (e.g., "generate a room with a hidden collectible behind a fake wall")

This saves `Loenn/pcg/output/generation_request.json`.

### 2. Generate via Windsurf/Cascade

In the Windsurf chat, ask Cascade to generate a map. Example:

> "Use the gamelab-mcp server to generate a map based on the request at Loenn/pcg/output/generation_request.json. Use the entity catalog at Loenn/pcg/entity_catalog.json. Save the result to Loenn/pcg/output/generated_map.json."

Cascade will use the `gamelab-mcp` MCP server tools to generate the map and write the JSON output.

### 3. Import into Lönn

Run the **PCG AI: Import Generated Map** script in Lönn. Configure:
- **mode**: `merge` (add to existing map) or `replace` (clear and replace)
- **targetRoom**: merge into a specific room, or leave empty to create new rooms
- **offset_x / offset_y**: shift imported content by a pixel offset
- **dryRun**: preview without applying

### 4. Review and Save

Review the imported rooms in Lönn, adjust as needed, then save (Ctrl+S).

## Generation Presets

| Preset | Description |
|--------|-------------|
| `platforming_room` | Standard platforming with spikes, springs, collectibles |
| `kirby_combat_room` | Kirby-mode combat with DZ enemies and healing items |
| `puzzle_room` | Switches, gates, sequence puzzles |
| `boss_arena` | Boss fight arena with controlled layout |
| `narrative_room` | NPCs, dialog triggers, story elements |
| `tower_climb` | Vertical tower climbing with tower obstacles |
| `speedrun_room` | Flow-oriented speedrun room with boosters and zippers |

## Entity Catalog

The `entity_catalog.json` contains:
- **vanilla_entities**: 35+ standard Celeste entities
- **dz_entities**: 180+ custom DZ mod entities (player, triggers, enemies, NPCs, collectibles, gameplay, decoration, solid, hazard)
- **tile_palette**: FG/BG tile character mappings
- **room_defaults**: Default room dimensions and properties
- **generation_constraints**: Min/max room sizes, entity limits

## MCP Server Config

The `gamelab-mcp` server is configured in `.windsurf/mcp_config.json`:
- **URL**: `http://api.gamelabstudio.co:8765/sse`
- **Auth**: `X-API-Key` header
- **Type**: SSE (Server-Sent Events)

Windsurf automatically loads this config and exposes the server's tools to Cascade.
