local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local barrelBomber = {}

barrelBomber.name = "DZ/BarrelBomber"
barrelBomber.depth = -50
barrelBomber.fieldInformation = {
    health = {
        minimumValue = 1
    },
    detectionRange = {
        minimumValue = 0
    },
    explosionRadius = {
        minimumValue = 0
    },
    fuseTime = {
        minimumValue = 0
    }
}
barrelBomber.placements = {
    {
        name = "main",
        data = {
            health = 1,
            detectionRange = 80,
            explosionRadius = 100,
            fuseTime = 1.5
        }
    }
}

function barrelBomber.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/barrel_bomber/hidden", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function barrelBomber.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 32, 24, 32)
end

return barrelBomber
