local kirbyHealthRoomController = {}

kirbyHealthRoomController.name = "DZ/KirbyHealthRoomController"
kirbyHealthRoomController.depth = 0
kirbyHealthRoomController.placements = {
    name = "kirby_health_room_controller",
    data = {
        maxHealth = 6,
        autoHealOnEnter = false,
        setAsRespawnRoom = false
    }
}

return kirbyHealthRoomController
