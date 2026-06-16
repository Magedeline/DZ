local elsGlitchRainbowBlackholeTrigger = {}

elsGlitchRainbowBlackholeTrigger.name = "DZ/ElsGlitchRainbowBlackholeTrigger"
elsGlitchRainbowBlackholeTrigger.displayName = "ELS Glitch Rainbow Blackhole Trigger"

elsGlitchRainbowBlackholeTrigger.fieldInformation = {
    glitchDuration = {
        options = {
            "Short",
            "Medium",
            "Long"
        },
        editable = false,
        fieldType = "string"
    },
    blackholeStrength = {
        options = {
            "Mild",
            "Medium",
            "High",
            "Wild",
            "Insane",
            "RainbowChaos",
            "Cosmic"
        },
        editable = false,
        fieldType = "string"
    },
    stayOn = {
        fieldType = "boolean"
    },
    doGlitch = {
        fieldType = "boolean"
    },
    changeBlackholeStrength = {
        fieldType = "boolean"
    },
    triggerOnce = {
        fieldType = "boolean"
    },
    sessionFlag = {
        fieldType = "string"
    }
}

elsGlitchRainbowBlackholeTrigger.placements = {
    {
        name = "short_glitch_medium_strength",
        data = {
            width = 16,
            height = 16,
            glitchDuration = "Short",
            blackholeStrength = "Medium",
            stayOn = false,
            doGlitch = true,
            changeBlackholeStrength = true,
            triggerOnce = true,
            sessionFlag = ""
        }
    },
    {
        name = "medium_glitch_high_strength",
        data = {
            width = 16,
            height = 16,
            glitchDuration = "Medium",
            blackholeStrength = "High",
            stayOn = false,
            doGlitch = true,
            changeBlackholeStrength = true,
            triggerOnce = true,
            sessionFlag = ""
        }
    },
    {
        name = "long_glitch_wild_strength",
        data = {
            width = 16,
            height = 16,
            glitchDuration = "Long",
            blackholeStrength = "Wild",
            stayOn = false,
            doGlitch = true,
            changeBlackholeStrength = true,
            triggerOnce = true,
            sessionFlag = ""
        }
    },
    {
        name = "insane_chaos",
        data = {
            width = 16,
            height = 16,
            glitchDuration = "Long",
            blackholeStrength = "Insane",
            stayOn = true,
            doGlitch = true,
            changeBlackholeStrength = true,
            triggerOnce = false,
            sessionFlag = ""
        }
    },
    {
        name = "rainbow_chaos",
        data = {
            width = 16,
            height = 16,
            glitchDuration = "Long",
            blackholeStrength = "RainbowChaos",
            stayOn = true,
            doGlitch = true,
            changeBlackholeStrength = true,
            triggerOnce = false,
            sessionFlag = "rainbow_chaos_activated"
        }
    },
    {
        name = "cosmic",
        data = {
            width = 16,
            height = 16,
            glitchDuration = "Long",
            blackholeStrength = "Cosmic",
            stayOn = true,
            doGlitch = true,
            changeBlackholeStrength = true,
            triggerOnce = false,
            sessionFlag = "cosmic_activated"
        }
    },
    {
        name = "strength_only_no_glitch",
        data = {
            width = 16,
            height = 16,
            glitchDuration = "Medium",
            blackholeStrength = "High",
            stayOn = false,
            doGlitch = false,
            changeBlackholeStrength = true,
            triggerOnce = true,
            sessionFlag = ""
        }
    },
    {
        name = "glitch_only_no_strength_change",
        data = {
            width = 16,
            height = 16,
            glitchDuration = "Medium",
            blackholeStrength = "Medium",
            stayOn = false,
            doGlitch = true,
            changeBlackholeStrength = false,
            triggerOnce = true,
            sessionFlag = ""
        }
    }
}

function elsGlitchRainbowBlackholeTrigger.texture(room, entity)
    return "ahorn/entityTrigger"
end

function elsGlitchRainbowBlackholeTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    
    -- Color based on strength level
    local colors = {
        Mild = {0.5, 0.8, 1.0, 0.8},           -- Light blue
        Medium = {0.3, 0.6, 1.0, 0.8},         -- Blue
        High = {0.8, 0.3, 1.0, 0.8},           -- Purple
        Wild = {1.0, 0.3, 0.8, 0.8},           -- Pink
        Insane = {1.0, 0.2, 0.2, 0.8},         -- Red
        RainbowChaos = {1.0, 1.0, 0.0, 0.8},   -- Yellow/Gold
        Cosmic = {0.0, 1.0, 1.0, 0.8}          -- Cyan
    }
    
    local strength = entity.blackholeStrength or "Medium"
    local color = colors[strength] or colors.Medium
    
    return {
        {
            texture = "ahorn/entityTrigger",
            x = 0,
            y = 0,
            scaleX = width / 8,
            scaleY = height / 8,
            color = color
        }
    }
end

return elsGlitchRainbowBlackholeTrigger
