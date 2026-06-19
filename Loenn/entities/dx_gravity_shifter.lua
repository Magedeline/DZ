-- DX Gravity Shifter - Gravity-altering zone
-- DX-Side exclusive platforming element

local utils = require("utils")

local dxGravityShifter = {}

dxGravityShifter.name = "DZ/DXGravityShifter"
dxGravityShifter.depth = 100
dxGravityShifter.canResize = {true, true}
dxGravityShifter.fillColor = {0.0, 0.8, 0.8, 0.2}
dxGravityShifter.borderColor = {0.0, 0.8, 0.8, 0.6}

dxGravityShifter.placements = {
    {
        name = "main",
        data = {
            width = 80,
            height = 80,
            gravityMultiplier = -1.0,
            startActive = true
        }
    },
    {
        name = "DXGravityShifter_Low",
        data = {
            width = 80,
            height = 80,
            gravityMultiplier = 0.3,
            startActive = true
        }
    },
    {
        name = "DXGravityShifter_Heavy",
        data = {
            width = 80,
            height = 80,
            gravityMultiplier = 2.0,
            startActive = true
        }
    }
}

dxGravityShifter.fieldInformation = {
    gravityMultiplier = {
        fieldType = "number",
        minimumValue = -3.0,
        maximumValue = 5.0
    },
    startActive = { fieldType = "boolean" }
}

dxGravityShifter.fieldOrder = { "x", "y", "width", "height", "gravityMultiplier", "startActive" }

function dxGravityShifter.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 80, entity.height or 80)
end

return dxGravityShifter
