local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local coreFragment = {}

coreFragment.name = "DZ/CoreFragment"
coreFragment.depth = -100
coreFragment.fieldInformation = {
    fragmentIndex = {
        minimumValue = 1
    },
    requiredShields = {
        minimumValue = 0
    }
}
coreFragment.placements = {
    {
        name = "protected",
        data = {
            fragmentId = "",
            fragmentIndex = 1,
            requiresProtection = true,
            requiredShields = 3
        }
    },
    {
        name = "open",
        data = {
            fragmentId = "",
            fragmentIndex = 1,
            requiresProtection = false,
            requiredShields = 0
        }
    }
}

function coreFragment.sprite(room, entity)
    local state = entity.requiresProtection and "protected" or "active"
    local sprite = drawableSprite.fromTexture("objects/core_fragment/" .. state, entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    
    -- Color based on index
    local colors = {
        {0.0, 1.0, 1.0, 1.0},
        {0.0, 1.0, 0.5, 1.0},
        {1.0, 0.5, 0.0, 1.0},
        {1.0, 0.0, 0.5, 1.0}
    }
    local idx = (entity.fragmentIndex or 1) % 4
    if idx == 0 then idx = 4 end
    sprite.color = colors[idx]
    
    return sprite
end

function coreFragment.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return coreFragment
