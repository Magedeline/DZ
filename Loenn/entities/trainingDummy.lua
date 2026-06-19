local trainingDummy = {}
trainingDummy.name = "DZ/TrainingDummy"
trainingDummy.depth = -100
trainingDummy.justification = {0.5, 1.0}
trainingDummy.texture = "characters/Kglobal::Player/idle00"
trainingDummy.placements = {
    { name = "main", data = { maxHealth = 10, showDamage = true, resetTime = 3.0 } }
}
trainingDummy.fieldInformation = {
    maxHealth = { fieldType = "integer", minimumValue = 1 },
    showDamage = { fieldType = "boolean" },
    resetTime = { fieldType = "number", minimumValue = 0.5 }
}
trainingDummy.fieldOrder = { "x", "y", "maxHealth", "showDamage", "resetTime" }
return trainingDummy
