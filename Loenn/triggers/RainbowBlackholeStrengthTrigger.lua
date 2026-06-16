local rainbowBlackholeStrengthTrigger = {}
rainbowBlackholeStrengthTrigger.name = "DZ/RainbowBlackholeStrengthTrigger"
rainbowBlackholeStrengthTrigger.placements = {
    { name = "RainbowBlackholeStrengthTrigger", data = { width = 16, height = 16, strength = "Mild", rainbowMode = false } }
}
rainbowBlackholeStrengthTrigger.fieldInformation = {
    strength = { fieldType = "string", options = { "Mild", "Medium", "High", "Wild" } },
    rainbowMode = { fieldType = "boolean" }
}
rainbowBlackholeStrengthTrigger.fieldOrder = { "x", "y", "width", "height", "strength", "rainbowMode" }
return rainbowBlackholeStrengthTrigger
