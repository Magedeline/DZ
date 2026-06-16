local scoreTrigger = {}
scoreTrigger.name = "DZ/ScoreTrigger"
scoreTrigger.placements = {
    { name = "ScoreTrigger", data = { width = 16, height = 16, points = 100, showPopup = true, flag = "" } }
}
scoreTrigger.fieldInformation = {
    points = { fieldType = "integer" },
    showPopup = { fieldType = "boolean" },
    flag = { fieldType = "string" }
}
scoreTrigger.fieldOrder = { "x", "y", "width", "height", "points", "showPopup", "flag" }
return scoreTrigger
