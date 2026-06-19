local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local ceremonyFlame = {}

ceremonyFlame.name = "DZ/CeremonyFlame"
ceremonyFlame.depth = -50
ceremonyFlame.fieldInformation = {
    spreadSpeed = {
        minimumValue = 0
    },
    maxSpreadDistance = {
        minimumValue = 0
    }
}
ceremonyFlame.placements = {
    {
        name = "main",
        data = {
            isSource = true,
            spreadSpeed = 20,
            maxSpreadDistance = 200,
            canSpread = true
        }
    },
    {
        name = "static",
        data = {
            isSource = false,
            spreadSpeed = 20,
            maxSpreadDistance = 200,
            canSpread = false
        }
    }
}

function ceremonyFlame.sprite(room, entity)
    local state = entity.isSource and "burning" or "dormant"
    local sprite = drawableSprite.fromTexture("objects/ceremony_flame/" .. state, entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {1.0, 0.4, 0.0, 0.9}
    return sprite
end

function ceremonyFlame.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 48, 24, 48)
end

return ceremonyFlame
