local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local flameCerberus = {}

flameCerberus.name = "DZ/FlameCerberus"
flameCerberus.depth = -100
flameCerberus.fieldInformation = {
    headHealth = {
        minimumValue = 1
    },
    detectionRange = {
        minimumValue = 0
    },
    moveSpeed = {
        minimumValue = 0
    }
}
flameCerberus.placements = {
    {
        name = "normal",
        data = {
            headHealth = 3,
            detectionRange = 200,
            moveSpeed = 60
        }
    }
}

function flameCerberus.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/flame_cerberus/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function flameCerberus.selection(room, entity)
    return utils.rectangle(entity.x - 40, entity.y - 48, 80, 48)
end

return flameCerberus
