local teleportTrigger = {}
teleportTrigger.name = "MaggyHelper/TeleportTrigger"
teleportTrigger.placements = {
    { name = "TeleportTrigger", data = { width = 16, height = 16, targetRoom = "", targetX = 0, targetY = 0, showEffect = true } }
}
teleportTrigger.fieldInformation = {
    targetRoom = { fieldType = "string" },
    targetX = { fieldType = "integer" },
    targetY = { fieldType = "integer" },
    showEffect = { fieldType = "boolean" }
}
teleportTrigger.fieldOrder = { "x", "y", "width", "height", "targetRoom", "targetX", "targetY", "showEffect" }
return teleportTrigger
