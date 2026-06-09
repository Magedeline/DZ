-- Loenn plugin for MaggyHelper - Kirby Player Spawner
-- Replaces vanilla Celeste Player with MaggyHelper's Kirby Player entity.
-- Place this in any room to get full Kirby gameplay without
-- relying on a vanilla Player spawn.
local drawableSprite = require("structs.drawable_sprite")

local kirbyPlayerSpawner = {}

kirbyPlayerSpawner.name = "MaggyHelper/KirbyPlayerSpawner"
kirbyPlayerSpawner.depth = -1000000
kirbyPlayerSpawner.placements = {
    {
        name = "kirby_player_spawner",
        data = {
            enableKirbyMode = true,
            spawnCompanion = false,
            startingAbility = "None"
        }
    },
    {
        name = "kirby_player_spawner_with_companion",
        data = {
            enableKirbyMode = true,
            spawnCompanion = true,
            startingAbility = "None"
        }
    },
    {
        name = "madeline_player_spawner",
        data = {
            enableKirbyMode = false,
            spawnCompanion = false,
            startingAbility = "None"
        }
    }
}

kirbyPlayerSpawner.fieldInformation = {
    startingAbility = {
        options = {
            "None",
            "Fire",
            "Ice",
            "Spark",
            "Sword",
            "Cutter",
            "Beam",
            "Stone",
            "Needle",
            "Parasol",
            "Wheel",
            "Bomb",
            "Fighter",
            "Suplex",
            "Ninja",
            "Mirror",
            "Hammer",
            "Knight",
            "Wing",
            "UFO",
            "Sleep"
        },
        editable = false
    }
}

kirbyPlayerSpawner.fieldOrder = {
    "x", "y",
    "enableKirbyMode",
    "spawnCompanion",
    "startingAbility"
}

function kirbyPlayerSpawner.sprite(room, entity)
    local texture
    if entity.enableKirbyMode then
        texture = "characters/kirby/idle00"
    else
        texture = "characters/player/sitDown00"
    end

    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

return kirbyPlayerSpawner
