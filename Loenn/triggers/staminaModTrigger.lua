local staminaModTrigger = {}
staminaModTrigger.name = "MaggyHelper/StaminaModTrigger"
staminaModTrigger.placements = {
    { name = "infinite", data = { width = 32, height = 32, staminaMultiplier = 999.0, regenMultiplier = 1.0 } },
    { name = "half_stamina", data = { width = 32, height = 32, staminaMultiplier = 0.5, regenMultiplier = 1.0 } }
}
staminaModTrigger.fieldInformation = {
    staminaMultiplier = { fieldType = "number", minimumValue = 0.1 },
    regenMultiplier = { fieldType = "number", minimumValue = 0.0 }
}
staminaModTrigger.fieldOrder = { "x", "y", "width", "height", "staminaMultiplier", "regenMultiplier" }
return staminaModTrigger
