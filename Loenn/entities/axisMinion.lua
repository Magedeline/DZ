local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local axisMinion = {}

axisMinion.name = "DZ/AxisMinion"
axisMinion.depth = -100
axisMinion.fieldInformation = {
    health = {
        minimumValue = 1
    },
    moveSpeed = {
        minimumValue = 0
    },
    detectionRange = {
        minimumValue = 0
    },
    patrolDistance = {
        minimumValue = 0
    }
}
axisMinion.placements = {
    {
        name = "normal",
        data = {
            health = 2,
            moveSpeed = 60,
            detectionRange = 120,
            patrolDistance = 80
        }
    }
}

function axisMinion.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/axis_minion/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function axisMinion.selection(room, entity)
    return utils.rectangle(entity.x - 10, entity.y - 24, 20, 24)
end

return axisMinion
