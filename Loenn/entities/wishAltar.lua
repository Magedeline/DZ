local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local wishAltar = {}

wishAltar.name = "DZ/WishAltar"
wishAltar.depth = -100
wishAltar.fieldInformation = {
    requiredHearts = {
        minimumValue = 0
    }
}
wishAltar.placements = {
    {
        name = "normal",
        data = {
            canInteract = true,
            dialoguePrefix = "WISH_ALTAR",
            requiredHearts = 0
        }
    }
}

function wishAltar.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/wish_altar/inactive", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.8, 0.6, 1.0, 1.0}
    return sprite
end

function wishAltar.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 48, 48, 48)
end

return wishAltar
