local staircaseSwitch = {}

staircaseSwitch.name = "DZ/StaircaseSwitch"
staircaseSwitch.depth = 0
staircaseSwitch.texture = "objects/DZ/staircase_switch"

staircaseSwitch.placements = {
    name = "staircase_switch",
    data = {
        clockwise = true,
        platformCount = 8,
        rotationSpeed = 0.5,
        maxSpeed = 2,
        radius = 100
    }
}

return staircaseSwitch