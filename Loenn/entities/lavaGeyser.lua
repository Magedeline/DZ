-- Loenn plugin for DZ/LavaGeyser (chapter 13 - Hotcliff).
-- Placeholder visual: vent plate plus an outlined eruption column. Swap the sprite function
-- for a drawableSprite once real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local lavaGeyser = {}

lavaGeyser.name = "DZ/LavaGeyser"
lavaGeyser.depth = -8500
lavaGeyser.minimumSize = {4, 8}

lavaGeyser.placements = {
    name = "lava_geyser",
    data = {
        width = 16,
        eruptHeight = 88.0,
        warningTime = 0.85,
        eruptDuration = 1.1,
        cooldown = 2.4,
        startDelay = 0.0,
        downward = false,
        deadly = true,
        flag = "",
        invertFlag = false
    }
}

lavaGeyser.fieldInformation = {
    eruptHeight = {
        minimumValue = 8.0
    },
    warningTime = {
        minimumValue = 0.0
    },
    eruptDuration = {
        minimumValue = 0.1
    },
    cooldown = {
        minimumValue = 0.1
    },
    startDelay = {
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

local ventFill = {0.29, 0.15, 0.09, 1.0}
local ventLine = {0.48, 0.14, 0.03, 1.0}
local columnFill = {0.91, 0.39, 0.11, 0.35}
local columnLine = {1.0, 0.82, 0.4, 0.8}

-- The C# entity is a point with its column centred on x; mirror that here.
function lavaGeyser.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 16
    local height = entity.eruptHeight or 88
    local sprites = {}

    local left = x - width / 2
    local columnTop = entity.downward and y or y - height

    pushRect(sprites, left, columnTop, width, height, columnFill, columnLine)
    pushRect(sprites, left - 2, entity.downward and y - 3 or y, width + 4, 3, ventFill, ventLine)

    return sprites
end

function lavaGeyser.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 16

    return utils.rectangle(x - width / 2 - 2, y - 3, width + 4, 6)
end

return lavaGeyser
