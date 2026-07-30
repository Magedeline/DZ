-- Loenn plugin for DZ/AntiGravityPowerUp (chapter 15 - Citadel).
-- Placeholder visual: purple circle pickup with an up arrow. Swap the sprite function for a
-- drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local antiGravityPowerUp = {}

antiGravityPowerUp.name = "DZ/AntiGravityPowerUp"
antiGravityPowerUp.depth = -100

antiGravityPowerUp.placements = {
    name = "anti_gravity_power_up",
    data = {
        duration = 4.0,
        riseAcceleration = 340.0,
        maxRiseSpeed = 110.0,
        respawnDelay = 3.0,
        oneUse = false,
        radius = 8.0,
        flag = ""
    }
}

antiGravityPowerUp.fieldInformation = {
    duration = {
        minimumValue = 0.2
    },
    riseAcceleration = {
        minimumValue = 1.0
    },
    maxRiseSpeed = {
        minimumValue = 1.0
    },
    respawnDelay = {
        minimumValue = 0.0
    },
    radius = {
        minimumValue = 4.0
    }
}

local fillColor = {0.42, 0.25, 0.63, 0.9}
local lineColor = {0.85, 0.72, 1.0, 1.0}

local function drawPowerUp(x, y, radius)
    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(fillColor)
        love.graphics.circle("fill", x, y, radius)
        love.graphics.setColor(lineColor)
        love.graphics.circle("line", x, y, radius)

        -- Up arrow: this pickup lifts the player.
        love.graphics.line(x, y + radius * 0.5, x, y - radius * 0.5)
        love.graphics.line(x - 2.5, y - radius * 0.5 + 2.5, x, y - radius * 0.5)
        love.graphics.line(x + 2.5, y - radius * 0.5 + 2.5, x, y - radius * 0.5)
    end)
end

function antiGravityPowerUp.sprite(room, entity)
    return drawableFunction.fromFunction(drawPowerUp, entity.x or 0, entity.y or 0, entity.radius or 8)
end

function antiGravityPowerUp.selection(room, entity)
    local radius = entity.radius or 8

    return utils.rectangle((entity.x or 0) - radius, (entity.y or 0) - radius, radius * 2, radius * 2)
end

return antiGravityPowerUp
