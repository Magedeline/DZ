local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local memoryFragment = {}

memoryFragment.name = "DZ/MemoryFragment"
memoryFragment.depth = -100
memoryFragment.fieldInformation = {
    fragmentNumber = {
        minimumValue = 1
    }
}
memoryFragment.placements = {
    {
        name = "main",
        data = {
            fragmentId = "",
            dialogueKey = "MEMORY_FRAGMENT",
            fragmentNumber = 1,
            showDialogue = true
        }
    }
}

function memoryFragment.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/memory_fragment/floating", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    
    -- Color based on fragment number
    local colors = {
        {0.0, 0.8, 1.0, 1.0},
        {0.0, 1.0, 0.5, 1.0},
        {1.0, 0.8, 0.0, 1.0},
        {1.0, 0.4, 0.8, 1.0}
    }
    local idx = (entity.fragmentNumber or 1) % 4
    if idx == 0 then idx = 4 end
    sprite.color = colors[idx]
    
    return sprite
end

function memoryFragment.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return memoryFragment
