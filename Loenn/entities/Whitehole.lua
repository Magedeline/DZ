local utils = require("utils")

local whitehole = {}

whitehole.name = "DZ/WhiteHole"

whitehole.placements = {
    name = "WhiteHole",
    data = {
        SpeedModifier = 1.02,
        ForceModifier = 0.8
    }
}

whitehole.texture = "objects/WhiteHole/WhiteHole00"

function whitehole.selection(room, entity)
    return utils.rectangle(entity.x - 15, entity.y - 15, 30, 30)
end

return whitehole