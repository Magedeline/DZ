-- DX Dimensional Warp Block - Teleport on dash
-- DX-Side exclusive movement mechanic

local utils = require("utils")

local dxWarpBlock = {}

dxWarpBlock.name = "MaggyHelper/DXDimensionalWarpBlock"
dxWarpBlock.depth = 0
dxWarpBlock.canResize = {true, true}
dxWarpBlock.fillColor = {0.0, 0.5, 1.0, 0.4}
dxWarpBlock.borderColor = {0.0, 0.7, 1.0, 0.8}
dxWarpBlock.nodeLimits = {1, 1}
dxWarpBlock.nodeLineRenderType = "line"

dxWarpBlock.placements = {
    {
        name = "DXDimensionalWarpBlock",
        data = {
            width = 16,
            height = 16,
            cooldown = 1.0
        }
    }
}

dxWarpBlock.fieldInformation = {
    cooldown = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    }
}

dxWarpBlock.fieldOrder = { "x", "y", "width", "height", "cooldown" }

function dxWarpBlock.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 16)
end

return dxWarpBlock
