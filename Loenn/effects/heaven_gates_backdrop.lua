local heavenGatesBackdrop = {}

heavenGatesBackdrop.name = "DZ/HeavenGatesBackdrop"
heavenGatesBackdrop.canForeground = false
heavenGatesBackdrop.canBackground = true

heavenGatesBackdrop.defaultData = {
    intensity = 1.0,
    speed = 1.0,
    gateHeight = 100.0,
    gateWidth = 40.0,
    voidRadius = 35.0,
    glowIntensity = 1.0,
    astralBirthScale = 1.0,
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
    only = "end-saved",
    exclude = ""
}

heavenGatesBackdrop.fieldInformation = {
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
    gateHeight = {
        fieldType = "number",
        minimumValue = 20.0,
        maximumValue = 200.0,
        defaultValue = 100.0
    },
    gateWidth = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 100.0,
        defaultValue = 40.0
    },
    voidRadius = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 100.0,
        defaultValue = 35.0
    },
    glowIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 3.0,
        defaultValue = 1.0
    },
    astralBirthScale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 3.0,
        defaultValue = 1.0
    },
    color = {
        fieldType = "color"
    },
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0,
        defaultValue = 1.0
    },
    only = {
        fieldType = "string",
        defaultValue = "end-saved"
    },
    exclude = {
        fieldType = "string",
        defaultValue = ""
    }
}

heavenGatesBackdrop.fieldOrder = {
    "intensity",
    "speed",
    "gateHeight",
    "gateWidth",
    "voidRadius",
    "glowIntensity",
    "astralBirthScale",
    "color",
    "alpha",
    "only",
    "exclude",
    "tag",
    "flag",
    "notFlag"
}

heavenGatesBackdrop.placements = {
    {
        name = "heaven_gates_backdrop",
        data = {
            intensity = 1.0,
            speed = 1.0,
            gateHeight = 100.0,
            gateWidth = 40.0,
            voidRadius = 35.0,
            glowIntensity = 1.0,
            astralBirthScale = 1.0,
            only = "end-saved"
        }
    },
    {
        name = "heaven_gates_large_void",
        data = {
            intensity = 1.0,
            speed = 0.8,
            gateHeight = 100.0,
            gateWidth = 40.0,
            voidRadius = 50.0,
            glowIntensity = 1.2,
            astralBirthScale = 1.5,
            only = "end-saved"
        }
    }
}

return heavenGatesBackdrop
