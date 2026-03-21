local utils = require("utils")
local loadedState = require("loaded_state")
local logging = require("logging")

local charaChaser = {
    name = "MaggyHelper/CharaChaser",
    depth = -1,
    nodeLineRenderType = "line",
    texture = "characters/chara/idle00",
    nodeLimits = {0, -1},
    fieldInformation = {
        canChangeMusic = {
            fieldType = "boolean"
        },
        aggressive = {
            fieldType = "boolean"
        },
        speedMultiplier = {
            fieldType = "number",
            minimumValue = 0.1,
            maximumValue = 5.0
        }
    },
    fieldOrder = {
        "x", "y",
        "canChangeMusic",
        "aggressive",
        "speedMultiplier"
    },
    placements = {
        {
            name = "CharaChaser",
            data = {
                canChangeMusic = true,
                aggressive = false,
                speedMultiplier = 1.0
            }
        },
        {
            name = "CharaChaser (Aggressive)",
            data = {
                canChangeMusic = true,
                aggressive = true,
                speedMultiplier = 1.5
            }
        }
    }
}

return charaChaser
