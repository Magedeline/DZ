local charaBoost = {}

charaBoost.name = "DZ/CharaBoost"
charaBoost.depth = -1000000
charaBoost.nodeLineRenderType = "line"
charaBoost.texture = "objects/charaboost/idle00"
charaBoost.nodeLimits = {0, -1}
charaBoost.placements = {
    name = "chara_boost",
    data = {
        lockCamera = true,
        canSkip = false,
        finalCh19Boost = false,
        finalCh19GoldenBoost = false,
        finalCh19PPBoost = false,
        finalCh19Dialog = false
    }
}

return charaBoost
