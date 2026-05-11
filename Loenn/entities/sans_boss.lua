local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local sansBoss = {}

sansBoss.name = "MaggyHelper/SansBoss"
sansBoss.depth = -8500
sansBoss.justification = {0.5, 1.0}

sansBoss.placements = {
    {
        name = "sans_boss",
        data = {
            health = 1100,
            maxHealth = 1100,
        }
    }
}

sansBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

sansBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function sansBoss.sprite(room, entity)
    local textures = {
        "characters/Maggy/DesoloZantas/sans/idle00",
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

function sansBoss.selection(room, entity)
    return utils.rectangle(entity.x - 13, entity.y - 36, 26, 36)
end

return sansBoss

