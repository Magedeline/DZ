local elsTrueFinalBackdrop = {}

elsTrueFinalBackdrop.name = "MaggyHelper/ElsTrueFinalBackdrop"
elsTrueFinalBackdrop.canForeground = false
elsTrueFinalBackdrop.canBackground = true

elsTrueFinalBackdrop.defaultData = {
    intensity = 1.0,
    speed = 1.0,
    voidRadius = 60.0,
    rainbowEdgeIntensity = 1.0,
    gridExpansionSpeed = 0.4,
    rainbowSpeed = 1.5,
    corruptionSpeed = 0.8,
    scrollX = 1.0,
    scrollY = 1.0,
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

elsTrueFinalBackdrop.fieldInformation = {
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
    voidRadius = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 150.0,
        defaultValue = 60.0
    },
    rainbowEdgeIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 3.0,
        defaultValue = 1.0
    },
    gridExpansionSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0,
        defaultValue = 0.4
    },
    rainbowSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0,
        defaultValue = 1.5
    },
    corruptionSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0,
        defaultValue = 0.8
    },
    scrollX = {
        fieldType = "number",
        minimumValue = -10.0,
        maximumValue = 10.0,
        defaultValue = 1.0
    },
    scrollY = {
        fieldType = "number",
        minimumValue = -10.0,
        maximumValue = 10.0,
        defaultValue = 1.0
    },
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0,
        defaultValue = 1.0
    }
}

elsTrueFinalBackdrop.fieldOrder = {
    "x", "y",
    "intensity", "speed",
    "voidRadius", "rainbowEdgeIntensity",
    "gridExpansionSpeed", "rainbowSpeed", "corruptionSpeed",
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

elsTrueFinalBackdrop.placements = {
    {
        name = "els_true_final_backdrop",
        data = {
            intensity = 1.0,
            speed = 1.0,
            voidRadius = 60.0,
            rainbowEdgeIntensity = 1.0,
            gridExpansionSpeed = 0.4,
            rainbowSpeed = 1.5,
            corruptionSpeed = 0.8,
            scrollX = 1.0,
            scrollY = 1.0,
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
    },
    {
        name = "els_true_final_backdrop_intense",
        data = {
            intensity = 1.5,
            speed = 1.2,
            voidRadius = 80.0,
            rainbowEdgeIntensity = 1.5,
            gridExpansionSpeed = 0.6,
            rainbowSpeed = 2.5,
            corruptionSpeed = 1.2,
            scrollX = 1.0,
            scrollY = 1.0,
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
    },
    {
        name = "els_true_final_backdrop_void",
        data = {
            intensity = 2.0,
            speed = 1.5,
            voidRadius = 100.0,
            rainbowEdgeIntensity = 2.0,
            gridExpansionSpeed = 0.8,
            rainbowSpeed = 4.0,
            corruptionSpeed = 1.5,
            scrollX = 1.0,
            scrollY = 1.0,
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
    },
    {
        name = "els_true_final_backdrop_small_void",
        data = {
            intensity = 1.0,
            speed = 1.0,
            voidRadius = 30.0,
            rainbowEdgeIntensity = 1.5,
            gridExpansionSpeed = 0.3,
            rainbowSpeed = 2.0,
            corruptionSpeed = 0.5,
            scrollX = 1.0,
            scrollY = 1.0,
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
    }
}

return elsTrueFinalBackdrop
