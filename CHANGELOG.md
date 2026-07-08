# Changelog

All notable changes to DZ are documented here. This file starts tracking history as of the polish pass below; earlier development is captured in the git commit log.

## Unreleased (polish pass)

### Fixed
- `MoonGlitchBackgroundTrigger`: fixed a CA2021 bug where `Backdrop` toggling/fading silently did nothing (`Entities.OfType<Backdrop>()` can never match, since `Backdrop` isn't an `Entity`). Now correctly targets `Level.Background`/`Foreground` backdrops.
- Chapter 11 boss cutscenes (`CS11_BossIntro`, `CS11_BossOutro`) and `CS11_CinematicBar`: restored broken NPC lookups so Starlo, Marlet, Theo, and Chara actually appear/animate during these scenes instead of silently no-oping.
- `CS20_ElsDeathTrueLastUltraLaunch`: fixed a missing `Tracker.GetEntity` call that prevented the Els boss from being hidden during its own death cutscene.
- `CS21_Cast`: fixed the credits scroll ignoring its fade-out state, which could let fast-scroll input conflict with the ending fade.
- `CS06_Gondola`: wired up a dead rumble-feedback field so anxiety rumble is applied alongside the existing anxiety value.
- Cleaned up ~50 dead/never-assigned fields across bosses, cutscenes, and chapter entities (CS0169/CS0649/CS0414 warnings), and added `override`/`new` to ~14 methods that were unintentionally hiding base-class members.
- Removed a recurring "already completed" skip-guard bug pattern across 10 cutscene files.

### Added
- Implemented `WhispyWoodsBoss`'s four previously-unimplemented attacks (Apple Drop, Root Spike, Leaf Tornado, and the enraged Poison Apple Barrage) — these used to only play a sound effect with no actual hazard. Added `WhispyApple`, `WhispyPoisonApple`, `WhispyRootSpike`, and `WhispyLeaf` entities with real collision, damage, and lifetime/animation behavior.

### Translation
- Found and fixed the root cause of ~150 untranslated dialog keys in every non-English language: a chapter/settings-menu key rename was never propagated to translations. Carried forward existing localized text under the corrected key names, and freshly translated the remaining new content (Gentle Breeze and Last Endemy assist-mode text, the player-select UI, and the Chapter 20 Asriel monologue) into French, German, Japanese, Korean, Simplified Chinese, and Spanish.
- Fixed a bug affecting German, Japanese, Korean, Simplified Chinese, and Spanish where D-Side unlock/complete text was mistakenly saved under the C-Side dialog key, so the D-Side text was never actually shown in those languages.

### Known non-issues (flagged, not changed)
- `Source/Nez/` is a standalone-engine reference port, not part of the shipping mod — warnings/TODOs there were intentionally left alone.
- `Source/Entities/Entities/PlayerPhase3Extensions.cs` is a deliberately deferred, unspecified future feature (double-jump/alternate dash) and was left as a stub.
- A handful of harmless duplicate dialog keys remain (e.g. `DZ_HOTRELOAD_*`, `DZ_MOD_SELECT_*`) where an old English placeholder line is shadowed by a properly translated line later in the same file — cosmetic only, no missing content.
