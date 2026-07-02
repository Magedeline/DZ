local elevatorCallButton = {}

elevatorCallButton.name = "DZ/ElevatorCallButton"
elevatorCallButton.depth = 0
elevatorCallButton.texture = "objects/DZ/DZ/DZ/elevator_call_button"

elevatorCallButton.placements = {
    name = "elevator_call_button",
    data = {
        elevatorId = "",
        moveSpeed = 80,
        waitTime = 1
    }
}

return elevatorCallButton