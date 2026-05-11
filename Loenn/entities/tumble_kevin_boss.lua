local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local tumbleKevinBoss = {}

tumbleKevinBoss.name = "MaggyHelper/TumbleKevinBoss"
tumbleKevinBoss.depth = -8500
tumbleKevinBoss.justification = {0.5, 0.5}

tumbleKevinBoss.placements = {
    {
        name = "tumble_kevin_boss",
        data = {
            health = 1200,
            maxHealth = 1200,
        }
    }
}

tumbleKevinBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

tumbleKevinBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function tumbleKevinBoss.sprite(room, entity)
    local textures = {
        "objects/crushblock/block00",
        "characters/Maggy/DesoloZantas/kirby/stone/crush00",
        "characters/Maggy/DesoloZantas/Kglobal::Player/sitDown00"
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

function tumbleKevinBoss.selection(room, entity)
    return utils.rectangle(entity.x - 32, entity.y - 32, 64, 64)
end

return tumbleKevinBoss

