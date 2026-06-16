local utils = require("utils")

local dreamOrb = {}

dreamOrb.name = "DZ/DreamOrb"
dreamOrb.depth = -100
dreamOrb.texture = "objects/dreamorb/00"

dreamOrb.placements = {
    {
        name = "normal",
        data = {
            oneUse = false
        }
    },
    {
        name = "one_use",
        data = {
            oneUse = true
        }
    }
}

dreamOrb.fieldInformation = {
    oneUse = {
        fieldType = "boolean",
        description = "If true, the dream orb will disappear permanently after use"
    }
}

dreamOrb.fieldOrder = {
    "x", "y", "oneUse"
}

function dreamOrb.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return dreamOrb
