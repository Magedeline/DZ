local KirbyHealthController = {}

KirbyHealthController.name = "MaggyHelper/KirbyHealthRoomController"
KirbyHealthController.depth = 0
KirbyHealthController.placements = {
    name = "default",
    data = {
        maxHealth = 6,
        autoHealOnEnter = false,
        setAsRespawnRoom = false
    }
}

KirbyHealthController.fieldInformation = {
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 20
    }
}

KirbyHealthController.texture = "objects/resortclutter/book_stack_c"

return KirbyHealthController
