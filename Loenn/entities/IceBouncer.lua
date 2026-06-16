local utils = require("utils")

return {
    name = "DZ/IceBouncer",
    depth = -8500,
    texture = "objects/Ingeste/iceBouncer/idle00",
    fieldInformation = {
        requiresCoreMode = {
            fieldType = "boolean"
        },
        dashesGranted = {
            fieldType = "integer",
            minimumValue = 1,
            maximumValue = 3
        },
        bounceStrength = {
            fieldType = "number"
        },
        iceColor = {
            fieldType = "color"
        }
    },
    fieldOrder = {
        "x", "y",
        "requiresCoreMode",
        "dashesGranted", 
        "bounceStrength",
        "iceColor"
    },
    placements = {
        name = "Ice Bouncer",
        data = {
            requiresCoreMode = true,
            dashesGranted = 2,
            bounceStrength = -180.0,
            iceColor = "87CEEB"
        }
    }
}