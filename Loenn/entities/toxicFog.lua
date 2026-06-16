local toxicFog = {}
toxicFog.name = "DZ/ToxicFog"
toxicFog.depth = -50
toxicFog.placements = {
    { name = "ToxicFog", data = { width = 64, height = 64, damageInterval = 1.0, slowFactor = 0.7 } },
    { name = "ToxicFog_dense", data = { width = 64, height = 64, damageInterval = 0.5, slowFactor = 0.5 } }
}
toxicFog.fieldInformation = {
    damageInterval = { fieldType = "number", minimumValue = 0.1 },
    slowFactor = { fieldType = "number", minimumValue = 0.1, maximumValue = 1.0 }
}
toxicFog.fieldOrder = { "x", "y", "width", "height", "damageInterval", "slowFactor" }
return toxicFog
