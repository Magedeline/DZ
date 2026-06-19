local springCloud = {}

springCloud.name = "DZ/SpringCloud"
springCloud.depth = -50
springCloud.fillColor = {0.9, 0.9, 1.0, 0.3}
springCloud.borderColor = {1.0, 1.0, 1.0, 0.5}
springCloud.placements = {
    {
        name = "main",
        data = { width = 24, respawnTime = 3.0, extraHeight = 50.0 }
    },
    {
        name = "one_use",
        data = { width = 24, respawnTime = 0.0, extraHeight = 50.0 }
    },
    {
        name = "super_bounce",
        data = { width = 24, respawnTime = 3.0, extraHeight = 120.0 }
    }
}
springCloud.fieldInformation = {
    respawnTime = { fieldType = "number", minimumValue = 0.0 },
    extraHeight = { fieldType = "number", minimumValue = 0.0 }
}
springCloud.fieldOrder = { "x", "y", "width", "respawnTime", "extraHeight" }

return springCloud
