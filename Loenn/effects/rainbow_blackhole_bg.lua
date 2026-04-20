local rainbow_blackhole_bg = {}

rainbow_blackhole_bg.name = "MaggyHelper/RainbowBlackholeBg"
rainbow_blackhole_bg.canForeground = false
rainbow_blackhole_bg.canBackground = true

rainbow_blackhole_bg.fieldInformation = {
    strength = {
        options = {
            "Mild",
            "Medium",
            "High",
            "Wild",
            "Insane",
            "RainbowChaos",
            "Cosmic"
        },
        editable = false
    },
    animationMode = {
        options = {
            "None",
            "Soul",
            "Zero",
            "Void"
        },
        editable = false
    },
    frameDelay = {
        fieldType = "number",
        minimumValue = 0.01,
        maximumValue = 1.0
    },
    animationScale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },
    glitchAmount = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    distortionAmount = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    distortionFrequency = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 8.0
    },
    chromaticAberration = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    }
}

rainbow_blackhole_bg.placements = {
    {
        name = "rainbow_blackhole_bg",
        data = {
            strength = "Mild",
            animationMode = "None",
            frameDelay = 0.08,
            animationScale = 1.0,
            glitchAmount = 0.0,
            distortionAmount = 0.0,
            distortionFrequency = 2.0,
            chromaticAberration = 0.0,
            animationLoops = true,
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
        name = "rainbow_blackhole_soul",
        data = {
            strength = "Cosmic",
            animationMode = "Soul",
            frameDelay = 0.08,
            animationScale = 1.0,
            glitchAmount = 0.0,
            distortionAmount = 0.0,
            distortionFrequency = 2.0,
            chromaticAberration = 0.0,
            animationLoops = true,
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
        name = "rainbow_blackhole_zero",
        data = {
            strength = "Cosmic",
            animationMode = "Zero",
            frameDelay = 0.08,
            animationScale = 1.0,
            glitchAmount = 0.0,
            distortionAmount = 0.0,
            distortionFrequency = 2.0,
            chromaticAberration = 0.0,
            animationLoops = true,
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
        name = "rainbow_blackhole_void",
        data = {
            strength = "Cosmic",
            animationMode = "Void",
            frameDelay = 0.08,
            animationScale = 1.0,
            glitchAmount = 0.0,
            distortionAmount = 0.0,
            distortionFrequency = 2.0,
            chromaticAberration = 0.0,
            animationLoops = true,
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
        name = "rainbow_blackhole_zero_glitch_distortion",
        data = {
            strength = "Cosmic",
            animationMode = "Zero",
            frameDelay = 0.07,
            animationScale = 1.05,
            glitchAmount = 0.28,
            distortionAmount = 0.24,
            distortionFrequency = 2.8,
            chromaticAberration = 0.6,
            animationLoops = true,
            scrollX = 1.0,
            scrollY = 1.0,
            speedX = 0.0,
            speedY = 0.0,
            fadeX = "",
            fadeY = "",
            color = "FFFFFF",
            alpha = 0.92,
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
        name = "rainbow_blackhole_darkmatter_glitch_distortion",
        data = {
            strength = "Cosmic",
            animationMode = "Void",
            frameDelay = 0.08,
            animationScale = 1.0,
            glitchAmount = 0.22,
            distortionAmount = 0.3,
            distortionFrequency = 2.2,
            chromaticAberration = 0.5,
            animationLoops = true,
            scrollX = 1.0,
            scrollY = 1.0,
            speedX = 0.0,
            speedY = 0.0,
            fadeX = "",
            fadeY = "",
            color = "FFFFFF",
            alpha = 0.88,
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

return rainbow_blackhole_bg
