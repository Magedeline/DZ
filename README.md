# Desolo Zantas

**Desolo Zantas** is a massive, story-driven Celeste mod featuring a full 21-chapter campaign, over 800 source files of custom gameplay code, original music, Kirby-inspired mechanics, and an extended side system spanning A through DX difficulty tiers.

Built on the `MaggyHelper` Everest module, this project includes campaign maps, C# gameplay code, dialogue, original art and audio, Spine animation support, Loenn editor integration, and full mod packaging. The mod requires Everest and a large helper stack (46+ dependencies) to run.

## Key Features

- **21-chapter story campaign** — from Prologue through Chapter 20 plus a post-epilogue chapter, each with custom dialogue, cutscenes, and progression.
- **5-tier side system** — A-Side, B-Side, C-Side, D-Side, and DX-Side support with sequential unlock logic, heart gem requirements, and unlock postcards.
- **Custom boss encounters** — including Apex Predator, Asriel God, Siamo Zero (the CH20 true final boss with knight clone summoning and multi-phase attack mechanics), Whispy Woods, and many more.
- **Lobby & submap architecture** — Chapters 10–14 feature dedicated hub lobbies with fragment/shard routes, EX maps, and boss arenas, with CollabUtils2 as the supported mini-heart and lobby-map stack.
- **Kirby crossover content** — custom Kirby player sprites, KirbyPlayerSpawner system, dedicated skin support, and crossover characters.
- **Spine animation runtime** — integrated Spine MonoGame support for skeletal character animations and custom font rendering.
- **Original audio** — FMOD sound banks with custom music and SFX, including dedicated DLC audio content.
- **Loenn editor integration** — full entity, trigger, effect, and tooling plugins for map editing.

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

This repo currently includes:

- A full story campaign with a Prologue, Chapters 1–20, and a post-epilogue chapter.
- Separate side folders for A-Side, B-Side, C-Side, D-Side, and DX-Side support.
- Chapter lobby and submap systems for later-game content, including fragment or shard routes, EX maps, and boss encounters.
- Custom gameplay systems such as side unlock progression, unlock postcards, credits flows, mod intro routing, and chapter panel extensions.
- A player compatibility shim layer for extended player spawner support and cross-mod interop.
- Spine-based skeletal animation support via the SpineMonoGame library and custom font rendering pipeline.
- Custom dialogue, sprites, portraits, sound effects, music events, and skin content.

## Main Campaign

- 00: Prologue
- 01: Forbidden Metropolis
- 02: Veil of Shadows
- 03: Arrival
- 04: Chronicles of Destiny
- 05: Fractured Memories
- 06: Fortress of Solitude
- 07: Infernal Reflections
- 08: Revelation's Edge
- 09: Apex of Reality
- 10: Echoes of the Past
- 11: Frozen Sanctuary
- 12: Cascading Depths
- 13: Blazing Territories
- 14: Cyber Nexus
- 15: Ethereal Citadel
- 16: Organ Garden of Despair
- 17: Final Resonance
- 18: Core of Existence
- 19: Farewell to Stars
- 20: Light Through the Dark
- 21: Desolo Zantas True Finale

Chapters 10-14 also include dedicated lobby maps plus submaps, EX routes, and boss maps for their respective themes.

## Repository Layout

- `Source/`: C# gameplay code, cutscenes, entities, UI, unlock logic, and packaging project files.
- `Maps/Maggy/ASide`: Main A-Side campaign maps.
- `Maps/Maggy/BSide`: B-Side campaign maps.
- `Maps/Maggy/CSide`: C-Side campaign maps.
- `Maps/Maggy/DSide`: D-Side campaign maps.
- `Maps/Maggy/DXSide`: DX-side folder reserved for extended content.
- `Maps/Maggy/Lobby`: Chapter lobby maps for the later-game submap structure.
- `Maps/Maggy/SmallMaps`: Fragment, shard, EX, and boss submaps.
- `Maps/Maggy/WIP`: In-progress maps and staging content.
- `Dialog/`: In-game text, chapter names, UI strings, postcards, and credits strings.
- `Graphics/`: Atlases, portraits, sprites, tiles, color grading, and UI assets.
- `Audio/`: FMOD banks and audio content.
- `Loenn/`: Editor plugins, entities, triggers, effects, metadata, and tooling.
- `Mountain/`: Mountain data used by the campaign.

## Collab Utils 2

CollabUtils2 is now the supported path for mini hearts and lobby minimaps.

The old MaggyHelper editor-side miniheart and submap lobby entities have been removed from Loenn so new content uses the community-standard CollabUtils2 entities instead:

- `CollabUtils2/MiniHeart`
- `CollabUtils2/FakeMiniHeart`
- `CollabUtils2/MiniHeartDoor`
- `CollabUtils2/LobbyMapController`
- `CollabUtils2/LobbyMapWarp`
- `CollabUtils2/LobbyMapMarker`

Legacy runtime code remains in the repository for backward compatibility with older map logic and cutscene flow, but it is no longer the recommended authoring path.

## Notable Systems

- Custom first-launch selection screen that lets the player start Desolo Zantas directly or continue to the normal Celeste flow.
- Extended chapter-side support up to D-Side and DX-Side through runtime area mode expansion.
- Sequential side unlock logic with save data tracking and custom unlock postcards.
- Chapter lobby and portal systems for Ruins, Snowdin, Wateredgefalls, Hotcliffland, and Cyber Nexus.
- Custom credits sequences, cutscenes, bosses, and chapter-specific progression hooks.
- PlayerCompatShim layer for robust player spawner compatibility across helper mods.
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

The campaign uses a side-based folder layout instead of suffix-based filenames:

- `Maps/Maggy/ASide/01_City.bin`
- `Maps/Maggy/BSide/01_City.bin`
- `Maps/Maggy/CSide/01_City.bin`
- `Maps/Maggy/DSide/01_City.bin`

This keeps side content separated cleanly while matching the runtime path logic used by `AreaModeExtender`.

## Development Notes

- The codebase targets Everest mod workflows and includes stripped Celeste references under `Source/lib-stripped` for local builds.
- The README intentionally documents the public project identity as Desolo Zantas while preserving the internal MaggyHelper naming used by the codebase.
- The solution includes two library subprojects: SpineMonoGame (skeletal animation) and Nez.FNA (game framework), both under `Libs/`.
- DX-Side support exists in code and folder structure, but the current DX map folder is still empty.
