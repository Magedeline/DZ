local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local virusCloud = {}

virusCloud.name = "DZ/VirusCloud"
virusCloud.depth = -50
virusCloud.fieldInformation = {
    health = {
        minimumValue = 1
    },
    spreadRadius = {
        minimumValue = 0
    },
    moveSpeed = {
        minimumValue = 0
    },
    damageRate = {
        minimumValue = 0
    }
}
virusCloud.placements = {
    {
        name = "main",
        data = {
            health = 3,
            spreadRadius = 100,
            moveSpeed = 40,
            damageRate = 0.5
        }
    }
}

function virusCloud.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/virus_cloud/dormant", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {0.5, 0.0, 0.5, 0.7}
    return sprite
end

function virusCloud.selection(room, entity)
    return utils.rectangle(entity.x - 20, entity.y - 20, 40, 40)
end

return virusCloud
