local saloonDoorController = {}

saloonDoorController.name = "DZ/SaloonDoorController"
saloonDoorController.depth = 0
saloonDoorController.texture = "objects/DZ/saloon_door_controller"

saloonDoorController.placements = {
    name = "saloon_door_controller",
    data = {
        isLocked = false,
        isDoubleDoor = true,
        swingSpeed = 3,
        knockbackForce = 150,
        autoCloseTime = 2
    }
}

return saloonDoorController