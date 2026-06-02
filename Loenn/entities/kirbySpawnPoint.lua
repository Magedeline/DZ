-- Loenn plugin for MaggyHelper - Kirby Spawn Point Entity
local drawableSprite = require("structs.drawable_sprite")

local kirbySpawnPoint = {}

kirbySpawnPoint.name = "MaggyHelper/KirbySpawnPoint"
kirbySpawnPoint.depth = -100
kirbySpawnPoint.placements = {
    name = "kirbyspawnpoint",
    data = {
        spawnAsKirby = true,
        startingAbility = "None"
    }
}

kirbySpawnPoint.fieldInformation = {
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
            "Wing",
            "UFO",
            "Sleep"
        },
        editable = false
    }
}

function kirbySpawnPoint.sprite(room, entity)
    local texture = "objects/kirby/spawnPoint/icon"
    return drawableSprite.fromTexture(texture, entity)
end

return kirbySpawnPoint
