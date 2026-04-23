local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local heatWave = {}

heatWave.name = "MaggyHelper/HeatWave"
heatWave.depth = -50
heatWave.fieldInformation = {
    maxRadius = {
        minimumValue = 0
    },
    expansionSpeed = {
        minimumValue = 0
    },
    pushForce = {
        minimumValue = 0
    },
    interval = {
        minimumValue = 0
    }
}
heatWave.placements = {
    {
        name = "normal",
        data = {
            maxRadius = 150,
            expansionSpeed = 100,
            pushForce = 150,
            interval = 5.0,
            isActive = true
        }
    }
}

function heatWave.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("effects/heat_wave/dormant", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {1.0, 0.3, 0.0, 0.6}
    return sprite
end

function heatWave.selection(room, entity)
    return utils.rectangle(entity.x - 4, entity.y - 4, 8, 8)
end

return heatWave
