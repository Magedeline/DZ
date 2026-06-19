local utils = require("utils")

local elsTerminaBoss = {}

elsTerminaBoss.name = "DZ/ELSTerminaBoss"
elsTerminaBoss.depth = -12500
elsTerminaBoss.placements = {
    {
        name = "main",
        data = {
        phase = 4,
        fromCutscene = false,
        hardMode = false
        }
    }
}

elsTerminaBoss.fieldInformation = {
    phase = {
        fieldType = "integer",
        options = {1, 2, 3, 4},
        editable = true
    },
    fromCutscene = {
        fieldType = "boolean"
    },
    hardMode = {
        fieldType = "boolean"
    }
}

elsTerminaBoss.fieldOrder = {
    "x",
    "y",
    "phase",
    "fromCutscene",
    "hardMode"
}

function elsTerminaBoss.selection(room, entity)
    return utils.rectangle(entity.x - 40, entity.y - 60, 80, 120)
end

return elsTerminaBoss
