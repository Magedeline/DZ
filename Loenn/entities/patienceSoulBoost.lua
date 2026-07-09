local patienceSoulBoost = {}

patienceSoulBoost.name = "DZ/PatienceSoulBoost"
patienceSoulBoost.depth = -1000000
patienceSoulBoost.texture = "characters/DZ/soul/soul/vessel_soulB"
patienceSoulBoost.nodeLineRenderType = "line"
patienceSoulBoost.nodeVisibility = "always"
patienceSoulBoost.nodeLimits = {1, -1}

patienceSoulBoost.placements = {
    name = "patience_soul_boost",
    nodes = {{x = 0, y = 0}},
    data = {
        canSkip = false,
        oneUse = false,
        boostSpeed = 320.0,
        slowMotionFactor = 0.5
    }
}

patienceSoulBoost.fieldInformation = {
    boostSpeed = {
        minimumValue = 0.0
    },
    slowMotionFactor = {
        minimumValue = 0.1,
        maximumValue = 1.0
    }
}

return patienceSoulBoost
