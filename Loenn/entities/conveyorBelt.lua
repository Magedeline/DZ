local conveyorBelt = {}

conveyorBelt.name = "DZ/ConveyorBelt"
conveyorBelt.depth = -10
conveyorBelt.fillColor = {0.5, 0.5, 0.5, 0.4}
conveyorBelt.borderColor = {0.7, 0.7, 0.7, 0.8}
conveyorBelt.placements = {
    {
        name = "right",
        data = { width = 48, speed = 60.0, moveRight = true }
    },
    {
        name = "left",
        data = { width = 48, speed = 60.0, moveRight = false }
    },
    {
        name = "fast_right",
        data = { width = 48, speed = 120.0, moveRight = true }
    }
}
conveyorBelt.fieldInformation = {
    speed = { fieldType = "number", minimumValue = 0.0 },
    moveRight = { fieldType = "boolean" }
}
conveyorBelt.fieldOrder = { "x", "y", "width", "speed", "moveRight" }

return conveyorBelt
