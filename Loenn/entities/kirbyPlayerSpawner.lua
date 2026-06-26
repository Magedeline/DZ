local kirbyPlayerSpawner = {}

kirbyPlayerSpawner.name = "DZ/KirbyPlayerSpawner"
kirbyPlayerSpawner.depth = 0
kirbyPlayerSpawner.texture = "objects/DZ/kirby_player_spawner"

kirbyPlayerSpawner.placements = {
    name = "kirby_player_spawner",
    data = {
        startingAbility = "None",
        enableKirbyMode = true,
        spawnCompanion = false
    }
}

return kirbyPlayerSpawner