local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local ridgeGate = {}

ridgeGate.name = "MaggyHelper/RidgeGate"
ridgeGate.depth = -9000
ridgeGate.minimumSize = {8, 8}

ridgeGate.placements = {
    {
        name = "RidgeGate",
        data = {
            width = 8,
            height = 48,
            flag = "",
            inverted = false
        }
    }
}

ridgeGate.fieldInformation = {
    flag = {
        fieldType = "string",
        description = "Session flag that controls the gate"
    },
    inverted = {
        fieldType = "boolean",
        description = "If true, gate opens when flag is false"
    }
}

ridgeGate.fieldOrder = {
    "x", "y", "width", "height", "flag", "inverted"
}

function ridgeGate.sprite(room, entity)
    local sprites = {}
    local texture = "objects/ridgeGate"
    
    local width = entity.width or 8
    local height = entity.height or 48
    
    for x = 0, width - 8, 8 do
        for y = 0, height - 8, 8 do
            local sprite = drawableSprite.fromTexture(texture, entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function ridgeGate.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 8, entity.height or 48)
end

return ridgeGate
