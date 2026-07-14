local ghostlyEcho = {}

ghostlyEcho.name = "DZ/GhostlyEcho"
ghostlyEcho.depth = 0
ghostlyEcho.texture = "objects/DZ/ghostly_echo"

ghostlyEcho.placements = {
    name = "ghostly_echo",
    data = {
        isDangerous = true,
        isSolid = false,
        mirrorDelay = 0.5,
        fadeTime = 5
    }
}

return ghostlyEcho