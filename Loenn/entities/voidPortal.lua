local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local voidPortal = {}

voidPortal.name = "DZ/VoidPortal"
voidPortal.depth = -100
voidPortal.placements = {
    {
        name = "main",
        data = {
            destinationId = "",
            isFinalPortal = false
        }
    },
    {
        name = "final",
        data = {
            destinationId = "",
            isFinalPortal = true
        }
    }
}

function voidPortal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/void_portal/inactive", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.5, 0.0, 0.5, 0.8}
    return sprite
end

function voidPortal.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 64, 48, 64)
end

return voidPortal
