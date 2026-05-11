local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local anotherVesselCorruptedBoss = {}

anotherVesselCorruptedBoss.name = "MaggyHelper/AnotherVesselCorruptedBoss"
anotherVesselCorruptedBoss.depth = -8500
anotherVesselCorruptedBoss.justification = {0.5, 1.0}

anotherVesselCorruptedBoss.placements = {
    {
        name = "another_vessel_corrupted_boss",
        data = {
            health = 1000,
            maxHealth = 1000,
        }
    }
}

anotherVesselCorruptedBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

anotherVesselCorruptedBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function anotherVesselCorruptedBoss.sprite(room, entity)
    local textures = {
        "characters/anothervesselcorrupted/idle00",
        "bgs/maggy/00/anotherhuman/VESSEL",
        "characters/Maggy/DesoloZantas/Kglobal::Player/sitDown00"
    }

    for _, texture in ipairs(textures) do
        local ok, sprite = pcall(drawableSprite.fromTexture, texture, entity)
        if ok and sprite then
            sprite:setJustification(0.5, 1.0)
            return {sprite}
        end
    end

    return {}
end

function anotherVesselCorruptedBoss.selection(room, entity)
    return utils.rectangle(entity.x - 18, entity.y - 58, 36, 58)
end

return anotherVesselCorruptedBoss
