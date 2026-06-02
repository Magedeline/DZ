local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local matrixRain = {}

matrixRain.name = "MaggyHelper/MatrixRain"
matrixRain.depth = -50
matrixRain.fieldInformation = {
    rainWidth = {
        minimumValue = 0
    },
    rainHeight = {
        minimumValue = 0
    },
    dropSpeed = {
        minimumValue = 0
    },
    density = {
        minimumValue = 0,
        maximumValue = 1
    },
    intensity = {
        options = {"Inactive", "Light", "Normal", "Heavy", "Intense"},
        editable = false
    }
}
matrixRain.placements = {
    {
        name = "normal",
        data = {
            rainWidth = 200,
            rainHeight = 400,
            dropSpeed = 150,
            density = 0.5,
            intensity = "Normal"
        }
    }
}

function matrixRain.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("effects/matrix_rain/normal", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {0.0, 1.0, 0.0, 0.5}
    return sprite
end

function matrixRain.selection(room, entity)
    local w = entity.rainWidth or 200
    local h = entity.rainHeight or 400
    return utils.rectangle(entity.x - w/2, entity.y - h/2, w, h)
end

return matrixRain
