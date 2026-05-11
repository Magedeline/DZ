local flingBird = {}

flingBird.name = "MaggyHelper/FlingBirdMod"
flingBird.depth = -1
flingBird.nodeLineRenderType = "line"
flingBird.texture = "characters/Maggy/DesoloZantas/bird/Hover04"
flingBird.nodeLimits = {0, -1}
flingBird.placements = {
    name = "fling_bird",
    data = {
        waiting = false
    }
}

return flingBird
