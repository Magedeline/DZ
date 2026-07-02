local teleportPipe = {}

teleportPipe.name = "DZ/TeleportPipe"
teleportPipe.depth = 0
teleportPipe.texture = "objects/DZ/DZ/DZ/teleport_pipe"

teleportPipe.placements = {
    name = "teleport_pipe",
    data = {
        targetPipeId = "",
        targetRoom = "",
        pipeColor = "228B22",
        autoEnter = false,
        targetX = 0,
        targetY = 0,
        enterDelay = 0.5
    }
}

return teleportPipe