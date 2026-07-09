local perseveranceSoulBoost = {}

perseveranceSoulBoost.name = "DZ/PerseveranceSoulBoost"
perseveranceSoulBoost.depth = -1000000
perseveranceSoulBoost.texture = "characters/DZ/soul/soul/vessel_soulE"
perseveranceSoulBoost.nodeLineRenderType = "line"
perseveranceSoulBoost.nodeVisibility = "always"
perseveranceSoulBoost.nodeLimits = {1, -1}

perseveranceSoulBoost.placements = {
    name = "perseverance_soul_boost",
    nodes = {{x = 0, y = 0}},
    data = {
        canSkip = false,
        oneUse = false,
        boostSpeed = 320.0
    }
}

perseveranceSoulBoost.fieldInformation = {
    boostSpeed = {
        minimumValue = 0.0
    }
}

return perseveranceSoulBoost
