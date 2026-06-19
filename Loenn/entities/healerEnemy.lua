local healerEnemy = {}
healerEnemy.name = "DZ/HealerEnemy"
healerEnemy.depth = -100
healerEnemy.placements = {
    { name = "main", data = { health = 1, healRange = 80.0, healRate = 1.0 } }
}
healerEnemy.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    healRange = { fieldType = "number", minimumValue = 20.0 },
    healRate = { fieldType = "number", minimumValue = 0.5 }
}
healerEnemy.fieldOrder = { "x", "y", "health", "healRange", "healRate" }
return healerEnemy
