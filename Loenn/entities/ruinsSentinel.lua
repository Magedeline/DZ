local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local ruinsSentinel = {}

ruinsSentinel.name = "MaggyHelper/RuinsSentinel"
ruinsSentinel.depth = -100
ruinsSentinel.fieldInformation = {
    health = {
        minimumValue = 1
    },
    detectionRange = {
        minimumValue = 0
    },
    attackRange = {
        minimumValue = 0
    },
    moveSpeed = {
        minimumValue = 0
    },
    patrolDistance = {
        minimumValue = 0
    }
}
ruinsSentinel.placements = {
    {
        name = "normal",
        data = {
            health = 3,
            detectionRange = 150,
            attackRange = 60,
            moveSpeed = 50,
            patrolDistance = 100
        }
    }
}

function ruinsSentinel.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/ruins_sentinel/dormant", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function ruinsSentinel.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 28, 24, 28)
end

return ruinsSentinel
