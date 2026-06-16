local abilitySwapTrigger = {}
abilitySwapTrigger.name = "DZ/AbilitySwapTrigger"
abilitySwapTrigger.placements = {
    { name = "AbilitySwapTrigger", data = { width = 16, height = 16, abilityName = "Sword", onlyOnce = false } }
}
abilitySwapTrigger.fieldInformation = {
    abilityName = { fieldType = "string" },
    onlyOnce = { fieldType = "boolean" }
}
abilitySwapTrigger.fieldOrder = { "x", "y", "width", "height", "abilityName", "onlyOnce" }
return abilitySwapTrigger
