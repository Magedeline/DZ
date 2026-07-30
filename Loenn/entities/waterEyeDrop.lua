-- Loenn plugin for DZ/WaterEyeDrop (chapter 12 - Wateredgefall).
-- Placeholder visual: white circle eye with a dark pupil. Swap the sprite function for a
-- drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local waterEyeDrop = {}

waterEyeDrop.name = "DZ/WaterEyeDrop"
waterEyeDrop.depth = -100

waterEyeDrop.placements = {
    name = "water_eye_drop",
    data = {
        dropInterval = 2.2,
        warningTime = 0.55,
        dropSpeed = 190.0,
        dropRadius = 4.0,
        maxFallDistance = 220.0,
        eyeRadius = 7.0,
        detectWidth = 24.0,
        onlyWhenPlayerBelow = true,
        deadly = true,
        flag = "",
        invertFlag = false
    }
}

waterEyeDrop.fieldInformation = {
    dropInterval = {
        minimumValue = 0.1
    },
    warningTime = {
        minimumValue = 0.0
    },
    dropSpeed = {
        minimumValue = 10.0
    },
    dropRadius = {
        minimumValue = 1.0
    },
    maxFallDistance = {
        minimumValue = 16.0
    },
    eyeRadius = {
        minimumValue = 3.0
    },
    detectWidth = {
        minimumValue = 4.0
    }
}

local scleraFill = {0.92, 0.96, 1.0, 0.95}
local scleraLine = {0.24, 0.42, 0.52, 1.0}
local pupilFill = {0.11, 0.24, 0.32, 1.0}
local columnColor = {0.35, 0.72, 0.88, 0.25}

local function drawEye(x, y, radius, fallDistance, showColumn, detectWidth)
    drawing.callKeepOriginalColor(function()
        if showColumn then
            love.graphics.setColor(columnColor)
            love.graphics.rectangle("line", x - detectWidth / 2, y, detectWidth, fallDistance)
        end

        love.graphics.setColor(scleraFill)
        love.graphics.circle("fill", x, y, radius)
        love.graphics.setColor(scleraLine)
        love.graphics.circle("line", x, y, radius)

        -- Pupil looking straight down, the direction the drop falls.
        love.graphics.setColor(pupilFill)
        love.graphics.circle("fill", x, y + radius * 0.3, radius * 0.45)
    end)
end

function waterEyeDrop.sprite(room, entity)
    return drawableFunction.fromFunction(drawEye,
        entity.x or 0, entity.y or 0,
        entity.eyeRadius or 7,
        entity.maxFallDistance or 220,
        entity.onlyWhenPlayerBelow ~= false,
        entity.detectWidth or 24)
end

function waterEyeDrop.selection(room, entity)
    local radius = entity.eyeRadius or 7

    return utils.rectangle((entity.x or 0) - radius, (entity.y or 0) - radius, radius * 2, radius * 2)
end

return waterEyeDrop
