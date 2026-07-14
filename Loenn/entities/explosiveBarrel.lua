local explosiveBarrel = {}

explosiveBarrel.name = "DZ/ExplosiveBarrel"
explosiveBarrel.depth = 0
explosiveBarrel.texture = "objects/DZ/explosive_barrel"

explosiveBarrel.placements = {
    name = "explosive_barrel",
    data = {
        health = 1,
        detectionRange = 80,
        explosionRadius = 100,
        fuseTime = 1.5
    }
}

return explosiveBarrel