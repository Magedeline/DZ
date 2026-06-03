-- Loenn integration for Kirby Mode Toggle Trigger
-- Comprehensive trigger for managing Kirby mode transformations with multiple activation modes

local kirbyModeToggleTrigger = {}

kirbyModeToggleTrigger.name = "MaggyHelper/Kirby_Mode_Toggle_Trigger"

-- Full list of Kirby power states matching KirbyPlayerExtension.KirbyPowerState enum
local kirbyPowerStates = {
    "None",
    "Fire",
    "Ice",
    "Spark",
    "Stone",
    "Sword",
    "Beam",
    "Cutter",
    "Hammer",
    "Wing",
    "Archer",
    "Leaf",
    "Water",
    "Mirror",
    "Esp",
    "Ranger",
    "Mike",
    "Crash",
    "Bomb",
    "Painter",
    "Cook",
    "Bell",
    "Light",
    "Drill",
    "Wheel",
    "Phase",
    "Umbrella",
    "Recycler",
    "Mini",
    "TripleSwap",
    "TimeCrash",
    -- Super abilities
    "InfernoSuper",
    "GrandHammer",
    "MechaniZeranger",
    "FrostMind",
    "UltraSword",
    "Knight"
}

kirbyModeToggleTrigger.fieldInformation = {
    -- Activation settings
    activationMode = {
        fieldType = "string",
        options = {
            "OnEnter",      -- Activate when player enters
            "OnExit",       -- Activate when player exits
            "Toggle",       -- Toggle mode on enter
            "OnStay",       -- Activate while staying inside
            "Persistent"    -- Enable until disabled
        },
        editable = false
    },

    transformEffect = {
        fieldType = "string",
        options = {
            "Instant",      -- Immediate with flash
            "Sparkle",      -- Sparkle particles
            "Flash",        -- Screen flash
            "Smooth",       -- Smooth transition
            "Custom"        -- Custom effect
        },
        editable = false
    },

    triggerState = {
        fieldType = "string",
        options = {
            "Enable",       -- Enable Kirby mode
            "Disable",      -- Disable Kirby mode
            "Toggle"        -- Toggle current state
        },
        editable = false
    },

    -- Boolean settings
    oneUse = {
        fieldType = "boolean"
    },

    respectSettings = {
        fieldType = "boolean"
    },

    silentMode = {
        fieldType = "boolean"
    },

    -- String settings
    flagRequired = {
        fieldType = "string"
    },

    flagToSet = {
        fieldType = "string"
    },

    transformSound = {
        fieldType = "string"
    },

    -- Numeric settings
    effectDuration = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },

    particleCount = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 100
    },

    shakeIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },

    -- Color setting
    particleColor = {
        fieldType = "color"
    },

    -- Audio settings
    playSound = {
        fieldType = "boolean"
    },

    -- Visual settings
    screenShake = {
        fieldType = "boolean"
    },

    -- Initial power state when enabling Kirby mode
    initialPower = {
        options = kirbyPowerStates,
        editable = false
    }
}

kirbyModeToggleTrigger.fieldOrder = {
    "x", "y", "width", "height",
    "activationMode",
    "transformEffect",
    "triggerState",
    "oneUse",
    "respectSettings",
    "flagRequired",
    "flagToSet",
    "silentMode",
    "initialPower",
    "effectDuration",
    "particleColor",
    "particleCount",
    "screenShake",
    "shakeIntensity",
    "transformSound",
    "playSound"
}

kirbyModeToggleTrigger.placements = {
    -- Enable Kirby Mode (OnEnter)
    {
        name = "enable_kirby_on_enter",
        data = {
            width = 16,
            height = 16,
            activationMode = "OnEnter",
            transformEffect = "Sparkle",
            triggerState = "Enable",
            oneUse = false,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "",
            silentMode = false,
            initialPower = "None",
            effectDuration = 1.0,
            particleColor = "FFC0CB",  -- Pink
            particleCount = 30,
            screenShake = true,
            shakeIntensity = 0.3,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    },

    -- Disable Kirby Mode (OnEnter)
    {
        name = "disable_kirby_on_enter",
        data = {
            width = 16,
            height = 16,
            activationMode = "OnEnter",
            transformEffect = "Sparkle",
            triggerState = "Disable",
            oneUse = false,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "",
            silentMode = false,
            initialPower = "None",
            effectDuration = 1.0,
            particleColor = "FFB6C1",  -- Light pink
            particleCount = 30,
            screenShake = true,
            shakeIntensity = 0.3,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    },

    -- Toggle Kirby Mode
    {
        name = "toggle_kirby_mode",
        data = {
            width = 16,
            height = 16,
            activationMode = "Toggle",
            transformEffect = "Sparkle",
            triggerState = "Toggle",
            oneUse = false,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "",
            silentMode = false,
            initialPower = "None",
            effectDuration = 1.0,
            particleColor = "FF69B4",  -- Hot pink
            particleCount = 30,
            screenShake = true,
            shakeIntensity = 0.3,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    },

    -- Enable with Fire Power
    {
        name = "enable_fire_power",
        data = {
            width = 16,
            height = 16,
            activationMode = "OnEnter",
            transformEffect = "Flash",
            triggerState = "Enable",
            oneUse = false,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "",
            silentMode = false,
            initialPower = "Fire",
            effectDuration = 1.0,
            particleColor = "FF4500",  -- Orange-red
            particleCount = 40,
            screenShake = true,
            shakeIntensity = 0.4,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    },

    -- Enable with Sword Power
    {
        name = "enable_sword_power",
        data = {
            width = 16,
            height = 16,
            activationMode = "OnEnter",
            transformEffect = "Sparkle",
            triggerState = "Enable",
            oneUse = false,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "",
            silentMode = false,
            initialPower = "Sword",
            effectDuration = 1.0,
            particleColor = "00FF7F",  -- Spring green
            particleCount = 35,
            screenShake = true,
            shakeIntensity = 0.3,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    },

    -- Enable with Knight (Super) Power
    {
        name = "enable_knight_power",
        data = {
            width = 16,
            height = 16,
            activationMode = "OnEnter",
            transformEffect = "Flash",
            triggerState = "Enable",
            oneUse = true,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "kirby_knight_unlocked",
            silentMode = false,
            initialPower = "Knight",
            effectDuration = 2.0,
            particleColor = "FFD700",  -- Gold
            particleCount = 60,
            screenShake = true,
            shakeIntensity = 0.6,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    },

    -- One-Use Enable
    {
        name = "one_use_enable",
        data = {
            width = 16,
            height = 16,
            activationMode = "OnEnter",
            transformEffect = "Flash",
            triggerState = "Enable",
            oneUse = true,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "kirby_mode_activated",
            silentMode = false,
            initialPower = "None",
            effectDuration = 1.5,
            particleColor = "FFC0CB",
            particleCount = 50,
            screenShake = true,
            shakeIntensity = 0.5,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    },

    -- Persistent Enable
    {
        name = "persistent_enable",
        data = {
            width = 16,
            height = 16,
            activationMode = "Persistent",
            transformEffect = "Smooth",
            triggerState = "Enable",
            oneUse = false,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "",
            silentMode = false,
            initialPower = "None",
            effectDuration = 2.0,
            particleColor = "FFC0CB",
            particleCount = 40,
            screenShake = false,
            shakeIntensity = 0.2,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    },

    -- Silent Toggle (No effects)
    {
        name = "silent_toggle",
        data = {
            width = 16,
            height = 16,
            activationMode = "OnEnter",
            transformEffect = "Instant",
            triggerState = "Toggle",
            oneUse = false,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "",
            silentMode = true,
            initialPower = "None",
            effectDuration = 0.1,
            particleColor = "FFC0CB",
            particleCount = 0,
            screenShake = false,
            shakeIntensity = 0.0,
            transformSound = "",
            playSound = false
        }
    },

    -- Custom Effect Toggle
    {
        name = "custom_effect_toggle",
        data = {
            width = 16,
            height = 16,
            activationMode = "OnEnter",
            transformEffect = "Custom",
            triggerState = "Toggle",
            oneUse = false,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "",
            silentMode = false,
            initialPower = "None",
            effectDuration = 1.2,
            particleColor = "FF1493",  -- Deep pink
            particleCount = 36,
            screenShake = true,
            shakeIntensity = 0.4,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    },

    -- Exit Trigger
    {
        name = "disable_on_exit",
        data = {
            width = 16,
            height = 16,
            activationMode = "OnExit",
            transformEffect = "Sparkle",
            triggerState = "Disable",
            oneUse = false,
            respectSettings = true,
            flagRequired = "",
            flagToSet = "",
            silentMode = false,
            initialPower = "None",
            effectDuration = 1.0,
            particleColor = "FFB6C1",
            particleCount = 25,
            screenShake = false,
            shakeIntensity = 0.2,
            transformSound = "event:/char/pusheen/kirby/transform",
            playSound = true
        }
    }
}

-- Color based on trigger state and power for visual distinction
function kirbyModeToggleTrigger.color(room, entity)
    local triggerState = entity.triggerState or "Toggle"
    local initialPower = entity.initialPower or "None"

    -- Special colors for powers
    if initialPower == "Knight" then
        return {1.0, 0.84, 0.0, 0.6}  -- Gold
    elseif initialPower == "Fire" then
        return {1.0, 0.27, 0.0, 0.6}  -- Orange-red
    elseif initialPower == "Ice" then
        return {0.68, 0.85, 0.9, 0.6}  -- Light blue
    elseif initialPower == "Sword" then
        return {0.0, 1.0, 0.5, 0.6}   -- Spring green
    elseif initialPower ~= "None" then
        return {1.0, 0.75, 0.8, 0.6}  -- Light pink for other powers
    end

    -- Colors based on trigger state
    if triggerState == "Enable" then
        return {1.0, 0.41, 0.71, 0.6}  -- Pink
    elseif triggerState == "Disable" then
        return {0.5, 0.5, 0.5, 0.6}   -- Gray
    else  -- Toggle
        return {0.41, 0.71, 1.0, 0.6}  -- Blue
    end
end

return kirbyModeToggleTrigger
