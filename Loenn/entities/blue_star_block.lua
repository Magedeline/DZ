local drawableSprite = require("structs.drawable_sprite")

local blueStarBlock = {}

blueStarBlock.name = "DZ/BlueStarBlock"
blueStarBlock.depth = -10000
blueStarBlock.minimumSize = {8, 8}
blueStarBlock.canResize = {true, true}

blueStarBlock.placements = {
    {
        name = "BlueStarBlock",
        data = {
            width = 8,
            height = 8
        }
    }
}

local function textureForSize(entity)
    local width = entity.width or 8
    local height = entity.height or 8
    local area = width * height

    if area >= 256 then
        return "objects/bluestarblock/oversized"
    end

    if area >= 128 then
        return "objects/bluestarblock/large"
    end

    return "objects/bluestarblock/normal"
end

function blueStarBlock.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(textureForSize(entity), entity)
    sprite:setJustification(0.0, 0.0)
    sprite:setScale((entity.width or 8) / sprite.meta.width, (entity.height or 8) / sprite.meta.height)
    return {sprite}
end

return blueStarBlock
