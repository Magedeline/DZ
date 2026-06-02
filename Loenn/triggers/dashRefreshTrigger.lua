local dashRefreshTrigger = {}
dashRefreshTrigger.name = "MaggyHelper/DashRefreshTrigger"
dashRefreshTrigger.placements = {
    { name = "DashRefreshTrigger", data = { width = 16, height = 16, dashCount = 2, onlyOnce = false } },
    { name = "triple", data = { width = 16, height = 16, dashCount = 3, onlyOnce = false } }
}
dashRefreshTrigger.fieldInformation = {
    dashCount = { fieldType = "integer", minimumValue = 1 },
    onlyOnce = { fieldType = "boolean" }
}
dashRefreshTrigger.fieldOrder = { "x", "y", "width", "height", "dashCount", "onlyOnce" }
return dashRefreshTrigger
