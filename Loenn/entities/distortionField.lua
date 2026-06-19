local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local distortionField = {}

distortionField.name = "DZ/DistortionField"
distortionField.fieldInformation = {
    distortionType = {
        options = {"Reverse", "Random", "GravityFlip", "SlowMotion"},
        editable = false
    },
    intensity = {
        minimumValue = 0,
        maximumValue = 1
    }
}
distortionField.placements = {
    {
        name = "main",
        data = {
            distortionType = "Reverse",
            intensity = 1.0,
            width = 32,
            height = 32
        }
    },
    {
        name = "random",
        data = {
            distortionType = "Random",
            intensity = 1.0,
            width = 32,
            height = 32
        }
    },
    {
        name = "gravity_flip",
        data = {
            distortionType = "GravityFlip",
            intensity = 1.0,
            width = 32,
            height = 32
        }
    },
    {
        name = "slow_motion",
        data = {
            distortionType = "SlowMotion",
            intensity = 0.5,
            width = 32,
            height = 32
        }
    }
}

function distortionField.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 32
    local height = entity.height or 32
    
    for x = 0, width - 8, 8 do
        for y = 0, height - 8, 8 do
            local sprite = drawableSprite.fromTexture("effects/distortion_field/default", entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            sprite.color = {0.5, 0.0, 0.5, 0.3}
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function distortionField.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 32, entity.height or 32)
end

return distortionField
