local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local whiteBlock = {}

whiteBlock.name = "DZ/WhiteBlock"
whiteBlock.depth = -9000
whiteBlock.minimumSize = {8, 8}
whiteBlock.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16
        }
    }
}

function whiteBlock.sprite(room, entity)
    local sprites = {}
    local texture = "objects/whiteblock"
    
    local width = entity.width or 16
    local height = entity.height or 16
    
    for x = 0, width - 8, 8 do
        for y = 0, height - 8, 8 do
            local sprite = drawableSprite.fromTexture(texture, entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function whiteBlock.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 16)
end

return whiteBlock
