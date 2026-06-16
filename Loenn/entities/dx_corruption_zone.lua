-- DX Corruption Zone - Hazardous corruption area
-- DX-Side exclusive hazard

local utils = require("utils")

local dxCorruptionZone = {}

dxCorruptionZone.name = "DZ/DXCorruptionZone"
dxCorruptionZone.depth = 50
dxCorruptionZone.canResize = {true, true}
dxCorruptionZone.fillColor = {0.5, 0.0, 0.5, 0.2}
dxCorruptionZone.borderColor = {0.8, 0.0, 0.8, 0.6}

dxCorruptionZone.placements = {
    {
        name = "DXCorruptionZone",
        data = {
            width = 64,
            height = 64,
            damageInterval = 1.0,
            intensity = 1.0
        }
    },
    {
        name = "DXCorruptionZone_Intense",
        data = {
            width = 64,
            height = 64,
            damageInterval = 0.5,
            intensity = 2.0
        }
    }
}

dxCorruptionZone.fieldInformation = {
    damageInterval = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },
    intensity = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    }
}

dxCorruptionZone.fieldOrder = { "x", "y", "width", "height", "damageInterval", "intensity" }

function dxCorruptionZone.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 64, entity.height or 64)
end

return dxCorruptionZone
