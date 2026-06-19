local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local realityGlitch = {}

realityGlitch.name = "DZ/RealityGlitch"
realityGlitch.depth = -50
realityGlitch.minimumSize = {16, 16}
realityGlitch.fieldInformation = {
    glitchIntensity = {
        minimumValue = 0,
        maximumValue = 1
    },
    teleportChance = {
        minimumValue = 0,
        maximumValue = 1
    }
}
realityGlitch.placements = {
    {
        name = "main",
        data = {
            width = 32,
            height = 32,
            glitchIntensity = 0.5,
            teleportChance = 0.1
        }
    }
}

function realityGlitch.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 32
    local height = entity.height or 32
    
    for x = 0, width - 16, 16 do
        for y = 0, height - 16, 16 do
            local sprite = drawableSprite.fromTexture("effects/reality_glitch/minor", entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            sprite.color = {0.5, 0.0, 0.5, 0.4}
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function realityGlitch.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 32, entity.height or 32)
end

return realityGlitch
