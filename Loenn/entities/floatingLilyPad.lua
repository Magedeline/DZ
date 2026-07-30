-- Loenn plugin for DZ/FloatingLilyPad (chapter 12 - Wateredgefall).
-- Placeholder visual: green rectangle with leaf veins. Swap the sprite function for a
-- drawableSprite once real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local floatingLilyPad = {}

floatingLilyPad.name = "DZ/FloatingLilyPad"
floatingLilyPad.depth = -9000
floatingLilyPad.minimumSize = {8, 4}

floatingLilyPad.placements = {
    name = "floating_lily_pad",
    data = {
        width = 24,
        height = 8,
        sinkSpeed = 22.0,
        sinkDepth = 14.0,
        riseSpeed = 34.0,
        submergeDelay = 0.5,
        resurfaceDelay = 1.6,
        driftSpeed = 0.0,
        driftDistance = 0.0,
        bobAmplitude = 2.0,
        bobSpeed = 2.4,
        submerges = true
    }
}

floatingLilyPad.fieldInformation = {
    sinkSpeed = {
        minimumValue = 0.0
    },
    sinkDepth = {
        minimumValue = 1.0
    },
    riseSpeed = {
        minimumValue = 1.0
    },
    submergeDelay = {
        minimumValue = 0.0
    },
    resurfaceDelay = {
        minimumValue = 0.0
    },
    driftDistance = {
        minimumValue = 0.0
    },
    bobAmplitude = {
        minimumValue = 0.0
    },
    bobSpeed = {
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

local fillColor = {0.31, 0.62, 0.34, 0.95}
local lineColor = {0.13, 0.32, 0.17, 1.0}
local veinColor = {0.49, 0.79, 0.52, 0.7}

function floatingLilyPad.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 8
    local sprites = {}

    pushRect(sprites, x, y, width, height, fillColor, lineColor)

    for veinX = x + 3, x + width - 3, 6 do
        pushRect(sprites, veinX, y + 1, 1, math.max(1, height - 2), veinColor, veinColor)
    end

    return sprites
end

function floatingLilyPad.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, entity.width or 24, entity.height or 8)
end

return floatingLilyPad
