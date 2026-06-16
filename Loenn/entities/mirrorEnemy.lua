local mirrorEnemy = {}
mirrorEnemy.name = "DZ/MirrorEnemy"
mirrorEnemy.depth = -100
mirrorEnemy.placements = {
    { name = "MirrorEnemy", data = { health = 2, reflectRadius = 40.0 } }
}
mirrorEnemy.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    reflectRadius = { fieldType = "number", minimumValue = 10.0 }
}
mirrorEnemy.fieldOrder = { "x", "y", "health", "reflectRadius" }
return mirrorEnemy
