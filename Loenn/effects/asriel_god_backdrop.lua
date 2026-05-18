local asrielGodBackdrop = {}

asrielGodBackdrop.name = "MaggyHelper/AsrielGodBackdrop"
asrielGodBackdrop.canForeground = false
asrielGodBackdrop.canBackground = true

asrielGodBackdrop.defaultData = {
    intensity = 1.0,
    speed = 1.0,
    starIntensity = 1.0,
    gridExpansionSpeed = 0.3,
    rainbowSpeed = 2.0,
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

asrielGodBackdrop.fieldInformation = {
    intensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0,
        defaultValue = 1.0,
        description = "Overall intensity of the backdrop effect (0-2)"
    },
    speed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0,
        defaultValue = 1.0,
        description = "Animation speed multiplier (0-5)"
    },
    starIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 3.0,
        defaultValue = 1.0,
        description = "Brightness/intensity of rainbow stars (0-3)"
    },
    gridExpansionSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0,
        defaultValue = 0.3,
        description = "Speed of the expanding perspective grid (0-2)"
    },
    rainbowSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0,
        defaultValue = 2.0,
        description = "Speed of rainbow color cycling (0-10)"
    },
    scrollX = {
        fieldType = "number",
        minimumValue = -10.0,
        maximumValue = 10.0,
        defaultValue = 1.0,
        description = "Horizontal scroll speed"
    },
    scrollY = {
        fieldType = "number",
        minimumValue = -10.0,
        maximumValue = 10.0,
        defaultValue = 1.0,
        description = "Vertical scroll speed"
    },
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0,
        defaultValue = 1.0,
        description = "Base opacity (0-1)"
    },
    rotspeed = {
        fieldType = "number",
        description = "Rotation speed in degrees per second (default: 0 = no rotation, backdrop expands outward instead)"
    },
    vk = {
        fieldType = "number",
        description = "GML vk parameter - scale velocity/expansion multiplier"
    }
}

asrielGodBackdrop.fieldOrder = {
    "x", "y",
    "intensity", "speed",
    "starIntensity", "gridExpansionSpeed", "rainbowSpeed",
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

asrielGodBackdrop.placements = {
    {
        name = "asriel_god_backdrop",
        data = {
            intensity = 1.0,
            speed = 1.0,
            starIntensity = 1.0,
            gridExpansionSpeed = 0.3,
            rainbowSpeed = 2.0,
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
        name = "asriel_god_backdrop_intense",
        data = {
            intensity = 1.5,
            speed = 1.5,
            starIntensity = 2.0,
            gridExpansionSpeed = 0.5,
            rainbowSpeed = 3.0,
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
        name = "asriel_god_backdrop_cosmic",
        data = {
            intensity = 2.0,
            speed = 2.0,
            starIntensity = 3.0,
            gridExpansionSpeed = 0.8,
            rainbowSpeed = 5.0,
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

return asrielGodBackdrop
