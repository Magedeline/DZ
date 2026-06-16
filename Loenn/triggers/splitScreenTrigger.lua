local splitScreenTrigger = {}
splitScreenTrigger.name = "DZ/SplitScreenTrigger"
splitScreenTrigger.placements = {
    { name = "horizontal", data = { width = 16, height = 16, splitDirection = "Horizontal", splitRatio = 0.5 } },
    { name = "vertical", data = { width = 16, height = 16, splitDirection = "Vertical", splitRatio = 0.5 } }
}
splitScreenTrigger.fieldInformation = {
    splitDirection = { fieldType = "string", options = { "Horizontal", "Vertical" }, editable = false },
    splitRatio = { fieldType = "number", minimumValue = 0.1, maximumValue = 0.9 }
}
splitScreenTrigger.fieldOrder = { "x", "y", "width", "height", "splitDirection", "splitRatio" }
return splitScreenTrigger
