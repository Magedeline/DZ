local darkMatterMinion = {}
darkMatterMinion.name = "MaggyHelper/DarkMatterMinion"
darkMatterMinion.depth = -100
darkMatterMinion.placements = {
    { name = "normal", data = { health = 2, fireInterval = 2.5, beamLength = 80.0 } },
    { name = "long_range", data = { health = 2, fireInterval = 3.0, beamLength = 140.0 } }
}
darkMatterMinion.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    fireInterval = { fieldType = "number", minimumValue = 0.5 },
    beamLength = { fieldType = "number", minimumValue = 20.0 }
}
darkMatterMinion.fieldOrder = { "x", "y", "health", "fireInterval", "beamLength" }
return darkMatterMinion
