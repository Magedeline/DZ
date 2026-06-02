-- DX Laser Gate - Toggling laser barrier between two points
-- DX-Side exclusive hazard

local utils = require("utils")

local dxLaserGate = {}

dxLaserGate.name = "MaggyHelper/DXLaserGate"
dxLaserGate.depth = -9500
dxLaserGate.nodeLimits = {1, 1}
dxLaserGate.nodeLineRenderType = "line"

dxLaserGate.placements = {
    {
        name = "DXLaserGate",
        data = {
            onTime = 2.0,
            offTime = 1.0,
            startOn = true,
            color = "FF0000"
        }
    },
    {
        name = "DXLaserGate_Fast",
        data = {
            onTime = 1.0,
            offTime = 0.5,
            startOn = true,
            color = "FF4400"
        }
    },
    {
        name = "DXLaserGate_Slow",
        data = {
            onTime = 4.0,
            offTime = 2.0,
            startOn = false,
            color = "CC0000"
        }
    }
}

dxLaserGate.fieldInformation = {
    onTime = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 20.0
    },
    offTime = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 20.0
    },
    startOn = { fieldType = "boolean" },
    color = { fieldType = "color" }
}

dxLaserGate.fieldOrder = { "x", "y", "onTime", "offTime", "startOn", "color" }

function dxLaserGate.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local mainRect = utils.rectangle(x - 4, y - 4, 8, 8)

    local nodes = entity.nodes or {}
    local nodeSelections = {}
    for _, node in ipairs(nodes) do
        table.insert(nodeSelections, utils.rectangle(node.x - 4, node.y - 4, 8, 8))
    end

    return mainRect, nodeSelections
end

return dxLaserGate
