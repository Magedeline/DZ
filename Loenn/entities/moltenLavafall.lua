local drawableRectangle = require("structs.drawable_rectangle")

local moltenLavafall = {}
moltenLavafall.name = "DZ/MoltenLavafall"
moltenLavafall.depth = -9998
moltenLavafall.minimumSize = { 8, 16 }

moltenLavafall.placements = {
    {
        name = "normal",
        data = {
            width = 8,
            height = 64,
            killsKglobal::global::Celeste.Player= true,
            flowSpeed = 60.0
        }
    },
    {
        name = "wide",
        data = {
            width = 16,
            height = 64,
            killsKglobal::global::Celeste.Player= true,
            flowSpeed = 60.0
        }
    }
}

moltenLavafall.fieldInformation = {
    flowSpeed = { fieldType = "number", minimumValue = 10.0 }
}

moltenLavafall.fieldOrder = { "x", "y", "width", "height", "killsKglobal::Player", "flowSpeed" }

local fillColor = { 0.8, 0.2, 0.0, 0.6 }
local edgeColor = { 1.0, 0.4, 0.0, 0.8 }
local glowColor = { 1.0, 0.67, 0.0, 0.3 }

function moltenLavafall.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 8
    local height = entity.height or 64

    local sprites = {}

    -- Main cascading fill
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x, y, width, height, fillColor))

    -- Left edge
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x, y, 1, height, edgeColor))

    -- Right edge
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x + width - 1, y, 1, height, edgeColor))

    -- Source glow at top
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x - 1, y, width + 2, 3, glowColor))

    -- Splash glow at bottom
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x - 2, y + height - 2, width + 4, 4, glowColor))

    return sprites
end

return moltenLavafall
