-- Loenn plugin for DZ/NukeDrop (chapter 16 - MyWorld).
-- Placeholder visual: red marker square with the blast radius outlined. Swap the sprite
-- function for a drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local nukeDrop = {}

nukeDrop.name = "DZ/NukeDrop"
nukeDrop.depth = -10000

nukeDrop.placements = {
    name = "nuke_drop",
    data = {
        mode = "Interval",
        interval = 5.0,
        startDelay = 1.5,
        fallSpeed = 220.0,
        fuseTime = 0.9,
        blastRadius = 84.0,
        spawnHeight = 64.0,
        shakeTime = 0.6,
        targetsPlayer = true,
        deadly = true,
        damage = 3,
        bigBlast = false,
        flag = ""
    }
}

nukeDrop.fieldInformation = {
    mode = {
        options = {"Once", "Interval", "Flag"},
        editable = false
    },
    interval = {
        minimumValue = 0.5
    },
    startDelay = {
        minimumValue = 0.0
    },
    fallSpeed = {
        minimumValue = 20.0
    },
    fuseTime = {
        minimumValue = 0.1
    },
    blastRadius = {
        minimumValue = 8.0
    },
    spawnHeight = {
        minimumValue = 0.0
    },
    shakeTime = {
        minimumValue = 0.0
    },
    damage = {
        fieldType = "integer",
        minimumValue = 1
    }
}

local fillColor = {0.84, 0.31, 0.31, 0.9}
local lineColor = {1.0, 0.88, 0.88, 1.0}
local blastColor = {1.0, 0.35, 0.23, 0.3}

local markerSize = 12

local function drawNuke(x, y, blastRadius, spawnHeight)
    drawing.callKeepOriginalColor(function()
        -- Blast footprint and the drop line the bomb falls down.
        love.graphics.setColor(blastColor)
        love.graphics.circle("line", x, y, blastRadius)
        love.graphics.line(x, y, x, y - spawnHeight)

        love.graphics.setColor(fillColor)
        love.graphics.rectangle("fill", x - markerSize / 2, y - markerSize / 2, markerSize, markerSize)
        love.graphics.setColor(lineColor)
        love.graphics.rectangle("line", x - markerSize / 2, y - markerSize / 2, markerSize, markerSize)
    end)
end

function nukeDrop.sprite(room, entity)
    return drawableFunction.fromFunction(drawNuke, entity.x or 0, entity.y or 0,
        entity.blastRadius or 84, entity.spawnHeight or 64)
end

function nukeDrop.selection(room, entity)
    return utils.rectangle((entity.x or 0) - markerSize / 2, (entity.y or 0) - markerSize / 2,
        markerSize, markerSize)
end

return nukeDrop
