local kirbyModeToggleTrigger = {}

kirbyModeToggleTrigger.name = "DZ/Kirby_Mode_Toggle_Trigger"
kirbyModeToggleTrigger.depth = 0
kirbyModeToggleTrigger.placements = {
    name = "kirby_mode_toggle_trigger",
    data = {
        activationMode = "OnEnter",
        transformEffect = "Sparkle",
        triggerState = "Toggle",
        oneUse = false,
        respectSettings = true,
        flagRequired = "",
        flagToSet = "",
        silentMode = false,
        effectDuration = 1,
        particleColor = "FF69B4",
        particleCount = 30,
        screenShake = true,
        shakeIntensity = 0.3,
        transformSound = "event:/DZ/char/kirby/transform_in",
        playSound = true,
        initialPower = "None"
    }
}

return kirbyModeToggleTrigger
