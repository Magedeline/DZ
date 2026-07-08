local drawableSprite = require("structs.drawable_sprite")

local starBlock = {}

starBlock.name = "DZ/StarBlock"
starBlock.depth = -10000
starBlock.warnBelowSize = {16, 16}

starBlock.placements = {
    name = "star_block",
    data = {
        width = 16,
        height = 16
    }
}

local textures = {
    small = "objects/DZ/starblock/normal",
    large = "objects/DZ/starblock/large",
    oversized = "objects/DZ/starblock/oversized"
}

local function getTexture(entity)
    local width = entity.width or 16
    local height = entity.height or 16
    local area = width * height

    if area >= 256 then
        return textures.oversized
    elseif area >= 128 then
        return textures.large
    end

    return textures.small
end

function starBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16

    local texture = getTexture(entity)
    local sprite = drawableSprite.fromTexture(texture, {
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    })

    if sprite then
        local scaleX = width / sprite.meta.width
        local scaleY = height / sprite.meta.height

        sprite:setScale(scaleX, scaleY)
    end

    return sprite
end

return starBlock
