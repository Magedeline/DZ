local sevenSoulBoost = {}

sevenSoulBoost.name = "DZ/SevenSoulBoost"
sevenSoulBoost.depth = -1000000
sevenSoulBoost.texture = "characters/DZ/soul/soul/vessel_soulA"
sevenSoulBoost.nodeLineRenderType = "line"
sevenSoulBoost.nodeVisibility = "always"
sevenSoulBoost.nodeLimits = {1, -1}

sevenSoulBoost.placements = {
    name = "seven_soul_boost",
    nodes = {{x = 0, y = 0}},
    data = {
        lockCamera = true,
        canSkip = false,
        oneUse = false,
        boostSpeed = 320.0,
        refillDashes = true,
        refillStamina = true,
        dashCount = 10,
        finalCh21Boost = false,
        finalCh21GoldenBoost = false,
        finalCh21Dialog = ""
    }
}

sevenSoulBoost.fieldInformation = {
    boostSpeed = {
        minimumValue = 0.0
    },
    dashCount = {
        minimumValue = 1,
        fieldType = "integer"
    }
}

return sevenSoulBoost
