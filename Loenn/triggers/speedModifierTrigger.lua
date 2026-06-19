local speedModifierTrigger = {}
speedModifierTrigger.name = "DZ/SpeedModifierTrigger"
speedModifierTrigger.placements = {
    { name = "main", data = { width = 32, height = 32, speedMultiplier = 0.5, affectsX = true, affectsY = true } },
    { name = "fast", data = { width = 32, height = 32, speedMultiplier = 2.0, affectsX = true, affectsY = true } }
}
speedModifierTrigger.fieldInformation = {
    speedMultiplier = { fieldType = "number", minimumValue = 0.1 },
    affectsX = { fieldType = "boolean" },
    affectsY = { fieldType = "boolean" }
}
speedModifierTrigger.fieldOrder = { "x", "y", "width", "height", "speedMultiplier", "affectsX", "affectsY" }
return speedModifierTrigger
