local checkpointTrigger = {}
checkpointTrigger.name = "DZ/CheckpointTrigger"
checkpointTrigger.placements = {
    { name = "CheckpointTrigger", data = { width = 16, height = 16, checkpointId = "cp_1", showEffect = true } }
}
checkpointTrigger.fieldInformation = {
    checkpointId = { fieldType = "string" },
    showEffect = { fieldType = "boolean" }
}
checkpointTrigger.fieldOrder = { "x", "y", "width", "height", "checkpointId", "showEffect" }
return checkpointTrigger
