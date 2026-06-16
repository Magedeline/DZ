local utils = require("utils")
local loadedState = require("loaded_state")
local logging = require("logging")

return {
    name = "DZ/CharaChaser2",
    depth = 0,
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
            minimumValue = 0.5,
            maximumValue = 3.0
        },
        triggerWarning = {
            fieldType = "boolean"
        }
    },
    placements = {
        {
            name = "CharaChaser2",
            data = {
                canChangeMusic = true,
                aggressive = false,
                speedMultiplier = 1.25,
                triggerWarning = true
            }
        },
        {
            name = "CharaChaser2 (No Warning)",
            data = {
                canChangeMusic = true,
                aggressive = false,
                speedMultiplier = 1.25,
                triggerWarning = false
            }
        }
    },
    fieldOrder = {
        "x", "y",
        "canChangeMusic",
        "aggressive",
        "speedMultiplier",
        "triggerWarning"
    }
}
