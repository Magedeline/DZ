-- Loenn plugin for DZ/BlueBone (chapter 11 - Snowding City).
-- Placeholder visual: blue capsule (rectangle shaft, circle knobs). Swap the sprite
-- function for a drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local blueBone = {}

blueBone.name = "DZ/BlueBone"
blueBone.depth = -8500
blueBone.nodeLimits = {1, 1}
blueBone.nodeLineRenderType = "line"

blueBone.placements = {
    name = "blue_bone",
    data = {
        boneLength = 48.0,
        boneThickness = 8.0,
        horizontal = false,
        speed = 90.0,
        moveThreshold = 12.0,
        pauseTime = 0.0,
        loop = true,
        flag = "",
        invertFlag = false
    }
}

blueBone.fieldInformation = {
    boneLength = {
        minimumValue = 8.0
    },
    boneThickness = {
        minimumValue = 4.0
    },
    speed = {
        minimumValue = 0.0
    },
    moveThreshold = {
        minimumValue = 0.0
    },
    pauseTime = {
        minimumValue = 0.0
    }
}

local fillColor = {0.23, 0.5, 0.84, 0.9}
local lineColor = {0.75, 0.89, 1.0, 1.0}

-- Matches the C# hitbox: centred on the entity, long axis set by "horizontal".
local function boneRect(entity)
    local length = entity.boneLength or 48
    local thickness = entity.boneThickness or 8

    if entity.horizontal then
        return length, thickness
    end

    return thickness, length
end

local function drawBone(x, y, width, height, thickness)
    drawing.callKeepOriginalColor(function()
        local left, top = x - width / 2, y - height / 2
        local knob = thickness * 0.75

        love.graphics.setColor(fillColor)
        love.graphics.rectangle("fill", left, top, width, height)

        if width > height then
            love.graphics.circle("fill", left, y, knob)
            love.graphics.circle("fill", left + width, y, knob)
        else
            love.graphics.circle("fill", x, top, knob)
            love.graphics.circle("fill", x, top + height, knob)
        end

        love.graphics.setColor(lineColor)
        love.graphics.rectangle("line", left, top, width, height)

        if width > height then
            love.graphics.circle("line", left, y, knob)
            love.graphics.circle("line", left + width, y, knob)
        else
            love.graphics.circle("line", x, top, knob)
            love.graphics.circle("line", x, top + height, knob)
        end
    end)
end

function blueBone.sprite(room, entity)
    local width, height = boneRect(entity)
    local sprites = {
        drawableFunction.fromFunction(drawBone, entity.x or 0, entity.y or 0, width, height,
            entity.boneThickness or 8)
    }

    -- Ghost of the bone at its end point, so the sweep is readable.
    for _, node in ipairs(entity.nodes or {}) do
        table.insert(sprites, drawableFunction.fromFunction(drawBone, node.x, node.y, width, height,
            entity.boneThickness or 8))
    end

    return sprites
end

function blueBone.selection(room, entity)
    local width, height = boneRect(entity)
    local x, y = entity.x or 0, entity.y or 0
    local main = utils.rectangle(x - width / 2, y - height / 2, width, height)
    local nodeRects = {}

    for _, node in ipairs(entity.nodes or {}) do
        table.insert(nodeRects, utils.rectangle(node.x - width / 2, node.y - height / 2, width, height))
    end

    return main, nodeRects
end

return blueBone
