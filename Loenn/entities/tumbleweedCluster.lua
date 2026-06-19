local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local tumbleweedCluster = {}

tumbleweedCluster.name = "DZ/TumbleweedCluster"
tumbleweedCluster.depth = -50
tumbleweedCluster.fieldInformation = {
    rollSpeed = {
        minimumValue = 0
    },
    pushForce = {
        minimumValue = 0
    },
    tumbleweedCount = {
        minimumValue = 1
    },
    bounceChance = {
        minimumValue = 0,
        maximumValue = 1
    }
}
tumbleweedCluster.placements = {
    {
        name = "main",
        data = {
            rollSpeed = 180,
            pushForce = 100,
            tumbleweedCount = 3,
            bounceChance = 0.3
        }
    }
}

function tumbleweedCluster.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/tumbleweed/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function tumbleweedCluster.selection(room, entity)
    return utils.rectangle(entity.x - 30, entity.y - 20, 60, 40)
end

return tumbleweedCluster
