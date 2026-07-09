local determinationSoulBoost = {}

determinationSoulBoost.name = "DZ/DeterminationSoulBoost"
determinationSoulBoost.depth = -1000000
determinationSoulBoost.texture = "characters/DZ/soul/soul/vessel_soulA"
determinationSoulBoost.nodeLineRenderType = "line"
determinationSoulBoost.nodeVisibility = "always"
determinationSoulBoost.nodeLimits = {1, -1}

determinationSoulBoost.placements = {
    name = "determination_soul_boost",
    nodes = {{x = 0, y = 0}},
    data = {
        canSkip = false,
        oneUse = false,
        boostSpeed = 320.0,
        extraDashes = 1
    }
}

determinationSoulBoost.fieldInformation = {
    boostSpeed = {
        minimumValue = 0.0
    },
    extraDashes = {
        minimumValue = 0,
        fieldType = "integer"
    }
}

return determinationSoulBoost
