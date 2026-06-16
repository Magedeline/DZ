local enums = require("consts.ingeste_enums")

local tesseractholeBG = {
    name = "DZ/TesseractholeBG",
    texture = "bgs/10/blackhole/particle",
    depth = 2000,
    placements = {
        {
            name = "normal",
            data = {
                alpha = 1.0,
                scale = 1.0,
                direction = 1.0,
                strength = "Mild",
                centerOffset = {x = 0, y = 0},
                offsetOffset = {x = 0, y = 0}
            }
        }
    },
    fieldInformation = {
        alpha = {
            fieldType = "number",
            minimumValue = 0.0,
            maximumValue = 1.0,
            defaultValue = 1.0
        },
        scale = {
            fieldType = "number",
            minimumValue = 0.1,
            maximumValue = 10.0,
            defaultValue = 1.0
        },
        direction = {
            fieldType = "number",
            minimumValue = -1.0,
            maximumValue = 1.0,
            defaultValue = 1.0
        },
        strength = {
            fieldType = "enum",
            options = enums.tesseracthole_bg_strengths,
            editable = false,
            defaultValue = "Mild"
        },
        centerOffset = {
            fieldType = "vector2"
        },
        offsetOffset = {
            fieldType = "vector2"
        }
    },
    fieldOrder = {
        "alpha",
        "scale",
        "direction",
        "strength",
        "centerOffset",
        "offsetOffset"
    }
}

return tesseractholeBG