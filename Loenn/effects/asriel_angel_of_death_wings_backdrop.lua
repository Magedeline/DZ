local asrielAngelWings = {}

asrielAngelWings.name = "MaggyHelper/AsrielAngelOfDeathWingsBackdrop"
asrielAngelWings.canForeground = false
asrielAngelWings.canBackground = true

asrielAngelWings.defaultData = {
    intensity = 1.0,
    speed = 1.0,
    wingScale = 1.0,
    expansionSpeed = 18.0,
    loop = true,
    bgAlpha = 1.0,
    wingAlpha = 1.0,
    scrollX = 0.0,
    scrollY = 0.0,
    speedX = 0.0,
    speedY = 0.0,
    fadeX = "",
    fadeY = "",
    color = "FFFFFF",
    alpha = 1.0,
    flipX = false,
    flipY = false,
    loopX = true,
    loopY = true,
    instantIn = false,
    instantOut = false,
    fadeIn = false,
    fadeOut = false,
    tag = "",
    flag = "",
    notFlag = "",
    only = "*",
    exclude = ""
}

asrielAngelWings.fieldInformation = {
    intensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0,
        defaultValue = 1.0,
        description = "Overall brightness/intensity of the effect (0-2)"
    },
    speed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0,
        defaultValue = 1.0,
        description = "Animation speed multiplier (0-5)"
    },
    wingScale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0,
        defaultValue = 1.0,
        description = "Scale of the wing sprites (0.1-5)"
    },
    expansionSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 200.0,
        defaultValue = 18.0,
        description = "Pixels per second that the wings move outward (0-200)"
    },
    loop = {
        fieldType = "boolean",
        defaultValue = true,
        description = "Whether the wings reset and loop after fully expanding"
    },
    bgAlpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0,
        defaultValue = 1.0,
        description = "Opacity of the Asriel God background visible through the gap (0-1)"
    },
    wingAlpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0,
        defaultValue = 1.0,
        description = "Opacity of the wing sprites (0-1)"
    },
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0,
        defaultValue = 1.0,
        description = "Base opacity of the entire backdrop (0-1)"
    }
}

asrielAngelWings.fieldOrder = {
    "x", "y",
    "intensity", "speed",
    "wingScale", "expansionSpeed", "loop",
    "bgAlpha", "wingAlpha",
    "scrollX", "scrollY",
    "speedX", "speedY",
    "fadeX", "fadeY",
    "color", "alpha",
    "flipX", "flipY",
    "loopX", "loopY",
    "instantIn", "instantOut",
    "fadeIn", "fadeOut",
    "tag", "flag", "notFlag",
    "only", "exclude"
}

asrielAngelWings.placements = {
    {
        name = "asriel_angel_wings_backdrop",
        data = {
            intensity = 1.0,
            speed = 1.0,
            wingScale = 1.0,
            expansionSpeed = 18.0,
            loop = true,
            bgAlpha = 1.0,
            wingAlpha = 1.0,
            only = "*",
            exclude = ""
        }
    },
    {
        name = "asriel_angel_wings_slow_reveal",
        data = {
            intensity = 1.2,
            speed = 0.8,
            wingScale = 1.3,
            expansionSpeed = 8.0,
            loop = false,
            bgAlpha = 1.0,
            wingAlpha = 1.0,
            only = "*",
            exclude = ""
        }
    },
    {
        name = "asriel_angel_wings_intense",
        data = {
            intensity = 1.8,
            speed = 1.5,
            wingScale = 1.5,
            expansionSpeed = 35.0,
            loop = true,
            bgAlpha = 1.0,
            wingAlpha = 1.0,
            only = "*",
            exclude = ""
        }
    }
}

return asrielAngelWings
