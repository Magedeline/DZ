# RySy Library for KIRBY_CELESTE

This is the RySy level editor entity library for the KIRBY_CELESTE mod.

## Structure

- `entities/` - Entity definitions for placeable objects
- `triggers/` - Trigger definitions for room events
- `effects/` - Effect/Styleground definitions

## Entity Format

RySy uses a simple Lua format for entity definitions:

```lua
local Entity = {}

Entity.name = "DZ/EntityName"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        -- additional fields with defaults
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    -- Return sprite or shape info
    return {
        texture = "path/to/sprite",
        x = x,
        y = y
    }
end

return Entity
```

## Available Entities

### Core Player Entities
- `K_Player.lua` - Main player controller with sprite modes and health settings
- `KirbyPlayerSpawner.lua` - Room-local Kirby mode controller
- `KirbyPlayer.lua` - Kirby spawn marker
- `KirbyNPC.lua` - Friendly NPCs (Bandana Dee, Dedede, Meta Knight, etc.)

### Food Items (Health/Collectibles)
- `Cherry.lua` - Small heal (1 HP)
- `EnergyDrink.lua` - Medium heal (2 HP)
- `MaximTomato.lua` - Full heal
- `InvincibilityCandy.lua` - Temporary invincibility

### Copy Abilities
- `abilityStar.lua` - Ability pickup with 21 ability types
- `DashCopyBerry.lua` - Copy ability berry for dash refills
- `PunchRefill.lua` - Punch ability refill
- `KirbyPuffJumpRefill.lua` - Jump puff refill

### Enemies
- `WaddleDee.lua` - Basic ground enemy
- `WaddleDoo.lua` - Beam-shooting enemy
- `BrontoBurt.lua` - Flying enemy
- `Gordo.lua` - Spiky invincible enemy

### Mid-Bosses
- `PoppyBrosJr.lua` - Bomb-throwing mid-boss
- `Bonkers.lua` - Hammer mid-boss
- `Bugzzy.lua` - Suplex mid-boss

### Bosses
- `MetaKnightBoss.lua` - Meta Knight boss with sword attacks
- `ELSTerminaBoss.lua` - ELS Termina multi-phase boss
- `ELSTerminaFinalBoss.lua` - True final boss form
- `ELSTerminaHealth.lua` - Boss health controller

### Utility/Gameplay
- `EventTrigger.lua` - Cutscene/event trigger (trigger)
- `KirbyModeToggleTrigger.lua` - Toggle Kirby mode on enter (trigger)
- `BossFightTrigger.lua` - Boss arena trigger (trigger)
- `RainbowBlackholeTrigger.lua` - Rainbow blackhole visual trigger (trigger)
- `WeatherChangeTrigger.lua` - Weather effect trigger (trigger)
- `PowerGenerator.lua` - Chapter 19 puzzle element
- `ClutterSwitch.lua` - Resort clutter cleanup switch
- `ClutterDoor.lua` - Door blocked by clutter
- `SuperCoreBlock.lua` - Powered-up Core block
- `GoldBlock.lua` - Breakable gold block
- `GreyBooster.lua` - Grey colored booster
- `TeleportPipe.lua` - Room teleport pipe
- `WarpStar.lua` - Level transition star

### Effects/Backgrounds
- `popstar_bg.lua` - Popstar background
- `rainbow_blackhole_bg.lua` - Rainbow blackhole effect
- `els_true_final_backdrop.lua` - True final boss backdrop

## Conversion from Loenn

The RySy format differs from Loenn in these ways:

| Loenn | RySy |
|-------|------|
| `placements` table | `place()` function |
| `sprite()` function | `draw()` function |
| `selection()` function | Return bounds in `draw()` |
| `fieldInformation` | Default values in `place()` |

Most entities have been converted maintaining the same field names and default values from the Loenn versions.

## Notes

- Field names match the C# entity Data attribute names
- Default values are taken from the C# entity constructors
- Texture paths use the game's sprite bank paths
- Entity depth values match the in-game rendering order
