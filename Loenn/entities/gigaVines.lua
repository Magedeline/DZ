-- Loenn plugin for DZ/GigaVines (chapter 16 - MyWorld).
-- Placeholder visual: dark green rectangle band with a growth guide. Swap the sprite
-- function for a drawableSprite once real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local gigaVines = {}

gigaVines.name = "DZ/GigaVines"
gigaVines.depth = -8500
gigaVines.minimumSize = {8, 8}

gigaVines.placements = {
    name = "giga_vines",
    data = {
        width = 32,
        height = 8,
        direction = "Up",
        growSpeed = 26.0,
        retractSpeed = 70.0,
        startDelay = 1.5,
        maxExtent = 0.0,
        deadly = true,
        tendrilCount = 8,
        flag = "",
        invertFlag = false,
        pauseFlag = "",
        retractFlag = ""
    }
}

gigaVines.fieldInformation = {
    direction = {
        options = {"Up", "Down", "Left", "Right"},
        editable = false
    },
    growSpeed = {
        minimumValue = 0.0
    },
    retractSpeed = {
        minimumValue = 1.0
    },
    startDelay = {
        minimumValue = 0.0
    },
    maxExtent = {
        minimumValue = 0.0
    },
    tendrilCount = {
        fieldType = "integer",
        minimumValue = 0
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

local fillColor = {0.12, 0.25, 0.14, 0.95}
local lineColor = {0.21, 0.42, 0.23, 1.0}
local strandColor = {0.48, 0.77, 0.5, 0.8}
local guideColor = {0.48, 0.77, 0.5, 0.3}

-- Growth preview: maxExtent of 0 means "grow to the room edge", which the editor
-- cannot know, so fall back to a fixed hint length.
local function extentPreview(entity)
    local extent = entity.maxExtent or 0

    return extent > 0 and extent or 48
end

function gigaVines.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 32, entity.height or 8
    local direction = entity.direction or "Up"
    local extent = extentPreview(entity)
    local sprites = {}

    local guideX, guideY, guideW, guideH = x, y, width, height

    if direction == "Up" then
        guideY, guideH = y - extent, extent
    elseif direction == "Down" then
        guideY, guideH = y + height, extent
    elseif direction == "Left" then
        guideX, guideW = x - extent, extent
    else
        guideX, guideW = x + width, extent
    end

    pushRect(sprites, guideX, guideY, guideW, guideH, guideColor, guideColor)
    pushRect(sprites, x, y, width, height, fillColor, lineColor)

    -- Strands along the band, matching the in-game look.
    if height >= width then
        for strandY = y + 3, y + height - 3, 7 do
            pushRect(sprites, x, strandY, width, 2, strandColor, strandColor)
        end
    else
        for strandX = x + 3, x + width - 3, 7 do
            pushRect(sprites, strandX, y, 2, height, strandColor, strandColor)
        end
    end

    return sprites
end

function gigaVines.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, entity.width or 32, entity.height or 8)
end

return gigaVines
