local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local tesseractBoss = {}

tesseractBoss.name = "MaggyHelper/TesseractBoss"
tesseractBoss.depth = -8500
tesseractBoss.justification = {0.5, 0.5}

tesseractBoss.placements = {
    {
        name = "tesseract_boss",
        data = {
            health = 1350,
            maxHealth = 1350,
    }
    }
}

tesseractBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    }
}

tesseractBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth"
}

function tesseractBoss.sprite(room, entity)
    local textures = {
        "objects/tesseract_temple/dashButton00",
        "objects/tesseract_temple/dashButtonMirror00",
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

function tesseractBoss.selection(room, entity)
    return utils.rectangle(entity.x - 27, entity.y - 27, 54, 54)
end

return tesseractBoss
