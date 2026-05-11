local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local galactaKnightBoss = {}

galactaKnightBoss.name = "MaggyHelper/GalactaKnightBoss"
galactaKnightBoss.depth = -8500
galactaKnightBoss.justification = {0.5, 1.0}

galactaKnightBoss.placements = {
    {
        name = "galacta_knight_boss",
        data = {
            health = 1700,
            maxHealth = 1700,
        }
    }
}

galactaKnightBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

galactaKnightBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function galactaKnightBoss.sprite(room, entity)
    local textures = {
        "Gui/Maggy/DesoloZantas/kirby/powers/knight",
        "characters/Maggy/DesoloZantas/Kglobal::Player/sitDown00"
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

function galactaKnightBoss.selection(room, entity)
    return utils.rectangle(entity.x - 17, entity.y - 48, 34, 48)
end

return galactaKnightBoss

