-- Loenn plugin for DZ/FlashingBlack (chapter 15 - Citadel).
-- Placeholder visual: small black marker square (the effect is full-screen in game). Swap
-- the sprite function for a drawableSprite once real art exists; no placement data has to
-- change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local flashingBlack = {}

flashingBlack.name = "DZ/FlashingBlack"
flashingBlack.depth = -1000000

flashingBlack.placements = {
    name = "flashing_black",
    data = {
        visibleDuration = 2.4,
        fadeDuration = 0.35,
        blackDuration = 1.1,
        maxAlpha = 1.0,
        keepPlayerVisible = false,
        warnFlash = true,
        startBlack = false,
        flag = "",
        invertFlag = false
    }
}

flashingBlack.fieldInformation = {
    visibleDuration = {
        minimumValue = 0.1
    },
    fadeDuration = {
        minimumValue = 0.01
    },
    blackDuration = {
        minimumValue = 0.1
    },
    maxAlpha = {
        minimumValue = 0.0,
        maximumValue = 1.0
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

local fillColor = {0.05, 0.05, 0.07, 0.95}
local lineColor = {0.65, 0.65, 0.7, 1.0}

-- One instance blacks out the whole room, so the editor only needs a marker.
local markerSize = 16

function flashingBlack.sprite(room, entity)
    local x, y = (entity.x or 0) - markerSize / 2, (entity.y or 0) - markerSize / 2
    local sprites = {}

    pushRect(sprites, x, y, markerSize, markerSize, fillColor, lineColor)

    return sprites
end

function flashingBlack.selection(room, entity)
    return utils.rectangle((entity.x or 0) - markerSize / 2, (entity.y or 0) - markerSize / 2,
        markerSize, markerSize)
end

return flashingBlack
