-- Rainbow Blackhole Trigger - controls Rainbow Blackhole background effects
local rainbowBlackholeTrigger = {}

rainbowBlackholeTrigger.name = "DZ/RainbowBlackholeTrigger"

rainbowBlackholeTrigger.placements = {
    {
        name = "enable",
        data = {
            width = 16,
            height = 16,
            action = "Enable",
            strength = "Medium",
            alpha = 1.0,
            scale = 1.0,
            direction = 1.0,
            triggerOnce = false,
            fadeTime = 1.0,
            flag = "",
            onlyIfFlag = false
        }
    },
    {
        name = "disable",
        data = {
            width = 16,
            height = 16,
            action = "Disable",
            strength = "Medium",
            alpha = 0.0,
            scale = 1.0,
            direction = 1.0,
            triggerOnce = false,
            fadeTime = 1.0,
            flag = "",
            onlyIfFlag = false
        }
    },
    {
        name = "change_strength",
        data = {
            width = 16,
            height = 16,
            action = "ChangeStrength",
            strength = "High",
            alpha = 1.0,
            scale = 1.0,
            direction = 1.0,
            triggerOnce = false,
            fadeTime = 0.0,
            flag = "",
            onlyIfFlag = false
        }
    },
    {
        name = "rainbow_chaos",
        data = {
            width = 16,
            height = 16,
            action = "ChangeStrength",
            strength = "RainbowChaos",
            alpha = 1.0,
            scale = 1.0,
            direction = 1.0,
            triggerOnce = false,
            fadeTime = 0.0,
            flag = "",
            onlyIfFlag = false
        }
    },
    {
        name = "set_alpha",
        data = {
            width = 16,
            height = 16,
            action = "SetAlpha",
            strength = "Medium",
            alpha = 0.5,
            scale = 1.0,
            direction = 1.0,
            triggerOnce = false,
            fadeTime = 2.0,
            flag = "",
            onlyIfFlag = false
        }
    },
    {
        name = "set_scale",
        data = {
            width = 16,
            height = 16,
            action = "SetScale",
            strength = "Medium",
            alpha = 1.0,
            scale = 1.5,
            direction = 1.0,
            triggerOnce = false,
            fadeTime = 2.0,
            flag = "",
            onlyIfFlag = false
        }
    },
    {
        name = "set_direction",
        data = {
            width = 16,
            height = 16,
            action = "SetDirection",
            strength = "Medium",
            alpha = 1.0,
            scale = 1.0,
            direction = -1.0,
            triggerOnce = false,
            fadeTime = 0.0,
            flag = "",
            onlyIfFlag = false
        }
    },
    {
        name = "toggle",
        data = {
            width = 16,
            height = 16,
            action = "Toggle",
            strength = "Medium",
            alpha = 1.0,
            scale = 1.0,
            direction = 1.0,
            triggerOnce = true,
            fadeTime = 0.0,
            flag = "",
            onlyIfFlag = false
        }
    }
}

rainbowBlackholeTrigger.fieldInformation = {
    action = {
        options = {
            "Enable",
            "Disable",
            "ChangeStrength",
            "SetAlpha",
            "SetScale",
            "SetDirection",
            "Toggle"
        },
        editable = false
    },
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
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    scale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    direction = {
        fieldType = "number"
    },
    fadeTime = {
        fieldType = "number",
        minimumValue = 0.0
    },
    triggerOnce = {
        fieldType = "boolean"
    },
    flag = {
        fieldType = "string"
    },
    onlyIfFlag = {
        fieldType = "boolean"
    }
}

return rainbowBlackholeTrigger