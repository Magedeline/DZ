-- Loenn plugin for DZ/Replayback (chapter 16 - MyWorld).
-- Placeholder visual: small violet marker square (the ghost spawns at the player). Swap the
-- sprite function for a drawableSprite once real art exists; no placement data has to
-- change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local replayback = {}

replayback.name = "DZ/Replayback"
replayback.depth = -10000

replayback.placements = {
    name = "replayback",
    data = {
        recordDuration = 4.0,
        startDelay = 2.0,
        deadly = true,
        loop = true,
        ghostAlpha = 0.65,
        ghostSize = 8.0,
        flag = "",
        invertFlag = false
    }
}

replayback.fieldInformation = {
    recordDuration = {
        minimumValue = 0.5
    },
    startDelay = {
        minimumValue = 0.1
    },
    ghostAlpha = {
        minimumValue = 0.05,
        maximumValue = 1.0
    },
    ghostSize = {
        minimumValue = 4.0
    }
}

-- drawableRectangle returns a single sprite in "fill" mode and a list of sprites in
-- "line" mode, so flatten whatever comes back before handing it to Loenn.
local function push(sprites, rectangle)
    local result = rectangle:getDrawableSprite()

    if result[1] ~= nil then
        for _, sprite in ipairs(result) do
            table.insert(sprites, sprite)
        end
    else
        table.insert(sprites, result)
    end
end

local function pushRect(sprites, x, y, width, height, fill, line)
    push(sprites, drawableRectangle.fromRectangle("fill", x, y, width, height, fill))
    push(sprites, drawableRectangle.fromRectangle("line", x, y, width, height, line))
end

local fillColor = {0.69, 0.44, 0.84, 0.6}
local lineColor = {0.94, 0.85, 1.0, 1.0}

-- Room-wide recorder: the replayed ghost follows the player, not this marker.
local markerSize = 12

function replayback.sprite(room, entity)
    local x, y = (entity.x or 0) - markerSize / 2, (entity.y or 0) - markerSize / 2
    local sprites = {}

    pushRect(sprites, x, y, markerSize, markerSize, fillColor, lineColor)

    return sprites
end

function replayback.selection(room, entity)
    return utils.rectangle((entity.x or 0) - markerSize / 2, (entity.y or 0) - markerSize / 2,
        markerSize, markerSize)
end

return replayback
