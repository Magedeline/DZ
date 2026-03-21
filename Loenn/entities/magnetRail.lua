local magnetRail = {}

magnetRail.name = "MaggyHelper/MagnetRail"
magnetRail.depth = -1000
magnetRail.nodeLimits = {1, -1}
magnetRail.nodeLineRenderType = "line"
magnetRail.placements = {
    {
        name = "MagnetRail",
        data = { speed = 120.0, color = "ffff00" }
    },
    {
        name = "fast",
        data = { speed = 240.0, color = "ff8800" }
    },
    {
        name = "slow",
        data = { speed = 60.0, color = "00ff88" }
    }
}
magnetRail.fieldInformation = {
    speed = { fieldType = "number", minimumValue = 10.0 },
    color = { fieldType = "color" }
}
magnetRail.fieldOrder = { "x", "y", "speed", "color" }

return magnetRail
