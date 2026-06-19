local drawableSprite = require("structs.drawable_sprite")

local powerSourceNumber = {}

powerSourceNumber.name = "DZ/PowerSourceNumber"
powerSourceNumber.depth = -10010

powerSourceNumber.placements = {
    {
        name = "main",
        data = {
            index = 1,
            flag = "disable_lightning",
            gotCollectable = false,
            numberSprite = "",
            glowSprite = ""
        }
    }
}

powerSourceNumber.fieldInformation = {
    index = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 9
    },
    flag = {
        fieldType = "string"
    },
    gotCollectable = {
        fieldType = "boolean"
    },
    numberSprite = {
        fieldType = "string"
    },
    glowSprite = {
        fieldType = "string"
    }
}

powerSourceNumber.fieldOrder = {
    "x", "y", "index", "flag", "gotCollectable", "numberSprite", "glowSprite"
}

function powerSourceNumber.sprite(room, entity)
    local index = entity.index or 1
    local texturePath = "scenery/powersource_numbers/" .. tostring(index)

    -- Try custom sprite path first, fall back to default
    if entity.numberSprite and entity.numberSprite ~= "" then
        texturePath = entity.numberSprite
    end

    local sprite = drawableSprite.fromTexture(texturePath, entity)
    if sprite then
        sprite:setJustification(0.5, 0.5)
        return sprite
    end

    -- Fallback: simple rectangle if texture not found
    return {
        {
            texture = "objects/kevins_pc/pc_idle",
            x = entity.x,
            y = entity.y,
            color = {0.4, 0.8, 1.0, 1.0}
        }
    }
end

function powerSourceNumber.selection(room, entity)
    return require("utils").rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return powerSourceNumber
