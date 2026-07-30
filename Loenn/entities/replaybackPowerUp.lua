-- Loenn plugin for DZ/ReplaybackPowerUp (chapter 16 - MyWorld).
-- Placeholder visual: blue circle pickup with a rewind arrow. Swap the sprite function for
-- a drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local replaybackPowerUp = {}

replaybackPowerUp.name = "DZ/ReplaybackPowerUp"
replaybackPowerUp.depth = -100

replaybackPowerUp.placements = {
    name = "replayback_power_up",
    data = {
        rewindTime = 1.5,
        charges = 1,
        respawnDelay = 4.0,
        oneUse = false,
        radius = 8.0,
        useButton = "Grab"
    }
}

replaybackPowerUp.fieldInformation = {
    rewindTime = {
        minimumValue = 0.2
    },
    charges = {
        fieldType = "integer",
        minimumValue = 1
    },
    respawnDelay = {
        minimumValue = 0.0
    },
    radius = {
        minimumValue = 4.0
    },
    useButton = {
        options = {"Grab", "Dash", "Jump", "Talk"},
        editable = false
    }
}

local fillColor = {0.31, 0.5, 0.84, 0.9}
local lineColor = {0.85, 0.91, 1.0, 1.0}

local function drawRewind(x, y, radius)
    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(fillColor)
        love.graphics.circle("fill", x, y, radius)
        love.graphics.setColor(lineColor)
        love.graphics.circle("line", x, y, radius)

        -- Backwards arrow: this pickup rewinds the player.
        love.graphics.line(x + 3, y, x - 3, y)
        love.graphics.line(x - 3, y, x, y - 3)
        love.graphics.line(x - 3, y, x, y + 3)
    end)
end

function replaybackPowerUp.sprite(room, entity)
    return drawableFunction.fromFunction(drawRewind, entity.x or 0, entity.y or 0, entity.radius or 8)
end

function replaybackPowerUp.selection(room, entity)
    local radius = entity.radius or 8

    return utils.rectangle((entity.x or 0) - radius, (entity.y or 0) - radius, radius * 2, radius * 2)
end

return replaybackPowerUp
