-- Loenn plugin for DZ/RudeBusterAxe (chapter 14 - Cyber Nexus).
-- Placeholder visual: purple circle pickup with an axe-head diamond. Swap the sprite
-- function for a drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local rudeBusterAxe = {}

rudeBusterAxe.name = "DZ/RudeBusterAxe"
rudeBusterAxe.depth = -100

rudeBusterAxe.placements = {
    name = "rude_buster_axe",
    data = {
        charges = 3,
        respawnDelay = 3.0,
        oneUse = false,
        flag = "",
        radius = 8.0,
        throwSpeed = 280.0,
        range = 120.0,
        damage = 2,
        boomerang = true,
        refundOnCatch = false,
        spinSpeed = 14.0,
        axeSize = 10.0,
        useButton = "Grab"
    }
}

rudeBusterAxe.fieldInformation = {
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
    throwSpeed = {
        minimumValue = 40.0
    },
    range = {
        minimumValue = 16.0
    },
    damage = {
        fieldType = "integer",
        minimumValue = 1
    },
    axeSize = {
        minimumValue = 4.0
    },
    useButton = {
        options = {"Grab", "Dash", "Jump", "Talk"},
        editable = false
    }
}

local fillColor = {0.56, 0.31, 0.84, 0.9}
local lineColor = {0.94, 0.85, 1.0, 1.0}
local bladeColor = {1.0, 0.88, 0.4, 1.0}

local function drawAxePickup(x, y, radius, axeSize)
    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(fillColor)
        love.graphics.circle("fill", x, y, radius)
        love.graphics.setColor(lineColor)
        love.graphics.circle("line", x, y, radius)

        -- Axe head: a square stood on its corner, matching the spinning projectile.
        local half = axeSize / 2
        love.graphics.setColor(bladeColor)
        love.graphics.polygon("line", x, y - half, x + half, y, x, y + half, x - half, y)
    end)
end

function rudeBusterAxe.sprite(room, entity)
    return drawableFunction.fromFunction(drawAxePickup, entity.x or 0, entity.y or 0,
        entity.radius or 8, entity.axeSize or 10)
end

function rudeBusterAxe.selection(room, entity)
    local radius = entity.radius or 8

    return utils.rectangle((entity.x or 0) - radius, (entity.y or 0) - radius, radius * 2, radius * 2)
end

return rudeBusterAxe
