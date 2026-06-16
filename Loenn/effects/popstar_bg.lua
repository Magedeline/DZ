local popstar_bg = {}

popstar_bg.name = "DZ/PopstarBg"
popstar_bg.canForeground = false
popstar_bg.canBackground = true

popstar_bg.fieldInformation = {
    style = {
        options = {
            "Normal",
            "Dreamy",
            "Sunset",
            "Night",
            "Rainbow"
        },
        editable = false
    },
    frameDelay = {
        fieldType = "number",
        minimumValue = 0.01,
        maximumValue = 1.0
    },
    scale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    rotationSpeed = {
        fieldType = "number",
        minimumValue = -10.0,
        maximumValue = 10.0
    },
    scrollSpeedX = {
        fieldType = "number",
        minimumValue = -100.0,
        maximumValue = 100.0
    },
    scrollSpeedY = {
        fieldType = "number",
        minimumValue = -100.0,
        maximumValue = 100.0
    },
    pulseAmount = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    pulseSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    glowIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0
    },
    rainbowSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    }
}

popstar_bg.fieldOrder = {
    "x", "y", "style", "frameDelay", "scale", "alpha",
    "loops", "pingPong", "rotationSpeed",
    "scrollSpeedX", "scrollSpeedY",
    "tintColor", "pulseAmount", "pulseSpeed",
    "glowIntensity", "rainbowSpeed"
}

popstar_bg.placements = {
    {
        name = "popstar_normal",
        data = {
            style = "Normal",
            frameDelay = 0.08,
            scale = 1.0,
            alpha = 1.0,
            loops = true,
            pingPong = false,
            rotationSpeed = 0.0,
            scrollSpeedX = 0.0,
            scrollSpeedY = 0.0,
            tintColor = "FFFFFF",
            pulseAmount = 0.0,
            pulseSpeed = 1.0,
            glowIntensity = 0.0,
            rainbowSpeed = 1.0,
            scrollX = 1.0,
            scrollY = 1.0,
            speedX = 0.0,
            speedY = 0.0,
            fadeX = "",
            fadeY = "",
            color = "FFFFFF",
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
        name = "popstar_dreamy",
        data = {
            style = "Dreamy",
            frameDelay = 0.1,
            scale = 1.0,
            alpha = 0.9,
            loops = true,
            pingPong = false,
            rotationSpeed = 0.0,
            scrollSpeedX = 0.0,
            scrollSpeedY = 0.0,
            tintColor = "FFFFFF",
            pulseAmount = 0.1,
            pulseSpeed = 0.5,
            glowIntensity = 0.5,
            rainbowSpeed = 1.0,
            scrollX = 1.0,
            scrollY = 1.0,
            speedX = 0.0,
            speedY = 0.0,
            fadeX = "",
            fadeY = "",
            color = "FFFFFF",
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
        name = "popstar_sunset",
        data = {
            style = "Sunset",
            frameDelay = 0.08,
            scale = 1.0,
            alpha = 1.0,
            loops = true,
            pingPong = false,
            rotationSpeed = 0.0,
            scrollSpeedX = 0.0,
            scrollSpeedY = 0.0,
            tintColor = "FFFFFF",
            pulseAmount = 0.05,
            pulseSpeed = 0.3,
            glowIntensity = 0.3,
            rainbowSpeed = 1.0,
            scrollX = 1.0,
            scrollY = 1.0,
            speedX = 0.0,
            speedY = 0.0,
            fadeX = "",
            fadeY = "",
            color = "FFFFFF",
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
        name = "popstar_night",
        data = {
            style = "Night",
            frameDelay = 0.08,
            scale = 1.0,
            alpha = 1.0,
            loops = true,
            pingPong = false,
            rotationSpeed = 0.0,
            scrollSpeedX = 0.0,
            scrollSpeedY = 0.0,
            tintColor = "FFFFFF",
            pulseAmount = 0.0,
            pulseSpeed = 1.0,
            glowIntensity = 0.0,
            rainbowSpeed = 1.0,
            scrollX = 1.0,
            scrollY = 1.0,
            speedX = 0.0,
            speedY = 0.0,
            fadeX = "",
            fadeY = "",
            color = "FFFFFF",
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
        name = "popstar_rainbow",
        data = {
            style = "Rainbow",
            frameDelay = 0.08,
            scale = 1.0,
            alpha = 1.0,
            loops = true,
            pingPong = false,
            rotationSpeed = 0.0,
            scrollSpeedX = 0.0,
            scrollSpeedY = 0.0,
            tintColor = "FFFFFF",
            pulseAmount = 0.1,
            pulseSpeed = 2.0,
            glowIntensity = 0.3,
            rainbowSpeed = 0.5,
            scrollX = 1.0,
            scrollY = 1.0,
            speedX = 0.0,
            speedY = 0.0,
            fadeX = "",
            fadeY = "",
            color = "FFFFFF",
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

return popstar_bg
