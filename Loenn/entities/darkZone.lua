local darkZone = {}
darkZone.name = "MaggyHelper/DarkZone"
darkZone.depth = -50
darkZone.placements = {
    { name = "normal", data = { width = 64, height = 64, playerLightRadius = 40.0, flag = "" } },
    { name = "dim", data = { width = 64, height = 64, playerLightRadius = 60.0, flag = "" } }
}
darkZone.fieldInformation = {
    playerLightRadius = { fieldType = "number", minimumValue = 10.0 },
    flag = { fieldType = "string" }
}
darkZone.fieldOrder = { "x", "y", "width", "height", "playerLightRadius", "flag" }
return darkZone
