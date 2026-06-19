local flingBirdIntro = {}

flingBirdIntro.name = "DZ/FlingBirdIntro"
flingBirdIntro.depth = 0
flingBirdIntro.nodeLineRenderType = "line"
flingBirdIntro.texture = "characters/bird/Hover04"
flingBirdIntro.nodeLimits = {1, -1}
flingBirdIntro.placements = {
    {
        name = "main",
        data = {
        crashes = false
        }
    }
}

return flingBirdIntro
