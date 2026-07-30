local flingBirdIntro = {}

flingBirdIntro.name = "DZ/FlingBirdIntro"
flingBirdIntro.depth = 0
flingBirdIntro.texture = "characters/DZ/bird/Hover04"
flingBirdIntro.nodeLineRenderType = "line"
flingBirdIntro.nodeVisibility = "always"
flingBirdIntro.nodeLimits = {1, -1}

flingBirdIntro.placements = {
    name = "fling_bird_intro",
    nodes = {
        {x = 16, y = -16},
        {x = 32, y = 0}
    },
    data = {
        crashes = false
    }
}

return flingBirdIntro