-- Loenn plugin for DZ - Enemy Spawn Trigger
local trigger = {}

trigger.name = "DZ/EnemySpawnTrigger"
trigger.placements = {
    {
        name = "waddle_dee",
        data = {
            width = 16,
            height = 16,
            enemyType = "WaddleDee",
            count = 1,
            spawnDelay = 0.0,
            respawn = false
        }
    },
    {
        name = "waddle_doo",
        data = {
            width = 16,
            height = 16,
            enemyType = "WaddleDoo",
            count = 1,
            spawnDelay = 0.0,
            respawn = false
        }
    },
    {
        name = "gordo",
        data = {
            width = 16,
            height = 16,
            enemyType = "Gordo",
            count = 1,
            spawnDelay = 0.0,
            respawn = false
        }
    },
    {
        name = "scarfy",
        data = {
            width = 16,
            height = 16,
            enemyType = "Scarfy",
            count = 1,
            spawnDelay = 0.0,
            respawn = false
        }
    }
}

trigger.fieldInformation = {
    enemyType = {
        options = {
            "WaddleDee",
            "WaddleDoo",
            "Gordo",
            "Scarfy"
        },
        editable = false
    },
    count = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    spawnDelay = {
        minimumValue = 0
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "enemyType",
    "count",
    "spawnDelay",
    "respawn"
}

return trigger
