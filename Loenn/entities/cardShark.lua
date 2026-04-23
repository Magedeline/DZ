local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local cardShark = {}

cardShark.name = "MaggyHelper/CardShark"
cardShark.depth = -100
cardShark.fieldInformation = {
    health = {
        minimumValue = 1
    },
    detectionRange = {
        minimumValue = 0
    },
    throwInterval = {
        minimumValue = 0
    },
    cardsPerThrow = {
        minimumValue = 1
    },
    patrolDistance = {
        minimumValue = 0
    }
}
cardShark.placements = {
    {
        name = "normal",
        data = {
            health = 2,
            detectionRange = 180,
            throwInterval = 1.5,
            cardsPerThrow = 3,
            patrolDistance = 80
        }
    }
}

function cardShark.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/card_shark/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function cardShark.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 28, 32, 28)
end

return cardShark
