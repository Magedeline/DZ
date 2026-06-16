local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local banditoRoller = {}

banditoRoller.name = "DZ/BanditoRoller"
banditoRoller.depth = -100
banditoRoller.fieldInformation = {
    health = {
        minimumValue = 1
    },
    rollSpeed = {
        minimumValue = 0
    },
    bounceSpeed = {
        minimumValue = 0
    },
    detectionRange = {
        minimumValue = 0
    }
}
banditoRoller.placements = {
    {
        name = "normal",
        data = {
            health = 2,
            rollSpeed = 150,
            bounceSpeed = 200,
            detectionRange = 200
        }
    }
}

function banditoRoller.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/bandito_roller/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function banditoRoller.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return banditoRoller
