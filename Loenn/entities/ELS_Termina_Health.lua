local utils = require("utils")

local elsTerminaHealth = {}

elsTerminaHealth.name = "MaggyHelper/ELSTerminaHealth"
elsTerminaHealth.depth = -100000
elsTerminaHealth.placements = {
    name = "default",
    data = {
        maxHealth = 300,
        hardMode = false
    }
}

elsTerminaHealth.fieldInformation = {
    maxHealth = {
        fieldType = "number",
        editable = true
    },
    hardMode = {
        fieldType = "boolean"
    }
}

elsTerminaHealth.fieldOrder = {
    "x",
    "y",
    "maxHealth",
    "hardMode"
}

function elsTerminaHealth.selection(room, entity)
    return utils.rectangle(entity.x - 50, entity.y - 20, 100, 40)
end

return elsTerminaHealth
