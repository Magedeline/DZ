local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local invincibilityCandy = {}

invincibilityCandy.name = "DZ/InvincibilityCandy"
invincibilityCandy.depth = -50

invincibilityCandy.placements = {
    {
        name = "main",
        data = {
            x = 0,
            y = 0
        }
    }
}

function invincibilityCandy.sprite(room, entity)
    return drawableSprite.fromTexture("items/food/invincibilitycandy/idle00", entity)
end

function invincibilityCandy.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    return utils.rectangle(x - 6, y - 12, 12, 12)
end

-- Give invincibility candy a golden glow
function invincibilityCandy.color(room, entity)
    return {1.0, 0.9, 0.5, 1.0}  -- Gold tint
end

return invincibilityCandy
