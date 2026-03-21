local drawableRectangle = require("structs.drawable_rectangle")

local infernoMirror = {}

infernoMirror.name = "MaggyHelper/InfernoMirror"
infernoMirror.depth = 9500
infernoMirror.minimumSize = {16, 16}

infernoMirror.placements = {
    {
        name = "normal",
        data = {
            width = 32,
            height = 32,
            reflectX = 0.0,
            reflectY = 0.0
        }
    }
}

infernoMirror.fieldInformation = {
    reflectX = { fieldType = "number" },
    reflectY = { fieldType = "number" }
}

infernoMirror.fieldOrder = {
    "x", "y", "width", "height", "reflectX", "reflectY"
}

function infernoMirror.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 32
    local height = entity.height or 32

    -- Dark mirror surface
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x + 3, y + 3, width - 6, height - 6, {0.1, 0.02, 0.02, 1}))
    -- Frame border
    table.insert(sprites, drawableRectangle.fromRectangle("line", x, y, width, height, {0.5, 0.3, 0.3, 1}))

    return sprites
end

return infernoMirror
