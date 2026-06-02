local drawableRectangle = require("structs.drawable_rectangle")

local infernoMirrorVortex = {}

infernoMirrorVortex.name = "MaggyHelper/InfernoMirrorVortex"
infernoMirrorVortex.depth = -1000000
infernoMirrorVortex.minimumSize = {16, 16}

infernoMirrorVortex.placements = {
    {
        name = "normal",
        data = {
            width = 64,
            height = 64,
            colorFrom = "ff3333",
            colorTo = "330000",
            distortion = 0.25,
            flag = ""
        }
    }
}

infernoMirrorVortex.fieldInformation = {
    colorFrom = { fieldType = "color" },
    colorTo = { fieldType = "color" },
    distortion = { fieldType = "number", minimumValue = 0.0, maximumValue = 1.0 },
    flag = { fieldType = "string" }
}

infernoMirrorVortex.fieldOrder = {
    "x", "y", "width", "height", "colorFrom", "colorTo", "distortion", "flag"
}

function infernoMirrorVortex.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 64
    local height = entity.height or 64

    -- Vortex center fill
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x - width / 2, y - height / 2, width, height, {0.6, 0.1, 0.1, 0.7}))
    -- Vortex border
    table.insert(sprites, drawableRectangle.fromRectangle("line", x - width / 2, y - height / 2, width, height, {1.0, 0.2, 0.2, 1}))

    return sprites
end

function infernoMirrorVortex.selection(room, entity)
    local utils = require("utils")
    local width = entity.width or 64
    local height = entity.height or 64
    return utils.rectangle(entity.x - width / 2, entity.y - height / 2, width, height)
end

return infernoMirrorVortex
