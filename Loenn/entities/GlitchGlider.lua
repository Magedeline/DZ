local utils = require("utils")

return {
    name = "DZ/GlitchGlider",
    depth = -100, 
    texture = "objects/Ingeste/glitchGlider/available00",
    fieldInformation = {
        maxUses = {
            fieldType = "integer",
            minimumValue = 1,
            maximumValue = 10
        },
        throwSpeed = {
            fieldType = "number",
            minimumValue = 50.0,
            maximumValue = 500.0
        },
        teleportRange = {
            fieldType = "number",
            minimumValue = 100.0,
            maximumValue = 600.0
        },
        glitchColor1 = {
            fieldType = "color"
        },
        glitchColor2 = {
            fieldType = "color"
        }
    },
    fieldOrder = {
        "x", "y",
        "maxUses",
        "throwSpeed",
        "teleportRange",
        "glitchColor1",
        "glitchColor2"
    },
    placements = {
        name = "Glitch Glider",
        data = {
            maxUses = 5,
            throwSpeed = 200.0,
            teleportRange = 300.0,
            glitchColor1 = "FF00FF",
            glitchColor2 = "00FFFF"
        }
    }
}