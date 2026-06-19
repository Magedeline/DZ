local flashbackTrigger = {}
flashbackTrigger.name = "DZ/FlashbackTrigger"
flashbackTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, targetRoom = "", flashbackDuration = 5.0, dialogId = "", onlyOnce = true } }
}
flashbackTrigger.fieldInformation = {
    targetRoom = { fieldType = "string" },
    flashbackDuration = { fieldType = "number", minimumValue = 1.0 },
    dialogId = { fieldType = "string" },
    onlyOnce = { fieldType = "boolean" }
}
flashbackTrigger.fieldOrder = { "x", "y", "width", "height", "targetRoom", "flashbackDuration", "dialogId", "onlyOnce" }
return flashbackTrigger
