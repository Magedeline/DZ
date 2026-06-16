local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local echoFlower = {}

echoFlower.name = "DZ/EchoFlowerEntity"
echoFlower.depth = -100
echoFlower.fieldInformation = {
    echoDelay = {
        minimumValue = 0
    },
    echoSpeed = {
        minimumValue = 0
    },
    cooldownTime = {
        minimumValue = 0
    },
    maxEchoes = {
        minimumValue = 1
    }
}
echoFlower.placements = {
    {
        name = "normal",
        data = {
            echoDelay = 0.5,
            echoSpeed = 200,
            cooldownTime = 1.0,
            maxEchoes = 3
        }
    }
}

function echoFlower.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/echo_flower/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.6, 0.8, 1.0, 1.0}
    return sprite
end

function echoFlower.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 24, 16, 24)
end

return echoFlower
