local kindnessSoulBoost = {}

kindnessSoulBoost.name = "DZ/KindnessSoulBoost"
kindnessSoulBoost.depth = -1000000
kindnessSoulBoost.texture = "characters/DZ/soul/soul/vessel_soulF"
kindnessSoulBoost.nodeLineRenderType = "line"
kindnessSoulBoost.nodeVisibility = "always"
kindnessSoulBoost.nodeLimits = {1, -1}

kindnessSoulBoost.placements = {
    name = "kindness_soul_boost",
    nodes = {{x = 0, y = 0}},
    data = {
        canSkip = false,
        oneUse = false,
        boostSpeed = 320.0,
        shieldHits = 3
    }
}

kindnessSoulBoost.fieldInformation = {
    boostSpeed = {
        minimumValue = 0.0
    },
    shieldHits = {
        minimumValue = 1,
        fieldType = "integer"
    }
}

return kindnessSoulBoost
