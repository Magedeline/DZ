local drawableRectangle = require("structs.drawable_rectangle")

local infernoShockwave = {}

infernoShockwave.name = "MaggyHelper/InfernoBigEyeballShockwave"
infernoShockwave.depth = -1000000

infernoShockwave.placements = {
    {
        name = "normal",
        data = {}
    }
}

function infernoShockwave.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x or 0, entity.y or 0

    -- Visual representation of the shockwave hitbox
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x - 30, y - 100, 48, 200, {0.8, 0.2, 0.2, 0.3}))
    table.insert(sprites, drawableRectangle.fromRectangle("line", x - 30, y - 100, 48, 200, {1.0, 0.3, 0.3, 0.8}))

    return sprites
end

function infernoShockwave.selection(room, entity)
    local utils = require("utils")
    return utils.rectangle(entity.x - 30, entity.y - 100, 48, 200)
end

return infernoShockwave
