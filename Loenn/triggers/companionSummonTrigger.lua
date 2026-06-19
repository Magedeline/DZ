local companionSummonTrigger = {}
companionSummonTrigger.name = "DZ/CompanionSummonTrigger"
companionSummonTrigger.placements = {
    { name = "main", data = { width = 16, height = 16, companionType = "Bandana_Dee", despawnOnLeave = true } }
}
companionSummonTrigger.fieldInformation = {
    companionType = { fieldType = "string", options = { "Bandana_Dee", "Ribbon", "Adeleine", "Marx", "Magolor", "Taranza", "Susie", "Francisca", "Flamberge", "Zan_Partizanne" }, editable = true },
    despawnOnLeave = { fieldType = "boolean" }
}
companionSummonTrigger.fieldOrder = { "x", "y", "width", "height", "companionType", "despawnOnLeave" }
return companionSummonTrigger
