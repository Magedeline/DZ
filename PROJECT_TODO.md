# Desolo Zantas Project TODO

This checklist summarizes the current project state and next work needed across sprites, maps, code quality, bug fixing, polish, and release readiness.

## Current Snapshot

- Code: about 849 C# source files under `Source/`, including gameplay systems, bosses, cutscenes, UI, compatibility hooks, and custom player/Kirby systems.
- Maps: current `.bin` map coverage is 22 A-Side maps, 16 B-Side maps, 16 C-Side maps, 16 D-Side maps, 5 lobby maps, 8 small maps, and 1 WIP map. `DXSide` exists in the route structure but has no map files yet.
- Assets: about 39,760 files under `Graphics/`, including player skins, Kirby combat sprites, portraits, GUI, tiles, and effects.
- Audio: 8 FMOD bank files under `Audio/`, split across classic, final DLC, music, SFX, and UI banks.
- Tooling: about 610 Loenn files under `Loenn/`, covering entities, triggers, effects, metadata, placements, and editor support.
- Build path: `.github/workflows/build.yml` restores and builds `MaggyHelper.sln` on Windows with .NET 8.

## Priority 0 — Project Hygiene And Safety

- [ ] Keep `README.md`, `everest.yaml`, release notes, and dependency versions synchronized before each public build.
- [ ] Remove duplicate dependency declarations in `everest.yaml`; Everest is currently listed multiple times.
- [ ] Decide whether checked-in build logs such as `build_log.txt` and `build_output.txt` should remain in the repo or move to release artifacts.
- [ ] Add a short release checklist covering build, package, smoke test, Loenn load test, and credit review.
- [ ] Keep public contribution policy clear: this repo is visible for feedback, while implementation changes require owner permission.

## Priority 1 — Bugs And Risky Code Patterns

- [ ] Replace `throw new NotImplementedException()` in production paths:
  - `Source/TapeManager.cs`
  - `Source/ReflectionTentacles.cs`
  - `Source/EnemyBossManager.cs`
- [ ] Replace placeholder logic in:
  - `Source/StrawberryHooks.cs`
  - `Source/SampleEntity.cs`
  - `Source/SampleSolid.cs`
  - placeholder attack comments in `Source/WhispyWoodsBoss.cs`
- [ ] Audit silent `catch { }` blocks and either log warnings or narrow expected exceptions in files such as `SuperCoreBlock.cs`, `TeleportPipe.cs`, `RenderTargetPool.cs`, `MountainOverworldManager.cs`, `GlitchGlider.cs`, `CosmicChapterPanelHook.cs`, `CS18_Outro.cs`, `AsrielGodBoss.cs`, and `AreaModeExtender.cs`.
- [ ] Remove or replace magic player-state numbers like `11` and `19` with named constants where possible.
- [ ] Review reflection-based combat damage compatibility in custom player code; prefer typed interfaces for bosses/enemies that are owned by this mod.
- [ ] Confirm every runtime hook registered in module load has a matching unload path.
- [ ] Test room transitions while Kirby mode is active, especially rooms without a `KirbyPlayerSpawner`.

## Priority 2 — Kirby Player And Combat Work

- [ ] Playtest the new Kirby action inputs on real maps:
  - Grab: punch.
  - Grab + horizontal: kick / slide-kick.
  - Grab in air: mid-air spin.
  - Grab + up: backflip.
  - Dash + down in air: dash ground pound.
- [ ] Confirm the action animations line up with the existing sprites in `Graphics/Atlases/Gameplay/characters/kirby/combat/`.
- [ ] Add missing dedicated kick sprites if the slide-kick placeholder does not match the intended animation.
- [ ] Tune movement values for punch, kick, air spin, backflip, and ground pound per playtester feedback.
- [ ] Add enemy/boss hit handling for Kirby actions after confirming which targets should be damageable.
- [ ] Add tutorial birds or short in-map prompts for the Kirby action inputs.
- [ ] Confirm Kirby action states do not break vanilla dash, dream dash, climb, water, cutscene, or screen-transition states.

## Priority 3 — Maps And Level Design

- [ ] Prioritize finishing all A-Side route polish before expanding more challenge tiers.
- [ ] For B/C/D-Sides, confirm each map has clear difficulty identity and avoids repeating the same mechanic test.
- [ ] Decide the intended scope for DX-Sides before adding files to `Maps/Maggy/DXSide/`.
- [ ] Review lobby/minimap authoring against the CollabUtils2 path documented in the README.
- [ ] Move or mark `Maps/Maggy/WIP/` content that is not meant for release builds.
- [ ] Check all checkpoint, heart, cassette, return, and submap paths for softlock risk.
- [ ] Create a per-chapter playtest sheet with: first clear, death count, unclear rooms, unfair rooms, visual clutter, missing dialogue, and performance issues.
- [ ] Validate all map metadata sidecar files are present for release maps.

## Priority 4 — Sprites, Art, And Visual Polish

- [ ] Review all custom player skins and Kirby forms for consistent origin, hair/scarf offsets, and hitbox readability.
- [ ] Finish or clean up WIP boss sprites before release packaging.
- [ ] Add/verify sprite XML entries whenever new sprite files are added.
- [ ] Check all portraits and dialogue-facing sprites for correct naming and atlas paths.
- [ ] Ensure combat sprites communicate gameplay timing: startup, active frame, recovery, and impact.
- [ ] Add visual effects for Kirby ground pound impact, air spin hitbox, and backflip invulnerability/escape if those become gameplay features.
- [ ] Audit GUI and menu images for correct resolution, palette consistency, and no placeholder text.

## Priority 5 — Audio And Music

- [ ] Build a track list mapping every FMOD event to its creator/remixer/source and where it is used in the mod.
- [ ] Verify music credits for Nintendo/HAL/Kirby-inspired material, Undertale/Deltarune-inspired material, Touhou-inspired material, and every remixer listed in dialog credits.
- [ ] Confirm loop points, transitions, and fallback silence behavior for every chapter and boss.
- [ ] Review SFX volume against vanilla Celeste and avoid clipping in dense boss encounters.
- [ ] Keep original music, remix, arrangement, and inspirational-source credits separate.

## Priority 6 — Loenn And Editor Support

- [ ] Open Loenn with the full plugin set and confirm all custom entities render without Lua errors.
- [ ] Add missing placements, field defaults, tooltips, and warnings for complex custom entities.
- [ ] Keep entity IDs, trigger IDs, and C# `CustomEntity` names synchronized.
- [ ] Mark deprecated entities in Loenn metadata and point mappers to recommended replacements.
- [ ] Validate CollabUtils2 lobby/minimap entities are used for new lobby work.

## Priority 7 — Testing And Release Readiness

- [ ] Run `dotnet restore MaggyHelper.sln` and `dotnet build MaggyHelper.sln --configuration Release` on a .NET 8 environment.
- [ ] Smoke test packaged `MaggyHelper.zip` in Everest with the dependency stack from `everest.yaml`.
- [ ] Smoke test at least one map from each side folder and each lobby route.
- [ ] Test save/load, death/respawn, room transition, chapter completion, and credits playback.
- [ ] Test with missing optional helpers where possible to verify dependency errors are understandable.
- [ ] Review performance in boss-heavy rooms, high-particle scenes, and large maps.

## Priority 8 — Documentation For Community Respect

- [ ] Keep `CREDITS.md` updated whenever art, music, code, mapping, testing, or inspiration sources change.
- [ ] Add credit notes to release pages and in-game credits when the exact contributor/source is known.
- [ ] Separate original work from inspiration, homage, remix, and third-party helper/library usage.
- [ ] Ask contributors how they want to be named before final public releases.
