-- Loenn plugin for DZ/NexusPortal (chapter 14 - Cyber Nexus).
-- Placeholder visual: cyan ring; node 1 is exit B and node 2 is exit C. Swap the sprite
-- function for a drawableSprite once real art exists; no placement data has to change.

local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local utils = require("utils")

local nexusPortal = {}

nexusPortal.name = "DZ/NexusPortal"
nexusPortal.depth = -100
nexusPortal.nodeLimits = {1, 2}
nexusPortal.nodeLineRenderType = "line"

nexusPortal.placements = {
    name = "nexus_portal",
    data = {
        exitMode = "Primary",
        flag = "",
        cooldown = 0.6,
        preserveMomentum = true,
        exitBoost = 1.0,
        radius = 12.0,
        requiresDash = false
    }
}

nexusPortal.fieldInformation = {
    exitMode = {
        options = {"Primary", "Alternate", "Random", "Flag"},
        editable = false
    },
    cooldown = {
        minimumValue = 0.05
    },
    exitBoost = {
        minimumValue = 0.0
    },
    radius = {
        minimumValue = 4.0
    }
}

local fillColor = {0.05, 0.25, 0.3, 0.9}
local lineColor = {0.18, 0.88, 0.77, 1.0}
local exitBColor = {0.18, 0.88, 0.77, 1.0}
local exitCColor = {0.78, 0.47, 0.95, 1.0}

local function drawPortal(x, y, radius, ringColor)
    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(fillColor)
        love.graphics.circle("fill", x, y, radius * 0.75)
        love.graphics.setColor(ringColor)
        love.graphics.circle("line", x, y, radius)
        love.graphics.circle("line", x, y, radius * 0.55)
    end)
end

function nexusPortal.sprite(room, entity)
    local radius = entity.radius or 12
    local nodes = entity.nodes or {}
    local sprites = {
        drawableFunction.fromFunction(drawPortal, entity.x or 0, entity.y or 0, radius, lineColor)
    }

    -- Exit B is the first node, exit C the optional second.
    if nodes[1] then
        table.insert(sprites, drawableFunction.fromFunction(drawPortal, nodes[1].x, nodes[1].y,
            radius, exitBColor))
    end

    if nodes[2] then
        table.insert(sprites, drawableFunction.fromFunction(drawPortal, nodes[2].x, nodes[2].y,
            radius, exitCColor))
    end

    return sprites
end

function nexusPortal.selection(room, entity)
    local radius = entity.radius or 12
    local x, y = entity.x or 0, entity.y or 0
    local nodeRects = {}

    for _, node in ipairs(entity.nodes or {}) do
        table.insert(nodeRects, utils.rectangle(node.x - radius, node.y - radius, radius * 2, radius * 2))
    end

    return utils.rectangle(x - radius, y - radius, radius * 2, radius * 2), nodeRects
end

return nexusPortal
