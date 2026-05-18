local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local alphaApexPredatorBoss = {}

alphaApexPredatorBoss.name = "MaggyHelper/AlphaApexPredatorBoss"
alphaApexPredatorBoss.depth = -8500
alphaApexPredatorBoss.justification = {0.5, 0.5}

alphaApexPredatorBoss.placements = {
    {
        name = "alpha_apex_predator_boss",
        data = {
            health = 1600,
            maxHealth = 1600,
    }
    }
}

alphaApexPredatorBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

alphaApexPredatorBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function alphaApexPredatorBoss.sprite(room, entity)
    local textures = {
        "characters/Maggy/DesoloZantas/monsters/predator00",
        "characters/Enemies/monsters/predator00",
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

function alphaApexPredatorBoss.selection(room, entity)
    return utils.rectangle(entity.x - 29, entity.y - 42, 58, 42)
end

return alphaApexPredatorBoss
