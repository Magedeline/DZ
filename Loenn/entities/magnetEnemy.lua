local magnetEnemy = {}
magnetEnemy.name = "DZ/MagnetEnemy"
magnetEnemy.depth = -100
magnetEnemy.placements = {
    { name = "MagnetEnemy", data = { health = 2, pullStrength = 80.0, pullRange = 120.0 } },
    { name = "MagnetEnemystrong", data = { health = 3, pullStrength = 150.0, pullRange = 160.0 } }
}
magnetEnemy.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    pullStrength = { fieldType = "number", minimumValue = 10.0 },
    pullRange = { fieldType = "number", minimumValue = 20.0 }
}
magnetEnemy.fieldOrder = { "x", "y", "health", "pullStrength", "pullRange" }
return magnetEnemy
