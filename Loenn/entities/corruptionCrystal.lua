local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local corruptionCrystal = {}

corruptionCrystal.name = "DZ/CorruptionCrystal"
corruptionCrystal.depth = -50
corruptionCrystal.fieldInformation = {
    health = {
        minimumValue = 1
    },
    corruptionRadius = {
        minimumValue = 0
    },
    spreadSpeed = {
        minimumValue = 0
    }
}
corruptionCrystal.placements = {
    {
        name = "main",
        data = {
            health = 3,
            corruptionRadius = 100,
            spreadSpeed = 20
        }
    }
}

function corruptionCrystal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/corruption_crystal/dormant", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.4, 0.0, 0.0, 1.0}
    return sprite
end

function corruptionCrystal.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 48, 32, 48)
end

return corruptionCrystal
