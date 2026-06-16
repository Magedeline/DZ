local burrowingEnemy = {}
burrowingEnemy.name = "DZ/BurrowingEnemy"
burrowingEnemy.depth = -100
burrowingEnemy.placements = {
    { name = "normal", data = { health = 1, detectionRange = 80.0, surfaceTime = 2.0, burrowTime = 3.0 } },
    { name = "quick", data = { health = 1, detectionRange = 100.0, surfaceTime = 1.0, burrowTime = 1.5 } }
}
burrowingEnemy.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    detectionRange = { fieldType = "number", minimumValue = 10.0 },
    surfaceTime = { fieldType = "number", minimumValue = 0.5 },
    burrowTime = { fieldType = "number", minimumValue = 0.5 }
}
burrowingEnemy.fieldOrder = { "x", "y", "health", "detectionRange", "surfaceTime", "burrowTime" }
return burrowingEnemy
