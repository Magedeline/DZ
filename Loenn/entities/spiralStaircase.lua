local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local spiralStaircase = {}

spiralStaircase.name = "DZ/SpiralStaircase"
spiralStaircase.depth = -100
spiralStaircase.fieldInformation = {
    rotationSpeed = {
        minimumValue = 0
    },
    maxSpeed = {
        minimumValue = 0
    },
    platformCount = {
        minimumValue = 1
    },
    radius = {
        minimumValue = 0
    }
}
spiralStaircase.placements = {
    {
        name = "clockwise",
        data = {
            rotationSpeed = 0.5,
            maxSpeed = 2.0,
            platformCount = 8,
            radius = 100,
            clockwise = true
        }
    },
    {
        name = "counterclockwise",
        data = {
            rotationSpeed = 0.5,
            maxSpeed = 2.0,
            platformCount = 8,
            radius = 100,
            clockwise = false
        }
    }
}

function spiralStaircase.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/spiral_staircase/center", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    return sprite
end

function spiralStaircase.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return spiralStaircase
