local pixelationTrigger = {}
pixelationTrigger.name = "DZ/PixelationTrigger"
pixelationTrigger.placements = {
    { name = "retro", data = { width = 16, height = 16, pixelSize = 4, transitionDuration = 1.0 } },
    { name = "heavy", data = { width = 16, height = 16, pixelSize = 8, transitionDuration = 0.5 } }
}
pixelationTrigger.fieldInformation = {
    pixelSize = { fieldType = "integer", minimumValue = 1, maximumValue = 16 },
    transitionDuration = { fieldType = "number", minimumValue = 0.0 }
}
pixelationTrigger.fieldOrder = { "x", "y", "width", "height", "pixelSize", "transitionDuration" }
return pixelationTrigger
