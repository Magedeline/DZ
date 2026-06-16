local utils = require("utils")

return {
    name = "DZ/CubeDreamBlock",
    depth = function(room, entity)
        return entity.below and 5000 or -11000
    end,
    fillColor = {0.3, 0.0, 0.5, 0.4},
    borderColor = {1.0, 0.4, 0.9, 1.0},
    fieldInformation = {
        oneUse = {
            fieldType = "boolean"
        },
        fastMoving = {
            fieldType = "boolean"
        },
        below = {
            fieldType = "boolean"
        },
        requiredCutsceneFlag = {
            fieldType = "string"
        },
        requiresCutscene = {
            fieldType = "boolean"
        },
        cubeColor = {
            fieldType = "color"
        },
        dreamColor = {
            fieldType = "color"
        }
    },
    fieldOrder = {
        "x", "y", "width", "height",
        "requiresCutscene",
        "requiredCutsceneFlag",
        "oneUse",
        "fastMoving", 
        "below",
        "cubeColor",
        "dreamColor"
    },
    placements = {
        name = "Cube Dream Block",
        data = {
            width = 16,
            height = 16,
            oneUse = false,
            fastMoving = false,
            below = false,
            requiredCutsceneFlag = "chara_mirror_cutscene_completed",
            requiresCutscene = true,
            cubeColor = "4B0082",
            dreamColor = "FF69B4"
        }
    }
}