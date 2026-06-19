local drawableRectangle = require("structs.drawable_rectangle")

local moltenLava = {}
moltenLava.name = "DZ/MoltenLava"
moltenLava.depth = -9999
moltenLava.minimumSize = { 16, 16 }

moltenLava.placements = {
    {
        name = "main",
        data = {
            width = 32,
            height = 32,
            hasTop = true,
            hasBottom = false,
            killsKglobal::global::Celeste.Player= true,
            damageGracePeriod = 0.0
        }
    },
    {
        name = "deep",
        data = {
            width = 64,
            height = 64,
            hasTop = true,
            hasBottom = true,
            killsKglobal::global::Celeste.Player= true,
            damageGracePeriod = 0.0
        }
    }
}

moltenLava.fieldInformation = {
    damageGracePeriod = { fieldType = "number", minimumValue = 0.0 }
}

moltenLava.fieldOrder = { "x", "y", "width", "height", "hasTop", "hasBottom", "killsKglobal::Player", "damageGracePeriod" }

local fillColor = { 0.8, 0.2, 0.0, 0.55 }
local surfaceColor = { 1.0, 0.4, 0.0, 0.9 }

function moltenLava.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 32
    local height = entity.height or 32

    local sprites = {}

    -- Main fill
    table.insert(sprites, drawableRectangle.fromRectangle("fill", x, y, width, height, fillColor))

    -- Top surface line
    if entity.hasTop ~= false then
        table.insert(sprites, drawableRectangle.fromRectangle("fill", x, y, width, 2, surfaceColor))
    end

    -- Bottom surface line
    if entity.hasBottom then
        table.insert(sprites, drawableRectangle.fromRectangle("fill", x, y + height - 2, width, 2, surfaceColor))
    end

    return sprites
end

return moltenLava
