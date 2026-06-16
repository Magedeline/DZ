local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local flameGeyser = {}

flameGeyser.name = "DZ/FlameGeyser"
flameGeyser.depth = -50
flameGeyser.fieldInformation = {
    eruptInterval = {
        minimumValue = 0
    },
    eruptDuration = {
        minimumValue = 0
    },
    warningTime = {
        minimumValue = 0
    },
    flameHeight = {
        minimumValue = 0
    },
    damageRadius = {
        minimumValue = 0
    }
}
flameGeyser.placements = {
    {
        name = "normal",
        data = {
            eruptInterval = 4.0,
            eruptDuration = 1.0,
            warningTime = 1.0,
            flameHeight = 200,
            damageRadius = 30
        }
    }
}

function flameGeyser.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/flame_geyser/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function flameGeyser.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 16, 32, 16)
end

return flameGeyser
