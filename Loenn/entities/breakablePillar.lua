-- Loenn plugin for DZ/BreakablePillar (chapter 10 - Ruins).
-- Placeholder visual: stone-grey rectangle. Swap the sprite function for a drawableSprite
-- once real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local breakablePillar = {}

breakablePillar.name = "DZ/BreakablePillar"
breakablePillar.depth = -9000
breakablePillar.minimumSize = {8, 8}

breakablePillar.placements = {
    name = "breakable_pillar",
    data = {
        width = 16,
        height = 40,
        requiresDash = true,
        reboundOnBreak = false,
        respawnDelay = 0.0,
        debrisCount = 10,
        flag = ""
    }
}

breakablePillar.fieldInformation = {
    respawnDelay = {
        minimumValue = 0.0
    },
    debrisCount = {
        fieldType = "integer",
        minimumValue = 0
    }
}

-- drawableRectangle returns a single sprite in "fill" mode and a list of sprites in
-- "line" mode, so flatten whatever comes back before handing it to Loenn.
local function push(sprites, rectangle)
    local result = rectangle:getDrawableSprite()

    if result[1] ~= nil then
        for _, sprite in ipairs(result) do
            table.insert(sprites, sprite)
        end
    else
        table.insert(sprites, result)
    end
end

local function pushRect(sprites, x, y, width, height, fill, line)
    push(sprites, drawableRectangle.fromRectangle("fill", x, y, width, height, fill))
    push(sprites, drawableRectangle.fromRectangle("line", x, y, width, height, line))
end

local fillColor = {0.44, 0.4, 0.34, 0.9}
local lineColor = {0.24, 0.22, 0.18, 1.0}

function breakablePillar.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 40
    local sprites = {}

    pushRect(sprites, x, y, width, height, fillColor, lineColor)

    return sprites
end

function breakablePillar.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, entity.width or 16, entity.height or 40)
end

return breakablePillar
