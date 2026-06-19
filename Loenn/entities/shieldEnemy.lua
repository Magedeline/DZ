local shieldEnemy = {}
shieldEnemy.name = "DZ/ShieldEnemy"
shieldEnemy.depth = -100
shieldEnemy.placements = {
    {
        name = "main",
        data = { health = 2, speed = 30.0, shieldHealth = 3 }
    }
}
shieldEnemy.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    speed = { fieldType = "number", minimumValue = 0.0 },
    shieldHealth = { fieldType = "integer", minimumValue = 1 }
}
shieldEnemy.fieldOrder = { "x", "y", "health", "speed", "shieldHealth" }
function shieldEnemy.sprite(room, entity) return "characters/badeline/yourway00", {0.5, 1.0} end
return shieldEnemy
