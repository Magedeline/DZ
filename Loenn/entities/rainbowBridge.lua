local rainbowBridge = {}

rainbowBridge.name = "DZ/RainbowBridge"
rainbowBridge.depth = 100
rainbowBridge.fillColor = {1.0, 0.5, 0.5, 0.3}
rainbowBridge.borderColor = {1.0, 0.3, 0.3, 0.6}
rainbowBridge.placements = {
    {
        name = "RainbowBridge",
        data = { width = 64, height = 8, speedThreshold = 20.0 }
    }
}
rainbowBridge.fieldInformation = {
    speedThreshold = { fieldType = "number", minimumValue = 0.0 }
}
rainbowBridge.fieldOrder = { "x", "y", "width", "height", "speedThreshold" }

return rainbowBridge
