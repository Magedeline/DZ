local ghostEnemy = {}
ghostEnemy.name = "MaggyHelper/GhostEnemy"
ghostEnemy.depth = -100
ghostEnemy.placements = {
    { name = "normal", data = { health = 3, chaseSpeed = 50.0 } },
    { name = "fast", data = { health = 2, chaseSpeed = 100.0 } }
}
ghostEnemy.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    chaseSpeed = { fieldType = "number", minimumValue = 10.0 }
}
ghostEnemy.fieldOrder = { "x", "y", "health", "chaseSpeed" }
return ghostEnemy
