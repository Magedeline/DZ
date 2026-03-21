local lavaGeyser = {}
lavaGeyser.name = "MaggyHelper/LavaGeyser"
lavaGeyser.depth = 0
lavaGeyser.placements = {
    { name = "hotgeysernormal", data = { eruptInterval = 3.0, eruptDuration = 1.5, height = 64, width = 16 } },
    { name = "hotgeyserrapid", data = { eruptInterval = 1.5, eruptDuration = 1.0, height = 48, width = 16 } }
}
lavaGeyser.fieldInformation = {
    eruptInterval = { fieldType = "number", minimumValue = 0.5 },
    eruptDuration = { fieldType = "number", minimumValue = 0.5 }
}
lavaGeyser.fieldOrder = { "x", "y", "width", "height", "eruptInterval", "eruptDuration" }
return lavaGeyser
