local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local saloonChandelier = {}

saloonChandelier.name = "DZ/SaloonChandelier"
saloonChandelier.depth = -100
saloonChandelier.fieldInformation = {
    swingPeriod = {
        minimumValue = 0
    },
    swingAngle = {
        minimumValue = 0
    },
    chainLength = {
        minimumValue = 0
    }
}
saloonChandelier.placements = {
    {
        name = "main",
        data = {
            swingPeriod = 3.0,
            swingAngle = 0.4,
            chainLength = 80,
            canFall = true,
            isHazard = true
        }
    }
}

function saloonChandelier.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/saloon_chandelier/swinging", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.0)
    return sprite
end

function saloonChandelier.selection(room, entity)
    return utils.rectangle(entity.x - 20, entity.y, 40, 24)
end

return saloonChandelier
