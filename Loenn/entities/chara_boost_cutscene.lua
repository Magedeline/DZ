local utils = require("utils")

local charaBoostCutscene = {}

charaBoostCutscene.name = "MaggyHelper/CustomCharaBoostCutscene"
charaBoostCutscene.depth = 0

charaBoostCutscene.placements = {
    {
        name = "normal",
        data = {
            canSkip = true,
            oneUse = false,
            cutsceneDash = true
        }
    }
}

charaBoostCutscene.fieldInformation = {
    canSkip = {
        fieldType = "boolean"
    },
    oneUse = {
        fieldType = "boolean" 
    },
    cutsceneDash = {
        fieldType = "boolean"
    }
}

function charaBoostCutscene.sprite(room, entity)
    return {
        {
            texture = "objects/charaboost/idle00",
            x = entity.x,
            y = entity.y,
            color = {1.0, 0.0, 0.0, 1.0}  -- Pure red color
        }
    }
end

function charaBoostCutscene.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return charaBoostCutscene
