local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local glitchBlock = {}

glitchBlock.name = "DZ/GlitchBlock"
glitchBlock.depth = -9000
glitchBlock.minimumSize = {8, 8}
glitchBlock.fieldInformation = {
    stability = {
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    glitchInterval = {
        minimumValue = 0.1
    },
    visibleTime = {
        minimumValue = 0.0
    },
    invisibleTime = {
        minimumValue = 0.0
    }
}
glitchBlock.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            stability = 0.7,
            glitchInterval = 3.0,
            visibleTime = 2.0,
            invisibleTime = 1.0,
            isPattern = false
        }
    }
}

function glitchBlock.sprite(room, entity)
    local sprites = {}
    local texture = "objects/glitch_block/stable"

    local width = entity.width or 16
    local height = entity.height or 16

    for x = 0, width - 8, 8 do
        for y = 0, height - 8, 8 do
            local sprite = drawableSprite.fromTexture(texture, entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            sprite.color = {0.0, 0.8, 1.0, 1.0}
            table.insert(sprites, sprite)
        end
    end

    return sprites
end

function glitchBlock.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 16)
end

return glitchBlock
