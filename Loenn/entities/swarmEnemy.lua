local swarmEnemy = {}
swarmEnemy.name = "DZ/SwarmEnemy"
swarmEnemy.depth = -100
swarmEnemy.placements = {
    { name = "small_swarm", data = { count = 5, chaseRange = 80.0 } },
    { name = "large_swarm", data = { count = 12, chaseRange = 120.0 } }
}
swarmEnemy.fieldInformation = {
    count = { fieldType = "integer", minimumValue = 3, maximumValue = 20 },
    chaseRange = { fieldType = "number", minimumValue = 20.0 }
}
swarmEnemy.fieldOrder = { "x", "y", "count", "chaseRange" }
return swarmEnemy
