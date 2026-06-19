local cameraShakeTrigger = {}
cameraShakeTrigger.name = "DZ/CameraShakeTrigger"
cameraShakeTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, intensity = 0.3, duration = 0.5, direction = "Both" } },
    { name = "heavy", data = { width = 16, height = 16, intensity = 1.0, duration = 1.0, direction = "Both" } }
}
cameraShakeTrigger.fieldInformation = {
    intensity = { fieldType = "number", minimumValue = 0.1 },
    duration = { fieldType = "number", minimumValue = 0.1 },
    direction = { fieldType = "string", options = { "Horizontal", "Vertical", "Both" }, editable = false }
}
cameraShakeTrigger.fieldOrder = { "x", "y", "width", "height", "intensity", "duration", "direction" }
return cameraShakeTrigger
