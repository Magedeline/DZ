local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local soulFragment = {}

soulFragment.name = "DZ/SoulFragment"
soulFragment.depth = -100
soulFragment.fieldInformation = {
    requiredSouls = {
        minimumValue = 1
    }
}
soulFragment.placements = {
    {
        name = "normal",
        data = {
            fragmentId = "",
            requiredSouls = 3
        }
    }
}

function soulFragment.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/soul_fragment/hidden", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {1.0, 1.0, 1.0, 0.9}
    return sprite
end

function soulFragment.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return soulFragment
