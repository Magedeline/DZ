local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local fountainSpirit = {}

fountainSpirit.name = "MaggyHelper/FountainSpirit"
fountainSpirit.depth = -100
fountainSpirit.fieldInformation = {
    spiritType = {
        options = {"Healing", "Platform", "Buff", "Guidance"},
        editable = false
    },
    healAmount = {
        minimumValue = 0
    },
    buffDuration = {
        minimumValue = 0
    }
}
fountainSpirit.placements = {
    {
        name = "healing",
        data = {
            spiritType = "Healing",
            healAmount = 3,
            buffDuration = 10.0
        }
    },
    {
        name = "buff",
        data = {
            spiritType = "Buff",
            healAmount = 0,
            buffDuration = 10.0
        }
    }
}

function fountainSpirit.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/fountain_spirit/dormant", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.6, 0.8, 1.0, 0.8}
    return sprite
end

function fountainSpirit.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 32, 24, 32)
end

return fountainSpirit
