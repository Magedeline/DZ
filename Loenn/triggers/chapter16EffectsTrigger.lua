local chapter16EffectsTrigger = {}

chapter16EffectsTrigger.name = "DZ/Chapter16EffectsTrigger"
chapter16EffectsTrigger.depth = 0
chapter16EffectsTrigger.placements = {
    name = "chapter16_effects_trigger",
    data = {
        effectType = "screen_shake",
        intensity = 5,
        duration = 2,
        triggerCommand = "",
        autoTrigger = false,
        triggerOnce = true,
        playerOnly = true,
        enableSound = false,
        soundEvent = "",
        flashColor = "FFFFFF"
    }
}

return chapter16EffectsTrigger
