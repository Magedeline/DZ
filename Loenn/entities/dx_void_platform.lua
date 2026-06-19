-- DX Void Platform - Phase in/out platform
-- DX-Side exclusive platforming element

local dxVoidPlatform = {}

dxVoidPlatform.name = "DZ/DXVoidPlatform"
dxVoidPlatform.depth = 0
dxVoidPlatform.canResize = {true, false}

dxVoidPlatform.placements = {
    {
        name = "main",
        data = {
            width = 48,
            phaseInterval = 2.0,
            phaseDuration = 1.0,
            phaseOffset = 0.0
        }
    }
}

dxVoidPlatform.fieldInformation = {
    phaseInterval = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 10.0
    },
    phaseDuration = {
        fieldType = "number",
        minimumValue = 0.2,
        maximumValue = 5.0
    },
    phaseOffset = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    }
}

dxVoidPlatform.fieldOrder = { "x", "y", "width", "phaseInterval", "phaseDuration", "phaseOffset" }

function dxVoidPlatform.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 48
    -- Simple rectangle representation
    return nil
end

function dxVoidPlatform.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 48, 8)
end

local utils = require("utils")

return dxVoidPlatform
