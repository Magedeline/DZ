local voidGateArena = {}

voidGateArena.name = "DZ/VoidGateArena"
voidGateArena.depth = 0
voidGateArena.texture = "objects/DZ/void_gate_arena"

voidGateArena.placements = {
    name = "void_gate_arena",
    data = {
        completionFlag = "void_gate_arena_complete",
        spawnBoss = true,
        requiredKills = 10,
        enemiesPerWave = 3,
        totalWaves = 3
    }
}

return voidGateArena