local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local poppyBrosJr = {}

poppyBrosJr.name = "MaggyHelper/PoppyBrosJr"
poppyBrosJr.depth = -100

poppyBrosJr.placements = {
    {
        name = "normal",
        data = {
            x = 0,
            y = 0
        }
    }
}

function poppyBrosJr.sprite(room, entity)
    return drawableSprite.fromTexture("characters/poppybrosjr/idle00", entity)
end

function poppyBrosJr.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    -- Hitbox is 24x24 with offset -12, -24 (KirbyMidBoss hitbox)
    return utils.rectangle(x - 12, y - 24, 24, 24)
end

-- Poppy Bros Jr drops Bomb ability
function poppyBrosJr.tooltip(room, entity)
    return "Poppy Bros Jr - Bomb-throwing mid-boss\nDrops: Bomb Ability"
end

return poppyBrosJr
