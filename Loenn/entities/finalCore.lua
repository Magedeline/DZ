local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local finalCore = {}

finalCore.name = "DZ/FinalCore"
finalCore.depth = -100
finalCore.fieldInformation = {
    health = {
        minimumValue = 1
    },
    attackInterval = {
        minimumValue = 0
    }
}
finalCore.placements = {
    {
        name = "main",
        data = {
            health = 10,
            attackInterval = 2.0
        }
    }
}

function finalCore.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/final_core/dormant", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {0.5, 0.0, 0.5, 1.0}
    return sprite
end

function finalCore.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 24, 48, 48)
end

return finalCore
