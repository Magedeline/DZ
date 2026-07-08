local ghostReplay = {}

ghostReplay.name = "DZ/GhostReplay"
ghostReplay.depth = 0
ghostReplay.texture = "objects/DZ/DZ/DZ/ghost_replay"

ghostReplay.placements = {
    name = "ghost_replay",
    data = {
        companionType = "waddle_dee",
        sprite = "",
        canPressSwitch = true,
        followSpeed = 100,
        followDistance = 30
    }
}

return ghostReplay