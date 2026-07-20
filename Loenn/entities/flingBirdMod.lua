local flingBird = {}

flingBird.name = "DZ/FlingBird"
flingBird.depth = 0
flingBird.texture = "characters/DZ/bird/Hover04"
flingBird.nodeLineRenderType = "line"
flingBird.nodeVisibility = "always"
flingBird.nodeLimits = {2, -1}

flingBird.placements = {
    name = "fling_bird",
    nodes = {
        {x = 16, y = -16},
        {x = 32, y = 0}
    },
    data = {
        waiting = false
    }
}

return flingBird