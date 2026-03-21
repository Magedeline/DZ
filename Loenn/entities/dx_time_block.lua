-- DX Time Block - Timed appear/disappear block
-- DX-Side exclusive platforming element

local utils = require("utils")

local dxTimeBlock = {}

dxTimeBlock.name = "MaggyHelper/DXTimeBlock"
dxTimeBlock.depth = 0
dxTimeBlock.canResize = {true, true}
dxTimeBlock.fillColor = {0.5, 0.3, 0.8, 0.5}
dxTimeBlock.borderColor = {0.7, 0.4, 1.0, 0.8}

dxTimeBlock.placements = {
    {
        name = "DXTimeBlock_GroupA",
        data = {
            width = 16,
            height = 16,
            groupId = 0,
            cycleTime = 3.0,
            phaseOffset = 0.0,
            color = "9966CC"
        }
    },
    {
        name = "DXTimeBlock_GroupB",
        data = {
            width = 16,
            height = 16,
            groupId = 1,
            cycleTime = 3.0,
            phaseOffset = 0.0,
            color = "CC6699"
        }
    }
}

dxTimeBlock.fieldInformation = {
    groupId = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 10
    },
    cycleTime = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 20.0
    },
    phaseOffset = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 20.0
    },
    color = {
        fieldType = "color"
    }
}

dxTimeBlock.fieldOrder = { "x", "y", "width", "height", "groupId", "cycleTime", "phaseOffset", "color" }

function dxTimeBlock.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 16)
end

return dxTimeBlock
