local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local summitCorruptedMountainBoss = {}

summitCorruptedMountainBoss.name = "MaggyHelper/SummitCorruptedMountainBoss"
summitCorruptedMountainBoss.depth = -8500
summitCorruptedMountainBoss.justification = {0.5, 0.5}

summitCorruptedMountainBoss.placements = {
    {
        name = "summit_corrupted_mountain_boss",
        data = {
            health = 1500,
            maxHealth = 1500,
        }
    }
}

summitCorruptedMountainBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

summitCorruptedMountainBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function summitCorruptedMountainBoss.sprite(room, entity)
    local textures = {
        "decals/maggy/9_beyond_summit/SummitFlag00",
        "decals/7-summit/SummitFlag00",
        "collectables/summitgems/0/gem00"
    }

    for _, texture in ipairs(textures) do
        local ok, sprite = pcall(drawableSprite.fromTexture, texture, entity)
        if ok and sprite then
            sprite:setJustification(0.5, 0.5)
            return {sprite}
        end
    end

    return {}
end

function summitCorruptedMountainBoss.selection(room, entity)
    return utils.rectangle(entity.x - 44, entity.y - 44, 88, 88)
end

return summitCorruptedMountainBoss