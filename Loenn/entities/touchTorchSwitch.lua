-- Loenn plugin for DZ/TouchTorchSwitch (chapter 10 - Ruins).
-- Placeholder visual: orange circle. Swap the sprite function for a drawableSprite once
-- real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local touchTorchSwitch = {}

touchTorchSwitch.name = "DZ/TouchTorchSwitch"
touchTorchSwitch.depth = -100

touchTorchSwitch.placements = {
    name = "touch_torch_switch",
    data = {
        group = "",
        flag = "",
        requiresDash = false,
        persistent = false,
        radius = 8.0
    }
}

touchTorchSwitch.fieldInformation = {
    radius = {
        minimumValue = 1.0
    }
}

-- Loenn ships no circle drawable, so draw to love.graphics inside a drawableFunction.
-- callKeepOriginalColor puts the editor's colour back afterwards.
local function drawCircle(x, y, radius, fill, line)
    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(fill)
        love.graphics.circle("fill", x, y, radius)
        love.graphics.setColor(line)
        love.graphics.circle("line", x, y, radius)
    end)
end

local fillColor = {1.0, 0.69, 0.31, 0.9}
local lineColor = {1.0, 0.9, 0.66, 1.0}

local function radiusOf(entity)
    return entity.radius or 8
end

function touchTorchSwitch.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local radius = radiusOf(entity)

    return drawableFunction.fromFunction(drawCircle, x, y, radius, fillColor, lineColor)
end

function touchTorchSwitch.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local radius = radiusOf(entity)

    return utils.rectangle(x - radius, y - radius, radius * 2, radius * 2)
end

return touchTorchSwitch
