local parallaxShiftTrigger = {}
parallaxShiftTrigger.name = "DZ/ParallaxShiftTrigger"
parallaxShiftTrigger.placements = {
    { name = "ParallaxShiftTrigger", data = { width = 16, height = 16, parallaxX = 0.5, parallaxY = 0.5, duration = 1.0 } }
}
parallaxShiftTrigger.fieldInformation = {
    parallaxX = { fieldType = "number" },
    parallaxY = { fieldType = "number" },
    duration = { fieldType = "number", minimumValue = 0.0 }
}
parallaxShiftTrigger.fieldOrder = { "x", "y", "width", "height", "parallaxX", "parallaxY", "duration" }
return parallaxShiftTrigger
