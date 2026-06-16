local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local crownBearing = {}

crownBearing.name = "DZ/CrownBearing"
crownBearing.depth = -50
crownBearing.fieldInformation = {
    gravityType = {
        options = {"Pull", "Push", "Orbit"},
        editable = false
    },
    gravityRadius = {
        minimumValue = 0
    },
    gravityStrength = {
        minimumValue = 0
    }
}
crownBearing.placements = {
    {
        name = "pull",
        data = {
            gravityType = "Pull",
            gravityRadius = 150,
            gravityStrength = 200,
            isActive = true
        }
    },
    {
        name = "push",
        data = {
            gravityType = "Push",
            gravityRadius = 150,
            gravityStrength = 200,
            isActive = true
        }
    }
}

function crownBearing.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/crown_bearing/active", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {0.8, 0.0, 1.0, 0.9}
    return sprite
end

function crownBearing.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return crownBearing
