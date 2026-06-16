local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local krackoBoss = {}

krackoBoss.name = "DZ/KrackoBoss"
krackoBoss.depth = -8500
krackoBoss.justification = {0.5, 0.5}

krackoBoss.placements = {
    {
        name = "kracko_boss",
        data = {
            health = 900,
            maxHealth = 900,
    }
    }
}

krackoBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

krackoBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function krackoBoss.sprite(room, entity)
    local textures = {
        "bosses/kirby/kracko/idle00",
        "characters/Kglobal::Player/sitDown00"
    }

    for _, texture in ipairs(textures) do
        local ok, sprite = pcall(drawableSprite.fromTexture, texture, entity)
        if ok and sprite then
            sprite:setJustification(0.5, 0.5)
            return {sprite}
        end
    end

    return {}
end

function krackoBoss.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 24, 48, 48)
end

return krackoBoss
