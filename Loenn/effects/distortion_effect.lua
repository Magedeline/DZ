-- Distortion Effect for Lönn
local distortionEffect = {}

distortionEffect.name = "MaggyHelper/DistortionEffect"
distortionEffect.canForeground = true
distortionEffect.canBackground = true

distortionEffect.fieldInformation = {
    anxiety = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    waterAlpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    gamerate = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    }
}

distortionEffect.placements = {
    {
        name = "distortion",
        data = {
            anxiety = 0.0,
            waterAlpha = 1.0,
            gamerate = 1.0
        }
    }
}

return distortionEffect
