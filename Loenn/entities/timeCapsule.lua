local timeCapsule = {}
timeCapsule.name = "DZ/TimeCapsule"
timeCapsule.depth = -100
timeCapsule.placements = {
    { name = "TimeCapsule", data = { slowFactor = 0.5, radius = 80.0, duration = 5.0 } },
    { name = "TimeCapsule_strong_slow", data = { slowFactor = 0.2, radius = 100.0, duration = 4.0 } }
}
timeCapsule.fieldInformation = {
    slowFactor = { fieldType = "number", minimumValue = 0.01, maximumValue = 1.0 },
    radius = { fieldType = "number", minimumValue = 20.0 },
    duration = { fieldType = "number", minimumValue = 1.0 }
}
timeCapsule.fieldOrder = { "x", "y", "slowFactor", "radius", "duration" }
return timeCapsule
