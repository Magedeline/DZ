local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local magmaPool = {}

magmaPool.name = "DZ/MagmaPool"
magmaPool.depth = -50
magmaPool.minimumSize = {16, 16}
magmaPool.fieldInformation = {
    bubbleInterval = {
        minimumValue = 0
    },
    eruptInterval = {
        minimumValue = 0
    }
}
magmaPool.placements = {
    {
        name = "deadly",
        data = {
            width = 48,
            height = 16,
            bubbleInterval = 0.5,
            eruptInterval = 3.0,
            isInstantDeath = true
        }
    },
    {
        name = "damage",
        data = {
            width = 48,
            height = 16,
            bubbleInterval = 0.5,
            eruptInterval = 3.0,
            isInstantDeath = false
        }
    }
}

function magmaPool.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 48
    local height = entity.height or 16
    
    for x = 0, width - 16, 16 do
        for y = 0, height - 16, 16 do
            local sprite = drawableSprite.fromTexture("objects/magma_pool/idle", entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            sprite.color = {1.0, 0.3, 0.0, 0.8}
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function magmaPool.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 48, entity.height or 16)
end

return magmaPool
