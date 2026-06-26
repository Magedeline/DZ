local elementalEffectTrigger = {}

elementalEffectTrigger.name = "DZ/ElementalEffectTrigger"
elementalEffectTrigger.depth = 0
elementalEffectTrigger.placements = {
    name = "elemental_effect_trigger",
    data = {
        effectType = "fire_burst",
        elementType = "Fire",
        triggerOnEnter = true,
        triggerOnExit = false,
        oneUse = false,
        intensity = 1,
        radius = 32,
        duration = 1
    }
}

return elementalEffectTrigger