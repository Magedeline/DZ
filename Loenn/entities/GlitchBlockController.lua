local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local glitchBlockController = {}

glitchBlockController.name = "MaggyHelper/GlitchBlockController"
glitchBlockController.depth = -10000
glitchBlockController.fieldInformation = {
    syncInterval = {
        minimumValue = 0.1
    }
}
glitchBlockController.placements = {
    {
        name = "normal",
        data = {
            syncInterval = 2.0
        }
    }
}

function glitchBlockController.sprite(room, entity)
    local texture = "objects/glitch_block/controller"
    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {0.8, 0.0, 1.0, 1.0}
    return sprite
end

function glitchBlockController.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return glitchBlockController
