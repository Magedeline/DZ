local utils = require("utils")

local magicFountain = {}

magicFountain.name = "MaggyHelper/MagicFountain"
magicFountain.depth = 0

magicFountain.placements = {
    {
        name = "MagicFountain",
        data = {
            fountainType = "healing",
            particleCount = 50,
            isActive = true,
            usesRemaining = -1,
            healAmount = 1
        }
    },
    {
        name = "power_fountain",
        data = {
            fountainType = "power",
            particleCount = 75,
            isActive = true,
            usesRemaining = 1,
            healAmount = 0
        }
    }
}

magicFountain.fieldInformation = {
    fountainType = {
        options = {"healing", "power", "save", "checkpoint"},
        editable = false
    },
    particleCount = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 200
    },
    isActive = {
        fieldType = "boolean"
    },
    usesRemaining = {
        fieldType = "integer",
        minimumValue = -1,
        maximumValue = 99
    },
    healAmount = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 10
    }
}

function magicFountain.sprite(room, entity)
    local color = {0.2, 0.4, 0.9, 1.0}
    
    if entity.fountainType == "healing" then
        color = {0.2, 0.9, 0.2, 1.0}
    elseif entity.fountainType == "power" then
        color = {0.9, 0.2, 0.9, 1.0}
    elseif entity.fountainType == "save" then
        color = {0.9, 0.9, 0.2, 1.0}
    end
    
    return {
        {
            texture = "objects/fountain/fountain_idle00",
            x = entity.x,
            y = entity.y,
            color = color
        }
    }
end

function magicFountain.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 24, 32, 32)
end

return magicFountain
