local screenFlashTrigger = {}
screenFlashTrigger.name = "MaggyHelper/ScreenFlashTrigger"
screenFlashTrigger.placements = {
    { name = "white_flash", data = { width = 16, height = 16, color = "ffffff", duration = 0.5, onlyOnce = true } },
    { name = "red_flash", data = { width = 16, height = 16, color = "ff0000", duration = 0.3, onlyOnce = false } }
}
screenFlashTrigger.fieldInformation = {
    color = { fieldType = "color" },
    duration = { fieldType = "number", minimumValue = 0.05 },
    onlyOnce = { fieldType = "boolean" }
}
screenFlashTrigger.fieldOrder = { "x", "y", "width", "height", "color", "duration", "onlyOnce" }
return screenFlashTrigger
