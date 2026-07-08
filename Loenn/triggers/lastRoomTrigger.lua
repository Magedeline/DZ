local lastRoomTrigger = {}

lastRoomTrigger.name = "DZ/LastRoomTrigger"
lastRoomTrigger.depth = 0
lastRoomTrigger.placements = {
    name = "last_room_trigger",
    data = {
        enableGravity = true,
        enableRoomWrapper = true,
        enableSparklingStars = true,
        enableRainbow = true,
        triggerOnce = true,
        starIntensity = 1.0,
        rainbowStrength = 1.0
    }
}

return lastRoomTrigger