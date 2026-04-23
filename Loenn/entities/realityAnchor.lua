local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local realityAnchor = {}

realityAnchor.name = "MaggyHelper/RealityAnchor"
realityAnchor.depth = -100
realityAnchor.fieldInformation = {
    stabilityRadius = {
        minimumValue = 0
    }
}
realityAnchor.placements = {
    {
        name = "normal",
        data = {
            stabilityRadius = 100
        }
    }
}

function realityAnchor.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/reality_anchor/inactive", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {1.0, 1.0, 1.0, 0.8}
    return sprite
end

function realityAnchor.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 40, 32, 40)
end

return realityAnchor
