local crumblingCeiling = {}
crumblingCeiling.name = "DZ/CrumblingCeiling"
crumblingCeiling.depth = 0
crumblingCeiling.placements = {
    { name = "main", data = { width = 24, height = 8, crumbleDelay = 0.5, respawnTime = 5.0 } }
}
crumblingCeiling.fieldInformation = {
    crumbleDelay = { fieldType = "number", minimumValue = 0.0 },
    respawnTime = { fieldType = "number", minimumValue = 0.0 }
}
crumblingCeiling.fieldOrder = { "x", "y", "width", "height", "crumbleDelay", "respawnTime" }
return crumblingCeiling
