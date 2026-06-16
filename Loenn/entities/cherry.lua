local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local cherry = {}

cherry.name = "DZ/Cherry"
cherry.depth = -50

cherry.placements = {
    {
        name = "normal",
        data = {
            x = 0,
            y = 0
        }
    }
}

function cherry.sprite(room, entity)
    return drawableSprite.fromTexture("items/food/cherry/idle00", entity)
end

function cherry.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    return utils.rectangle(x - 6, y - 12, 12, 12)
end

return cherry
