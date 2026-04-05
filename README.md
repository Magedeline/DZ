# Desolo Zantas

Desolo Zantas is a large-scale Celeste mod built on the `MaggyHelper` module. This repository contains the campaign maps, runtime code, dialogue, art, audio, Loenn metadata, and packaging setup used to build and ship the project.

`Desolo Zantas` is the public-facing mod name. `MaggyHelper` remains the internal assembly, namespace, and Everest module name used by the project.

## Project Snapshot

- Public mod title: Desolo Zantas
- Internal module name: MaggyHelper
- Current manifest version: 3.0.0
- Target framework: net8.0
- Primary build output: `bin/MaggyHelper.dll`

## Content Overview

This repo currently includes:

- A full story campaign with a Prologue, Chapters 1-20, and a post-epilogue chapter.
- Separate side folders for A-Side, B-Side, C-Side, D-Side, and DX-side support.
- Chapter lobby and submap systems for later-game content, including fragment or shard routes, EX maps, and boss encounters.
- Custom gameplay systems such as side unlock progression, unlock postcards, credits flows, mod intro routing, and chapter panel extensions.
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
- 20: The Last Push
- 21: Post Respite

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

## Notable Systems

- Custom first-launch selection screen that lets the player start Desolo Zantas directly or continue to the normal Celeste flow.
- Extended chapter-side support up to D-Side and DX-side through runtime area mode expansion.
- Sequential side unlock logic with save data tracking and custom unlock postcards.
- Chapter lobby and portal systems for Ruins, Snowdin, Wateredgefalls, Hotcliffland, and Cyber Nexus.
- Custom credits sequences, cutscenes, bosses, and chapter-specific progression hooks.

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
- DX-side support exists in code and folder structure, but the current DX map folder is still empty.