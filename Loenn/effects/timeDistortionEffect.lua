local timeDistortionEffect = {}

timeDistortionEffect.name = "DZ/TimeDistortionEffect"
timeDistortionEffect.depth = 10000
timeDistortionEffect.texture = "objects/DZ/DZ/DZ/effects/timedistortion"
timeDistortionEffect.placements = {
    name = "time_distortion_effect",
    data = {
        multiplier = 0.5,
        radius = 200.0
    }
}

return timeDistortionEffect