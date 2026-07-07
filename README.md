# DZ (Desolo Zantas)

A large-scale crossover mod for [Celeste](https://www.celestegame.com/), built on [Everest](https://everestapi.github.io/). DZ blends Celeste's movement and level design with Undertale/Deltarune and Kirby, adding a second playable character, new bosses, custom mechanics, and a full campaign spanning A/B/C/D-Sides plus a DX-Side finale.

## Features

- **Two playable characters.** Play as Madeline (with an optional combat mode) or Kirby (inhale, hover, float jumps, and a combat mode of his own). A player-select screen lets you choose per map, with a recommended-character hint and an auto-select option.
- **Full campaign.** 19 main chapters (A-Sides) running from *Forbidden Metropolis* through *Core of Existence* and *Farewell to Stars*, each with B-Side and C-Side remixes, plus a DX-Side finale chapter and a True Finale.
- **Original bosses.** Multiple fully scripted boss fights (including a multi-attack final boss sequence) with telegraphed attacks, enrage phases, and dedicated cutscenes.
- **Gentle Breeze Mode.** An optional, fully reversible assist bundle for the Kirby player: slow-motion dash aiming and infinite stamina/dashes for players who want a gentler introduction to the movement.
- **Last Endemy Mode.** Optional online play through [CelesteNet](https://github.com/CelesteNet/CelesteNet), syncing Kirby health and progress between players in the same session, with an in-game rules/consent flow before enabling it.
- **Hot Reload tooling.** A developer-focused hot-reload system (F5–F8 by default) for iterating on mod code without restarting the game.
- **Deep Everest integration.** Custom entities, triggers, cutscenes, and UI built on top of CommunalHelper, VivHelper, MaxHelpingHand, FrostHelper, and Extended Variant Mode, with optional support for VidPlayer, BounceHelper, FlaglinesAndSuch, and Deathlink.
- **Localization.** Dialog is available in English, French, German, Japanese, Korean, Simplified Chinese, and Spanish.

## Installation

1. Install [Everest](https://everestapi.github.io/) for Celeste.
2. Copy this mod folder (or its packaged zip) into your Celeste `Mods` directory.
3. Launch the game through Everest and enable **DZ** from the mod list.

### Dependencies

Required: EverestCore, CommunalHelper, VivHelper, MaxHelpingHand, FrostHelper, Extended Variant Mode (see `everest.yaml` for exact versions).

Optional (unlock additional features if installed): VidPlayer, CelesteNet.Client (for Last Endemy online mode), BounceHelper, FlaglinesAndSuch, Deathlink.

## Project Layout

- `Source/` — mod code: `Bosses/`, `Cutscenes/`, `Entities/`, `Mechanics/`, `Triggers/`, `UI/`, `HotReload/`, `Integration(s)/`, and a standalone-engine reference port under `Source/Nez/`.
- `Maps/DZ/` — chapter maps, organized by side (`0` = A-Side, `1` = B-Side, `2` = C-Side, `3` = D-Side, `21` = DX-Side/finale).
- `Dialog/` — localized dialog files, one per language.
- `Graphics/`, `Audio/`, `Tutorials/` — art, FMOD audio banks, and tutorial assets.
- `Loenn/` — custom entity/trigger plugins for the Lönn map editor.

## Contributing / Notes for Developers

- The Hot Reload settings (under the mod's Mod Options menu) let you iterate on `Source/` changes live; see `_HOTRELOAD_*` settings and the F5–F8 bindings.
- `Source/Nez/` contains a standalone-engine (non-Celeste) port of several entities for reference/portability experiments — it is not compiled into the shipping mod.
- See `CHANGELOG.md` for a history of notable fixes and additions.

## Credits

_Add mod authors, artists, composers, and playtesters here._
