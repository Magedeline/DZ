local cameraShakeTrigger = {}

cameraShakeTrigger.name = "DZ/CameraShakeTrigger"
cameraShakeTrigger.depth = 0
cameraShakeTrigger.placements = {
    name = "camera_shake_trigger",
    data = {
        onlyOnce = true,
        intensity = 0.5,
        duration = 0.5
    }
}

return cameraShakeTrigger