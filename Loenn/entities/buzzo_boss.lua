-- Loenn plugin for MaggyHelper - Buzzo Mid-Boss (chainsaw-wielding maniac)
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local buzzoBoss = {}

buzzoBoss.name = "MaggyHelper/BuzzoBoss"
buzzoBoss.depth = -8500

buzzoBoss.placements = {
    {
        name = "buzzo_boss",
        data = {
            health = 280,
            maxHealth = 280
        }
    }
}

buzzoBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

buzzoBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function buzzoBoss.sprite(room, entity)
    local textures = {
        "characters/buzzo/body_idle00",
        "characters/buzzo/idle00",
        "characters/enemies/buzzo/idle00"
    }
    for _, texture in ipairs(textures) do
        local ok, sprite = pcall(drawableSprite.fromTexture, texture, entity)
        if ok and sprite then
            sprite:setJustification(0.5, 1.0)
            return {sprite}
        end
    end
    return {}
end

function buzzoBoss.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 48, 32, 48)
end

return buzzoBoss
