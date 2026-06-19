local utils = require("utils")

return {
    name = "DZ/SuperCoreBlock", 
    depth = -200,
    texture = "objects/Ingeste/superCoreBlock/super_idle00",
    fieldInformation = {
        speedMultiplier = {
            fieldType = "number",
            minimumValue = 1.0,
            maximumValue = 5.0
        },
        launchRange = {
            fieldType = "number",
            minimumValue = 100.0,
            maximumValue = 800.0
        },
        requiresCoreMode = {
            fieldType = "boolean"
        },
        hotColor = {
            fieldType = "color"
        },
        coldColor = {
            fieldType = "color"
        },
        superColor = {
            fieldType = "color"
        }
    },
    fieldOrder = {
        "x", "y",
        "speedMultiplier",
        "launchRange",
        "requiresCoreMode",
        "hotColor",
        "coldColor",
        "superColor"
    },
    placements = {
    {
        name = "main",
        data = {
        speedMultiplier = 3.0,
        launchRange = 400.0,
        requiresCoreMode = false,
        hotColor = "FF4500",
        coldColor = "00BFFF", 
        superColor = "FFD700"
        }
    }
    }
}