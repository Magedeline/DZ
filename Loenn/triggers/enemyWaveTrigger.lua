local enemyWaveTrigger = {}
enemyWaveTrigger.name = "MaggyHelper/EnemyWaveTrigger"
enemyWaveTrigger.placements = {
    { name = "EnemyWaveTrigger", data = { width = 32, height = 32, waveCount = 3, enemiesPerWave = 3, spawnDelay = 2.0, flag = "" } }
}
enemyWaveTrigger.fieldInformation = {
    waveCount = { fieldType = "integer", minimumValue = 1 },
    enemiesPerWave = { fieldType = "integer", minimumValue = 1 },
    spawnDelay = { fieldType = "number", minimumValue = 0.5 },
    flag = { fieldType = "string" }
}
enemyWaveTrigger.fieldOrder = { "x", "y", "width", "height", "waveCount", "enemiesPerWave", "spawnDelay", "flag" }
return enemyWaveTrigger
