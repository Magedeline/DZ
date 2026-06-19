local utils = require("utils")

local uwumper = {}

uwumper.name = "DZ/UwUmper"
uwumper.depth = -8500
uwumper.texture = "objects/uwumper/Idle00"

uwumper.placements = {
    {
        name = "main",
        data = {
            fireMode = false
        }
    },
    {
        name = "fire_mode",
        data = {
            fireMode = true
        }
    }
}

uwumper.fieldInformation = {
    fireMode = {
        fieldType = "boolean",
        description = "If true, uses stronger launch force and different sound"
    }
}

uwumper.fieldOrder = {
    "x", "y", "fireMode"
}

function uwumper.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return uwumper
