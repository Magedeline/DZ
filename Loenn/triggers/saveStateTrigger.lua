local saveStateTrigger = {}
saveStateTrigger.name = "MaggyHelper/SaveStateTrigger"
saveStateTrigger.placements = {
    { name = "save", data = { width = 16, height = 16, action = "Save", slotId = "slot_1" } },
    { name = "load", data = { width = 16, height = 16, action = "Load", slotId = "slot_1" } }
}
saveStateTrigger.fieldInformation = {
    action = { fieldType = "string", options = { "Save", "Load" }, editable = false },
    slotId = { fieldType = "string" }
}
saveStateTrigger.fieldOrder = { "x", "y", "width", "height", "action", "slotId" }
return saveStateTrigger
