local cloneEnemy = {}
cloneEnemy.name = "MaggyHelper/CloneEnemy"
cloneEnemy.depth = -100
cloneEnemy.placements = {
    { name = "normal", data = { health = 1, delaySeconds = 2.0, color = "8800ff" } },
    { name = "short_delay", data = { health = 1, delaySeconds = 1.0, color = "ff0088" } }
}
cloneEnemy.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    delaySeconds = { fieldType = "number", minimumValue = 0.5 },
    color = { fieldType = "color" }
}
cloneEnemy.fieldOrder = { "x", "y", "health", "delaySeconds", "color" }
return cloneEnemy
