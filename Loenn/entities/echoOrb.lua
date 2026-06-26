local echoOrb = {}

echoOrb.name = "DZ/EchoOrb"
echoOrb.depth = 0
echoOrb.texture = "objects/DZ/echo_orb"

echoOrb.placements = {
    name = "echo_orb",
    data = {
        isDangerous = true,
        isSolid = false,
        mirrorDelay = 0.5,
        fadeTime = 5
    }
}

return echoOrb