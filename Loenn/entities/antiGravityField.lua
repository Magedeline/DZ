-- Loenn plugin for DZ/AntiGravityField (chapter 15 - Citadel).
-- Placeholder visual: hollow purple rectangle with rising chevrons. Swap the sprite
-- function for a drawableSprite once real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local antiGravityField = {}

antiGravityField.name = "DZ/AntiGravityField"
antiGravityField.depth = 1000
antiGravityField.minimumSize = {8, 8}

antiGravityField.placements = {
    name = "anti_gravity_field",
    data = {
        width = 32,
        height = 32,
        riseAcceleration = 340.0,
        maxRiseSpeed = 110.0,
        showBorder = true,
        affectsEnemies = false,
        flag = "",
        invertFlag = false
    }
}

antiGravityField.fieldInformation = {
    riseAcceleration = {
        minimumValue = 1.0
    },
    maxRiseSpeed = {
        minimumValue = 1.0
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

local fillColor = {0.42, 0.25, 0.63, 0.18}
local lineColor = {0.85, 0.72, 1.0, 0.7}
local arrowColor = {0.85, 0.72, 1.0, 0.45}

function antiGravityField.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 32, entity.height or 32
    local sprites = {}

    pushRect(sprites, x, y, width, height, fillColor, lineColor)

    -- Chevrons showing the upward push.
    for arrowY = y + height - 4, y + 4, -12 do
        for arrowX = x + 4, x + width - 6, 12 do
            pushRect(sprites, arrowX, arrowY - 1, 5, 1, arrowColor, arrowColor)
            pushRect(sprites, arrowX + 2, arrowY - 3, 1, 3, arrowColor, arrowColor)
        end
    end

    return sprites
end

function antiGravityField.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, entity.width or 32, entity.height or 32)
end

return antiGravityField
