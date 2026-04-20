# Desolo Zantas

**Desolo Zantas** is a massive, story-driven Celeste mod featuring an extensive campaign, over 800 source files of custom gameplay code, original music, and custom mechanics with extended difficulty tiers.

Built on the `MaggyHelper` Everest module, this project includes campaign maps, C# gameplay code, dialogue, original art and audio, Spine animation support, Loenn editor integration, and full mod packaging. The mod requires Everest and a substantial helper stack to run.

## Key Features

- **Story-driven campaign** — A full narrative experience with custom dialogue, cutscenes, and progression across many chapters.
- **Multiple difficulty tiers** — Extended side system with varied challenge levels for seasoned players.
- **Custom boss encounters** — Unique encounters with distinct mechanics and designs.
- **Hub and submap systems** — Dedicated lobby areas with varied routes and additional challenges.
- **Custom crossover content** — Integrated guest character and sprite systems with dedicated support.
- **Spine animation runtime** — Integrated Spine MonoGame support for skeletal character animations and custom font rendering.
- **Original audio** — FMOD sound banks with custom music and sound effects.
- **Loenn editor integration** — Full entity, trigger, effect, and tooling plugins for map editing.

## Ownership And Repository Policy

This project was built from scratch for Desolo Zantas. Public access is provided so players and followers can view development history, report bugs, and leave feedback.

Public access does not grant permission to modify, reuse, redistribute, or publish altered versions of this codebase or its assets. Pull requests are not accepted unless the repository owner gives explicit written permission in advance.

## Project Snapshot

- Public mod title: Desolo Zantas
- Internal module name: MaggyHelper
- Current manifest version: 3.0.0
- Target framework: net8.0
- Primary build output: `bin/MaggyHelper.dll`
- Source files: 800+ C# files across gameplay, bosses, cutscenes, UI, and entity systems

## Content Overview

This mod includes:

- A full story campaign with extensive narrative content.
- Multiple difficulty tiers with separate maps and progression systems.
- Hub and submap systems with varied challenge routes.
- Custom gameplay systems such as unlock progression, unlock tracking, and extended difficulty support.
- Cross-mod compatibility layers for player spawner support and interoperability.
- Spine-based skeletal animation support via the SpineMonoGame library and custom font rendering pipeline.
- Custom dialogue, sprites, portraits, sound effects, music events, and skin content.

## Campaign Structure

The mod features an extensive story campaign with multiple chapters, each offering unique themes, mechanics, and challenges. Detailed chapter information is intentionally omitted to preserve the discovery experience for players. Players are encouraged to experience the campaign firsthand to enjoy the full narrative journey.

Extended difficulty tiers are available for each main chapter, allowing players to return for additional challenges after completing the base campaign.

## Repository Layout

- `Source/`: C# gameplay code, cutscenes, entities, UI, progression systems, and packaging project files.
- `Maps/Maggy/`: Campaign maps organized by difficulty tier (ASide, BSide, CSide, DSide, DXSide).
- `Maps/Maggy/Lobby`: Hub and lobby maps.
- `Maps/Maggy/SmallMaps`: Challenge routes, additional content, and boss encounter maps.
- `Maps/Maggy/WIP`: In-progress maps and staging content.
- `Dialog/`: In-game text, chapter names, UI strings, and dialogue content.
- `Graphics/`: Atlases, portraits, sprites, tiles, color grading, and UI assets.
- `Audio/`: FMOD banks and audio content.
- `Loenn/`: Editor plugins, entities, triggers, effects, metadata, and tooling.
- `Mountain/`: Mountain progression data.

## CollabUtils2 Integration

CollabUtils2 is the supported path for mini hearts and lobby minimaps. The mod uses community-standard CollabUtils2 entities for map editing:

- `CollabUtils2/MiniHeart`
- `CollabUtils2/FakeMiniHeart`
- `CollabUtils2/MiniHeartDoor`
- `CollabUtils2/LobbyMapController`
- `CollabUtils2/LobbyMapWarp`
- `CollabUtils2/LobbyMapMarker`

Legacy runtime code remains in the repository for backward compatibility, but CollabUtils2 entities are the recommended authoring path for new content.

## Notable Systems

- Custom launch systems for mod flow and progression.
- Extended difficulty tier support with sequential unlock logic and progression tracking.
- Hub and portal systems for navigating complex map structures.
- Custom cutscenes, bosses, and chapter-specific progression hooks.
- Cross-mod compatibility layer for robust player spawner interoperability.
- Spine skeletal animation runtime (SpineMonoGame) and Nez framework integration for advanced rendering and gameplay systems.

## Building

### Requirements

- .NET 8 SDK
- Celeste modding references available through `Source/lib-stripped` or a valid `CelestePrefix` pointing to a local Celeste install or reference directory

### Build Command

From the repository root:

```powershell
dotnet build MaggyHelper.sln
```

The default build copies the mod DLL and required runtime dependencies into `bin/`.

## Packaging

Release builds also create a packaged mod zip:

```powershell
dotnet build MaggyHelper.sln -c Release
```

This produces `MaggyHelper.zip` at the repository root and packages files from `everest.yaml`, `bin/`, `Audio/`, `Dialog/`, `Graphics/`, and `Loenn/`. The build script also includes `Ahorn/` content when that folder is present.

## Dependencies

Runtime dependencies are declared in `everest.yaml`. The mod currently depends on Everest plus a large helper stack, including AdventureHelper, CommunalHelper, FrostHelper, MaxHelpingHand, SkinModHelper, SkinModHelperPlus, and several other helper mods.

Use `everest.yaml` as the source of truth when preparing releases or validating player installs.

## Loenn MCP Bootstrap

To install or update the local Loenn MCP server package in this repo's workspace virtual environment, run:

```bat
scripts\bootstrap-loenn-mcp.cmd
```

This updates `loenn-mcp` inside `.venv` and prints the installed version.

## Current Map Structure

The campaign uses a difficulty-tier-based folder layout for organizing maps:

- `Maps/Maggy/ASide/` — Core campaign maps
- `Maps/Maggy/BSide/` — First challenge tier maps
- `Maps/Maggy/CSide/` — Second challenge tier maps
- `Maps/Maggy/DSide/` — Third challenge tier maps
- `Maps/Maggy/DXSide/` — Extended content tier

This keeps content organized by difficulty while matching the runtime path logic used by the mod's area management systems.

## Development Notes

- The codebase targets Everest mod workflows and includes stripped Celeste references under `Source/lib-stripped` for local builds.
- The README intentionally documents the public project identity as Desolo Zantas while preserving the internal MaggyHelper naming used by the codebase.
- The solution includes two library subprojects: SpineMonoGame (skeletal animation) and Nez.FNA (game framework), both under `Libs/`.
- DX-Side support exists in code and folder structure, but the current DX map folder is still empty.
