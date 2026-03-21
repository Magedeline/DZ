local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local luckyBlock = {}

luckyBlock.name = "MaggyHelper/LuckyBlock"
luckyBlock.depth = -9000
luckyBlock.minimumSize = {8, 8}
luckyBlock.placements = {
    {
        name = "luckyblock",
        data = {
            width = 16,
            height = 16,
            maxHits = 3,
            rewardType = "coin"
        }
    },
    {
        name = "luckyblock_single_use",
        data = {
            width = 16,
            height = 16,
            maxHits = 1,
            rewardType = "coin"
        }
    }
}

luckyBlock.fieldInformation = {
    maxHits = {
        fieldType = "integer",
        description = "Number of times the block can be hit before becoming inactive"
    },
    rewardType = {
        fieldType = "string",
        options = {"coin", "power", "star"},
        editable = false,
        description = "Type of reward that spawns when hit"
    }
}

luckyBlock.fieldOrder = {
    "x", "y", "width", "height", "maxHits", "rewardType"
}

function luckyBlock.sprite(room, entity)
    local sprites = {}
    local texture = "objects/luckyblock"
    
    local width = entity.width or 16
    local height = entity.height or 16
    
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

function luckyBlock.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 16)
end

return luckyBlock
