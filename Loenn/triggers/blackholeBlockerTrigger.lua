local blackholeBlockerTrigger = {}

blackholeBlockerTrigger.name = "DZ/BlackholeBlockerTrigger"
blackholeBlockerTrigger.depth = 0
blackholeBlockerTrigger.placements = {
    name = "blackhole_blocker_trigger",
    data = {
        releaseFlag = "",
        pullDirection = "Center",
        oneUse = false,
        killPlayer = false,
        visualEffect = true,
        stopDuration = 1.5,
        pullStrength = 200
    }
}

return blackholeBlockerTrigger