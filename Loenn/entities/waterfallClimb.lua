local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local waterfallClimb = {}

waterfallClimb.name = "MaggyHelper/WaterfallClimb"
waterfallClimb.depth = -50
waterfallClimb.minimumSize = {16, 16}
waterfallClimb.fieldInformation = {
    flowStrength = {
        minimumValue = 0
    },
    rushInterval = {
        minimumValue = 0
    },
    rushDuration = {
        minimumValue = 0
    }
}
waterfallClimb.placements = {
    {
        name = "normal",
        data = {
            width = 32,
            height = 64,
            flowStrength = 80,
            rushInterval = 5.0,
            rushDuration = 2.0
        }
    }
}

function waterfallClimb.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 32
    local height = entity.height or 64
    
    for x = 0, width - 16, 16 do
        for y = 0, height - 16, 16 do
            local sprite = drawableSprite.fromTexture("objects/waterfall/flowing", entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            sprite.color = {0.4, 0.7, 1.0, 0.7}
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function waterfallClimb.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 32, entity.height or 64)
end

return waterfallClimb
