local braverySoulBoost = {}

braverySoulBoost.name = "DZ/BraverySoulBoost"
braverySoulBoost.depth = -1000000
braverySoulBoost.texture = "characters/DZ/soul/soul/vessel_soulC"
braverySoulBoost.nodeLineRenderType = "line"
braverySoulBoost.nodeVisibility = "always"
braverySoulBoost.nodeLimits = {1, -1}

braverySoulBoost.placements = {
    name = "bravery_soul_boost",
    nodes = {{x = 0, y = 0}},
    data = {
        canSkip = false,
        oneUse = false,
        boostSpeed = 320.0
    }
}

braverySoulBoost.fieldInformation = {
    boostSpeed = {
        minimumValue = 0.0
    }
}

return braverySoulBoost
