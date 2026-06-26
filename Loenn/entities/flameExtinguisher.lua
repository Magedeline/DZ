local flameExtinguisher = {}

flameExtinguisher.name = "DZ/FlameExtinguisher"
flameExtinguisher.depth = 0
flameExtinguisher.texture = "objects/DZ/flame_extinguisher"

flameExtinguisher.placements = {
    name = "flame_extinguisher",
    data = {
        isSource = true,
        canSpread = true,
        spreadSpeed = 20,
        maxSpreadDistance = 200
    }
}

return flameExtinguisher