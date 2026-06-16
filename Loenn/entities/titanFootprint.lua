local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local titanFootprint = {}

titanFootprint.name = "DZ/TitanFootprint"
titanFootprint.depth = -50
titanFootprint.fieldInformation = {
    warningDuration = {
        minimumValue = 0
    },
    crushDuration = {
        minimumValue = 0
    },
    crushWidth = {
        minimumValue = 0
    },
    crushHeight = {
        minimumValue = 0
    },
    cooldownTime = {
        minimumValue = 0
    },
    triggerDistance = {
        minimumValue = 0
    }
}
titanFootprint.placements = {
    {
        name = "normal",
        data = {
            warningDuration = 1.5,
            crushDuration = 0.3,
            crushWidth = 120,
            crushHeight = 200,
            cooldownTime = 3.0,
            triggerDistance = 80
        }
    }
}

function titanFootprint.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/titan_footprint/shadow", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.0, 0.0, 0.0, 0.3}
    return sprite
end

function titanFootprint.selection(room, entity)
    return utils.rectangle(entity.x - 60, entity.y - 100, 120, 100)
end

return titanFootprint
