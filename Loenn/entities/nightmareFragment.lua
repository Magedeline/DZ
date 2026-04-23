local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local nightmareFragment = {}

nightmareFragment.name = "MaggyHelper/NightmareFragment"
nightmareFragment.depth = -100
nightmareFragment.fieldInformation = {
    fragmentNumber = {
        minimumValue = 1
    }
}
nightmareFragment.placements = {
    {
        name = "normal",
        data = {
            fragmentId = "",
            fragmentNumber = 1
        }
    }
}

function nightmareFragment.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/nightmare_fragment/hidden", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {0.5, 0.0, 0.5, 0.8}
    return sprite
end

function nightmareFragment.selection(room, entity)
    return utils.rectangle(entity.x - 10, entity.y - 10, 20, 20)
end

return nightmareFragment
