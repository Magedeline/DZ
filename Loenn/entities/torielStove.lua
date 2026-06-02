local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local torielStove = {}

torielStove.name = "MaggyHelper/TorielStoveEntity"
torielStove.depth = -100
torielStove.fieldInformation = {
    healAmount = {
        minimumValue = 0
    },
    cookDuration = {
        minimumValue = 0
    }
}
torielStove.placements = {
    {
        name = "normal",
        data = {
            canInteract = true,
            hasPie = true,
            healAmount = 3,
            dialogueId = "TORIEL_STOVE",
            cookDuration = 5.0
        }
    }
}

function torielStove.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/toriel_stove/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {1.0, 0.8, 0.6, 1.0}
    return sprite
end

function torielStove.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 40, 32, 40)
end

return torielStove
