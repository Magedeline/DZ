local drawableRectangle = require("structs.drawable_rectangle")

local templeMirror = {}

templeMirror.name = "MaggyHelper/TesseractMirror"
templeMirror.depth = 9500
templeMirror.placements = {
    {
        name = "tesseract_mirror",
        data = {
            width = 16,
            height = 16,
            reflectX = 0.0,
            reflectY = 0.0
        }
    }
}

function templeMirror.sprite(room, entity)
    -- Simple rectangle as a placeholder; replace with a sprite if you have one
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 16
    local height = entity.height or 16

    table.insert(sprites, drawableRectangle.fromRectangle("line", x, y, width, height, {0, 0, 0, 1}))

    return sprites
end

return templeMirror
