local integritySoulBoost = {}

integritySoulBoost.name = "DZ/IntegritySoulBoost"
integritySoulBoost.depth = -1000000
integritySoulBoost.texture = "characters/DZ/soul/soul/vessel_soulD"
integritySoulBoost.nodeLineRenderType = "line"
integritySoulBoost.nodeVisibility = "always"
integritySoulBoost.nodeLimits = {1, -1}

integritySoulBoost.placements = {
    name = "integrity_soul_boost",
    nodes = {{x = 0, y = 0}},
    data = {
        canSkip = false,
        oneUse = false,
        boostSpeed = 320.0,
        speedMultiplier = 1.5
    }
}

integritySoulBoost.fieldInformation = {
    boostSpeed = {
        minimumValue = 0.0
    },
    speedMultiplier = {
        minimumValue = 0.1
    }
}

return integritySoulBoost
