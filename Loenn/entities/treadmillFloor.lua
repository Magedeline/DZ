-- Loenn plugin for DZ/TreadmillFloor (chapter 13 - Hotcliff).
-- Placeholder visual: hot orange rectangle with direction chevrons. Swap the sprite
-- function for a drawableSprite once real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local treadmillFloor = {}

treadmillFloor.name = "DZ/TreadmillFloor"
treadmillFloor.depth = -9000
treadmillFloor.minimumSize = {8, 4}

treadmillFloor.placements = {
    name = "treadmill_floor",
    data = {
        width = 24,
        height = 8,
        speed = 70.0,
        moveLeft = false,
        reverseInterval = 0.0,
        affectsEnemies = false,
        flag = "",
        invertFlag = false
    }
}

treadmillFloor.fieldInformation = {
    speed = {
        minimumValue = 0.0
    },
    reverseInterval = {
        minimumValue = 0.0
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

local fillColor = {0.55, 0.23, 0.12, 0.95}
local lineColor = {0.24, 0.08, 0.03, 1.0}
local chevronColor = {1.0, 0.62, 0.29, 1.0}

function treadmillFloor.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 8
    local sprites = {}

    pushRect(sprites, x, y, width, height, fillColor, lineColor)

    -- Static chevrons pointing the way the belt drags.
    local step = entity.moveLeft and -1 or 1
    for chevronX = x + 2, x + width - 6, 8 do
        pushRect(sprites, chevronX, y + 1, 4, 1, chevronColor, chevronColor)
        pushRect(sprites, chevronX + (step > 0 and 3 or 0), y + 1, 1, math.max(1, height - 2),
            chevronColor, chevronColor)
    end

    return sprites
end

function treadmillFloor.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, entity.width or 24, entity.height or 8)
end

return treadmillFloor
