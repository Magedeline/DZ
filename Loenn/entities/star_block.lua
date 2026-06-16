local drawableSprite = require("structs.drawable_sprite")

local starBlock = {}

starBlock.name = "DZ/StarBlock"
starBlock.depth = -10000
starBlock.minimumSize = {8, 8}
starBlock.canResize = {true, true}

starBlock.placements = {
    {
        name = "StarBlock",
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
        return "objects/starblock/oversized"
    end

    if area >= 128 then
        return "objects/starblock/large"
    end

    return "objects/starblock/normal"
end

function starBlock.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(textureForSize(entity), entity)
    sprite:setJustification(0.0, 0.0)
    sprite:setScale((entity.width or 8) / sprite.meta.width, (entity.height or 8) / sprite.meta.height)

    return {sprite}
end

return starBlock
