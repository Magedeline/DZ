local bombWaddleDee = {}
bombWaddleDee.name = "DZ/BombWaddleDee"
bombWaddleDee.depth = -100
bombWaddleDee.placements = {
    { name = "normal", data = { health = 1, throwInterval = 2.0, throwRange = 120.0, bombSpeed = 150.0 } },
    { name = "rapid", data = { health = 1, throwInterval = 1.0, throwRange = 150.0, bombSpeed = 200.0 } }
}
bombWaddleDee.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    throwInterval = { fieldType = "number", minimumValue = 0.5 },
    throwRange = { fieldType = "number", minimumValue = 30.0 },
    bombSpeed = { fieldType = "number", minimumValue = 50.0 }
}
bombWaddleDee.fieldOrder = { "x", "y", "health", "throwInterval", "throwRange", "bombSpeed" }
return bombWaddleDee
