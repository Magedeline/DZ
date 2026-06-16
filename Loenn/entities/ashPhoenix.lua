local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local ashPhoenix = {}

ashPhoenix.name = "DZ/AshPhoenix"
ashPhoenix.depth = -100
ashPhoenix.fieldInformation = {
    health = {
        minimumValue = 1
    },
    flySpeed = {
        minimumValue = 0
    },
    detectionRange = {
        minimumValue = 0
    }
}
ashPhoenix.placements = {
    {
        name = "normal",
        data = {
            health = 3,
            flySpeed = 100,
            detectionRange = 250
        }
    }
}

function ashPhoenix.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/ash_phoenix/fly", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {1.0, 0.5, 0.0, 1.0}
    return sprite
end

function ashPhoenix.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 14, 32, 28)
end

return ashPhoenix
