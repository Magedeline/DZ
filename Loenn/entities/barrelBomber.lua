local barrelBomber = {}

barrelBomber.name = "DZ/BarrelBomber"
barrelBomber.depth = 0
barrelBomber.texture = "objects/DZ/barrel_bomber"

barrelBomber.placements = {
    name = "barrel_bomber",
    data = {
        health = 1,
        detectionRange = 80,
        explosionRadius = 100,
        fuseTime = 1.5
    }
}

return barrelBomber