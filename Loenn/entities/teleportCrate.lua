local teleportCrate = {}
teleportCrate.name = "DZ/TeleportCrate"
teleportCrate.depth = 0
teleportCrate.placements = {
    { name = "normal", data = { teleportRange = 120.0, width = 16, height = 16 } }
}
teleportCrate.fieldInformation = {
    teleportRange = { fieldType = "number", minimumValue = 20.0 }
}
teleportCrate.fieldOrder = { "x", "y", "width", "height", "teleportRange" }
return teleportCrate
