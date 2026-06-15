-- Loenn plugin for MaggyHelper - Blackhole Angel Boss (final boss)
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local blackholeAngelBoss = {}

blackholeAngelBoss.name = "MaggyHelper/BlackholeAngelBoss"
blackholeAngelBoss.depth = -8500

blackholeAngelBoss.placements = {
    {
        name = "blackhole_angel_boss",
        data = {
            health = 1500,
            maxHealth = 1500
        }
    }
}

blackholeAngelBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

blackholeAngelBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function blackholeAngelBoss.sprite(room, entity)
    local textures = {
        "characters/blackhole_angel/idle00",
        "characters/bosses/blackhole_angel/idle00",
        "characters/Kglobal::Player/sitDown00"
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

function blackholeAngelBoss.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 48, 48, 48)
end

return blackholeAngelBoss
