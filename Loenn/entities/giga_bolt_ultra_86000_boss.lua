local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local gigaBoltUltraBoss = {}

gigaBoltUltraBoss.name = "MaggyHelper/GigaBoltUltra86000Boss"
gigaBoltUltraBoss.depth = -8500
gigaBoltUltraBoss.justification = {0.5, 0.5}

gigaBoltUltraBoss.placements = {
    {
        name = "giga_bolt_ultra_86000_boss",
        data = {
            health = 1100,
            maxHealth = 1100,
    }
    }
}

gigaBoltUltraBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

gigaBoltUltraBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function gigaBoltUltraBoss.sprite(room, entity)
    local textures = {
        "characters/gigaboltultra86000/idle00",
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

function gigaBoltUltraBoss.selection(room, entity)
    return utils.rectangle(entity.x - 28, entity.y - 28, 56, 56)
end

return gigaBoltUltraBoss
