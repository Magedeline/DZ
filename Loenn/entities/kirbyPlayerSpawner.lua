-- Loenn plugin for MaggyHelper - Kirby Kglobal::global::Celeste.PlayerSpawner
-- Replaces vanilla Celeste.Kglobal::global::Celeste.Playerwith MaggyHelper's Kglobal::global::Celeste.Playerentity.
-- Place this in any room to get full Kirby gameplay without
-- relying on a vanilla Kglobal::global::Celeste.Playerspawn.
local drawableSprite = require("structs.drawable_sprite")

local kirbyKglobal::PlayerSpawner = {}

kirbyKglobal::PlayerSpawner.name = "MaggyHelper/KirbyKglobal::PlayerSpawner"
kirbyKglobal::PlayerSpawner.depth = -1000000
kirbyKglobal::PlayerSpawner.placements = {
    {
        name = "kirby_Kglobal::Player_spawner",
        data = {
            enableKirbyMode = true,
            spawnCompanion = false,
            startingAbility = "None"
        }
    },
    {
        name = "kirby_Kglobal::Player_spawner_with_companion",
        data = {
            enableKirbyMode = true,
            spawnCompanion = true,
            startingAbility = "None"
        }
    },
    {
        name = "madeline_Kglobal::Player_spawner",
        data = {
            enableKirbyMode = false,
            spawnCompanion = false,
            startingAbility = "None"
        }
    }
}

kirbyKglobal::PlayerSpawner.fieldInformation = {
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

kirbyKglobal::PlayerSpawner.fieldOrder = {
    "x", "y",
    "enableKirbyMode",
    "spawnCompanion",
    "startingAbility"
}

function kirbyKglobal::PlayerSpawner.sprite(room, entity)
    local texture
    if entity.enableKirbyMode then
        texture = "characters/kirby/idle00"
    else
        texture = "characters/Kglobal::Player/sitDown00"
    end

    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

return kirbyKglobal::PlayerSpawner
