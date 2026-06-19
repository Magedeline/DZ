local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local mrBonesDoor = {}

mrBonesDoor.name = "DZ/MrBonesDoor"
mrBonesDoor.depth = -9000
mrBonesDoor.minimumSize = {8, 8}
mrBonesDoor.canResize = {false, true}

mrBonesDoor.placements = {
    {
        name = "main",
        data = {
            height = 48,
            requiredKeys = 0,
            flagToSet = ""
        }
    },
    {
        name = "key_locked",
        data = {
            height = 48,
            requiredKeys = 1,
            flagToSet = ""
        }
    }
}

mrBonesDoor.fieldInformation = {
    requiredKeys = {
        fieldType = "integer",
        description = "Number of keys required to open the door"
    },
    flagToSet = {
        fieldType = "string",
        description = "Session flag to set when the door opens"
    }
}

mrBonesDoor.fieldOrder = {
    "x", "y", "height", "requiredKeys", "flagToSet"
}

function mrBonesDoor.sprite(room, entity)
    local sprites = {}
    local texture = "objects/mrbonesdoor"
    
    local height = entity.height or 48
    
    for y = 0, height - 8, 8 do
        local sprite = drawableSprite.fromTexture(texture, entity)
        sprite:setPosition(entity.x, entity.y + y)
        sprite:setJustification(0, 0)
        table.insert(sprites, sprite)
    end
    
    return sprites
end

function mrBonesDoor.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, entity.height or 48)
end

return mrBonesDoor
