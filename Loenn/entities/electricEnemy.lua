local electricEnemy = {}
electricEnemy.name = "MaggyHelper/ElectricEnemy"
electricEnemy.depth = -100
electricEnemy.placements = {
    { name = "normal", data = { health = 2, chargeTime = 3.0, shockRadius = 60.0, shockSpeed = 100.0 } },
    { name = "fast_charge", data = { health = 2, chargeTime = 1.5, shockRadius = 80.0, shockSpeed = 150.0 } }
}
electricEnemy.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    chargeTime = { fieldType = "number", minimumValue = 0.5 },
    shockRadius = { fieldType = "number", minimumValue = 20.0 },
    shockSpeed = { fieldType = "number", minimumValue = 20.0 }
}
electricEnemy.fieldOrder = { "x", "y", "health", "chargeTime", "shockRadius", "shockSpeed" }
return electricEnemy
