local swingingDoor = {}

swingingDoor.name = "DZ/SwingingDoor"
swingingDoor.depth = 0
swingingDoor.texture = "objects/DZ/DZ/DZ/swinging_door"

swingingDoor.placements = {
    name = "swinging_door",
    data = {
        isLocked = false,
        isDoubleDoor = true,
        swingSpeed = 3,
        knockbackForce = 150,
        autoCloseTime = 2
    }
}

return swingingDoor