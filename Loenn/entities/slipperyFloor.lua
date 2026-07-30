-- Loenn plugin for DZ/SlipperyFloor (chapter 11 - Snowding City).
-- Placeholder visual: pale blue rectangle with a highlight band. Swap the sprite function
-- for a drawableSprite once real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local slipperyFloor = {}

slipperyFloor.name = "DZ/SlipperyFloor"
slipperyFloor.depth = -9000
slipperyFloor.minimumSize = {8, 8}

slipperyFloor.placements = {
    name = "slippery_floor",
    data = {
        width = 24,
        height = 8,
        slipperiness = 0.9,
        slideSpeed = 0.0,
        slideDirection = "None",
        safeGround = true,
        affectsEnemies = false
    }
}

slipperyFloor.fieldInformation = {
    slipperiness = {
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    slideSpeed = {
        minimumValue = 0.0
    },
    slideDirection = {
        options = {"None", "Left", "Right"},
        editable = false
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

local fillColor = {0.62, 0.85, 0.91, 0.9}
local lineColor = {0.36, 0.58, 0.68, 1.0}
local shineColor = {0.89, 0.97, 1.0, 1.0}

function slipperyFloor.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 8
    local sprites = {}

    pushRect(sprites, x, y, width, height, fillColor, lineColor)
    pushRect(sprites, x + 1, y + 1, math.max(1, width - 2), 1, shineColor, shineColor)

    return sprites
end

function slipperyFloor.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, entity.width or 24, entity.height or 8)
end

return slipperyFloor
