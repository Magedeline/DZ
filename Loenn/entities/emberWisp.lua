local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local emberWisp = {}

emberWisp.name = "MaggyHelper/EmberWisp"
emberWisp.depth = -50
emberWisp.fieldInformation = {
    health = {
        minimumValue = 1
    },
    floatSpeed = {
        minimumValue = 0
    },
    igniteRadius = {
        minimumValue = 0
    },
    burnDuration = {
        minimumValue = 0
    }
}
emberWisp.placements = {
    {
        name = "normal",
        data = {
            health = 1,
            floatSpeed = 40,
            igniteRadius = 16,
            burnDuration = 3.0
        }
    }
}

function emberWisp.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/ember_wisp/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {1.0, 0.6, 0.0, 0.8}
    return sprite
end

function emberWisp.selection(room, entity)
    return utils.rectangle(entity.x - 6, entity.y - 6, 12, 12)
end

return emberWisp
