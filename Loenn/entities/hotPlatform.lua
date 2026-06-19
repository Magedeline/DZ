local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local hotPlatform = {}

hotPlatform.name = "DZ/HotPlatform"
hotPlatform.depth = -9000
hotPlatform.minimumSize = {8, 8}
hotPlatform.fieldInformation = {
    heatRate = {
        minimumValue = 0
    },
    coolRate = {
        minimumValue = 0
    },
    maxHeat = {
        minimumValue = 0
    }
}
hotPlatform.placements = {
    {
        name = "main",
        data = {
            width = 32,
            height = 8,
            heatRate = 20,
            coolRate = 10,
            maxHeat = 100
        }
    }
}

function hotPlatform.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 32
    local height = entity.height or 8
    
    for x = 0, width - 8, 8 do
        for y = 0, height - 8, 8 do
            local sprite = drawableSprite.fromTexture("objects/hot_platform/cool", entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function hotPlatform.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 32, entity.height or 8)
end

return hotPlatform
