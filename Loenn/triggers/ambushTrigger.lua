local ambushTrigger = {}
ambushTrigger.name = "DZ/AmbushTrigger"
ambushTrigger.placements = {
    { name = "main", data = { width = 32, height = 32, enemyCount = 4, lockCamera = true, flag = "" } }
}
ambushTrigger.fieldInformation = {
    enemyCount = { fieldType = "integer", minimumValue = 1 },
    lockCamera = { fieldType = "boolean" },
    flag = { fieldType = "string" }
}
ambushTrigger.fieldOrder = { "x", "y", "width", "height", "enemyCount", "lockCamera", "flag" }
return ambushTrigger
