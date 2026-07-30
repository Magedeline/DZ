-- Loenn plugin for DZ/NexusTouchSwitch (chapter 14 - Cyber Nexus).
-- Placeholder visual: 2.5D cube (front face plus an offset back face). Swap the sprite
-- function for a drawableSprite once real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local nexusTouchSwitch = {}

nexusTouchSwitch.name = "DZ/NexusTouchSwitch"
nexusTouchSwitch.depth = -100

nexusTouchSwitch.placements = {
    name = "nexus_touch_switch",
    data = {
        group = "",
        flag = "",
        requiresDash = false,
        persistent = false,
        size = 12.0,
        depthOffset = 4.0
    }
}

nexusTouchSwitch.fieldInformation = {
    size = {
        minimumValue = 6.0
    },
    depthOffset = {
        minimumValue = 0.0
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

local fillColor = {0.12, 0.23, 0.3, 0.95}
local sideColor = {0.08, 0.16, 0.2, 0.95}
local lineColor = {0.75, 0.97, 0.93, 1.0}

function nexusTouchSwitch.sprite(room, entity)
    local size = entity.size or 12
    local width, height = size, size
    local x, y = (entity.x or 0) - size / 2, (entity.y or 0) - size / 2
    local depthOffset = entity.depthOffset or 4
    local sprites = {}

    -- Back face first, so the front face overlaps it.
    pushRect(sprites, x + depthOffset, y - depthOffset, width, height, sideColor, lineColor)
    pushRect(sprites, x, y, width, height, fillColor, lineColor)

    return sprites
end

function nexusTouchSwitch.selection(room, entity)
    local size = entity.size or 12
    local depthOffset = entity.depthOffset or 4

    return utils.rectangle((entity.x or 0) - size / 2, (entity.y or 0) - size / 2 - depthOffset,
        size + depthOffset, size + depthOffset)
end

return nexusTouchSwitch
