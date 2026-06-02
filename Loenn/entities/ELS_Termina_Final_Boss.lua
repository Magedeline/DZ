local utils = require("utils")

local elsTerminaFinalBoss = {}

elsTerminaFinalBoss.name = "MaggyHelper/ELSTerminaFinalBoss"
elsTerminaFinalBoss.depth = -12500
elsTerminaFinalBoss.placements = {
    name = "default",
    data = {
        difficultyMode = 0,
        fromCutscene = false,
        hasFiveHeartGems = false
    }
}

elsTerminaFinalBoss.fieldInformation = {
    difficultyMode = {
        fieldType = "integer",
        options = {0, 1, 2},
        fieldOptions = {
            [0] = "Normal (Darkness ELS)",
            [1] = "Morpho ELS",
            [2] = "Celestial Morpho ELS"
        },
        editable = true
    },
    fromCutscene = {
        fieldType = "boolean"
    },
    hasFiveHeartGems = {
        fieldType = "boolean"
    }
}

elsTerminaFinalBoss.fieldOrder = {
    "x",
    "y",
    "difficultyMode",
    "fromCutscene",
    "hasFiveHeartGems"
}

function elsTerminaFinalBoss.selection(room, entity)
    return utils.rectangle(entity.x - 50, entity.y - 70, 100, 140)
end

return elsTerminaFinalBoss
