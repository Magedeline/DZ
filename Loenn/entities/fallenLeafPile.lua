local fallenLeafPile = {}

fallenLeafPile.name = "DZ/FallenLeafPile"
fallenLeafPile.depth = 0
fallenLeafPile.texture = "objects/DZ/fallen_leaf_pile"

fallenLeafPile.placements = {
    name = "fallen_leaf_pile",
    data = {
        enemyType = "DZ/RuinsSentinel",
        collectibleType = "",
        detectionRange = 40
    }
}

return fallenLeafPile