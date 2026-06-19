local mirrorModeTrigger = {}
mirrorModeTrigger.name = "DZ/MirrorModeTrigger"
mirrorModeTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, mirrorX = true, mirrorY = false } },
    { name = "vertical", data = { width = 16, height = 16, mirrorX = false, mirrorY = true } },
    { name = "both", data = { width = 16, height = 16, mirrorX = true, mirrorY = true } }
}
mirrorModeTrigger.fieldInformation = {
    mirrorX = { fieldType = "boolean" },
    mirrorY = { fieldType = "boolean" }
}
mirrorModeTrigger.fieldOrder = { "x", "y", "width", "height", "mirrorX", "mirrorY" }
return mirrorModeTrigger
