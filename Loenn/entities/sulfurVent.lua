local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local sulfurVent = {}

sulfurVent.name = "DZ/SulfurVent"
sulfurVent.depth = -50
sulfurVent.placements = {
    {
        name = "normal",
        data = {}
    }
}

function sulfurVent.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/sulfur_vent/default", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.8, 0.8, 0.4, 1.0}
    return sprite
end

function sulfurVent.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 12, 16, 12)
end

return sulfurVent
