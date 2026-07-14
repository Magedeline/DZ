local pixelEnemy = {}

pixelEnemy.name = "DZ/PixelEnemy"
pixelEnemy.depth = 0
pixelEnemy.texture = "objects/DZ/pixel_enemy"

pixelEnemy.placements = {
    name = "pixel_enemy",
    data = {
        health = 2,
        gridSize = 8,
        moveSpeed = 60,
        detectionRange = 150
    }
}

return pixelEnemy