local musicLayerTrigger = {}
musicLayerTrigger.name = "DZ/MusicLayerTrigger"
musicLayerTrigger.placements = {
    { name = "MusicLayerTrigger", data = { width = 16, height = 16, layerIndex = 1, enabled = true, fade = true } }
}
musicLayerTrigger.fieldInformation = {
    layerIndex = { fieldType = "integer", minimumValue = 0, maximumValue = 7 },
    enabled = { fieldType = "boolean" },
    fade = { fieldType = "boolean" }
}
musicLayerTrigger.fieldOrder = { "x", "y", "width", "height", "layerIndex", "enabled", "fade" }
return musicLayerTrigger
