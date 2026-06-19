local volcanicRock = {}
volcanicRock.name = "DZ/VolcanicRock"
volcanicRock.depth = -200
volcanicRock.placements = {
    { name = "main", data = { spawnInterval = 3.0, rockSpeed = 120.0, rockCount = 3 } },
    { name = "VolcanicRock_intense", data = { spawnInterval = 1.5, rockSpeed = 180.0, rockCount = 6 } }
}
volcanicRock.fieldInformation = {
    spawnInterval = { fieldType = "number", minimumValue = 0.5 },
    rockSpeed = { fieldType = "number", minimumValue = 30.0 },
    rockCount = { fieldType = "integer", minimumValue = 1, maximumValue = 10 }
}
volcanicRock.fieldOrder = { "x", "y", "spawnInterval", "rockSpeed", "rockCount" }
return volcanicRock
