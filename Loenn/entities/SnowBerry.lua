local utils = require("utils")

local snowBerry = {}

snowBerry.name = "DZ/SnowBerry"
snowBerry.depth = -100
snowBerry.texture = "objects/snowberry"

snowBerry.placements = {
    {
        name = "SnowBerry",
        data = {}
    }
}

snowBerry.fieldOrder = {
    "x", "y"
}

function snowBerry.selection(room, entity)
    return utils.rectangle(entity.x - 7, entity.y - 7, 14, 14)
end

return snowBerry
