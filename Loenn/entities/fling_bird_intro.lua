local flingBirdIntro = {}

flingBirdIntro.name = "MaggyHelper/FlingBirdIntroMod"
flingBirdIntro.depth = 0
flingBirdIntro.nodeLineRenderType = "line"
flingBirdIntro.texture = "characters/bird/Hover04"
flingBirdIntro.nodeLimits = {1, -1}
flingBirdIntro.placements = {
    name = "Intro_fling_bird",
    data = {
        crashes = false
    }
}

return flingBirdIntro