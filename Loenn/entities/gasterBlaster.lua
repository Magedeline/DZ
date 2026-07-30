-- Loenn plugin for DZ/GasterBlaster (chapter 15 - Citadel).
-- Placeholder visual: square skull with circle eyes and a beam guide. Swap the sprite
-- function for a drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local gasterBlaster = {}

gasterBlaster.name = "DZ/GasterBlaster"
gasterBlaster.depth = -8600

gasterBlaster.placements = {
    name = "gaster_blaster",
    data = {
        angle = 90.0,
        aimsAtPlayer = false,
        chargeTime = 1.0,
        beamDuration = 0.55,
        cooldown = 1.6,
        beamWidth = 14.0,
        beamLength = 260.0,
        headSize = 16.0,
        startDelay = 0.0,
        shots = 0,
        deadly = true,
        flag = "",
        invertFlag = false
    }
}

gasterBlaster.fieldInformation = {
    angle = {
        minimumValue = -360.0,
        maximumValue = 360.0
    },
    chargeTime = {
        minimumValue = 0.1
    },
    beamDuration = {
        minimumValue = 0.05
    },
    cooldown = {
        minimumValue = 0.05
    },
    beamWidth = {
        minimumValue = 2.0
    },
    beamLength = {
        minimumValue = 16.0
    },
    headSize = {
        minimumValue = 8.0
    },
    startDelay = {
        minimumValue = 0.0
    },
    shots = {
        fieldType = "integer",
        minimumValue = 0
    }
}

local skullFill = {0.95, 0.95, 0.95, 0.95}
local skullLine = {0.16, 0.16, 0.2, 1.0}
local eyeColor = {0.16, 0.16, 0.2, 1.0}
local beamColor = {0.81, 0.91, 1.0, 0.35}

local function drawBlaster(x, y, size, angle, beamWidth, beamLength)
    drawing.callKeepOriginalColor(function()
        local dirX, dirY = math.cos(angle), math.sin(angle)
        local sideX, sideY = -dirY, dirX

        -- Beam guide, so the firing line is visible while placing.
        local muzzleX = x + dirX * (size / 2 + 2)
        local muzzleY = y + dirY * (size / 2 + 2)

        love.graphics.setColor(beamColor)
        love.graphics.setLineWidth(beamWidth)
        love.graphics.line(muzzleX, muzzleY, muzzleX + dirX * beamLength, muzzleY + dirY * beamLength)
        love.graphics.setLineWidth(1)

        love.graphics.setColor(skullFill)
        love.graphics.rectangle("fill", x - size / 2, y - size / 2, size, size)
        love.graphics.setColor(skullLine)
        love.graphics.rectangle("line", x - size / 2, y - size / 2, size, size)

        -- Eyes sit perpendicular to the firing direction.
        love.graphics.setColor(eyeColor)
        love.graphics.circle("fill", x + sideX * size * 0.22 - dirX * size * 0.1,
            y + sideY * size * 0.22 - dirY * size * 0.1, size * 0.14)
        love.graphics.circle("fill", x - sideX * size * 0.22 - dirX * size * 0.1,
            y - sideY * size * 0.22 - dirY * size * 0.1, size * 0.14)

        -- Muzzle block on the firing face.
        love.graphics.setColor(skullFill)
        love.graphics.rectangle("fill", x + dirX * size * 0.42 - size * 0.2,
            y + dirY * size * 0.42 - size * 0.2, size * 0.4, size * 0.4)
        love.graphics.setColor(skullLine)
        love.graphics.rectangle("line", x + dirX * size * 0.42 - size * 0.2,
            y + dirY * size * 0.42 - size * 0.2, size * 0.4, size * 0.4)
    end)
end

function gasterBlaster.sprite(room, entity)
    return drawableFunction.fromFunction(drawBlaster,
        entity.x or 0, entity.y or 0,
        entity.headSize or 16,
        math.rad(entity.angle or 90),
        entity.beamWidth or 14,
        entity.beamLength or 260)
end

function gasterBlaster.selection(room, entity)
    local size = entity.headSize or 16

    return utils.rectangle((entity.x or 0) - size / 2, (entity.y or 0) - size / 2, size, size)
end

return gasterBlaster
