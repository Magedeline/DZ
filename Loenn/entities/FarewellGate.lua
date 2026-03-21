local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local farewellGate = {}

farewellGate.name = "MaggyHelper/FarewellGate"
farewellGate.depth = -9000
farewellGate.minimumSize = {8, 8}

farewellGate.placements = {
    {
        name = "normal",
        data = {
            width = 8,
            height = 48,
            flag = "",
            inverted = false,
            heartGems = 0
        }
    },
    {
        name = "heart_locked",
        data = {
            width = 8,
            height = 48,
            flag = "",
            inverted = false,
            heartGems = 1
        }
    }
}

farewellGate.fieldInformation = {
    flag = {
        fieldType = "string",
        description = "Session flag that controls the gate"
    },
    inverted = {
        fieldType = "boolean",
        description = "If true, gate opens when conditions are false"
    },
    heartGems = {
        fieldType = "integer",
        description = "Number of heart gems required to open"
    }
}

farewellGate.fieldOrder = {
    "x", "y", "width", "height", "flag", "inverted", "heartGems"
}

function farewellGate.sprite(room, entity)
    local sprites = {}
    local texture = "objects/farewellGate"
    
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

function farewellGate.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 8, entity.height or 48)
end

return farewellGate
