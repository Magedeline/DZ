local justiceSoulBoost = {}

justiceSoulBoost.name = "DZ/JusticeSoulBoost"
justiceSoulBoost.depth = -1000000
justiceSoulBoost.texture = "characters/DZ/soul/soul/vessel_soulG"
justiceSoulBoost.nodeLineRenderType = "line"
justiceSoulBoost.nodeVisibility = "always"
justiceSoulBoost.nodeLimits = {1, -1}

justiceSoulBoost.placements = {
    name = "justice_soul_boost",
    nodes = {{x = 0, y = 0}},
    data = {
        canSkip = false,
        oneUse = false,
        boostSpeed = 320.0,
        projectileCount = 5,
        projectileSpeed = 400.0,
        spreadShot = true
    }
}

justiceSoulBoost.fieldInformation = {
    boostSpeed = {
        minimumValue = 0.0
    },
    projectileCount = {
        minimumValue = 1,
        fieldType = "integer"
    },
    projectileSpeed = {
        minimumValue = 0.0
    }
}

return justiceSoulBoost
