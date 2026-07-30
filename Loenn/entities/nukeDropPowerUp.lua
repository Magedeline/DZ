-- Loenn plugin for DZ/NukeDropPowerUp (chapter 16 - MyWorld).
-- Placeholder visual: red circle pickup with a down arrow. Swap the sprite function for a
-- drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local nukeDropPowerUp = {}

nukeDropPowerUp.name = "DZ/NukeDropPowerUp"
nukeDropPowerUp.depth = -100

nukeDropPowerUp.placements = {
    name = "nuke_drop_power_up",
    data = {
        charges = 2,
        respawnDelay = 4.0,
        oneUse = false,
        radius = 8.0,
        dropHeight = 72.0,
        fallSpeed = 260.0,
        fuseTime = 0.4,
        blastRadius = 72.0,
        damage = 4,
        bigBlast = false,
        useButton = "Grab"
    }
}

nukeDropPowerUp.fieldInformation = {
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
    dropHeight = {
        minimumValue = 8.0
    },
    fallSpeed = {
        minimumValue = 20.0
    },
    fuseTime = {
        minimumValue = 0.05
    },
    blastRadius = {
        minimumValue = 8.0
    },
    damage = {
        fieldType = "integer",
        minimumValue = 1
    },
    useButton = {
        options = {"Grab", "Dash", "Jump", "Talk"},
        editable = false
    }
}

local fillColor = {0.84, 0.31, 0.31, 0.9}
local lineColor = {1.0, 0.88, 0.88, 1.0}
local blastColor = {1.0, 0.35, 0.23, 0.25}

local function drawNukePickup(x, y, radius, blastRadius)
    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(blastColor)
        love.graphics.circle("line", x, y, blastRadius)

        love.graphics.setColor(fillColor)
        love.graphics.circle("fill", x, y, radius)
        love.graphics.setColor(lineColor)
        love.graphics.circle("line", x, y, radius)

        -- Down arrow: the nuke is called down onto the player's position.
        love.graphics.line(x, y - radius * 0.5, x, y + radius * 0.5)
        love.graphics.line(x - 2.5, y + radius * 0.5 - 2.5, x, y + radius * 0.5)
        love.graphics.line(x + 2.5, y + radius * 0.5 - 2.5, x, y + radius * 0.5)
    end)
end

function nukeDropPowerUp.sprite(room, entity)
    return drawableFunction.fromFunction(drawNukePickup, entity.x or 0, entity.y or 0,
        entity.radius or 8, entity.blastRadius or 72)
end

function nukeDropPowerUp.selection(room, entity)
    local radius = entity.radius or 8

    return utils.rectangle((entity.x or 0) - radius, (entity.y or 0) - radius, radius * 2, radius * 2)
end

return nukeDropPowerUp
