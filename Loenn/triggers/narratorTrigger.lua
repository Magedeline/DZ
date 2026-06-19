local narratorTrigger = {}
narratorTrigger.name = "DZ/NarratorTrigger"
narratorTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, dialogId = "", duration = 3.0, position = "Top", onlyOnce = true } }
}
narratorTrigger.fieldInformation = {
    dialogId = { fieldType = "string" },
    duration = { fieldType = "number", minimumValue = 0.5 },
    position = { fieldType = "string", options = { "Top", "Bottom", "Center" }, editable = false },
    onlyOnce = { fieldType = "boolean" }
}
narratorTrigger.fieldOrder = { "x", "y", "width", "height", "dialogId", "duration", "position", "onlyOnce" }
return narratorTrigger
