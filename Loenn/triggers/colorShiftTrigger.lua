local colorShiftTrigger = {}
colorShiftTrigger.name = "DZ/ColorShiftTrigger"
colorShiftTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, targetColor = "d2b48c", blendStrength = 0.5, transitionDuration = 1.0 } },
    { name = "blue_tint", data = { width = 16, height = 16, targetColor = "4488ff", blendStrength = 0.3, transitionDuration = 0.5 } }
}
colorShiftTrigger.fieldInformation = {
    targetColor = { fieldType = "color" },
    blendStrength = { fieldType = "number", minimumValue = 0.0, maximumValue = 1.0 },
    transitionDuration = { fieldType = "number", minimumValue = 0.0 }
}
colorShiftTrigger.fieldOrder = { "x", "y", "width", "height", "targetColor", "blendStrength", "transitionDuration" }
return colorShiftTrigger
