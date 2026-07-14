local rockSpawner = {}

rockSpawner.name = "DZ/RockSpawner"
rockSpawner.depth = 0
rockSpawner.texture = "objects/DZ/rock_spawner"

rockSpawner.placements = {
    name = "rock_spawner",
    data = {
        fallSpeed = 300,
        damageRadius = 60,
        triggerRange = 100
    }
}

return rockSpawner