local spiralStaircase = {}

spiralStaircase.name = "DZ/SpiralStaircase"
spiralStaircase.depth = 0
spiralStaircase.texture = "objects/DZ/spiral_staircase"

spiralStaircase.placements = {
    name = "spiral_staircase",
    data = {
        clockwise = true,
        platformCount = 8,
        rotationSpeed = 0.5,
        maxSpeed = 2,
        radius = 100
    }
}

return spiralStaircase