local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local digitalPortal = {}

digitalPortal.name = "DZ/DigitalPortal"
digitalPortal.depth = -100
digitalPortal.fieldInformation = {
    transportDelay = {
        minimumValue = 0
    }
}
digitalPortal.placements = {
    {
        name = "two_way",
        data = {
            destinationId = "",
            transportDelay = 0.5,
            isTwoWay = true
        }
    },
    {
        name = "one_way",
        data = {
            destinationId = "",
            transportDelay = 0.5,
            isTwoWay = false
        }
    }
}

function digitalPortal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/digital_portal/inactive", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.0, 0.8, 1.0, 0.8}
    return sprite
end

function digitalPortal.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 48, 32, 48)
end

return digitalPortal
