-- Loenn plugin for DZ/SparkBall (chapter 13 - Hotcliff).
-- Placeholder visual: bright circle with an orbit guide. Swap the sprite function for a
-- drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local sparkBall = {}

sparkBall.name = "DZ/SparkBall"
sparkBall.depth = -8500
sparkBall.nodeLimits = {0, 4}
sparkBall.nodeLineRenderType = "line"

sparkBall.placements = {
    name = "spark_ball",
    data = {
        mode = "Orbit",
        radius = 6.0,
        orbitRadius = 32.0,
        speed = 120.0,
        clockwise = true,
        startAngle = 0.0,
        chaseAcceleration = 180.0,
        chaseMaxSpeed = 90.0,
        sparkCount = 4,
        deadly = true,
        pingPong = false,
        flag = "",
        invertFlag = false
    }
}

sparkBall.fieldInformation = {
    mode = {
        options = {"Orbit", "Patrol", "Chase", "Static"},
        editable = false
    },
    radius = {
        minimumValue = 2.0
    },
    orbitRadius = {
        minimumValue = 4.0
    },
    speed = {
        minimumValue = 0.0
    },
    chaseAcceleration = {
        minimumValue = 1.0
    },
    chaseMaxSpeed = {
        minimumValue = 1.0
    },
    sparkCount = {
        fieldType = "integer",
        minimumValue = 0
    }
}

local fillColor = {1.0, 0.96, 0.69, 0.95}
local lineColor = {0.56, 0.85, 1.0, 1.0}
local guideColor = {0.44, 0.73, 1.0, 0.4}

local function drawSpark(x, y, radius, orbitRadius, showOrbit)
    drawing.callKeepOriginalColor(function()
        if showOrbit then
            love.graphics.setColor(guideColor)
            love.graphics.circle("line", x, y, orbitRadius)
        end

        love.graphics.setColor(fillColor)
        love.graphics.circle("fill", x, y, radius)
        love.graphics.setColor(lineColor)
        love.graphics.circle("line", x, y, radius)
        love.graphics.circle("line", x, y, radius * 2)
    end)
end

function sparkBall.sprite(room, entity)
    local mode = entity.mode or "Orbit"

    return drawableFunction.fromFunction(drawSpark,
        entity.x or 0, entity.y or 0,
        entity.radius or 6,
        entity.orbitRadius or 32,
        mode == "Orbit")
end

function sparkBall.selection(room, entity)
    local radius = entity.radius or 6
    local x, y = entity.x or 0, entity.y or 0
    local nodeRects = {}

    for _, node in ipairs(entity.nodes or {}) do
        table.insert(nodeRects, utils.rectangle(node.x - radius, node.y - radius, radius * 2, radius * 2))
    end

    return utils.rectangle(x - radius, y - radius, radius * 2, radius * 2), nodeRects
end

return sparkBall
