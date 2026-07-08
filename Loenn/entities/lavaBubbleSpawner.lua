local lavaBubbleSpawner = {}

lavaBubbleSpawner.name = "DZ/LavaBubbleSpawner"
lavaBubbleSpawner.depth = 0
lavaBubbleSpawner.texture = "objects/DZ/DZ/DZ/lava_bubble_spawner"

lavaBubbleSpawner.placements = {
    name = "lava_bubble_spawner",
    data = {
        riseSpeed = 80,
        burstHeight = 100,
        damageRadius = 50
    }
}

return lavaBubbleSpawner