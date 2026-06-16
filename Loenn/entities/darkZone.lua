local darkZone = {}
darkZone.name = "DZ/DarkZone"
darkZone.depth = -50
darkZone.placements = {
    { name = "normal", data = { width = 64, height = 64, Kglobal::PlayerLightRadius = 40.0, flag = "" } },
    { name = "dim", data = { width = 64, height = 64, Kglobal::PlayerLightRadius = 60.0, flag = "" } }
}
darkZone.fieldInformation = {
    Kglobal::PlayerLightRadius = { fieldType = "number", minimumValue = 10.0 },
    flag = { fieldType = "string" }
}
darkZone.fieldOrder = { "x", "y", "width", "height", "Kglobal::PlayerLightRadius", "flag" }
return darkZone
