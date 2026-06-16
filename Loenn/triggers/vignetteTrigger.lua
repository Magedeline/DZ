local vignetteTrigger = {}
vignetteTrigger.name = "DZ/VignetteTrigger"
vignetteTrigger.placements = {
    { name = "VignetteTrigger", data = { width = 16, height = 16, vignetteStrength = 0.5, vignetteColor = "000000" } },
    { name = "heavy", data = { width = 16, height = 16, vignetteStrength = 1.0, vignetteColor = "330000" } }
}
vignetteTrigger.fieldInformation = {
    vignetteStrength = { fieldType = "number", minimumValue = 0.0, maximumValue = 1.0 },
    vignetteColor = { fieldType = "color" }
}
vignetteTrigger.fieldOrder = { "x", "y", "width", "height", "vignetteStrength", "vignetteColor" }
return vignetteTrigger
