local invincibilityTrigger = {}
invincibilityTrigger.name = "DZ/InvincibilityTrigger"
invincibilityTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, duration = 5.0, flashColor = "ffff00" } },
    { name = "star_power", data = { width = 16, height = 16, duration = 10.0, flashColor = "ff8800" } }
}
invincibilityTrigger.fieldInformation = {
    duration = { fieldType = "number", minimumValue = 0.5 },
    flashColor = { fieldType = "color" }
}
invincibilityTrigger.fieldOrder = { "x", "y", "width", "height", "duration", "flashColor" }
return invincibilityTrigger
