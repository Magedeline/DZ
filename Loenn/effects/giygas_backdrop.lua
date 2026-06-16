local giygasBackdrop = {}

giygasBackdrop.name = "DZ/GiygasBackdrop"

giygasBackdrop.defaultData = {
    intensity = 1.0,
    speed = 1.0,
    colorShiftSpeed = 0.5,
    distortionStrength = 1.0,
    baseColor1R = 80,
    baseColor1G = 0,
    baseColor1B = 0,
    baseColor2R = 120,
    baseColor2G = 0,
    baseColor2B = 60,
    baseColor3R = 40,
    baseColor3G = 0,
    baseColor3B = 80,
    accentColorR = 255,
    accentColorG = 100,
    accentColorB = 100,
    only = "",
    exclude = "",
    flag = "",
    notFlag = "",
    fadeIn = false
}

giygasBackdrop.fieldInformation = {
    intensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0,
        defaultValue = 1.0
    },
    speed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0,
        defaultValue = 1.0
    },
    colorShiftSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 3.0,
        defaultValue = 0.5
    },
    distortionStrength = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 3.0,
        defaultValue = 1.0
    },
    baseColor1R = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 80
    },
    baseColor1G = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 0
    },
    baseColor1B = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 0
    },
    baseColor2R = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 120
    },
    baseColor2G = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 0
    },
    baseColor2B = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 60
    },
    baseColor3R = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 40
    },
    baseColor3G = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 0
    },
    baseColor3B = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 80
    },
    accentColorR = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 255
    },
    accentColorG = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 100
    },
    accentColorB = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 255,
        defaultValue = 100
    }
}

giygasBackdrop.fieldOrder = {
    "intensity",
    "speed",
    "colorShiftSpeed",
    "distortionStrength",
    "baseColor1R",
    "baseColor1G",
    "baseColor1B",
    "baseColor2R",
    "baseColor2G",
    "baseColor2B",
    "baseColor3R",
    "baseColor3G",
    "baseColor3B",
    "accentColorR",
    "accentColorG",
    "accentColorB",
    "only",
    "exclude",
    "flag",
    "notFlag",
    "fadeIn"
}

return giygasBackdrop
