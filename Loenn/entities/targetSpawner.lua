local targetSpawner = {}

targetSpawner.name = "DZ/TargetSpawner"
targetSpawner.depth = 0
targetSpawner.texture = "objects/DZ/target_spawner"

targetSpawner.placements = {
    name = "target_spawner",
    data = {
        points = 100,
        showTime = 2,
        resetTime = 3,
        moveSpeed = 50
    }
}

return targetSpawner