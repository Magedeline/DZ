local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local bonkers = {}

bonkers.name = "DZ/Bonkers"
bonkers.depth = -100

bonkers.placements = {
    {
        name = "main",
        data = {
            x = 0,
            y = 0
        }
    }
}

function bonkers.sprite(room, entity)
    -- Bonkers uses "boss" as the idle frame
    return drawableSprite.fromTexture("characters/bonkers/boss00", entity)
end

function bonkers.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    -- Hitbox is 24x24 with offset -12, -24 (KirbyMidBoss hitbox)
    return utils.rectangle(x - 12, y - 24, 24, 24)
end

-- Bonkers drops Hammer ability
function bonkers.tooltip(room, entity)
    return "Bonkers - Hammer-wielding mid-boss\nDrops: Hammer Ability"
end

return bonkers
