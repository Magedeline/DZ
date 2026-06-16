local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local bugzzy = {}

bugzzy.name = "DZ/Bugzzy"
bugzzy.depth = -100

bugzzy.placements = {
    {
        name = "normal",
        data = {
            x = 0,
            y = 0
        }
    }
}

function bugzzy.sprite(room, entity)
    return drawableSprite.fromTexture("characters/bugzzy/idle00", entity)
end

function bugzzy.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    -- Hitbox is 24x24 with offset -12, -24 (KirbyMidBoss hitbox)
    return utils.rectangle(x - 12, y - 24, 24, 24)
end

-- Bugzzy drops Suplex ability
function bugzzy.tooltip(room, entity)
    return "Bugzzy - Suplex-wrestling mid-boss\nDrops: Suplex Ability"
end

return bugzzy
