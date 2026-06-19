local gravityWell = {}
gravityWell.name = "DZ/GravityWell"
gravityWell.depth = -100
gravityWell.placements = {
    { name = "main", data = { pullStrength = 100.0, radius = 80.0, affectsEnemies = false } },
    { name = "gravitywellstrong", data = { pullStrength = 200.0, radius = 120.0, affectsEnemies = true } }
}
gravityWell.fieldInformation = {
    pullStrength = { fieldType = "number", minimumValue = 10.0 },
    radius = { fieldType = "number", minimumValue = 20.0 },
    affectsEnemies = { fieldType = "boolean" }
}
gravityWell.fieldOrder = { "x", "y", "pullStrength", "radius", "affectsEnemies" }
return gravityWell
