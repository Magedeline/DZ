local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local shadowFigure = {}

shadowFigure.name = "DZ/ShadowFigure"
shadowFigure.depth = -50
shadowFigure.fieldInformation = {
    detectionRange = {
        minimumValue = 0
    },
    followDistance = {
        minimumValue = 0
    }
}
shadowFigure.placements = {
    {
        name = "passive",
        data = {
            detectionRange = 150,
            followDistance = 80,
            isHostile = false
        }
    },
    {
        name = "hostile",
        data = {
            detectionRange = 150,
            followDistance = 80,
            isHostile = true
        }
    }
}

function shadowFigure.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/shadow_figure/hidden", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.0, 0.0, 0.0, 0.7}
    return sprite
end

function shadowFigure.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 48, 24, 48)
end

return shadowFigure
