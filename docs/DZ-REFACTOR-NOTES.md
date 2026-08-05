# DZ Refactor Notes — Initial Implementation Slice

> **Scope:** Groundwork only. No rewrites of `K_Player.cs`, no broad map
> changes, no new external dependencies.

---

## 1. Canonical A/B/C/D Side Mapping

`Source/Core/DzSideMap.cs` is the **single source of truth** for the
DZ folder/mode/side convention:

| Side | Mode (int) | Folder | Label | Char |
|------|-----------|--------|-------|------|
| A    | 0         | `"0"`  | `"A"` | `'A'` |
| B    | 1         | `"1"`  | `"B"` | `'B'` |
| C    | 2         | `"2"`  | `"C"` | `'C'` |
| D    | 3         | `"3"`  | `"D"` | `'D'` |

### Key API

```csharp
// Convert mode integer → DzSide
DzSide side = DzSideMap.FromMode(3);           // → DzSide.D

// Convert DzSide → folder string
string folder = DzSideMap.ToFolder(DzSide.D);  // → "3"

// Build a SID
string sid = DzSideMap.BuildSid(DzSide.D, "01_City");  // → "DZ/3/01_City"

// Parse a SID
DzSideMap.TryParseSid("DZ/3/01_City", out DzSide s, out string map);  // s=D, map="01_City"

// Side character (for TapeManager compatibility)
char c = DzSideMap.CharFromMode(3);            // → 'D'
```

### Bug fixed

`TapeManager.getCurrentSide()` previously had:
```csharp
default: return 'A'; // ← BUG: mode 3 silently returned 'A' instead of 'D'
```
This is now replaced with `DzSideMap.CharFromMode((int)areaKey.Mode)`, which
correctly returns `'D'` for mode 3.

### Known unresolved ownership overlap

`AreaModeExtender` and `AreaMapData` both contain their own copies of
folder/label mapping logic (`GetSideFolder`, `GetSideLabel`, `GetModeName`,
`SideFolders`, etc.). They are **not** changed by this PR to minimise risk.

**Follow-up work:** Migrate `AreaModeExtender.GetSideFolder` /
`GetSideLabel` / `GetModeName` to delegate to `DzSideMap` in a later PR
after confirming no callers depend on the current return-value format.

---

## 2. Side-Selection Diagnostics

### Enabling

Open **Mod Options → DZ Settings** (or edit
`Saves/modsettings-DZ.yaml`):

```yaml
EnableSideDiagnostics: true
```

All diagnostic output appears in the Everest log under the prefix
`DZ/SideDiag`.

### What is logged

The helper `Source/Core/SideDiagnostics.cs` logs only at lifecycle /
selection / collection boundaries, never per-frame:

| Helper method | When to call |
|---|---|
| `LogAreaEntry(context, sid, mode)` | Level load, chapter panel reset |
| `LogModeArray(context, sid, lengths, nonNull)` | Mode-array inspection |
| `LogSaveAreaStats(context, sid, saveLen, expected)` | Save data read |
| `LogPanelOption(context, sid, optionIndex, optionMode)` | Chapter panel option change |
| `LogCollectibleTransition(context, type, sid, mode)` | Berry/cassette/heart collect |

Instrument new boundary points by calling the appropriate helper;
add `if (!SideDiagnostics.IsEnabled) return;` guards around any
expensive lookups you put around those calls.

---

## 3. Sprite/Editor Contract Validator

### Running

**Automatic (DEBUG builds only):** Called once, deferred, from
`DZModule.Load()` — results appear in the log after content finishes
loading.

**Manual / production:** Call explicitly from a debug command:
```csharp
var results = SpriteContractValidator.RunAll();
```
Results are also written to the log under `DZ/SpriteValidator`.

### What is checked

1. **`DzSideMap` self-check** — verifies the mapping tables haven't drifted.
2. **High-risk sprite-bank IDs** — checks `Graphics/Sprites.xml` for IDs
   that are referenced in gameplay paths (`player_no_backpack`,
   `heartgem0`–`heartgem3`, etc.).
3. **Entity Lönn definitions** — checks that `.lua` files in `Loenn/` exist
   for the high-risk entities: `DZ/KirbyPlayerSpawner`, `DZ/DZHeartGem`,
   `DZ/FakeHeartGem`, `DZ/CassetteTape`, `DZ/PopstarBerry`,
   `DZ/PinkPlatinumStrawberry`.
4. **`PlayerSpriteMode` → sprite bank ID** — cross-checks the manifest in
   the validator against the stub mapping in `PlayerSpriteModeExtensions`.

Grow the manifest lists in `SpriteContractValidator.cs` when new
high-risk entity or sprite paths are identified.

---

## 4. Follow-up Work

The following items were explicitly **out of scope** for this slice and
must be addressed in later PRs:

| Item | Priority | Notes |
|---|---|---|
| Migrate `AreaModeExtender` side/folder/label helpers to delegate to `DzSideMap` | Medium | Low-risk mechanical change; coordinate with `AreaMapData` |
| Consolidate `AreaMapData` overlapping ownership | Medium | Add diagnostics/comments first; refactor second |
| Wire `SideDiagnostics` into `AreaModeExtender` and `OuiChapterPanel` hooks | Medium | Add `LogAreaEntry` / `LogPanelOption` calls at key boundaries |
| Unify collectible event handling (HeartGem, CassetteTape, PopstarBerry, PinkPlatinum) | High | Needs separate design; do not bake into TapeManager |
| Kirby Helper Mechanics extraction into standalone repo | Low | Blocked on `K_Player.cs` rewrite |
| Rename Pink Platinum Berry → Green Greens Berry | Low | Needs save migration; do NOT rename map entity ID yet |
| Complete `PlayerSpriteModeExtensions.GetSpriteBankId()` implementation | Medium | Currently a stub returning `""` |
