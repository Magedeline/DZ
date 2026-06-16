local splittingEnemy = {}
splittingEnemy.name = "DZ/SplittingEnemy"
splittingEnemy.depth = -100
splittingEnemy.placements = {
    { name = "SplittingEnemy", data = { health = 2, splitCount = 2, isSmall = false, speed = 30.0 } },
    { name = "SplittingEnemy_triple_split", data = { health = 3, splitCount = 3, isSmall = false, speed = 30.0 } }
}
splittingEnemy.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    splitCount = { fieldType = "integer", minimumValue = 2, maximumValue = 5 },
    isSmall = { fieldType = "boolean" },
    speed = { fieldType = "number", minimumValue = 0.0 }
}
splittingEnemy.fieldOrder = { "x", "y", "health", "splitCount", "isSmall", "speed" }
return splittingEnemy
