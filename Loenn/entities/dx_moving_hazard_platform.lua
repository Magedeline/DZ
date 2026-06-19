-- DX Moving Hazard Platform - Platform with hazard edges
-- DX-Side exclusive platforming element

local utils = require("utils")

local dxMovingHazardPlatform = {}

dxMovingHazardPlatform.name = "DZ/DXMovingHazardPlatform"
dxMovingHazardPlatform.depth = 0
dxMovingHazardPlatform.canResize = {true, false}
dxMovingHazardPlatform.nodeLimits = {1, 8}
dxMovingHazardPlatform.nodeLineRenderType = "line"

dxMovingHazardPlatform.placements = {
    {
        name = "main",
        data = {
            width = 48,
            speed = 80.0,
            pauseTime = 0.5,
            hazardSides = true
        }
    },
    {
        name = "DXMovingHazardPlatform_Fast",
        data = {
            width = 32,
            speed = 160.0,
            pauseTime = 0.2,
            hazardSides = true
        }
    }
}

dxMovingHazardPlatform.fieldInformation = {
    speed = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 500.0
    },
    pauseTime = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0
    },
    hazardSides = { fieldType = "boolean" }
}

dxMovingHazardPlatform.fieldOrder = { "x", "y", "width", "speed", "pauseTime", "hazardSides" }

function dxMovingHazardPlatform.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 48, 8)
end

return dxMovingHazardPlatform
