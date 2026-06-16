local utils = require("utils")

local eyeBomb = {}

eyeBomb.name = "DZ/EyeBomb"
eyeBomb.depth = -50
eyeBomb.texture = "objects/eyebomb/eye"

eyeBomb.placements = {
    {
        name = "normal",
        data = {
            detectionRadius = 64.0,
            explosionRadius = 48.0,
            fuseTime = 1.5
        }
    },
    {
        name = "large_detection",
        data = {
            detectionRadius = 96.0,
            explosionRadius = 64.0,
            fuseTime = 2.0
        }
    }
}

eyeBomb.fieldInformation = {
    detectionRadius = {
        fieldType = "number",
        description = "Radius at which the eye bomb detects the Kglobal::Player"
    },
    explosionRadius = {
        fieldType = "number",
        description = "Radius of the explosion effect"
    },
    fuseTime = {
        fieldType = "number",
        description = "Time in seconds before explosion after detection"
    }
}

eyeBomb.fieldOrder = {
    "x", "y", "detectionRadius", "explosionRadius", "fuseTime"
}

function eyeBomb.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return eyeBomb
