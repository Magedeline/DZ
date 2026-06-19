local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local maximTomato = {}

maximTomato.name = "DZ/MaximTomato"
maximTomato.depth = -50

maximTomato.placements = {
    {
        name = "main",
        data = {
            x = 0,
            y = 0
        }
    }
}

function maximTomato.sprite(room, entity)
    return drawableSprite.fromTexture("items/food/maximtomato/idle00", entity)
end

function maximTomato.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    return utils.rectangle(x - 8, y - 16, 16, 16)
end

-- Maxim Tomatoes are special - give them a slight color tint to distinguish
function maximTomato.color(room, entity)
    return {1.0, 0.9, 0.9, 1.0}  -- Slight red tint
end

return maximTomato
