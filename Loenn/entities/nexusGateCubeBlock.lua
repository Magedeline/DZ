-- Loenn plugin for DZ/NexusGateCubeBlock (chapter 14 - Cyber Nexus).
-- Placeholder visual: 2.5D cube block. Swap the sprite function for a drawableSprite once
-- real art exists; no placement data has to change.

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local nexusGateCubeBlock = {}

nexusGateCubeBlock.name = "DZ/NexusGateCubeBlock"
nexusGateCubeBlock.depth = -9000
nexusGateCubeBlock.minimumSize = {8, 8}

nexusGateCubeBlock.placements = {
    name = "nexus_gate_cube_block",
    data = {
        width = 16,
        height = 16,
        group = "",
        startOpen = false,
        openTime = 0.45,
        depthOffset = 4.0,
        invert = false
    }
}

nexusGateCubeBlock.fieldInformation = {
    openTime = {
        minimumValue = 0.05
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

local fillColor = {0.17, 0.31, 0.42, 0.95}
local sideColor = {0.1, 0.2, 0.27, 0.95}
local lineColor = {0.62, 0.83, 0.92, 1.0}

function nexusGateCubeBlock.sprite(room, entity)
    local width, height = entity.width or 16, entity.height or 16
    local x, y = entity.x or 0, entity.y or 0
    local depthOffset = entity.depthOffset or 4
    local sprites = {}

    -- Back face first, so the front face overlaps it.
    pushRect(sprites, x + depthOffset, y - depthOffset, width, height, sideColor, lineColor)
    pushRect(sprites, x, y, width, height, fillColor, lineColor)

    return sprites
end

function nexusGateCubeBlock.selection(room, entity)
    local depthOffset = entity.depthOffset or 4
    local width, height = entity.width or 16, entity.height or 16

    return utils.rectangle(entity.x or 0, (entity.y or 0) - depthOffset, width + depthOffset, height + depthOffset)
end

return nexusGateCubeBlock
