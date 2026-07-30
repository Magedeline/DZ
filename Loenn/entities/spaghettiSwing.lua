-- Loenn plugin for DZ/SpaghettiSwing (chapter 11 - Snowding City).
-- Placeholder visual: circle anchor, noodle strand, square handle. Swap the sprite function
-- for a drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local spaghettiSwing = {}

spaghettiSwing.name = "DZ/SpaghettiSwing"
spaghettiSwing.depth = -50

spaghettiSwing.placements = {
    name = "spaghetti_swing",
    data = {
        length = 48.0,
        startAngle = -30.0,
        maxAngle = 65.0,
        gravity = 900.0,
        damping = 0.9992,
        pumpStrength = 2.2,
        launchBoost = 1.15,
        grabRadius = 10.0,
        holdTimeLimit = 0.0,
        autoGrab = true
    }
}

spaghettiSwing.fieldInformation = {
    length = {
        minimumValue = 8.0
    },
    maxAngle = {
        minimumValue = 5.0,
        maximumValue = 150.0
    },
    gravity = {
        minimumValue = 0.0
    },
    damping = {
        minimumValue = 0.9,
        maximumValue = 1.0
    },
    grabRadius = {
        minimumValue = 1.0
    },
    holdTimeLimit = {
        minimumValue = 0.0
    }
}

local anchorFill = {0.54, 0.42, 0.29, 1.0}
local anchorLine = {0.29, 0.22, 0.13, 1.0}
local strandColor = {0.95, 0.85, 0.55, 1.0}
local handleFill = {0.85, 0.31, 0.24, 0.95}
local handleLine = {0.43, 0.12, 0.09, 1.0}

-- Handle position for the resting angle, so the editor shows where the swing hangs.
local function handleOffset(entity)
    local length = entity.length or 48
    local angle = math.rad(entity.startAngle or -30)

    return math.sin(angle) * length, math.cos(angle) * length
end

local function drawSwing(x, y, handleX, handleY, grabRadius)
    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(strandColor)
        love.graphics.line(x, y, handleX, handleY)

        love.graphics.setColor(anchorFill)
        love.graphics.circle("fill", x, y, 4)
        love.graphics.setColor(anchorLine)
        love.graphics.circle("line", x, y, 4)

        love.graphics.setColor(handleFill)
        love.graphics.rectangle("fill", handleX - 5, handleY - 5, 10, 10)
        love.graphics.setColor(handleLine)
        love.graphics.rectangle("line", handleX - 5, handleY - 5, 10, 10)
        love.graphics.circle("line", handleX, handleY, grabRadius)
    end)
end

function spaghettiSwing.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local offsetX, offsetY = handleOffset(entity)

    return drawableFunction.fromFunction(drawSwing, x, y, x + offsetX, y + offsetY, entity.grabRadius or 10)
end

function spaghettiSwing.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local offsetX, offsetY = handleOffset(entity)

    -- Cover both the anchor and the hanging handle.
    local left = math.min(x - 6, x + offsetX - 6)
    local top = math.min(y - 6, y + offsetY - 6)
    local right = math.max(x + 6, x + offsetX + 6)
    local bottom = math.max(y + 6, y + offsetY + 6)

    return utils.rectangle(left, top, right - left, bottom - top)
end

return spaghettiSwing
