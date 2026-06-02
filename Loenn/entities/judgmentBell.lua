local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local judgmentBell = {}

judgmentBell.name = "MaggyHelper/JudgmentBell"
judgmentBell.depth = -100
judgmentBell.fieldInformation = {
    shockwaveSpeed = {
        minimumValue = 0
    },
    shockwaveRadius = {
        minimumValue = 0
    },
    cooldownTime = {
        minimumValue = 0
    },
    maxRings = {
        minimumValue = 1
    }
}
judgmentBell.placements = {
    {
        name = "normal",
        data = {
            shockwaveSpeed = 200,
            shockwaveRadius = 300,
            cooldownTime = 2.0,
            maxRings = 3,
            canKglobal::PlayerRing = true
        }
    }
}

function judgmentBell.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/judgment_bell/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.8, 0.7, 0.4, 1.0}
    return sprite
end

function judgmentBell.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 48, 48, 48)
end

return judgmentBell
