local charaBoost = {}

charaBoost.name = "DZ/CharaBoost"
charaBoost.depth = -1000000
charaBoost.texture = "objects/DZ/charaboost/idle00"
charaBoost.nodeLineRenderType = "line"
charaBoost.nodeVisibility = "always"
charaBoost.nodeLimits = {1, -1}

charaBoost.placements = {
    name = "chara_boost",
    nodes = {{x = 32, y = 0}},
    data = {
        lockCamera = true,
        canSkip = false,
        finalCh19Boost = false,
        finalCh19GoldenBoost = false,
        finalCh19Dialog = false
    }
}

return charaBoost
